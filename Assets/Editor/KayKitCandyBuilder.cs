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

[InitializeOnLoad]
public static class KayKitCandyBuilder
{
    const string Source = "Assets/ThirdParty/KayKit/Spooktober/Models/candyA.fbx";
    const string Prefab = "Assets/Resources/Props/KayKitCandy.prefab";
    const string Request = "Temp/LostDreamKayKitCandy.request";
    const string Result = "Temp/LostDreamKayKitCandy.result.json";
    const string Evidence = "docs/qa/kaykit-candy";

    static KayKitCandyBuilder()
    {
        EditorApplication.delayCall += RunRequested;
        EditorApplication.projectChanged += RunRequested;
    }

    static void RunRequested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += RunRequested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += RunRequested; return; }
        File.Delete(Request);
        try { ReplaceSavedCandy(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) {
            File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true));
            Debug.LogException(error);
        }
    }

    public static void BuildAndApply()
    {
        BuildResources();
        ReplaceSavedCandy();
        PrototypeBuilder.BuildCurrentScenes();
    }

    [MenuItem("Lost Dream/Art/Build KayKit candy prefab")]
    public static void BuildResources()
    {
        Directory.CreateDirectory("Assets/Resources/Props"); AssetDatabase.Refresh();
        var importer = (ModelImporter)AssetImporter.GetAtPath(Source);
        if (importer == null) throw new FileNotFoundException(Source);
        importer.isReadable = true; importer.importAnimation = false;
        importer.importCameras = false; importer.importLights = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.SaveAndReimport();
        var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Source));
        var filters = model.GetComponentsInChildren<MeshFilter>();
        if (filters.Length != 1) throw new InvalidOperationException("Expected one KayKit candy mesh.");
        var filter = filters[0]; var renderer = filter.GetComponent<MeshRenderer>();
        var original = filter.sharedMesh;
        var colors = renderer.sharedMaterials;
        Debug.Log("KAYKIT_CANDY_SOURCE: " + string.Join(",", colors.Select(x => x.name)) + " submeshes=" + original.subMeshCount);
        var body = new List<int>(); var stripes = new List<int>();
        for (int i = 0; i < original.subMeshCount; i++)
            (colors[i].name.IndexOf("White", StringComparison.OrdinalIgnoreCase) >= 0 ? stripes : body).AddRange(original.GetTriangles(i));
        if (body.Count == 0 || stripes.Count == 0) throw new InvalidOperationException("Original color and white stripe regions are required.");
        Matrix4x4 matrix = model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
        Vector3[] vertices = original.vertices.Select(matrix.MultiplyPoint3x4).ToArray();
        Bounds bounds = new Bounds(vertices[0], Vector3.zero);
        foreach (Vector3 v in vertices) bounds.Encapsulate(v);
        float fit = 1.15f / bounds.size.x;
        Vector3[] normalized = vertices.Select(v => (v - bounds.center) * fit).ToArray();
        Vector3[] normals = original.normals.Select(n => matrix.inverse.transpose.MultiplyVector(n).normalized).ToArray();
        Mesh colored = Part("KayKitCandyBody", original, normalized, normals, body);
        Mesh white = Part("KayKitCandyStripes", original, normalized, normals, stripes);
        SaveMesh(colored, "Assets/Resources/Meshes/KayKitCandyBody.asset");
        SaveMesh(white, "Assets/Resources/Meshes/KayKitCandyStripes.asset");
        Object.DestroyImmediate(model);
        var stripe = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/KayKitCandyStripe.mat");
        if (stripe == null) {
            stripe = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(stripe, "Assets/Resources/Materials/KayKitCandyStripe.mat");
        }
        stripe.color = new Color(.97f, .96f, .9f); stripe.SetFloat("_Glossiness", .3f);
        stripe.EnableKeyword("_EMISSION"); stripe.SetColor("_EmissionColor", stripe.color * .08f);
        EditorUtility.SetDirty(stripe);
        var root = new GameObject("KayKit candy"); root.layer = 2;
        Surface(root.transform, "Sweet", AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Resources/Meshes/KayKitCandyBody.asset"), DreamArt.Mat("Candy"));
        Surface(root.transform, "Candy stripes", AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Resources/Meshes/KayKitCandyStripes.asset"), stripe);
        PrefabUtility.SaveAsPrefabAsset(root, Prefab); Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("LOST_DREAM_KAYKIT_CANDY_PREFAB_READY");
    }

    static Mesh Part(string name, Mesh source, Vector3[] vertices, Vector3[] normals, List<int> triangles)
    {
        int[] used = triangles.Distinct().OrderBy(x => x).ToArray();
        var map = used.Select((old, next) => new { old, next }).ToDictionary(x => x.old, x => x.next);
        var mesh = new Mesh { name = name, vertices = used.Select(x => vertices[x]).ToArray(), triangles = triangles.Select(x => map[x]).ToArray() };
        if (normals.Length == vertices.Length) mesh.normals = used.Select(x => normals[x]).ToArray(); else mesh.RecalculateNormals();
        if (source.uv.Length == vertices.Length) { Vector2[] uv = source.uv; mesh.uv = used.Select(x => uv[x]).ToArray(); }
        mesh.RecalculateBounds(); return mesh;
    }

    static void SaveMesh(Mesh mesh, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null) AssetDatabase.CreateAsset(mesh, path);
        else { EditorUtility.CopySerialized(mesh, existing); EditorUtility.SetDirty(existing); Object.DestroyImmediate(mesh); }
    }

    static void Surface(Transform parent, string name, Mesh mesh, Material material)
    {
        var go = new GameObject(name); go.layer = 2; go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    [MenuItem("Lost Dream/Art/Replace saved candy with KayKit artwork")]
    public static void ReplaceSavedCandy()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Leave Play mode first.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        if (prefab == null) throw new FileNotFoundException("Build the KayKit candy prefab first.");
        Scene active = SceneManager.GetActiveScene();
        string backup = "Builds/archive/before-kaykit-candy-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(backup); Directory.CreateDirectory(Evidence);
        var reports = new List<Chapter>();
        foreach (string name in DreamGame.SceneNames)
        {
            string path = "Assets/Scenes/" + name + ".unity";
            Scene scene = SceneManager.GetSceneByPath(path); bool loaded = scene.IsValid() && scene.isLoaded;
            if (!loaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            var pickups = game.GetComponentsInChildren<CandyPickup>(true);
            var roots = pickups.Select(x => x.visual).Concat(new[] { game.player.heldCandy.transform }).ToArray();
            var original = game.GetComponentsInChildren<Component>(true).Where(c =>
                !roots.Any(r => c.transform != r && c.transform.IsChildOf(r))).ToArray();
            string[] signatures = original.Select(Signature).ToArray();
            File.Copy(path, Path.Combine(backup, name + ".unity")); File.Copy(path + ".meta", Path.Combine(backup, name + ".unity.meta"));
            if (!EditorSceneManager.SaveScene(scene, Path.Combine(backup, "open-" + name + ".unity"), true))
                throw new IOException("Cannot preserve open scene edits.");
            int replaced = 0;
            foreach (var pickup in pickups) if (Replace(pickup.visual, pickup.hue, prefab)) replaced++;
            if (Replace(game.player.heldCandy.transform, CandyHue.Pink, prefab)) replaced++;
            for (int n = 0; n < original.Length; n++)
                if (original[n] == null || Signature(original[n]) != signatures[n]) throw new InvalidOperationException("Existing map or gameplay component changed.");
            foreach (var root in roots)
            {
                if (root.Find("KayKit candy") == null || root.GetComponentsInChildren<Collider>(true).Length != 0)
                    throw new InvalidOperationException("Candy must have the imported appearance and no new collision.");
            }
            if (replaced > 0 && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + path);
            reports.Add(new Chapter { scene = path, replacedVisuals = replaced, originalComponentsPreserved = original.Length, geometryPhysicsAndGameplayPreserved = true });
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            Debug.Log("LOST_DREAM_KAYKIT_CANDY_SAVED: " + name + " visuals=" + replaced);
        }
        if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        File.WriteAllText(Evidence + "/placement-audit.json", JsonUtility.ToJson(new Report { backup = backup, chapters = reports.ToArray() }, true));
        AssetDatabase.SaveAssets();
    }

    static bool Replace(Transform root, CandyHue hue, GameObject prefab)
    {
        if (root.Find("KayKit candy") != null) return false;
        Transform sweet = root.Find("Sweet");
        if (sweet == null) throw new InvalidOperationException("Unknown candy artwork; refusing to overwrite it.");
        float size = sweet.localScale.x / .7f;
        foreach (Transform child in root.Cast<Transform>().ToArray()) Undo.DestroyObjectImmediate(child.gameObject);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        instance.name = "KayKit candy"; instance.transform.localScale = Vector3.one * size;
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
        Undo.RegisterCreatedObjectUndo(instance, "Replace candy artwork");
        CandyPalette.Paint(root, hue);
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>()) PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        return true;
    }

    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F9") + "|" +
            t.localRotation.ToString("F9") + "|" + t.localScale.ToString("F9") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }

    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Report { public string backup; public Chapter[] chapters; }
    [Serializable] class Chapter { public string scene; public int replacedVisuals, originalComponentsPreserved; public bool geometryPhysicsAndGameplayPreserved; }
}
