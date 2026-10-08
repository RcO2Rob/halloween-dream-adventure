using System.Linq;
using UnityEngine;

namespace LostDream
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        public Transform visual;
        public TimmyAnimation characterAnimation;
        public Transform leftLeg, rightLeg, leftArm, rightArm;
        public GameObject heldCandy;
        public int maxHealth = 5;
        public float moveSpeed = 6;
        public float dashSpeed = 17;
        public float jumpHeight = 2.2f;
        public float gravity = 24;
        public bool IsGrounded => controller != null && controller.isGrounded && verticalSpeed <= 0;
        public bool IsJumping { get; private set; }
        public float VerticalSpeed => verticalSpeed;
        public int Health { get; private set; } = 5;
        public bool CarryingCandy { get; private set; }
        public CandyHue HeldCandyHue { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public float DashCooldown => Mathf.Max(0, dashReady - Time.time);
        public bool Invulnerable => Time.time < invulnerableUntil || Time.time < dashUntil;
        public DreamInteractable Nearby { get; private set; }
        public bool automation;
        public Vector3 automationMove;
        CharacterController controller;
        float verticalSpeed, dashUntil, dashReady, invulnerableUntil, bowReady, throwReady;
        float stride, attackPose, footsteps, lookUntil;
        float airTime;
        Vector3 dashDirection, moveDirection;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (characterAnimation != null && controller.height < 2.1f)
            {
                float growth = 2.1f - controller.height;
                controller.height = 2.1f;
                controller.center += Vector3.up * (growth * .5f);
            }
            Health = maxHealth;
            if (heldCandy != null) heldCandy.SetActive(false);
        }

        void Update()
        {
            DreamGame game = DreamGame.Instance;
            if (game == null) return;
            UpdateNearby();
            if (game.Phase != GamePhase.Playing) return;
            UpdateAim();
            Vector3 forward = Vector3.ProjectOnPlane(game.gameCamera.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(game.gameCamera.transform.right, Vector3.up).normalized;
            float x = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            x -= Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
            float z = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            z -= Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
            moveDirection = automation ? automationMove : Vector3.ClampMagnitude(forward * z + right * x, 1);
            if (!automation && Input.GetKeyDown(KeyCode.Space)) TryJump();
            if (!automation && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))) TryDash(moveDirection);
            Vector3 horizontal = Time.time < dashUntil ? dashDirection * dashSpeed : moveDirection * moveSpeed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed -= gravity * Time.deltaTime;
            bool airborneBeforeMove = !IsGrounded;
            CollisionFlags collision = controller.Move((horizontal + Vector3.up * verticalSpeed) * Time.deltaTime);
            if ((collision & CollisionFlags.Above) != 0 && verticalSpeed > 0) verticalSpeed = 0;
            if ((collision & CollisionFlags.Below) != 0 && verticalSpeed < 0)
            {
                if (airborneBeforeMove && airTime > .08f)
                {
                    if (characterAnimation != null) characterAnimation.Land();
                    game.audioSystem.Play("step", .2f, .85f);
                }
                verticalSpeed = -2;
                IsJumping = false;
                airTime = 0;
            }
            else airTime += Time.deltaTime;
            if (transform.position.y < -8) Fall();
            if (!automation)
            {
                if (Input.GetKeyDown(KeyCode.E)) Interact();
                if (Input.GetMouseButton(0) && !game.gameCamera.GetComponent<DreamCamera>().IsOrbiting) FireAt(AimPoint);
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Q)) ThrowAt(AimPoint);
            }
            Animate(horizontal.magnitude);
            if (horizontal.magnitude > 1 && controller.isGrounded)
            {
                footsteps -= Time.deltaTime;
                if (footsteps <= 0) { game.audioSystem.Play("step", .12f, Random.Range(.9f, 1.1f)); footsteps = .34f; }
            }
        }

        void UpdateAim()
        {
            Camera camera = DreamGame.Instance.gameCamera;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            AimPoint = transform.position + Vector3.forward * 10 + Vector3.up;
            if (Physics.Raycast(ray, out RaycastHit hit, 250, ~(1 << 2), QueryTriggerInteraction.Collide))
            {
                AimPoint = hit.point;
                if (hit.collider.GetComponentInParent<FlyingDragon>() == null
                    && hit.collider.GetComponentInParent<PumpkinBoss>() == null
                    && hit.collider.GetComponentInParent<DreamRune>() == null)
                    AimPoint += Vector3.up * 1.35f;
            }
            else if (new Plane(Vector3.up, Vector3.up * 1.4f).Raycast(ray, out float distance)) AimPoint = ray.GetPoint(distance);
            // Small screen-space aim assistance makes airborne targets readable with this camera.
            float closest = 48;
            foreach (var dragon in DreamGame.Instance.dragons)
            {
                if (dragon == null || dragon.Defeated) continue;
                Vector3 screen = camera.WorldToScreenPoint(dragon.AimPosition);
                float pixels = Vector2.Distance(screen, Input.mousePosition);
                if (screen.z > 0 && pixels < closest && HasLineTo(dragon.AimPosition, dragon.transform))
                { closest = pixels; AimPoint = dragon.AimPosition; }
            }
            foreach (var rune in DreamGame.Instance.runes)
            {
                if (rune == null || rune.Cleared) continue;
                Vector3 screen = camera.WorldToScreenPoint(rune.AimPosition);
                float pixels = Vector2.Distance(screen, Input.mousePosition);
                if (screen.z > 0 && pixels < closest && HasLineTo(rune.AimPosition, rune.transform))
                { closest = pixels; AimPoint = rune.AimPosition; }
            }
            var boss = DreamGame.Instance.boss;
            if (boss != null && !boss.Defeated)
            {
                Vector3 screen = camera.WorldToScreenPoint(boss.AimPosition);
                if (Vector2.Distance(screen, Input.mousePosition) < 100) AimPoint = boss.AimPosition;
            }
        }
        bool HasLineTo(Vector3 point, Transform target)
        {
            Vector3 origin = transform.position + Vector3.up * 1.3f;
            if (!Physics.Raycast(origin, point - origin, out RaycastHit hit, Vector3.Distance(origin, point), ~(1 << 2))) return true;
            return hit.transform == target || hit.transform.IsChildOf(target);
        }
        void UpdateNearby()
        {
            Nearby = FindObjectsByType<DreamInteractable>(FindObjectsSortMode.None)
                .Where(x => x.Available && Vector3.Distance(transform.position + Vector3.up * .6f, x.transform.position) < 2.8f)
                .OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).FirstOrDefault();
        }
        public bool Interact()
        {
            if (DreamGame.Instance.Phase != GamePhase.Playing) return false;
            UpdateNearby();
            return Nearby != null && Nearby.Use(this);
        }
        public bool TryDash(Vector3 direction)
        {
            if (Time.time < dashReady || !IsGrounded || DreamGame.Instance.Phase != GamePhase.Playing) return false;
            dashDirection = direction.sqrMagnitude > .01f ? direction.normalized : visual.forward;
            dashUntil = Time.time + .22f;
            dashReady = Time.time + 1.05f;
            DreamGame.Instance.audioSystem.Play("dash", .35f);
            DreamArt.Burst(transform.position + Vector3.up * .5f, "GlowTeal", 8, 1);
            return true;
        }
        public bool TryJump()
        {
            if (!IsGrounded || DreamGame.Instance.Phase != GamePhase.Playing) return false;
            verticalSpeed = Mathf.Sqrt(2 * gravity * jumpHeight);
            IsJumping = true;
            dashUntil = Time.time;
            airTime = 0;
            if (characterAnimation != null) characterAnimation.BeginJump();
            DreamGame.Instance.audioSystem.Play("dash", .18f, 1.25f);
            return true;
        }
        public bool FireAt(Vector3 target)
        {
            if (Time.time < bowReady || DreamGame.Instance.Phase != GamePhase.Playing) return false;
            bowReady = Time.time + .38f;
            Face(target);
            lookUntil = bowReady + .02f;
            Vector3 origin = transform.position + Vector3.up * 1.3f;
            if (characterAnimation != null)
            {
                characterAnimation.Shoot(target);
                origin = characterAnimation.BowGripPosition;
            }
            Vector3 direction = (target - origin).normalized;
            DreamProjectile.Spawn(origin + direction * (characterAnimation != null ? .15f : .65f), direction * 31, false);
            attackPose = .25f;
            DreamGame.Instance.audioSystem.Play("bow", .35f, Random.Range(.95f, 1.08f));
            return true;
        }
        public bool ThrowAt(Vector3 target)
        {
            if (!CarryingCandy || Time.time < throwReady || DreamGame.Instance.Phase != GamePhase.Playing)
            {
                if (!CarryingCandy) DreamGame.Instance.ShowToast("Pick up a glowing candy with E first.");
                return false;
            }
            Vector3 origin = transform.position + Vector3.up * 1.3f;
            float travel = Mathf.Clamp(Vector3.Distance(origin, target) / 18, .42f, 1.25f);
            Vector3 velocity = (target - origin) / travel + Vector3.up * (12 * travel * .5f);
            DreamProjectile.Spawn(origin, velocity, true, false, HeldCandyHue);
            CarryingCandy = false;
            heldCandy.SetActive(false);
            throwReady = Time.time + .6f;
            Face(target);
            attackPose = .4f;
            if (characterAnimation != null) characterAnimation.Throw();
            DreamGame.Instance.audioSystem.Play("throw", .5f);
            return true;
        }
        void Face(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .01f) visual.rotation = Quaternion.LookRotation(direction);
            lookUntil = Time.time + .35f;
        }
        public bool PickCandy(CandyHue hue = CandyHue.Pink)
        {
            if (CarryingCandy) { DreamGame.Instance.ShowToast("You can carry one candy at a time."); return false; }
            CarryingCandy = true;
            HeldCandyHue = hue;
            CandyPalette.Paint(heldCandy.transform, hue);
            heldCandy.SetActive(true);
            DreamGame.Instance.audioSystem.Play("pickup", .5f);
            return true;
        }
        public bool SwapCandy(CandyHue hue)
        {
            if (!CarryingCandy || HeldCandyHue == hue || DreamGame.Instance.Phase != GamePhase.Playing) return false;
            HeldCandyHue = hue;
            CandyPalette.Paint(heldCandy.transform, hue);
            DreamGame.Instance.audioSystem.Play("pickup", .5f);
            DreamGame.Instance.ShowToast("Holding " + CandyPalette.Label(hue) + " candy. Match the King's color.");
            return true;
        }
        public bool Damage(int amount, bool unavoidable = false)
        {
            if (DreamGame.Instance.Phase != GamePhase.Playing || (!unavoidable && Invulnerable)) return false;
            Health = Mathf.Max(0, Health - amount);
            invulnerableUntil = Time.time + 1.05f;
            DreamGame.Instance.audioSystem.Play("hurt", .5f);
            DreamArt.Burst(transform.position + Vector3.up, "Candy", 10, 2);
            if (Health == 0) DreamGame.Instance.Defeat();
            return true;
        }
        public void Heal(int amount) { Health = Mathf.Min(maxHealth, Health + amount); }
        public void Teleport(Vector3 point)
        {
            controller.enabled = false;
            transform.position = point;
            controller.enabled = true;
            verticalSpeed = -2;
            IsJumping = false;
            airTime = 0;
            if (characterAnimation != null) characterAnimation.ResetJumpPose();
        }
        public void Fall()
        {
            Damage(1, true);
            Teleport(DreamGame.Instance.Checkpoint + Vector3.up * .15f);
            DreamGame.Instance.ShowToast("The dream caught you. Return to your last lantern.");
        }
        void Animate(float speed)
        {
            stride += Time.deltaTime * speed * 2.4f;
            attackPose = Mathf.Max(0, attackPose - Time.deltaTime);
            if (Time.time > lookUntil && moveDirection.sqrMagnitude > .02f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 14);
            float walk = speed > .1f ? Mathf.Sin(stride) * 25 : 0;
            if (characterAnimation != null) characterAnimation.Move(speed);
            else
            {
            leftLeg.localRotation = Quaternion.Euler(walk, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-walk, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-walk * .5f - (attackPose > 0 ? 50 : 0), 0, -8);
            rightArm.localRotation = Quaternion.Euler(walk * .5f - (attackPose > 0 ? 65 : 0), 0, 8);
            visual.localPosition = Vector3.up * (speed > .1f ? Mathf.Abs(Mathf.Sin(stride)) * .04f : Mathf.Sin(Time.time * 2) * .018f);
            }
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
                renderer.enabled = !Invulnerable || (int)(Time.time * 14) % 2 == 0;
        }
    }

}
