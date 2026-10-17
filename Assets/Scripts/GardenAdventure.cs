using System.Linq;
using UnityEngine;

namespace LostDream
{
    // Chapter-two progress is local to the saved scene and survives checkpoint falls.
    // No timers or waves block progression; each lock is an explicit player action.
    public class GardenAdventure : MonoBehaviour
    {
        public DreamMemory[] memories;
        public DreamRune[] sequenceRunes;
        public DreamRune candySeal;
        public Lantern groveLight, observatoryLight, cryptLight;
        public Vector3[] candyPoints;
        public int SequenceProgress { get; private set; }
        public bool SequenceComplete => SequenceProgress == sequenceRunes.Length;
        public int MemoriesCollected => memories.Count(x => x.Collected);
        public bool Complete => MemoriesCollected == 3;
        public string StageLabel => MemoriesCollected == 0 ? "I  /  WHISPER GROVE" :
            MemoriesCollected == 1 ? "II  /  MOON OBSERVATORY" : MemoriesCollected == 2 ?
            "III  /  CANDY CRYPT" : "RETURN TO THE DREAM GATE";
        float candyTimer = 3;
        Transform[] labels;
        void Start()
        {
            labels = GetComponentsInChildren<TextMesh>(true).Where(x => x.name == "Readable objective label")
                .Select(x => x.transform).ToArray();
        }
        void LateUpdate()
        {
            var camera = DreamGame.Instance.gameCamera;
            foreach (var label in labels)
            {
                Vector3 away = label.position - camera.transform.position;
                if (away.sqrMagnitude > .01f) label.rotation = Quaternion.LookRotation(away);
            }
        }

        void Update()
        {
            if (DreamGame.Instance.Phase != GamePhase.Playing) return;
            if (cryptLight.Lit && !candySeal.Cleared)
            {
                candyTimer -= Time.deltaTime;
                if (candyTimer <= 0) { candyTimer = 3; RefillCandy(); }
            }
        }
        void RefillCandy()
        {
            var sweets = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
            foreach (var point in candyPoints)
            {
                if (sweets.Any(x => Vector3.Distance(x.transform.position, point) < 1.5f)) continue;
                var hits = Physics.RaycastAll(point + Vector3.up * 5, Vector3.down, 10, ~(1 << 2), QueryTriggerInteraction.Ignore);
                if (!hits.Any(x => x.normal.y > .6f && Mathf.Abs(x.point.y - point.y) < .3f)) continue;
                DreamWorld.Candy(DreamGame.Instance.transform, point + Vector3.up * .65f);
            }
        }
        public void HitSequence(DreamRune rune, bool candy)
        {
            var game = DreamGame.Instance;
            if (SequenceComplete || game.Phase != GamePhase.Playing) return;
            if (MemoriesCollected < 1 || !observatoryLight.Lit)
            { game.ShowToast("Recover the Whisper Grove memory, then light the Observatory route.", 4); return; }
            if (candy) { game.ShowToast("Use arrows on the numbered moon runes: I, II, III."); return; }
            if (sequenceRunes[SequenceProgress] != rune)
            {
                SequenceProgress = 0; PaintSequence();
                game.audioSystem.Play("shield", .35f);
                game.ShowToast("The moon melody reset. Shoot I, then II, then III. Arrows are unlimited.", 4);
                return;
            }
            SequenceProgress++;
            PaintSequence(); game.audioSystem.Play("lantern", .5f);
            DreamArt.Burst(rune.AimPosition, "GlowTeal", 20, 2);
            if (SequenceComplete)
            {
                foreach (var item in sequenceRunes) item.Complete();
                game.ShowToast("Moon melody restored! Clear the guardians and recover memory II.", 4);
            }
            else game.ShowToast("Moon rune " + SequenceProgress + "/3. Next: " + (SequenceProgress == 1 ? "II" : "III") + ".");
        }
        void PaintSequence()
        {
            for (int i = 0; i < sequenceRunes.Length; i++)
                sequenceRunes[i].crystal.GetComponent<Renderer>().sharedMaterial = DreamArt.Mat(i < SequenceProgress ? "GlowTeal" : "GlowGold");
        }
        public string NextHint()
        {
            int count = MemoriesCollected;
            int guardians = count < 3 ? memories[count].guardians.Count(x => x != null && !x.Defeated) : 0;
            if (count == 0) return !groveLight.Lit ?
                "Reach the orchard. Light the WEST lantern.\nJump to Whisper Grove for memory I." :
                guardians > 0 ? "Orchard + Whisper Grove / " + guardians + " guardians remain.\nClear both islands, then recover memory I with E." :
                "Whisper Grove / E at the tall blue shrine.\nRecover memory I to unlock the Terrace lantern.";
            if (count == 1) return !observatoryLight.Lit ?
                "Light Terrace, then the EAST Observatory lantern.\nCross the round platforms to memory II." :
                !SequenceComplete ? "Observatory / shoot the numbered runes I > II > III.\n" + SequenceProgress + "/3 correct. A wrong rune resets the melody." :
                guardians > 0 ? "Terrace + Observatory / " + guardians + " guardians remain.\nClear both islands, then recover memory II with E." :
                "Observatory / E at the blue shrine.\nRecover memory II to unlock the Gate lantern.";
            if (count == 2) return !cryptLight.Lit ?
                "Light Gate, then the WEST Crypt lantern.\nJump LEFT from the gate garden to memory III." :
                !candySeal.Cleared ? "Crypt / E to pick up candy, right-click to break the seal.\nMissed throws refill every 3 seconds." :
                guardians > 0 ? "Gate garden + Crypt / " + guardians + " guardians remain.\nClear both islands, then recover memory III with E." :
                "Crypt / E at the blue shrine for the final memory.\nThen return EAST to the dream gate.";
            var game = DreamGame.Instance;
            if (game.DragonsRemaining > 0) return "Memories restored. Clear the remaining nightmares.";
            if (game.LitCount < game.lanterns.Length) return "Memories restored. Light the remaining lanterns.";
            return "All three memories restored!\nFollow the glowing path through the iron gate.";
        }
    }
}
