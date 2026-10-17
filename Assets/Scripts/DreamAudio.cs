using UnityEngine;

namespace LostDream
{
    public class DreamAudio : MonoBehaviour
    {
        AudioSource effects;
        AudioSource ambience;
        AudioSource[] music;
        int activeMusic;
        bool musicPaused;
        static bool rememberedMute;
        [Range(0, 1)] public float musicVolume = .36f;
        public bool Muted => rememberedMute;
        public string MusicTrack { get; private set; }
        public bool MusicPaused => musicPaused;
        public bool MusicPlaying => music != null && music[activeMusic].isPlaying;
        public AudioClip MusicClip => music == null ? null : music[activeMusic].clip;
        public int MusicTimeSamples => MusicClip == null ? 0 : music[activeMusic].timeSamples;
        public float MusicGain => music == null ? 0 : music[activeMusic].volume;
        public int PlayingMusicSources => music == null ? 0 : System.Array.FindAll(music, x => x.isPlaying).Length;
        void Awake()
        {
            effects = gameObject.AddComponent<AudioSource>();
            effects.spatialBlend = 0;
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.clip = Resources.Load<AudioClip>("Audio/dream_ambience");
            ambience.loop = true;
            ambience.volume = .055f;
            ambience.Play();
            music = new[] { gameObject.AddComponent<AudioSource>(), gameObject.AddComponent<AudioSource>() };
            foreach (var source in music)
            {
                source.loop = true;
                source.playOnAwake = false;
                source.spatialBlend = 0;
                source.volume = 0;
                source.priority = 96;
            }
            AudioListener.volume = Muted ? 0 : 1;
        }
        void Update()
        {
            var game = DreamGame.Instance;
            if (game == null) return;
            bool paused = game.Phase == GamePhase.Paused;
            if (paused != musicPaused)
            {
                musicPaused = paused;
                foreach (var source in music) { if (paused) source.Pause(); else source.UnPause(); }
                if (paused) ambience.Pause(); else ambience.UnPause();
            }
            if (paused) return;
            bool finished = game.Phase == GamePhase.Defeated || game.Phase == GamePhase.LevelComplete;
            string desired = game.Phase == GamePhase.Victory ? "dream_celebration" : finished ? "" : game.boss != null && game.boss.AwakeInArena ?
                game.boss.SecondPhase ? "pumpkin_rage" : "pumpkin_battle" : "dream_journey";
            if (desired != MusicTrack)
            {
                MusicTrack = desired;
                if (desired.Length > 0)
                {
                    int next = 1 - activeMusic;
                    music[next].Stop();
                    music[next].clip = Resources.Load<AudioClip>("Audio/Music/" + desired);
                    music[next].volume = 0;
                    if (music[next].clip != null) music[next].Play();
                    activeMusic = next;
                }
            }
            for (int i = 0; i < music.Length; i++)
            {
                float target = desired.Length > 0 && i == activeMusic ? musicVolume * (game.Phase == GamePhase.Menu ? .65f : 1) : 0;
                music[i].volume = Mathf.MoveTowards(music[i].volume, target, Time.unscaledDeltaTime * Mathf.Max(.36f, musicVolume) / 1.5f);
                // Zero gain should stay silent and resume naturally when the Inspector
                // volume is raised; only outgoing or finished tracks stop their loop.
                if (target == 0 && music[i].volume == 0 && music[i].isPlaying &&
                    (i != activeMusic || desired.Length == 0)) music[i].Stop();
            }
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
            rememberedMute = !rememberedMute;
            AudioListener.volume = Muted ? 0 : 1;
        }
        void OnDestroy() { AudioListener.volume = 1; }
    }
}
