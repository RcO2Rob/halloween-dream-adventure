using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LostDream
{
    // Fragments use the actual head/crown surfaces. No rigidbodies or collision.
    public class PumpkinBurst : MonoBehaviour
    {
        class Surface
        {
            public Material material;
            public readonly List<Vector3> vertices = new List<Vector3>();
        }
        class Piece
        {
            public Transform transform;
            public Vector3 origin, velocity, spin;
            public Quaternion rotation;
        }
        readonly List<Piece> pieces = new List<Piece>();
        readonly List<Mesh> meshes = new List<Mesh>();
        float age;
        public int PieceCount => pieces.Count;
        public float Age => age;
        public static PumpkinBurst Begin(PumpkinBoss boss)
        {
            var root = new GameObject("Pumpkin shell burst"); root.layer = 2;
            root.transform.SetParent(boss.transform.parent, false);
            var effect = root.AddComponent<PumpkinBurst>(); effect.Create(boss);
            return effect;
        }
        void Create(PumpkinBoss boss)
        {
            Vector3 center = boss.AimPosition;
            var surfaces = new Dictionary<string, Surface>();
            foreach (var part in new[] { boss.pumpkin, boss.crown })
            foreach (var filter in part.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                Mesh mesh = filter.sharedMesh;
                if (renderer == null || mesh == null || !mesh.isReadable) continue;
                var vertices = mesh.vertices;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    var material = renderer.sharedMaterials[Mathf.Min(sub, renderer.sharedMaterials.Length - 1)];
                    var triangles = mesh.GetTriangles(sub);
                    for (int t = 0; t < triangles.Length; t += 3)
                    {
                        Vector3 a = filter.transform.TransformPoint(vertices[triangles[t]]);
                        Vector3 b = filter.transform.TransformPoint(vertices[triangles[t + 1]]);
                        Vector3 c = filter.transform.TransformPoint(vertices[triangles[t + 2]]);
                        Vector3 direction = (a + b + c) / 3 - center;
                        int sector = Mathf.FloorToInt(Mathf.Repeat(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 360) / 60);
                        int cell = sector + (direction.y > 0 ? 6 : 0);
                        string key = material.GetEntityId() + "/" + cell;
                        if (!surfaces.TryGetValue(key, out Surface surface))
                        { surface = new Surface { material = material }; surfaces.Add(key, surface); }
                        surface.vertices.Add(a); surface.vertices.Add(b); surface.vertices.Add(c);
                    }
                }
            }
            var random = new System.Random(20261016);
            foreach (var surface in surfaces.Values)
            {
                Vector3 pivot = Vector3.zero;
                foreach (var v in surface.vertices) pivot += v;
                pivot /= surface.vertices.Count;
                var mesh = new Mesh { name = "Broken pumpkin shell", indexFormat = IndexFormat.UInt32 };
                var triangles = new int[surface.vertices.Count];
                for (int n = 0; n < triangles.Length; n++) { surface.vertices[n] -= pivot; triangles[n] = n; }
                mesh.SetVertices(surface.vertices); mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); meshes.Add(mesh);
                var piece = new GameObject("Pumpkin shell fragment"); piece.layer = 2;
                piece.transform.SetParent(transform, false); piece.transform.position = pivot;
                piece.AddComponent<MeshFilter>().sharedMesh = mesh;
                piece.AddComponent<MeshRenderer>().sharedMaterial = surface.material;
                Vector3 outward = Vector3.ProjectOnPlane(pivot - center, Vector3.up).normalized;
                float speed = 3.8f + (float)random.NextDouble() * 2.5f;
                pieces.Add(new Piece {
                    transform = piece.transform, origin = pivot, rotation = Quaternion.identity,
                    velocity = outward * speed + Vector3.up * (3.5f + (float)random.NextDouble() * 3),
                    spin = new Vector3((float)random.NextDouble() * 150 - 75,
                        (float)random.NextDouble() * 160 - 80, (float)random.NextDouble() * 150 - 75)
                });
            }
        }
        void Update()
        {
            age += Time.unscaledDeltaTime;
            float size = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.8f, 3.2f, age));
            foreach (var piece in pieces)
            {
                piece.transform.position = piece.origin + piece.velocity * age + Vector3.down * (2.8f * age * age);
                piece.transform.rotation = Quaternion.Euler(piece.spin * age) * piece.rotation;
                piece.transform.localScale = Vector3.one * size;
            }
            if (age >= 3.3f) Destroy(gameObject);
        }
        void OnDestroy() { foreach (var mesh in meshes) if (mesh != null) Destroy(mesh); }
    }
}
