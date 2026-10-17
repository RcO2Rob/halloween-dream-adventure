using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Works on the saved maps and any currently loaded edits, without regeneration.
[InitializeOnLoad]
public static class SideFenceBuilder
{
    const string Request = "Temp/LostDreamSideFences.request";
    const string Result = "Temp/LostDreamSideFences.result.json";
    const string Group = "Iron side fences";
    const string Source = "Assets/polyperfect/Poly Halloween/Prefabs/Modular Parts/Fences/";
    const string Evidence = "docs/qa/side-fences";
    const float Height = 1.55f;

    static SideFenceBuilder()
    {
        EditorApplication.delayCall += RunRequested;
    }

    static void RunRequested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += RunRequested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += RunRequested; return; }
        File.Delete(Request);
        try
        {
            AddToSavedChapters();
            File.WriteAllText(Result, "{\"passed\":true}");
        }
        catch (Exception error)
        {
            File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true));
            Debug.LogException(error);
        }
    }

    [MenuItem("Lost Dream/Map/Add iron side fences to saved chapters")]
    public static void AddToSavedChapters()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before editing maps.");
        var originalActive = SceneManager.GetActiveScene();
        var reports = new List<Chapter>();
        Directory.CreateDirectory(Evidence);
        string backup = "Builds/archive/before-side-fences-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(backup);
        for (int i = 0; i < DreamGame.SceneNames.Length; i++)
        {
            string path = "Assets/Scenes/" + DreamGame.SceneNames[i] + ".unity";
            Scene scene = SceneManager.GetSceneByPath(path);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!alreadyLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            if (game.transform.Find(Group) != null)
            {
                reports.Add(Inspect(game));
                if (!alreadyLoaded) EditorSceneManager.CloseScene(scene, true);
                continue;
            }
            // Preserve both the on-disk map and any unsaved edits in the open scene.
            File.Copy(path, Path.Combine(backup, Path.GetFileName(path)), false);
            File.Copy(path + ".meta", Path.Combine(backup, Path.GetFileName(path) + ".meta"), false);
            if (!EditorSceneManager.SaveScene(scene, Path.Combine(backup, "open-" + Path.GetFileName(path)), true))
                throw new IOException("Could not back up the open map: " + path);
            var original = game.GetComponentsInChildren<Component>(true);
            string[] before = original.Select(Signature).ToArray();
            var root = new GameObject(Group); root.transform.SetParent(game.transform, false);
            Undo.RegisterCreatedObjectUndo(root, "Add iron side fences");
            var floors = game.GetComponentsInChildren<BoxCollider>(true).Where(x => x.name == "Solid dream island").ToArray();
            var paths = RouteBoxes(game);
            foreach (var floor in floors) AddIsland(root.transform, floor, paths, game.GetComponentsInChildren<DreamPath>(true));
            Physics.SyncTransforms();
            for (int n = 0; n < original.Length; n++)
                if (original[n] == null || Signature(original[n]) != before[n])
                    throw new InvalidOperationException("An existing component changed: " + (original[n] == null ? "removed" : original[n].name));
            var report = Inspect(game); report.existingComponentsPreserved = original.Length;
            report.backup = backup; reports.Add(report);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + path);
            Debug.Log("LOST_DREAM_SIDE_FENCES_SAVED: " + path + " panels=" + report.panels);
            if (!alreadyLoaded) EditorSceneManager.CloseScene(scene, true);
        }
        if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
        File.WriteAllText(Evidence + "/placement-audit.json", JsonUtility.ToJson(new Report { chapters = reports.ToArray() }, true));
        AssetDatabase.SaveAssets();
        var activeGame = originalActive.IsValid() ? originalActive.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).FirstOrDefault() : null;
        if (activeGame != null)
        {
            Selection.activeGameObject = activeGame.transform.Find(Group).gameObject;
            var view = SceneView.lastActiveSceneView;
            if (view != null) view.Repaint();
        }
    }

    static string Signature(Component component)
    {
        // Transform child lists gain the fence group; existing transforms themselves must not move.
        if (component is Transform t)
            return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" +
                t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(component);
    }

    static BoxCollider[] RouteBoxes(DreamGame game)
    {
        return game.GetComponentsInChildren<DreamPath>(true)
            .SelectMany(x => x.solid.GetComponentsInChildren<BoxCollider>(true)).ToArray();
    }

    static bool OnRoute(Vector3 point, BoxCollider box, float margin)
    {
        Vector3 local = box.transform.InverseTransformPoint(point) - box.center;
        Vector3 scale = box.transform.lossyScale;
        return Mathf.Abs(local.x) <= box.size.x / 2 + margin / Mathf.Abs(scale.x) &&
            Mathf.Abs(local.z) <= box.size.z / 2 + margin / Mathf.Abs(scale.z);
    }

    static bool OnJumpRoute(Vector3 point, DreamPath path)
    {
        if (!path.IsJumpRoute) return false;
        Vector3 local = path.transform.InverseTransformPoint(point);
        return Mathf.Abs(local.x) < 2.7f && local.z > path.jumpEntry.localPosition.z - 1 && local.z < path.jumpExit.localPosition.z + 1;
    }
    static void AddIsland(Transform root, BoxCollider floor, BoxCollider[] paths, DreamPath[] routes)
    {
        var island = new GameObject(floor.transform.parent.name); island.transform.SetParent(root, false);
        Vector3 right = floor.transform.right.normalized, forward = floor.transform.forward.normalized;
        Vector3 top = floor.transform.TransformPoint(floor.center + Vector3.up * floor.size.y / 2);
        float width = floor.size.x * Mathf.Abs(floor.transform.lossyScale.x);
        float depth = floor.size.z * Mathf.Abs(floor.transform.lossyScale.z);
        foreach (int side in new[] { -1, 1 })
        {
            var edge = new GameObject(side < 0 ? "Left edge" : "Right edge"); edge.transform.SetParent(island.transform, false);
            Vector3 origin = top + right * side * (width / 2 - .18f) - forward * depth / 2;
            // Short samples leave a generous opening where diagonal bridges meet a side.
            int steps = Mathf.CeilToInt(depth / .2f); float step = depth / steps; int start = -1;
            for (int n = 0; n <= steps; n++)
            {
                Vector3 sample = origin + forward * ((n + .5f) * step);
                bool clear = n < steps && !paths.Any(p => OnRoute(sample, p, .85f)) && !routes.Any(p => OnJumpRoute(sample, p));
                if (clear && start < 0) start = n;
                if (!clear && start >= 0)
                {
                    float length = (n - start) * step;
                    if (length > .7f) AddRun(edge.transform, origin + forward * (start * step), forward, length);
                    start = -1;
                }
            }
        }
    }
    public static void RebuildCourtyard(DreamGame game, BoxCollider floor)
    {
        Transform root = game.transform.Find(Group);
        if (root == null) throw new InvalidOperationException("Saved side fences are missing.");
        Transform previous = root.Find(floor.transform.parent.name);
        if (previous != null) Object.DestroyImmediate(previous.gameObject);
        AddIsland(root, floor, RouteBoxes(game), game.GetComponentsInChildren<DreamPath>(true));
    }

    public static Transform AddFenceSegment(Transform parent, Vector3 from, Vector3 to, float height)
    {
        Vector3 delta = to - from;
        return AddRun(parent, from, delta.normalized, delta.magnitude, height);
    }
    static Transform AddRun(Transform parent, Vector3 from, Vector3 forward, float length, float height = Height)
    {
        var run = new GameObject("Fence run"); run.transform.SetParent(parent, false);
        run.transform.position = from + forward * length / 2;
        run.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        int panels = Mathf.CeilToInt(length / 2.8f); float spacing = length / panels;
        for (int n = 0; n < panels; n++)
            Prop(run.transform, "Wall_Graveyard_3m", new Vector3(0, 0, -length / 2 + spacing * (n + .5f)), height, spacing, 90);
        // Pillars cap each run and repeat every roughly eight metres.
        int bays = Mathf.CeilToInt(length / 8);
        for (int n = 0; n <= bays; n++)
            Prop(run.transform, "Wall_Graveyard_Pillar", new Vector3(0, 0, -length / 2 + length * n / bays), height + .22f, 0, 0);
        var collider = run.AddComponent<BoxCollider>();
        collider.center = Vector3.up * height / 2;
        collider.size = new Vector3(.34f, height, length);
        return run.transform;
    }

    static void Prop(Transform parent, string name, Vector3 local, float height, float length, float yaw)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".prefab");
        if (asset == null) throw new FileNotFoundException(Source + name);
        var anchor = new GameObject(length > 0 ? "Iron railing" : "Stone end post");
        anchor.transform.SetParent(parent, false); anchor.transform.localPosition = local;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, anchor.transform);
        instance.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        Bounds bounds = BoundsOf(instance);
        instance.transform.localScale *= height / bounds.size.y;
        bounds = BoundsOf(instance);
        if (length > 0)
        {
            Vector3 scale = instance.transform.localScale;
            // The panel spans its own X axis. Measure along the actual world-space
            // run direction, including transverse exit fences, rather than world Z.
            Vector3 direction = parent.forward.normalized;
            float span = Mathf.Abs(direction.x) * bounds.size.x + Mathf.Abs(direction.z) * bounds.size.z;
            scale.x *= length / Mathf.Max(.001f, span); instance.transform.localScale = scale;
        }
        bounds = BoundsOf(instance);
        instance.transform.position += new Vector3(anchor.transform.position.x - bounds.center.x,
            anchor.transform.position.y - bounds.min.y, anchor.transform.position.z - bounds.center.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        foreach (var c in instance.GetComponentsInChildren<Collider>(true))
        { c.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(c); }
        foreach (var body in instance.GetComponentsInChildren<Rigidbody>(true))
        { body.isKinematic = true; body.detectCollisions = false; PrefabUtility.RecordPrefabInstancePropertyModifications(body); }
        foreach (var t in instance.GetComponentsInChildren<Transform>(true))
        { t.gameObject.layer = 2; PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject); }
        if (instance.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
            throw new InvalidOperationException("Fence prefab contains unexpected scripts.");
    }

    static Bounds BoundsOf(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static Chapter Inspect(DreamGame game)
    {
        Transform root = game.transform.Find(Group);
        if (root == null) throw new InvalidOperationException("Missing side fences.");
        var runs = root.GetComponentsInChildren<BoxCollider>(true);
        if (runs.Length < root.childCount * 2) throw new InvalidOperationException("Missing left or right fence.");
        foreach (var island in root.Cast<Transform>())
            foreach (string side in new[] { "Left edge", "Right edge" })
                if (island.Find(side).childCount == 0) throw new InvalidOperationException("Empty edge: " + island.name + " " + side);
        foreach (var run in runs)
        {
            if (!run.enabled || run.isTrigger || run.gameObject.layer != 0) throw new InvalidOperationException("Fence barrier is inactive.");
            var ray = new Ray(run.transform.position + Vector3.up * .8f - run.transform.right * 1.5f, run.transform.right);
            if (!run.Raycast(ray, out _, 3)) throw new InvalidOperationException("Fence does not block its side.");
            for (float z = -run.size.z / 2; z <= run.size.z / 2; z += .1f)
                if (RouteBoxes(game).Any(p => OnRoute(run.transform.TransformPoint(new Vector3(0, 0, z)), p, .55f)))
                    throw new InvalidOperationException("Fence obstructs a road: " + run.transform.parent.parent.name);
        }
        return new Chapter {
            scene = game.gameObject.scene.path, islands = root.childCount, fenceRuns = runs.Length,
            panels = root.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Iron railing"),
            height = Height, physicalBarriersValidated = true, bridgeOpeningsValidated = true
        };
    }

    // Run in an isolated project to inspect each saved map without touching open edits.
    public static void ApplyAndValidate()
    {
        AddToSavedChapters();
        ValidateAndCapture();
    }

    public static void ValidateAndCapture()
    {
        Directory.CreateDirectory(Evidence);
        var chapters = new List<Chapter>();
        foreach (string name in DreamGame.SceneNames)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            var game = Object.FindAnyObjectByType<DreamGame>(); Physics.SyncTransforms();
            chapters.Add(Inspect(game));
            Capture(game, Evidence + "/" + name + ".png", false);
            Capture(game, Evidence + "/" + name + "-overview.png", true);
        }
        File.WriteAllText(Evidence + "/saved-scene-validation.json", JsonUtility.ToJson(new Report { chapters = chapters.ToArray() }, true));
        PrototypeBuilder.ValidateScenes();
        Debug.Log("LOST_DREAM_SIDE_FENCES_VALIDATED");
    }

    static void Capture(DreamGame game, string path, bool overview)
    {
        var camera = game.gameCamera;
        Vector3 focus = overview
            ? (game.boss != null ? new Vector3(0, 1, -5) : game.levelIndex == 1 ? new Vector3(-6, 0, 18) : new Vector3(0, 0, 7))
            : (game.boss != null ? new Vector3(0, 0, -12) : new Vector3(0, 0, 1));
        camera.transform.position = focus + (overview ? new Vector3(30, 28, -40) : new Vector3(18, 17, -23));
        camera.transform.LookAt(focus);
        var target = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        var pixels = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        var before = RenderTexture.active;
        try {
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); pixels.Apply(); File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally { camera.targetTexture = null; RenderTexture.active = before; target.Release(); Object.DestroyImmediate(target); Object.DestroyImmediate(pixels); }
    }

    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Report { public Chapter[] chapters; }
    [Serializable] class Chapter {
        public string scene, backup;
        public int islands, fenceRuns, panels, existingComponentsPreserved;
        public float height;
        public bool physicalBarriersValidated, bridgeOpeningsValidated;
    }
}
