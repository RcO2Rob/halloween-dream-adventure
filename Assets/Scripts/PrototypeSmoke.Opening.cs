using System.Collections;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator OpeningReview()
        {
            var game = DreamGame.Instance; var player = game.player;
            player.automation = true;
            Require(game.IntroductionVisible && game.Phase == GamePhase.Playing && Time.timeScale == 1,
                "Starting the tutorial shows the story card without adding a confirmation phase");
            Vector3 spawn = player.transform.position;
            foreach (var size in new[] { new Vector2Int(1440,900),new Vector2Int(1920,1080),new Vector2Int(1280,960),new Vector2Int(1280,720) })
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.4f);
                yield return Capture("opening-" + size.x + "x" + size.y);
                Require(game.IntroductionVisible && Vector3.Distance(player.transform.position,spawn) < .15f,
                    "Opening remains readable over a safe, idle tutorial spawn at " + size);
            }
            float remaining = game.IntroductionSecondsRemaining;
            game.Pause(); yield return new WaitForSecondsRealtime(.4f);
            Require(game.IntroductionVisible && game.IntroductionSecondsRemaining == remaining,
                "Pause preserves the opening card's remaining reading time");
            game.Resume(); player.automationMove = Vector3.forward; yield return new WaitForSeconds(.3f);
            player.automationMove = Vector3.zero;
            Require(!game.IntroductionVisible && Vector3.Distance(player.transform.position,spawn) > .5f &&
                game.Phase == GamePhase.Playing && Cursor.lockState == CursorLockMode.Locked,
                "Real controller movement dismisses the card and continues mouse-look gameplay without clicking");
            yield return Capture("opening-dismissed-by-walking");
            game.Retry(); yield return new WaitForSecondsRealtime(.7f);
            Require(DreamGame.Instance.levelIndex == 0 && !DreamGame.Instance.IntroductionVisible,
                "Retrying the tutorial does not repeat the opening explanation");
            DreamGame.Instance.ReturnToMenu(); yield return new WaitForSecondsRealtime(.7f);
            Require(!DreamGame.Instance.IntroductionVisible && DreamGame.Instance.Phase == GamePhase.Menu,
                "The cover menu does not show the tutorial story card");
            DreamGame.Instance.StartPlaying(); yield return new WaitForSeconds(.4f);
            Require(DreamGame.Instance.IntroductionVisible,"Starting a fresh journey shows the explanation again");
            Time.timeScale = 4;
            yield return new WaitForSeconds(DreamGame.Instance.IntroductionSecondsRemaining + .6f);
            Time.timeScale = 1;
            Require(!DreamGame.Instance.IntroductionVisible && DreamGame.Instance.Phase == GamePhase.Playing,
                "The explanation fades away automatically after its reading interval without any input");
            DreamGame.Instance.LoadChapter(0); yield return new WaitForSecondsRealtime(.7f);
            Require(DreamGame.Instance.IntroductionVisible,"Selecting chapter one directly also shows its opening explanation");
            for (int chapter = 1; chapter < DreamGame.SceneNames.Length; chapter++)
            {
                DreamGame.Instance.LoadChapter(chapter); yield return new WaitForSecondsRealtime(.7f);
                Require(!DreamGame.Instance.IntroductionVisible && DreamGame.Instance.Phase == GamePhase.Playing,
                    "Later chapter starts have no tutorial opening card: " + (chapter + 1));
                DreamGame.Instance.Retry(); yield return new WaitForSecondsRealtime(.7f);
                Require(DreamGame.Instance.levelIndex == chapter && !DreamGame.Instance.IntroductionVisible,
                    "Retry preserves the selected later chapter: " + (chapter + 1));
            }
            Require(DreamTypography.Overflow.Count == 0,"Story card, cover and HUD lettering fit their rectangles: " + string.Join(" | ",DreamTypography.Overflow));
            Save(true,""); Debug.Log("LOST_DREAM_OPENING_SMOKE_PASSED: " + checks.Count);
            Application.Quit(0);
        }
    }
}
