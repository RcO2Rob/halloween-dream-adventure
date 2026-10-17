using UnityEngine;

namespace LostDream
{
    public class DreamHUD : MonoBehaviour
    {
        GUIStyle title, heading, text, small, button, centered;
        GUIStyle menuTitle, menuSubtitle, menuPrimary, menuChapter, menuSmall, compactButton;
        Texture2D menuShade;
        readonly Color cream = new Color(.96f, .91f, .79f);
        readonly Color gold = new Color(1, .69f, .3f);
        readonly Color teal = new Color(.4f, .91f, .83f);
        readonly Color panel = new Color(.045f, .04f, .075f, .82f);
        float width, height, scale;
        void Awake() { gameObject.AddComponent<TitleBackdropDisplay>(); }
        void Styles()
        {
            Font font = DreamTypography.Body;
            text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 21, wordWrap = true, padding = new RectOffset(0,0,0,0) };
            text.normal.textColor = cream;
            small = new GUIStyle(text) { fontSize = 18 };
            heading = new GUIStyle(text) { font = DreamTypography.Display, fontSize = 32 };
            title = new GUIStyle(text) { font = DreamTypography.Display, fontSize = 88 };
            centered = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { font = DreamTypography.Strong, fontSize = 22, padding = new RectOffset(15, 15, 12, 12) };
            button.normal.textColor = cream;
            button.normal.background = ButtonTexture(new Color(.05f, .06f, .10f), new Color(.35f, .43f, .48f));
            button.hover.background = ButtonTexture(new Color(.08f, .20f, .20f), teal);
            button.active.background = ButtonTexture(new Color(.14f, .22f, .22f), gold);
            button.hover.textColor = cream; button.active.textColor = gold;
            button.border = new RectOffset(2, 2, 2, 2);
            compactButton = new GUIStyle(button) { fontSize = 18, padding = new RectOffset(12,12,4,4) };
            menuShade = new Texture2D(128, 1, TextureFormat.RGBA32, false, true);
            menuShade.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < 128; x++)
            {
                float shade = 1 - x / 127f;
                menuShade.SetPixel(x, 0, new Color(0, 0, 0, .94f * shade * shade));
            }
            menuShade.Apply();
            menuTitle = new GUIStyle(title) { fontSize = 96 };
            menuSubtitle = new GUIStyle(title) { fontSize = 38 };
            menuSmall = new GUIStyle(small) { fontSize = 17 };
            menuPrimary = new GUIStyle(button) { fontSize = 23 };
            menuPrimary.normal.background = ButtonTexture(gold, gold);
            menuPrimary.normal.textColor = new Color(.08f, .045f, .08f);
            menuPrimary.hover.background = ButtonTexture(cream, cream);
            menuPrimary.hover.textColor = menuPrimary.normal.textColor;
            menuPrimary.active.background = ButtonTexture(teal, teal);
            menuPrimary.active.textColor = menuPrimary.normal.textColor;
            menuChapter = new GUIStyle(button) { font = DreamTypography.Body, fontSize = 21, alignment = TextAnchor.MiddleLeft };
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
        {
            label = DreamTypography.ReadingCase(label);
            Color before = style.normal.textColor;
            if (color.HasValue) style.normal.textColor = color.Value;
            int originalSize = style.fontSize;
            // Font metrics differ: fit the actual line height, rather than clipping descenders.
            while (style.fontSize > 15 && style.CalcHeight(new GUIContent(label),rect.width) > rect.height + .5f) style.fontSize--;
            DreamTypography.Audit(rect,label,style);
            GUI.Label(rect,label,style);
            style.fontSize = originalSize; style.normal.textColor = before;
        }
        bool Button(Rect rect, string label, GUIStyle style)
        {
            label = DreamTypography.ReadingCase(label);
            DreamTypography.Audit(rect,label,style);
            return GUI.Button(rect,label,style);
        }
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
            else if (game.Phase == GamePhase.Victory) Victory(game);
            else if (game.Phase == GamePhase.Playing && game.IntroductionVisible) Introduction(game);
            else
            {
                PlayHUD(game);
                if (game.Phase == GamePhase.Paused) Modal(game, "The dream is paused", "Take a breath. The nightmare can wait.", "Resume", game.Resume);
                else if (game.Phase == GamePhase.Defeated) Modal(game, "Lost in the nightmare", "Your lanterns remember the way. Try this chapter again.", "Try again  /  Enter", game.Retry);
            }
            GUI.matrix = before;
        }
        void Menu(DreamGame game)
        {
            GUI.DrawTexture(new Rect(0, 0, Mathf.Min(width * .52f, 740), height), menuShade);
            Label(new Rect(64, 54, 560, 28), "A Halloween dream adventure", menuSmall, gold);
            Label(new Rect(58, 86, 650, 126), "Lanterns", menuTitle);
            Label(new Rect(64, 210, 650, 60), "of the Lost Dream", menuSubtitle);
            Panel(new Rect(64, 279, 74, 2), gold);
            Label(new Rect(64, 302, 380, 64), "Light the lanterns.\nFind your way through the dream.", text);
            float actionsY = Mathf.Max(365, height - 490);
            if (Button(new Rect(64, actionsY, 320, 58), "Begin the dream", menuPrimary)) game.StartPlaying();
            Label(new Rect(64, actionsY + 68, 320, 24), "Press Enter to begin", menuSmall);
            Label(new Rect(64, actionsY + 115, 340, 25), "Choose a chapter", menuSmall, gold);
            for (int i = 0; i < DreamGame.SceneNames.Length; i++)
            {
                int chapter = i;
                if (Button(new Rect(64, actionsY + 150 + i * 48, 320, 40), "0" + (i + 1) + "   " + DreamGame.ChapterNames[i], menuChapter)) game.LoadChapter(chapter);
            }
            Label(new Rect(64, height - 82, 640, 60), "WASD / move    Mouse / look    Space / jump    Shift / dodge\nLeft-click / bow    E / interact    Q / candy    Esc / pause", menuSmall);
            if (Button(new Rect(width - 228, 38, 188, 36), game.audioSystem.Muted ? "SOUND OFF  /  M" : "SOUND ON  /  M", menuChapter))
                game.audioSystem.ToggleMute();
        }
        void OnDestroy()
        {
            if (menuShade != null) Destroy(menuShade);
        }
        void Introduction(DreamGame game)
        {
            Color before = GUI.color;
            GUI.color = new Color(before.r,before.g,before.b,before.a * game.IntroductionOpacity);
            Rect box = new Rect(width / 2 - 420,height / 2 - 190,840,380);
            Panel(box,new Color(.045f,.04f,.075f,.94f * game.IntroductionOpacity));
            Label(new Rect(box.x + 40,box.y + 24,760,26),"Chapter one / The first light",small,gold);
            Label(new Rect(box.x + 40,box.y + 58,760,64),"Find your way home",new GUIStyle(heading) { fontSize = 44 },cream);
            Label(new Rect(box.x + 40,box.y + 134,760,32),"You are trapped in a Halloween nightmare.",text);
            string[] steps = {
                "Light the pumpkin lanterns to reveal hidden paths.",
                "Find the lost memory fragments deeper in the dream.",
                "Follow the light and escape the nightmare."
            };
            for (int i = 0; i < steps.Length; i++)
            {
                Label(new Rect(box.x + 40,box.y + 186 + i * 39,36,30),(i + 1).ToString("00"),small,gold);
                Label(new Rect(box.x + 86,box.y + 184 + i * 39,714,34),steps[i],text);
            }
            Panel(new Rect(box.x + 40,box.y + 320,760,1),new Color(.9f,.75f,.5f,.3f * game.IntroductionOpacity));
            Label(new Rect(box.x + 40,box.y + 337,760,28),"Move with WASD when you're ready",small,teal);
            GUI.color = before;
        }
        void Victory(DreamGame game)
        {
            Rect box = new Rect(width / 2 - 380, height - 254, 760, 226);
            Panel(box, panel);
            Label(new Rect(box.x + 30, box.y + 20, 700, 42), "The nightmare is over", heading, gold);
            Label(new Rect(box.x + 30, box.y + 69, 700, 40), "The Pumpkin King has fallen. Every lantern shines again.", text);
            if (Button(new Rect(box.x + 30, box.y + 128, 420, 55), "Dream again  /  Enter", button)) game.LoadChapter(0);
            if (Button(new Rect(box.x + 470, box.y + 128, 260, 55), "Chapter select", button)) game.ReturnToMenu();
        }
        void PlayHUD(DreamGame game)
        {
            Panel(new Rect(28, 24, 452, 82), panel);
            Label(new Rect(48, 36, 400, 23), "CHAPTER 0" + (game.levelIndex + 1) + "  /  LOST DREAM", small, gold);
            Label(new Rect(48, 59, 410, 43), DreamGame.ChapterNames[game.levelIndex], heading);
            Panel(new Rect(width - 302, 24, 274, 82), panel);
            Label(new Rect(width - 282, 36, 230, 23), "DREAMWALKER", small, teal);
            for (int i = 0; i < game.player.maxHealth; i++)
            {
                Panel(new Rect(width - 279 + i * 45, 68, 31, 17), i < game.player.Health ? new Color(.97f, .4f, .46f) : new Color(.24f, .2f, .3f));
            }
            if (game.boss != null && game.boss.AwakeInArena)
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
            if (game.boss == null) progress += "     ENEMIES  " + (game.dragons.Length - game.DragonsRemaining) + "/" + game.dragons.Length;
            else if (game.boss.AwakeInArena) progress += "     WAVE " + game.boss.SummonWavesStarted + "/2  /  ENEMIES " + game.boss.ActiveSummons;
            Label(new Rect(48, height - 113, 560, 24), progress, small, gold);
            if (game.garden != null)
            {
                Panel(new Rect(28, 118, 452, 62), panel);
                Label(new Rect(48, 124, 410, 24), "DREAM MEMORIES  " + game.garden.MemoriesCollected + "/3", small, teal);
                Label(new Rect(48, 148, 410, 25), game.garden.StageLabel, small, gold);
            }
            if (game.marsh != null)
            {
                Panel(new Rect(28, 118, 452, 62), panel);
                Label(new Rect(48, 124, 410, 24), "WITCHLIGHT BREWS  " + game.marsh.BrewsComplete + "/3", small, teal);
                Label(new Rect(48, 148, 410, 25), game.marsh.StageLabel, small, gold);
            }
            Label(new Rect(48, height - 82, 552, 56), game.Objective(), text);
            Panel(new Rect(width - 360, height - 156, 332, 128), panel);
            Label(new Rect(width - 340, height - 141, 300, 26), game.player.CarryingCandy ? CandyPalette.Label(game.player.HeldCandyHue) + " CANDY  /  Q" : "BOW READY  /  LEFT-CLICK", small, game.player.CarryingCandy ? CandyPalette.Color(game.player.HeldCandyHue) : teal);
            Label(new Rect(width - 340, height - 113, 300, 25), game.player.IsGrounded ? "SPACE  /  JUMP" : "IN THE AIR", small);
            Label(new Rect(width - 340, height - 86, 300, 25), game.player.DashCooldown <= 0 ? "SHIFT  /  DODGE READY" : "DODGE RECHARGING", small);
            Label(new Rect(width - 340, height - 59, 300, 24), "E / interact    Esc / pause", small);
            if (game.Phase == GamePhase.Playing)
            {
                if (game.player.Nearby != null && game.player.Nearby.Available)
                {
                    Panel(new Rect(width / 2 - 310, height - 200, 620, 48), panel);
                    Label(new Rect(width / 2 - 295, height - 194, 590, 36), "[ E ]  " + game.player.Nearby.Prompt, centered, gold);
                }
                if (Time.unscaledTime < game.ToastUntil)
                {
                    float toastY = (game.garden != null || game.marsh != null) ? 192 : game.boss != null && game.boss.AwakeInArena ? 185 : 151;
                    float toastHeight = Mathf.Clamp(centered.CalcHeight(new GUIContent(game.Toast), 868) + 12, 50, 92);
                    Panel(new Rect(width / 2 - 450, toastY, 900, toastHeight), panel);
                    Label(new Rect(width / 2 - 434, toastY + 6, 868, toastHeight - 12), game.Toast, centered);
                }
                Reticle(game);
                TargetLabels(game);
                if (game.Tutorial != null) TutorialPrompt(game.Tutorial);
            }
        }
        void TutorialPrompt(TutorialGuide guide)
        {
            Rect box = new Rect(width / 2 - 215, 24, 430, 124);
            Panel(box, panel);
            Label(new Rect(box.x + 16, box.y + 8, 398, 23), "Follow the golden trail  /  " + guide.StepNumber + " of 8", small, gold);
            Label(new Rect(box.x + 16, box.y + 35, 398, 30), guide.Title,
                new GUIStyle(small) { font = DreamTypography.Strong, fontSize = 22 }, cream);
            Label(new Rect(box.x + 16, box.y + 70, 398, 48), guide.Instruction,
                new GUIStyle(small) { fontSize = 17 }, cream);
        }
        void Reticle(DreamGame game)
        {
            Vector2 aim = game.player.AimScreenPoint;
            Vector2 p = new Vector2(aim.x / scale, (Screen.height - aim.y) / scale);
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
                Panel(new Rect(x - 24, y, 48, 5), panel);
                Panel(new Rect(x - 24, y, 48f * dragon.health / dragon.MaxHealth, 5), teal);
                if (game.levelIndex != 2)
                {
                    var nameStyle = new GUIStyle(small) { fontSize = 17, alignment = TextAnchor.MiddleCenter };
                    Label(new Rect(x - 120, y - 28, 240, 24), dragon.DisplayName, nameStyle, cream);
                    if (dragon.Phase == DragonPhase.Telegraph)
                        Label(new Rect(x - 160, y + 7, 320, 24), dragon.AttackHint, nameStyle, gold);
                }
            }
        }
        void Modal(DreamGame game, string caption, string description, string action, System.Action callback)
        {
            Panel(new Rect(0, 0, width, height), new Color(.025f, .02f, .06f, .7f));
            Rect box = new Rect(width / 2 - 370, height / 2 - 165, 740, 330);
            Panel(box, panel);
            Label(new Rect(box.x + 35, box.y + 35, 670, 58), caption, new GUIStyle(heading) { fontSize = 43, alignment = TextAnchor.MiddleCenter }, gold);
            Label(new Rect(box.x + 45, box.y + 105, 650, 65), description, centered);
            if (Button(new Rect(box.x + 95, box.y + 193, 550, 56), action, button)) callback();
            if (Button(new Rect(box.x + 245, box.y + 266, 250, 37), "Chapter select", compactButton))
                game.ReturnToMenu();
        }
    }
}
