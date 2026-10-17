using System;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class FourChapterBuilder
{
    const string Request = "Temp/LostDreamFourChapters.request";
    const string Result = "Temp/LostDreamFourChapters.result.json";
    const string OldBoss = "Assets/Scenes/03_PumpkinKeep.unity";
    const string Boss = "Assets/Scenes/04_PumpkinKeep.unity";
    const string Marsh = "Assets/Scenes/03_WitchlightMarsh.unity";
    static FourChapterBuilder()
    { EditorApplication.delayCall += Requested; EditorApplication.projectChanged += Requested; }
    static void Requested()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Requested; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.isPlaying = false; EditorApplication.delayCall += Requested; return; }
        File.Delete(Request);
        try { Upgrade(); File.WriteAllText(Result, "{\"passed\":true}"); }
        catch (Exception e) { File.WriteAllText(Result, JsonUtility.ToJson(new Failure { error = e.ToString() }, true)); Debug.LogException(e); }
    }
    [MenuItem("Lost Dream/Map/Add Witchlight Marsh and move keep to chapter four")]
    public static void Upgrade()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        var active = SceneManager.GetActiveScene();
        string backup = "Builds/archive/before-four-chapters-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(backup);
        string path = File.Exists(OldBoss) ? OldBoss : Boss;
        var scene = SceneManager.GetSceneByPath(path); bool loaded = scene.IsValid() && scene.isLoaded;
        if (!loaded) scene = EditorSceneManager.OpenScene(path, Application.isBatchMode ? OpenSceneMode.Single : OpenSceneMode.Additive);
        try
        {
            File.Copy(path, backup + "/" + Path.GetFileName(path));
            File.Copy(path + ".meta", backup + "/" + Path.GetFileName(path) + ".meta");
            if (!EditorSceneManager.SaveScene(scene, backup + "/open-" + Path.GetFileName(path), true)) throw new IOException("Loaded Boss map backup failed.");
            var game = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DreamGame>(true)).Single();
            var preserved = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Component>(true)).Where(x => x != game).ToArray();
            var signatures = preserved.Select(Signature).ToArray();
            game.levelIndex = DreamGame.BossChapterIndex;
            EditorUtility.SetDirty(game); EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Boss chapter index save failed.");
            if (path == OldBoss)
            {
                string error = AssetDatabase.MoveAsset(OldBoss, Boss);
                if (!string.IsNullOrEmpty(error)) throw new IOException(error);
            }
            for (int i = 0; i < preserved.Length; i++)
                if (preserved[i] == null || Signature(preserved[i]) != signatures[i]) throw new InvalidOperationException("Boss content changed: " + preserved[i]?.name);
            if (!File.Exists(Marsh))
            {
                var marshScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(marshScene);
                try { CreateMarsh(); if (!EditorSceneManager.SaveScene(marshScene, Marsh)) throw new IOException("Marsh save failed."); }
                finally { EditorSceneManager.CloseScene(marshScene, true); }
            }
            EditorBuildSettings.scenes = DreamGame.SceneNames.Select(x => new EditorBuildSettingsScene("Assets/Scenes/" + x + ".unity", true)).ToArray();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("docs/qa/four-chapters");
            File.WriteAllText("docs/qa/four-chapters/map-audit.json", JsonUtility.ToJson(new Audit {
                backup = backup, preservedBossComponents = preserved.Length, originalBossLayoutPreserved = true,
                originalFirstTwoMapsUntouched = true, chapters = DreamGame.SceneNames, newIslands = 6, newEnemies = 12,
                brews = 3, recipes = new[] { "PINK > BLUE", "BLUE > GOLD", "GOLD > PINK > BLUE" }
            }, true));
            Debug.Log("LOST_DREAM_FOUR_CHAPTERS_READY: Boss preserved=" + preserved.Length);
        }
        finally
        {
            if (!loaded) EditorSceneManager.CloseScene(scene, true);
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        }
    }
    public static DreamGame CreateMarsh()
    {
        MakeMaterial("MarshGround", new Color(.16f, .25f, .24f));
        MakeMaterial("MarshStone", new Color(.31f, .40f, .37f));
        var game = DreamWorld.Core(2, new Vector3(0, .12f, -5));
        RenderSettings.fogColor = new Color(.055f,.12f,.13f); RenderSettings.fogDensity = .0065f;
        RenderSettings.ambientLight = new Color(.33f,.40f,.43f);
        game.transform.Find("Moonlight").GetComponent<Light>().color = new Color(.65f,.92f,.88f);
        game.name = "Lost Dream - Chapter 3 - Witchlight Marsh";
        var root = game.transform;
        var marsh = game.gameObject.AddComponent<WitchlightMarsh>(); game.marsh = marsh;
        game.gameCamera.backgroundColor = new Color(.035f, .085f, .1f);
        Vector3[] centers = { Vector3.zero, new Vector3(0, 0, 34), new Vector3(-32, 0, 34),
            new Vector3(-32, 0, 68), new Vector3(0, 0, 68), new Vector3(0, 0, 102) };
        string[] names = { "Mushroom landing", "Glowcap grove", "Witch's workshop", "Breathing marsh", "Abandoned watchtower", "Road to the keep" };
        float[] widths = { 22, 26, 22, 26, 22, 24 }, depths = { 22, 24, 24, 22, 22, 24 };
        for (int i = 0; i < centers.Length; i++)
        {
            DreamWorld.Island(root, centers[i], widths[i], depths[i], names[i], false);
            var island = root.Find(names[i]);
            island.Find("Solid dream island").GetComponent<Renderer>().sharedMaterial = DreamArt.Mat("MarshGround");
            foreach (var stone in island.GetComponentsInChildren<Renderer>().Where(x => x.name == "Dream stepping stone")) stone.sharedMaterial = DreamArt.Mat("MarshStone");
        }
        var routes = new[] {
            DreamWorld.Path(root, new Vector3(0,0,11), new Vector3(0,0,22), 5),
            DreamWorld.Path(root, new Vector3(-13,0,34), new Vector3(-21,0,34), 5),
            DreamWorld.Path(root, new Vector3(-32,0,46), new Vector3(-32,0,57), 5),
            DreamWorld.Path(root, new Vector3(-19,0,68), new Vector3(-11,0,68), 5),
            DreamWorld.Path(root, new Vector3(0,0,79), new Vector3(0,0,90), 5)
        };
        for (int i = 0; i < routes.Length; i++)
        {
            DreamWorld.MakeJumpPath(routes[i], 1);
            routes[i].name = "Crossing " + (i + 1) + (i >= 2 ? " - drifting dreamstones" : " - glowcap stones");
            if (i >= 2)
            {
                var drift = routes[i].jumpPlatforms[1].gameObject.AddComponent<DriftingPlatform>(); drift.phase = i;
                foreach (var r in routes[i].jumpPlatforms[1].GetComponentsInChildren<Renderer>().Where(x => x.name == "Flat round landing surface")) r.sharedMaterial = DreamArt.Mat("GlowTeal");
            }
        }
        Vector3[] lampPoints = { new Vector3(-4,0,7), new Vector3(-9,0,34), new Vector3(-32,0,43), new Vector3(-22,0,68), new Vector3(4,0,75) };
        string[] lightNames = { "Landing light", "Grove light", "Workshop light", "Marsh light", "Watchtower light" };
        marsh.lights = lampPoints.Select((p, i) => DreamWorld.Lamp(root, p, lightNames[i], routes[i], p + new Vector3(0,.12f,-1.8f))).ToArray();
        for (int i = 0; i < marsh.lights.Length; i++) { marsh.lights[i].marsh = marsh; marsh.lights[i].marshStage = i; }
        game.lanterns = marsh.lights;
        foreach (var lamp in game.lanterns)
        {
            foreach (var r in lamp.transform.Find("Jack-o-lantern").GetComponentsInChildren<Renderer>()) r.enabled = false;
            var skin = lamp.gameObject.AddComponent<LanternSkin>(); skin.lantern = lamp;
            skin.unlit = Prop(lamp.transform, "Pumpkin_Carved_B", lamp.transform.position + Vector3.up * .6f, 1.05f);
            skin.lit = Prop(lamp.transform, "Pumpkin_Carved_Lit_B", lamp.transform.position + Vector3.up * .6f, 1.05f);
            skin.Refresh();
        }
        Vector3[] guardCenters = { centers[1], centers[2], centers[3], centers[4] };
        var enemies = guardCenters.SelectMany((p, i) => new[] {
            DreamWorld.Dragon(root, p + new Vector3(-4,2.7f,-4), 1.25f, 2, i % 2 == 0 ? NightmareKind.Armabee : NightmareKind.Ghost),
            DreamWorld.Dragon(root, p + new Vector3(4,3,3), 1.4f, 2, NightmareKind.Dragon),
            DreamWorld.Dragon(root, p + new Vector3(5,2.8f,-4), 1.2f, 2, NightmareKind.Demon)
        }).ToArray();
        game.dragons = enemies; game.runes = new DreamRune[0]; game.candySpawnPoints = new Vector3[0];
        marsh.towerGuards = enemies.Skip(9).ToArray();
        marsh.cauldrons = new[] {
            Pot(root, new Vector3(5,0,36), "Moonwake", new[] { CandyHue.Pink, CandyHue.Blue }, enemies.Take(3).ToArray()),
            Pot(root, new Vector3(-37,0,36), "Starlight", new[] { CandyHue.Blue, CandyHue.Gold }, enemies.Skip(3).Take(3).ToArray()),
            Pot(root, new Vector3(-37,0,71), "Dawnbloom", new[] { CandyHue.Gold, CandyHue.Pink, CandyHue.Blue }, enemies.Skip(6).Take(3).ToArray())
        };
        for (int i = 0; i < marsh.cauldrons.Length; i++)
        {
            var pot = marsh.cauldrons[i];
            for (int j = 0; j < 3; j++) DreamWorld.Candy(root, pot.supplyPoints[j], (CandyHue)j);
            if (i > 0)
                for (int j = 0; j < 2; j++) Mist(root, guardCenters[i] + new Vector3(j == 0 ? -2 : 3, .04f, j == 0 ? -5 : 3), pot, i + j * 2);
        }
        DreamWorld.Gate(root, new Vector3(0,0,109));
        var iron = GardenIronGateBuilder.ConfigureGate(game.GetComponentInChildren<DreamGate>());
        DreamArt.Group("Iron side fences", root, Vector3.zero);
        Physics.SyncTransforms();
        foreach (var floor in root.GetComponentsInChildren<BoxCollider>().Where(x => x.name == "Solid dream island").ToArray()) SideFenceBuilder.RebuildCourtyard(game, floor);
        ExitEnclosureBuilder.Enclose(iron, root.Find(names[5] + "/Solid dream island").GetComponent<BoxCollider>());
        var art = DreamArt.Group("Poly Halloween scenery", root, Vector3.zero).transform;
        for (int i = 0; i < centers.Length; i++)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Prop(art, "Mushroom_Magical_Amanita_Mature", centers[i] + new Vector3(side * 7,0,-7), i % 2 == 0 ? 3.6f : 2.8f);
                Prop(art, "Mushroom_Magical_Cluster_A", centers[i] + new Vector3(side * 7,0,7), 1.8f);
                Prop(art, "Pumpkin_Carved_Lit_C", centers[i] + new Vector3(side * 8,0,2), .8f);
            }
            var light = DreamArt.Group("Glowcap atmosphere", art, centers[i] + new Vector3(0,4,0)).AddComponent<Light>();
            light.type = LightType.Point; light.color = i % 2 == 0 ? new Color(.24f,.85f,.73f) : new Color(.65f,.36f,.95f); light.range = 21; light.intensity = 2;
        }
        Prop(art, "Book_Stand_Halloween", new Vector3(-39,0,28), 2.2f);
        Prop(art, "Book_Evil_Open", new Vector3(-37,1,28), .6f);
        Prop(art, "Scarecrow_A", new Vector3(7,0,70), 3);
        // A raised silhouette frames the fourth encounter without blocking its floor.
        var tower = DreamArt.Group("Abandoned timber watchtower", art, new Vector3(-7,0,73)).transform;
        for (int side = -1; side <= 1; side += 2)
            DreamArt.Shape("Timber post", PrimitiveType.Cube, tower, new Vector3(side * 1.6f,3,0), new Vector3(.4f,6,.4f), "Wood");
        DreamArt.Shape("Tower platform", PrimitiveType.Cube, tower, new Vector3(0,5,0), new Vector3(4,.35f,3), "Wood");
        DreamArt.Mesh("Pointed watchtower roof", "Cone", tower, new Vector3(0,7,0), new Vector3(5,2,5), "CoatDark");
        DreamWorld.Sign(root, new Vector3(5,0,-2), "WITCHLIGHT MARSH", "BREW THREE POTIONS / OPEN THE CASTLE ROAD");
        DreamWorld.Sign(root, new Vector3(5,0,26), "FOLLOW THE RECIPE", "E / CANDY     Q / THROW INTO THE POT");
        DreamWorld.Sign(root, new Vector3(-26,0,59), "THE MARSH BREATHES", "GOLD WARNING > PINK MIST / JUMP OR GO AROUND");
        DreamWorld.Sign(root, new Vector3(-28,0,44), "DRIFTING DREAMSTONES", "TEAL STONES MOVE / WAIT FOR YOUR JUMP");
        DreamWorld.Sign(root, new Vector3(5,0,96), "THE PUMPKIN KEEP AWAITS", "THROUGH THE GATE / CHAPTER IV");
        foreach (var t in art.GetComponentsInChildren<Transform>()) t.gameObject.layer = 2;
        return game;
    }
    static WitchCauldron Pot(Transform root, Vector3 point, string name, CandyHue[] recipe, FlyingDragon[] guards)
    {
        var pot = DreamArt.Group(name + " cauldron", root, point).AddComponent<WitchCauldron>();
        pot.brewName = name; pot.recipe = recipe; pot.guardians = guards;
        // Decorative prefab is ignored by aim casts; the generous pot receiver is authoritative.
        Prop(pot.transform, "Cauldron_Potion_Empty", point, 1.8f);
        var collider = pot.gameObject.AddComponent<SphereCollider>(); collider.center = Vector3.up * 1.35f; collider.radius = 1.05f;
        pot.liquid = DreamArt.Shape("Reactive potion", PrimitiveType.Cylinder, pot.transform, Vector3.up * 1.55f, new Vector3(1.4f,.08f,1.4f), "Candy").GetComponent<Renderer>();
        pot.label = DreamArt.Group("Recipe and next ingredient", pot.transform, Vector3.up * 3.6f).AddComponent<TextMesh>();
        pot.label.fontSize = 64; pot.label.characterSize = .07f; pot.label.anchor = TextAnchor.MiddleCenter;
        pot.glow = DreamArt.Group("Potion glow", pot.transform, Vector3.up * 2).AddComponent<Light>(); pot.glow.range = 8; pot.glow.intensity = 2;
        pot.supplyPoints = Enumerable.Range(0,3).Select(i => point + new Vector3(-2.5f + i * 2,.65f,-5)).ToArray();
        return pot;
    }
    static void Mist(Transform root, Vector3 point, WitchCauldron source, float offset)
    {
        var mist = DreamArt.Group("Breathing nightmare vent", root, point).AddComponent<MarshMist>(); mist.source = source; mist.offset = offset;
        mist.warning = DreamArt.Shape("Gold warning / pink danger", PrimitiveType.Cylinder, mist.transform, Vector3.zero, new Vector3(5.2f,.015f,5.2f), "GlowGold").GetComponent<Renderer>();
        mist.plume = DreamArt.Shape("Active nightmare mist", PrimitiveType.Sphere, mist.transform, Vector3.up * .35f, new Vector3(4.8f,.6f,4.8f), "Mist").transform;
        mist.warning.gameObject.SetActive(false); mist.plume.gameObject.SetActive(false);
    }
    static GameObject Prop(Transform root, string name, Vector3 point, float height)
    {
        string path = "Assets/polyperfect/Poly Halloween/Prefabs/Halloween/" + name + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (prefab == null) throw new FileNotFoundException(path);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        var renderers = instance.GetComponentsInChildren<Renderer>(); Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        instance.transform.localScale *= height / bounds.size.y;
        bounds = renderers[0].bounds; foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        instance.transform.position += point - new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        foreach (var collider in instance.GetComponentsInChildren<Collider>()) { collider.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(collider); }
        foreach (var t in instance.GetComponentsInChildren<Transform>()) { t.gameObject.layer = 2; PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject); }
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        return instance;
    }
    static void MakeMaterial(string name, Color color)
    {
        string path = "Assets/Resources/Materials/" + name + ".mat";
        if (File.Exists(path)) return;
        var material = new Material(Shader.Find("Standard")); material.color = color; material.SetFloat("_Glossiness",.1f); AssetDatabase.CreateAsset(material,path);
    }
    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.localPosition.ToString("F8") + "|" + t.localRotation.ToString("F8") + "|" + t.localScale.ToString("F8") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    public static void ApplyAndBuild() { Upgrade(); PrototypeBuilder.BuildCurrentScenes(); }
    [Serializable] class Failure { public bool passed; public string error; }
    [Serializable] class Audit { public string backup; public int preservedBossComponents, newIslands, newEnemies, brews; public bool originalBossLayoutPreserved, originalFirstTwoMapsUntouched; public string[] chapters, recipes; }
}
