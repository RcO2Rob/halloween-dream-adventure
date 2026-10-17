using System;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GardenIronGateBuilder
{
    const string Map = "Assets/Scenes/02_ShatteredGardens.unity";
    const string Request = "Temp/LostDreamIronGate.request";
    const string Result = "Temp/LostDreamIronGate.result.json";
    const string Source = "Assets/polyperfect/Poly Halloween/Prefabs/Modular Parts/Fences/Gate_Graveyard.prefab";
    static GardenIronGateBuilder()
    { EditorApplication.delayCall += Requested; EditorApplication.projectChanged += Requested; }
    static void Requested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Requested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += Requested; return; }
        File.Delete(Request);
        try { ReplaceSavedGate(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true)); Debug.LogException(error); }
    }
    [MenuItem("Lost Dream/Map/Replace garden exit with opening iron gate")]
    public static void ReplaceSavedGate()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        var active = SceneManager.GetActiveScene(); var scene = SceneManager.GetSceneByPath(Map);
        bool loaded = scene.IsValid() && scene.isLoaded;
        if (!loaded) scene = EditorSceneManager.OpenScene(Map, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            var gate = game.GetComponentInChildren<DreamGate>(true);
            if (gate.ironGate != null) { Debug.Log("LOST_DREAM_IRON_GATE_ALREADY_INSTALLED"); return; }
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (asset == null) throw new FileNotFoundException(Source);
            string backup = "Builds/archive/before-garden-iron-gate-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(backup);
            File.Copy(Map, backup + "/02_ShatteredGardens.unity"); File.Copy(Map + ".meta", backup + "/02_ShatteredGardens.unity.meta");
            if (!EditorSceneManager.SaveScene(scene, backup + "/open-02_ShatteredGardens.unity", true)) throw new IOException("Loaded scene backup failed.");
            var preserved = game.GetComponentsInChildren<Component>(true).Where(c => c.transform != gate.transform && !c.transform.IsChildOf(gate.transform)).ToArray();
            var before = preserved.Select(Signature).ToArray();
            Vector3 originalPosition = gate.transform.position; Quaternion originalRotation = gate.transform.rotation;
            var iron = ConfigureGate(gate);
            var model = iron.leftLeaf.parent;
            Bounds doors = BoundsOf(new[] { iron.leftLeaf, iron.rightLeaf }.SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray());
            Bounds frame = BoundsOf(model.GetComponentsInChildren<Renderer>());
            var trigger = gate.GetComponent<BoxCollider>();
            Physics.SyncTransforms();
            for (int i = 0; i < preserved.Length; i++)
                if (preserved[i] == null || Signature(preserved[i]) != before[i]) throw new InvalidOperationException("Unrelated map content changed: " + preserved[i]?.name);
            if (gate.transform.position != originalPosition || gate.transform.rotation != originalRotation) throw new InvalidOperationException("Gate anchor moved.");
            EditorUtility.SetDirty(gate); EditorUtility.SetDirty(trigger);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Iron gate save failed.");
            Directory.CreateDirectory("docs/qa/garden-iron-gate");
            File.WriteAllText("docs/qa/garden-iron-gate/map-audit.json", JsonUtility.ToJson(new Audit {
                backup = backup, preservedComponents = preserved.Length, gatePosition = originalPosition,
                doorwaySize = doors.size, frameSize = frame.size, gateAnchorPreserved = true, unrelatedMapContentPreserved = true, prefab = Source
            }, true));
            AssetDatabase.SaveAssets();
            Debug.Log("LOST_DREAM_IRON_GATE_SAVED: preserved=" + preserved.Length + "; doorway=" + doors.size + "; frame=" + frame.size);
        }
        finally
        {
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        }
    }
    public static GardenIronGate ConfigureGate(DreamGate gate)
    {
        if (gate.ironGate != null) return gate.ironGate;
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
        if (asset == null) throw new FileNotFoundException(Source);
        foreach (Transform child in gate.transform) child.gameObject.SetActive(false);
        gate.glowParts = new Renderer[0];
        var iron = gate.gameObject.AddComponent<GardenIronGate>(); gate.ironGate = iron;
        var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, gate.transform); model.name = "Opening Poly Halloween iron gate";
        var leaves = model.GetComponentsInChildren<Transform>().Where(t => t.name.Contains("door-")).OrderBy(t => t.position.x).ToArray();
        if (leaves.Length != 2) throw new InvalidOperationException("Expected two native iron gate hinges.");
        Bounds doors = BoundsOf(leaves.SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray());
        model.transform.localScale *= 4f / doors.size.x;
        doors = BoundsOf(leaves.SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray());
        Bounds frame = BoundsOf(model.GetComponentsInChildren<Renderer>());
        model.transform.position += gate.transform.position - new Vector3(doors.center.x, frame.min.y, doors.center.z);
        iron.leftLeaf = leaves[0]; iron.rightLeaf = leaves[1];
        foreach (var leaf in leaves) foreach (var collider in leaf.GetComponentsInChildren<Collider>())
        { collider.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(collider); }
        PrefabUtility.RecordPrefabInstancePropertyModifications(model.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(model.gameObject);
        doors = BoundsOf(leaves.SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray());
        frame = BoundsOf(model.GetComponentsInChildren<Renderer>());
        iron.closedBarrier = DreamArt.Group("Closed gate blocks passage", gate.transform, Vector3.zero).AddComponent<BoxCollider>();
        iron.closedBarrier.center = new Vector3(0, Mathf.Max(3.6f, doors.size.y) / 2, 0);
        iron.closedBarrier.size = new Vector3(4.1f, Mathf.Max(3.6f, doors.size.y), .35f);
        iron.groundCue = DreamArt.Group("Unlocked glowing exit trail", gate.transform, Vector3.zero);
        iron.groundCue.layer = 2;
        for (int i = 0; i < 8; i++)
            DreamArt.Shape("Glowing exit stepping light " + (i + 1), PrimitiveType.Cube, iron.groundCue.transform,
                new Vector3(0, .075f, -4.5f + i * .85f), new Vector3(1.8f, .025f, .57f), "GlowTeal");
        for (int i = 0; i < 3; i++) foreach (int side in new[] { -1, 1 })
        {
            var arrow = DreamArt.Shape("Forward arrow on the glowing trail", PrimitiveType.Cube, iron.groundCue.transform,
                new Vector3(side * .24f, .095f, -3.5f + i * 1.6f), new Vector3(.075f, .025f, .7f), "GlowGold");
            arrow.transform.localRotation = Quaternion.Euler(0, -side * 45, 0);
        }
        iron.cueLight = DreamArt.Group("Exit trail glow", iron.groundCue.transform, new Vector3(0, .8f, -1.8f)).AddComponent<Light>();
        iron.cueLight.type = LightType.Point; iron.cueLight.color = new Color(.25f, 1, .82f); iron.cueLight.range = 8; iron.cueLight.intensity = 1.5f;
        iron.groundCue.SetActive(false);
        var trigger = gate.GetComponent<BoxCollider>();
        trigger.center = new Vector3(0, 1.5f, 1.15f); trigger.size = new Vector3(3.6f, 3, 1.1f);
        EditorUtility.SetDirty(gate); EditorUtility.SetDirty(trigger);
        return iron;
    }
    static Bounds BoundsOf(Renderer[] renderers)
    { var bounds = renderers[0].bounds; foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds); return bounds; }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" +
            t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { ReplaceSavedGate(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Audit {
        public string backup, prefab; public int preservedComponents; public Vector3 gatePosition, doorwaySize, frameSize;
        public bool gateAnchorPreserved, unrelatedMapContentPreserved;
    }
}
