using System.Linq;
using UnityEngine;

namespace LostDream
{
    public class WitchlightMarsh : MonoBehaviour
    {
        public WitchCauldron[] cauldrons;
        public Lantern[] lights;
        public FlyingDragon[] towerGuards;
        float refill;
        public int BrewsComplete => cauldrons.Count(x => x.Complete);
        public bool Complete => BrewsComplete == cauldrons.Length;
        public string StageLabel => !lights[0].Lit ? "THE MUSHROOM LANDING" :
            !lights[1].Lit ? "I / THE GLOWING GROVE" : !lights[2].Lit ? "II / THE WITCH'S WORKSHOP" :
            !lights[3].Lit ? "III / THE BREATHING MARSH" : "IV / THE WATCHTOWER";
        public bool CanLight(int stage)
        {
            if (stage == 0) return true;
            if (stage <= 3) return cauldrons.Take(stage).All(x => x.Complete && x.GuardsRemaining == 0);
            return Complete && cauldrons.All(x => x.GuardsRemaining == 0) && towerGuards.All(x => x == null || x.Defeated);
        }
        public string LockHint(int stage)
        {
            if (stage > 3) return "CLEAR THE WATCHTOWER GUARDIANS";
            var pot = cauldrons[Mathf.Max(0, stage - 1)];
            return !pot.Complete ? "BREW " + pot.brewName.ToUpperInvariant() + " FIRST" : "CLEAR THE GROVE'S REMAINING GUARDIANS";
        }
        public string NextHint()
        {
            if (!lights[0].Lit) return "Light the landing lantern.\nJump across to the mushroom grove.";
            for (int i = 0; i < cauldrons.Length; i++)
            {
                if (!cauldrons[i].Complete) return "Brew " + cauldrons[i].brewName + ": " + string.Join(" > ", cauldrons[i].recipe.Select(x => x.ToString().ToUpperInvariant())) + ".\nE / choose candy. Q / throw into the pot.";
                if (cauldrons[i].GuardsRemaining > 0) return "Clear the " + cauldrons[i].GuardsRemaining + " remaining grove guardians.\nThen light the lantern to restore the next crossing.";
                if (!lights[i + 1].Lit) return "The potion dispelled the mist. Light the next lantern.\nSpace / jump. The teal platforms drift sideways.";
            }
            if (!lights[4].Lit) return towerGuards.Any(x => x != null && !x.Defeated) ?
                "Reach the watchtower. Clear its three guardians.\nLight the final lantern to reach the castle gate." :
                "Light the watchtower lantern.\nCross the last floating platforms to the castle gate.";
            return "The castle gate is open.\nFollow the glowing path through the doorway.";
        }
        void Update()
        {
            if (DreamGame.Instance.Phase != GamePhase.Playing) return;
            refill -= Time.deltaTime;
            if (refill > 0) return;
            refill = 3;
            var existing = FindObjectsByType<CandyPickup>(FindObjectsSortMode.None);
            foreach (var pot in cauldrons.Where(x => !x.Complete))
                for (int i = 0; i < pot.supplyPoints.Length; i++)
                {
                    var stock = existing.FirstOrDefault(x => Vector3.Distance(x.transform.position, pot.supplyPoints[i]) < 1);
                    if (stock == null) DreamWorld.Candy(transform, pot.supplyPoints[i], (CandyHue)i);
                    // Swapping leaves the old held color on the floor. Restore each slot's
                    // designated color so occupied wrong-color slots cannot block a recipe.
                    else if (stock.hue != (CandyHue)i) stock.SetHue((CandyHue)i);
                }
        }
    }
}
