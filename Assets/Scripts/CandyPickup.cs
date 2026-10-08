using UnityEngine;

namespace LostDream
{
    public class CandyPickup : DreamInteractable
    {
        public Transform visual;
        public CandyHue hue;
        TextMesh label;
        public override bool Available => DreamGame.Instance != null &&
            (!DreamGame.Instance.player.CarryingCandy || DreamGame.Instance.player.HeldCandyHue != hue);
        public override string Prompt => (DreamGame.Instance.player.CarryingCandy ? "SWAP FOR " : "PICK UP ") +
            CandyPalette.Label(hue) + " CANDY  /  Q TO THROW";
        void Start() { SetHue(hue); }
        public void SetHue(CandyHue color)
        {
            hue = color;
            CandyPalette.Paint(transform, hue);
            if (label == null)
            {
                var root = DreamArt.Group("Candy color label", transform, Vector3.up * .65f);
                label = root.AddComponent<TextMesh>();
                label.characterSize = .065f; label.fontSize = 40;
                label.anchor = TextAnchor.MiddleCenter;
                label.color = CandyPalette.Color(hue);
            }
            label.text = CandyPalette.Label(hue);
            label.color = CandyPalette.Color(hue);
        }
        public override bool Use(PlayerMotor player)
        {
            // Exchange the held sweet with this pickup so choosing a color never wastes ammo.
            if (player.CarryingCandy)
            {
                CandyHue old = player.HeldCandyHue;
                if (!player.SwapCandy(hue)) return false;
                SetHue(old);
                return true;
            }
            if (!player.PickCandy(hue)) return false;
            Destroy(gameObject);
            return true;
        }
        void Update()
        {
            visual.localPosition = Vector3.up * Mathf.Sin(Time.time * 3 + transform.position.x) * .12f;
            visual.Rotate(0, Time.deltaTime * 40, 0);
            if (label != null && DreamGame.Instance != null)
            {
                bool nearby = DreamGame.Instance.levelIndex == 2 &&
                    Vector3.Distance(transform.position, DreamGame.Instance.player.transform.position) < 14;
                bool clear = nearby && !Physics.Linecast(DreamGame.Instance.gameCamera.transform.position,
                    label.transform.position, ~(1 << 2), QueryTriggerInteraction.Ignore);
                label.gameObject.SetActive(clear);
                label.transform.rotation = DreamGame.Instance.gameCamera.transform.rotation;
            }
        }
    }
}
