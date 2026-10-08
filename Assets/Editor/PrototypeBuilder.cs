using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using LostDream;

public static class PrototypeBuilder
{
    public static void BuildPrototype()
    {
        CreateScenes(); ValidateScenes(); BuildMac();
    }
    public static void BuildCurrentScenes()
    {
        PlayerSettings.bundleVersion = "0.9.0";
        MakeCombatMaterials();
        ValidateScenes();
        AssetDatabase.SaveAssets();
        BuildMac();
    }
    [MenuItem("Lost Dream/Add enemy variety to saved garden scene")]
    public static void AddEnemyVarietyToCurrentScenes()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        MakeCombatMaterials();
        MonsterAssetBuilder.BuildVarietyPrefabs();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/02_ShatteredGardens.unity");
        var game = UnityEngine.Object.FindFirstObjectByType<DreamGame>();
        int[] indices = { 1, 3, 4 };
        NightmareKind[] kinds = { NightmareKind.Ghost, NightmareKind.Armabee, NightmareKind.Demon };
        for (int i = 0; i < indices.Length; i++)
        {
            var enemy = game.dragons[indices[i]];
            enemy.kind = kinds[i];
            foreach (Transform child in enemy.body.Cast<Transform>().ToArray())
                if (child.GetComponentInChildren<Animation>() != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var prefab = Resources.Load<GameObject>("Monsters/" + kinds[i]);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, enemy.body);
            enemy.modelAnimation = model.GetComponentInChildren<Animation>();
            enemy.chargeMarker.sharedMaterial = DreamArt.Mat(enemy.AttackMaterial);
            EditorUtility.SetDirty(enemy);
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        BuildCurrentScenes();
    }
    static void MakeCombatMaterials()
    {
        Material("CandyBlue", CandyPalette.Color(CandyHue.Blue), .8f);
        Material("CandyGold", CandyPalette.Color(CandyHue.Gold), .8f);
        foreach (CandyHue hue in Enum.GetValues(typeof(CandyHue)))
        {
            Color color = CandyPalette.Color(hue) * .65f; color.a = 1;
            Material("Boss" + hue, color, .08f);
            Color dark = color * .72f; dark.a = 1;
            Material("Boss" + hue + "Dark", dark, .08f);
        }
    }
    [MenuItem("Lost Dream/Add round jump platforms to saved scenes")]
    public static void AddJumpPlatformsToCurrentScenes()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        PlayerSettings.bundleVersion = "0.9.0";
        for (int i = 0; i < 3; i++)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + DreamGame.SceneNames[i] + ".unity");
            var game = UnityEngine.Object.FindFirstObjectByType<DreamGame>();
            var path = game.lanterns[i == 1 ? 2 : 0].path;
            if (path.IsJumpRoute) continue;
            DreamWorld.MakeJumpPath(path, i);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("LOST_DREAM_JUMP_ROUTE_ADDED_CHAPTER_" + (i + 1));
        }
        ValidateScenes();
        AssetDatabase.SaveAssets();
        BuildMac();
    }
    [MenuItem("Lost Dream/Create or regenerate prototype scenes")]
    public static void CreateScenes()
    {
        Directory.CreateDirectory("Assets/Resources/Materials");
        Directory.CreateDirectory("Assets/Resources/Meshes");
        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();
        MakeMaterials(); MakeCombatMaterials(); MakeMeshes(); MonsterAssetBuilder.BuildPrefabs();
        MonsterAssetBuilder.BuildVarietyPrefabs(); TimmyAssetBuilder.BuildPrefab();
        PlayerSettings.companyName = "Lost Dream Studio";
        PlayerSettings.productName = "Lanterns of the Lost Dream";
        PlayerSettings.bundleVersion = "0.9.0";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone, "com.lostdream.halloween");
        PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        QualitySettings.vSyncCount = 1; QualitySettings.shadowDistance = 50;
        QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.pixelLightCount = 4;
        EditorSettings.serializationMode = SerializationMode.ForceText;
        for (int i = 0; i < 3; i++)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            DreamWorld.Build(i);
            string path = "Assets/Scenes/" + DreamGame.SceneNames[i] + ".unity";
            EditorSceneManager.SaveScene(scene, path);
        }
        EditorBuildSettings.scenes = DreamGame.SceneNames.Select(x => new EditorBuildSettingsScene("Assets/Scenes/" + x + ".unity", true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/01_FirstLight.unity");
        Debug.Log("LOST_DREAM_SCENES_READY: three playable chapters generated.");
    }

    [MenuItem("Lost Dream/Build macOS prototype")]
    public static void BuildMac()
    {
        if (!File.Exists("Assets/Scenes/01_FirstLight.unity")) CreateScenes();
        Directory.CreateDirectory("Builds/macOS");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray(),
            locationPathName = "Builds/macOS/Lanterns of the Lost Dream.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Mac build failed: " + report.SummarizeErrors());
        Debug.Log("LOST_DREAM_BUILD_OK: " + report.summary.totalSize + " bytes.");
    }

    [MenuItem("Lost Dream/Validate prototype scenes")]
    public static void ValidateScenes()
    {
        for (int i = 0; i < 3; i++)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + DreamGame.SceneNames[i] + ".unity");
            DreamGame game = UnityEngine.Object.FindFirstObjectByType<DreamGame>();
            Physics.SyncTransforms();
            Require(game != null && game.levelIndex == i, "Chapter root " + i);
            Require(game.player != null && game.gameCamera != null && game.audioSystem != null, "Core references " + i);
            Require(!game.gameCamera.orthographic && game.gameCamera.fieldOfView == 58, "Third-person perspective camera " + i);
            var character = game.player.characterAnimation;
            Require(character != null && character.animator.avatar.isValid && character.animator.isHuman, "Timmy humanoid rig " + i);
            Require(character.animator.runtimeAnimatorController != null && !character.animator.applyRootMotion, "Timmy animation controller and motor movement " + i);
            Require(character.LeftHand != null && character.RightHand != null && character.bow != null && game.player.heldCandy != null, "Hand-held props " + i);
            foreach (var skin in character.GetComponentsInChildren<SkinnedMeshRenderer>())
                Require(skin.sharedMaterials.All(x => x != null && x.mainTexture != null), "Timmy textured skin " + i);
            Require(game.lanterns.Length == (i == 0 ? 2 : i == 1 ? 3 : 1), "Lantern count " + i);
            foreach (var lamp in game.lanterns)
            {
                Require(lamp.path != null && !lamp.path.solid.activeSelf, "Hidden road starts inactive");
                Require(lamp.path.solid.GetComponentsInChildren<Collider>(true).Length > 0, "Road has collision");
                Require(lamp.glow != null && lamp.flame != null, "Lantern visual references");
                Require(Physics.Raycast(lamp.safeCheckpoint + Vector3.up * 3, Vector3.down, 6), "Checkpoint sits over solid ground");
                if (lamp.path.IsJumpRoute)
                {
                    Require(lamp.path.jumpEntry != null && lamp.path.jumpExit != null, "Jump route island anchors");
                    Require(lamp.path.jumpPlatforms.Length == (i == 1 ? 3 : 2), "Chapter jumping platform count");
                    Require(lamp.path.solid.GetComponentsInChildren<BoxCollider>(true).Length == 0, "Jump route has no continuous bridge collider");
                    foreach (var platform in lamp.path.jumpPlatforms)
                        Require(platform.GetComponentInChildren<MeshCollider>(true) != null, "Flat round landing collision");
                }
            }
            Require(game.lanterns.Count(x => x.path.IsJumpRoute) == 1, "One jump route per chapter");
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) == 0, "No missing script: " + go.name);
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                Require(renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null, "Material on " + renderer.name);
            foreach (var dragon in game.dragons)
            {
                Require(dragon.modelAnimation != null, "Imported enemy animation reference");
                Require(dragon.body.GetComponentInChildren<SkinnedMeshRenderer>() != null, "Imported skinned dragon mesh");
                Require(dragon.modelAnimation.GetClip("CharacterArmature|Flying_Idle") != null, "Flight clip exists");
                foreach (var skin in dragon.body.GetComponentsInChildren<SkinnedMeshRenderer>())
                    Require(skin.sharedMaterials.All(x => x != null && x.mainTexture != null), "Monster atlas material");
            }
            if (i == 2) Require(game.boss != null && game.boss.weakPoint.Length == 3 && game.candySpawnPoints.Length >= 4, "Boss and safe candy points");
            Debug.Log("LOST_DREAM_VALIDATED_CHAPTER_" + (i + 1));
        }
        EditorSceneManager.OpenScene("Assets/Scenes/01_FirstLight.unity");
    }

    static void Require(bool condition, string message) { if (!condition) throw new Exception("Scene validation failed: " + message); }
    static void Material(string name, Color color, float emission = 0, bool transparent = false)
    {
        string path = "Assets/Resources/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
        material.color = color; material.SetFloat("_Glossiness", .12f); material.SetFloat("_Metallic", 0);
        if (emission > 0) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(color.r, color.g, color.b) * emission); }
        if (transparent)
        {
            material.SetFloat("_Mode", 3); material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0); material.EnableKeyword("_ALPHABLEND_ON"); material.renderQueue = 3000;
        }
        EditorUtility.SetDirty(material);
    }
    static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color color); return color; }
    static void MakeMaterials()
    {
        Material("Ground", Hex("34354F")); Material("CastleFloor", Hex("424052"));
        Material("Rock", Hex("25223B")); Material("Rim", Hex("59516F"));
        Material("Stone", Hex("686174")); Material("Path", Hex("444057"));
        Material("Wood", Hex("654457")); Material("Stem", Hex("517154"));
        Material("Pumpkin", Hex("F58B3B")); Material("PumpkinDark", Hex("C95B2A"));
        Material("Coat", Hex("4EB2AD")); Material("CoatDark", Hex("323255")); Material("Ink", Hex("242139"));
        Material("Skin", Hex("F4C9A5")); Material("Leaf", Hex("BC6D91")); Material("LeafGold", Hex("D69B61"));
        Material("Dragon", Hex("9866C1")); Material("DragonBelly", Hex("CF92B7")); Material("DragonWing", Hex("C985D0"));
        Material("CastleWall", Hex("615B78"));
        Material("GlowGold", Hex("FFC15A"), 1.2f); Material("GlowTeal", Hex("66E6D0"), 1.4f);
        Material("GlowMoon", Hex("DDD5FF"), .8f); Material("Candy", Hex("FF83BB"), .8f);
        Material("CandyWrap", Hex("ACEEDE"), .5f); Material("Warning", Hex("FF633E"), 1.2f);
        Material("Mist", new Color(.56f, .44f, .75f, .24f), .3f, true);
        Material("Fog", new Color(.12f, .08f, .21f, .72f), 0, true);
        Material("WarningFade", new Color(1, .3f, .12f, .19f), .3f, true);
    }
    static void MeshAsset(string name, Vector3[] vertices, int[] triangles)
    {
        string path = "Assets/Resources/Meshes/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
        mesh.Clear(); mesh.name = name; mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
    }
    static void MakeMeshes()
    {
        MeshAsset("Triangle", new[] { new Vector3(-.5f, -.5f, 0), new Vector3(0, .5f, 0), new Vector3(.5f, -.5f, 0) }, new[] { 0, 1, 2, 2, 1, 0 });
        var vertices = new System.Collections.Generic.List<Vector3>();
        var indices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4, b = (i + 1) * Mathf.PI / 4;
            int offset = vertices.Count;
            vertices.Add(new Vector3(Mathf.Cos(a) * .5f, 0, Mathf.Sin(a) * .5f));
            vertices.Add(Vector3.up);
            vertices.Add(new Vector3(Mathf.Cos(b) * .5f, 0, Mathf.Sin(b) * .5f));
            indices.AddRange(new[] { offset, offset + 1, offset + 2, offset + 2, offset + 1, offset });
        }
        MeshAsset("Cone", vertices.ToArray(), indices.ToArray());
        MeshAsset("Wing", new[]
        {
            Vector3.zero, new Vector3(1.8f, .6f, .4f), new Vector3(1.4f, -.1f, -.1f),
            new Vector3(1.1f, .05f, -.6f), new Vector3(.7f, -.1f, -.9f), new Vector3(.3f, .1f, -.65f)
        }, new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 2, 1, 0, 3, 2, 0, 4, 3, 0, 5, 4, 0 });
    }
}
