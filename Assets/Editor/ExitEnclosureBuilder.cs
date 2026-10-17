using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Preserve saved and loaded map edits; touch only exit subtrees and tutorial tree art.
[InitializeOnLoad]
public static class ExitEnclosureBuilder
{
    const string Request = "Temp/LostDreamExitEnclosures.request";
    const string Result = "Temp/LostDreamExitEnclosures.result.json";
    const string TreeGroup = "Tutorial trees - Poly Halloween";
    static ExitEnclosureBuilder()
    { EditorApplication.delayCall += Requested; EditorApplication.projectChanged += Requested; }
    static void Requested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Requested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += Requested; return; }
        File.Delete(Request);
        try { UpdateSavedChapters(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true)); Debug.LogException(error); }
    }
    [MenuItem("Lost Dream/Map/Enclose both exits and match tutorial tree art")]
    public static void UpdateSavedChapters()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        var active = SceneManager.GetActiveScene(); var audits = new List<Audit>();
        string backup = "Builds/archive/before-exit-enclosures-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(backup);
        try
        {
            for (int chapter = 0; chapter < 2; chapter++)
            {
                string path = "Assets/Scenes/" + DreamGame.SceneNames[chapter] + ".unity";
                var scene = SceneManager.GetSceneByPath(path); bool loaded = scene.IsValid() && scene.isLoaded;
                if (!loaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                try
                {
                    var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
                    var gate = game.GetComponentInChildren<DreamGate>(true);
                    bool treesNeeded = chapter == 0 && game.transform.Find(TreeGroup) == null;
                    var existingEnclosure = gate.transform.Find("Exit perimeter enclosure");
                    bool rearOpeningNeeded = existingEnclosure != null && existingEnclosure.Find("Rear island perimeter railing") != null;
                    bool axisFixNeeded = existingEnclosure != null && existingEnclosure.Find("Axis-correct fence geometry") == null;
                    if (existingEnclosure != null && !treesNeeded && !rearOpeningNeeded && !axisFixNeeded) continue;
                    File.Copy(path, backup + "/" + Path.GetFileName(path));
                    File.Copy(path + ".meta", backup + "/" + Path.GetFileName(path) + ".meta");
                    if (!EditorSceneManager.SaveScene(scene, backup + "/open-" + Path.GetFileName(path), true)) throw new IOException("Loaded scene backup failed.");
                    var trees = treesNeeded ? game.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Dream orchard tree").ToArray() : new Transform[0];
                    var scenery = game.transform.Find("Poly Halloween scenery");
                    var importedTrees = treesNeeded ? scenery.Cast<Transform>().Where(t => t.name.StartsWith("Tree_")).ToArray() : new Transform[0];
                    var treeRenderers = trees.SelectMany(t => t.GetComponentsInChildren<Renderer>(true)).ToArray();
                    var preserved = game.GetComponentsInChildren<Component>(true).Where(c => c.transform != gate.transform &&
                        !c.transform.IsChildOf(gate.transform) && !treeRenderers.Contains(c as Renderer) &&
                        !importedTrees.Any(t => c.transform == t || c.transform.IsChildOf(t))).ToArray();
                    var signatures = preserved.Select(Signature).ToArray();
                    var iron = GardenIronGateBuilder.ConfigureGate(gate);
                    Physics.SyncTransforms();
                    var floor = game.GetComponentsInChildren<BoxCollider>(true).Where(c => c.name == "Solid dream island")
                        .OrderBy(c => Vector3.Distance(c.bounds.center, gate.transform.position)).First();
                    if (existingEnclosure == null || axisFixNeeded)
                    {
                        if (existingEnclosure != null) UnityEngine.Object.DestroyImmediate(existingEnclosure.gameObject);
                        Enclose(iron, floor);
                    }
                    else if (rearOpeningNeeded) OpenRearSightline(iron, floor);
                    int count = 0;
                    if (treesNeeded)
                    {
                        foreach (var renderer in treeRenderers) { renderer.enabled = false; EditorUtility.SetDirty(renderer); }
                        foreach (var tree in importedTrees) tree.gameObject.SetActive(false);
                        var group = DreamArt.Group(TreeGroup, game.transform, Vector3.zero).transform;
                        foreach (var tree in trees)
                        {
                            string source = count % 3 == 0 ? "Halloween/Tree_Scary_Dead" : "Nature/Trees/Tree_Poplar_Dead_A";
                            Tree(group, source, tree.position, count % 3 == 0 ? 4.9f : 5.1f, (count * 73) % 360);
                            count++;
                        }
                    }
                    Physics.SyncTransforms();
                    for (int i = 0; i < preserved.Length; i++)
                        if (preserved[i] == null || Signature(preserved[i]) != signatures[i])
                            throw new InvalidOperationException("Unrelated content changed: " + preserved[i]?.name);
                    EditorUtility.SetDirty(iron); EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Map save failed: " + path);
                    audits.Add(new Audit { scene = path, backup = backup, preservedComponents = preserved.Length,
                        replacedTrees = count, exitPosition = gate.transform.position,
                        originalTreeCollisionPreserved = true, unrelatedContentPreserved = true, enclosedAtSidesAndRear = true });
                    Debug.Log("LOST_DREAM_EXIT_ENCLOSED: chapter=" + (chapter + 1) + "; trees=" + count + "; preserved=" + preserved.Length);
                }
                finally { if (!loaded) EditorSceneManager.CloseScene(scene, true); }
            }
            Directory.CreateDirectory("docs/qa/exit-enclosures");
            File.WriteAllText("docs/qa/exit-enclosures/map-audit.json", JsonUtility.ToJson(new Report { chapters = audits.ToArray() }, true));
            AssetDatabase.SaveAssets();
        }
        finally { if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active); }
    }
    public static void Enclose(GardenIronGate iron, BoxCollider floor)
    {
        Physics.SyncTransforms();
        Bounds ground = floor.bounds;
        var model = iron.leftLeaf.parent;
        Bounds frame = BoundsOf(model.gameObject);
        var root = DreamArt.Group("Exit perimeter enclosure", iron.transform, Vector3.zero).transform;
        float left = ground.min.x + .18f, right = ground.max.x - .18f, y = ground.max.y, z = iron.transform.position.z;
        // Visible wings meet the prefab's outer posts and the existing side railings.
        var leftWing = SideFenceBuilder.AddFenceSegment(root, new Vector3(left, y, z), new Vector3(frame.min.x + .12f, y, z), 2.8f);
        var rightWing = SideFenceBuilder.AddFenceSegment(root, new Vector3(frame.max.x - .12f, y, z), new Vector3(right, y, z), 2.8f);
        leftWing.name = "Left gate-to-island railing"; rightWing.name = "Right gate-to-island railing";
        leftWing.GetComponent<BoxCollider>().enabled = false; rightWing.GetComponent<BoxCollider>().enabled = false;
        float halfDoor = iron.closedBarrier.size.x * .5f;
        float center = iron.transform.position.x;
        iron.sideBarriers = new[] {
            Barrier(root, "Left continuous exit fence collision", left, center - halfDoor, y, z),
            Barrier(root, "Right continuous exit fence collision", center + halfDoor, right, y, z)
        };
        OpenRearSightline(iron, floor);
        DreamArt.Group("Axis-correct fence geometry", root, Vector3.zero);
    }
    static void OpenRearSightline(GardenIronGate iron, BoxCollider floor)
    {
        var root = iron.transform.Find("Exit perimeter enclosure");
        var previous = root.Find("Rear island perimeter railing");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        Bounds ground = floor.bounds;
        float left = ground.min.x + .18f, right = ground.max.x - .18f, y = ground.max.y, z = ground.max.z - .18f;
        float center = iron.transform.position.x, opening = 2.35f;
        var rearLeft = SideFenceBuilder.AddFenceSegment(root, new Vector3(left, y, z), new Vector3(center - opening, y, z), 1.55f);
        var rearRight = SideFenceBuilder.AddFenceSegment(root, new Vector3(center + opening, y, z), new Vector3(right, y, z), 1.55f);
        rearLeft.name = "Rear left perimeter - clear exit sightline";
        rearRight.name = "Rear right perimeter - clear exit sightline";
        iron.rearBarriers = new[] { rearLeft.GetComponent<BoxCollider>(), rearRight.GetComponent<BoxCollider>() };
        iron.rearBarrier = iron.rearBarriers[0];
    }
    static BoxCollider Barrier(Transform parent, string name, float from, float to, float y, float z)
    {
        var go = DreamArt.Group(name, parent, Vector3.zero); go.transform.position = new Vector3((from + to) * .5f, y, z);
        var collider = go.AddComponent<BoxCollider>(); collider.center = Vector3.up * 1.4f;
        collider.size = new Vector3(to - from, 2.8f, .34f); return collider;
    }
    static void Tree(Transform parent, string name, Vector3 point, float height, float yaw)
    {
        string path = "Assets/polyperfect/Poly Halloween/Prefabs/" + name + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (prefab == null) throw new FileNotFoundException(path);
        var anchor = DreamArt.Group("Matching dead tree " + parent.childCount, parent, point).transform;
        anchor.rotation = Quaternion.Euler(0, yaw, 0); anchor.gameObject.layer = 2;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor);
        var bounds = BoundsOf(instance); instance.transform.localScale *= height / bounds.size.y;
        bounds = BoundsOf(instance); instance.transform.position += point - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
        { collider.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(collider); }
        foreach (var t in instance.GetComponentsInChildren<Transform>(true))
        { t.gameObject.layer = 2; PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject); }
    }
    static Bounds BoundsOf(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true); var bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds); return bounds;
    }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" +
            t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { UpdateSavedChapters(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Report { public Audit[] chapters; }
    [Serializable] class Audit {
        public string scene, backup; public int preservedComponents, replacedTrees; public Vector3 exitPosition;
        public bool originalTreeCollisionPreserved, unrelatedContentPreserved, enclosedAtSidesAndRear;
    }
}
