using System.Collections;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator TutorialGuideReview()
        {
            var game = DreamGame.Instance; var player = game.player;
            player.automation = true;
            foreach (var enemy in game.dragons) enemy.enabled = false;
            DreamTypography.AuditEnabled = true;
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial != null && game.Tutorial.Step == TutorialStep.FirstLantern,
                "Tutorial starts by guiding the player to the first lantern");
            yield return GuideGroundChecks(game);
            foreach (var size in new[] { new Vector2Int(1440,900), new Vector2Int(1280,720), new Vector2Int(1280,960) })
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.3f);
                yield return Capture("tutorial-guide-01-lantern-" + size.x + "x" + size.y);
            }
            Screen.SetResolution(1440,900,FullScreenMode.Windowed);
            player.Teleport(game.lanterns[0].safeCheckpoint);
            Require(game.lanterns[0].Use(player), "Lighting the real lantern reveals the first crossing");
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial.Step == TutorialStep.Bow, "Lantern completion switches guidance to bow practice");
            var bow = game.runes.First(x => !x.candyOnly);
            player.Teleport(bow.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            yield return new WaitForSeconds(.6f);
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("tutorial-guide-02-bow");
            Require(player.FireAt(bow.AimPosition), "The player launches a real practice arrow");
            yield return new WaitForSeconds(.8f);
            Require(bow.Cleared && game.Tutorial.Step == TutorialStep.Jump,
                "Hitting the practice crystal advances to the jump lesson");
            var jump = game.lanterns[0].path;
            player.Teleport(jump.jumpEntry.position + Vector3.back + Vector3.up * .12f);
            yield return new WaitForSeconds(.6f);
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return GuideGroundChecks(game);
            yield return Capture("tutorial-guide-03-jump");
            player.Fall(); player.Heal(5);
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial.Step == TutorialStep.Jump && bow.Cleared,
                "A fall keeps completed lessons and points back to the crossing");
            player.Teleport(jump.jumpExit.position + Vector3.forward * 2 + Vector3.up * .12f);
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial.Step == TutorialStep.Monster, "Crossing the platforms introduces the monster and bow controls");
            yield return Capture("tutorial-guide-04-monster");
            var dragon = game.dragons[0];
            player.Teleport(new Vector3(dragon.home.x,.12f,dragon.home.z - 4));
            for (int i = 0; i < 8 && !dragon.Defeated; i++)
            { player.FireAt(dragon.AimPosition); yield return new WaitForSeconds(.55f); }
            yield return new WaitForSeconds(.5f);
            Require(dragon.Defeated && game.Tutorial.Step == TutorialStep.SecondLantern,
                "Real bow hits defeat the monster and reveal the next lighting instruction");
            player.Teleport(game.lanterns[1].safeCheckpoint);
            yield return new WaitForSeconds(.5f);
            yield return Capture("tutorial-guide-05-second-lantern");
            Require(game.lanterns[1].Use(player), "Second lantern restores the road to the candy island");
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial.Step == TutorialStep.Candy, "Guidance asks for candy after the second light");
            var seal = game.runes.First(x => x.candyOnly);
            var candy = game.GetComponentsInChildren<CandyPickup>().OrderBy(x => Vector3.Distance(x.transform.position,seal.transform.position)).First();
            player.Teleport(candy.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            yield return new WaitForSeconds(.6f);
            yield return GuideGroundChecks(game);
            yield return Capture("tutorial-guide-06-candy");
            Require(candy.Use(player), "The player picks up a real candy");
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial.Step == TutorialStep.Seal, "Holding candy switches the hint to throwing at the seal");
            Require(player.ThrowAt(player.transform.position + Vector3.right * 5), "The player can miss a teaching throw");
            yield return new WaitForSeconds(.7f);
            Require(!seal.Cleared && game.Tutorial.Step == TutorialStep.Candy,
                "A missed throw returns the guide to candy pickup instead of leaving a stale throwing instruction");
            yield return new WaitForSeconds(3.2f);
            candy = game.GetComponentsInChildren<CandyPickup>().OrderBy(x => Vector3.Distance(x.transform.position,seal.transform.position)).First();
            Require(candy.Use(player), "A replenished candy remains usable after a missed throw");
            player.Teleport(seal.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            yield return new WaitForSeconds(.6f);
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("tutorial-guide-07-seal");
            Require(player.ThrowAt(seal.AimPosition), "A real candy projectile launches at the seal");
            yield return new WaitForSeconds(1);
            Require(seal.Cleared && game.ObjectivesComplete && game.Tutorial.Step == TutorialStep.Exit,
                "Finishing the real tutorial objectives directs the trail to the open exit");
            yield return new WaitForSeconds(2);
            yield return GuideGroundChecks(game);
            yield return Capture("tutorial-guide-08-exit");
            Require(DreamTypography.Overflow.Count == 0, "Tutorial instructions fit supported screen sizes: " + string.Join(" | ",DreamTypography.Overflow));
            game.Retry(); yield return new WaitForSecondsRealtime(.7f);
            game = DreamGame.Instance; game.Resume(); game.DismissIntroduction();
            yield return new WaitForSeconds(.6f);
            Require(game.Tutorial != null && game.Tutorial.Step == TutorialStep.FirstLantern,
                "Restarting resets guidance without leaving old trail objects");
            game.LoadChapter(2); yield return new WaitForSecondsRealtime(.7f);
            Require(DreamGame.Instance.Tutorial == null && FindFirstObjectByType<TutorialGuide>() == null,
                "Ground teaching guidance is restricted to chapter one");
            Save(true, ""); Debug.Log("LOST_DREAM_TUTORIAL_GUIDE_SMOKE_PASSED: " + checks.Count); Application.Quit(0);
        }
        IEnumerator GuideGroundChecks(DreamGame game)
        {
            var navigation = new TutorialGroundRoute(game);
            var arrows = game.Tutorial.GetComponentsInChildren<MeshRenderer>().Where(x => x.name == "Golden tutorial arrow").ToArray();
            Require(arrows.Length > 2, "A visible golden trail leads from the current teaching position");
            foreach (var arrow in arrows)
            {
                Require(navigation.Ground(arrow.transform.position,out Vector3 floor) &&
                    Mathf.Abs(arrow.transform.position.y - floor.y - .1f) < .02f && navigation.Clearance(floor),
                    "Teaching arrow is grounded and outside scenery: " + arrow.transform.position);
            }
            Require(game.transform.Find("Tutorial golden ground trail").GetComponentsInChildren<Collider>(true).Length == 0,
                "Guide graphics add no collision or aiming targets");
            yield break;
        }
    }
}
