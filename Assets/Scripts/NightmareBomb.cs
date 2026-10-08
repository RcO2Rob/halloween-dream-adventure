using UnityEngine;

namespace LostDream
{
    public class NightmareBomb : MonoBehaviour
    {
        public float fuse = 1.5f;
        public float radius = 2.5f;
        public Transform fallingBomb, disc;
        float age;
        public static NightmareBomb Spawn(Vector3 point, float fuse)
        {
            var root = DreamArt.Group("Telegraphed pumpkin bomb", null, point);
            DreamArt.Ring(root.transform, 2.5f, "Warning", 40, .09f);
            var bomb = root.AddComponent<NightmareBomb>();
            bomb.fuse = fuse;
            bomb.disc = DreamArt.Shape("Danger area", PrimitiveType.Cylinder, root.transform, new Vector3(0, .02f, 0),
                new Vector3(5, .008f, 5), "WarningFade").transform;
            var visual = DreamArt.Group("Falling bomb", root.transform, Vector3.up * 11);
            DreamArt.Shape("Bomb", PrimitiveType.Sphere, visual.transform, Vector3.zero, Vector3.one * .65f, "PumpkinDark");
            DreamArt.Shape("Fuse", PrimitiveType.Cylinder, visual.transform, Vector3.up * .4f, new Vector3(.08f, .12f, .08f), "GlowGold");
            bomb.fallingBomb = visual.transform;
            DreamGame.Instance.audioSystem.Play("warning", .22f);
            return bomb;
        }
        void Update()
        {
            if (DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Playing)
            { if (DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Paused) Destroy(gameObject); return; }
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / fuse);
            fallingBomb.localPosition = Vector3.up * Mathf.Lerp(11, .4f, t * t);
            fallingBomb.Rotate(110 * Time.deltaTime, 180 * Time.deltaTime, 0);
            disc.localScale = new Vector3(radius * 2 * (.5f + t * .5f), .008f, radius * 2 * (.5f + t * .5f));
            if (age < fuse) return;
            PlayerMotor player = DreamGame.Instance.player;
            if (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z),
                new Vector2(transform.position.x, transform.position.z)) <= radius)
                player.Damage(1);
            DreamGame.Instance.audioSystem.Play("explosion", .5f);
            DreamArt.Burst(transform.position + Vector3.up * .3f, "Warning", 25, 5);
            Destroy(gameObject);
        }
    }
}
