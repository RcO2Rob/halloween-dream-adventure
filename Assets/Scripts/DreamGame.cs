using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostDream
{
    public enum GamePhase { Menu, Playing, Paused, Defeated, LevelComplete, Victory }

    public class DreamGame : MonoBehaviour
    {
        public static DreamGame Instance { get; private set; }
        public static bool JourneyStarted;
        public static readonly string[] SceneNames = { "01_FirstLight", "02_ShatteredGardens", "03_PumpkinKeep" };
        public static readonly string[] ChapterNames = { "THE FIRST LIGHT", "SHATTERED GARDENS", "THE PUMPKIN KEEP" };
        public int levelIndex;
        public Vector3 spawnPoint;
        public PlayerMotor player;
        public Camera gameCamera;
        public DreamAudio audioSystem;
        public PumpkinBoss boss;
        public Lantern[] lanterns;
        public FlyingDragon[] dragons;
        public DreamRune[] runes;
        public Vector3[] candySpawnPoints;
        public GamePhase Phase { get; private set; }
        public Vector3 Checkpoint { get; private set; }
        public string Toast { get; private set; }
        public float ToastUntil { get; private set; }
        public float PlaySeconds { get; private set; }
        public int LitCount => lanterns.Count(x => x != null && x.Lit);
        public int DragonsRemaining => dragons.Count(x => x != null && !x.Defeated);
        public int RunesRemaining => runes.Count(x => x != null && !x.Cleared);
        public bool ObjectivesComplete => LitCount == lanterns.Length && DragonsRemaining == 0 && RunesRemaining == 0;
        float candyTimer;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1;
            Checkpoint = spawnPoint;
            candyTimer = levelIndex == 0 ? 3 : 0;
            Phase = JourneyStarted ? GamePhase.Playing : GamePhase.Menu;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Application.targetFrameRate = 60;
            foreach (var sign in GetComponentsInChildren<TextMesh>())
                sign.text = sign.text.Replace("SPACE  /  DODGE", "SHIFT  /  DODGE");
        }
        void Start()
        {
            if (levelIndex == 2) ReplenishCandy();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) audioSystem.ToggleMute();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Phase == GamePhase.Playing) Pause();
                else if (Phase == GamePhase.Paused) Resume();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (Phase == GamePhase.Menu) StartPlaying();
                else if (Phase == GamePhase.LevelComplete) ContinueJourney();
                else if (Phase == GamePhase.Defeated) Retry();
                else if (Phase == GamePhase.Victory) LoadChapter(0);
            }
            if (Input.GetKeyDown(KeyCode.R) && Phase != GamePhase.Menu) Retry();
            if (Phase != GamePhase.Playing) return;
            PlaySeconds += Time.deltaTime;
            if (levelIndex == 0 && runes.Any(x => x != null && x.candyOnly && !x.Cleared))
            {
                candyTimer -= Time.deltaTime;
                if (candyTimer <= 0)
                {
                    candyTimer = 3;
                    ReplenishTutorialCandy();
                }
            }
            if (levelIndex == 2 && LitCount > 0)
            {
                candyTimer -= Time.deltaTime;
                if (candyTimer <= 0)
                {
                    candyTimer = 4;
                    ReplenishCandy();
                }
            }
        }

        public void StartPlaying()
        {
            JourneyStarted = true;
            Phase = GamePhase.Playing;
            Time.timeScale = 1;
            audioSystem.Play("lantern", .5f);
            ShowToast("Light the lanterns. Bring the dream back.", 4);
        }
        public void Pause() { if (Phase == GamePhase.Playing) { Phase = GamePhase.Paused; Time.timeScale = 0; } }
        public void Resume() { Phase = GamePhase.Playing; Time.timeScale = 1; }
        public void LoadChapter(int chapter)
        {
            JourneyStarted = true;
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneNames[Mathf.Clamp(chapter, 0, 2)]);
        }
        public void Retry() { LoadChapter(levelIndex); }
        public void ReturnToMenu()
        {
            JourneyStarted = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneNames[0]);
        }
        public void ContinueJourney() { if (levelIndex < 2) LoadChapter(levelIndex + 1); }
        public void SetCheckpoint(Vector3 safePosition)
        {
            Checkpoint = safePosition;
            player.Heal(1);
        }
        public void ShowToast(string message, float seconds = 2.5f)
        {
            Toast = message;
            ToastUntil = Time.unscaledTime + seconds;
        }
        public bool TryExit()
        {
            if (Phase != GamePhase.Playing) return false;
            if (!ObjectivesComplete)
            {
                ShowToast("Restore every lantern and clear the nightmares to open this dream gate.", 3);
                return false;
            }
            Phase = GamePhase.LevelComplete;
            audioSystem.Play("victory", .65f);
            return true;
        }
        public void Defeat()
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.Defeated;
            audioSystem.Play("hurt");
        }
        public void Win()
        {
            Phase = GamePhase.Victory;
            foreach (var bomb in FindObjectsByType<NightmareBomb>(FindObjectsSortMode.None)) Destroy(bomb.gameObject);
            audioSystem.Play("victory", .8f);
            DreamArt.Burst(boss.transform.position + Vector3.up * 3, "GlowGold", 60, 7);
        }
        void ReplenishTutorialCandy()
        {
            if (candySpawnPoints == null) return;
            var sweets = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
            foreach (Vector3 point in candySpawnPoints)
            {
                // Refill each tutorial spot independently, including while candy is held.
                if (sweets.Any(x => Vector3.Distance(x.transform.position, point) < 1.5f)) continue;
                if (Physics.Raycast(point + Vector3.up * 4, Vector3.down, out RaycastHit hit, 10, ~(1 << 2))
                    && hit.normal.y > .6f)
                {
                    DreamWorld.Candy(transform, new Vector3(point.x, hit.point.y + .65f, point.z));
                    sweets = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
                }
            }
        }
        public void ReplenishCandy()
        {
            if (levelIndex != 2 || boss == null || boss.Defeated) return;
            var sweets = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
            if (candySpawnPoints == null || candySpawnPoints.Length == 0) return;
            int offset = UnityEngine.Random.Range(0, candySpawnPoints.Length);
            for (int i = 0; i < candySpawnPoints.Length && sweets.Length < 4; i++)
            {
                Vector3 point = candySpawnPoints[(offset + i) % candySpawnPoints.Length];
                if (Vector3.ProjectOnPlane(point - boss.transform.position, Vector3.up).magnitude < 4) continue;
                if (sweets.Any(x => Vector3.Distance(x.transform.position, point) < 1.5f)) continue;
                if (Physics.Raycast(point + Vector3.up * 4, Vector3.down, out RaycastHit hit, 10, ~(1 << 2))
                    && hit.normal.y > .6f && hit.collider.GetComponentInParent<PumpkinBoss>() == null)
                {
                    DreamWorld.Candy(transform, new Vector3(point.x, hit.point.y + .65f, point.z));
                    sweets = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
                }
            }
            // At each refill/cycle boundary preserve two required sweets, plus one of each
            // other unlocked hue. Held and in-flight sweets keep their original color.
            CandyHue required = boss.RequiredHue;
            CandyHue[] desired = { required, required,
                boss.UnlockedColors >= 2 ? (CandyHue)(((int)required + 1) % boss.UnlockedColors) : required,
                boss.UnlockedColors >= 3 ? (CandyHue)(((int)required + 2) % boss.UnlockedColors) : required };
            var remaining = sweets.Where(x => Vector3.ProjectOnPlane(x.transform.position - boss.transform.position, Vector3.up).magnitude >= 4)
                .OrderByDescending(x => Vector3.Distance(x.transform.position, player.transform.position)).ToList();
            foreach (var hue in desired)
            {
                if (remaining.Count == 0) break;
                var sweet = remaining.FirstOrDefault(x => x.hue == hue) ?? remaining[0];
                if (sweet.hue != hue) sweet.SetHue(hue);
                remaining.Remove(sweet);
            }
        }
        public string Objective()
        {
            if (levelIndex == 2)
            {
                if (LitCount == 0) return "Find the entrance lantern. Restore the bridge to the keep.";
                if (!boss.AwakeInArena) return "Jump across the glowing platforms. Face the Pumpkin King.";
                if (boss.ChargeWarningActive || boss.Phase == BossPhase.Charging) return "Leave the marked charge lane.\nMove sideways or dodge with Shift.";
                if (player.CarryingCandy && player.HeldCandyHue != boss.RequiredHue)
                    return "Need " + boss.RequiredCandyLabel + " candy.\nE at a different candy / swap your held color.";
                if (player.CarryingCandy) return boss.Exposed ? "SHELL OPEN! Throw your matching candy." : "Match his color. Wait for SHELL OPEN.";
                return "E / pick up " + boss.RequiredCandyLabel + " candy.\nThrow when his shell opens. Shift / dodge.";
            }
            foreach (var lamp in lanterns)
            {
                var path = lamp.path;
                if (path == null || !path.IsJumpRoute || !path.Revealed) continue;
                Vector3 local = path.transform.InverseTransformPoint(player.transform.position);
                if (Mathf.Abs(local.x) < 3 && local.z > path.jumpEntry.localPosition.z - 1.5f && local.z < path.jumpExit.localPosition.z + 1)
                    return "SPACE / jump. Aim for each glowing circle.\nA missed jump returns you to your last lantern.";
            }
            var nearbyRune = runes.FirstOrDefault(x => x != null && !x.Cleared && Vector3.Distance(x.transform.position, player.transform.position) < 12);
            if (nearbyRune != null) return nearbyRune.candyOnly ? "E / candy. Right-click / break the seal.\nCandy refills every 3 seconds." : "Aim at the gold practice rune. Left-click to shoot your bow.";
            if (LitCount < lanterns.Length) return "Explore the islands. Light the next pumpkin lantern with E.";
            if (DragonsRemaining > 0) return "Use your bow to clear the flying nightmares.";
            if (RunesRemaining > 0) return "A nightmare seal remains. Follow its glowing marker.";
            return "The dream gate is open. Walk into the glowing arch.";
        }
        void OnDestroy() { if (Instance == this) Instance = null; Time.timeScale = 1; }
    }
}
