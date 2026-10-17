using UnityEngine;

namespace LostDream
{
    [DefaultExecutionOrder(10)]
    public class DriftingPlatform : MonoBehaviour
    {
        public float amplitude = .45f, speed = 1.1f, phase;
        Vector3 origin;
        float clock;
        void Awake() { origin = transform.localPosition; }
        void Update()
        {
            var game = DreamGame.Instance;
            if (game == null || game.Phase != GamePhase.Playing) return;
            var player = game.player;
            bool riding = player.IsGrounded && Physics.Raycast(player.transform.position + Vector3.up * .15f,
                Vector3.down, out RaycastHit hit, .4f, ~(1 << 2), QueryTriggerInteraction.Ignore) && hit.collider.transform.IsChildOf(transform);
            Vector3 before = transform.position;
            clock += Time.deltaTime;
            transform.localPosition = origin + Vector3.right * (Mathf.Sin(clock * speed + phase) - Mathf.Sin(phase)) * amplitude;
            if (riding) player.GetComponent<CharacterController>().Move(transform.position - before);
        }
    }
}
