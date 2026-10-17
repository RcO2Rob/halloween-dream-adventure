using System.Collections;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator AutomaticTransitionChecks(int nextChapter)
        {
            Require(DreamTransition.Running, "Walking through the gate starts the fade without any click or Enter action");
            bool black = false, loadedUnderBlack = false;
            float deadline = Time.realtimeSinceStartup + 12;
            while (DreamTransition.Running && Time.realtimeSinceStartup < deadline)
            {
                if (DreamTransition.BlackOpacity >= .999f)
                {
                    if (!black) { black = true; yield return Capture("transition-to-0" + (nextChapter + 1) + "-black"); }
                    if (DreamGame.Instance != null && DreamGame.Instance.levelIndex == nextChapter)
                    {
                        loadedUnderBlack = true;
                        Require(DreamGame.Instance.Phase == GamePhase.LevelComplete && Time.timeScale == 0,
                            "The next chapter loads behind black with gameplay frozen");
                    }
                }
                yield return null;
            }
            Require(black && loadedUnderBlack && !DreamTransition.Running && DreamGame.Instance.levelIndex == nextChapter &&
                DreamGame.Instance.Phase == GamePhase.Playing && Time.timeScale == 1,
                "Black fade automatically loads and resumes chapter " + (nextChapter + 1));
            Require(Cursor.lockState == CursorLockMode.Locked && !Cursor.visible,
                "Automatic fade restores mouse look and hides the cursor");
            yield return new WaitForSecondsRealtime(.15f);
        }
        IEnumerator MarshAdventureChecks(DreamGame game)
        {
            Time.timeScale = 1;
            var marsh = game.marsh; var player = game.player; player.automation = true;
            Require(game.levelIndex == 2 && game.boss == null && marsh != null, "Chapter three is a separate adventure before the Boss");
            Require(game.GetComponentsInChildren<BoxCollider>().Count(x => x.name == "Solid dream island") == 6 &&
                game.dragons.Length == 12 && game.lanterns.Length == 5 && marsh.cauldrons.Length == 3,
                "Marsh has six distinct islands, twelve guardians, five crossings and three potions");
            Require(!game.ObjectivesComplete && !game.TryExit(), "Marsh exit rejects incomplete potion and guardian objectives");
            HalloweenArtChecks(game);
            foreach (var enemy in game.dragons) enemy.enabled = false;
            yield return MarshOverview(game);
            player.Teleport(marsh.lights[1].safeCheckpoint); yield return new WaitForSeconds(.15f);
            Require(!marsh.lights[1].Use(player) && !marsh.lights[1].path.Revealed,
                "Grove lantern cannot bypass its unbrewed potion");
            yield return MarshLightAndCross(game, marsh.lights[0]);
            var first = marsh.cauldrons[0];
            player.Teleport(first.transform.position + new Vector3(0,.12f,-6)); yield return new WaitForSeconds(.5f);
            Require(player.FireAt(first.AimPosition), "A real arrow launches at the first potion pot"); yield return new WaitForSeconds(.6f);
            Require(first.Ingredients == 0, "Bow arrows cannot substitute for potion ingredients");
            var pink = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).First(x => x.hue == CandyHue.Pink && Vector3.Distance(x.transform.position,first.supplyPoints[0]) < 1);
            player.Teleport(pink.transform.position + Vector3.back * .3f); yield return new WaitForSeconds(.2f);
            Require(player.Interact() && player.HeldCandyHue == CandyHue.Pink, "Recipe swap regression picks up a real pink candy");
            yield return ThrowMarshIngredient(game, first, CandyHue.Blue);
            Require(first.Ingredients == 0, "A wrong-color ballistic throw preserves recipe progress");
            yield return new WaitForSeconds(3.2f);
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Any(x => x.hue == CandyHue.Blue &&
                Vector3.Distance(x.transform.position,first.supplyPoints[1]) < 1),
                "A blue supply slot recovers its color after a swap leaves pink candy occupying it");
            foreach (var sweet in FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Where(x => Vector3.Distance(x.transform.position, first.transform.position) < 9)) Destroy(sweet.gameObject);
            game.Pause(); yield return new WaitForSecondsRealtime(.5f);
            Require(!FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Any(x => Vector3.Distance(x.transform.position,first.transform.position) < 9),
                "Pause freezes marsh candy replenishment");
            game.Resume(); yield return new WaitForSeconds(3.2f);
            Require(Enumerable.Range(0,3).All(i => FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Any(x =>
                x.hue == (CandyHue)i && Vector3.Distance(x.transform.position,first.supplyPoints[i]) < 1)),
                "All three colors return after discarded teaching throws, preventing a potion softlock");
            for (int stage = 0; stage < 3; stage++)
            {
                var pot = marsh.cauldrons[stage];
                if (stage == 1) yield return MarshMistChecks(game, pot);
                for (int i = 0; i < pot.recipe.Length; i++)
                {
                    yield return ThrowMarshIngredient(game, pot, pot.recipe[i]);
                    if (stage == 0 && i == 0)
                    {
                        game.player.Heal(5); int beforeFall = game.player.Health;
                        game.player.Teleport(new Vector3(0,-9,16)); yield return new WaitForSeconds(.2f);
                        Require(game.player.Health == beforeFall - 1 && pot.Ingredients == 1 &&
                            Vector3.Distance(game.player.transform.position,game.Checkpoint) < .35f,
                            "A missed marsh jump preserves recipe progress and restores the lantern checkpoint");
                        game.player.Heal(5);
                    }
                    Require(pot.Ingredients == i + 1, "Real candy projectile adds ingredient " + (i + 1) + " to " + pot.brewName);
                }
                Require(pot.Complete && marsh.BrewsComplete == stage + 1, "Potion completes its authored recipe: " + pot.brewName);
                Require(!marsh.lights[stage + 1].Use(player), "A brewed potion still requires its living guardians to be cleared");
                if (stage > 0)
                {
                    yield return null;
                    Require(game.GetComponentsInChildren<MarshMist>().Where(x => x.source == pot).All(x => !x.Active && !x.Warning && !x.plume.gameObject.activeSelf),
                        "Completed potion permanently dispels its dangerous mist vents");
                }
                foreach (var enemy in pot.guardians)
                {
                    enemy.enabled = true;
                    yield return DefeatGardenEnemy(game, enemy, false);
                }
                Require(marsh.CanLight(stage + 1), "Potion and guardians unlock the next marsh lantern");
                player.Teleport(pot.transform.position + new Vector3(0,.12f,-5));
                game.gameCamera.GetComponent<DreamCamera>().ResetView(); game.gameCamera.GetComponent<DreamCamera>().Snap();
                yield return Capture("03-brew-" + (stage + 1));
                yield return MarshLightAndCross(game, marsh.lights[stage + 1]);
            }
            Require(!marsh.lights[4].Use(player), "Watchtower guardians guard the final restored route");
            foreach (var enemy in marsh.towerGuards) { enemy.enabled = true; yield return DefeatGardenEnemy(game, enemy, false); }
            yield return MarshLightAndCross(game, marsh.lights[4]);
            Require(game.ObjectivesComplete && game.DragonsRemaining == 0 && marsh.BrewsComplete == 3,
                "Marsh opens only after all seven ingredients, twelve guardians and five lanterns");
            yield return new WaitForSeconds(2);
            Require(game.GetComponentInChildren<GardenIronGate>().IsOpen, "Marsh uses the opening iron gate and glowing exit trail");
            yield return Capture("03-restored-castle-road");
        }
        IEnumerator ThrowMarshIngredient(DreamGame game, WitchCauldron pot, CandyHue hue)
        {
            var sweet = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).FirstOrDefault(x => x.hue == hue &&
                Vector3.Distance(x.transform.position, pot.supplyPoints[(int)hue]) < 1);
            if (sweet == null)
            {
                yield return new WaitForSeconds(3.2f);
                sweet = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).FirstOrDefault(x => x.hue == hue &&
                    Vector3.Distance(x.transform.position,pot.supplyPoints[(int)hue]) < 1);
            }
            Require(sweet != null, "Renewable " + hue + " supply exists beside " + pot.brewName);
            var player = game.player;
            player.Teleport(sweet.transform.position + Vector3.back * .3f); yield return new WaitForSeconds(.2f);
            if (!player.CarryingCandy || player.HeldCandyHue != hue)
                Require(player.Interact() && player.CarryingCandy && player.HeldCandyHue == hue, "Real E interaction selects " + hue + " potion candy");
            player.Teleport(pot.transform.position + new Vector3(0,.12f,-6)); yield return new WaitForSeconds(.6f);
            Require(player.ThrowAt(pot.AimPosition), "A ballistic " + hue + " candy throw targets " + pot.brewName);
            yield return new WaitForSeconds(.9f);
            Require(!player.CarryingCandy, "Potion throw consumes its held candy");
        }
        IEnumerator MarshLightAndCross(DreamGame game, Lantern lamp)
        {
            var player = game.player; player.Heal(5);
            player.Teleport(lamp.safeCheckpoint); yield return new WaitForSeconds(.2f);
            Require(player.Interact() && lamp.Lit && lamp.path.Revealed, "Real E lights " + lamp.lanternName + " and restores its crossing");
            yield return null;
            Require(Vector3.Distance(game.Checkpoint,lamp.safeCheckpoint) < .1f, "Marsh lantern saves its safe island checkpoint");
            var follow = game.gameCamera.GetComponent<DreamCamera>();
            follow.yaw = Vector3.SignedAngle(Vector3.forward, lamp.path.transform.forward, Vector3.up); follow.Snap();
            yield return JumpRouteChecks(game,lamp.path);
            var drift = lamp.path.GetComponentInChildren<DriftingPlatform>();
            if (drift != null)
            {
                player.Teleport(drift.transform.position + Vector3.up * .04f); yield return new WaitForSeconds(.2f);
                Vector3 local = player.transform.position - drift.transform.position;
                Vector3 before = drift.transform.position;
                float travelled = 0, maxSlip = 0, until = Time.time + 1.2f;
                while (Time.time < until)
                {
                    yield return null;
                    travelled += Vector3.Distance(before,drift.transform.position);
                    before = drift.transform.position;
                    maxSlip = Mathf.Max(maxSlip,Vector3.ProjectOnPlane(player.transform.position - drift.transform.position - local,Vector3.up).magnitude);
                }
                // Net displacement can be zero at a turnaround despite continuous travel.
                Require(travelled > .05f && maxSlip < .12f,
                    "Standing character rides the drifting platform without sliding off (travel=" + travelled.ToString("F3") + ", slip=" + maxSlip.ToString("F3") + ")");
                game.Pause(); before = drift.transform.position; yield return new WaitForSecondsRealtime(.3f);
                Require(drift.transform.position == before, "Pause freezes the drifting dreamstone"); game.Resume();
                player.Teleport(lamp.path.jumpExit.position); yield return new WaitForSeconds(.2f);
            }
        }
        IEnumerator MarshMistChecks(DreamGame game, WitchCauldron pot)
        {
            var mist = game.GetComponentsInChildren<MarshMist>().First(x => x.source == pot);
            game.player.Teleport(pot.transform.position + new Vector3(0,.12f,-5));
            float deadline = Time.time + 9;
            while (!mist.Warning && Time.time < deadline) yield return null;
            Require(mist.Warning && !mist.Active && mist.warning.gameObject.activeSelf, "Mist vents show a gold warning before dealing damage");
            game.Pause(); yield return new WaitForSecondsRealtime(.4f);
            Require(mist.Warning && !mist.Active, "Pause freezes the mist warning interval"); game.Resume();
            game.player.Heal(5); game.player.Teleport(mist.transform.position + Vector3.up * .1f);
            int health = game.player.Health;
            deadline = Time.time + 2;
            while (game.player.Health == health && Time.time < deadline) yield return null;
            Require(mist.Active && game.player.Health < health, "A player who remains inside an active marked mist vent takes real damage");
            game.player.Teleport(pot.transform.position + new Vector3(0,.12f,-5)); game.player.Heal(5);
            yield return Capture("03-mist-warning-and-workshop");
        }
        IEnumerator MarshOverview(DreamGame game)
        {
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>();
            follow.enabled = false; Vector3 before = camera.transform.position; Quaternion rotation = camera.transform.rotation;
            bool fog = RenderSettings.fog; RenderSettings.fog = false;
            camera.transform.position = new Vector3(68,125,-67); camera.transform.LookAt(new Vector3(-14,0,52));
            yield return Capture("03-witchlight-marsh-overview");
            RenderSettings.fog = fog;
            camera.transform.SetPositionAndRotation(before,rotation); follow.enabled = true; follow.Snap();
        }
    }
}
