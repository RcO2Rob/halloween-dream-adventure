using UnityEngine;

namespace LostDream
{
    public class DreamAudio : MonoBehaviour
    {
        AudioSource effects;
        AudioSource ambience;
        public bool Muted { get; private set; }
        void Awake()
        {
            effects = gameObject.AddComponent<AudioSource>();
            effects.spatialBlend = 0;
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.clip = Resources.Load<AudioClip>("Audio/dream_ambience");
            ambience.loop = true;
            ambience.volume = .15f;
            ambience.Play();
        }
        public void Play(string name, float volume = .6f, float pitch = 1)
        {
            if (Muted || effects == null) return;
            var clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip == null) return;
            effects.pitch = pitch;
            effects.PlayOneShot(clip, volume);
        }
        public void ToggleMute()
        {
            Muted = !Muted;
            AudioListener.volume = Muted ? 0 : 1;
        }
        void OnDestroy() { AudioListener.volume = 1; }
    }
}
