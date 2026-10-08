using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MonsterAssetBuilder
{
    const string Source = "Assets/ThirdParty/Quaternius/UltimateMonsters/Models/";
    public static void BuildVarietyPrefabs()
    {
        AssetDatabase.Refresh();
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Monsters/MonsterAtlas.mat");
        foreach (string name in new[] { "Ghost", "Armabee", "Demon" })
        {
            string path = Source + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true; importer.isReadable = true;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.loopTime = clip.name.Contains("Flying");
                clip.wrapMode = clip.loopTime ? WrapMode.Loop : WrapMode.Once;
            }
            importer.clipAnimations = clips; importer.SaveAndReimport();
            var root = new GameObject("Quaternius " + name);
            var fit = new GameObject("Model fit"); fit.transform.SetParent(root.transform, false);
            var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), fit.transform);
            var flight = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .First(x => x.name.EndsWith("|Flying_Idle"));
            flight.SampleAnimation(model, .3f);
            Bounds bounds = new Bounds(); bool first = true;
            foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.sharedMaterials = skin.sharedMaterials.Select(_ => material).ToArray();
                skin.updateWhenOffscreen = true;
                var baked = new Mesh(); skin.BakeMesh(baked);
                foreach (var vertex in baked.vertices)
                {
                    Vector3 world = skin.transform.TransformPoint(vertex);
                    if (first) { bounds = new Bounds(world, Vector3.zero); first = false; }
                    else bounds.Encapsulate(world);
                }
                Object.DestroyImmediate(baked);
            }
            // FBX skin transforms include 100x authored units; runtime legacy skinning
            // evaluates in rig units. Normalize outside the animated model as for Dragon.
            float authoredUnits = model.GetComponentInChildren<SkinnedMeshRenderer>().transform.lossyScale.x;
            bounds.center /= authoredUnits; bounds.size /= authoredUnits;
            float targetSize = name == "Demon" ? 3.6f : name == "Ghost" ? 2.6f : 2.8f;
            float scale = targetSize / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            fit.transform.localScale = Vector3.one * scale;
            fit.transform.localPosition = -bounds.center * scale;
            var animation = model.GetComponentInChildren<Animation>();
            animation.playAutomatically = false; animation.cullingType = AnimationCullingType.AlwaysAnimate;
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/Monsters/" + name + ".prefab");
            Debug.Log("MONSTER_VARIETY_FIT: " + name + " sampled=" + bounds.size + " fit=" + scale +
                " clips=" + string.Join(",", clips.Select(x => x.name)));
            Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
    }
    public static void BuildPrefabs()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory("Assets/Resources/Monsters");
        AssetDatabase.Refresh();
        string texturePath = "Assets/ThirdParty/Quaternius/UltimateMonsters/Textures/Atlas_Monsters.png";
        var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        textureImporter.textureType = TextureImporterType.Default;
        textureImporter.sRGBTexture = true;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.SaveAndReimport();
        string materialPath = "Assets/Resources/Monsters/MonsterAtlas.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, materialPath); }
        material.color = Color.white;
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        material.SetFloat("_Glossiness", .15f);
        EditorUtility.SetDirty(material);
        foreach (string name in new[] { "Dragon", "Dragon_Evolved" })
        {
            string path = Source + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.isReadable = true;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                bool flying = clip.name.Contains("Flying");
                clip.loopTime = flying; clip.wrapMode = flying ? WrapMode.Loop : WrapMode.Once;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            var root = new GameObject("Quaternius " + name);
            var fit = new GameObject("Model fit"); fit.transform.SetParent(root.transform, false);
            var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), fit.transform);
            var renderers = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => material).ToArray();
                if (renderer is SkinnedMeshRenderer skin) skin.updateWhenOffscreen = true;
            }
            // These FBX files have a 100x mesh transform. Their cached bind-pose
            // bounds include that factor, while the evaluated legacy skin does not.
            // Normalize in the rig's authored units, outside the animated root.
            float rigUnits = model.GetComponentInChildren<SkinnedMeshRenderer>().transform.lossyScale.x;
            bounds.center /= rigUnits; bounds.size /= rigUnits;
            float scale = (name == "Dragon" ? 4.2f : 4.6f) / bounds.size.x;
            fit.transform.localScale = Vector3.one * scale;
            fit.transform.localPosition = -bounds.center * scale;
            var animation = model.GetComponentInChildren<Animation>();
            animation.playAutomatically = false;
            animation.cullingType = AnimationCullingType.AlwaysAnimate;
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/Monsters/" + name + ".prefab");
            Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("QUATERNIUS_PREFABS_READY: Dragon and Dragon_Evolved with authored rig and animations.");
    }
    public static void RefitAndInspect() { BuildPrefabs(); Inspect(); }
    public static void Inspect()
    {
        AssetDatabase.Refresh();
        foreach (string name in new[] { "Dragon", "Dragon_Evolved" })
        {
            string path = Source + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = Object.Instantiate(model);
            Bounds bounds = new Bounds(); bool first = true;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds);
                Debug.Log("MONSTER_MATERIAL: " + name + " " + renderer.name + " " + string.Join(",", renderer.sharedMaterials.Select(x => x.name + "=" + x.color)));
            }
            Debug.Log("MONSTER_BOUNDS: " + name + " center=" + bounds.center + " size=" + bounds.size);
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(x => !x.name.StartsWith("__preview__")))
                Debug.Log("MONSTER_CLIP: " + name + " " + clip.name + " " + clip.length);
            Debug.Log("MONSTER_ANIMATION_COMPONENT: " + instance.GetComponentsInChildren<Animation>().Length);
            foreach (var t in instance.GetComponentsInChildren<Transform>())
                if (t.parent == instance.transform || t == instance.transform) Debug.Log("MONSTER_TRANSFORM: " + t.name + " " + t.localPosition + " scale=" + t.localScale);
            var flight = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(x => x.name.EndsWith("|Flying_Idle"));
            flight.SampleAnimation(instance, .3f);
            foreach (var skin in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var baked = new Mesh(); skin.BakeMesh(baked);
                Bounds world = new Bounds(skin.transform.TransformPoint(baked.vertices[0]), Vector3.zero);
                foreach (var v in baked.vertices) world.Encapsulate(skin.transform.TransformPoint(v));
                Debug.Log("MONSTER_SAMPLED: " + name + " renderer=" + skin.bounds + " mesh=" + baked.bounds + " scale=" + skin.transform.lossyScale + " root=" + instance.transform.localScale + " world=" + world);
                Object.DestroyImmediate(baked);
            }
            var fitted = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Monsters/" + name + ".prefab"));
            var legacy = fitted.GetComponentInChildren<Animation>();
            flight.SampleAnimation(legacy.gameObject, .3f);
            foreach (var skin in fitted.GetComponentsInChildren<SkinnedMeshRenderer>())
                Debug.Log("MONSTER_FITTED: " + name + " renderer=" + skin.bounds + " root=" + legacy.transform.localScale + " position=" + legacy.transform.localPosition + " bones=" + skin.rootBone.position + " meshTransform=" + skin.transform.localScale + " rootBoneScale=" + skin.rootBone.lossyScale);
            Object.DestroyImmediate(fitted);
            Object.DestroyImmediate(instance);
        }
    }
}
