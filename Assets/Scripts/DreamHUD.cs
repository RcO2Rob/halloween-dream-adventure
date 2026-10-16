using UnityEngine;

namespace LostDream
{
    public class DreamHUD : MonoBehaviour
    {
        GUIStyle title, heading, text, small, button, centered;
        GUIStyle menuTitle, menuSubtitle, menuPrimary, menuChapter, menuSmall;
        Texture2D menuShade;
        readonly Color cream = new Color(.96f, .91f, .79f);
        readonly Color gold = new Color(1, .69f, .3f);
        readonly Color teal = new Color(.4f, .91f, .83f);
        readonly Color panel = new Color(.045f, .04f, .09f, .88f);
        float width, height, scale;
        void Awake() { gameObject.AddComponent<TitleBackdropDisplay>(); }
        void Styles()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 21, wordWrap = true };
            text.normal.textColor = cream;
            small = new GUIStyle(text) { fontSize = 16 };
            heading = new GUIStyle(text) { fontSize = 28, fontStyle = FontStyle.Bold };
            title = new GUIStyle(text) { fontSize = 68, fontStyle = FontStyle.Bold };
            centered = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 22, padding = new RectOffset(15, 15, 12, 12) };
            button.normal.textColor = cream;
            button.normal.background = ButtonTexture(new Color(.05f, .06f, .10f), new Color(.35f, .43f, .48f));
            button.hover.background = ButtonTexture(new Color(.08f, .20f, .20f), teal);
            button.active.background = ButtonTexture(new Color(.14f, .22f, .22f), gold);
            button.hover.textColor = cream; button.active.textColor = gold;
            button.border = new RectOffset(2, 2, 2, 2);
            menuShade = new Texture2D(128, 1, TextureFormat.RGBA32, false, true);
            menuShade.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < 128; x++)
            {
                float shade = 1 - x / 127f;
                menuShade.SetPixel(x, 0, new Color(0, 0, 0, .94f * shade * shade));
            }
            menuShade.Apply();
            menuTitle = new GUIStyle(title) { fontSize = 64 };
            menuSubtitle = new GUIStyle(title) { fontSize = 34 };
            menuSmall = new GUIStyle(small) { fontSize = 14 };
            menuPrimary = new GUIStyle(button) { fontSize = 21, fontStyle = FontStyle.Bold };
            menuPrimary.normal.background = ButtonTexture(gold, gold);
            menuPrimary.normal.textColor = new Color(.08f, .045f, .08f);
            menuPrimary.hover.background = ButtonTexture(cream, cream);
            menuPrimary.hover.textColor = menuPrimary.normal.textColor;
            menuPrimary.active.background = ButtonTexture(teal, teal);
            menuPrimary.active.textColor = menuPrimary.normal.textColor;
            menuChapter = new GUIStyle(button) { fontSize = 17, alignment = TextAnchor.MiddleLeft };
            menuChapter.padding = new RectOffset(14, 14, 4, 4);
            menuChapter.normal.background = ButtonTexture(new Color(.045f, .035f, .085f, .8f), new Color(.55f, .47f, .65f, .55f));
        }
        Texture2D ButtonTexture(Color fill, Color border)
        {
            var texture = new Texture2D(8, 8);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                texture.SetPixel(x, y, x == 0 || y == 0 || x == 7 || y == 7 ? border : fill);
            texture.Apply(); return texture;
        }
        void Panel(Rect rect, Color color)
        { Color before = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = before; }
        void Label(Rect rect, string label, GUIStyle style, Color? color = null)
        { Color before = style.normal.textColor; if (color.HasValue) style.normal.textColor = color.Value; GUI.Label(rect, label, style); style.normal.textColor = before; }
        void OnGUI()
        {
            var game = DreamGame.Instance;
            if (game == null || game.player == null) return;
            if (text == null) Styles();
            scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            width = Screen.width / scale; height = Screen.height / scale;
            Matrix4x4 before = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            if (game.Phase == GamePhase.Menu) Menu(game);
            else
            {
                PlayHUD(game);
                if (game.Phase == GamePhase.Paused) Modal(game, "THE DREAM IS PAUSED", "Take a breath. The nightmare can wait.", "RESUME", game.Resume);
                else if (game.Phase == GamePhase.Defeated) Modal(game, "LOST IN THE NIGHTMARE", "Your lanterns remember the way. Try this chapter again.", "TRY AGAIN  /  ENTER", game.Retry);
                else if (game.Phase == GamePhase.LevelComplete) Modal(game, "A DREAM RESTORED", "The gate leads deeper into the dream.", "NEXT CHAPTER  /  ENTER", game.ContinueJourney);
                else if (game.Phase == GamePhase.Victory) Modal(game, "THE NIGHTMARE IS OVER", "The Pumpkin King has fallen. Every lantern shines again.", "DREAM AGAIN  /  ENTER", () => game.LoadChapter(0));
            }
            GUI.matrix = before;
        }
        void Menu(DreamGame game)
        {
            GUI.DrawTexture(new Rect(0, 0, Mathf.Min(width * .52f, 740), height), menuShade);
            Label(new Rect(64, 56, 560, 28), "A HALLOWEEN DREAM ADVENTURE", menuSmall, gold);
            Label(new Rect(61, 101, 650, 90), "LANTERNS", menuTitle);
            Label(new Rect(64, 188, 650, 54), "OF THE LOST DREAM", menuSubtitle);
            Panel(new Rect(64, 261, 54, 3), gold);
            Label(new Rect(64, 288, 350, 70), "Light the lanterns.\nFind your way through the dream.", text);
            float actionsY = Mathf.Max(390, height - 440);
            if (GUI.Button(new Rect(64, actionsY, 320, 58), "BEGIN THE DREAM", menuPrimary)) game.StartPlaying();
            Label(new Rect(64, actionsY + 68, 320, 24), "PRESS ENTER TO BEGIN", menuSmall);
            Label(new Rect(64, actionsY + 115, 340, 25), "OR CHOOSE A CHAPTER", menuSmall, gold);
            for (int i = 0; i < 3; i++)
            {
                int chapter = i;
                if (GUI.Button(new Rect(64, actionsY + 150 + i * 48, 320, 40), "0" + (i + 1) + "   " + DreamGame.ChapterNames[i], menuChapter)) game.LoadChapter(chapter);
            }
            Label(new Rect(64, height - 82, 640, 60), "WASD / move    Space / jump    Shift / dodge\nLeft-click / bow    E / interact    Q / candy", menuSmall);
            if (GUI.Button(new Rect(width - 228, 38, 188, 36), game.audioSystem.Muted ? "SOUND OFF  /  M" : "SOUND ON  /  M", menuChapter))
                game.audioSystem.ToggleMute();
        }
        void OnDestroy()
        {
            if (menuShade != null) Destroy(menuShade);
        }
        void PlayHUD(DreamGame game)
        {
            Panel(new Rect(28, 24, 452, 82), panel);
            Label(new Rect(48, 36, 400, 23), "CHAPTER 0" + (game.levelIndex + 1) + "  /  LOST DREAM", small, gold);
            Label(new Rect(48, 62, 410, 34), DreamGame.ChapterNames[game.levelIndex], heading);
            Panel(new Rect(width - 302, 24, 274, 82), panel);
            Label(new Rect(width - 282, 36, 230, 23), "DREAMWALKER", small, teal);
            for (int i = 0; i < game.player.maxHealth; i++)
            {
                Panel(new Rect(width - 279 + i * 45, 68, 31, 17), i < game.player.Health ? new Color(.97f, .4f, .46f) : new Color(.24f, .2f, .3f));
            }
            if (game.levelIndex == 2 && game.boss.AwakeInArena)
            {
                Panel(new Rect(width / 2 - 245, 26, 490, 78), panel);
                Color required = CandyPalette.Color(game.boss.RequiredHue);
                Label(new Rect(width / 2 - 225, 35, 450, 28), "THE PUMPKIN KING", centered, required);
                Panel(new Rect(width / 2 - 220, 78, 440, 10), new Color(.22f, .16f, .28f));
                Panel(new Rect(width / 2 - 220, 78, 440 * game.boss.Health / game.boss.maxHealth, 10), required);
                string attack = game.boss.ChargeWarningActive ? "CHARGE INCOMING  /  LEAVE THE LANE" :
                    game.boss.Phase == BossPhase.Charging ? "CHARGE  /  DODGE SIDEWAYS" :
                    game.boss.Phase == BossPhase.Recovering ? "STUNNED  /  THROW CANDY" :
                    game.boss.Exposed ? "WEAK POINT OPEN  /  THROW CANDY" :
                    game.boss.Phase == BossPhase.Returning ? "SHELL CLOSED  /  PREPARE" : "WATCH THE BOMB CIRCLES";
                Label(new Rect(width / 2 - 240, 108, 480, 30), attack, centered, game.boss.Exposed ? Color.white : cream);
                Panel(new Rect(width / 2 - 245, 140, 490, 34), panel);
                Label(new Rect(width / 2 - 240, 141, 480, 30), "MATCH " + game.boss.RequiredCandyLabel + " CANDY", centered, required);
            }
            Panel(new Rect(28, height - 128, 600, 100), panel);
            string progress = "LANTERNS  " + game.LitCount + "/" + game.lanterns.Length;
            if (game.levelIndex != 2) progress += "     ENEMIES  " + (game.dragons.Length - game.DragonsRemaining) + "/" + game.dragons.Length;
            else if (game.boss.AwakeInArena) progress += "     WAVE " + game.boss.SummonWavesStarted + "/2  /  ENEMIES " + game.boss.ActiveSummons;
            Label(new Rect(48, height - 113, 560, 24), progress, small, gold);
            Label(new Rect(48, height - 82, 552, 56), game.Objective(), text);
            Panel(new Rect(width - 360, height - 156, 332, 128), panel);
            Label(new Rect(width - 340, height - 141, 300, 26), game.player.CarryingCandy ? CandyPalette.Label(game.player.HeldCandyHue) + " CANDY  /  Q" : "BOW READY  /  LEFT-CLICK", small, game.player.CarryingCandy ? CandyPalette.Color(game.player.HeldCandyHue) : teal);
            Label(new Rect(width - 340, height - 113, 300, 25), game.player.IsGrounded ? "SPACE  /  JUMP" : "IN THE AIR", small);
            Label(new Rect(width - 340, height - 86, 300, 25), game.player.DashCooldown <= 0 ? "SHIFT  /  DODGE READY" : "DODGE RECHARGING", small);
            Label(new Rect(width - 340, height - 59, 300, 22), "E / interact    Esc / pause    R / retry", small);
            if (game.Phase == GamePhase.Playing)
            {
                if (game.player.Nearby != null)
                {
                    Panel(new Rect(width / 2 - 310, height - 200, 620, 48), panel);
                    Label(new Rect(width / 2 - 295, height - 194, 590, 36), "[ E ]  " + game.player.Nearby.Prompt, centered, gold);
                }
                if (Time.unscaledTime < game.ToastUntil)
                {
                    float toastY = game.levelIndex == 2 && game.boss.AwakeInArena ? 185 : 151;
                    Panel(new Rect(width / 2 - 450, toastY, 900, 50), panel);
                    Label(new Rect(width / 2 - 434, toastY + 6, 868, 37), game.Toast, centered);
                }
                Reticle(game);
                TargetLabels(game);
            }
        }
        void Reticle(DreamGame game)
        {
            Vector2 p = new Vector2(Input.mousePosition.x / scale, (Screen.height - Input.mousePosition.y) / scale);
            Color color = game.player.CarryingCandy ? CandyPalette.Color(game.player.HeldCandyHue) : teal;
            Panel(new Rect(p.x - 11, p.y - 1, 7, 2), color);
            Panel(new Rect(p.x + 4, p.y - 1, 7, 2), color);
            Panel(new Rect(p.x - 1, p.y - 11, 2, 7), color);
            Panel(new Rect(p.x - 1, p.y + 4, 2, 7), color);
        }
        void TargetLabels(DreamGame game)
        {
            foreach (var dragon in game.dragons)
            {
                if (dragon == null || dragon.Defeated || Vector3.Distance(dragon.transform.position, game.player.transform.position) > 17) continue;
                Vector3 p = game.gameCamera.WorldToScreenPoint(dragon.AimPosition + Vector3.up * 1.2f);
                if (p.z <= 0) continue;
                float x = p.x / scale, y = (Screen.height - p.y) / scale;
                var nameStyle = new GUIStyle(small) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                Label(new Rect(x - 120, y - 28, 240, 24), dragon.DisplayName, nameStyle, cream);
                Panel(new Rect(x - 24, y, 48, 5), panel);
                Panel(new Rect(x - 24, y, 48f * dragon.health / dragon.MaxHealth, 5), teal);
                if (dragon.Phase == DragonPhase.Telegraph)
                    Label(new Rect(x - 160, y + 7, 320, 24), dragon.AttackHint, nameStyle, gold);
            }
        }
        void Modal(DreamGame game, string caption, string description, string action, System.Action callback)
        {
            Panel(new Rect(0, 0, width, height), new Color(.025f, .02f, .06f, .7f));
            Rect box = new Rect(width / 2 - 370, height / 2 - 165, 740, 330);
            Panel(box, panel);
            Label(new Rect(box.x + 35, box.y + 35, 670, 58), caption, new GUIStyle(heading) { alignment = TextAnchor.MiddleCenter }, gold);
            Label(new Rect(box.x + 45, box.y + 105, 650, 65), description, centered);
            if (GUI.Button(new Rect(box.x + 95, box.y + 193, 550, 56), action, button)) callback();
            if (GUI.Button(new Rect(box.x + 245, box.y + 266, 250, 37), "CHAPTER SELECT", button))
                game.ReturnToMenu();
        }
    }
}
