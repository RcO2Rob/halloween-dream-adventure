using UnityEngine;
using System.Collections;

namespace LostDream
{
    public enum DragonPhase { Patrol, Telegraph, Attack, Recover, Defeated }
    public enum NightmareKind { Dragon, Ghost, Armabee, Demon }
    public class FlyingDragon : MonoBehaviour
    {
        public Transform body, leftWing, rightWing;
        public Animation modelAnimation;
        public Renderer chargeMarker;
        public NightmareKind kind;
        public Vector3 home;
        public float patrolRadius = 2.2f;
        public int health = 2;
        public int MaxHealth { get; private set; } = 2;
        public bool Defeated { get; private set; }
        public DragonPhase Phase { get; private set; }
        public int AttacksCompleted { get; private set; }
        public int LastProjectileCount { get; private set; }
        public Vector3 DiveStart { get; private set; }
        public Vector3 DiveTarget { get; private set; }
        public GameObject DiveWarning { get; private set; }
        public string DisplayName => kind == NightmareKind.Ghost ? "WISP GHOST" : kind == NightmareKind.Armabee ? "FANSHOT BEE" :
            kind == NightmareKind.Demon ? "DIVING DEMON" : MaxHealth >= 3 ? "ELDER DRAGON" : "EMBER DRAGON";
        public string AttackHint => kind == NightmareKind.Ghost ? "SLOW ORB / KEEP MOVING" : kind == NightmareKind.Armabee ? "3 SHOTS / FIND A GAP" :
            kind == NightmareKind.Demon ? "LOCKED DIVE / STEP SIDEWAYS" : "AIMED EMBER / DODGE";
        public string AttackMaterial => kind == NightmareKind.Ghost ? "CandyBlue" : kind == NightmareKind.Armabee ? "CandyGold" :
            kind == NightmareKind.Demon ? "Candy" : "GlowGold";
        public Vector3 AimPosition => transform.position + Vector3.up * .1f;
        float phaseTime, orbit;
        Vector3 attackTarget;
        bool diveContact;
        Renderer[] bodyRenderers;
        void Start()
        {
            MaxHealth = health;
            orbit = Random.Range(0, Mathf.PI * 2);
            Phase = DragonPhase.Patrol;
            phaseTime = Random.Range(2, 4);
            bodyRenderers = body.GetComponentsInChildren<Renderer>();
            PlayModel("Flying_Idle", true);
        }
        void Update()
        {
            if (Defeated || DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Playing) return;
            float flap = Mathf.Sin(Time.time * 12 + home.x) * 30;
            if (leftWing != null) leftWing.localRotation = Quaternion.Euler(0, 0, flap);
            if (rightWing != null) rightWing.localRotation = Quaternion.Euler(0, 0, -flap);
            PlayerMotor player = DreamGame.Instance.player;
            if (Phase == DragonPhase.Patrol && Vector3.Distance(home, player.transform.position) > 18)
            {
                phaseTime = Mathf.Max(phaseTime, 1);
                return;
            }
            phaseTime -= Time.deltaTime;
            if (Phase == DragonPhase.Patrol)
            {
                orbit += Time.deltaTime * (kind == NightmareKind.Armabee ? .95f : .65f);
                Vector3 point = home + new Vector3(Mathf.Cos(orbit) * patrolRadius, Mathf.Sin(Time.time * 2) * .3f, Mathf.Sin(orbit) * patrolRadius);
                transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime * 3);
                Face(player.transform.position);
                if (phaseTime <= 0) BeginAttack(player);
            }
            else if (Phase == DragonPhase.Telegraph && phaseTime <= 0)
            {
                Phase = DragonPhase.Attack;
                if (kind == NightmareKind.Demon)
                {
                    phaseTime = 1.6f;
                    diveContact = false;
                    LastProjectileCount = 0;
                    PlayModel("Headbutt");
                }
                else
                {
                    phaseTime = .15f;
                    Vector3 direction = (attackTarget - transform.position).normalized;
                    int shots = kind == NightmareKind.Armabee ? 3 : 1;
                    float speed = kind == NightmareKind.Ghost ? 5 : kind == NightmareKind.Armabee ? 7.5f : 9;
                    for (int i = 0; i < shots; i++)
                    {
                        Vector3 shot = Quaternion.AngleAxis(shots == 3 ? (i - 1) * 18 : 0, Vector3.up) * direction;
                        DreamProjectile.Spawn(transform.position + shot * 1.1f, shot * speed, false, true, CandyHue.Pink, AttackMaterial);
                    }
                    LastProjectileCount = shots;
                    AttacksCompleted++;
                }
                DreamGame.Instance.audioSystem.Play("dragon", .35f);
            }
            else if (Phase == DragonPhase.Attack)
            {
                if (kind == NightmareKind.Demon)
                {
                    Vector3 previous = transform.position;
                    Vector3 next = Vector3.MoveTowards(previous, DiveTarget, 10 * Time.deltaTime);
                    Vector3 delta = next - previous, offset = player.transform.position + Vector3.up * .9f - previous;
                    float along = delta.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector3.Dot(offset, delta) / delta.sqrMagnitude) : 0;
                    float radius = GetComponent<SphereCollider>().radius + player.GetComponent<CharacterController>().radius;
                    if (!diveContact && (offset - delta * along).magnitude <= radius)
                    {
                        diveContact = true;
                        player.Damage(1);
                    }
                    transform.position = next;
                    if (Vector3.Distance(next, DiveTarget) < .02f || phaseTime <= 0)
                    {
                        AttacksCompleted++;
                        BeginRecovery();
                    }
                }
                else if (phaseTime <= 0) BeginRecovery();
            }
            else if (Phase == DragonPhase.Recover)
            {
                if (kind == NightmareKind.Demon)
                    transform.position = Vector3.MoveTowards(transform.position, home, 5 * Time.deltaTime);
                if (phaseTime <= 0 && (kind != NightmareKind.Demon || Vector3.Distance(transform.position, home) < .05f))
                { Phase = DragonPhase.Patrol; phaseTime = kind == NightmareKind.Dragon ? 2.7f : 3.2f; }
            }
        }
        void Face(Vector3 target)
        {
            Vector3 facing = target - transform.position; facing.y = 0;
            if (facing.sqrMagnitude > .01f) body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(facing), Time.deltaTime * 3);
        }
        void BeginAttack(PlayerMotor player)
        {
            Phase = DragonPhase.Telegraph;
            phaseTime = kind == NightmareKind.Demon ? 1.3f : kind == NightmareKind.Armabee ? 1.15f : .9f;
            attackTarget = player.transform.position + Vector3.up * .8f;
            SetGlow(true);
            PlayModel("Headbutt");
            if (kind != NightmareKind.Demon) return;
            DiveStart = transform.position;
            DiveTarget = transform.position + Vector3.ClampMagnitude(attackTarget - transform.position, 8);
            DiveTarget = new Vector3(DiveTarget.x, Mathf.Max(.9f, DiveTarget.y), DiveTarget.z);
            Vector3 flat = Vector3.ProjectOnPlane(DiveTarget - DiveStart, Vector3.up);
            DiveWarning = DreamArt.Group("Locked demon dive lane", null,
                new Vector3((DiveStart.x + DiveTarget.x) * .5f, .045f, (DiveStart.z + DiveTarget.z) * .5f));
            DiveWarning.transform.rotation = Quaternion.LookRotation(flat.sqrMagnitude > .01f ? flat : Vector3.forward);
            float radius = GetComponent<SphereCollider>().radius + player.GetComponent<CharacterController>().radius;
            DreamArt.Shape("Dive danger lane", PrimitiveType.Cube, DiveWarning.transform, Vector3.zero,
                new Vector3(radius * 2, .02f, Mathf.Max(.2f, flat.magnitude)), "WarningFade");
            var ring = DreamArt.Group("Dive landing warning", null, new Vector3(DiveTarget.x, .07f, DiveTarget.z));
            ring.transform.SetParent(DiveWarning.transform, true);
            DreamArt.Ring(ring.transform, radius, "Warning", 24, .07f);
        }
        void BeginRecovery()
        {
            Phase = DragonPhase.Recover;
            phaseTime = kind == NightmareKind.Demon ? 1 : .8f;
            SetGlow(false);
            ClearDiveWarning();
            PlayModel("Flying_Idle", true);
        }
        void SetGlow(bool charge)
        {
            if (chargeMarker != null)
            {
                chargeMarker.sharedMaterial = DreamArt.Mat(AttackMaterial);
                chargeMarker.enabled = charge;
            }
            foreach (var renderer in bodyRenderers)
                if (renderer.name == "Belly") renderer.sharedMaterial = DreamArt.Mat(charge ? "GlowGold" : "DragonBelly");
        }
        void PlayModel(string suffix, bool loop = false)
        {
            if (modelAnimation == null) return;
            foreach (AnimationState state in modelAnimation)
                if (state.name.EndsWith("|" + suffix))
                {
                    state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                    modelAnimation.CrossFade(state.name, .12f);
                    return;
                }
        }
        void ClearDiveWarning()
        {
            if (DiveWarning != null) Destroy(DiveWarning);
            DiveWarning = null;
        }
        IEnumerator FinishDefeat()
        {
            PlayModel("Death");
            yield return new WaitForSeconds(.75f);
            body.gameObject.SetActive(false);
        }
        public void Hit(int damage)
        {
            if (Defeated) return;
            health -= damage;
            DreamGame.Instance.audioSystem.Play("hit", .45f);
            DreamArt.Burst(transform.position, "GlowTeal", 12, 2);
            if (health > 0) return;
            Defeated = true;
            Phase = DragonPhase.Defeated;
            GetComponent<Collider>().enabled = false;
            SetGlow(false);
            ClearDiveWarning();
            if (modelAnimation != null) StartCoroutine(FinishDefeat());
            else body.gameObject.SetActive(false);
            DreamWorld.Candy(DreamGame.Instance.transform, new Vector3(transform.position.x, .65f, transform.position.z));
            DreamGame.Instance.ShowToast(DisplayName + " cleared. It left a candy behind.");
        }
        void OnDestroy() { ClearDiveWarning(); }
    }
}
