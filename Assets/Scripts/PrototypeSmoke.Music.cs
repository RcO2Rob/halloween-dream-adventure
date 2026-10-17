using System.Collections;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator SoundtrackReview()
        {
            DreamGame.Instance.player.automation = true;
            yield return MusicChecks();
            foreach (string name in new[] { "dream_journey", "shattered_gardens", "witchlight_marsh", "pumpkin_battle", "pumpkin_rage", "dream_celebration" })
            {
                var clip = Resources.Load<AudioClip>("Audio/Music/" + name);
                Require(clip != null && clip.channels == 2 && clip.length >= 60,
                    "Longer arranged stereo score imports: " + name);
            }
            string[] exploration = { "dream_journey", "shattered_gardens", "witchlight_marsh", "dream_journey" };
            for (int chapter = 0; chapter < 4; chapter++)
            {
                DreamGame.Instance.LoadChapter(chapter);
                yield return new WaitForSecondsRealtime(2);
                var game = DreamGame.Instance; game.player.automation = true;
                var audio = game.audioSystem;
                Require(audio.MusicTrack == exploration[chapter] && audio.MusicPlaying && audio.PlayingMusicSources == 1,
                    "Chapter " + (chapter+1) + " selects its exploration score without overlapping loops");
                int before = audio.MusicTimeSamples;
                yield return new WaitForSecondsRealtime(.3f);
                Require(audio.MusicTimeSamples > before,"Ordinary exploration advances the same arrangement without restarting: " + (chapter+1));
            }
            var bossGame = DreamGame.Instance;
            var player = bossGame.player;
            player.Teleport(bossGame.lanterns[0].safeCheckpoint);
            yield return new WaitForSeconds(.1f);
            Require(player.Interact(), "Restore the castle lantern before entering combat");
            player.Teleport(new Vector3(0,.12f,6));
            yield return new WaitForSecondsRealtime(.2f);
            Require(bossGame.boss.AwakeInArena && bossGame.audioSystem.MusicTrack == "pumpkin_battle" &&
                bossGame.audioSystem.PlayingMusicSources == 2,
                "Entering the courtyard crossfades from exploration to the Boss score");
            yield return new WaitForSecondsRealtime(1.7f);
            Require(bossGame.audioSystem.PlayingMusicSources == 1,"Combat crossfade settles to one source");
            Time.timeScale = 3;
            float deadline = Time.time + 80;
            while (!bossGame.boss.SecondPhase && Time.time < deadline)
            {
                player.Heal(5);
                if (bossGame.boss.Exposed) bossGame.boss.Hit(true,bossGame.boss.RequiredHue);
                yield return null;
            }
            Time.timeScale = 1;
            yield return null;
            Require(bossGame.boss.SecondPhase && bossGame.audioSystem.MusicTrack == "pumpkin_rage",
                "Real matching-color hits at half health select the faster orchestral score");
            bossGame.boss.enabled = false;
            bossGame.boss.ClearSummons();
            player.Teleport(new Vector3(-9,.12f,4)); player.Heal(5);
            yield return new WaitForSecondsRealtime(1.7f);
            Require(DreamGame.Instance.audioSystem.PlayingMusicSources == 1,
                "Half-health crossfade finishes with one playing music source");
            bossGame.Win();
            yield return new WaitForSecondsRealtime(.2f);
            Require(bossGame.audioSystem.MusicTrack == "dream_celebration" && bossGame.audioSystem.PlayingMusicSources == 2,
                "Victory begins a crossfade from combat into the major-key homecoming");
            yield return new WaitForSecondsRealtime(1.7f);
            Require(bossGame.audioSystem.PlayingMusicSources == 1 && bossGame.audioSystem.MusicGain > .1f,
                "Victory crossfade leaves one audible celebration score");
            Save(true, ""); Debug.Log("LOST_DREAM_MUSIC_SMOKE_PASSED: " + checks.Count);
            Application.Quit(0);
        }
    }
}
