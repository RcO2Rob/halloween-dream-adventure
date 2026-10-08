using System.Collections.Generic;
using UnityEngine;

namespace LostDream
{
    // Shared, serialized materials and meshes are authored by PrototypeBuilder.
    public static class DreamArt
    {
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        public static Material Mat(string name)
        {
            if (!materials.TryGetValue(name, out Material material) || material == null)
            {
                material = Resources.Load<Material>("Materials/" + name);
                if (material == null) material = new Material(Shader.Find("Standard"));
                materials[name] = material;
            }
            return material;
        }

        public static GameObject Shape(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, string material, bool collision = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(material);
            if (!collision)
            {
                Collider collider = go.GetComponent<Collider>();
                collider.enabled = false;
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);
            }
            return go;
        }

        public static GameObject Mesh(string name, string resource, Transform parent,
            Vector3 position, Vector3 scale, string material)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = Resources.Load<Mesh>("Meshes/" + resource);
            go.AddComponent<MeshRenderer>().sharedMaterial = Mat(material);
            return go;
        }

        public static GameObject Group(string name, Transform parent, Vector3 position)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        public static void Ring(Transform parent, float radius, string mat, int segments = 36, float width = .045f)
        {
            var line = parent.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = segments;
            line.startWidth = line.endWidth = width;
            line.sharedMaterial = Mat(mat);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, .04f, Mathf.Sin(angle) * radius));
            }
        }

        public static void Pumpkin(Transform parent, float size, bool face = true)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Mathf.PI * 2 * i / 8;
                Shape("Pumpkin rib", PrimitiveType.Sphere, parent,
                    new Vector3(Mathf.Cos(angle) * .22f, .48f, Mathf.Sin(angle) * .22f) * size,
                    new Vector3(.55f, .85f, .55f) * size, i % 2 == 0 ? "Pumpkin" : "PumpkinDark");
            }
            Shape("Stem", PrimitiveType.Cylinder, parent, new Vector3(0, .97f, 0) * size,
                new Vector3(.12f, .14f, .12f) * size, "Stem");
            if (!face) return;
            Mesh("Left eye", "Triangle", parent, new Vector3(-.17f, .61f, -.465f) * size,
                new Vector3(.22f, .2f, .1f) * size, "GlowGold");
            Mesh("Right eye", "Triangle", parent, new Vector3(.17f, .61f, -.465f) * size,
                new Vector3(.22f, .2f, .1f) * size, "GlowGold");
            Shape("Smile", PrimitiveType.Cube, parent, new Vector3(0, .34f, -.46f) * size,
                new Vector3(.47f, .08f, .055f) * size, "GlowGold");
            for (int i = -1; i <= 1; i++)
                Shape("Tooth", PrimitiveType.Cube, parent, new Vector3(i * .14f, .38f, -.495f) * size,
                    new Vector3(.055f, .075f, .04f) * size, "PumpkinDark");
        }

        public static GameObject CandyVisual(Transform parent, Vector3 position, float size, CandyHue hue = CandyHue.Pink)
        {
            var root = Group("Wrapped candy", parent, position);
            Shape("Sweet", PrimitiveType.Sphere, root.transform, Vector3.zero,
                new Vector3(.7f, .45f, .45f) * size, CandyPalette.Material(hue));
            for (int i = -1; i <= 1; i += 2)
            {
                var wrapper = Mesh("Wrapper", "Cone", root.transform, new Vector3(i * .43f, 0, 0) * size,
                    new Vector3(.35f, .4f, .35f) * size, "CandyWrap");
                wrapper.transform.localRotation = Quaternion.Euler(0, 0, i * -90);
            }
            return root;
        }

        public static void Burst(Vector3 position, string material, int count = 16, float power = 3)
        {
            for (int i = 0; i < count; i++)
            {
                var go = Shape("Dream spark", PrimitiveType.Sphere, null, position,
                    Vector3.one * Random.Range(.055f, .14f), material);
                var spark = go.AddComponent<DreamSpark>();
                spark.velocity = Random.onUnitSphere * power + Vector3.up * 1.4f;
                spark.lifetime = Random.Range(.35f, .8f);
            }
        }
    }

    public class DreamSpark : MonoBehaviour
    {
        public Vector3 velocity;
        public float lifetime = .5f;
        float age;
        void Update()
        {
            age += Time.deltaTime;
            velocity += Vector3.down * 6 * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.localScale *= Mathf.Pow(.12f, Time.deltaTime);
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
