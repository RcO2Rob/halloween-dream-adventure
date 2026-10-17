using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator TypographyReview()
        {
            DreamTypography.AuditEnabled = true;
            Require(DreamTypography.Display != null && DreamTypography.Body != null && DreamTypography.Strong != null,
                "All three embedded font faces load in the standalone player without installed system fonts");
            foreach (var font in new[] { DreamTypography.Display, DreamTypography.Body, DreamTypography.Strong })
                Require("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789[]<>/.,'!?+-".All(font.HasCharacter),
                    "Typeface covers controls, candy symbols, Roman numerals and punctuation: " + font.name);
            for (int chapter = 0; chapter < DreamGame.SceneNames.Length; chapter++)
            {
                DreamGame.Instance.LoadChapter(chapter); yield return new WaitForSecondsRealtime(.7f);
                var game = DreamGame.Instance; game.DismissIntroduction(); game.player.automation = true;
                foreach (var enemy in game.dragons) enemy.enabled = false;
                var words = game.GetComponentsInChildren<TextMesh>(true);
                Require(words.Length > 0 && words.All(x => x.font == DreamTypography.Body || x.font == DreamTypography.Strong),
                    "Saved signs, shrines and recipes use the shared typefaces in chapter " + (chapter + 1));
                Require(words.All(x => x.GetComponent<Renderer>().sharedMaterial == x.font.material),
                    "World lettering uses its own font atlas, preventing missing glyph rectangles in chapter " + (chapter + 1));
                var sign = words.First(x => x.name == "Instructions");
                game.player.Teleport(sign.transform.position + new Vector3(0,-1.58f,-4.5f));
                var follow = game.gameCamera.GetComponent<DreamCamera>(); follow.ResetView(); follow.Snap();
                game.ShowToast("Follow the lanterns. A missed jump returns you to your last safe light.",4);
                yield return Capture("0" + (chapter + 1) + "-lettering-and-hud");
                game.Pause(); yield return Capture("0" + (chapter + 1) + "-pause"); game.Resume();
                if (game.marsh != null)
                {
                    var pot = game.marsh.cauldrons[2];
                    game.player.Teleport(pot.transform.position + new Vector3(0,.12f,-6));
                    follow.ResetView(); follow.Snap();
                    game.ShowToast("The pot needs Blue candy next. Your recipe progress is safe.",4);
                    yield return Capture("03-potion-recipe");
                }
                if (game.boss != null)
                {
                    game.lanterns[0].Use(game.player); game.player.Teleport(new Vector3(0,.12f,6));
                    yield return new WaitForSeconds(.4f); follow.ResetView(); follow.Snap();
                    game.player.PickCandy(CandyHue.Gold);
                    game.ShowToast("Match his color. Keep moving, then throw when the shell opens.",4);
                    yield return Capture("04-boss-type-hierarchy");
                    game.Defeat(); yield return Capture("04-retry"); game.Retry();
                    yield return new WaitForSecondsRealtime(.7f);
                    game = DreamGame.Instance; game.player.automation = true;
                    game.Win(); yield return new WaitForSecondsRealtime(1);
                    yield return Capture("04-victory-typography");
                }
            }
            // Repeat the densest HUD at each supported aspect ratio, not only the cover menu.
            DreamGame.Instance.LoadChapter(2); yield return new WaitForSecondsRealtime(.7f);
            var marshGame = DreamGame.Instance; marshGame.player.automation = true;
            foreach (var enemy in marshGame.dragons) enemy.enabled = false;
            foreach (var size in new[] { new Vector2Int(1440,900), new Vector2Int(1920,1080), new Vector2Int(1280,960), new Vector2Int(1280,720) })
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.4f);
                marshGame.ShowToast("Dawnbloom brewed! The mist clears. Defeat the guardians, then light the crossing lantern.",4);
                yield return Capture("03-hud-" + size.x + "x" + size.y);
            }
            Require(DreamTypography.Overflow.Count == 0,"Menus, HUD, prompts and modals fit their actual font metrics: " + string.Join(" | ",DreamTypography.Overflow));
            DreamGame.Instance.ReturnToMenu(); yield return new WaitForSecondsRealtime(.7f);
            Require(DreamGame.Instance.Phase == GamePhase.Menu,"Typography review returns safely to the four-chapter menu");
            Save(true,""); Debug.Log("LOST_DREAM_TYPOGRAPHY_SMOKE_PASSED: " + checks.Count);
            Application.Quit(0);
        }
    }
}
