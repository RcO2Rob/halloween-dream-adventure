using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostDream
{
    // Survives scene replacement so there is never an uncovered loading frame.
    public class DreamTransition : MonoBehaviour
    {
        static DreamTransition current;
        public static bool Running => current != null;
        public static float BlackOpacity => current == null ? 0 : current.opacity;
        float opacity;
        public static void Begin(int chapter)
        {
            if (Running || chapter >= DreamGame.SceneNames.Length) return;
            current = new GameObject("Automatic chapter fade").AddComponent<DreamTransition>();
            DontDestroyOnLoad(current.gameObject);
            current.StartCoroutine(current.Cross(chapter));
        }
        IEnumerator Cross(int chapter)
        {
            Time.timeScale = 0;
            yield return Fade(0, 1, .45f);
            var load = SceneManager.LoadSceneAsync(DreamGame.SceneNames[chapter]);
            while (!load.isDone) yield return null;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.15f);
            yield return Fade(1, 0, .55f);
            current = null;
            DreamGame.Instance.FinishTransition();
            Destroy(gameObject);
        }
        IEnumerator Fade(float from, float to, float seconds)
        {
            for (float elapsed = 0; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            { opacity = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / seconds)); yield return null; }
            opacity = to;
        }
        void OnGUI()
        {
            GUI.depth = -10000;
            Color old = GUI.color; GUI.color = new Color(0, 0, 0, opacity);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
