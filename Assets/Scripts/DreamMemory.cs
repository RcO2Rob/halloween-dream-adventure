using System.Linq;
using UnityEngine;

namespace LostDream
{
    public class DreamMemory : DreamInteractable
    {
        public GardenAdventure garden;
        public int index;
        public FlyingDragon[] guardians;
        public Transform crystal;
        public bool Collected { get; private set; }
        public override bool Available => !Collected;
        public override string Prompt => "MEMORY " + (index + 1) + " / " + (BlockedReason() ?? "RECOVER");
        public string BlockedReason()
        {
            if (garden.MemoriesCollected < index) return "FIND PREVIOUS MEMORY";
            if (guardians.Any(x => x != null && !x.Defeated)) return "CLEAR GUARDIANS";
            if (index == 1 && !garden.SequenceComplete) return "SHOOT I > II > III";
            if (index == 2 && !garden.candySeal.Cleared) return "BREAK THE CANDY SEAL";
            return null;
        }
        public override bool Use(PlayerMotor player)
        {
            if (Collected) return false;
            string blocked = BlockedReason();
            if (blocked != null) { DreamGame.Instance.ShowToast(blocked + ". " + garden.NextHint(), 4); return false; }
            Collected = true; crystal.gameObject.SetActive(false);
            // Each recovered memory is a safe return point on its island.
            DreamGame.Instance.SetCheckpoint(transform.position + Vector3.back * 2 + Vector3.up * .12f);
            DreamGame.Instance.audioSystem.Play("lantern", .8f);
            DreamArt.Burst(transform.position + Vector3.up * 2, "GlowTeal", 40, 4);
            DreamGame.Instance.ShowToast("Memory " + (index + 1) + "/3 restored. " + garden.NextHint(), 5);
            return true;
        }
        void Update()
        {
            if (Collected) return;
            crystal.Rotate(0, 40 * Time.deltaTime, 0);
            crystal.localPosition = Vector3.up * (2.25f + Mathf.Sin(Time.time * 2) * .13f);
        }
    }
}
