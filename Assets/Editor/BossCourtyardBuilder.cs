using System;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// A targeted edit to the existing castle scene, including currently loaded edits.
[InitializeOnLoad]
public static class BossCourtyardBuilder
{
    const string Path = "Assets/Scenes/04_PumpkinKeep.unity";
    const string Request = "Temp/LostDreamExpandedCourtyard.request";
    const string Result = "Temp/LostDreamExpandedCourtyard.result.json";
    const float XFactor = 1.4f, ZFactor = 38f / 28f;
    static BossCourtyardBuilder()
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
        try { ExpandSavedCourtyard(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true)); Debug.LogException(error); }
    }
    static Vector3 Expanded(Vector3 world)
    { return new Vector3(world.x * XFactor, world.y, -2 + (world.z + 2) * ZFactor); }

    [MenuItem("Lost Dream/Map/Expand saved boss courtyard")]
    public static void ExpandSavedCourtyard()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        Scene active = SceneManager.GetActiveScene(), scene = SceneManager.GetSceneByPath(Path);
        bool loaded = scene.IsValid() && scene.isLoaded;
        if (!loaded) scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
        var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
        var courtyard = game.transform.Find("Pumpkin keep courtyard");
        var floor = courtyard.Find("Solid dream island").GetComponent<BoxCollider>();
        Physics.SyncTransforms();
        if (floor.bounds.size.x > 40)
        {
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            return;
        }
        string backup = "Builds/archive/before-expanded-courtyard-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(backup);
        File.Copy(Path, backup + "/04_PumpkinKeep.unity");
        File.Copy(Path + ".meta", backup + "/04_PumpkinKeep.unity.meta");
        if (!EditorSceneManager.SaveScene(scene, backup + "/open-04_PumpkinKeep.unity", true))
            throw new IOException("Could not back up loaded scene edits.");
        // Snapshot unrelated content. Only these authored courtyard subtrees may move.
        var walls = game.transform.Find("Pumpkin keep walls");
        var scenery = game.transform.Find("Poly Halloween scenery");
        var fence = game.transform.Find("Iron side fences/Pumpkin keep courtyard");
        var candies = game.GetComponentsInChildren<CandyPickup>(true).Where(c => c.transform.position.z >= -2).ToArray();
        Func<Transform, bool> edited = t => t == courtyard || t.IsChildOf(courtyard) ||
            t == walls || t.IsChildOf(walls) || t == game.boss.transform || t.IsChildOf(game.boss.transform) ||
            (fence != null && (t == fence || t.IsChildOf(fence))) ||
            (scenery != null && t.IsChildOf(scenery) && t.position.z >= -2) ||
            candies.Any(c => t == c.transform || t.IsChildOf(c.transform));
        var preserved = game.GetComponentsInChildren<Component>(true).Where(c => c != game && c != game.boss && !edited(c.transform)).ToArray();
        var signatures = preserved.Select(Signature).ToArray();
        var children = courtyard.Cast<Transform>().ToArray();
        Vector3[] positions = children.Select(t => Expanded(t.position)).ToArray();
        courtyard.position = Expanded(courtyard.position);
        for (int i = 0; i < children.Length; i++)
        {
            Transform t = children[i]; t.position = positions[i];
            if (t.name == "Solid dream island" || t.name == "Floating island crystal" || t.name == "Island rim")
                t.localScale = Vector3.Scale(t.localScale, new Vector3(XFactor, 1, ZFactor));
        }
        foreach (Transform t in walls)
        {
            t.position = Expanded(t.position);
            if (t.name == "Castle side wall") t.localScale = Vector3.Scale(t.localScale, new Vector3(1, 1, ZFactor));
            else if (t.name == "Keep back wall") t.localScale = Vector3.Scale(t.localScale, new Vector3(XFactor, 1, 1));
        }
        if (scenery != null) foreach (Transform t in scenery)
            if (t.position.z >= -2) { t.position = Expanded(t.position); PrefabUtility.RecordPrefabInstancePropertyModifications(t); }
        foreach (var candy in candies) candy.transform.position = Expanded(candy.transform.position);
        game.boss.transform.position = Expanded(game.boss.transform.position);
        game.boss.arenaMin = new Vector2(-17.5f, 1);
        game.boss.arenaMax = new Vector2(17.5f, 33.5f);
        for (int i = 0; i < game.candySpawnPoints.Length; i++) game.candySpawnPoints[i] = Expanded(game.candySpawnPoints[i]);
        SideFenceBuilder.RebuildCourtyard(game, floor);
        Physics.SyncTransforms();
        for (int i = 0; i < preserved.Length; i++)
            if (preserved[i] == null || Signature(preserved[i]) != signatures[i])
                throw new InvalidOperationException("Unrelated saved content changed: " + preserved[i]?.name);
        if (Mathf.Abs(floor.bounds.size.x - 42) > .05f || Mathf.Abs(floor.bounds.size.z - 38) > .05f ||
            Mathf.Abs(floor.bounds.min.z + 2) > .05f) throw new InvalidOperationException("Unexpected courtyard extent.");
        EditorUtility.SetDirty(game); EditorUtility.SetDirty(game.boss);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save expanded courtyard.");
        Directory.CreateDirectory("docs/qa/expanded-boss-ending");
        File.WriteAllText("docs/qa/expanded-boss-ending/map-audit.json", JsonUtility.ToJson(new Audit {
            backup = backup, width = floor.bounds.size.x, depth = floor.bounds.size.z,
            previousWidth = 30, previousDepth = 28, preservedComponents = preserved.Length,
            entranceAnchored = true, unrelatedContentPreserved = true
        }, true));
        Debug.Log("LOST_DREAM_EXPANDED_COURTYARD_SAVED: 42 x 38; preserved=" + preserved.Length);
        if (!loaded) EditorSceneManager.CloseScene(scene, true);
        if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        AssetDatabase.SaveAssets();
    }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" +
            t.localPosition.ToString("F8") + "|" + t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") +
            "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { ExpandSavedCourtyard(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Audit {
        public string backup;
        public float width, depth, previousWidth, previousDepth;
        public int preservedComponents;
        public bool entranceAnchored, unrelatedContentPreserved;
    }
}
