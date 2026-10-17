using System.Linq;
using UnityEngine;

namespace LostDream
{
    public class WitchCauldron : MonoBehaviour
    {
        public string brewName;
        public CandyHue[] recipe;
        public Vector3[] supplyPoints;
        public FlyingDragon[] guardians;
        public Renderer liquid;
        public TextMesh label;
        public Light glow;
        public int Ingredients { get; private set; }
        public bool Complete => Ingredients >= recipe.Length;
        public int GuardsRemaining => guardians.Count(x => x != null && !x.Defeated);
        public string RecipeLabel => string.Join(" > ", recipe.Select(x => CandyPalette.Label(x)));
        public Vector3 AimPosition => transform.position + Vector3.up * 1.7f;
        void Start() { Refresh(); }
        public void Hit(bool candy, CandyHue hue)
        {
            if (Complete || DreamGame.Instance.Phase != GamePhase.Playing) return;
            if (!candy || hue != recipe[Ingredients])
            {
                DreamGame.Instance.ShowToast("The pot needs " + CandyPalette.Label(recipe[Ingredients]) + " candy next. Your recipe progress is safe.", 3);
                DreamGame.Instance.audioSystem.Play("locked", .4f);
                return;
            }
            Ingredients++;
            Refresh();
            DreamArt.Burst(AimPosition, Complete ? "GlowTeal" : CandyPalette.Material(hue), 24, 3);
            DreamGame.Instance.audioSystem.Play("lantern", .65f);
            DreamGame.Instance.ShowToast(Complete ? brewName + " brewed! The mist clears. Defeat the guardians, then light the crossing lantern." :
                "Ingredient accepted. Next: " + CandyPalette.Label(recipe[Ingredients]) + " candy.", 4);
        }
        void Refresh()
        {
            string mat = Complete ? "GlowTeal" : CandyPalette.Material(recipe[Ingredients]);
            liquid.sharedMaterial = DreamArt.Mat(mat);
            glow.color = Complete ? new Color(.3f, 1, .8f) : CandyPalette.Color(recipe[Ingredients]);
            label.text = brewName.ToUpperInvariant() + "\n" + (Complete ? "BREW COMPLETE" : RecipeLabel + "\nNEXT: " + CandyPalette.Label(recipe[Ingredients]));
            label.color = Complete ? new Color(.5f, 1, .8f) : CandyPalette.Color(recipe[Ingredients]);
        }
        void LateUpdate()
        {
            if (DreamGame.Instance == null) return;
            label.transform.rotation = DreamGame.Instance.gameCamera.transform.rotation;
            label.gameObject.SetActive(Vector3.Distance(transform.position, DreamGame.Instance.player.transform.position) < 23);
        }
    }
}
