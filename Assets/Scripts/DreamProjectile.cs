using UnityEngine;

namespace LostDream
{
    public class DreamProjectile : MonoBehaviour
    {
        public Vector3 velocity;
        public bool candy, hostile;
        public CandyHue hue;
        public string hostileMaterial = "GlowGold";
        float age;
        public static DreamProjectile Spawn(Vector3 origin, Vector3 velocity, bool candy, bool hostile = false,
            CandyHue hue = CandyHue.Pink, string hostileMaterial = "GlowGold")
        {
            GameObject root = DreamArt.Group(hostile ? "Dragon ember" : candy ? "Thrown candy" : "Arrow", null, origin);
            var projectile = root.AddComponent<DreamProjectile>();
            projectile.velocity = velocity;
            projectile.candy = candy;
            projectile.hostile = hostile;
            projectile.hue = hue;
            projectile.hostileMaterial = hostileMaterial;
            if (candy) DreamArt.CandyVisual(root.transform, Vector3.zero, .7f, hue);
            else if (hostile) DreamArt.Shape("Ember", PrimitiveType.Sphere, root.transform, Vector3.zero, Vector3.one * .28f, hostileMaterial);
            else
            {
                DreamArt.Shape("Shaft", PrimitiveType.Cube, root.transform, Vector3.zero, new Vector3(.055f, .055f, .9f), "Wood");
                var tip = DreamArt.Mesh("Arrowhead", "Cone", root.transform, Vector3.forward * .52f, new Vector3(.17f, .23f, .17f), "GlowTeal");
                tip.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            var trail = root.AddComponent<TrailRenderer>();
            trail.sharedMaterial = DreamArt.Mat(hostile ? hostileMaterial : candy ? CandyPalette.Material(hue) : "GlowTeal");
            trail.time = .16f;
            trail.startWidth = candy ? .16f : .07f;
            trail.endWidth = 0;
            return projectile;
        }
        void Update()
        {
            if (DreamGame.Instance != null && DreamGame.Instance.Phase == GamePhase.Paused) return;
            if (DreamGame.Instance == null || DreamGame.Instance.Phase != GamePhase.Playing) { Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            if (candy) velocity += Vector3.down * 12 * dt;
            Vector3 delta = velocity * dt;
            int mask = hostile ? ~0 : ~(1 << 2);
            if (Physics.SphereCast(transform.position, candy ? .22f : .11f, delta.normalized, out RaycastHit hit,
                delta.magnitude, mask, QueryTriggerInteraction.Ignore))
            {
                if (hostile)
                {
                    var player = hit.collider.GetComponentInParent<PlayerMotor>();
                    if (player != null) player.Damage(1);
                }
                else
                {
                    var dragon = hit.collider.GetComponentInParent<FlyingDragon>();
                    var rune = hit.collider.GetComponentInParent<DreamRune>();
                    var boss = hit.collider.GetComponentInParent<PumpkinBoss>();
                    if (candy && DreamGame.Instance.player.automation)
                        Debug.Log("CANDY_COLLISION: hue=" + hue + " target=" + hit.collider.name + " boss=" + (boss != null) + " point=" + hit.point);
                    if (dragon != null) dragon.Hit(candy ? 2 : 1);
                    else if (rune != null) rune.Hit(candy);
                    else if (boss != null) boss.Hit(candy, hue);
                }
                DreamArt.Burst(hit.point, candy ? CandyPalette.Material(hue) : hostile ? hostileMaterial : "GlowTeal", 7, 1.5f);
                Destroy(gameObject);
                return;
            }
            transform.position += delta;
            if (candy) transform.Rotate(270 * dt, 120 * dt, 90 * dt);
            else if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(velocity);
            age += dt;
            if (age > 4 || transform.position.y < -10) Destroy(gameObject);
        }
    }
}
