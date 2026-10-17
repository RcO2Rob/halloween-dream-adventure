using UnityEngine;

namespace LostDream
{
    [DefaultExecutionOrder(-20)]
    public class DreamCamera : MonoBehaviour
    {
        public Transform target;
        public bool arena;
        public float distance = 10.5f;
        public float pitch = 25;
        public float yaw;
        public bool MouseLookActive => DreamGame.Instance != null &&
            DreamGame.Instance.Phase == GamePhase.Playing && Application.isFocused &&
            Cursor.lockState == CursorLockMode.Locked;
        public float horizontalSensitivity = 3;
        public float verticalSensitivity = 2.5f;
        Camera view;
        Vector3 previousTarget, focus, lookAhead;
        bool initialized;
        float boomDistance;

        void Awake()
        {
            view = GetComponent<Camera>();
            if (!arena) return;
            // The castle's decorative roofs must obstruct the camera boom too.
            // Otherwise collecting sweets near a corner places the view inside a roof.
            foreach (var mesh in FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                if (mesh.name == "Tower roof" && mesh.GetComponent<Collider>() == null)
                    mesh.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh.sharedMesh;
        }
        void Update()
        {
            if (DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Playing) return;
            if (!MouseLookActive || !DreamGame.Instance.MouseActionsReady ||
                DreamGame.Instance.player.automation || Input.GetKeyDown(KeyCode.Escape)) return;
            ApplyLookInput(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > .001f) ZoomView(wheel * 12);
            if (Input.GetKeyDown(KeyCode.F)) ResetView();
        }
        public void ApplyLookInput(Vector2 delta)
        {
            if (DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Playing) return;
            RotateView(delta.x * horizontalSensitivity, -delta.y * verticalSensitivity);
        }
        public void RotateView(float horizontal, float vertical)
        {
            yaw = Mathf.Repeat(yaw + horizontal, 360);
            pitch = Mathf.Clamp(pitch + vertical, -25, 65);
        }
        public void ZoomView(float amount) { distance = Mathf.Clamp(distance - amount, 6.5f, 15); }
        public void ResetView() { yaw = 0; pitch = 25; distance = 10.5f; }
        public void Snap() { initialized = false; LateUpdate(); }
        void LateUpdate()
        {
            if (target == null) return;
            if (view == null) view = GetComponent<Camera>();
            view.orthographic = false; view.fieldOfView = 58;
            var game = DreamGame.Instance;
            if (game != null && game.Phase == GamePhase.Victory && game.boss != null)
            {
                Vector3 center = game.VictoryCenter;
                float width = game.boss.arenaMax.x - game.boss.arenaMin.x;
                Vector3 position = center + new Vector3(Mathf.Max(23, width * .74f), 22, -43);
                Quaternion rotation = Quaternion.LookRotation(center + new Vector3(0, 7, 4) - position);
                float blend = 1 - Mathf.Exp(-2.5f * Time.unscaledDeltaTime);
                transform.position = Vector3.Lerp(transform.position, position, blend);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, blend);
                return;
            }
            float dt = Mathf.Max(Time.unscaledDeltaTime, .001f);
            bool snap = !initialized || Vector3.Distance(previousTarget, target.position) > 8;
            Vector3 velocity = snap ? Vector3.zero : (target.position - previousTarget) / dt;
            velocity.y = 0;
            lookAhead = Vector3.Lerp(lookAhead, Vector3.ClampMagnitude(velocity * .18f, 1.2f), 1 - Mathf.Exp(-4 * dt));
            // An over-shoulder pivot leaves the center reticle beside the hero,
            // rather than drawing it over Timmy's head and bow arm.
            Vector3 shoulder = Quaternion.Euler(0, yaw, 0) * Vector3.right * 1.15f;
            Vector3 desiredFocus = target.position + Vector3.up * 1.7f + shoulder + lookAhead;
            float desiredDistance = distance, desiredPitch = pitch;
            var boss = DreamGame.Instance != null ? DreamGame.Instance.boss : null;
            if (arena && target.position.z > -2 && boss != null && !boss.Defeated)
            {
                Vector3 toBoss = boss.AimPosition - target.position;
                // Keep the player as the orbit pivot; the mouse owns both angles.
                // Boss framing must not pull the reticle away from a flying enemy.
                desiredDistance = Mathf.Clamp(distance + toBoss.magnitude * .32f, distance, 24);
            }
            focus = snap ? desiredFocus : Vector3.Lerp(focus, desiredFocus, 1 - Mathf.Exp(-9 * dt));
            Quaternion orbit = Quaternion.Euler(desiredPitch, yaw, 0);
            Vector3 backward = orbit * Vector3.back;
            float clearDistance = desiredDistance;
            if (Physics.SphereCast(focus, .3f, backward, out RaycastHit hit, desiredDistance, ~(1 << 2), QueryTriggerInteraction.Ignore))
                clearDistance = Mathf.Max(.55f, hit.distance - .25f);
            // Pull in immediately at obstacles, then ease back out after clearing them.
            boomDistance = snap || clearDistance < boomDistance ? clearDistance : Mathf.Lerp(boomDistance, clearDistance, 1 - Mathf.Exp(-5 * dt));
            transform.position = focus + backward * boomDistance;
            // Shoulder framing offsets the focus from the hero. Protect both rays
            // when a roof or wall clips a castle corner view.
            Vector3 heroAnchor = target.position + Vector3.up * 1.2f;
            Vector3 heroRay = transform.position - heroAnchor;
            bool heroObstructed = arena && heroRay.sqrMagnitude > .01f &&
                Physics.SphereCast(heroAnchor, .3f, heroRay.normalized, out hit, heroRay.magnitude,
                    ~(1 << 2), QueryTriggerInteraction.Ignore);
            if (heroObstructed)
                transform.position = heroAnchor + heroRay.normalized * Mathf.Max(.55f, hit.distance - .25f);
            transform.rotation = heroObstructed ? Quaternion.LookRotation(focus - transform.position) : orbit;
            previousTarget = target.position; initialized = true;
        }
    }
}
