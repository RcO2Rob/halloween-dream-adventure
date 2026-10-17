using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LostDream
{
    // One pair of embedded typefaces for the entire game, including world-space labels.
    public static class DreamTypography
    {
        static Font display, body, strong;
        static readonly Dictionary<string,string> readingCase = new Dictionary<string,string>();
        public static bool AuditEnabled;
        public static readonly List<string> Overflow = new List<string>();
        public static void Audit(Rect rect, string value, GUIStyle style)
        {
            if (!AuditEnabled || Event.current.type != EventType.Repaint) return;
            float height = style.CalcHeight(new GUIContent(value),rect.width);
            if (height > rect.height + 1 && !Overflow.Contains(value)) Overflow.Add(value);
        }
        public static Font Display => display != null ? display : display = Resources.Load<Font>("Typography/Griffy-Regular");
        public static Font Body => body != null ? body : body = Resources.Load<Font>("Typography/AveriaSansLibre-Regular");
        public static Font Strong => strong != null ? strong : strong = Resources.Load<Font>("Typography/AveriaSansLibre-Bold");

        public static string ReadingCase(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Any(char.IsLower)) return value;
            if (readingCase.TryGetValue(value,out string cached)) return cached;
            string result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
            result = Regex.Replace(result, @"\b(W|A|S|D|E|Q|R|F|M|I|Ii|Iii|Iv|Wasd)\b", m => m.Value.ToUpperInvariant());
            result = result.Replace(" / ", "  /  ");
            readingCase[value] = result; return result;
        }

        public static void ApplyWorld(Transform root)
        {
            foreach (var text in root.GetComponentsInChildren<TextMesh>(true)) ApplyWorld(text);
        }
        public static void ApplyWorld(TextMesh text)
        {
            Font target = text.name == "Instructions" ? Strong : Body;
            if (target == null || text.font == target) return;
            Font old = text.font != null ? text.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            float before = LineWidth(old, text.text, text.fontSize, text.fontStyle);
            float after = LineWidth(target, text.text, text.fontSize, FontStyle.Normal);
            // Keep the existing sign's physical width. Text position, color and billboard behavior stay authored.
            if (before > 0 && after > before) text.characterSize *= before / after;
            text.font = target; text.fontStyle = FontStyle.Normal;
            text.GetComponent<Renderer>().sharedMaterial = target.material;
        }
        static float LineWidth(Font font, string value, int size, FontStyle style)
        {
            if (font == null || string.IsNullOrEmpty(value)) return 0;
            size = Mathf.Max(1,size);
            font.RequestCharactersInTexture(value,size,style);
            float widest = 0, line = 0;
            foreach (char c in value)
            {
                if (c == '\n') { widest = Mathf.Max(widest,line); line = 0; continue; }
                if (font.GetCharacterInfo(c,out CharacterInfo info,size,style)) line += info.advance;
            }
            return Mathf.Max(widest,line);
        }
    }
}
