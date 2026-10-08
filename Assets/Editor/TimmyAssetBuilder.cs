using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class TimmyAssetBuilder
{
    const string Source = "Assets/ThirdParty/Mixamo/Timmy/";
    const string Output = "Assets/Resources/Characters/";
    public static void BuildPrefab()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory(Source + "Textures");
        AssetDatabase.Refresh();
        string modelPath = Source + "Models/Timmy.fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.isReadable = true;
        importer.importAnimation = false;
        importer.SaveAndReimport();
        importer.ExtractTextures(Source + "Textures");
        AssetDatabase.Refresh();
        foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { Source + "Textures" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var texture = (TextureImporter)AssetImporter.GetAtPath(path);
            texture.maxTextureSize = 2048;
            texture.textureType = path.Contains("Normal") ? TextureImporterType.NormalMap : TextureImporterType.Default;
            texture.sRGBTexture = path.Contains("Diffuse");
            texture.SaveAndReimport();
        }
        var sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var avatar = sourceModel.GetComponent<Animator>().avatar;
        var clips = new System.Collections.Generic.Dictionary<string, AnimationClip>();
        foreach (string name in new[] { "Idle", "Running", "BowAim", "Throw" })
        {
            string path = Source + "Animations/" + name + ".fbx";
            var motion = (ModelImporter)AssetImporter.GetAtPath(path);
            motion.animationType = ModelImporterAnimationType.Human;
            motion.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            motion.sourceAvatar = avatar;
            motion.importAnimation = true;
            var settings = motion.defaultClipAnimations;
            foreach (var clip in settings)
            {
                clip.name = name;
                clip.loopTime = name != "Throw";
                clip.loopPose = name != "Throw";
                clip.lockRootRotation = true;
                clip.keepOriginalOrientation = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionXZ = true;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionY = true;
                clip.heightFromFeet = true;
            }
            motion.clipAnimations = settings;
            motion.SaveAndReimport();
            clips[name] = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(x => !x.name.StartsWith("__preview__"));
            Debug.Log("TIMMY_CLIP: " + name + " duration=" + clips[name].length + " human=" + clips[name].humanMotion);
        }
        string controllerPath = Output + "Timmy.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null) AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var locomotion = controller.layers[0].stateMachine.AddState("Locomotion");
        var blend = new BlendTree { name = "Idle and run", blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(blend, controller);
        blend.AddChild(clips["Idle"], 0); blend.AddChild(clips["Running"], 6);
        locomotion.motion = blend;
        controller.layers[0].stateMachine.defaultState = locomotion;
        var mask = new AvatarMask { name = "Upper body actions" };
        for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
        foreach (var part in new[] { AvatarMaskBodyPart.Body, AvatarMaskBodyPart.Head, AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftFingers, AvatarMaskBodyPart.RightFingers })
            mask.SetHumanoidBodyPartActive(part, true);
        AssetDatabase.AddObjectToAsset(mask, controller);
        controller.AddLayer("Upper body actions");
        var layers = controller.layers;
        layers[1].defaultWeight = 1; layers[1].avatarMask = mask;
        controller.layers = layers;
        var actionMachine = controller.layers[1].stateMachine;
        var relax = actionMachine.AddState("Relax");
        actionMachine.defaultState = relax;
        actionMachine.AddState("BowAim").motion = clips["BowAim"];
        var throwing = actionMachine.AddState("Throw"); throwing.motion = clips["Throw"];
        throwing.speed = clips["Throw"].length / .55f;
        var finish = throwing.AddTransition(relax); finish.hasExitTime = true; finish.exitTime = .95f; finish.duration = .08f;
        var material = AssetDatabase.LoadAssetAtPath<Material>(Output + "TimmyBody.mat");
        if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, Output + "TimmyBody.mat"); }
        material.color = Color.white;
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "Textures/Ch09_1001_Diffuse.png");
        material.SetFloat("_Glossiness", .1f);
        material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "Textures/Ch09_1001_Normal.png"));
        material.EnableKeyword("_NORMALMAP");
        var root = new GameObject("Timmy — Mixamo");
        var fit = new GameObject("Model fit"); fit.transform.SetParent(root.transform, false);
        var model = Object.Instantiate(sourceModel, fit.transform);
        var animator = model.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // Use evaluated skin vertices rather than the importer's conservative animated bounds.
        Bounds bounds = new Bounds(); bool first = true;
        foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skin.sharedMaterials = skin.sharedMaterials.Select(_ => material).ToArray();
            skin.updateWhenOffscreen = true;
            var mesh = new Mesh(); skin.BakeMesh(mesh);
            foreach (var vertex in mesh.vertices)
            {
                Vector3 point = skin.transform.TransformPoint(vertex);
                if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
            }
            Object.DestroyImmediate(mesh);
        }
        float scale = 2.1f / bounds.size.y;
        fit.transform.localScale = Vector3.one * scale;
        fit.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, 0) * scale;
        var rig = root.AddComponent<LostDream.TimmyAnimation>();
        rig.animator = animator;
        rig.chestBindRotation = animator.GetBoneTransform(HumanBodyBones.Chest).rotation;
        rig.headBindRotation = animator.GetBoneTransform(HumanBodyBones.Head).rotation;
        PrefabUtility.SaveAsPrefabAsset(root, Output + "Timmy.prefab");
        Debug.Log("TIMMY_PREFAB_READY: evaluated bounds=" + bounds + " scale=" + scale);
        Object.DestroyImmediate(root);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
    }
    public static void Inspect()
    {
        AssetDatabase.Refresh();
        string path = Source + "Models/Timmy.fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.isReadable = true;
        importer.SaveAndReimport();
        Directory.CreateDirectory(Source + "Textures");
        importer.ExtractTextures(Source + "Textures");
        AssetDatabase.Refresh();
        var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        var animator = model.GetComponent<Animator>();
        Debug.Log("TIMMY_AVATAR: " + animator.avatar.isValid + " human=" + animator.avatar.isHuman);
        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            Debug.Log("TIMMY_SKIN: " + renderer.name + " bounds=" + renderer.bounds + " scale=" + renderer.transform.lossyScale + " materials=" + string.Join(",",renderer.sharedMaterials.Select(x => x.name + " texture=" + x.mainTexture)));
        foreach (HumanBodyBones bone in new[] { HumanBodyBones.Hips, HumanBodyBones.LeftFoot, HumanBodyBones.LeftHand, HumanBodyBones.RightHand })
        {
            var t = animator.GetBoneTransform(bone);
            Debug.Log("TIMMY_BONE: " + bone + " position=" + t.position + " rotation=" + t.rotation.eulerAngles);
        }
        Object.DestroyImmediate(model);
        AssetDatabase.SaveAssets();
    }
}
