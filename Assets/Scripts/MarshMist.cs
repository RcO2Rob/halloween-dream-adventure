using UnityEngine;

namespace LostDream
{
    public class MarshMist : MonoBehaviour
    {
        public WitchCauldron source;
        public Renderer warning;
        public Transform plume;
        public float offset;
        public float radius = 2.6f;
        public bool Active { get; private set; }
        public bool Warning { get; private set; }
        float clock, hurtAt;
        void Update()
        {
            var game = DreamGame.Instance;
            if (game == null || game.Phase != GamePhase.Playing) return;
            clock += Time.deltaTime;
            float cycle = (clock + offset) % 8;
            Warning = !source.Complete && cycle >= 4 && cycle < 5.5f;
            Active = !source.Complete && cycle >= 5.5f && cycle < 7.5f;
            warning.gameObject.SetActive(Warning || Active);
            warning.sharedMaterial = DreamArt.Mat(Active ? "Candy" : "GlowGold");
            plume.gameObject.SetActive(Active);
            if (Active)
            {
                plume.localScale = new Vector3(radius * 1.8f, .6f + Mathf.Sin(clock * 5) * .12f, radius * 1.8f);
                Vector3 delta = game.player.transform.position - transform.position;
                if (new Vector2(delta.x, delta.z).magnitude < radius && delta.y < 1.1f && delta.y > -.3f && Time.time > hurtAt)
                { hurtAt = Time.time + 1.2f; game.player.Damage(1); }
            }
        }
    }
}
