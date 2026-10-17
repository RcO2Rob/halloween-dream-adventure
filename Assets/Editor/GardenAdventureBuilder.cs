using System;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Extend the saved second map in place; never regenerate any existing island.
[InitializeOnLoad]
public static class GardenAdventureBuilder
{
    const string Map = "Assets/Scenes/02_ShatteredGardens.unity";
    const string Request = "Temp/LostDreamGardenAdventure.request";
    const string Result = "Temp/LostDreamGardenAdventure.result.json";
    static GardenAdventureBuilder()
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
        try { ExpandSavedGarden(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception error) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = error.ToString() }, true)); Debug.LogException(error); }
    }
    [MenuItem("Lost Dream/Map/Expand saved garden adventure")]
    public static void ExpandSavedGarden()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        Scene active = SceneManager.GetActiveScene(), scene = SceneManager.GetSceneByPath(Map);
        bool loaded = scene.IsValid() && scene.isLoaded;
        if (!loaded) scene = EditorSceneManager.OpenScene(Map, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            if (game.garden != null) { Debug.Log("LOST_DREAM_GARDEN_ALREADY_EXPANDED"); return; }
            string backup = "Builds/archive/before-expanded-garden-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(backup);
            File.Copy(Map, backup + "/02_ShatteredGardens.unity"); File.Copy(Map + ".meta", backup + "/02_ShatteredGardens.unity.meta");
            if (!EditorSceneManager.SaveScene(scene, backup + "/open-02_ShatteredGardens.unity", true)) throw new IOException("Loaded map backup failed.");
            Transform fences = game.transform.Find("Iron side fences");
            // Existing fence runs need openings for the new side paths.
            var changedFences = new[] { fences.Find("Wandering orchard"), fences.Find("Moonlit terrace"), fences.Find("Dream gate garden") };
            var originalLamps = game.lanterns.ToArray();
            var preserved = game.GetComponentsInChildren<Component>(true).Where(c => c != game &&
                !originalLamps.Contains(c as Lantern) && !changedFences.Any(t => t != null && (c.transform == t || c.transform.IsChildOf(t)))).ToArray();
            var signatures = preserved.Select(Signature).ToArray();
            var root = DreamArt.Group("Garden adventure - three lost memories", game.transform, Vector3.zero).transform;
            var garden = root.gameObject.AddComponent<GardenAdventure>(); game.garden = garden;
            var randomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(20261016);
            DreamWorld.Island(root, new Vector3(-44, 0, 25), 16, 18, "Whisper Grove", false);
            DreamWorld.Island(root, new Vector3(31, 0, 45), 18, 20, "Moon Observatory", false);
            DreamWorld.Island(root, new Vector3(-33, 0, 74), 20, 16, "Candy Crypt", false);
            var grove = DreamWorld.Path(root, new Vector3(-26, 0, 25), new Vector3(-36, 0, 25), 4.8f);
            var observatory = DreamWorld.Path(root, new Vector3(13, 0, 45), new Vector3(22, 0, 45), 4.8f);
            var crypt = DreamWorld.Path(root, new Vector3(-11, 0, 76), new Vector3(-23, 0, 76), 4.8f);
            foreach (var route in new[] { grove, observatory, crypt }) DreamWorld.MakeJumpPath(route, 1);
            garden.groveLight = Lamp(originalLamps[0], root, new Vector3(-23, 0, 25), "Whisper Grove light", grove, new Vector3(-22, .12f, 25), garden, 0);
            garden.observatoryLight = Lamp(originalLamps[0], root, new Vector3(10, 0, 45), "Observatory light", observatory, new Vector3(9, .12f, 43), garden, 1);
            garden.cryptLight = Lamp(originalLamps[0], root, new Vector3(-8, 0, 75), "Crypt light", crypt, new Vector3(-7, .12f, 73), garden, 2);
            originalLamps[1].garden = garden; originalLamps[1].requiredMemories = 1;
            originalLamps[2].garden = garden; originalLamps[2].requiredMemories = 2;
            var guards = new[] {
                DreamWorld.Dragon(root, new Vector3(-46, 2.8f, 22), 1.5f, 2, NightmareKind.Ghost),
                DreamWorld.Dragon(root, new Vector3(-40, 2.5f, 29), 1.4f, 2, NightmareKind.Armabee),
                DreamWorld.Dragon(root, new Vector3(26, 2.8f, 43), 1.5f, 3),
                DreamWorld.Dragon(root, new Vector3(36, 2.5f, 48), 1.4f, 2, NightmareKind.Armabee),
                DreamWorld.Dragon(root, new Vector3(-38, 2.8f, 73), 1.5f, 2, NightmareKind.Demon),
                DreamWorld.Dragon(root, new Vector3(-28, 2.7f, 78), 1.4f, 2, NightmareKind.Ghost)
            };
            garden.sequenceRunes = new[] {
                DreamWorld.Rune(root, new Vector3(36, 0, 40), false),
                DreamWorld.Rune(root, new Vector3(26, 0, 49), false),
                DreamWorld.Rune(root, new Vector3(36, 0, 52), false)
            };
            for (int i = 0; i < 3; i++)
            {
                var rune = garden.sequenceRunes[i]; rune.sequence = garden; rune.name = "Moon melody rune " + (i + 1);
                Label(rune.transform, Vector3.up * 3, new[] { "I", "II", "III" }[i], .18f);
            }
            garden.candySeal = DreamWorld.Rune(root, new Vector3(-33, 0, 72), true);
            garden.candySeal.name = "Crypt candy seal";
            garden.candySeal.clearedMessage = "Crypt seal shattered. Clear its guardians and recover memory III.";
            garden.memories = new[] {
                Memory(root, new Vector3(-46, 0, 30), 0, garden, new[] { game.dragons[0], game.dragons[1], guards[0], guards[1] }),
                Memory(root, new Vector3(31, 0, 52), 1, garden, new[] { game.dragons[2], game.dragons[3], guards[2], guards[3] }),
                Memory(root, new Vector3(-33, 0, 79), 2, garden, new[] { game.dragons[4], guards[4], guards[5] })
            };
            garden.candyPoints = new[] { new Vector3(-38, 0, 69), new Vector3(-29, 0, 69) };
            foreach (var point in garden.candyPoints) DreamWorld.Candy(root, point + Vector3.up * .65f);
            DreamWorld.Sign(root, new Vector3(4, 0, 8), "THREE LOST MEMORIES", "WEST / EAST / WEST");
            DreamWorld.Sign(root, new Vector3(-19, 0, 20), "I / WHISPER GROVE", "WEST LANTERN > ROUND LIGHTS");
            DreamWorld.Sign(root, new Vector3(5, 0, 41), "II / MOON OBSERVATORY", "EAST LANTERN > MOON RUNES");
            DreamWorld.Sign(root, new Vector3(31, 0, 37), "MOON MELODY", "SHOOT I > II > III");
            DreamWorld.Sign(root, new Vector3(-4, 0, 74), "III / CANDY CRYPT", "WEST LANTERN > CANDY SEAL");
            DreamWorld.Sign(root, new Vector3(-38, 0, 76), "MISSED YOUR THROW?", "CANDY RETURNS EVERY 3 SECONDS");
            Decorate(root);
            UnityEngine.Random.state = randomState;
            game.lanterns = originalLamps.Concat(new[] { garden.groveLight, garden.observatoryLight, garden.cryptLight }).ToArray();
            game.dragons = game.dragons.Concat(guards).ToArray();
            game.runes = game.runes.Concat(garden.sequenceRunes).Concat(new[] { garden.candySeal }).ToArray();
            foreach (var floor in root.GetComponentsInChildren<BoxCollider>(true).Where(x => x.name == "Solid dream island"))
                SideFenceBuilder.RebuildCourtyard(game, floor);
            foreach (string name in new[] { "Wandering orchard", "Moonlit terrace", "Dream gate garden" })
                SideFenceBuilder.RebuildCourtyard(game, game.transform.Find(name + "/Solid dream island").GetComponent<BoxCollider>());
            Physics.SyncTransforms();
            for (int i = 0; i < preserved.Length; i++)
                if (preserved[i] == null || Signature(preserved[i]) != signatures[i]) throw new InvalidOperationException("Existing content changed: " + preserved[i]?.name);
            EditorUtility.SetDirty(game);
            foreach (var lamp in originalLamps) EditorUtility.SetDirty(lamp);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Expanded garden save failed.");
            Directory.CreateDirectory("docs/qa/expanded-garden");
            File.WriteAllText("docs/qa/expanded-garden/map-audit.json", JsonUtility.ToJson(new Audit {
                backup = backup, preservedComponents = preserved.Length, originalIslandsPreserved = true,
                islands = 7, memories = 3, lanterns = game.lanterns.Length, enemies = game.dragons.Length,
                originalEnemyHealthPreserved = true, changedExistingFences = new[] { "Wandering orchard", "Moonlit terrace", "Dream gate garden" }
            }, true));
            AssetDatabase.SaveAssets();
            Debug.Log("LOST_DREAM_GARDEN_EXPANDED: seven islands, six lanterns, eleven enemies, three memories; preserved=" + preserved.Length);
        }
        finally
        {
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        }
    }
    static Lantern Lamp(Lantern template, Transform root, Vector3 point, string name, DreamPath route, Vector3 safe, GardenAdventure garden, int required)
    {
        var clone = Object.Instantiate(template.gameObject, root);
        clone.name = name; clone.transform.position = point;
        var lamp = clone.GetComponent<Lantern>();
        lamp.lanternName = name; lamp.path = route; lamp.safeCheckpoint = safe; lamp.garden = garden; lamp.requiredMemories = required;
        return lamp;
    }
    static DreamMemory Memory(Transform parent, Vector3 point, int index, GardenAdventure garden, FlyingDragon[] guards)
    {
        var root = DreamArt.Group("Memory " + (index + 1) + " shrine", parent, point);
        var memory = root.AddComponent<DreamMemory>(); memory.index = index; memory.garden = garden; memory.guardians = guards;
        DreamArt.Shape("Memory plinth", PrimitiveType.Cylinder, root.transform, Vector3.up * .35f, new Vector3(1.7f, .35f, 1.7f), "Stone", true);
        memory.crystal = DreamArt.Shape("Lost dream memory", PrimitiveType.Cube, root.transform, Vector3.up * 2.25f, Vector3.one * .75f, "GlowTeal").transform;
        memory.crystal.localRotation = Quaternion.Euler(45, 20, 45);
        DreamArt.Ring(DreamArt.Group("Memory halo", root.transform, Vector3.up * .06f).transform, 1.4f, "GlowTeal", 48, .09f);
        Label(root.transform, Vector3.up * 3.4f, "MEMORY " + new[] { "I", "II", "III" }[index], .08f);
        var light = DreamArt.Group("Memory beacon", root.transform, Vector3.up * 3).AddComponent<Light>();
        light.type = LightType.Point; light.color = new Color(.3f, 1, .85f); light.range = 9; light.intensity = 1.6f;
        return memory;
    }
    static void Label(Transform parent, Vector3 position, string value, float size)
    {
        var go = DreamArt.Group("Readable objective label", parent, position);
        var text = go.AddComponent<TextMesh>(); text.text = value; text.fontSize = 64; text.characterSize = size;
        text.anchor = TextAnchor.MiddleCenter; text.color = new Color(.65f, 1, .92f);
    }
    static void Decorate(Transform root)
    {
        foreach (var center in new[] { new Vector3(-44, 0, 25), new Vector3(31, 0, 45), new Vector3(-33, 0, 74) })
        {
            Prop(root, "Halloween/Tree_Scary_Dead", center + new Vector3(-5, 0, 6), 5.5f);
            Prop(root, "Halloween/Gravestone_Old_Cross", center + new Vector3(5, 0, -5), 1.8f);
            Prop(root, "Halloween/Pumpkin_Carved_Lit_C", center + new Vector3(-4, 0, -5), .9f);
            Prop(root, "Halloween/Coffin_New", center + new Vector3(5, 0, 6), 1.5f);
        }
    }
    static void Prop(Transform parent, string resource, Vector3 position, float height)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/polyperfect/Poly Halloween/Prefabs/" + resource + ".prefab");
        if (prefab == null) throw new IOException("Missing installed Halloween prop: " + resource);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        var renderers = go.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds; foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
        go.transform.localScale *= height / bounds.size.y;
        bounds = renderers[0].bounds; foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
        go.transform.position += position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        foreach (var collider in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
    }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" +
            t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { ExpandSavedGarden(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Audit {
        public string backup; public int preservedComponents, islands, memories, lanterns, enemies;
        public bool originalIslandsPreserved, originalEnemyHealthPreserved;
        public string[] changedExistingFences;
    }
}
