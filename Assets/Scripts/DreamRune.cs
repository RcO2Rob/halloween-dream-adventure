using UnityEngine;

namespace LostDream
{
    public class DreamRune : MonoBehaviour
    {
        public bool candyOnly;
        public Transform crystal;
        public GardenAdventure sequence;
        [TextArea] public string clearedMessage;
        public bool Cleared { get; private set; }
        public Vector3 AimPosition => transform.position + Vector3.up * 1.4f;
        public void Hit(bool candy)
        {
            if (Cleared) return;
            if (sequence != null) { sequence.HitSequence(this, candy); return; }
            if (candyOnly && !candy)
            {
                DreamGame.Instance.ShowToast("This pink seal needs a candy. Pick one up with E.");
                DreamGame.Instance.audioSystem.Play("shield", .4f);
                return;
            }
            Complete();
            DreamArt.Burst(AimPosition, candyOnly ? "Candy" : "GlowGold", 25, 3);
            DreamGame.Instance.audioSystem.Play("hit", .6f);
            DreamGame.Instance.ShowToast(!string.IsNullOrEmpty(clearedMessage) ? clearedMessage :
                candyOnly ? "Nightmare seal shattered. Your dream gate is ready." : "Nice shot! Your bow can reach the flying dragons.");
        }
        public void Complete()
        {
            Cleared = true;
            GetComponent<Collider>().enabled = false;
            crystal.gameObject.SetActive(false);
        }
        void Update()
        {
            if (!Cleared) { crystal.Rotate(0, 55 * Time.deltaTime, 0); crystal.localPosition = Vector3.up * (1.4f + Mathf.Sin(Time.time * 2) * .12f); }
        }
    }
}
