using UnityEngine;

namespace LostDream
{
    public enum CandyHue { Pink, Blue, Gold }

    public static class CandyPalette
    {
        public static string Name(CandyHue hue) => hue.ToString().ToUpperInvariant();
        public static string Symbol(CandyHue hue) => hue == CandyHue.Pink ? "[O]" : hue == CandyHue.Blue ? "[<>]" : "[*]";
        public static string Label(CandyHue hue) => Name(hue) + " " + Symbol(hue);
        public static string Material(CandyHue hue) => hue == CandyHue.Pink ? "Candy" : "Candy" + hue;
        public static Color Color(CandyHue hue) => hue == CandyHue.Pink ? new Color(1, .51f, .73f) :
            hue == CandyHue.Blue ? new Color(.32f, .72f, 1) : new Color(1, .8f, .27f);

        public static void Paint(Transform candy, CandyHue hue)
        {
            foreach (var renderer in candy.GetComponentsInChildren<Renderer>(true))
                if (renderer.name == "Sweet" || renderer.name == "Candy glimmer")
                    renderer.sharedMaterial = DreamArt.Mat(Material(hue));
        }
    }
}
