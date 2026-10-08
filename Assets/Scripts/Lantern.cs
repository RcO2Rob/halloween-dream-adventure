using UnityEngine;

namespace LostDream
{
    public class Lantern : DreamInteractable
    {
        public string lanternName = "Pumpkin lantern";
        public DreamPath path;
        public Light glow;
        public Transform flame;
        public Vector3 safeCheckpoint;
        public bool Lit { get; private set; }
        public override bool Available => !Lit;
        public override string Prompt => "LIGHT " + lanternName.ToUpperInvariant();
        void Start() { if (flame != null) flame.gameObject.SetActive(false); glow.intensity = .3f; }
        public override bool Use(PlayerMotor player)
        {
            if (Lit) return false;
            Lit = true;
            flame.gameObject.SetActive(true);
            glow.intensity = 3;
            if (path != null) path.Reveal();
            DreamGame.Instance.SetCheckpoint(safeCheckpoint);
            DreamGame.Instance.audioSystem.Play("lantern", .7f);
            DreamGame.Instance.ShowToast(path != null && path.IsJumpRoute ?
                "Round platforms restored. Space / jump. Checkpoint saved." : "A piece of the dream returns. Checkpoint restored.", 3);
            DreamArt.Burst(transform.position + Vector3.up * 1.4f, "GlowGold", 30, 3);
            return true;
        }
        void Update()
        {
            if (!Lit) return;
            flame.localScale = Vector3.one * (.22f + Mathf.Sin(Time.time * 5) * .04f);
            glow.intensity = 2.5f + Mathf.Sin(Time.time * 3) * .35f;
        }
    }
}
