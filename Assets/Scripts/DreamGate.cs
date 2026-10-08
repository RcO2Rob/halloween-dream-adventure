using UnityEngine;

namespace LostDream
{
    public class DreamGate : MonoBehaviour
    {
        public Renderer[] glowParts;
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
            if (other.GetComponent<PlayerMotor>() == null || Time.time < promptReady) return;
            promptReady = Time.time + 2;
            DreamGame.Instance.TryExit();
        }
    }
}
