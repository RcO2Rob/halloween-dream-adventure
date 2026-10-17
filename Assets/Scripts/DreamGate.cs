using UnityEngine;

namespace LostDream
{
    public class DreamGate : MonoBehaviour
    {
        public Renderer[] glowParts;
        public GardenIronGate ironGate;
        bool lastOpen;
        float promptReady;
        void Update()
        {
            bool open = DreamGame.Instance != null && DreamGame.Instance.ObjectivesComplete;
            if (open != lastOpen)
            {
                lastOpen = open;
                foreach (var part in glowParts) part.sharedMaterial = DreamArt.Mat(open ? "GlowTeal" : "Mist");
            }
        }
        void OnTriggerStay(Collider other)
        {
            var player = other.GetComponent<PlayerMotor>();
            if (player == null) return;
            if (ironGate != null)
            {
                if (ironGate.CanPass(player)) DreamGame.Instance.TryExit();
                return;
            }
            if (Time.time < promptReady) return;
            promptReady = Time.time + 2;
            DreamGame.Instance.TryExit();
        }
    }
}
