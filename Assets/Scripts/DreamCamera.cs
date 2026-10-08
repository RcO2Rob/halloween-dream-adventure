using UnityEngine;

namespace LostDream
{
    public class DreamCamera : MonoBehaviour
    {
        public Transform target;
        public bool arena;
        public float distance = 10.5f;
        public float pitch = 25;
        public float yaw;
        public bool IsOrbiting => Input.GetMouseButton(2) ||
            ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(0));
        Camera view;
        Vector3 previousTarget, focus, lookAhead;
        bool initialized;
        float manualUntil, boomDistance;

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
            if (IsOrbiting) RotateView(Input.GetAxis("Mouse X") * 3, -Input.GetAxis("Mouse Y") * 2.5f);
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > .001f) ZoomView(wheel * 12);
            if (Input.GetKeyDown(KeyCode.F)) ResetView();
        }
        public void RotateView(float horizontal, float vertical)
        {
            yaw += horizontal; pitch = Mathf.Clamp(pitch + vertical, 15, 48);
            manualUntil = Time.unscaledTime + 4;
        }
        public void ZoomView(float amount) { distance = Mathf.Clamp(distance - amount, 6.5f, 15); }
        public void ResetView() { yaw = 0; pitch = 25; distance = 10.5f; manualUntil = 0; }
        public void Snap() { initialized = false; LateUpdate(); }
        void LateUpdate()
        {
            if (target == null) return;
            if (view == null) view = GetComponent<Camera>();
            view.orthographic = false; view.fieldOfView = 58;
            float dt = Mathf.Max(Time.unscaledDeltaTime, .001f);
            bool snap = !initialized || Vector3.Distance(previousTarget, target.position) > 8;
            Vector3 velocity = snap ? Vector3.zero : (target.position - previousTarget) / dt;
            velocity.y = 0;
            lookAhead = Vector3.Lerp(lookAhead, Vector3.ClampMagnitude(velocity * .18f, 1.2f), 1 - Mathf.Exp(-4 * dt));
            Vector3 desiredFocus = target.position + Vector3.up * 1.2f + lookAhead;
            float desiredDistance = distance, desiredPitch = pitch;
            var boss = DreamGame.Instance != null ? DreamGame.Instance.boss : null;
            if (arena && target.position.z > -2 && boss != null && !boss.Defeated)
            {
                Vector3 toBoss = boss.AimPosition - target.position;
                desiredFocus = Vector3.Lerp(target.position + Vector3.up * 1.2f, boss.AimPosition, .3f);
                desiredDistance = Mathf.Clamp(distance + toBoss.magnitude * .32f, distance, 24);
                desiredPitch = Mathf.Max(pitch, 32);
                if (Time.unscaledTime > manualUntil)
                    yaw = Mathf.LerpAngle(yaw, Mathf.Atan2(toBoss.x, toBoss.z) * Mathf.Rad2Deg, snap ? 1 : 1 - Mathf.Exp(-2 * dt));
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
            // Arena focus sits between player and boss. Protect the player's sightline
            // as well as that focus ray when a roof or wall clips a castle corner view.
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
