using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Edits the saved scenes: never calls DreamWorld.Build or regenerates map geometry.
public static class PolyHalloweenSceneDresser
{
    const string Source = "Assets/polyperfect/Poly Halloween/Prefabs/";
    const string GroupName = "Poly Halloween scenery";
    static readonly List<ChapterReport> chapters = new List<ChapterReport>();
    static int count;

    [MenuItem("Lost Dream/Art/Add Poly Halloween scenery to saved chapters")]
    public static void DressSavedScenes()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        if (!AssetDatabase.IsValidFolder(Source.TrimEnd('/'))) throw new FileNotFoundException("Restore Poly Halloween assets first.");
        chapters.Clear();
        for (int level = 0; level < DreamGame.SceneNames.Length; level++)
        {
            string path = "Assets/Scenes/" + DreamGame.SceneNames[level] + ".unity";
            var scene = EditorSceneManager.OpenScene(path);
            var game = Object.FindFirstObjectByType<DreamGame>();
            if (game.transform.Find(GroupName) != null) continue;
            var originalTransforms = game.GetComponentsInChildren<Transform>(true);
            var originalPhysicsAndGameplay = game.GetComponentsInChildren<Component>(true).Where(c =>
                c is Collider || c is MonoBehaviour || c is Camera || c is Light || c is Rigidbody).ToArray();
            string beforeGeometry = GeometrySignature(originalTransforms);
            string beforeGameplay = ComponentSignature(originalPhysicsAndGameplay);
            count = 0;
            var root = new GameObject(GroupName).transform; root.SetParent(game.transform, false);
            var islands = game.GetComponentsInChildren<BoxCollider>().Where(x => x.name == "Solid dream island")
                .Select(x => x.transform.parent).ToArray();
            int hiddenRenderers = 0;
            for (int islandIndex = 0; islandIndex < islands.Length; islandIndex++)
            {
                Transform island = islands[islandIndex];
                var trees = island.GetComponentsInChildren<Transform>().Where(x => x.name == "Dream orchard tree").ToArray();
                for (int i = 0; i < trees.Length; i++)
                {
                    if (level == 0 && i % 3 != 0) continue;
                    float height = level == 0 ? 4 : i % 3 == 0 ? 4.9f : 5.1f;
                    string name = level == 0 || i % 3 != 0 ? "Nature/Trees/Tree_Poplar_Dead_A" : "Halloween/Tree_Scary_Dead";
                    Place(root, name, trees[i].position, height, (i * 73 + islandIndex * 41) % 360);
                    hiddenRenderers += HideArtwork(trees[i]);
                }
                foreach (var pumpkin in island.GetComponentsInChildren<Transform>().Where(x => x.name == "Autumn pumpkin").ToArray())
                {
                    float height = Mathf.Clamp(BoundsOf(pumpkin.gameObject).size.y, .5f, 1.05f);
                    Place(root, "Halloween/Pumpkin_A", pumpkin.position, height, 170);
                    hiddenRenderers += HideArtwork(pumpkin);
                }
                if (level == 1)
                {
                    var floor = island.Find("Solid dream island").GetComponent<BoxCollider>();
                    float halfWidth = floor.size.x * Mathf.Abs(floor.transform.localScale.x) / 2;
                    float halfDepth = floor.size.z * Mathf.Abs(floor.transform.localScale.z) / 2;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        for (int row = 0; row < 3; row++)
                        {
                            Vector3 point = island.TransformPoint(new Vector3(side * (halfWidth - 3), 0,
                                -halfDepth * .5f + row * halfDepth * .42f));
                            if (!SafeForScenery(game, point, 2.8f)) continue;
                            Place(root, "Halloween/" + (row % 2 == 0 ? "Gravestone_Old_Classic_Cross" : "Gravestone_Old_Small_C"),
                                point, 1.2f + row * .12f, 180 + side * 12);
                        }
                    }
                    Vector3 corner = island.TransformPoint(new Vector3(-halfWidth + 3.5f, 0, -halfDepth + 2.5f));
                    if (SafeForScenery(game, corner, 3)) Place(root, "Halloween/Coffin_Old", corner, .46f, 20);
                }
            }

            if (level == 2)
            {
                var approach = islands.Single(x => x.name == "Castle approach");
                var court = islands.Single(x => x.name == "Pumpkin keep courtyard");
                for (int side = -1; side <= 1; side += 2)
                {
                    Place(root, "Modular Parts/Fences/Wall_Graveyard_Pillar", approach.TransformPoint(new Vector3(side * 6.6f, 0, 2.8f)), 2.1f, 0);
                    Place(root, "Halloween/Pumpkin_Carved_Lit_B", approach.TransformPoint(new Vector3(side * 6.6f, 2.1f, 2.8f)), .68f, 180);
                    Place(root, "Halloween/Pumpkin_A", approach.TransformPoint(new Vector3(side * 6.6f, 0, -3)), .85f, 180);
                    for (int row = 0; row < 3; row++)
                    {
                        Vector3 point = court.TransformPoint(new Vector3(side * 12.3f, 0, -5 + row * 7));
                        Place(root, "Halloween/Gravestone_Old_Small_C", point, 1.2f, -side * 80);
                        Place(root, "Halloween/Candle_Stick_B_Lit", court.TransformPoint(new Vector3(side * 14, 4.05f, -6 + row * 7)), .65f, 0);
                    }
                }
                Place(root, "Halloween/Cauldron_Potion_Full", approach.TransformPoint(new Vector3(-6.2f, 0, -2)), 1.3f, 150);
                Place(root, "Halloween/Candle_Stick_C_Lit", approach.TransformPoint(new Vector3(6.1f, 0, -1)), 1, 0);
                Place(root, "Halloween/Spiderweb_A", court.TransformPoint(new Vector3(-11, 2.7f, 12.3f)), 1.3f, 180);
                Place(root, "Halloween/Spiderweb_B", court.TransformPoint(new Vector3(11, 2.7f, 12.3f)), 1.3f, 180);
            }

            foreach (var lantern in game.lanterns)
            {
                hiddenRenderers += HideArtwork(lantern.transform.Find("Jack-o-lantern"));
                var skin = lantern.gameObject.AddComponent<LanternSkin>(); skin.lantern = lantern;
                skin.unlit = Place(lantern.transform, "Halloween/Pumpkin_Carved_B", lantern.transform.TransformPoint(Vector3.up * .6f), 1.05f, 180);
                skin.lit = Place(lantern.transform, "Halloween/Pumpkin_Carved_Lit_B", lantern.transform.TransformPoint(Vector3.up * .6f), 1.05f, 180);
                skin.unlit.name = "Poly Halloween - unlit switch"; skin.lit.name = "Poly Halloween - lit switch";
                skin.Refresh();
                EditorUtility.SetDirty(skin);
            }

            string afterGeometry = GeometrySignature(originalTransforms);
            string afterGameplay = ComponentSignature(originalPhysicsAndGameplay);
            if (beforeGeometry != afterGeometry || beforeGameplay != afterGameplay)
                throw new InvalidOperationException("Existing map geometry, physics or gameplay component changed in " + path);
            if (root.GetComponentsInChildren<Collider>(true).Any(x => x.enabled))
                throw new InvalidOperationException("Scenery must not introduce physical obstacles.");
            EditorSceneManager.SaveScene(scene);
            chapters.Add(new ChapterReport {
                scene = path, propInstances = count, replacedRenderers = hiddenRenderers,
                preservedTransformCount = originalTransforms.Length,
                preservedGameplayAndPhysicsComponentCount = originalPhysicsAndGameplay.Length,
                geometrySignatureBefore = beforeGeometry, geometrySignatureAfter = afterGeometry,
                gameplaySignatureBefore = beforeGameplay, gameplaySignatureAfter = afterGameplay,
                originalMapAndGameplayPreserved = true
            });
            CaptureChapter(game, "docs/qa/v0.11.0/" + DreamGame.SceneNames[level] + "-art.png");
            Debug.Log("POLY_HALLOWEEN_CHAPTER_READY: " + (level + 1) + " props=" + count);
        }
        if (chapters.Count == 0) return;
        Directory.CreateDirectory("docs/qa/v0.11.0");
        File.WriteAllText("docs/qa/v0.11.0/scene-art-audit.json", JsonUtility.ToJson(new Report { chapters = chapters.ToArray() }, true));
        AssetDatabase.SaveAssets();
    }

    public static void DressAndBuild()
    {
        DressSavedScenes();
        PlayerSettings.bundleVersion = "0.11.0";
        PrototypeBuilder.ValidateScenes();
        PrototypeBuilder.BuildMac();
    }

    static bool SafeForScenery(DreamGame game, Vector3 point, float radius)
    {
        if (Vector3.Distance(point, game.spawnPoint) < 4) return false;
        if (game.lanterns.Any(x => Vector3.Distance(point, x.transform.position) < radius || Vector3.Distance(point, x.safeCheckpoint) < radius)) return false;
        if (game.runes.Any(x => Vector3.Distance(point, x.transform.position) < radius)) return false;
        if (game.candySpawnPoints != null && game.candySpawnPoints.Any(x => Vector3.Distance(point, x) < radius)) return false;
        return true;
    }

    static int HideArtwork(Transform root)
    {
        int count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false; EditorUtility.SetDirty(renderer); count++;
        }
        return count;
    }

    static GameObject Place(Transform parent, string name, Vector3 position, float height, float yaw)
    {
        string path = Source + name + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new FileNotFoundException(path);
        var anchor = new GameObject(Path.GetFileName(name)); anchor.transform.SetParent(parent, false);
        anchor.transform.position = position; anchor.transform.rotation = Quaternion.Euler(0, yaw, 0);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor.transform);
        Bounds bounds = BoundsOf(instance);
        if (bounds.size.y < .0001f) throw new InvalidOperationException("Empty scenery: " + name);
        instance.transform.localScale *= height / bounds.size.y;
        bounds = BoundsOf(instance);
        instance.transform.position += new Vector3(position.x - bounds.center.x, position.y - bounds.min.y, position.z - bounds.center.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
        }
        foreach (var body in instance.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true; body.detectCollisions = false; PrefabUtility.RecordPrefabInstancePropertyModifications(body);
        }
        foreach (var script in instance.GetComponentsInChildren<MonoBehaviour>(true))
            if (script != null) throw new InvalidOperationException("Unexpected behaviour in scenery prefab: " + path);
        foreach (var transform in instance.GetComponentsInChildren<Transform>(true))
        {
            transform.gameObject.layer = 2; PrefabUtility.RecordPrefabInstancePropertyModifications(transform.gameObject);
        }
        anchor.gameObject.layer = 2;
        count++;
        return anchor;
    }

    static Bounds BoundsOf(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true).Where(x => !(x is ParticleSystemRenderer)).ToArray();
        if (renderers.Length == 0) throw new InvalidOperationException("Missing scenery mesh: " + go.name);
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static string GeometrySignature(IEnumerable<Transform> transforms)
    {
        return Hash(string.Join("\n", transforms.Select(t => {
            if (t == null) throw new InvalidOperationException("An existing map object was removed.");
            return t.GetEntityId() + "|" + t.name + "|" + (t.parent == null ? "root" : t.parent.GetEntityId().ToString()) + "|" +
                Vector(t.localPosition) + "|" + Vector(t.localScale) + "|" + t.localRotation.ToString("F9") + "|" +
                t.gameObject.activeSelf + "|" + t.gameObject.layer;
        })));
    }

    static string ComponentSignature(IEnumerable<Component> components)
    {
        return Hash(string.Join("\n", components.Select(c => c == null ? "REMOVED" : EditorJsonUtility.ToJson(c))));
    }

    static string Vector(Vector3 value) { return value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture); }
    static string Hash(string value)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
    }

    static void CaptureChapter(DreamGame game, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var camera = game.gameCamera;
        if (game.levelIndex == 2) { camera.transform.position = new Vector3(25, 22, -30); camera.transform.LookAt(new Vector3(0, 2, 8)); }
        else if (game.levelIndex == 1) { camera.transform.position = new Vector3(-34, 18, 7); camera.transform.LookAt(new Vector3(-18, 1, 25)); }
        else { camera.transform.position = new Vector3(17, 15, -17); camera.transform.LookAt(new Vector3(0, .5f, 1)); }
        var texture = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        var pixels = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
            pixels.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); pixels.Apply(); File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally { camera.targetTexture = null; RenderTexture.active = previous; texture.Release(); Object.DestroyImmediate(texture); Object.DestroyImmediate(pixels); }
    }

    [Serializable] class Report { public ChapterReport[] chapters; }
    [Serializable] class ChapterReport
    {
        public string scene, geometrySignatureBefore, geometrySignatureAfter, gameplaySignatureBefore, gameplaySignatureAfter;
        public int propInstances, replacedRenderers, preservedTransformCount, preservedGameplayAndPhysicsComponentCount;
        public bool originalMapAndGameplayPreserved;
    }
}
