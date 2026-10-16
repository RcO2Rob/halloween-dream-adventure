using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// A separate art study. Never rebuilds gameplay scenes or changes the build list.
public static class PolyHalloweenPreviewBuilder
{
    const string Source = "Assets/polyperfect/Poly Halloween/Prefabs/";
    const string Output = "Assets/Scenes/Previews/PolyHalloweenPreview.unity";
    const string Art = "Assets/PreviewArt/PolyHalloween/";
    static Transform props;
    static int propCount;
    static readonly HashSet<string> used = new HashSet<string>();

    [MenuItem("Lost Dream/Preview/Create Poly Halloween art study")]
    public static void Create()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Leave Play mode before creating the art study.");
        AssetDatabase.Refresh();
        if (!AssetDatabase.IsValidFolder(Source.TrimEnd('/')))
            throw new InvalidOperationException("Import the locally acquired Poly Halloween pack first; see docs/POLY_HALLOWEEN_SETUP.md.");
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        Directory.CreateDirectory(Art);
        AssetDatabase.Refresh();
        // Additive creation keeps any open, unsaved gameplay scenes intact.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
            Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        propCount = 0; used.Clear();
        props = new GameObject("Polyperfect scenery - editable placements").transform;
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.38f, .4f, .6f);
        RenderSettings.ambientEquatorColor = new Color(.27f, .22f, .38f);
        RenderSettings.ambientGroundColor = new Color(.15f, .12f, .22f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(.075f, .05f, .14f); RenderSettings.fogDensity = .012f;
        var moon = new GameObject("Moonlight").AddComponent<Light>();
        moon.type = LightType.Directional; moon.color = new Color(.64f, .72f, 1);
        moon.intensity = 1.3f; moon.shadows = LightShadows.Soft;
        moon.transform.rotation = Quaternion.Euler(45, -35, 0);
        var fill = new GameObject("Soft front fill").AddComponent<Light>();
        fill.type = LightType.Directional; fill.intensity = .6f;
        fill.color = new Color(.84f, .69f, .88f); fill.transform.rotation = Quaternion.Euler(25, 155, 0);
        Material soil = Material("Dream soil", new Color(.19f, .16f, .29f));
        Material rock = Material("Floating island rock", new Color(.10f, .075f, .18f));
        Material paving = Material("Dream path", new Color(.35f, .31f, .46f));
        Shape("Floating dream island", PrimitiveType.Cylinder, new Vector3(0, -.45f, 0), new Vector3(24, .45f, 20), soil);
        Shape("Island underside", PrimitiveType.Cylinder, new Vector3(0, -1.1f, 0), new Vector3(22, .35f, 18), rock);
        Shape("Island lower tier", PrimitiveType.Cylinder, new Vector3(0, -1.7f, 0), new Vector3(18, .3f, 14), rock);
        for (int i = 0; i < 10; i++)
            Shape("Visible stepping stone " + (i + 1), PrimitiveType.Cube,
                new Vector3(Mathf.Sin(i * .65f) * .3f, -.12f, -7.4f + i * 1.5f),
                new Vector3(2.6f, .28f, 1.23f), paving);

        Prop("Modular Parts/Fences/Gate_Graveyard", new Vector3(0, 0, 6.5f), 3.3f, 0);
        for (int sign = -1; sign <= 1; sign += 2)
        {
            for (int i = 0; i < 2; i++)
                Prop("Modular Parts/Fences/Wall_Graveyard_3m", new Vector3(sign * (4 + i * 3), 0, 6.5f), 1.6f, 0);
            Prop("Modular Parts/Fences/Wall_Graveyard_Pillar", new Vector3(sign * 2.2f, 0, 6.5f), 2.1f, 0);
            Prop("Halloween/Tree_Scary_Dead", new Vector3(sign * 8.3f, 0, 2.2f), 6.6f, sign * 28);
            Prop("Nature/Trees/Tree_Poplar_Dead_A", new Vector3(sign * 6.8f, 0, 6), 5.8f, sign * 40);
            Prop("Halloween/Pumpkin_Carved_Lit_B", new Vector3(sign * 2.2f, 2.1f, 6.5f), .7f, 180);
            Point("Gate pumpkin glow", new Vector3(sign * 2.2f, 2.5f, 6.15f), new Color(1, .38f, .08f), 3, 5);
            for (int i = 0; i < 3; i++)
            {
                Prop("Halloween/Pumpkin_Carved_Lit_B", new Vector3(sign * (2.4f + i * .15f), 0, -6.4f + i * 2), .7f + .13f * (i % 2), 155 + sign * 20);
                Point("Path pumpkin glow", new Vector3(sign * 2.4f, .5f, -6.4f + i * 2), new Color(1, .4f, .07f), 2, 3.5f);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            string name = i % 2 == 0 ? "Gravestone_Old_Classic_Cross" : "Gravestone_Old_Small_C";
            Prop("Halloween/" + name, new Vector3(-4.2f - i % 2 * 1.5f, 0, -2 + i * 1.45f), 1.5f + (i % 2) * .25f, 155 + i * 9);
        }
        Prop("Halloween/Coffin_Old", new Vector3(-6.6f, 0, -4.3f), .7f, 35);
        Prop("Halloween/Scarecrow_A", new Vector3(6.3f, 0, 1.8f), 3.4f, 200);
        Prop("Halloween/Cauldron_Potion_Full", new Vector3(5.4f, 0, -4), 1.8f, 150);
        Prop("Halloween/Book_Evil_Open", new Vector3(4.2f, 0, -5.3f), .38f, 180);
        Prop("Halloween/Pumpkin_A", new Vector3(6.8f, 0, -4.6f), 1.05f, 100);
        Prop("Halloween/Candle_Stick_B_Lit", new Vector3(4, 0, -3.1f), 1.2f, 0);
        Prop("Halloween/Candle_Stick_C_Lit", new Vector3(6.2f, 0, -2.5f), 1, 25);
        Prop("Halloween/Mushroom_Magical_Cluster_A", new Vector3(4.4f, 0, 3.8f), .8f, 100);
        Prop("Halloween/Mushroom_Magical_Cluster_B", new Vector3(-7.8f, 0, -1.1f), .7f, 20);
        Point("Potion glow", new Vector3(5.4f, 1.8f, -4), new Color(.15f, 1, .64f), 3, 5);

        var timmy = Actor("Assets/Resources/Characters/Timmy.prefab", "Timmy - existing game scale", new Vector3(-.6f, 0, -3.4f), 155);
        var idle = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Mixamo/Timmy/Animations/Idle.fbx")
            .OfType<AnimationClip>().First(x => !x.name.StartsWith("__preview__"));
        var animator = timmy.GetComponentInChildren<Animator>();
        animator.Rebind();
        animator.Update(0);
        idle.SampleAnimation(animator.gameObject, .4f);
        var dragon = Actor("Assets/Resources/Monsters/Dragon.prefab", "Dragon - existing game scale", new Vector3(2.1f, 3, .2f), 190);
        var flying = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Quaternius/UltimateMonsters/Models/Dragon.fbx")
            .OfType<AnimationClip>().First(x => x.name.EndsWith("|Flying_Idle"));
        var animation = dragon.GetComponentInChildren<Animation>();
        flying.SampleAnimation(animation.gameObject, .3f);
        animation.clip = flying; animation.playAutomatically = true;
        foreach (var actor in new[] { timmy, dragon })
        {
            foreach (var transform in actor.GetComponentsInChildren<Transform>())
                PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(animation);
        Point("Timmy warm fill", new Vector3(-.7f, 2.1f, -5.7f), new Color(1, .65f, .3f), 2, 5);

        var camera = new GameObject("Preview camera").AddComponent<Camera>();
        camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.055f, .035f, .105f);
        camera.nearClipPlane = .1f; camera.farClipPlane = 160;
        camera.fieldOfView = 42; camera.allowHDR = true;
        camera.transform.position = new Vector3(18, 14, -23);
        camera.transform.LookAt(new Vector3(0, 1.1f, .3f));
        camera.gameObject.AddComponent<AudioListener>();
        string path = AssetDatabase.GenerateUniqueAssetPath(Output);
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        if (!Application.isBatchMode && SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.LookAt(new Vector3(0, 1, 0), camera.transform.rotation, 25);
        Debug.Log("POLY_HALLOWEEN_PREVIEW_READY: " + path + " props=" + propCount);
    }

    public static void BuildAndCapture()
    {
        Create();
        // Reopen the saved file so validation and images use persisted placements.
        EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path, OpenSceneMode.Single);
        var camera = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Single();
        string folder = "docs/qa/poly-halloween-preview";
        Directory.CreateDirectory(folder);
        // Validate the selected scenery, rather than demo content we do not use.
        var errors = new List<string>();
        foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!renderer.enabled) continue;
            foreach (var material in renderer.sharedMaterials)
                if (material == null || material.shader == null || !material.shader.isSupported)
                    errors.Add("Missing or unsupported material: " + renderer.name);
        }
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var transform in go.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                    errors.Add("Missing script: " + transform.name);
        if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
        Capture(camera, folder + "/overview.png");
        camera.transform.position = new Vector3(9, 6.5f, -13);
        camera.transform.LookAt(new Vector3(.8f, 1.5f, -.7f));
        camera.fieldOfView = 46;
        Capture(camera, folder + "/characters-and-props.png");
        File.WriteAllText(folder + "/validation.json", JsonUtility.ToJson(new Report {
            unityVersion = Application.unityVersion, scene = SceneManager.GetActiveScene().path,
            propInstances = propCount, uniquePrefabs = used.OrderBy(x => x).ToArray(),
            selectedMaterialsAndScriptsValid = true,
            note = "Separate visual study; no gameplay scene changes, build-list changes, or independent human review."
        }, true));
    }

    static GameObject Actor(string path, string name, Vector3 position, float yaw)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new FileNotFoundException(path);
        var actor = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        actor.name = name; actor.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor);
        return actor;
    }

    static void Prop(string name, Vector3 position, float height, float yaw)
    {
        string path = Source + name + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new FileNotFoundException(path);
        var anchor = new GameObject(Path.GetFileName(name)); anchor.transform.SetParent(props);
        anchor.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor.transform);
        Bounds bounds = BoundsOf(instance);
        if (bounds.size.y <= .0001f) throw new InvalidOperationException("Empty model: " + name);
        instance.transform.localScale *= height / bounds.size.y;
        bounds = BoundsOf(instance);
        instance.transform.position += new Vector3(position.x - bounds.center.x, position.y - bounds.min.y, position.z - bounds.center.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        propCount++; used.Add(name);
    }

    static Bounds BoundsOf(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>().Where(x => !(x is ParticleSystemRenderer)).ToArray();
        if (renderers.Length == 0) throw new InvalidOperationException("No mesh: " + go.name);
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static Material Material(string name, Color color)
    {
        string path = Art + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
        material.color = color; material.SetFloat("_Glossiness", .05f); EditorUtility.SetDirty(material);
        return material;
    }

    static void Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(type); go.name = name;
        go.transform.position = position; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
    }

    static void Point(string name, Vector3 position, Color color, float intensity, float range)
    {
        var light = new GameObject(name).AddComponent<Light>(); light.type = LightType.Point;
        light.transform.position = position; light.color = color; light.intensity = intensity; light.range = range;
    }

    static void Capture(Camera camera, string path)
    {
        var render = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
        var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = previous;
            render.Release(); Object.DestroyImmediate(render); Object.DestroyImmediate(texture);
        }
    }

    [Serializable] class Report
    {
        public string unityVersion, scene, note;
        public int propInstances;
        public string[] uniquePrefabs;
        public bool selectedMaterialsAndScriptsValid;
    }
}
