using UnityEditor;
using UnityEngine;

public static class TitleScreenBuilder
{
    public static void Build()
    {
        AssetDatabase.Refresh();
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/TitleBackdrop.png");
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        PlayerSettings.bundleVersion = "0.12.0";
        PrototypeBuilder.ValidateScenes();
        PrototypeBuilder.BuildMac();
    }
}
