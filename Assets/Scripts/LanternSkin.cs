using UnityEngine;

namespace LostDream
{
    // Artwork follows the existing switch state; the Lantern retains all gameplay.
    [DisallowMultipleComponent, RequireComponent(typeof(Lantern))]
    public class LanternSkin : MonoBehaviour
    {
        public Lantern lantern;
        public GameObject unlit, lit;

        void OnEnable() { Refresh(); }
        void LateUpdate() { Refresh(); }

        public void Refresh()
        {
            if (lantern == null || unlit == null || lit == null) return;
            if (unlit.activeSelf == lantern.Lit) unlit.SetActive(!lantern.Lit);
            if (lit.activeSelf != lantern.Lit) lit.SetActive(lantern.Lit);
        }
    }
}
