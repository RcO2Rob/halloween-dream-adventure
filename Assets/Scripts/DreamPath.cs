using UnityEngine;

namespace LostDream
{
    public class DreamPath : MonoBehaviour
    {
        public GameObject solid, nightmare, outline;
        public Transform[] jumpPlatforms = new Transform[0];
        public Transform jumpEntry, jumpExit;
        public bool IsJumpRoute => jumpPlatforms != null && jumpPlatforms.Length > 0;
        public bool Revealed { get; private set; }
        float transition;
        void Start() { solid.SetActive(false); outline.SetActive(true); }
        public void Reveal()
        {
            Revealed = true;
            solid.SetActive(true);
            outline.SetActive(false);
            transition = 1;
        }
        void Update()
        {
            if (Revealed)
            {
                transition = Mathf.MoveTowards(transition, 0, Time.deltaTime * .7f);
                nightmare.transform.localScale = Vector3.one * Mathf.Max(.001f, transition);
                if (transition <= 0) nightmare.SetActive(false);
            }
            else
            {
                float pulse = 1 + Mathf.Sin(Time.time * 1.8f) * .045f;
                nightmare.transform.localScale = Vector3.one * pulse;
            }
        }
    }
}
