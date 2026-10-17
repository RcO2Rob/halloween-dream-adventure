using System;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEngine;

public static class TypographyBuilder
{
    [MenuItem("Lost Dream/Art/Validate embedded typography")]
    public static void Validate()
    {
        foreach (string name in new[] { "Griffy-Regular", "AveriaSansLibre-Regular", "AveriaSansLibre-Bold" })
        {
            string path = "Assets/Resources/Typography/" + name + ".ttf";
            AssetDatabase.ImportAsset(path);
            var importer = (TrueTypeFontImporter)AssetImporter.GetAtPath(path);
            if (!importer.includeFontData || importer.fontTextureCase != FontTextureCase.Dynamic)
            {
                importer.includeFontData = true; importer.fontTextureCase = FontTextureCase.Dynamic;
                importer.SaveAndReimport();
            }
            Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font == null || !font.dynamic || !"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789[]<>/.,'!?+-".All(font.HasCharacter))
                throw new InvalidOperationException("Incomplete embedded font: " + path);
        }
        Debug.Log("LOST_DREAM_TYPOGRAPHY_VALIDATED: embedded display, body and emphasis fonts");
    }
    public static void ValidateAndBuild() { Validate(); PrototypeBuilder.BuildCurrentScenes(); }
}
