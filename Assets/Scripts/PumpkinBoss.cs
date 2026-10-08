using UnityEngine;

namespace LostDream
{
    public enum BossPhase { Dormant, Telegraph, Bombardment, Exposed, ChargeTelegraph, Charging, Recovering, Returning, Defeated }
    public class PumpkinBoss : MonoBehaviour
    {
        public Transform pumpkin, crown;
        public Renderer[] weakPoint;
        public int maxHealth = 6;
        public int Health { get; private set; } = 6;
        public BossPhase Phase { get; private set; } = BossPhase.Dormant;
        public bool AwakeInArena => Phase != BossPhase.Dormant;
        public bool Exposed => Phase == BossPhase.Exposed || Phase == BossPhase.Recovering;
        public bool Defeated => Phase == BossPhase.Defeated;
        public bool SecondPhase => Health * 2 <= maxHealth;
        public CandyHue RequiredHue { get; private set; }
        public int UnlockedColors => Mathf.Clamp(maxHealth - Health + 1, 1, 3);
        public string RequiredCandyLabel => CandyPalette.Label(RequiredHue);
        public bool ChargeWarningActive => Phase == BossPhase.ChargeTelegraph;
        public Vector3 ChargeTarget => chargeEnd;
        public Vector3 ChargeStart => chargeStart;
        public int ChargesCompleted { get; private set; }
        public GameObject ChargeWarning => chargeWarning;
        public Vector3 AimPosition => transform.position + Vector3.up * 3;
        float timer, bombTimer, hitCooldown;
        int bombsRemaining;
        Vector3 baseScale;
        Vector3 homePosition, chargeStart, chargeEnd;
        Quaternion homeRotation;
        GameObject chargeWarning;
        bool chargeContact;
        bool returnColorPrepared;
        const float ChargeSpeed = 15;
        const float ChargeRadius = 2.7f;
        void Start()
        {
            Health = maxHealth;
            baseScale = pumpkin.localScale;
            homePosition = transform.position;
            homeRotation = transform.rotation;
            PaintColor();
            // The head charges; its original stone dais remains in the courtyard.
            foreach (string name in new[] { "Boss dais", "Nightmare sigil" })
            {
                var decoration = transform.Find(name);
                if (decoration != null)
                {
                    decoration.SetParent(transform.parent, true);
                    if (name == "Boss dais")
                    {
                        // Unity's cylinder uses a capsule: a wide, thin scale makes its
                        // collision spherical and blocks candy well above the visible stone.
                        var capsule = decoration.GetComponent<CapsuleCollider>();
                        if (capsule != null) { capsule.enabled = false; Destroy(capsule); }
                        var mesh = decoration.GetComponent<MeshCollider>() ?? decoration.gameObject.AddComponent<MeshCollider>();
                        mesh.sharedMesh = decoration.GetComponent<MeshFilter>().sharedMesh;
                    }
                }
            }
        }
        void Update()
        {
            DreamGame game = DreamGame.Instance;
            if (game == null || game.Phase != GamePhase.Playing || Defeated) return;
            if (Phase == BossPhase.Dormant)
            {
                if (game.LitCount > 0 && game.player.transform.position.z > 0)
                {
                    Phase = BossPhase.Telegraph;
                    timer = 2.4f;
                    game.ShowToast("The Pumpkin King awakens. Watch the bomb circles!", 4);
                    game.audioSystem.Play("boss", .65f);
                }
                return;
            }
            timer -= Time.deltaTime;
            float breathe = Mathf.Sin(Time.time * (Exposed ? 2 : 4)) * .03f;
            pumpkin.localScale = Vector3.Scale(baseScale, new Vector3(1 + breathe, 1 - breathe, 1 + breathe));
            pumpkin.localRotation = Quaternion.Euler(Phase == BossPhase.Charging ? -18 : 0,
                Exposed ? Mathf.Sin(Time.time * 3) * 5 : Mathf.Sin(Time.time * 2) * 12, 0);
            crown.localPosition = Vector3.up * (5.1f + Mathf.Sin(Time.time * 2) * .12f);
            if (Phase == BossPhase.Telegraph && timer <= 0)
            {
                Phase = BossPhase.Bombardment;
                bombsRemaining = SecondPhase ? 4 : 3;
                bombTimer = 0;
                timer = bombsRemaining * .7f + 1.7f;
            }
            else if (Phase == BossPhase.Bombardment)
            {
                bombTimer -= Time.deltaTime;
                if (bombsRemaining > 0 && bombTimer <= 0)
                {
                    Vector3 p = game.player.transform.position + new Vector3(Random.Range(-1.8f, 1.8f), 0, Random.Range(-1.8f, 1.8f));
                    p.x = Mathf.Clamp(p.x, -11.5f, 11.5f);
                    p.z = Mathf.Clamp(p.z, 0, 24);
                    NightmareBomb.Spawn(new Vector3(p.x, .06f, p.z), SecondPhase ? 1.15f : 1.5f);
                    bombsRemaining--;
                    bombTimer = .7f;
                }
                if (timer <= 0)
                {
                    if (SecondPhase) BeginCharge();
                    else Expose(4, false);
                }
            }
            else if (Phase == BossPhase.Exposed && timer <= 0)
            {
                NextColor();
                Phase = BossPhase.Telegraph;
                timer = 1.8f;
                SetWeakPoint(false);
            }
            else if (Phase == BossPhase.ChargeTelegraph)
            {
                Vector3 direction = chargeEnd - chargeStart;
                if (direction.sqrMagnitude > .01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-direction, Vector3.up), Time.deltaTime * 6);
                if (timer <= 0)
                {
                    Phase = BossPhase.Charging;
                    chargeContact = false;
                    game.audioSystem.Play("dash", .65f, .7f);
                }
            }
            else if (Phase == BossPhase.Charging)
            {
                Vector3 previous = transform.position;
                Vector3 next = Vector3.MoveTowards(previous, chargeEnd, ChargeSpeed * Time.deltaTime);
                Vector3 playerPoint = game.player.transform.position;
                Vector3 segment = Vector3.ProjectOnPlane(next - previous, Vector3.up);
                Vector3 toPlayer = Vector3.ProjectOnPlane(playerPoint - previous, Vector3.up);
                float along = segment.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector3.Dot(toPlayer, segment) / segment.sqrMagnitude) : 0;
                float radius = ChargeRadius + game.player.GetComponent<CharacterController>().radius;
                if (!chargeContact && (toPlayer - segment * along).magnitude <= radius && playerPoint.y < previous.y + 5.5f)
                {
                    chargeContact = true;
                    game.player.Damage(1);
                }
                transform.position = next;
                if (Vector3.Distance(next, chargeEnd) < .01f)
                {
                    ChargesCompleted++;
                    ClearChargeWarning();
                    Expose(2.4f, true);
                }
            }
            else if (Phase == BossPhase.Recovering)
            {
                Vector3 facing = Vector3.ProjectOnPlane(game.player.transform.position - transform.position, Vector3.up);
                if (facing.sqrMagnitude > .01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-facing, Vector3.up), Time.deltaTime * 6);
                if (timer <= 0) BeginReturn();
            }
            else if (Phase == BossPhase.Returning)
            {
                transform.position = Vector3.MoveTowards(transform.position, homePosition, 7 * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, homeRotation, Time.deltaTime * 5);
                if (Vector3.Distance(transform.position, homePosition) < .01f)
                {
                    transform.position = homePosition;
                    transform.rotation = homeRotation;
                    if (!returnColorPrepared) NextColor();
                    returnColorPrepared = false;
                    Phase = BossPhase.Telegraph;
                    timer = 1.8f;
                }
            }
        }
        void BeginCharge()
        {
            var game = DreamGame.Instance;
            if (!SecondPhase || Defeated) return;
            SetWeakPoint(false);
            chargeStart = transform.position;
            Vector3 direction = Vector3.ProjectOnPlane(game.player.transform.position - chargeStart, Vector3.up).normalized;
            if (direction.sqrMagnitude < .01f) direction = Vector3.back;
            float distance = Mathf.Clamp(Vector3.ProjectOnPlane(game.player.transform.position - chargeStart, Vector3.up).magnitude, 5, 10);
            chargeEnd = chargeStart + direction * distance;
            chargeEnd.x = Mathf.Clamp(chargeEnd.x, -9.5f, 9.5f);
            chargeEnd.z = Mathf.Clamp(chargeEnd.z, 2.5f, 22);
            chargeEnd.y = homePosition.y;
            Phase = BossPhase.ChargeTelegraph;
            timer = 1.6f;
            ClearChargeWarning();
            Vector3 center = (chargeStart + chargeEnd) * .5f; center.y = .045f;
            chargeWarning = DreamArt.Group("Locked pumpkin charge lane", null, center);
            chargeWarning.transform.rotation = Quaternion.LookRotation(chargeEnd - chargeStart);
            float length = Vector3.Distance(chargeStart, chargeEnd);
            float radius = ChargeRadius + game.player.GetComponent<CharacterController>().radius;
            DreamArt.Shape("Charge danger corridor", PrimitiveType.Cube, chargeWarning.transform, Vector3.zero,
                new Vector3(radius * 2, .02f, length), "WarningFade");
            var border = chargeWarning.AddComponent<LineRenderer>();
            border.useWorldSpace = false; border.loop = true; border.positionCount = 4;
            border.startWidth = border.endWidth = .08f; border.sharedMaterial = DreamArt.Mat("Warning");
            border.SetPositions(new[] { new Vector3(-radius, .04f, -length / 2), new Vector3(radius, .04f, -length / 2),
                new Vector3(radius, .04f, length / 2), new Vector3(-radius, .04f, length / 2) });
            for (int side = -1; side <= 1; side += 2)
            {
                var cap = DreamArt.Group("Charge lane end", chargeWarning.transform, new Vector3(0, 0, side * length / 2));
                DreamArt.Ring(cap.transform, radius, "Warning", 40, .07f);
            }
            var arrow = DreamArt.Mesh("Charge direction", "Triangle", chargeWarning.transform, Vector3.up * .06f,
                new Vector3(1.5f, 1.5f, 1), "Warning");
            arrow.transform.localRotation = Quaternion.Euler(90, 0, 0);
            game.audioSystem.Play("warning", .45f, .7f);
            game.ShowToast("CHARGE! Dodge sideways. Prepare " + RequiredCandyLabel + " candy.", 2);
        }
        void Expose(float seconds, bool recovering)
        {
            Phase = recovering ? BossPhase.Recovering : BossPhase.Exposed;
            timer = seconds;
            SetWeakPoint(true);
            DreamGame.Instance.ReplenishCandy();
            DreamGame.Instance.audioSystem.Play("exposed", .65f);
            DreamGame.Instance.ShowToast((recovering ? "STUNNED! " : "SHELL OPEN! ") + "Throw " + RequiredCandyLabel + " candy!", 2.5f);
        }
        void BeginReturn(bool colorPrepared = false)
        {
            Phase = BossPhase.Returning;
            returnColorPrepared = colorPrepared;
            SetWeakPoint(false);
        }
        void ClearChargeWarning()
        {
            if (chargeWarning != null) Destroy(chargeWarning);
            chargeWarning = null;
        }
        void SetWeakPoint(bool exposed)
        {
            foreach (var renderer in weakPoint) renderer.sharedMaterial = DreamArt.Mat(exposed ? CandyPalette.Material(RequiredHue) : "GlowMoon");
        }
        void PaintColor()
        {
            int rib = 0;
            foreach (var renderer in pumpkin.GetComponentsInChildren<Renderer>())
                if (renderer.name == "Pumpkin rib")
                    renderer.sharedMaterial = DreamArt.Mat("Boss" + RequiredHue + (rib++ % 2 == 0 ? "" : "Dark"));
            SetWeakPoint(Exposed);
        }
        void NextColor(bool unlocked = false)
        {
            RequiredHue = (CandyHue)(unlocked ? UnlockedColors - 1 : ((int)RequiredHue + 1) % UnlockedColors);
            PaintColor();
            DreamGame.Instance.ReplenishCandy();
        }
        public bool Hit(bool candy, CandyHue hue = CandyHue.Pink)
        {
            if (Defeated || DreamGame.Instance.Phase != GamePhase.Playing) return false;
            if (!candy || !Exposed)
            {
                DreamGame.Instance.audioSystem.Play("shield", .35f);
                DreamGame.Instance.ShowToast(!candy ? "Candy can crack his shell. Save your arrows for the dragons." :
                    ChargeWarningActive || Phase == BossPhase.Charging ? "Avoid the charge. His face opens when he stops." : "His shell is closed. Wait for SHELL OPEN, then match his color!", 1.8f);
                return false;
            }
            if (hue != RequiredHue)
            {
                DreamGame.Instance.audioSystem.Play("shield", .35f);
                DreamGame.Instance.ShowToast("Wrong color! Need " + RequiredCandyLabel + ". Press E at a different candy to swap.", 2.5f);
                return false;
            }
            if (Time.time < hitCooldown) return false;
            hitCooldown = Time.time + .6f;
            int previousHealth = Health;
            Health--;
            DreamArt.Burst(AimPosition, CandyPalette.Material(RequiredHue), 30, 5);
            DreamGame.Instance.audioSystem.Play("hit", .7f, .65f);
            if (Health <= 0)
            {
                Phase = BossPhase.Defeated;
                ClearChargeWarning();
                GetComponent<Collider>().enabled = false;
                DreamGame.Instance.Win();
            }
            else
            {
                // One successful hit per attack cycle keeps the boss timing meaningful.
                SetWeakPoint(false);
                NextColor(maxHealth - Health < 3);
                if (previousHealth * 2 > maxHealth && SecondPhase) BeginCharge();
                else if (Vector3.Distance(transform.position, homePosition) > .01f) BeginReturn(true);
                else
                {
                    Phase = BossPhase.Telegraph;
                    timer = 2.1f;
                    DreamGame.Instance.ShowToast((maxHealth - Health < 3 ? "New color: " : "Prepare ") +
                        RequiredCandyLabel + " candy. E / pick up or swap.", 3);
                }
            }
            return true;
        }
        void OnDestroy() { ClearChargeWarning(); }
    }
}
