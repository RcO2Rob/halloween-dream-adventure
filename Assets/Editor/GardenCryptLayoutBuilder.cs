using System;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Move the existing Crypt and its authored contents, preserving loaded map edits.
[InitializeOnLoad]
public static class GardenCryptLayoutBuilder
{
    const string Map = "Assets/Scenes/02_ShatteredGardens.unity";
    const string Request = "Temp/LostDreamCryptLeft.request";
    const string Result = "Temp/LostDreamCryptLeft.result.json";
    static GardenCryptLayoutBuilder()
    {
        EditorApplication.delayCall += Requested;
        EditorApplication.projectChanged += Requested;
    }
    static void Requested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Requested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += Requested; return; }
        File.Delete(Request);
        try { MoveSavedCrypt(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true)); Debug.LogException(error); }
    }
    [MenuItem("Lost Dream/Map/Move saved Candy Crypt to the left")]
    public static void MoveSavedCrypt()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        Scene active = SceneManager.GetActiveScene(), scene = SceneManager.GetSceneByPath(Map);
        bool loaded = scene.IsValid() && scene.isLoaded;
        if (!loaded) scene = EditorSceneManager.OpenScene(Map, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            var garden = game.garden;
            if (garden == null) throw new InvalidOperationException("Expanded garden is required.");
            var island = garden.transform.Find("Candy Crypt");
            var mainFloor = game.transform.Find("Dream gate garden/Solid dream island").GetComponent<BoxCollider>();
            var floor = island.Find("Solid dream island").GetComponent<BoxCollider>();
            Physics.SyncTransforms();
            if (floor.bounds.max.x < mainFloor.bounds.min.x)
            { Debug.Log("LOST_DREAM_CRYPT_ALREADY_LEFT"); return; }
            string backup = "Builds/archive/before-crypt-left-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(backup);
            File.Copy(Map, backup + "/02_ShatteredGardens.unity"); File.Copy(Map + ".meta", backup + "/02_ShatteredGardens.unity.meta");
            if (!EditorSceneManager.SaveScene(scene, backup + "/open-02_ShatteredGardens.unity", true)) throw new IOException("Loaded-map backup failed.");
            Bounds oldBounds = floor.bounds;
            var route = garden.cryptLight.path;
            Func<Vector3, bool> inside = p => p.x >= oldBounds.min.x - .5f && p.x <= oldBounds.max.x + .5f &&
                p.z >= oldBounds.min.z - .5f && p.z <= oldBounds.max.z + .5f;
            var moved = garden.transform.Cast<Transform>().Where(t => t != route.transform && t != garden.cryptLight.transform &&
                (inside(t.position) || t.GetComponentsInChildren<Renderer>(true).Any(r => inside(r.bounds.center)))).ToArray();
            if (!moved.Contains(island) || !moved.Contains(garden.memories[2].transform) || !moved.Contains(garden.candySeal.transform))
                throw new InvalidOperationException("Crypt content could not be identified safely.");
            var signs = garden.GetComponentsInChildren<TextMesh>(true).Where(t => t.text.Contains("NORTH")).ToArray();
            var fences = game.transform.Find("Iron side fences");
            var changedFences = new[] { fences.Find("Candy Crypt"), fences.Find("Dream gate garden") };
            Func<Transform, bool> edited = t => moved.Any(p => t == p || t.IsChildOf(p)) ||
                t == route.transform || t.IsChildOf(route.transform) || t == garden.cryptLight.transform || t.IsChildOf(garden.cryptLight.transform) ||
                changedFences.Any(p => p != null && (t == p || t.IsChildOf(p)));
            var preserved = game.GetComponentsInChildren<Component>(true).Where(c => c != garden && !edited(c.transform) && !signs.Contains(c as TextMesh)).ToArray();
            var before = preserved.Select(Signature).ToArray();
            Vector3 delta = new Vector3(-33, island.position.y, 74) - island.position;
            foreach (var item in moved)
            {
                item.position += delta;
                foreach (var enemy in item.GetComponentsInChildren<FlyingDragon>(true))
                { enemy.home += delta; EditorUtility.SetDirty(enemy); }
                if (PrefabUtility.IsPartOfPrefabInstance(item)) PrefabUtility.RecordPrefabInstancePropertyModifications(item);
            }
            garden.candyPoints = garden.candyPoints.Select(p => p + delta).ToArray();
            // Keep the existing platform spacing/height pattern; turn the complete route west.
            route.transform.position = new Vector3((mainFloor.bounds.min.x + oldBounds.max.x + delta.x) * .5f, route.transform.position.y, 76);
            route.transform.rotation = Quaternion.LookRotation(Vector3.left);
            garden.cryptLight.transform.position = new Vector3(-8, garden.cryptLight.transform.position.y, 75);
            garden.cryptLight.safeCheckpoint = new Vector3(-7, .12f, 73);
            foreach (var sign in signs) { sign.text = sign.text.Replace("NORTH", "WEST"); EditorUtility.SetDirty(sign); }
            SideFenceBuilder.RebuildCourtyard(game, mainFloor);
            SideFenceBuilder.RebuildCourtyard(game, floor);
            Physics.SyncTransforms();
            for (int i = 0; i < preserved.Length; i++)
                if (preserved[i] == null || Signature(preserved[i]) != before[i]) throw new InvalidOperationException("Unrelated content changed: " + preserved[i]?.name);
            if (floor.bounds.max.x >= mainFloor.bounds.min.x || Mathf.Abs(floor.bounds.center.z - mainFloor.bounds.center.z) > .1f)
                throw new InvalidOperationException("Crypt must sit to the left beside the gate garden.");
            EditorUtility.SetDirty(garden); EditorUtility.SetDirty(garden.cryptLight);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the relocated Crypt.");
            Directory.CreateDirectory("docs/qa/crypt-left");
            File.WriteAllText("docs/qa/crypt-left/map-audit.json", JsonUtility.ToJson(new Audit {
                backup = backup, preservedComponents = preserved.Length, movedRoots = moved.Length,
                center = floor.bounds.center, translation = delta, unrelatedContentPreserved = true,
                gatePreserved = true, checkpointAndSupplyUpdated = true
            }, true));
            AssetDatabase.SaveAssets();
            Debug.Log("LOST_DREAM_CRYPT_MOVED_LEFT: preserved=" + preserved.Length + "; center=" + island.position);
        }
        finally
        {
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        }
    }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" +
            t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { MoveSavedCrypt(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Audit {
        public string backup; public int preservedComponents, movedRoots; public Vector3 center, translation;
        public bool unrelatedContentPreserved, gatePreserved, checkpointAndSupplyUpdated;
    }
}
