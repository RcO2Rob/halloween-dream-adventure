using UnityEngine;

namespace LostDream
{
    public abstract class DreamInteractable : MonoBehaviour
    {
        public abstract bool Available { get; }
        public abstract string Prompt { get; }
        public abstract bool Use(PlayerMotor player);
    }
}
