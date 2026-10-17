using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    // Opt-in integration test. Never runs during ordinary play.
    public partial class PrototypeSmoke : MonoBehaviour
    {
        [Serializable] public class Report
        {
            public bool passed;
            public string unityVersion;
            public string[] checks;
            public string error;
        }
        readonly List<string> checks = new List<string>();
        bool failed;
        string outputDirectory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Environment.GetCommandLineArgs().Contains("--smoke-test")) return;
            var runner = new GameObject("Opt-in prototype integration test").AddComponent<PrototypeSmoke>();
            DontDestroyOnLoad(runner.gameObject);
        }
        void Start()
        {
            UnityEngine.Random.InitState(20261012);
            outputDirectory = "/tmp/lost-dream-smoke";
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--test-output");
            if (index >= 0 && index + 1 < args.Length) outputDirectory = args[index + 1];
            Directory.CreateDirectory(outputDirectory);
            StartCoroutine(Guard(Run()));
        }
        IEnumerator Guard(IEnumerator routine)
        {
            while (!failed)
            {
                object current;
                try
                {
                    if (!routine.MoveNext()) yield break;
                    current = routine.Current;
                }
                catch (Exception error)
                {
                    failed = true;
                    Save(false, error.ToString());
                    Debug.LogError("LOST_DREAM_SMOKE_FAILED: " + error);
                    Application.Quit(2);
                    yield break;
                }
                if (current is IEnumerator child) yield return Guard(child);
                else yield return current;
            }
        }
        void Require(bool condition, string name)
        {
            if (!condition) throw new Exception(name);
            checks.Add(name);
            Debug.Log("SMOKE_CHECK_OK: " + name);
        }
        IEnumerator Capture(string name)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, name + ".png"));
            yield return new WaitForSecondsRealtime(.25f);
        }
        IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(1);
            Require(DreamGame.Instance != null && DreamGame.Instance.levelIndex == 0, "Tutorial loads in standalone player");
            if (Environment.GetCommandLineArgs().Contains("--typography-smoke") || Environment.GetCommandLineArgs().Contains("--opening-smoke")) DreamTypography.AuditEnabled = true;
            yield return TitleScreenChecks();
            yield return Capture("01-title");
            DreamGame.Instance.StartPlaying();
            yield return null;
            Require(DreamGame.Instance.Phase == GamePhase.Playing, "Start action leaves the cover menu and enters the tutorial");
            if (Environment.GetCommandLineArgs().Contains("--opening-smoke"))
            { yield return OpeningReview(); yield break; }
            DreamGame.Instance.DismissIntroduction();
            if (Environment.GetCommandLineArgs().Contains("--tutorial-guide-smoke"))
            { yield return TutorialGuideReview(); yield break; }
            if (Environment.GetCommandLineArgs().Contains("--scenery-collision-smoke") || Environment.GetCommandLineArgs().Contains("--scenery-contact-smoke"))
            { yield return SceneryCollisionReview(); yield break; }
            if (Environment.GetCommandLineArgs().Contains("--music-smoke"))
            { yield return SoundtrackReview(); yield break; }
            if (Environment.GetCommandLineArgs().Contains("--typography-smoke"))
            { yield return TypographyReview(); yield break; }
            if (Environment.GetCommandLineArgs().Contains("--marsh-smoke"))
            {
                DreamGame.Instance.LoadChapter(2); yield return new WaitForSecondsRealtime(.7f);
                yield return MarshAdventureChecks(DreamGame.Instance);
                yield return WalkThroughGardenGate(DreamGame.Instance);
                yield return AutomaticTransitionChecks(3);
                Require(DreamGame.Instance.boss != null && DreamGame.Instance.levelIndex == 3, "The fourth chapter is the preserved castle Boss arena");
                DreamGame.Instance.LoadChapter(2); yield return new WaitForSecondsRealtime(.7f);
                Require(DreamGame.Instance.marsh.BrewsComplete == 0 && DreamGame.Instance.LitCount == 0 && DreamGame.Instance.DragonsRemaining == 12,
                    "Marsh revisit resets every potion, lantern, guardian and route");
                Save(true, ""); Debug.Log("LOST_DREAM_MARSH_SMOKE_PASSED: " + checks.Count); Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--garden-smoke") || Environment.GetCommandLineArgs().Contains("--exit-enclosures-smoke"))
            {
                if (Environment.GetCommandLineArgs().Contains("--exit-enclosures-smoke"))
                {
                    DreamGame.Instance.player.automation = true;
                    yield return TutorialTreeChecks(DreamGame.Instance);
                    yield return ClosedGardenGateChecks(DreamGame.Instance);
                }
                DreamGame.Instance.LoadChapter(1); yield return new WaitForSecondsRealtime(.7f);
                DreamGame.Instance.player.automation = true;
                yield return GardenAdventureChecks(DreamGame.Instance);
                var finishedGarden = DreamGame.Instance;
                yield return WalkThroughGardenGate(finishedGarden);
                Require(finishedGarden.Phase == GamePhase.LevelComplete, "Returning from the left Crypt completes the original physical gate");
                yield return AutomaticTransitionChecks(2);
                DreamGame.Instance.LoadChapter(1); yield return new WaitForSecondsRealtime(.7f);
                var retry = DreamGame.Instance;
                Require(retry.garden.MemoriesCollected == 0 && retry.garden.SequenceProgress == 0 &&
                    retry.LitCount == 0 && retry.DragonsRemaining == 11 && retry.RunesRemaining == 4,
                    "Garden retry restores all memories, puzzle locks, enemies and hidden routes");
                Save(true, ""); Debug.Log("LOST_DREAM_GARDEN_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--aim-alignment-smoke"))
            {
                yield return AimAlignmentChecks();
                Save(true, ""); Debug.Log("LOST_DREAM_AIM_ALIGNMENT_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--ending-smoke"))
            {
                DreamGame.Instance.LoadChapter(DreamGame.BossChapterIndex); yield return new WaitForSecondsRealtime(.6f);
                var ending = DreamGame.Instance; ending.player.automation = true;
                yield return ExpandedCourtyardChecks(ending);
                ending.player.Teleport(new Vector3(0, .15f, 12)); yield return new WaitForSeconds(.15f);
                ending.player.characterAnimation.Move(6);
                ending.player.PickCandy(CandyHue.Blue); ending.player.ThrowAt(ending.boss.AimPosition);
                ending.Win();
                yield return PumpkinBurstChecks();
                yield return VictoryMusicChecks();
                yield return VictoryFireworksChecks();
                ending.Retry(); yield return new WaitForSecondsRealtime(.6f);
                Require(FindObjectsByType<PumpkinBurst>(FindObjectsSortMode.None).Length == 0 &&
                    FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 0 &&
                    DreamGame.Instance.boss.pumpkin.gameObject.activeSelf,
                    "Retry removes celebration fragments and restores the intact boss");
                Save(true, ""); Debug.Log("LOST_DREAM_ENDING_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--mouse-look-smoke"))
            {
                yield return CameraChecks();
                yield return MouseLookChecks();
                DreamGame.Instance.LoadChapter(DreamGame.BossChapterIndex);
                yield return new WaitForSecondsRealtime(.6f);
                var castle = DreamGame.Instance;
                castle.player.automation = true;
                castle.player.Teleport(new Vector3(0, .12f, 1));
                var follow = castle.gameCamera.GetComponent<DreamCamera>();
                follow.ResetView(); follow.ApplyLookInput(new Vector2(30, 14)); follow.Snap();
                float yaw = follow.yaw, pitch = follow.pitch;
                yield return new WaitForSecondsRealtime(4.5f);
                Require(Mathf.Abs(Mathf.DeltaAngle(follow.yaw, yaw)) < .01f && Mathf.Abs(follow.pitch - pitch) < .01f,
                    "Boss framing never overrides mouse yaw or upward pitch after four idle seconds");
                yield return Capture("00-castle-mouse-look");
                castle.Win(); yield return null;
                Require(Cursor.lockState == CursorLockMode.None && Cursor.visible,
                    "Victory releases the mouse for restart and chapter buttons");
                Save(true, ""); Debug.Log("LOST_DREAM_MOUSE_LOOK_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--title-only"))
            {
                yield return Capture("01-title-to-game");
                Save(true, "");
                Debug.Log("LOST_DREAM_TITLE_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0);
                yield break;
            }
            if (!Environment.GetCommandLineArgs().Contains("--journey-smoke")) yield return KayKitCandyChecks();
            if (Environment.GetCommandLineArgs().Contains("--candy-model-smoke"))
            {
                Save(true, ""); Debug.Log("LOST_DREAM_CANDY_MODEL_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--jump-pose-smoke"))
            {
                yield return JumpChecks();
                yield return JumpPoseReview();
                yield return CharacterChecks();
                Save(true, ""); Debug.Log("LOST_DREAM_JUMP_POSE_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            if (Environment.GetCommandLineArgs().Contains("--fireworks-smoke"))
            {
                DreamGame.Instance.LoadChapter(DreamGame.BossChapterIndex);
                yield return new WaitForSecondsRealtime(.6f);
                var game = DreamGame.Instance;
                Require(FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 0,
                    "Fireworks do not start during ordinary castle gameplay");
                game.Win();
                yield return VictoryFireworksChecks();
                game.Retry(); yield return new WaitForSecondsRealtime(.6f);
                Require(FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 0,
                    "Retry removes the complete victory fireworks show");
                DreamGame.Instance.ReturnToMenu(); yield return new WaitForSecondsRealtime(.6f);
                Require(FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 0,
                    "Returning to chapter selection leaves no fireworks in the menu");
                Save(true, ""); Debug.Log("LOST_DREAM_FIREWORKS_SMOKE_PASSED: " + checks.Count);
                Application.Quit(0); yield break;
            }
            yield return MusicChecks();
            if (Environment.GetCommandLineArgs().Contains("--boss-smoke"))
            {
                DreamGame.Instance.LoadChapter(DreamGame.BossChapterIndex);
                yield return new WaitForSecondsRealtime(.5f);
                Time.timeScale = 3;
                yield return BossBattle();
                yield return Capture("04-victory");
                yield return SummonCleanupChecks();
                Save(true, "");
                Debug.Log("LOST_DREAM_BOSS_SMOKE_PASSED: " + checks.Count + " boss integration checks.");
                Application.Quit(0);
                yield break;
            }
            if (!Environment.GetCommandLineArgs().Contains("--journey-smoke"))
            {
                yield return TutorialSupplyChecks();
                yield return JumpChecks();
                yield return CameraChecks();
                yield return MouseLookChecks();
                yield return AimAlignmentChecks();
                yield return CharacterChecks();
            }
            Time.timeScale = 3;
            yield return new WaitForSeconds(.2f);
            var pausedArrow = DreamProjectile.Spawn(new Vector3(0, 8, 0), Vector3.up * 5, false);
            Vector3 pausedPosition = pausedArrow.transform.position;
            DreamGame.Instance.Pause();
            yield return new WaitForSecondsRealtime(.15f);
            Require(pausedArrow != null && pausedArrow.transform.position == pausedPosition, "Pause preserves in-flight projectile and position");
            DreamGame.Instance.Resume(); Time.timeScale = 3;
            yield return new WaitForSeconds(.1f);
            Require(pausedArrow.transform.position.y > pausedPosition.y, "Resume continues projectile simulation");
            Destroy(pausedArrow.gameObject);
            for (int level = 0; level < 2; level++)
            {
                var game = DreamGame.Instance;
                HalloweenArtChecks(game);
                Require(game.player.characterAnimation != null && game.player.characterAnimation.animator.isHuman,
                    "Chapter uses the selected Timmy humanoid character " + (level + 1));
                game.player.automation = true;
                Require(!game.TryExit(), "Exit rejects incomplete chapter " + (level + 1));
                if (level == 0)
                {
                    yield return TutorialTreeChecks(game);
                    yield return ClosedGardenGateChecks(game);
                }
                if (level == 1)
                {
                    yield return GardenAdventureChecks(game);
                    yield return WalkThroughGardenGate(game);
                    Require(game.Phase == GamePhase.LevelComplete, "Restored garden gate physically advances the expanded chapter");
                    yield return AutomaticTransitionChecks(2); Time.timeScale = 3;
                    Require(DreamGame.Instance.levelIndex == 2, "Expanded garden automatically continues into Witchlight Marsh");
                    continue;
                }
                foreach (var lantern in game.lanterns)
                {
                    Require(!lantern.path.solid.activeSelf, "Hidden road has no active floor");
                    game.player.Teleport(lantern.safeCheckpoint);
                    yield return new WaitForSeconds(.12f);
                    Require(game.player.Interact(), "E interaction lights " + lantern.lanternName);
                    yield return null;
                    var skin = lantern.GetComponent<LanternSkin>();
                    Require(skin != null && skin.lit.activeSelf && !skin.unlit.activeSelf,
                        "Poly Halloween lantern changes to its lit model after interaction");
                    Require(lantern.Lit && lantern.path.solid.activeSelf, "Lantern restores collidable road");
                    Require(Vector3.Distance(game.Checkpoint, lantern.safeCheckpoint) < .1f, "Lantern saves safe checkpoint");
                    if (level == 0 && lantern == game.lanterns[0]) yield return Capture("01-first-lantern");
                    if (lantern.path.IsJumpRoute)
                        yield return JumpRouteChecks(game, lantern.path);
                    // Walk through the retained tutorial bridge with the normal controller.
                    else if (level == 0)
                    {
                        Vector3 start = lantern.path.transform.position - lantern.path.transform.forward * 3;
                        game.player.Teleport(start + Vector3.up * .1f);
                        game.player.automationMove = lantern.path.transform.forward;
                        yield return new WaitForSeconds(.8f);
                        game.player.automationMove = Vector3.zero;
                        Require(game.player.transform.position.y > -.3f, "Restored bridge supports player movement");
                    }
                }
                foreach (var rune in game.runes.Where(x => !x.candyOnly))
                {
                    game.player.Teleport(rune.transform.position + Vector3.back * 4 + Vector3.up * .1f);
                    game.player.FireAt(rune.AimPosition);
                    yield return new WaitForSeconds(.5f);
                    Require(rune.Cleared, "Real arrow clears tutorial practice rune");
                }
                foreach (var dragon in game.dragons)
                {
                    Require(dragon.modelAnimation != null && dragon.body.GetComponentInChildren<SkinnedMeshRenderer>() != null,
                        "Enemy uses imported Quaternius skinned model and animation rig");
                    game.player.Teleport(new Vector3(dragon.home.x, .12f, dragon.home.z - 4));
                    if (level == 0)
                    {
                        yield return new WaitForSeconds(.2f);
                        var skin = dragon.body.GetComponentInChildren<SkinnedMeshRenderer>();
                        Debug.Log("MONSTER_RUNTIME_BOUNDS: " + skin.bounds + " model=" + dragon.modelAnimation.transform.localScale + " origin=" + dragon.modelAnimation.transform.localPosition);
                        var before = new Mesh(); skin.BakeMesh(before);
                        yield return new WaitForSeconds(.23f);
                        var after = new Mesh(); skin.BakeMesh(after);
                        Require(skin.bounds.size.x > 1.5f && skin.bounds.size.x < 6
                            && Vector3.Distance(skin.bounds.center, dragon.transform.position) < 2,
                            "Animated dragon has visible game-scale bounds centered on its hit collider");
                        Require(dragon.modelAnimation.isPlaying && before.vertices.Where((v, i) => Vector3.Distance(v, after.vertices[i]) > .001f).Any(),
                            "Imported flight animation deforms the dragon mesh during gameplay");
                        Destroy(before); Destroy(after);
                        yield return new WaitForSecondsRealtime(.6f);
                        yield return Capture("01-quaternius-dragon");
                    }
                    else if (dragon.kind == NightmareKind.Dragon && dragon.health == 3)
                    {
                        yield return new WaitForSecondsRealtime(.6f);
                        var skin = dragon.body.GetComponentInChildren<SkinnedMeshRenderer>();
                        Require(skin.bounds.size.x > 1.5f && skin.bounds.size.x < 7 && Vector3.Distance(skin.bounds.center, dragon.transform.position) < 2,
                            "Evolved dragon is visible and centered on its hit collider");
                        yield return Capture("02-quaternius-evolved");
                    }
                    if (dragon.kind != NightmareKind.Dragon) yield return EnemyVarietyChecks(game, dragon);
                    int attempts = 0;
                    while (!dragon.Defeated && attempts++ < 16)
                    {
                        game.player.FireAt(dragon.AimPosition);
                        yield return new WaitForSeconds(.45f);
                    }
                    Require(dragon.Defeated, "Real arrows defeat moving dragon");
                    game.player.Heal(5);
                }
                foreach (var rune in game.runes.Where(x => x.candyOnly))
                {
                    for (int round = 0; round < 3; round++)
                    {
                        var supply = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                            .Where(x => game.candySpawnPoints.Any(point => Vector3.Distance(x.transform.position, point) < 1.5f)).ToArray();
                        Require(supply.Length == 2, "Tutorial has two collectible candies before miss round " + round);
                        foreach (var sweet in supply)
                        {
                            game.player.Teleport(sweet.transform.position + Vector3.back * .8f);
                            yield return new WaitForSeconds(.15f);
                            Require(game.player.Interact() && game.player.CarryingCandy, "E collects tutorial candy in miss round " + round);
                            game.player.Teleport(new Vector3(0, .12f, 53));
                            Require(game.player.ThrowAt(new Vector3(-20, 1, 53)), "Real candy throw misses the seal in round " + round);
                            yield return new WaitForSeconds(.6f);
                        }
                        yield return new WaitForSeconds(3.2f);
                        Require(!rune.Cleared && !game.player.CarryingCandy && FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                            .Count(x => game.candySpawnPoints.Any(point => Vector3.Distance(x.transform.position, point) < 1.5f)) == 2,
                            "Timer restores both tutorial candies after every original candy was thrown away in round " + round);
                    }
                    yield return Capture("01-tutorial-candy-refill");
                    var candy = FindFirstObjectByType<CandyPickup>();
                    game.player.Teleport(candy.transform.position + Vector3.back * .8f);
                    yield return new WaitForSeconds(.15f);
                    Require(game.player.Interact() && game.player.CarryingCandy, "Replenished tutorial candy can be collected");
                    game.player.Teleport(rune.transform.position + Vector3.back * 5 + Vector3.up * .1f);
                    game.player.ThrowAt(rune.AimPosition);
                    yield return new WaitForSeconds(1.5f);
                    Require(rune.Cleared && !game.player.CarryingCandy, "Ballistic candy shatters nightmare seal and consumes carried candy");
                    foreach (var spare in FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)) Destroy(spare.gameObject);
                    yield return new WaitForSeconds(3.2f);
                    Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == 0,
                        "Tutorial refill stops once the candy seal is cleared");
                }
                int previousHealth = game.player.Health;
                game.player.Teleport(new Vector3(0, -9, 0));
                yield return new WaitForSeconds(.2f);
                Require(game.player.Health == previousHealth - 1 && game.player.transform.position.y > -.5f, "Falling costs health and returns to safe checkpoint");
                game.player.Heal(5);
                Require(game.ObjectivesComplete, "All chapter objectives complete " + (level + 1));
                game.player.Teleport(level == 0 ? new Vector3(0, .1f, 66) : new Vector3(2, .1f, 79));
                yield return Capture("0" + (level + 1) + "-restored");
                var gate = FindFirstObjectByType<DreamGate>();
                yield return WalkThroughGardenGate(game);
                Require(game.Phase == GamePhase.LevelComplete, "Physical gate trigger completes chapter " + (level + 1));
                yield return AutomaticTransitionChecks(level + 1);
                Time.timeScale = 3;
                Require(DreamGame.Instance.levelIndex == level + 1, "Scene transition preserves chapter order");
            }
            yield return MarshAdventureChecks(DreamGame.Instance);
            yield return WalkThroughGardenGate(DreamGame.Instance);
            yield return AutomaticTransitionChecks(3);
            yield return BossBattle();
            yield return Capture("04-victory");
            yield return SummonCleanupChecks();
            DreamGame.Instance.LoadChapter(1); yield return new WaitForSecondsRealtime(.7f);
            var resetGarden = DreamGame.Instance; resetGarden.player.automation = true;
            Require(resetGarden.garden.MemoriesCollected == 0 && resetGarden.garden.SequenceProgress == 0 &&
                resetGarden.LitCount == 0 && resetGarden.DragonsRemaining == 11 && resetGarden.RunesRemaining == 4,
                "Revisiting the completed garden restores a fresh adventure with all objectives reset");
            resetGarden.player.Teleport(resetGarden.lanterns[0].safeCheckpoint); yield return new WaitForSeconds(.15f);
            Require(resetGarden.player.Interact() && resetGarden.LitCount == 1, "Garden retry setup restores its first road through a real E action");
            resetGarden.Retry(); yield return new WaitForSecondsRealtime(.7f);
            Require(DreamGame.Instance.LitCount == 0 && DreamGame.Instance.garden.MemoriesCollected == 0 &&
                DreamGame.Instance.lanterns.All(x => !x.path.Revealed), "Retry resets garden progression and hides every restored route");
            Save(true, null);
            Debug.Log("LOST_DREAM_SMOKE_PASSED: " + checks.Count + " functional integration checks.");
            Application.Quit(0);
        }
        IEnumerator GardenAdventureChecks(DreamGame game)
        {
            Time.timeScale = 1;
            var garden = game.garden; var player = game.player;
            Require(garden != null && garden.memories.Length == 3 && garden.sequenceRunes.Length == 3,
                "Saved garden contains three authored memories and the ordered moon puzzle");
            Require(game.GetComponentsInChildren<BoxCollider>(true).Count(x => x.name == "Solid dream island") == 7 &&
                game.lanterns.Length == 6 && game.dragons.Length == 11,
                "Garden expands to seven islands, six lanterns and eleven distributed enemies");
            Require(game.dragons.All(x => x.health <= 3) && game.dragons.Select(x => x.kind).Distinct().Count() == 4,
                "Garden adds encounters with all four species without inflating enemy health");
            var cryptFloor = garden.transform.Find("Candy Crypt/Solid dream island").GetComponent<BoxCollider>();
            var gateFloor = game.transform.Find("Dream gate garden/Solid dream island").GetComponent<BoxCollider>();
            Require(cryptFloor.bounds.max.x < gateFloor.bounds.min.x && Mathf.Abs(cryptFloor.bounds.center.z - gateFloor.bounds.center.z) < .1f,
                "Final Crypt island sits to the left of the gate garden instead of behind its portal");
            Require(Vector3.Dot(garden.cryptLight.path.transform.forward, Vector3.left) > .99f &&
                garden.cryptLight.path.jumpEntry.position.x > garden.cryptLight.path.jumpExit.position.x,
                "Final round-platform route connects westward from the main island to the left Crypt");
            Require(!game.TryExit() && !garden.Complete, "Garden gate rejects a zero-memory shortcut");
            yield return ClosedGardenGateChecks(game);
            var labelCamera = game.gameCamera.GetComponent<DreamCamera>();
            var labels = garden.GetComponentsInChildren<TextMesh>().Where(x => x.name == "Readable objective label").ToArray();
            foreach (float yaw in new[] { 0f, 90f, 180f, -90f })
            {
                labelCamera.yaw = yaw; labelCamera.Snap(); yield return null; yield return new WaitForEndOfFrame();
                Require(labels.Length == 6 && labels.All(x => Vector3.Dot(x.transform.forward,
                    (x.transform.position - game.gameCamera.transform.position).normalized) > .999f),
                    "Memory and rune numbers remain readable as the mouse camera orbits: " + yaw);
            }
            player.Teleport(game.lanterns[1].safeCheckpoint); yield return new WaitForSeconds(.15f);
            Require(!player.Interact() && !game.lanterns[1].Lit && !game.lanterns[1].path.Revealed,
                "Terrace lantern requires the Whisper Grove memory");
            yield return ShootGardenRune(game, garden.sequenceRunes[0]);
            Require(garden.SequenceProgress == 0, "Shooting the Observatory before reaching its unlocked stage cannot advance it");
            yield return LightGardenRoute(game, game.lanterns[0]);
            foreach (var enemy in garden.memories[0].guardians.Take(2)) yield return DefeatGardenEnemy(game, enemy, true);
            yield return LightGardenRoute(game, garden.groveLight);
            player.Teleport(garden.memories[0].transform.position + Vector3.back * 2 + Vector3.up * .12f);
            yield return new WaitForSeconds(.1f);
            Require(!player.Interact() && !garden.memories[0].Collected, "Guardians protect the first memory until defeated");
            foreach (var enemy in garden.memories[0].guardians.Skip(2)) yield return DefeatGardenEnemy(game, enemy, false);
            yield return CollectGardenMemory(game, garden.memories[0]);
            Vector3 checkpoint = game.Checkpoint;
            player.Teleport(new Vector3(-44, -9, 25)); yield return new WaitForSeconds(.2f);
            Require(garden.MemoriesCollected == 1 && Vector3.Distance(player.transform.position, checkpoint) < 1,
                "A fall preserves recovered memories and returns to the new shrine checkpoint");
            player.Heal(5);
            yield return ReturnGardenRoute(game, garden.groveLight.path);
            Require(!game.TryExit(), "One recovered memory does not open the dream gate");
            Require(!game.GetComponentInChildren<GardenIronGate>().Unlocked,
                "Recovering only memory I leaves the iron gate closed");
            yield return LightGardenRoute(game, game.lanterns[1]);
            foreach (var enemy in garden.memories[1].guardians.Take(2)) yield return DefeatGardenEnemy(game, enemy, true);
            yield return LightGardenRoute(game, garden.observatoryLight);
            foreach (var enemy in garden.memories[1].guardians.Skip(2)) yield return DefeatGardenEnemy(game, enemy, false);
            player.Teleport(garden.memories[1].transform.position + Vector3.back * 2 + Vector3.up * .12f);
            yield return new WaitForSeconds(.1f);
            Require(!player.Interact(), "Observatory memory requires its moon melody even after all guardians are cleared");
            yield return ShootGardenRune(game, garden.sequenceRunes[0]);
            Require(garden.SequenceProgress == 1 && !garden.sequenceRunes[0].Cleared,
                "Real arrow lights the first moon rune without prematurely consuming its retry collider");
            game.Pause(); yield return new WaitForSecondsRealtime(.2f);
            Require(garden.SequenceProgress == 1, "Pausing preserves the partial moon melody"); game.Resume();
            yield return ShootGardenRune(game, garden.sequenceRunes[2]);
            Require(garden.SequenceProgress == 0 && garden.sequenceRunes.All(x => x.GetComponent<Collider>().enabled),
                "Wrong-order arrow resets the melody and leaves every rune available to retry");
            foreach (var rune in garden.sequenceRunes) yield return ShootGardenRune(game, rune);
            Require(garden.SequenceComplete && garden.sequenceRunes.All(x => x.Cleared),
                "Real arrows in I II III order permanently restore all three moon runes");
            yield return Capture("02-observatory-melody");
            yield return CollectGardenMemory(game, garden.memories[1]);
            yield return ReturnGardenRoute(game, garden.observatoryLight.path);
            Require(!game.TryExit(), "Two recovered memories do not open the dream gate");
            Require(!game.GetComponentInChildren<GardenIronGate>().Unlocked,
                "Recovering only memories I and II leaves the iron gate closed");
            yield return LightGardenRoute(game, game.lanterns[2]);
            yield return DefeatGardenEnemy(game, garden.memories[2].guardians[0], true);
            yield return LightGardenRoute(game, garden.cryptLight);
            foreach (var enemy in garden.memories[2].guardians.Skip(1)) yield return DefeatGardenEnemy(game, enemy, false);
            player.Teleport(garden.memories[2].transform.position + Vector3.back * 2 + Vector3.up * .12f);
            yield return new WaitForSeconds(.1f);
            Require(!player.Interact(), "Final memory requires breaking the candy seal after clearing its guardians");
            yield return ShootGardenRune(game, garden.candySeal);
            Require(!garden.candySeal.Cleared, "Bow arrows cannot bypass the crypt candy seal");
            // Waste both supplies through the ordinary pickup/throw flow, then wait for refill.
            foreach (var point in garden.candyPoints)
            {
                player.Teleport(point + Vector3.back * .5f + Vector3.up * .12f); yield return new WaitForSeconds(.1f);
                Require(player.Interact() && player.CarryingCandy, "Crypt candy supply can be picked up with E");
                Require(player.ThrowAt(point + Vector3.up * 10), "Crypt candy miss launches through the normal throw cooldown");
                yield return new WaitForSeconds(.8f);
            }
            Require(!player.CarryingCandy, "Wasting both crypt candies consumes both held throws");
            yield return new WaitForSeconds(3.2f);
            Require(garden.candyPoints.All(point => FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                .Any(x => Vector3.Distance(point, x.transform.position) < 1.5f)),
                "Every crypt candy supply replenishes after missed throws, preventing a soft lock");
            var sweetPoint = garden.candyPoints[0];
            player.Teleport(sweetPoint + Vector3.back * .5f + Vector3.up * .12f); yield return new WaitForSeconds(.1f);
            Require(player.Interact() && player.CarryingCandy, "Replenished crypt candy remains usable");
            player.Teleport(garden.candySeal.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            player.ThrowAt(garden.candySeal.AimPosition); yield return new WaitForSeconds(1.3f);
            Require(garden.candySeal.Cleared && !player.CarryingCandy, "A real ballistic candy throw shatters the crypt seal");
            Require(game.LitCount == 6 && game.DragonsRemaining == 0 && game.RunesRemaining == 0 && !game.TryExit(),
                "Clearing every old-style objective still cannot bypass collecting the final memory");
            yield return CollectGardenMemory(game, garden.memories[2]);
            yield return OpeningGardenGateChecks(game);
            foreach (var sweet in FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                .Where(x => garden.candyPoints.Any(point => Vector3.Distance(point, x.transform.position) < 1.5f))) Destroy(sweet.gameObject);
            yield return new WaitForSeconds(3.2f);
            Require(!FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                .Any(x => garden.candyPoints.Any(point => Vector3.Distance(point, x.transform.position) < 1.5f)),
                "Crypt supply stops replenishing once its seal is solved");
            yield return ReturnGardenRoute(game, garden.cryptLight.path);
            Require(game.ObjectivesComplete && garden.MemoriesCollected == 3 && game.DragonsRemaining == 0 && game.RunesRemaining == 0,
                "Expanded chapter opens only after all three memories, six lanterns, eleven enemies and four runes");
            var restoredGate = game.GetComponentInChildren<GardenIronGate>();
            Require(restoredGate.IsOpen && restoredGate.groundCue.activeSelf && !restoredGate.closedBarrier.enabled &&
                game.Phase == GamePhase.Playing, "Open iron gate lights the route and waits for the player to walk through");
            player.Teleport(new Vector3(2, .12f, 77)); game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("02-three-memories-restored");
            var gateCamera = game.gameCamera.GetComponent<DreamCamera>(); gateCamera.yaw = 0; gateCamera.Snap();
            yield return Capture("02-iron-gate-open-trail");
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>();
            Vector3 oldPosition = camera.transform.position; Quaternion oldRotation = camera.transform.rotation;
            bool oldFog = RenderSettings.fog; float oldSize = camera.orthographicSize;
            follow.enabled = false; camera.orthographic = true; camera.orthographicSize = 65;
            camera.transform.SetPositionAndRotation(new Vector3(-5, 140, 55), Quaternion.Euler(90, 0, 0));
            RenderSettings.fog = false;
            yield return Capture("02-seven-island-layout");
            camera.orthographic = false; camera.orthographicSize = oldSize; RenderSettings.fog = oldFog;
            camera.transform.SetPositionAndRotation(oldPosition, oldRotation); follow.enabled = true;
        }
        IEnumerator ClosedGardenGateChecks(DreamGame game)
        {
            var iron = game.GetComponentInChildren<GardenIronGate>();
            Require(iron != null && !iron.Unlocked && !iron.IsOpen && iron.closedBarrier.enabled && !iron.groundCue.activeSelf,
                "Saved iron gate starts shut, physically blocking passage, with its ground trail dark");
            var active = game.dragons.Where(x => x.enabled).ToArray(); foreach (var enemy in active) enemy.enabled = false;
            var player = game.player;
            player.Teleport(iron.transform.TransformPoint(new Vector3(0, .12f, -3)));
            var follow = game.gameCamera.GetComponent<DreamCamera>(); follow.yaw = 0; follow.Snap();
            yield return Capture("0" + (game.levelIndex + 1) + "-iron-gate-closed");
            player.automationMove = iron.transform.forward;
            yield return new WaitForSeconds(.9f); player.automationMove = Vector3.zero;
            Require(iron.transform.InverseTransformPoint(player.transform.position).z < 0 && game.Phase == GamePhase.Playing,
                "Real player movement cannot walk through the closed iron gate");
            yield return ExitEnclosureChecks(game);
            player.Teleport(game.spawnPoint); foreach (var enemy in active) enemy.enabled = true;
        }
        IEnumerator ExitEnclosureChecks(DreamGame game)
        {
            var iron = game.GetComponentInChildren<GardenIronGate>(); var player = game.player;
            Require(iron.sideBarriers != null && iron.sideBarriers.Length == 2 && iron.sideBarriers.All(c => c.enabled && !c.isTrigger) &&
                iron.rearBarriers != null && iron.rearBarriers.Length == 2 && iron.rearBarriers.All(c => c.enabled),
                "Exit has two continuous side wings and a physical rear perimeter in chapter " + (game.levelIndex + 1));
            Require(iron.rearBarriers[0].bounds.max.x < iron.transform.position.x - 2 &&
                iron.rearBarriers[1].bounds.min.x > iron.transform.position.x + 2,
                "Rear enclosure leaves a clear centered sightline through the opening gate");
            var enclosure = iron.transform.Find("Exit perimeter enclosure");
            foreach (string side in new[] { "Left", "Right" })
            {
                var parts = enclosure.Cast<Transform>().Where(t => t.name.StartsWith(side) || t.name.StartsWith("Rear " + side.ToLowerInvariant()))
                    .SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray();
                Require(parts.Length > 0 && parts.All(r => side == "Left" ? r.bounds.max.x < iron.transform.position.x - 1.5f :
                    r.bounds.min.x > iron.transform.position.x + 1.5f),
                    "Visible " + side.ToLowerInvariant() + " fence meshes stay outside the doorway instead of stretching across its sightline");
            }
            foreach (var wall in iron.sideBarriers)
            {
                Require(wall.bounds.size.y > player.jumpHeight + .3f, "Exit wing railing exceeds the reachable jump height");
                foreach (float fraction in new[] { .15f, .5f, .85f })
                {
                    float x = Mathf.Lerp(wall.bounds.min.x, wall.bounds.max.x, fraction);
                    player.Teleport(new Vector3(x, .12f, iron.transform.position.z - 1.5f));
                    yield return new WaitForSeconds(.15f);
                    player.automationMove = iron.transform.forward; yield return new WaitForSeconds(.5f);
                    player.automationMove = Vector3.zero;
                    Require(iron.transform.InverseTransformPoint(player.transform.position).z < 0,
                        "Real controller cannot walk around the exit through a side opening");
                }
                player.Teleport(new Vector3(wall.bounds.center.x, .12f, iron.transform.position.z - 1.2f));
                yield return new WaitForSeconds(.2f);
                Require(player.IsGrounded && player.TryJump(), "Exit wing test starts a real grounded jump");
                player.automationMove = iron.transform.forward; yield return new WaitForSeconds(1.1f);
                player.automationMove = Vector3.zero;
                Require(iron.transform.InverseTransformPoint(player.transform.position).z < 0,
                    "A real jump cannot bypass the exit through its side wing");
            }
            foreach (var rearWall in iron.rearBarriers)
            {
                var rear = rearWall.bounds;
                player.Teleport(new Vector3(rear.min.x + 1.5f, .12f, rear.center.z - 1.2f));
                yield return new WaitForSeconds(.15f);
                player.automationMove = iron.transform.forward; yield return new WaitForSeconds(.5f); player.automationMove = Vector3.zero;
                Require(player.transform.position.z < rear.center.z && player.transform.position.y > -.2f,
                    "Rear side perimeter blocks walking off beside the exit opening");
            }
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>();
            player.Teleport(iron.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            follow.enabled = false;
            camera.transform.position = iron.transform.position + new Vector3(8, 13, -14);
            camera.transform.LookAt(iron.transform.position + Vector3.back);
            yield return Capture("0" + (game.levelIndex + 1) + "-enclosed-exit-overview");
            follow.enabled = true; follow.Snap(); player.Heal(5);
        }
        IEnumerator TutorialTreeChecks(DreamGame game)
        {
            var group = game.transform.Find("Tutorial trees - Poly Halloween");
            var old = game.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Dream orchard tree").ToArray();
            Require(group != null && group.childCount == old.Length && old.Length == 18 &&
                old.SelectMany(t => t.GetComponentsInChildren<Renderer>(true)).All(r => !r.enabled),
                "All eighteen tutorial tree positions use imported dead trees and hide the former round canopy art");
            Require(group.GetComponentsInChildren<Transform>().Any(t => t.name.Contains("Tree_Scary_Dead")) &&
                group.GetComponentsInChildren<Transform>().Any(t => t.name.Contains("Tree_Poplar_Dead_A")) &&
                group.GetComponentsInChildren<SceneryObstacle>(true).All(o => o.Solid) &&
                group.GetComponentsInChildren<SceneryObstacle>(true).Length == 18,
                "Tutorial trees match the garden's two dead-tree models with fitted physical collision");
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>();
            follow.enabled = false; camera.transform.position = new Vector3(14, 13, -14); camera.transform.LookAt(new Vector3(0, 1, 3));
            yield return Capture("01-matching-dead-trees"); follow.enabled = true; follow.Snap();
        }
        IEnumerator OpeningGardenGateChecks(DreamGame game)
        {
            var iron = game.GetComponentInChildren<GardenIronGate>();
            Require(iron.Unlocked && iron.OpenAmount > 0 && iron.OpenAmount < 1 && iron.closedBarrier.enabled && !iron.groundCue.activeSelf,
                "The final memory starts the two-leaf opening animation before the exit ground lights turn on");
            float opening = iron.OpenAmount; Quaternion left = iron.leftLeaf.localRotation, right = iron.rightLeaf.localRotation;
            game.Pause(); yield return new WaitForSecondsRealtime(.25f);
            Require(Mathf.Abs(iron.OpenAmount - opening) < .0001f && Quaternion.Angle(left, iron.leftLeaf.localRotation) < .01f &&
                Quaternion.Angle(right, iron.rightLeaf.localRotation) < .01f,
                "Pause freezes both iron gate leaves during opening");
            game.Resume(); yield return new WaitForSeconds(2);
            Require(iron.IsOpen && Quaternion.Angle(left, iron.leftLeaf.localRotation) > 40 &&
                Quaternion.Angle(right, iron.rightLeaf.localRotation) > 40 && !iron.closedBarrier.enabled && iron.groundCue.activeSelf,
                "Both iron leaves swing open, remove their blocker, then reveal the glowing ground trail");
            Require(iron.groundCue.GetComponentsInChildren<Renderer>().Length >= 8 && iron.cueLight.enabled &&
                iron.groundCue.GetComponentsInChildren<Collider>().Length == 0,
                "The unlocked ground cue has visible glowing steps, direction arrows and light without obstructing movement");
            Vector3 previous = game.player.transform.position;
            game.player.Teleport(iron.transform.TransformPoint(new Vector3(0, .12f, 1.2f)));
            yield return new WaitForSeconds(.2f);
            Require(game.Phase == GamePhase.Playing,
                "Unlocking does not auto-complete the chapter for a player already standing behind the gate");
            game.player.Teleport(previous);
        }
        IEnumerator WalkThroughGardenGate(DreamGame game)
        {
            var iron = game.GetComponentInChildren<GardenIronGate>(); var player = game.player;
            Require(iron.IsOpen && game.Phase == GamePhase.Playing, "Player approaches the open iron gate without an automatic scene change");
            player.Teleport(iron.transform.TransformPoint(new Vector3(0, .12f, -3)));
            yield return new WaitForSeconds(.15f);
            int health = player.Health;
            player.automationMove = iron.transform.forward;
            float deadline = Time.time + 2.5f;
            while (game.Phase == GamePhase.Playing && Time.time < deadline) yield return null;
            player.automationMove = Vector3.zero;
            Require(game.Phase == GamePhase.LevelComplete && player.Health == health &&
                iron.transform.InverseTransformPoint(player.transform.position).z > .8f,
                "Real controller movement follows the light through the open iron gate and completes chapter " + (game.levelIndex + 1));
        }
        IEnumerator LightGardenRoute(DreamGame game, Lantern lamp)
        {
            Require(!lamp.path.Revealed && !lamp.path.solid.activeSelf, "Garden route starts hidden: " + lamp.lanternName);
            game.player.Teleport(lamp.safeCheckpoint); yield return new WaitForSeconds(.15f);
            // A nearby guardian's drop can correctly take priority over a farther lamp.
            // Use ordinary pickup/throw actions to clear that incidental interaction first.
            int interactions = 0;
            while (!lamp.Lit && interactions++ < 6)
            {
                if (!game.player.Interact()) break;
                if (!lamp.Lit)
                {
                    Require(game.player.CarryingCandy && game.player.ThrowAt(game.player.transform.position + Vector3.up * 12),
                        "Clear an incidental guardian candy through normal E and throw actions before lighting " + lamp.lanternName);
                    yield return new WaitForSeconds(.7f);
                }
            }
            Require(lamp.Lit && lamp.path.Revealed, "E unlocks garden route: " + lamp.lanternName);
            yield return null;
            var skin = lamp.GetComponent<LanternSkin>();
            Require(skin != null && skin.lit.activeSelf && !skin.unlit.activeSelf,
                "Garden lantern retains the imported lit/unlit artwork: " + lamp.lanternName);
            if (lamp.path.IsJumpRoute)
            {
                var follow = game.gameCamera.GetComponent<DreamCamera>();
                follow.yaw = Vector3.SignedAngle(Vector3.forward, lamp.path.transform.forward, Vector3.up);
                follow.Snap();
                yield return JumpRouteChecks(game, lamp.path);
            }
        }
        IEnumerator CollectGardenMemory(DreamGame game, DreamMemory memory)
        {
            game.player.Teleport(memory.transform.position + Vector3.back * 2 + Vector3.up * .12f);
            yield return new WaitForSeconds(.1f);
            Require(game.player.Interact() && memory.Collected && game.garden.MemoriesCollected == memory.index + 1,
                "E collects the unlocked dream memory " + (memory.index + 1));
            Require(Vector3.Distance(game.Checkpoint, game.player.transform.position) < .2f,
                "Recovered memory saves a safe island checkpoint " + (memory.index + 1));
            game.gameCamera.GetComponent<DreamCamera>().Snap(); yield return Capture("02-memory-" + (memory.index + 1));
        }
        IEnumerator ShootGardenRune(DreamGame game, DreamRune rune)
        {
            game.player.Teleport(rune.transform.position + Vector3.back * 4 + Vector3.up * .12f);
            yield return new WaitForSeconds(.45f);
            Require(game.player.FireAt(rune.AimPosition), "Bow launches at garden rune " + rune.name);
            yield return new WaitForSeconds(.6f);
        }
        IEnumerator DefeatGardenEnemy(DreamGame game, FlyingDragon enemy, bool reviewAttack)
        {
            Require(enemy.modelAnimation != null && enemy.body.GetComponentInChildren<SkinnedMeshRenderer>() != null,
                "Garden guardian uses an imported animated monster: " + enemy.DisplayName);
            if (reviewAttack && enemy.kind != NightmareKind.Dragon && !Environment.GetCommandLineArgs().Contains("--journey-smoke"))
                yield return EnemyVarietyChecks(game, enemy);
            foreach (var shot in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Where(x => x.hostile)) Destroy(shot.gameObject);
            game.player.Teleport(new Vector3(enemy.home.x, .12f, enemy.home.z - 4));
            yield return new WaitForSeconds(.25f);
            int attempts = 0;
            while (!enemy.Defeated && attempts++ < 20)
            {
                game.player.FireAt(enemy.AimPosition); yield return new WaitForSeconds(.45f); game.player.Heal(5);
            }
            Require(enemy.Defeated, "Actual bow shots defeat garden guardian at " + enemy.home);
        }
        IEnumerator ReturnGardenRoute(DreamGame game, DreamPath route)
        {
            var player = game.player;
            player.Teleport(route.jumpExit.position); yield return new WaitForSeconds(.3f);
            var targets = route.jumpPlatforms.Reverse().Select(x => x.position + Vector3.up * .04f)
                .Concat(new[] { route.jumpEntry.position }).ToArray();
            foreach (var target in targets)
            {
                Require(player.IsGrounded && player.TryJump(), "Return trip launches a grounded garden jump");
                bool airborne = false; float deadline = Time.time + 2;
                while (Time.time < deadline)
                {
                    if (!player.IsGrounded) airborne = true;
                    if (airborne && player.IsGrounded) break;
                    Vector3 delta = Vector3.ProjectOnPlane(target - player.transform.position, Vector3.up);
                    player.automationMove = delta.magnitude > .06f ? delta.normalized * Mathf.Clamp01(delta.magnitude / (player.moveSpeed * Time.deltaTime)) : Vector3.zero;
                    yield return null;
                }
                player.automationMove = Vector3.zero;
                Require(airborne && player.IsGrounded && Vector3.ProjectOnPlane(player.transform.position - target, Vector3.up).magnitude < .25f,
                    "Garden branch is traversable in reverse with the real jump controller");
                yield return new WaitForSeconds(.12f);
            }
        }
        IEnumerator EnemyVarietyChecks(DreamGame game, FlyingDragon enemy)
        {
            float previousScale = Time.timeScale; Time.timeScale = 1;
            var player = game.player;
            var others = game.dragons.Where(x => x != enemy && x.enabled).ToArray();
            foreach (var other in others) other.enabled = false;
            foreach (var shot in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Where(x => x.hostile)) Destroy(shot.gameObject);
            player.Teleport(new Vector3(enemy.home.x, .12f, enemy.home.z - 4)); player.Heal(5);
            var skin = enemy.body.GetComponentInChildren<SkinnedMeshRenderer>();
            yield return new WaitForSeconds(.25f);
            var beforeMesh = new Mesh(); skin.BakeMesh(beforeMesh);
            yield return new WaitForSeconds(.25f);
            var afterMesh = new Mesh(); skin.BakeMesh(afterMesh);
            Debug.Log("VARIETY_RUNTIME_BOUNDS: " + enemy.kind + " center=" + skin.bounds.center + " size=" + skin.bounds.size);
            Require(skin.bounds.size.magnitude > 1.3f && skin.bounds.size.magnitude < 7 &&
                Vector3.Distance(skin.bounds.center, enemy.transform.position) < 1.5f,
                "New species is visibly sized and centered on its hit collider: " + enemy.kind);
            Require(enemy.modelAnimation.isPlaying && beforeMesh.vertices.Where((v, i) => Vector3.Distance(v, afterMesh.vertices[i]) > .001f).Any(),
                "New species uses real deformed flight animation: " + enemy.kind);
            Destroy(beforeMesh); Destroy(afterMesh);
            float deadline = Time.time + 12;
            while (enemy.Phase != DragonPhase.Telegraph && Time.time < deadline) yield return null;
            Require(enemy.Phase == DragonPhase.Telegraph && enemy.chargeMarker.enabled,
                "New species visibly telegraphs before attacking: " + enemy.kind);
            foreach (var shot in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Where(x => x.hostile)) Destroy(shot.gameObject);
            Vector3 paused = enemy.transform.position;
            int attacks = enemy.AttacksCompleted;
            game.Pause(); yield return new WaitForSecondsRealtime(.2f);
            Require(enemy.Phase == DragonPhase.Telegraph && enemy.transform.position == paused && enemy.AttacksCompleted == attacks,
                "Pause freezes species warning and attack: " + enemy.kind);
            game.Resume(); Time.timeScale = 1;
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("02-enemy-" + enemy.kind.ToString().ToLowerInvariant());
            if (enemy.kind == NightmareKind.Demon)
            {
                Vector3 target = enemy.DiveTarget, start = enemy.DiveStart;
                Vector3 side = Vector3.Cross(Vector3.ProjectOnPlane(target - start, Vector3.up).normalized, Vector3.up);
                if (side.sqrMagnitude < .01f) side = Vector3.right;
                Vector3 safe = (target + start) * .5f + side * 4; safe.y = .12f;
                if (!Physics.Raycast(safe + Vector3.up * 3, Vector3.down, 5))
                    safe = new Vector3(enemy.home.x + 6, .12f, enemy.home.z);
                Require(enemy.DiveWarning != null && LaneDistance(safe, start, target) > 2,
                    "Demon has a visible locked lane and a safe sideways escape");
                player.Teleport(safe);
                int hp = player.Health;
                Require(!player.Invulnerable, "Demon avoidance is tested without immunity");
                yield return null;
                Require(enemy.DiveTarget == target, "Demon dive does not retarget after a sidestep");
                deadline = Time.time + 4;
                while (enemy.AttacksCompleted == attacks && Time.time < deadline) yield return null;
                Require(enemy.AttacksCompleted == attacks + 1 && enemy.LastProjectileCount == 0 && enemy.DiveWarning == null,
                    "Demon physically dives and clears its warning on recovery");
                Require(player.Health == hp, "A sideways step avoids the actual demon dive");
                player.Teleport(new Vector3(enemy.home.x, .12f, enemy.home.z - 4));
                deadline = Time.time + 12;
                while (enemy.Phase != DragonPhase.Telegraph && Time.time < deadline) yield return null;
                Require(enemy.Phase == DragonPhase.Telegraph, "Demon returns and starts another warned attack");
                target = enemy.DiveTarget; attacks = enemy.AttacksCompleted;
                player.Teleport(new Vector3(target.x, .12f, target.z));
                hp = player.Health;
                deadline = Time.time + 4;
                while (enemy.AttacksCompleted == attacks && Time.time < deadline) yield return null;
                Require(enemy.AttacksCompleted == attacks + 1 && player.Health == hp - 1,
                    "Standing in the demon dive path takes exactly one contact hit");
            }
            else
            {
                deadline = Time.time + 3;
                while (enemy.AttacksCompleted == attacks && Time.time < deadline) yield return null;
                var projectiles = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Where(x => x.hostile).ToArray();
                int expected = enemy.kind == NightmareKind.Armabee ? 3 : 1;
                Require(enemy.AttacksCompleted == attacks + 1 && projectiles.Length == expected,
                    "Actual species projectile count matches its attack: " + enemy.kind);
                Require(projectiles.All(x => x.hostileMaterial == enemy.AttackMaterial &&
                    Mathf.Abs(x.velocity.magnitude - (enemy.kind == NightmareKind.Ghost ? 5 : 7.5f)) < .01f),
                    "Species projectiles have distinct visible color and speed: " + enemy.kind);
                if (expected == 3)
                    Require(projectiles.SelectMany(a => projectiles.Select(b => Vector3.Angle(a.velocity, b.velocity))).Max() > 20,
                        "Bee fires three genuinely separated fan directions");
                int hp = player.Health;
                yield return new WaitForSeconds(1.3f);
                Require(player.Health == hp - 1, "A real species projectile damages the player: " + enemy.kind);
            }
            foreach (var shot in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Where(x => x.hostile)) Destroy(shot.gameObject);
            foreach (var other in others) other.enabled = true;
            player.Heal(5); Time.timeScale = previousScale;
        }
        IEnumerator ColorMatchingChecks(DreamGame game)
        {
            float previousScale = Time.timeScale; Time.timeScale = 1;
            var player = game.player; var boss = game.boss;
            Require(boss.Health == boss.maxHealth && boss.UnlockedColors == 1 && boss.RequiredHue == CandyHue.Pink,
                "Boss introduces matching with only pink before unlocking other colors");
            var dais = game.transform.Find("Boss dais");
            Require(dais.GetComponent<MeshCollider>() != null && dais.GetComponent<CapsuleCollider>() == null,
                "Boss stone base has flat mesh collision without an invisible capsule blocking candy");
            var spot = new Vector3(0, .12f, 11);
            player.Teleport(spot);
            var blue = DreamWorld.Candy(game.transform, spot + Vector3.up * .53f, CandyHue.Blue);
            yield return new WaitForSeconds(.05f);
            Require(player.Interact() && player.HeldCandyHue == CandyHue.Blue, "E picks up the actual blue candy hue");
            var pink = DreamWorld.Candy(game.transform, spot + Vector3.up * .53f + Vector3.right * .8f, CandyHue.Pink);
            yield return new WaitForSeconds(.05f);
            int count = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length;
            Require(player.Interact() && player.HeldCandyHue == CandyHue.Pink && pink.hue == CandyHue.Blue,
                "E exchanges a held wrong color for the required floor candy");
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == count,
                "Candy swapping preserves both held and ground supplies");
            Require(player.Interact() && player.HeldCandyHue == CandyHue.Blue && pink.hue == CandyHue.Pink,
                "Candy swapping can be reversed without throwing away ammunition");
            Require(player.heldCandy.GetComponentsInChildren<Renderer>().First(x => x.name == "Sweet").sharedMaterial == DreamArt.Mat("CandyBlue"),
                "Held candy visibly retains its blue material");
            CandyHue hue = boss.RequiredHue;
            int health = boss.Health;
            game.Pause(); yield return new WaitForSecondsRealtime(.2f);
            Require(boss.RequiredHue == hue && boss.Exposed, "Pause preserves required color and vulnerable window");
            game.Resume(); Time.timeScale = 1;
            Require(player.ThrowAt(boss.AimPosition), "Wrong-color regression launches a real blue candy projectile");
            var shot = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).First(x => x.candy);
            Require(shot.hue == CandyHue.Blue, "Thrown projectile preserves the carried candy hue");
            yield return new WaitForSeconds(.65f);
            Debug.Log("COLOR_WRONG_RESULT: health=" + boss.Health + " expected=" + health + " exposed=" + boss.Exposed +
                " phase=" + boss.Phase + " hue=" + boss.RequiredHue + " toast=" + game.Toast +
                " projectile=" + (shot == null ? "destroyed" : shot.transform.position.ToString()));
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("04-color-mismatch");
            Require(boss.Health == health && boss.Exposed && boss.RequiredHue == hue && game.Toast.Contains("Wrong color"),
                "Wrong-color projectile does no damage, explains the mismatch and leaves the window open");
            game.ReplenishCandy();
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Count(x => x.hue == boss.RequiredHue) >= 2,
                "Refill guarantees required color even when floor slots are occupied by wrong colors");
            Destroy(pink.gameObject);
            yield return null;
            var covered = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
            for (int i = 0; i < covered.Length; i++)
                covered[i].transform.position = new Vector3(boss.transform.position.x + i * .1f, .65f, boss.transform.position.z);
            game.ReplenishCandy();
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Count(x => x.hue == boss.RequiredHue &&
                Vector3.ProjectOnPlane(x.transform.position - boss.transform.position, Vector3.up).magnitude >= 4 && x.transform.position.y < 1) >= 2,
                "Covered candy stock cannot block safe matching-color ground replenishment");
            foreach (var sweet in covered) Destroy(sweet.gameObject);
            yield return null;
            Time.timeScale = previousScale;
        }
        IEnumerator BossBattle()
        {
            var game = DreamGame.Instance;
            yield return ExpandedCourtyardChecks(game);
            HalloweenArtChecks(game);
            var player = game.player;
            Require(player.characterAnimation != null && player.characterAnimation.animator.isHuman,
                "Castle chapter uses Timmy humanoid character");
            player.automation = true;
            var lamp = game.lanterns[0];
            player.Teleport(lamp.safeCheckpoint);
            yield return new WaitForSeconds(.12f);
            Require(player.Interact(), "Castle entrance lantern restores approach");
            yield return null;
            var skin = lamp.GetComponent<LanternSkin>();
            Require(skin != null && skin.lit.activeSelf && !skin.unlit.activeSelf,
                "Castle lantern switches imported artwork on without changing the jump route");
            yield return JumpRouteChecks(game, lamp.path);
            player.Teleport(new Vector3(0, .1f, 6));
            yield return new WaitForSeconds(.3f);
            Require(game.boss.AwakeInArena, "Boss wakes when entering restored courtyard");
            Require(game.boss.SummonWavesStarted == 1 && game.boss.PendingSummons == 2 && game.boss.ActiveSummons == 0,
                "Awakening warns for exactly two enemies before the first summon wave materializes");
            game.Pause();
            yield return new WaitForSecondsRealtime(.3f);
            Require(game.boss.SummonWavesCompleted == 0 && game.boss.PendingSummons == 2,
                "Pausing freezes pending summons without spawning enemies");
            game.Resume(); Time.timeScale = 3;
            yield return SummonWaveChecks(game, 1);
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            Vector3 heroScreen = game.gameCamera.WorldToViewportPoint(player.transform.position + Vector3.up);
            Vector3 bossScreen = game.gameCamera.WorldToViewportPoint(game.boss.AimPosition);
            Require(InView(heroScreen) && InView(bossScreen), "Boss framing keeps player and pumpkin weak point visible");
            yield return Capture("04-perspective-arena");
            int bossHealth = game.boss.Health;
            Require(!game.boss.SecondPhase && game.boss.ChargesCompleted == 0, "Full-health boss has no second-phase charge");
            player.FireAt(game.boss.AimPosition);
            yield return new WaitForSeconds(1);
            Require(game.boss.Health == bossHealth, "Boss shell rejects arrows");
            player.Heal(5);
            player.Teleport(new Vector3(-9, .12f, 4));
            yield return new WaitForSeconds(.2f);
            Require(player.TryDash(Vector3.forward), "Grounded player can dash");
            int before = player.Health;
            Require(!player.Damage(1) && player.Health == before, "Dash grants damage immunity");
            yield return new WaitForSeconds(1.2f);
            before = player.Health;
            NightmareBomb.Spawn(player.transform.position, .25f);
            yield return new WaitForSeconds(.4f);
            Require(player.Health == before - 1, "Telegraphed bomb blast damages player within radius");
            player.Heal(5);
            bool testedColors = false;
            var observedHues = new HashSet<CandyHue>();
            float deadline = Time.realtimeSinceStartup + 120;
            int corner = 0;
            Vector3[] safe = { new Vector3(-9, .12f, 4), new Vector3(9, .12f, 4), new Vector3(8, .12f, 20), new Vector3(-8, .12f, 20) };
            float changeAt = Time.time;
            while (!game.boss.Defeated && Time.realtimeSinceStartup < deadline)
            {
                if (Time.time > changeAt)
                {
                    player.Teleport(safe[corner++ % safe.Length]);
                    changeAt = Time.time + .7f;
                }
                // Enter a fresh enough window for pickup, screenshot and projectile
                // travel, rather than asserting damage after an expired opening.
                float minimumWindow = !testedColors ? 3.4f : game.boss.SecondPhase ? 1.5f : 2.1f;
                if (game.boss.Exposed && game.boss.PhaseSecondsLeft >= minimumWindow)
                {
                    if (!testedColors)
                    {
                        yield return ColorMatchingChecks(game);
                        testedColors = true;
                    }
                    CandyHue cycleHue = game.boss.RequiredHue;
                    observedHues.Add(cycleHue);
                    game.ReplenishCandy();
                    Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Count(x => x.hue == cycleHue &&
                        Vector3.ProjectOnPlane(x.transform.position - game.boss.transform.position, Vector3.up).magnitude >= 4) >= 2,
                        "Boss replenishment maintains two reachable matching-color sweets: " + cycleHue);
                    var candy = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)
                        .Where(x => x.hue == cycleHue && Vector3.ProjectOnPlane(x.transform.position - game.boss.transform.position, Vector3.up).magnitude >= 4)
                        .OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).FirstOrDefault();
                    Require(candy != null, "Boss fight maintains collectible candy supply");
                    player.Teleport(candy.transform.position + Vector3.back * .7f);
                    yield return new WaitForSeconds(.1f);
                    Require(player.Interact(), "Candy pickup remains reachable in boss arena");
                    Require(player.HeldCandyHue == cycleHue, "Picked up candy matches the current boss hue " + cycleHue);
                    int hp = game.boss.Health;
                    if (hp == 5 || hp == 4)
                    {
                        game.gameCamera.GetComponent<DreamCamera>().Snap();
                        bool blocked = Physics.Linecast(game.gameCamera.transform.position, player.transform.position + Vector3.up,
                            out RaycastHit obstruction, ~(1 << 2), QueryTriggerInteraction.Ignore);
                        Debug.Log("CORNER_VIEW: " + cycleHue + " blocked=" + blocked + " object=" +
                            (blocked ? obstruction.collider.name : "none") + " camera=" + game.gameCamera.transform.position);
                        yield return Capture("04-color-" + cycleHue.ToString().ToLowerInvariant());
                        Require(!blocked, "Castle corner view keeps the candy-carrying player clear of roof occlusion: " + cycleHue);
                    }
                    if (hp == game.boss.maxHealth) yield return Capture("04-weak-point");
                    player.ThrowAt(game.boss.AimPosition);
                    yield return new WaitForSeconds(1.4f);
                    Require(game.boss.Health == hp - 1, "Real candy projectile damages exposed boss exactly once");
                    if (hp == 1) yield return PumpkinBurstChecks();
                    if (hp > game.boss.maxHealth / 2 + 1)
                        Require(!game.boss.SecondPhase && game.boss.ChargesCompleted == 0, "Bomb-only first phase stays active above half health");
                    if (hp == game.boss.maxHealth / 2 + 1)
                    {
                        Require(game.boss.SecondPhase, "Candy hit at exactly half health activates the second phase");
                        Require(game.boss.SummonWavesStarted == 2, "Half health triggers the second and final summon wave");
                        yield return SummonWaveChecks(game, 2);
                        yield return BossChargeChecks(game);
                        player.Heal(5);
                    }
                    if (!game.boss.Defeated) yield return Capture("04-boss-" + game.boss.Health);
                    changeAt = 0;
                }
                yield return null;
            }
            Require(observedHues.Count == 3, "Victory uses all three candy colors through gradual first-half introduction");
            Require(game.boss.Defeated && game.Phase == GamePhase.Victory, "Boss defeat leads to victory screen");
            Require(FindObjectsByType<NightmareBomb>(FindObjectsSortMode.None).Length == 0, "Victory clears pending bomb hazards");
            Require(game.boss.ChargesCompleted > 0 && game.boss.ChargeWarning == null,
                "Boss victory includes second-phase charges and leaves no charge warning");
            Require(game.boss.SummonWavesStarted == 2 && game.boss.SummonWavesCompleted == 2,
                "The entire boss fight summons exactly twice with no repeat waves");
            Require(game.boss.ActiveSummons == 0 && game.boss.PendingSummons == 0 && game.dragons.Length == 0,
                "Victory removes summoned enemies and pending portals from target registration");
            yield return VictoryMusicChecks();
            yield return VictoryFireworksChecks();
        }
        IEnumerator ExpandedCourtyardChecks(DreamGame game)
        {
            var floor = game.transform.Find("Pumpkin keep courtyard/Solid dream island").GetComponent<BoxCollider>();
            Physics.SyncTransforms();
            Require(Mathf.Abs(floor.bounds.size.x - 42) < .1f && Mathf.Abs(floor.bounds.size.z - 38) < .1f &&
                Mathf.Abs(floor.bounds.min.z + 2) < .1f,
                "Saved boss courtyard is 42 by 38 with the original jump-route landing edge preserved");
            var approach = game.transform.Find("Castle approach/Solid dream island").GetComponent<BoxCollider>();
            Require(Mathf.Abs(approach.bounds.size.x - 17) < .1f && Mathf.Abs(approach.bounds.size.z - 10) < .1f,
                "Castle approach island keeps its original size");
            var player = game.player; player.automation = true;
            Vector3 start = player.transform.position;
            foreach (Vector3 point in new[] { new Vector3(-16, .15f, 8), new Vector3(16, .15f, 8),
                new Vector3(-16, .15f, 29), new Vector3(16, .15f, 29) })
            {
                player.Teleport(point); yield return new WaitForSeconds(.15f);
                Require(player.IsGrounded && player.transform.position.y > -.1f,
                    "The real controller can stand in the enlarged courtyard: " + point);
            }
            player.Teleport(start);
        }
        IEnumerator PumpkinBurstChecks()
        {
            var game = DreamGame.Instance;
            var effect = FindFirstObjectByType<PumpkinBurst>();
            Require(game.boss.Defeated && !game.boss.pumpkin.gameObject.activeSelf && !game.boss.crown.gameObject.activeSelf,
                "Victory hides the intact pumpkin head and crown");
            Require(effect != null && effect.PieceCount >= 12 && effect.PieceCount <= 64,
                "The real pumpkin and crown surfaces split into a bounded number of fragments");
            Require(effect.GetComponentsInChildren<Collider>().Length == 0 && effect.GetComponentsInChildren<Rigidbody>().Length == 0,
                "Pumpkin fragments cannot damage or block the hero");
            Vector3 before = effect.transform.GetChild(0).position;
            yield return new WaitForSecondsRealtime(.2f);
            Require(Vector3.Distance(before, effect.transform.GetChild(0).position) > .2f,
                "Pumpkin shell fragments physically separate and arc away from the defeated head");
            yield return Capture("04-pumpkin-burst");
        }
        IEnumerator VictoryMusicChecks()
        {
            var audio = DreamGame.Instance.audioSystem;
            float deadline = Time.realtimeSinceStartup + 4;
            while ((audio.MusicTrack != "dream_celebration" || audio.PlayingMusicSources != 1 || audio.MusicGain < .1f) &&
                Time.realtimeSinceStartup < deadline) yield return null;
            Require(audio.MusicTrack == "dream_celebration" && audio.MusicPlaying && audio.PlayingMusicSources == 1 &&
                audio.MusicClip.name == "dream_celebration" && audio.MusicGain > .1f,
                "Victory crossfades from battle music into one audible looping celebration score");
        }
        IEnumerator VictoryFireworksChecks()
        {
            var game = DreamGame.Instance;
            var show = game.GetComponentInChildren<VictoryFireworks>();
            Require(game.Phase == GamePhase.Victory && show != null, "Victory starts one scene-owned fireworks show");
            var character = game.player.characterAnimation; var animator = character.animator;
            yield return new WaitForSecondsRealtime(.3f);
            Require(animator.GetFloat("Speed") == 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion") &&
                animator.GetCurrentAnimatorStateInfo(1).IsName("Relax") &&
                game.player.visual.GetComponentsInChildren<SkinnedMeshRenderer>().All(r => r.enabled),
                "Victory clears running, throw and bow poses and restores visible idle animation");
            var idleSkin = character.GetComponentInChildren<SkinnedMeshRenderer>();
            var idleBeforeMesh = new Mesh(); idleSkin.BakeMesh(idleBeforeMesh);
            yield return new WaitForSecondsRealtime(.45f);
            var idleAfterMesh = new Mesh(); idleSkin.BakeMesh(idleAfterMesh);
            Require(idleBeforeMesh.vertices.Where((v, i) => Vector3.Distance(v, idleAfterMesh.vertices[i]) > .0001f).Any(),
                "Victory idle visibly deforms Timmy's mesh instead of holding the final combat frame");
            Destroy(idleBeforeMesh); Destroy(idleAfterMesh);
            game.Win();
            float openingDeadline = Time.realtimeSinceStartup + 4;
            while ((show.BurstsCompleted < 2 || show.LiveParticles == 0) && Time.realtimeSinceStartup < openingDeadline)
                yield return null;
            Require(FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 1,
                "Repeated victory notification does not duplicate the celebration");
            Require(show.BurstsCompleted >= 2 && show.LiveParticles > 0,
                "Rockets launch and burst into live rendered sky particles: bursts=" + show.BurstsCompleted +
                ", particles=" + show.LiveParticles);
            var renderer = show.GetComponentInChildren<ParticleSystemRenderer>();
            Require(renderer.sharedMaterial != null && renderer.sharedMaterial.shader.isSupported,
                "Fireworks use a supported included glow shader in the built player");
            Require(show.GetComponentsInChildren<Collider>().Length == 0,
                "Celebration particles have no gameplay collision");
            Vector3 sky = game.gameCamera.WorldToViewportPoint(show.LastBurstPosition);
            Require(sky.z > 0 && sky.x > 0 && sky.x < 1 && sky.y > .4f && sky.y < 1,
                "Victory camera frames the sky bursts above the lower victory controls");
            yield return Capture("04-victory-fireworks");
            int count = show.BurstsCompleted;
            yield return new WaitForSecondsRealtime(6);
            // Late launches allow 2.6 seconds plus 1.15 seconds of rocket flight.
            // Observe a complete cadence rather than sampling a possible quiet gap.
            float visibleDeadline = Time.realtimeSinceStartup + 4.5f;
            while ((show.BurstsCompleted <= count || show.LiveParticles == 0) && Time.realtimeSinceStartup < visibleDeadline) yield return null;
            Require(show.BurstsCompleted > count && show.LiveParticles > 0 && show.LiveParticles <= 1800,
                "Fireworks continue beyond the particle-system duration within a fixed particle budget: previous=" + count +
                ", current=" + show.BurstsCompleted + ", particles=" + show.LiveParticles);
            float speed = Time.timeScale; Time.timeScale = 0;
            float idleBefore = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            count = show.BurstsCompleted;
            // Late cadence can be 2.6 seconds plus a 1.15-second rocket flight.
            float frozenDeadline = Time.realtimeSinceStartup + 4.5f;
            while ((show.BurstsCompleted <= count || show.LiveParticles == 0) && Time.realtimeSinceStartup < frozenDeadline)
                yield return null;
            Require(show.BurstsCompleted > count && show.LiveParticles > 0,
                "Victory celebration advances in real time even when gameplay time is frozen");
            Require(animator.updateMode == AnimatorUpdateMode.UnscaledTime &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime > idleBefore + .01f,
                "Timmy keeps playing idle throughout the fireworks even with gameplay time frozen");
            Require(FindObjectsByType<PumpkinBurst>(FindObjectsSortMode.None).Length == 0,
                "Pumpkin fragments finish dissipating and clean up their temporary meshes");
            Time.timeScale = speed;
            yield return Capture("04-victory-fireworks-later");
            var follow = game.gameCamera.GetComponent<DreamCamera>(); var hud = game.GetComponent<DreamHUD>();
            Vector3 cameraPosition = game.gameCamera.transform.position; Quaternion cameraRotation = game.gameCamera.transform.rotation;
            follow.enabled = false; hud.enabled = false;
            game.gameCamera.transform.position = game.player.transform.position + new Vector3(3, 2.5f, 4.5f);
            game.gameCamera.transform.LookAt(game.player.transform.position + Vector3.up * 1.1f);
            yield return Capture("04-victory-idle-closeup");
            game.gameCamera.transform.position = cameraPosition; game.gameCamera.transform.rotation = cameraRotation;
            follow.enabled = true; hud.enabled = true;
        }
        IEnumerator MusicChecks()
        {
            var game = DreamGame.Instance;
            var audio = game.audioSystem;
            foreach (string track in new[] { "dream_journey", "pumpkin_battle", "pumpkin_rage", "dream_celebration" })
            {
                var clip = Resources.Load<AudioClip>("Audio/Music/" + track);
                Require(clip != null && clip.length > 20 && clip.channels == 2,
                    "Original stereo background score imports and lasts more than twenty seconds: " + track);
            }
            yield return new WaitForSecondsRealtime(.2f);
            Require(audio.MusicTrack == "dream_journey" && audio.MusicClip != null && audio.MusicPlaying,
                "Exploration music plays automatically on normal start");
            float volume = audio.musicVolume;
            audio.musicVolume = 0;
            yield return new WaitForSecondsRealtime(1.6f);
            Require(audio.MusicGain == 0 && audio.MusicPlaying,
                "Setting background volume to zero silences the score without losing its playback timeline");
            audio.musicVolume = volume;
            yield return new WaitForSecondsRealtime(.4f);
            Require(audio.MusicGain > 0 && audio.MusicPlaying,
                "Raising background volume restores the current score without restarting the scene");
            audio.ToggleMute();
            Require(audio.Muted && AudioListener.volume == 0, "Mute silences background music and event audio together");
            audio.ToggleMute();
            Require(!audio.Muted && AudioListener.volume == 1, "Unmute restores music and event audio");
            game.Pause(); yield return null;
            int sample = audio.MusicTimeSamples;
            yield return new WaitForSecondsRealtime(.2f);
            Require(audio.MusicPaused && !audio.MusicPlaying && audio.MusicTimeSamples == sample,
                "Pause stops the music timeline instead of restarting the track");
            game.Resume(); yield return null;
            Require(!audio.MusicPaused && audio.MusicPlaying, "Resume continues the background score");
        }
        IEnumerator SummonCleanupChecks()
        {
            var previous = DreamGame.Instance;
            previous.audioSystem.ToggleMute();
            previous.Retry();
            yield return new WaitForSecondsRealtime(.6f);
            var game = DreamGame.Instance;
            Require(game.audioSystem.Muted && AudioListener.volume == 0,
                "Music mute preference survives retry and scene recreation");
            Require(FindObjectsByType<VictoryFireworks>(FindObjectsSortMode.None).Length == 0,
                "Retry removes the complete victory fireworks show");
            game.audioSystem.ToggleMute();
            Require(game.boss.SummonWavesStarted == 0 && game.dragons.Length == 0 && !game.boss.AwakeInArena,
                "Retry resets summon counters and removes all reinforcements from the previous fight");
            game.player.automation = true;
            game.player.Teleport(game.lanterns[0].safeCheckpoint);
            yield return new WaitForSeconds(.1f);
            Require(game.player.Interact(), "Cleanup regression restores the castle entrance lantern");
            game.player.Teleport(new Vector3(0, .12f, 6));
            yield return new WaitForSeconds(.2f);
            Require(game.boss.PendingSummons == 2, "Fresh fight starts two pending summon portals");
            game.Defeat(); yield return null;
            Require(game.boss.PendingSummons == 0 && game.dragons.Length == 0,
                "Defeat clears pending portals before they can materialize");
            yield return new WaitForSeconds(2.2f);
            Require(game.boss.SummonWavesCompleted == 0 && game.boss.ActiveSummons == 0,
                "A defeated fight never creates delayed reinforcement enemies");
            game.Retry(); yield return new WaitForSecondsRealtime(.6f);
            game = DreamGame.Instance; game.player.automation = true;
            game.player.Teleport(game.lanterns[0].safeCheckpoint);
            yield return new WaitForSeconds(.1f); game.player.Interact();
            game.player.Teleport(new Vector3(0, .12f, 6));
            yield return new WaitForSeconds(2.4f);
            Require(game.boss.ActiveSummons == 2, "Victory cleanup regression begins with two living summoned enemies");
            DreamProjectile.Spawn(new Vector3(0, 7, 7), Vector3.forward, false, true);
            game.Win(); yield return null;
            Require(game.boss.ActiveSummons == 0 && game.boss.PendingSummons == 0 && game.dragons.Length == 0 &&
                !FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Any(x => x.hostile),
                "Victory clears living reinforcements and their hostile projectiles without requiring extra kills");
        }
        IEnumerator SummonWaveChecks(DreamGame game, int wave)
        {
            var boss = game.boss; var player = game.player;
            int initialBombs = boss.BombsLaunched, initialCharges = boss.ChargesCompleted;
            float deadline = Time.time + 6;
            while (boss.SummonWavesCompleted < wave && Time.time < deadline) { player.Heal(5); yield return null; }
            Require(boss.SummonWavesCompleted == wave && boss.PendingSummons == 0,
                "Warned summon wave materializes and clears its portals: " + wave);
            var enemies = game.dragons.Where(x => x != null && x.bossSummon && !x.Defeated).ToArray();
            Require(enemies.Length == 2 && enemies.Any(x => x.kind == NightmareKind.Dragon) &&
                enemies.Any(x => x.kind == (wave == 1 ? NightmareKind.Ghost : NightmareKind.Demon)),
                "Wave has an arrow-targetable dragon and the appropriate small monster: " + wave);
            Require(enemies.All(x => x.modelAnimation != null && x.GetComponent<Collider>().enabled),
                "Summoned enemies have real Quaternius animated models and colliders");
            if (wave == 1)
            {
                var enemy = enemies.First(x => x.kind == NightmareKind.Ghost);
                Vector3 saved = enemy.transform.position;
                game.gameCamera.GetComponent<DreamCamera>().Snap();
                enemy.transform.position = boss.AimPosition + (game.gameCamera.transform.position - boss.AimPosition).normalized * 4;
                Physics.SyncTransforms();
                Vector3 cursor = game.gameCamera.WorldToScreenPoint(enemy.AimPosition);
                Vector3 bossCursor = game.gameCamera.WorldToScreenPoint(boss.AimPosition);
                Ray sight = game.gameCamera.ScreenPointToRay(cursor);
                Require(Vector2.Distance(cursor, bossCursor) < 100 &&
                    Physics.Raycast(sight, out RaycastHit direct, 120, ~(1 << 2), QueryTriggerInteraction.Ignore) &&
                    direct.collider.GetComponentInParent<FlyingDragon>() == enemy &&
                    Vector3.Distance(player.AimFromScreenPoint(cursor), direct.point) < .01f,
                    "A reticle aimed at a summon near the boss selects its actual surface without redirecting toward the boss");
                enemy.transform.position = saved; Physics.SyncTransforms();
            }
            Require(game.audioSystem.MusicTrack == (wave == 1 ? "pumpkin_battle" : "pumpkin_rage"),
                "Music transitions to the appropriate boss stage: " + wave);
            foreach (var enemy in enemies)
                Require(Physics.RaycastAll(enemy.home + Vector3.up, Vector3.down, 8, ~(1 << 2), QueryTriggerInteraction.Ignore)
                    .Any(floor => floor.normal.y > .8f && floor.point.y < 1 &&
                        floor.collider.GetComponentInParent<FlyingDragon>() == null && floor.collider.GetComponentInParent<PlayerMotor>() == null),
                    "Summoned enemy home is over actual courtyard floor: " + enemy.kind);
            yield return Capture("04-summon-wave-" + wave);
            // Observe live concurrent attacks before removing them for charge isolation.
            deadline = Time.time + 10;
            float relocateAt = 0; bool right = false;
            while ((enemies.All(x => x.AttacksCompleted == 0) ||
                boss.BombsLaunched == initialBombs && boss.ChargesCompleted == initialCharges) && Time.time < deadline)
            {
                if (Time.time > relocateAt)
                {
                    right = !right; player.Teleport(new Vector3(right ? 8 : -8, .12f, 10));
                    relocateAt = Time.time + 1.1f;
                }
                player.Heal(5); yield return null;
            }
            Require(enemies.Any(x => x.AttacksCompleted > 0), "Summoned enemies actively attack the player: " + wave);
            Require(boss.BombsLaunched > initialBombs || boss.ChargesCompleted > initialCharges,
                "The pumpkin continues its own attacks while reinforcements are alive: " + wave);
            // Isolate reward assertions from automatic cycle recoloring and a moving
            // boss footprint; concurrent combat was observed above with the boss live.
            boss.enabled = false;
            foreach (var enemy in enemies)
            {
                int health = enemy.health;
                deadline = Time.time + 10;
                while (!enemy.Defeated && Time.time < deadline)
                {
                    player.Heal(5);
                    Vector3 position = enemy.AimPosition + Vector3.back * 3;
                    player.Teleport(new Vector3(Mathf.Clamp(position.x, -9, 9), .12f, Mathf.Clamp(position.z, 2, 23)));
                    player.FireAt(enemy.AimPosition);
                    yield return new WaitForSeconds(.4f);
                }
                Require(enemy.Defeated && enemy.health <= 0 && health >= 2,
                    "Real bow arrows defeat the summoned " + enemy.kind + " in wave " + wave);
                Require(enemy.DroppedCandy != null && enemy.DroppedCandy.hue == boss.RequiredHue &&
                    enemy.DroppedCandy.transform.position.y < 1 &&
                    Vector3.ProjectOnPlane(enemy.DroppedCandy.transform.position - boss.transform.position, Vector3.up).magnitude >= 4,
                    "Defeated reinforcement rewards matching candy on safe floor outside the boss footprint");
                Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Where(x => x != enemy.DroppedCandy).All(x =>
                    Vector3.ProjectOnPlane(x.transform.position - enemy.DroppedCandy.transform.position, Vector3.up).magnitude >= 2.5f),
                    "Reward candy does not overlap previous waves or existing different-color pickups");
            }
            foreach (var projectile in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None))
                if (projectile.hostile) Destroy(projectile.gameObject);
            player.Teleport(new Vector3(-9, .12f, 4)); player.Heal(5);
            boss.enabled = true;
            Require(boss.ActiveSummons == 0, "Both summons can be cleared without damaging the boss: " + wave);
        }
        IEnumerator BossChargeChecks(DreamGame game)
        {
            float previousScale = Time.timeScale; Time.timeScale = 1;
            var boss = game.boss;
            var player = game.player;
            float deadline = Time.time + 15;
            while (!boss.ChargeWarningActive && Time.time < deadline) yield return null;
            Require(boss.ChargeWarningActive && boss.ChargeWarning != null, "Half-health charge has a visible windup lane");
            CandyHue chargeHue = boss.RequiredHue;
            Vector3 start = boss.ChargeStart, target = boss.ChargeTarget;
            Vector3 midpoint = (start + target) * .5f;
            Vector3 perpendicular = Vector3.Cross((target - start).normalized, Vector3.up);
            Vector3 sideA = midpoint + perpendicular * 6, sideB = midpoint - perpendicular * 6;
            sideA.x = Mathf.Clamp(sideA.x, -10, 10); sideA.z = Mathf.Clamp(sideA.z, 3, 23);
            sideB.x = Mathf.Clamp(sideB.x, -10, 10); sideB.z = Mathf.Clamp(sideB.z, 3, 23);
            Vector3 safeSide = LaneDistance(sideA, start, target) > LaneDistance(sideB, start, target) ? sideA : sideB;
            safeSide.y = .12f;
            player.Teleport(safeSide);
            yield return new WaitForSeconds(.05f);
            Require(Vector3.Distance(boss.ChargeTarget, target) < .001f, "Charge direction remains locked after the player sidesteps");
            int hp = boss.Health;
            Require(!boss.Hit(true, boss.RequiredHue) && boss.Health == hp, "Candy cannot damage the boss during the charge windup");
            Vector3 paused = boss.transform.position;
            game.Pause();
            yield return new WaitForSecondsRealtime(.3f);
            Require(boss.ChargeWarningActive && Vector3.Distance(boss.transform.position, paused) < .001f,
                "Pause preserves the charge windup without moving the boss");
            game.Resume(); Time.timeScale = 1;
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("04-charge-warning");
            player.Teleport(new Vector3(midpoint.x, .12f, midpoint.z));
            yield return new WaitForSeconds(.05f);
            int health = player.Health, count = boss.ChargesCompleted;
            Debug.Log("CHARGE_CONTACT_START: phase=" + boss.Phase + " invulnerable=" + player.Invulnerable +
                " player=" + player.transform.position + " boss=" + boss.transform.position);
            Require(boss.ChargeWarningActive && !player.Invulnerable,
                "Contact regression places a vulnerable player in the lane before the charge starts");
            bool moved = false;
            deadline = Time.time + 4;
            while ((boss.ChargeWarningActive || boss.Phase == BossPhase.Charging) && Time.time < deadline)
            {
                moved |= Vector3.Distance(boss.transform.position, start) > .5f;
                yield return null;
            }
            Require(moved && boss.ChargesCompleted == count + 1 && Vector3.Distance(boss.transform.position, target) < .1f,
                "The real boss charges forward along the locked lane and stops at its target");
            Require(player.Health == health - 1, "Swept charge contact damages the player exactly once");
            Require(boss.RequiredHue == chargeHue, "Required candy color stays fixed through charge and recovery");
            Require(boss.Phase == BossPhase.Recovering && boss.Exposed && boss.ChargeWarning == null,
                "Completed charge opens a stationary recovery weak point and clears the lane");
            var dais = game.transform.Find("Boss dais");
            Require(dais != null && Vector3.Distance(dais.position, start) < .1f,
                "The stone dais remains in place while the pumpkin head charges");
            Require(target.x >= boss.arenaMin.x + 2 && target.x <= boss.arenaMax.x - 2 &&
                target.z >= boss.arenaMin.y + 2.5f && target.z <= boss.arenaMax.y - 2 &&
                Physics.Raycast(target + Vector3.up * 2, Vector3.down, 4, ~(1 << 2), QueryTriggerInteraction.Ignore),
                "Charge ends on safe courtyard ground away from the jump platforms");
            player.Teleport(safeSide); player.Heal(5);
            yield return new WaitForSeconds(.7f);
            Require(Vector3.Dot(-boss.transform.forward, Vector3.ProjectOnPlane(player.transform.position - boss.transform.position, Vector3.up).normalized) > .95f,
                "Recovery turns the matching-color face toward the player so the weak point remains visible");
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("04-charge-recovery");
            deadline = Time.time + 6;
            while ((boss.Phase == BossPhase.Recovering || boss.Phase == BossPhase.Returning) && Time.time < deadline) yield return null;
            Require(boss.Phase != BossPhase.Recovering && boss.Phase != BossPhase.Returning,
                "Charge recovery and return complete before timeout");
            Require(Vector3.Distance(boss.transform.position, start) < .1f && !boss.Exposed,
                "Unanswered recovery returns the boss home with a closed shell");
            deadline = Time.time + 15;
            float relocateAt = 0; bool left = false;
            while (!boss.ChargeWarningActive && Time.time < deadline)
            {
                if (Time.time > relocateAt)
                {
                    left = !left; player.Teleport(new Vector3(left ? -9 : 9, .12f, 4));
                    relocateAt = Time.time + .55f;
                }
                yield return null;
            }
            Require(boss.ChargeWarningActive && FindObjectsByType<NightmareBomb>(FindObjectsSortMode.None).Length == 0,
                "Later bomb volleys finish before the next second-phase charge begins");
            start = boss.ChargeStart; target = boss.ChargeTarget; midpoint = (start + target) * .5f;
            perpendicular = Vector3.Cross((target - start).normalized, Vector3.up);
            sideA = midpoint + perpendicular * 6; sideB = midpoint - perpendicular * 6;
            sideA.x = Mathf.Clamp(sideA.x, -10, 10); sideA.z = Mathf.Clamp(sideA.z, 3, 23);
            sideB.x = Mathf.Clamp(sideB.x, -10, 10); sideB.z = Mathf.Clamp(sideB.z, 3, 23);
            safeSide = LaneDistance(sideA, start, target) > LaneDistance(sideB, start, target) ? sideA : sideB;
            safeSide.y = .12f; player.Teleport(safeSide);
            health = player.Health; count = boss.ChargesCompleted;
            Require(!player.Invulnerable && LaneDistance(safeSide, start, target) > 4,
                "Sideways charge avoidance starts outside the lane without damage immunity");
            deadline = Time.time + 4;
            while (boss.ChargesCompleted == count && Time.time < deadline) yield return null;
            Require(boss.ChargesCompleted == count + 1 && player.Health == health,
                "Moving outside the locked lane avoids the complete charge without damage");
            Time.timeScale = previousScale;
        }
        float LaneDistance(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 segment = Vector3.ProjectOnPlane(end - start, Vector3.up);
            Vector3 offset = Vector3.ProjectOnPlane(point - start, Vector3.up);
            float along = segment.sqrMagnitude > .001f ? Mathf.Clamp01(Vector3.Dot(offset, segment) / segment.sqrMagnitude) : 0;
            return (offset - segment * along).magnitude;
        }
        IEnumerator CharacterChecks()
        {
            var game = DreamGame.Instance;
            var player = game.player;
            var character = player.characterAnimation;
            Require(character != null && character.animator.isHuman && character.animator.avatar.isValid,
                "Timmy has a valid humanoid avatar and animation adapter");
            Require(!character.animator.applyRootMotion,
                "Timmy root motion is disabled so the CharacterController owns movement");
            var skin = character.GetComponentInChildren<SkinnedMeshRenderer>();
            Require(skin != null && skin.sharedMaterials.All(x => x != null && x.mainTexture != null),
                "Timmy skin uses imported color textures");
            player.automation = true;
            yield return new WaitForSeconds(.2f);
            var before = new Mesh(); skin.BakeMesh(before);
            Vector3 start = player.transform.position;
            player.automationMove = Vector3.forward;
            yield return new WaitForSeconds(.3f);
            var after = new Mesh(); skin.BakeMesh(after);
            Require(character.animator.GetFloat("Speed") > 4 && before.vertices.Where((v, i) => Vector3.Distance(v, after.vertices[i]) > .005f).Any(),
                "Moving Timmy plays an imported animation and deforms the mesh");
            Require(Vector3.Distance(start, player.transform.position) > 1 && player.transform.position.y > -.3f,
                "Animated Timmy moves over the floor using the original controller");
            Destroy(before); Destroy(after);
            player.automationMove = Vector3.zero;
            player.Teleport(start);
            player.visual.rotation = Quaternion.LookRotation(Vector3.back);
            yield return new WaitForSeconds(.2f);
            Debug.Log("TIMMY_RUNTIME_BOUNDS: " + skin.bounds + " feet=" + character.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
            Require(skin.bounds.size.y > 1.7f && skin.bounds.size.y < 3 && Vector3.Distance(skin.bounds.center, player.transform.position + Vector3.up) < 1,
                "Timmy is visible at player scale and centered on the movement collider");
            Require(Vector3.Distance(character.bow.position, character.LeftHand.position) < .2f,
                "Bow follows Timmy's animated left hand");
            player.PickCandy();
            yield return new WaitForSeconds(.1f);
            Require(player.heldCandy.activeSelf && Vector3.Distance(player.heldCandy.transform.position, character.RightHand.position) < .2f,
                "Carried candy follows Timmy's animated right hand");
            var follow = game.gameCamera.GetComponent<DreamCamera>();
            follow.enabled = false;
            Vector3 savedCamera = game.gameCamera.transform.position;
            Quaternion savedRotation = game.gameCamera.transform.rotation;
            float savedFov = game.gameCamera.fieldOfView;
            var hud = game.GetComponent<DreamHUD>(); hud.enabled = false;
            game.gameCamera.transform.position = start + new Vector3(3.3f, 2.6f, -5);
            game.gameCamera.transform.LookAt(start + Vector3.up * 1.1f);
            game.gameCamera.fieldOfView = 40;
            yield return Capture("00-timmy-closeup");
            player.ThrowAt(start + Vector3.forward * 7 + Vector3.up);
            yield return new WaitForSeconds(.08f);
            Require(character.animator.GetCurrentAnimatorStateInfo(1).IsName("Throw") || character.animator.GetNextAnimatorStateInfo(1).IsName("Throw"),
                "Candy throw starts Timmy's masked throw animation");
            Require(!player.heldCandy.activeSelf && !player.CarryingCandy,
                "Throwing removes candy from the animated hand");
            yield return new WaitForSeconds(.7f);
            player.FireAt(start + Vector3.forward * 8 + Vector3.up);
            yield return new WaitForSeconds(.08f);
            Require(character.animator.GetCurrentAnimatorStateInfo(1).IsName("BowAim") || character.animator.GetNextAnimatorStateInfo(1).IsName("BowAim"),
                "Arrow shot starts Timmy's bow aiming animation");
            RequireForwardBowPose(player, "forward");
            var arrow = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).First(x => !x.candy && !x.hostile);
            Require(Vector3.Cross(arrow.transform.position - character.BowGripPosition, arrow.velocity.normalized).magnitude < .15f,
                "Real arrow trajectory starts on the line of the forward bow grip");
            yield return Capture("00-timmy-bow");
            game.gameCamera.transform.position = start + new Vector3(3.5f, 2.5f, 4.5f);
            game.gameCamera.transform.LookAt(start + Vector3.up * 1.3f);
            yield return new WaitForSeconds(.12f);
            Require(player.FireAt(start + Vector3.forward * 8 + Vector3.up), "Front pose capture uses a fresh arrow shot");
            yield return Capture("00-bow-forward-front");
            yield return new WaitForSeconds(.4f);
            player.FireAt(start + Vector3.right * 8 + Vector3.up);
            yield return new WaitForSeconds(.08f);
            RequireForwardBowPose(player, "right-facing");
            yield return new WaitForSeconds(.4f);
            player.automationMove = Vector3.back;
            player.FireAt(start + Vector3.back * 8 + Vector3.up * 4);
            yield return new WaitForSeconds(.08f);
            RequireForwardBowPose(player, "moving and aiming upward");
            player.automationMove = Vector3.zero;
            yield return new WaitForSeconds(.6f);
            Require(character.animator.GetCurrentAnimatorStateInfo(1).IsName("Relax"),
                "Upper-body action returns to locomotion without leaving a frozen pose");
            game.gameCamera.transform.position = savedCamera;
            game.gameCamera.transform.rotation = savedRotation;
            game.gameCamera.fieldOfView = savedFov;
            follow.enabled = true; hud.enabled = true;
        }
        void RequireForwardBowPose(PlayerMotor player, string context)
        {
            var character = player.characterAnimation;
            Vector3 left = player.visual.InverseTransformPoint(character.LeftHand.position);
            Vector3 right = player.visual.InverseTransformPoint(character.RightHand.position);
            Debug.Log("FORWARD_BOW_POSE: " + context + " left=" + left + " right=" + right);
            Require(left.z > .4f && Mathf.Abs(left.x) < .16f,
                "Left bow hand extends in front of the character rather than sideways: " + context);
            Require(right.x > .07f && right.z < left.z - .25f && right.y > 1.2f,
                "Right drawing hand remains near the face behind the bow: " + context);
            Vector3 grip = character.bow.TransformPoint(character.bow.GetComponent<LineRenderer>().GetPosition(4));
            Require(Vector3.Distance(grip, character.LeftHand.position) < .12f,
                "Bow stays attached to the forward left hand: " + context);
        }
        bool InView(Vector3 point) { return point.z > 0 && point.x > .05f && point.x < .95f && point.y > .05f && point.y < .95f; }
        IEnumerator JumpRouteChecks(DreamGame game, DreamPath path)
        {
            var player = game.player;
            // The traversal regression measures falls. Isolate it from random enemy
            // projectile damage; enemy attacks have their own live combat checks.
            var activeEnemies = game.dragons.Where(x => x != null && x.enabled && !x.Defeated).ToArray();
            foreach (var enemy in activeEnemies) enemy.enabled = false;
            foreach (var projectile in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None))
                if (projectile.hostile) Destroy(projectile.gameObject);
            int chapter = game.levelIndex + 1;
            Require(path.IsJumpRoute && path.Revealed && path.jumpPlatforms.Length == (chapter == 2 || chapter == 3 ? 3 : 2),
                "Lantern reveals the chapter's round jumping route " + chapter);
            Require(path.solid.GetComponentsInChildren<BoxCollider>().Length == 0,
                "Jump route has no invisible continuous bridge floor " + chapter);
            var gap = (path.jumpPlatforms[0].position + path.jumpPlatforms[1].position) * .5f;
            Require(!Physics.Raycast(gap + Vector3.up * 4, Vector3.down, 12, ~(1 << 2), QueryTriggerInteraction.Ignore),
                "Space between round platforms is a real fallable gap " + chapter);
            player.automationMove = Vector3.zero;
            player.Teleport(path.jumpEntry.position);
            yield return new WaitForSeconds(1.6f);
            Require(player.IsGrounded, "Jump route starts on safe island ground " + chapter);
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("0" + chapter + "-round-platform-route");
            if (chapter == 1)
            {
                int beforeWalking = player.Health;
                float walkDeadline = Time.time + 3.2f;
                var walkTargets = path.jumpPlatforms.Select(x => x.position).Concat(new[] { path.jumpExit.position }).ToArray();
                int nextWalkTarget = 0;
                while (Time.time < walkDeadline && player.Health == beforeWalking)
                {
                    Vector3 offset = Vector3.ProjectOnPlane(walkTargets[nextWalkTarget] - player.transform.position, Vector3.up);
                    if (offset.magnitude < .1f && nextWalkTarget < walkTargets.Length - 1) nextWalkTarget++;
                    player.automationMove = offset.magnitude > .06f ? offset.normalized *
                        Mathf.Clamp01(offset.magnitude / (player.moveSpeed * Time.deltaTime)) : Vector3.zero;
                    yield return null;
                }
                player.automationMove = Vector3.zero;
                Require(player.Health == beforeWalking - 1,
                    "Walking across the tutorial's circular platforms without jumping falls into a gap");
                player.Heal(5);
                player.Teleport(path.jumpEntry.position);
                yield return new WaitForSeconds(.2f);
            }
            var targets = path.jumpPlatforms.Select(x =>
            {
                var floor = x.GetComponentInChildren<MeshCollider>();
                Require(floor != null && floor.enabled, "Round platform has flat mesh collision " + chapter);
                return new Vector3(x.position.x, floor.bounds.max.y + .04f, x.position.z);
            }).Concat(new[] { path.jumpExit.position }).ToArray();
            int health = player.Health;
            for (int i = 0; i < targets.Length; i++)
            {
                Vector3 target = targets[i];
                Require(player.IsGrounded && player.TryJump(), "Grounded route jump launches " + chapter + " hop " + (i + 1));
                bool leftFloor = false;
                bool captured = false;
                float deadline = Time.time + 2;
                while (Time.time < deadline)
                {
                    if (!player.IsGrounded) leftFloor = true;
                    if (leftFloor && player.IsGrounded) break;
                    if (chapter == 3 && i < path.jumpPlatforms.Length)
                        target = path.jumpPlatforms[i].position + Vector3.up * .04f;
                    Vector3 offset = Vector3.ProjectOnPlane(target - player.transform.position, Vector3.up);
                    player.automationMove = offset.magnitude > .06f ? offset.normalized *
                        Mathf.Clamp01(offset.magnitude / (player.moveSpeed * Time.deltaTime)) : Vector3.zero;
                    if (chapter != 3 && i == 0 && !captured && offset.magnitude < .15f && player.VerticalSpeed < 0)
                    {
                        captured = true;
                        player.automationMove = Vector3.zero;
                        yield return Capture("0" + chapter + "-round-platform-jump");
                    }
                    yield return null;
                }
                player.automationMove = Vector3.zero;
                if (chapter == 3 && i < path.jumpPlatforms.Length) target = path.jumpPlatforms[i].position + Vector3.up * .04f;
                Require(leftFloor && player.IsGrounded && Vector3.ProjectOnPlane(player.transform.position - target, Vector3.up).magnitude < .25f
                    && Mathf.Abs(player.transform.position.y - target.y) < .18f,
                    "Real controller jump lands on the next round platform or exit island " + chapter + " hop " + (i + 1));
                yield return new WaitForSeconds(.12f);
            }
            Require(player.Health == health, "All round-platform hops finish without fall damage " + chapter);
            if (chapter == 1)
            {
                player.Teleport(gap + Vector3.up * 2);
                yield return new WaitForSeconds(1.5f);
                Require(player.Health == health - 1 && Vector3.Distance(player.transform.position, game.Checkpoint) < .3f,
                    "A missed tutorial platform returns the player to the lantern checkpoint");
                player.Heal(5);
                player.Teleport(path.jumpExit.position);
                yield return new WaitForSeconds(.2f);
            }
            foreach (var enemy in activeEnemies) if (enemy != null) enemy.enabled = true;
        }
        IEnumerator TutorialSupplyChecks()
        {
            var game = DreamGame.Instance;
            var player = game.player;
            Vector3 start = player.transform.position;
            player.automation = true;
            Require(game.LitCount == 0, "Tutorial refill regression starts before any lantern is lit");
            var sweet = FindFirstObjectByType<CandyPickup>();
            player.Teleport(sweet.transform.position + Vector3.back * .8f);
            yield return new WaitForSeconds(.15f);
            Require(player.Interact() && player.CarryingCandy, "Tutorial candy is picked up before testing held-candy refill");
            player.Teleport(start);
            foreach (var spare in FindObjectsByType<CandyPickup>(FindObjectsSortMode.None)) Destroy(spare.gameObject);
            game.Pause();
            yield return new WaitForSecondsRealtime(.5f);
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == 0,
                "Pause freezes the tutorial candy refill timer");
            game.Resume();
            yield return new WaitForSeconds(3.2f);
            Require(player.CarryingCandy && FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == 2,
                "Tutorial refills both empty spots within three seconds while candy is held and lanterns are unlit");
            yield return new WaitForSeconds(3.2f);
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == 2,
                "Repeated tutorial refill ticks do not stack extra candy at occupied spots");
            var unrelated = new List<CandyPickup>();
            for (int i = 0; i < 4; i++) unrelated.Add(DreamWorld.Candy(game.transform, new Vector3(50 + i * 3, .65f, 54)));
            foreach (var item in FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Where(x => !unrelated.Contains(x))) Destroy(item.gameObject);
            yield return new WaitForSeconds(3.2f);
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Length == 6,
                "Candy elsewhere cannot prevent the tutorial's two designated spots from refilling");
            foreach (var item in unrelated) Destroy(item.gameObject);
            Require(player.ThrowAt(new Vector3(-20, 1, 53)), "Held tutorial test candy can be thrown away");
            yield return new WaitForSeconds(1);
            player.Teleport(start);
            yield return new WaitForSeconds(.2f);
        }
        IEnumerator JumpChecks()
        {
            var game = DreamGame.Instance;
            var player = game.player;
            var character = player.characterAnimation;
            player.automation = true;
            yield return new WaitForSeconds(.25f);
            Vector3 start = player.transform.position;
            Require(player.IsGrounded, "Player settles on the ground before jumping");
            game.Pause();
            Require(!player.TryJump(), "Pause prevents jump input");
            game.Resume();
            var skin = character.GetComponentInChildren<SkinnedMeshRenderer>();
            var before = new Mesh(); skin.BakeMesh(before);
            Require(player.TryJump() && player.VerticalSpeed > 0 && player.IsJumping,
                "Grounded jump starts upward controller velocity");
            Require(!player.TryJump(), "Repeated input cannot launch a second jump before leaving the floor");
            yield return new WaitForSeconds(.025f);
            Require(character.JumpStage == JumpAnimationStage.Takeoff, "Jump has a distinct takeoff animation stage");
            var camera = game.gameCamera;
            var follow = camera.GetComponent<DreamCamera>();
            Vector3 savedCamera = camera.transform.position;
            Quaternion savedRotation = camera.transform.rotation;
            float savedFov = camera.fieldOfView;
            follow.enabled = false;
            var hud = game.GetComponent<DreamHUD>(); hud.enabled = false;
            camera.transform.position = start + new Vector3(3.5f, 3.5f, -6);
            camera.transform.LookAt(start + Vector3.up * 1.6f); camera.fieldOfView = 45;
            yield return new WaitForSeconds(.15f);
            Require(!player.IsGrounded && player.transform.position.y > start.y + 1 && character.JumpStage == JumpAnimationStage.Rising,
                "Jump rises off the floor with the ascending animation");
            Require(!player.TryJump(), "Airborne jump input cannot produce a double jump");
            var after = new Mesh(); skin.BakeMesh(after);
            Require(before.vertices.Where((v, i) => Vector3.Distance(v, after.vertices[i]) > .02f).Any(),
                "Jump animation deforms the skin independently of the upward root movement");
            var leftKnee = character.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            var leftFoot = character.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Require(Vector3.Angle(leftKnee.position - player.leftLeg.position, leftFoot.position - leftKnee.position) > 35,
                "Airborne jump visibly bends the leg instead of sliding the standing pose upward");
            RequireRelaxedJumpHands(character, "rising");
            yield return Capture("00-jump-rising");
            float apex = player.transform.position.y;
            float deadline = Time.time + 2;
            bool falling = false;
            while (!player.IsGrounded && Time.time < deadline)
            {
                apex = Mathf.Max(apex, player.transform.position.y);
                if (!falling && player.VerticalSpeed < -2)
                {
                    falling = true;
                    Require(character.JumpStage == JumpAnimationStage.Falling, "Descending jump uses the falling animation stage");
                    RequireRelaxedJumpHands(character, "falling");
                    yield return Capture("00-jump-falling");
                }
                yield return null;
            }
            Require(apex - start.y > 1.7f && apex - start.y < 2.5f,
                "Actual jump height stays near the configured 2.2 units");
            Require(player.IsGrounded && !player.IsJumping && Mathf.Abs(player.transform.position.y - start.y) < .15f,
                "Jump lands back on the collidable floor and resets the jump state");
            Require(character.JumpStage == JumpAnimationStage.Landing, "Touching the floor starts landing compression");
            yield return new WaitForSeconds(.07f);
            yield return Capture("00-jump-landing");
            yield return new WaitForSeconds(.15f);
            Require(character.JumpStage == JumpAnimationStage.None, "Landing animation returns to idle or running");
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Temporary jumping head collision test";
            ceiling.transform.position = start + Vector3.up * 3;
            ceiling.transform.localScale = new Vector3(3, .3f, 3);
            Physics.SyncTransforms();
            Require(player.TryJump(), "Landing enables the next jump");
            yield return new WaitForSeconds(.2f);
            var controller = player.GetComponent<CharacterController>();
            Require(player.VerticalSpeed <= 0 && player.transform.position.y + controller.center.y + controller.height / 2 <= ceiling.GetComponent<Collider>().bounds.min.y + .1f,
                "Head collision stops the ascent before passing through a ceiling");
            Destroy(ceiling);
            yield return new WaitForSeconds(.8f);
            player.PickCandy();
            player.automationMove = Vector3.forward;
            Require(player.TryJump(), "Running player can jump while carrying candy");
            yield return new WaitForSeconds(.12f);
            Require(Vector3.Distance(start, player.transform.position) > .5f && player.CarryingCandy && player.heldCandy.activeSelf,
                "Airborne movement keeps the carried candy attached and available");
            Require(player.FireAt(start + Vector3.forward * 10 + Vector3.up * 2), "Bow can fire while jumping");
            yield return new WaitForSeconds(.06f);
            RequireForwardBowPose(player, "airborne jump shot");
            Require(!player.Invulnerable && player.Damage(1), "Jumping alone does not grant dash damage immunity");
            player.Heal(5);
            player.ThrowAt(start + Vector3.right * 8 + Vector3.up);
            player.automationMove = Vector3.zero;
            yield return new WaitForSeconds(1);
            player.Teleport(start);
            Destroy(before); Destroy(after);
            camera.transform.position = savedCamera; camera.transform.rotation = savedRotation; camera.fieldOfView = savedFov;
            follow.enabled = true; hud.enabled = true;
            yield return new WaitForSeconds(.25f);
        }
        void RequireRelaxedJumpHands(TimmyAnimation character, string stage)
        {
            foreach (bool left in new[] { true, false })
            {
                var upper = character.animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
                var elbow = character.animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
                var hand = left ? character.LeftHand : character.RightHand;
                var knuckle = character.animator.GetBoneTransform(left ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
                string side = left ? "left" : "right";
                Debug.Log("JUMP_HAND_GEOMETRY: " + stage + " " + side + " shoulder=" + character.transform.InverseTransformPoint(upper.position) +
                    " elbow=" + character.transform.InverseTransformPoint(elbow.position) + " hand=" + character.transform.InverseTransformPoint(hand.position) +
                    " wristAngle=" + Vector3.Angle(knuckle.position - hand.position, hand.position - elbow.position));
                Require(hand.position.y < upper.position.y - .12f,
                    "Jump keeps the " + side + " hand below its shoulder: " + stage);
                Require(Vector3.Angle(knuckle.position - hand.position, hand.position - elbow.position) < 35,
                    "Jump keeps the " + side + " wrist aligned with the forearm: " + stage);
            }
        }
        IEnumerator JumpPoseReview()
        {
            var game = DreamGame.Instance; var player = game.player; var character = player.characterAnimation;
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>(); var hud = game.GetComponent<DreamHUD>();
            player.automation = true; player.automationMove = Vector3.zero; player.visual.rotation = Quaternion.identity;
            yield return new WaitForSeconds(.6f);
            Vector3 start = player.transform.position, cameraPosition = camera.transform.position;
            Quaternion cameraRotation = camera.transform.rotation; float fov = camera.fieldOfView;
            follow.enabled = false; hud.enabled = false; camera.fieldOfView = 36;
            Require(player.TryJump(), "Pose review starts a real grounded jump");
            yield return new WaitForSeconds(.18f);
            Time.timeScale = .08f;
            string[] names = { "front", "side", "behind" };
            Vector3[] offsets = { new Vector3(2.1f, 1.5f, 4.8f), new Vector3(4.8f, 1.5f, .8f), new Vector3(2.1f, 1.5f, -4.8f) };
            for (int i = 0; i < names.Length; i++)
            {
                camera.transform.position = player.transform.position + offsets[i];
                camera.transform.LookAt(player.transform.position + Vector3.up * 1.15f);
                yield return Capture("00-jump-hands-" + names[i]);
            }
            RequireRelaxedJumpHands(character, "multi-view jump review");
            Require(Vector3.Distance(character.bow.position, character.LeftHand.position) < .2f,
                "Bow stays attached to the lowered jumping hand");
            Time.timeScale = 1; yield return new WaitForSeconds(1);
            player.PickCandy(); Require(player.TryJump(), "Carried-candy pose review starts a real jump");
            yield return new WaitForSeconds(.18f); Time.timeScale = .12f;
            camera.transform.position = player.transform.position + offsets[0];
            camera.transform.LookAt(player.transform.position + Vector3.up * 1.15f);
            yield return Capture("00-jump-hands-candy");
            Require(player.CarryingCandy && Vector3.Distance(player.heldCandy.transform.position, character.RightHand.position) < .2f,
                "Jump arm correction preserves the candy-carry hand and prop attachment");
            Require(player.FireAt(player.transform.position + Vector3.forward * 12 + Vector3.up * 1.5f),
                "Pose review can shoot an arrow during a jump");
            yield return new WaitForSeconds(.07f); RequireForwardBowPose(player, "corrected jump arms");
            yield return Capture("00-jump-hands-bow");
            Require(player.ThrowAt(player.transform.position + Vector3.forward * 9 + Vector3.up),
                "Pose review can throw candy during a jump");
            yield return new WaitForSeconds(.07f); yield return Capture("00-jump-hands-throw");
            Time.timeScale = 1; yield return new WaitForSeconds(1.2f);
            Require(!player.CarryingCandy && character.JumpStage == JumpAnimationStage.None,
                "Jump and throw return to normal grounded locomotion");
            player.Teleport(start);
            camera.transform.position = cameraPosition; camera.transform.rotation = cameraRotation; camera.fieldOfView = fov;
            follow.enabled = true; hud.enabled = true;
            yield return new WaitForSeconds(.2f);
        }
        IEnumerator CameraChecks()
        {
            var game = DreamGame.Instance;
            var camera = game.gameCamera;
            var follow = camera.GetComponent<DreamCamera>();
            var player = game.player;
            player.automation = true;
            follow.ResetView(); follow.Snap();
            Require(!camera.orthographic && camera.fieldOfView >= 54 && camera.fieldOfView <= 65,
                "Gameplay uses a perspective third-person camera");
            Vector3 feet = camera.WorldToViewportPoint(player.transform.position);
            Vector3 head = camera.WorldToViewportPoint(player.transform.position + Vector3.up * 2.1f);
            Require(InView(feet) && InView(head) && head.y - feet.y > .15f,
                "Follow camera shows the whole character at a readable screen size");
            Vector3 torso = camera.WorldToScreenPoint(player.transform.position + Vector3.up * 1.2f);
            Require(Mathf.Abs(torso.x - Screen.width * .5f) > 35,
                "Over-shoulder framing keeps Timmy clear of the central reticle");
            Require(camera.transform.position.y - player.transform.position.y < 7,
                "Follow camera is lower than the previous overhead view");
            yield return Capture("00-perspective-follow");
            Vector3 oldForward = camera.transform.forward;
            follow.RotateView(45, -5); follow.Snap();
            Require(Vector3.Angle(oldForward, camera.transform.forward) > 30,
                "Orbit changes the actual view around the player");
            Vector3 start = player.transform.position;
            player.automationMove = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            yield return new WaitForSeconds(.25f);
            player.automationMove = Vector3.zero;
            Require(Vector3.Dot(player.transform.position - start, camera.transform.forward) > 1,
                "Controller movement remains relative to the rotated camera");
            player.Teleport(start);
            follow.ResetView(); follow.Snap();
            float before = Vector3.Distance(camera.transform.position, player.transform.position + Vector3.up * 1.2f);
            follow.ZoomView(3);
            yield return new WaitForSeconds(.3f);
            Require(Vector3.Distance(camera.transform.position, player.transform.position + Vector3.up * 1.2f) < before - 1,
                "Zoom brings the actual camera closer to Timmy");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Temporary camera obstruction test";
            wall.transform.position = player.transform.position + Vector3.up * 2.5f - camera.transform.forward * 3.5f;
            wall.transform.localScale = new Vector3(8, 6, .5f);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(.1f);
            Require(Vector3.Distance(camera.transform.position, player.transform.position + Vector3.up * 1.2f) < 3.5f,
                "Camera pulls in before colliding with a solid wall");
            Destroy(wall);
            yield return new WaitForSeconds(.5f);
            Require(Vector3.Distance(camera.transform.position, player.transform.position + Vector3.up * 1.2f) > 6,
                "Camera eases back after the obstruction is removed");
            follow.ResetView(); follow.Snap();
            var rune = game.runes.First(x => !x.candyOnly);
            Vector3 screen = camera.WorldToScreenPoint(rune.AimPosition);
            Ray ray = camera.ScreenPointToRay(screen);
            Require(Physics.Raycast(ray, out RaycastHit hit, 100, ~(1 << 2)) && hit.collider.GetComponent<DreamRune>() == rune,
                "Perspective cursor ray can select the bow practice target");
        }
        IEnumerator MouseLookChecks()
        {
            var game = DreamGame.Instance; var player = game.player;
            var follow = game.gameCamera.GetComponent<DreamCamera>();
            player.automation = true; player.automationMove = Vector3.zero;
            Require(Cursor.lockState == CursorLockMode.Locked && !Cursor.visible,
                "Ordinary gameplay captures and hides the mouse for button-free camera control");
            follow.ResetView(); follow.Snap();
            Vector3 before = game.gameCamera.transform.forward;
            follow.ApplyLookInput(new Vector2(15, 4)); follow.Snap();
            Require(Mathf.Abs(Mathf.DeltaAngle(follow.yaw, 45)) < .01f && Mathf.Abs(follow.pitch - 15) < .01f &&
                Vector3.Angle(before, game.gameCamera.transform.forward) > 30,
                "Horizontal and vertical mouse deltas rotate the rendered view without holding an orbit button");
            follow.ApplyLookInput(new Vector2(0, 1000));
            Require(follow.pitch == -25, "Mouse can look up toward flying targets without flipping the camera");
            follow.ApplyLookInput(new Vector2(0, -1000));
            Require(follow.pitch == 65, "Downward mouse pitch stops before the camera flips");
            follow.ApplyLookInput(new Vector2(10000, 0));
            Require(follow.yaw >= 0 && follow.yaw < 360, "Long mouse rotations keep yaw bounded");
            game.Pause(); float pausedYaw = follow.yaw, pausedPitch = follow.pitch;
            follow.ApplyLookInput(new Vector2(20, 20));
            Require(Cursor.lockState == CursorLockMode.None && Cursor.visible && !game.MouseActionsReady &&
                follow.yaw == pausedYaw && follow.pitch == pausedPitch,
                "Pause frees the cursor, stops mouse attacks and ignores look input");
            game.Resume();
            Require(!game.MouseActionsReady, "Resume button cannot leak its click into an accidental bow shot");
            yield return null; yield return null;
            Require(Cursor.lockState == CursorLockMode.Locked && !Cursor.visible &&
                game.MouseActionsReady == Application.isFocused,
                "Resuming recaptures the mouse after click release and gates attacks on actual application focus");
            game.SendMessage("OnApplicationFocus", false);
            Require(game.Phase == GamePhase.Paused && Cursor.visible && Cursor.lockState == CursorLockMode.None,
                "Switching away from the game pauses play and releases the cursor");
            game.Resume();
            Vector3 start = player.transform.position;
            Quaternion facing = player.visual.rotation;
            // A temporary platform keeps the real controller and projectiles isolated
            // from map content while checking the shared central aiming ray.
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Temporary mouse aim test floor";
            platform.transform.position = new Vector3(60, 9.5f, 0);
            platform.transform.localScale = new Vector3(30, 1, 30);
            Physics.SyncTransforms(); player.Teleport(new Vector3(60, 10.05f, 0));
            follow.ResetView(); follow.RotateView(0, -25); follow.Snap();
            var probe = new GameObject("Temporary central-reticle bow target");
            probe.transform.position = game.gameCamera.ScreenPointToRay(player.AimScreenPoint).GetPoint(17.5f) - Vector3.up * 1.4f;
            var collider = probe.AddComponent<BoxCollider>(); collider.center = Vector3.up * 1.4f;
            collider.size = Vector3.one;
            var rune = probe.AddComponent<DreamRune>();
            rune.crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            rune.crystal.SetParent(probe.transform, false); rune.crystal.gameObject.layer = 2;
            Physics.SyncTransforms(); yield return null; yield return null;
            Vector2 center = new Vector2(Screen.width * .5f, Screen.height * .5f);
            Require(player.AimScreenPoint == center &&
                Vector3.Distance(player.AimPoint, player.AimFromScreenPoint(center)) < .05f &&
                Vector3.Distance(player.AimPoint, rune.AimPosition) < .8f,
                "Actual player aim and the HUD reticle share the screen center and select a physical target");
            Require(player.FireAt(player.AimPoint), "Bow fires along the actual central-reticle aim point");
            yield return new WaitForSeconds(.5f);
            Require(rune.Cleared, "Real central-reticle arrow hits and clears its physical target");
            Destroy(probe);
            var candyProbe = new GameObject("Temporary central-reticle candy target");
            candyProbe.transform.position = game.gameCamera.ScreenPointToRay(player.AimScreenPoint).GetPoint(17.5f) - Vector3.up * 1.4f;
            var candyCollider = candyProbe.AddComponent<BoxCollider>(); candyCollider.center = Vector3.up * 1.4f;
            candyCollider.size = Vector3.one;
            var candyRune = candyProbe.AddComponent<DreamRune>(); candyRune.candyOnly = true;
            candyRune.crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            candyRune.crystal.SetParent(candyProbe.transform, false); candyRune.crystal.gameObject.layer = 2;
            Physics.SyncTransforms(); yield return null; yield return null;
            Require(player.PickCandy(CandyHue.Blue) && player.ThrowAt(player.AimPoint),
                "Candy throws use the same central-reticle aim point");
            yield return new WaitForSeconds(1);
            Require(candyRune.Cleared, "Real central-reticle candy projectile clears a candy-only target");
            Destroy(candyProbe); Destroy(platform);
            player.Teleport(start); player.visual.rotation = facing; follow.ResetView(); follow.Snap();
            yield return null;
            yield return Capture("00-mouse-look-center-reticle");
        }
        IEnumerator AimAlignmentChecks()
        {
            var game = DreamGame.Instance; var player = game.player;
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>();
            Vector3 start = player.transform.position; Quaternion facing = player.visual.rotation;
            player.automation = true; player.automationMove = Vector3.zero;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Aim alignment test floor";
            floor.transform.position = new Vector3(60, 9.5f, 0); floor.transform.localScale = new Vector3(50, 1, 50);
            Physics.SyncTransforms(); player.Teleport(new Vector3(60, 10.05f, 0));
            follow.ResetView(); follow.Snap();
            yield return new WaitForSeconds(.2f); yield return new WaitForEndOfFrame();
            Ray sight = camera.ScreenPointToRay(player.AimScreenPoint);
            Require(Physics.Raycast(sight, out RaycastHit ground, 120, ~(1 << 2), QueryTriggerInteraction.Ignore) && ground.collider.gameObject == floor,
                "Downward central reticle selects an ordinary floor collider");
            Require(Vector3.Distance(player.AimPoint, ground.point) < .001f &&
                Vector2.Distance(camera.WorldToScreenPoint(player.AimPoint), player.AimScreenPoint) < .1f,
                "Ground aim stays at the exact reticle impact with no legacy upward offset");
            follow.RotateView(0, -25); follow.Snap(); yield return new WaitForEndOfFrame();
            sight = camera.ScreenPointToRay(player.AimScreenPoint);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Aim alignment test wall";
            wall.transform.position = sight.GetPoint(18); wall.transform.localScale = new Vector3(4, 4, .4f);
            Physics.SyncTransforms(); yield return new WaitForEndOfFrame();
            Require(Physics.Raycast(camera.ScreenPointToRay(player.AimScreenPoint), out RaycastHit wallHit, 120, ~(1 << 2), QueryTriggerInteraction.Ignore) &&
                wallHit.collider.gameObject == wall && Vector3.Distance(player.AimPoint, wallHit.point) < .001f,
                "Wall aim uses the visible surface instead of lifting the shot above the crosshair");
            Destroy(wall); yield return null;
            foreach (float range in new[] { 5f, 18f })
            {
                yield return new WaitForSeconds(.42f);
                sight = camera.ScreenPointToRay(player.AimScreenPoint);
                var target = AimProbe(sight.GetPoint(10.5f + range));
                Physics.SyncTransforms(); player.RequestBowShot(); yield return new WaitForEndOfFrame();
                var arrow = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Single(x => !x.candy && !x.hostile);
                Require(Vector2.Distance(camera.WorldToScreenPoint(player.AimPoint), player.AimScreenPoint) < .1f &&
                    Vector3.Cross(player.AimPoint - arrow.transform.position, arrow.velocity.normalized).magnitude < .002f,
                    "Real queued shot converges from the bow to the displayed reticle at range " + range);
                Require(Vector3.Dot(arrow.transform.forward, arrow.velocity.normalized) > .9999f &&
                    Vector3.Dot(player.characterAnimation.bow.forward, arrow.velocity.normalized) > .999f &&
                    Vector3.Distance(arrow.transform.position, player.characterAnimation.BowGripPosition) < .03f,
                    "Arrow model, bow pitch and grip agree on their first rendered frame at range " + range);
                yield return new WaitForSeconds(.8f);
                Require(target.Cleared, "A real arrow strikes the small physical target under the reticle at range " + range);
                Destroy(target.gameObject); yield return null;
            }
            var previousRunes = game.runes;
            var offAxis = AimProbe(camera.ScreenPointToRay(player.AimScreenPoint + Vector2.right * 22).GetPoint(20));
            offAxis.GetComponent<BoxCollider>().size = Vector3.one * .2f;
            game.runes = previousRunes.Concat(new[] { offAxis }).ToArray(); Physics.SyncTransforms();
            yield return new WaitForEndOfFrame();
            Require(Vector2.Distance(camera.WorldToScreenPoint(offAxis.AimPosition), player.AimScreenPoint) < 48 &&
                Vector2.Distance(camera.WorldToScreenPoint(player.AimPoint), player.AimScreenPoint) < .1f &&
                Vector3.Distance(player.AimPoint, offAxis.AimPosition) > 1,
                "Nearby off-center targets do not secretly redirect a centered shot");
            game.runes = previousRunes; Destroy(offAxis.gameObject); yield return null;
            yield return new WaitForSeconds(.42f);
            var covered = AimProbe(camera.ScreenPointToRay(player.AimScreenPoint).GetPoint(28.5f));
            Physics.SyncTransforms(); player.RequestBowShot(); yield return new WaitForEndOfFrame();
            var blockedArrow = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Single(x => !x.candy && !x.hostile);
            var cover = GameObject.CreatePrimitive(PrimitiveType.Cube); cover.name = "Near-bow cover outside the camera sight ray";
            cover.transform.position = blockedArrow.transform.position + blockedArrow.velocity.normalized * .75f;
            cover.transform.localScale = Vector3.one * .45f; Physics.SyncTransforms();
            Require(Physics.Raycast(camera.ScreenPointToRay(player.AimScreenPoint), out RaycastHit clearSight, 120, ~(1 << 2), QueryTriggerInteraction.Ignore) &&
                clearSight.collider.GetComponent<DreamRune>() == covered,
                "Shoulder camera can see the target while a separate obstacle blocks the bow");
            yield return new WaitForSeconds(.8f);
            Require(blockedArrow == null && !covered.Cleared,
                "A real arrow stops at nearby bow cover instead of passing through it toward the crosshair");
            Destroy(cover); Destroy(covered.gameObject); yield return null;
            foreach (var arrow in FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None)) Destroy(arrow.gameObject);
            yield return new WaitForSeconds(.42f);
            follow.ApplyLookInput(new Vector2(30, 0)); follow.ZoomView(2);
            player.RequestBowShot(); yield return new WaitForEndOfFrame();
            var turnedArrow = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Single(x => !x.candy && !x.hostile);
            Require(Vector3.Dot(turnedArrow.velocity.normalized, Vector3.right) > .99f &&
                Vector2.Distance(camera.WorldToScreenPoint(player.AimPoint), player.AimScreenPoint) < .1f,
                "A shot queued during a 90-degree turn and zoom uses this frame's camera direction");
            yield return Capture("00-arrow-turn-alignment"); Destroy(turnedArrow.gameObject);
            yield return new WaitForSeconds(.42f);
            Require(player.TryJump(), "Airborne aim check begins with a real controller jump");
            player.automationMove = Vector3.forward * .5f;
            yield return new WaitForSeconds(.08f);
            follow.ApplyLookInput(new Vector2(-12, 5)); player.RequestBowShot(); yield return new WaitForEndOfFrame();
            var airArrow = FindObjectsByType<DreamProjectile>(FindObjectsSortMode.None).Single(x => !x.candy && !x.hostile);
            Require(!player.IsGrounded && Vector2.Distance(camera.WorldToScreenPoint(player.AimPoint), player.AimScreenPoint) < .1f &&
                Vector3.Cross(player.AimPoint - airArrow.transform.position, airArrow.velocity.normalized).magnitude < .002f &&
                Vector3.Dot(airArrow.transform.forward, airArrow.velocity.normalized) > .9999f &&
                Vector3.Distance(airArrow.transform.position, player.characterAnimation.BowGripPosition) < .03f,
                "Moving and jumping fire uses the current-frame reticle and correctly oriented arrow");
            yield return Capture("00-arrow-airborne-alignment"); Destroy(airArrow.gameObject);
            player.automationMove = Vector3.zero; player.Teleport(start); player.visual.rotation = facing;
            Destroy(floor); follow.ResetView(); follow.Snap(); yield return null;
        }
        DreamRune AimProbe(Vector3 point)
        {
            var root = new GameObject("Physical reticle alignment target"); root.transform.position = point - Vector3.up * 1.4f;
            var collider = root.AddComponent<BoxCollider>(); collider.center = Vector3.up * 1.4f; collider.size = new Vector3(.8f, .8f, .4f);
            var rune = root.AddComponent<DreamRune>();
            rune.crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            rune.crystal.SetParent(root.transform, false); rune.crystal.gameObject.layer = 2;
            rune.crystal.localPosition = Vector3.up * 1.4f; rune.crystal.localScale = Vector3.one * .5f;
            return rune;
        }
        IEnumerator KayKitCandyChecks()
        {
            var game = DreamGame.Instance;
            var prefab = Resources.Load<GameObject>("Props/KayKitCandy");
            Require(prefab != null && prefab.GetComponentsInChildren<MeshFilter>().Any(x => x.sharedMesh.name == "KayKitCandyBody"),
                "Selected KayKit candy mesh is included in the built player");
            Require(game.GetComponentsInChildren<CandyPickup>().All(x => x.visual.Find("KayKit candy") != null),
                "Saved ground candies use the selected imported model");
            Require(game.player.heldCandy.transform.Find("KayKit candy") != null,
                "Saved hand-held candy uses the same imported model");
            Require(prefab.GetComponentsInChildren<Collider>(true).Length == 0,
                "Candy artwork adds no colliders that interfere with pickup or projectile logic");
            var camera = game.gameCamera; var follow = camera.GetComponent<DreamCamera>(); var hud = game.GetComponent<DreamHUD>();
            Vector3 position = camera.transform.position; Quaternion rotation = camera.transform.rotation; float fov = camera.fieldOfView;
            follow.enabled = false; hud.enabled = false;
            var preview = new GameObject("Candy color visual check"); preview.transform.position = new Vector3(60, 5, 0);
            foreach (CandyHue hue in Enum.GetValues(typeof(CandyHue)))
            {
                var model = DreamArt.CandyVisual(preview.transform, new Vector3(((int)hue - 1) * 1.6f, 0, 0), 1, hue);
                CandyPalette.Paint(model.transform, hue);
                var body = model.GetComponentsInChildren<MeshRenderer>().Single(x => x.name == "Sweet");
                var white = model.GetComponentsInChildren<MeshRenderer>().Single(x => x.name == "Candy stripes");
                Require(body.sharedMaterial == DreamArt.Mat(CandyPalette.Material(hue)) &&
                    white.sharedMaterial == Resources.Load<Material>("Materials/KayKitCandyStripe"),
                    "Imported candy preserves its white pattern while matching the " + hue + " gameplay color");
                Require(body.sharedMaterial.IsKeywordEnabled("_EMISSION") &&
                    body.sharedMaterial.GetColor("_EmissionColor").maxColorComponent > .2f,
                    "Matching candy color remains readable in night lighting: " + hue);
                Require(body.GetComponent<MeshFilter>().sharedMesh.vertexCount > 20 && body.bounds.size.x > .2f &&
                    body.bounds.size.x < 1.3f, "Imported candy has visible game-scale source geometry: " + hue);
                var shot = DreamProjectile.Spawn(preview.transform.position + Vector3.up * 8, Vector3.right, true, false, hue);
                var sweet = shot.GetComponentsInChildren<MeshFilter>().Single(x => x.name == "Sweet");
                Require(shot.hue == hue && sweet.sharedMesh.name == "KayKitCandyBody" &&
                    sweet.GetComponent<Renderer>().sharedMaterial == DreamArt.Mat(CandyPalette.Material(hue)),
                    "Thrown candy retains the imported mesh and matching hue: " + hue);
                Destroy(shot.gameObject);
            }
            camera.transform.position = preview.transform.position + new Vector3(0, .75f, 6.5f);
            camera.transform.LookAt(preview.transform.position); camera.fieldOfView = 40;
            yield return Capture("00-kaykit-candy-colors");
            Destroy(preview);
            camera.transform.position = position; camera.transform.rotation = rotation; camera.fieldOfView = fov;
            follow.enabled = true; hud.enabled = true;
            yield return null;
        }

        IEnumerator TitleScreenChecks()
        {
            var game = DreamGame.Instance;
            Require(game.Phase == GamePhase.Menu, "Game launches into the cover menu before gameplay");
            Require(Cursor.lockState == CursorLockMode.None && Cursor.visible,
                "Title menu keeps a visible free cursor for its buttons");
            var art = Resources.Load<Texture2D>("UI/TitleBackdrop");
            Require(art != null && art.width == 1600 && art.height == 1000,
                "Chosen cover composition imports at its original 1600 by 1000 aspect ratio");
            Require(game.audioSystem.MusicPlaying && game.audioSystem.MusicClip.name == "dream_journey",
                "Dream music plays while the cover menu is displayed");
            Vector3 start = game.player.transform.position;
            foreach (Vector2Int size in new[] { new Vector2Int(1440, 900), new Vector2Int(1920, 1080), new Vector2Int(1280, 960) })
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.4f);
                yield return Capture("00-cover-" + size.x + "x" + size.y);
                Require(game.Phase == GamePhase.Menu && Vector3.Distance(game.player.transform.position, start) < .01f,
                    "Cover menu remains idle and preserves the gameplay spawn while resizing: " + size);
            }
            Screen.SetResolution(1440, 900, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(.4f);
        }

        void HalloweenArtChecks(DreamGame game)
        {
            Transform scenery = game.transform.Find("Poly Halloween scenery");
            Require(scenery != null && scenery.childCount > 0, "Saved chapter includes imported Halloween scenery " + (game.levelIndex + 1));
            Require(scenery.GetComponentsInChildren<SceneryObstacle>(true).All(o => o.Solid),
                "Solid scenery uses fitted colliders " + (game.levelIndex + 1));
            Require(scenery.GetComponentsInChildren<Collider>(true).All(x => !x.enabled || x.GetComponentInParent<SceneryObstacle>() != null),
                "Physical decoration uses explicitly marked obstacle shapes " + (game.levelIndex + 1));
            foreach (var lantern in game.lanterns)
            {
                var skin = lantern.GetComponent<LanternSkin>();
                Require(skin != null && skin.lantern == lantern && skin.unlit.activeSelf && !skin.lit.activeSelf,
                    "Imported switch starts unlit and references the existing lantern: " + lantern.lanternName);
                Require(skin.GetComponentsInChildren<Collider>(true).All(x => !x.enabled || x.transform == lantern.transform.Find("Lantern pedestal") || x.GetComponentInParent<SceneryObstacle>(true) != null),
                    "Lantern retains its pedestal and fitted pumpkin collision: " + lantern.lanternName);
            }
        }

        void Save(bool passed, string error)
        {
            var report = new Report { passed = passed, unityVersion = Application.unityVersion, checks = checks.ToArray(), error = error };
            File.WriteAllText(Path.Combine(outputDirectory, "integration-results.json"), JsonUtility.ToJson(report, true));
        }
    }
}
