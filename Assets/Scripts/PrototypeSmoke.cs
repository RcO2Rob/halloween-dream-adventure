using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    // Opt-in integration test. Never runs during ordinary play.
    public class PrototypeSmoke : MonoBehaviour
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
            yield return Capture("01-title");
            DreamGame.Instance.StartPlaying();
            if (Environment.GetCommandLineArgs().Contains("--boss-smoke"))
            {
                DreamGame.Instance.LoadChapter(2);
                yield return new WaitForSecondsRealtime(.5f);
                Time.timeScale = 3;
                yield return BossBattle();
                yield return Capture("03-victory");
                Save(true, "");
                Debug.Log("LOST_DREAM_BOSS_SMOKE_PASSED: " + checks.Count + " boss integration checks.");
                Application.Quit(0);
                yield break;
            }
            yield return TutorialSupplyChecks();
            yield return JumpChecks();
            yield return CameraChecks();
            yield return CharacterChecks();
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
                Require(game.player.characterAnimation != null && game.player.characterAnimation.animator.isHuman,
                    "Chapter uses the selected Timmy humanoid character " + (level + 1));
                game.player.automation = true;
                Require(!game.TryExit(), "Exit rejects incomplete chapter " + (level + 1));
                foreach (var lantern in game.lanterns)
                {
                    Require(!lantern.path.solid.activeSelf, "Hidden road has no active floor");
                    game.player.Teleport(lantern.safeCheckpoint);
                    yield return new WaitForSeconds(.12f);
                    Require(game.player.Interact(), "E interaction lights " + lantern.lanternName);
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
                if (level == 1)
                    Require(game.dragons.Length == 5 && game.dragons.Select(x => x.kind).Distinct().Count() == 4,
                        "Garden keeps five encounters and includes dragon, ghost, bee and demon species");
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
                game.player.Teleport(gate.transform.position + Vector3.up * .1f);
                yield return new WaitForSeconds(.3f);
                Require(game.Phase == GamePhase.LevelComplete, "Physical gate trigger completes chapter " + (level + 1));
                game.ContinueJourney();
                yield return new WaitForSeconds(.8f);
                Time.timeScale = 3;
                Require(DreamGame.Instance.levelIndex == level + 1, "Scene transition preserves chapter order");
            }
            yield return BossBattle();
            yield return Capture("03-victory");
            Save(true, null);
            Debug.Log("LOST_DREAM_SMOKE_PASSED: " + checks.Count + " functional integration checks.");
            Application.Quit(0);
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
            yield return Capture("03-color-mismatch");
            Require(boss.Health == health && boss.Exposed && boss.RequiredHue == hue && game.Toast.Contains("Wrong color"),
                "Wrong-color projectile does no damage, explains the mismatch and leaves the window open");
            game.ReplenishCandy();
            Require(FindObjectsByType<CandyPickup>(FindObjectsSortMode.None).Count(x => x.hue == boss.RequiredHue) >= 2,
                "Refill guarantees required color even when floor slots are occupied by wrong colors");
            Time.timeScale = previousScale;
        }
        IEnumerator BossBattle()
        {
            var game = DreamGame.Instance;
            var player = game.player;
            Require(player.characterAnimation != null && player.characterAnimation.animator.isHuman,
                "Castle chapter uses Timmy humanoid character");
            player.automation = true;
            var lamp = game.lanterns[0];
            player.Teleport(lamp.safeCheckpoint);
            yield return new WaitForSeconds(.12f);
            Require(player.Interact(), "Castle entrance lantern restores approach");
            yield return JumpRouteChecks(game, lamp.path);
            player.Teleport(new Vector3(0, .1f, 6));
            yield return new WaitForSeconds(.3f);
            Require(game.boss.AwakeInArena, "Boss wakes when entering restored courtyard");
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            Vector3 heroScreen = game.gameCamera.WorldToViewportPoint(player.transform.position + Vector3.up);
            Vector3 bossScreen = game.gameCamera.WorldToViewportPoint(game.boss.AimPosition);
            Require(InView(heroScreen) && InView(bossScreen), "Boss framing keeps player and pumpkin weak point visible");
            yield return Capture("03-perspective-arena");
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
                if (game.boss.Exposed)
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
                        yield return Capture("03-color-" + cycleHue.ToString().ToLowerInvariant());
                        Require(!blocked, "Castle corner view keeps the candy-carrying player clear of roof occlusion: " + cycleHue);
                    }
                    if (hp == game.boss.maxHealth) yield return Capture("03-weak-point");
                    player.ThrowAt(game.boss.AimPosition);
                    yield return new WaitForSeconds(1.4f);
                    Require(game.boss.Health == hp - 1, "Real candy projectile damages exposed boss exactly once");
                    if (hp > game.boss.maxHealth / 2 + 1)
                        Require(!game.boss.SecondPhase && game.boss.ChargesCompleted == 0, "Bomb-only first phase stays active above half health");
                    if (hp == game.boss.maxHealth / 2 + 1)
                    {
                        Require(game.boss.SecondPhase, "Candy hit at exactly half health activates the second phase");
                        yield return BossChargeChecks(game);
                        player.Heal(5);
                    }
                    if (!game.boss.Defeated) yield return Capture("03-boss-" + game.boss.Health);
                    changeAt = 0;
                }
                yield return null;
            }
            Require(observedHues.Count == 3, "Victory uses all three candy colors through gradual first-half introduction");
            Require(game.boss.Defeated && game.Phase == GamePhase.Victory, "Boss defeat leads to victory screen");
            Require(FindObjectsByType<NightmareBomb>(FindObjectsSortMode.None).Length == 0, "Victory clears pending bomb hazards");
            Require(game.boss.ChargesCompleted > 0 && game.boss.ChargeWarning == null,
                "Boss victory includes second-phase charges and leaves no charge warning");
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
            yield return Capture("03-charge-warning");
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
            Require(target.x >= -9.5f && target.x <= 9.5f && target.z >= 2.5f && target.z <= 22,
                "Charge ends on safe courtyard ground away from the jump platforms");
            player.Teleport(safeSide); player.Heal(5);
            yield return new WaitForSeconds(.7f);
            Require(Vector3.Dot(-boss.transform.forward, Vector3.ProjectOnPlane(player.transform.position - boss.transform.position, Vector3.up).normalized) > .95f,
                "Recovery turns the matching-color face toward the player so the weak point remains visible");
            game.gameCamera.GetComponent<DreamCamera>().Snap();
            yield return Capture("03-charge-recovery");
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
            int chapter = game.levelIndex + 1;
            Require(path.IsJumpRoute && path.Revealed && path.jumpPlatforms.Length == (chapter == 2 ? 3 : 2),
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
                    Vector3 offset = Vector3.ProjectOnPlane(target - player.transform.position, Vector3.up);
                    player.automationMove = offset.magnitude > .06f ? offset.normalized *
                        Mathf.Clamp01(offset.magnitude / (player.moveSpeed * Time.deltaTime)) : Vector3.zero;
                    if (i == 0 && !captured && offset.magnitude < .15f && player.VerticalSpeed < 0)
                    {
                        captured = true;
                        player.automationMove = Vector3.zero;
                        yield return Capture("0" + chapter + "-round-platform-jump");
                    }
                    yield return null;
                }
                player.automationMove = Vector3.zero;
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
        void Save(bool passed, string error)
        {
            var report = new Report { passed = passed, unityVersion = Application.unityVersion, checks = checks.ToArray(), error = error };
            File.WriteAllText(Path.Combine(outputDirectory, "integration-results.json"), JsonUtility.ToJson(report, true));
        }
    }
}
