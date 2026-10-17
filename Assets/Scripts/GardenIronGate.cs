using UnityEngine;

namespace LostDream
{
    // Chapter exits use the installed Poly Halloween gate's native hinges.
    public class GardenIronGate : MonoBehaviour
    {
        public Transform leftLeaf, rightLeaf;
        public BoxCollider closedBarrier;
        public GameObject groundCue;
        public Light cueLight;
        public float openingSeconds = 1.8f;
        public BoxCollider[] sideBarriers;
        public BoxCollider rearBarrier;
        public BoxCollider[] rearBarriers;
        public float OpenAmount { get; private set; }
        public bool Unlocked { get; private set; }
        public bool IsOpen => OpenAmount >= .999f;
        Quaternion leftClosed, rightClosed;
        PlayerMotor approaching;

        void Awake()
        {
            leftClosed = leftLeaf.localRotation; rightClosed = rightLeaf.localRotation;
            closedBarrier.enabled = true; groundCue.SetActive(false);
        }
        void Update()
        {
            var game = DreamGame.Instance;
            if (game == null || game.Phase != GamePhase.Playing) return;
            if (!Unlocked && game.ObjectivesComplete)
            {
                Unlocked = true;
                game.audioSystem.Play("lantern", .8f);
                game.ShowToast((game.levelIndex == 1 ? "All memories restored. " : "Dream restored. ") +
                    "The iron gate is opening! Follow the glowing path.", 5);
            }
            if (!Unlocked) return;
            OpenAmount = Mathf.MoveTowards(OpenAmount, 1, Time.deltaTime / Mathf.Max(.1f, openingSeconds));
            float eased = Mathf.SmoothStep(0, 1, OpenAmount);
            leftLeaf.localRotation = leftClosed * Quaternion.Euler(0, -100 * eased, 0);
            rightLeaf.localRotation = rightClosed * Quaternion.Euler(0, 100 * eased, 0);
            closedBarrier.enabled = !IsOpen;
            groundCue.SetActive(IsOpen);
            if (!IsOpen) return;
            cueLight.intensity = 1.5f + Mathf.Sin(Time.time * 2) * .22f;
            Vector3 local = transform.InverseTransformPoint(game.player.transform.position);
            if (Mathf.Abs(local.x) < 2.4f && local.z < -.6f && local.z > -9)
                approaching = game.player;
        }
        public bool CanPass(PlayerMotor player)
        {
            if (!IsOpen || approaching != player) return false;
            Vector3 local = transform.InverseTransformPoint(player.transform.position);
            return Mathf.Abs(local.x) < 1.8f && local.z > .8f;
        }
    }
}
