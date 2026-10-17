# Chapter soundtrack refresh

The earlier three exploration chapters shared a 46.8-second oscillator loop. The
new arrangements use sampled instrument timbres, separate chapter themes, and a
six-part form: quiet introduction, main theme, instrumental reply, contrasting
bridge, developed refrain, and a quieter turnaround. At the user's request the
current direction is gentle and ethereal: slow harmonic changes, held warm pads,
wordless choir, breathing melodies and sparse harp notes replace bouncy plucked
strings and percussion. Exploration and victory have no drums; Boss stages use
only a restrained low heartbeat. Melodies occupy a softer middle register, and
the mix targets a lower RMS level than the earlier whimsical arrangement.

| Use | Resource | Length | Tempo | Character |
| --- | --- | --- | --- | --- |
| Cover, tutorial, castle approach | `dream_journey` | 169.4 s | 68 BPM | Breathy flute, distant celesta, warm pads, slow harp |
| Shattered Gardens | `shattered_gardens` | 160 s | 72 BPM | Airy bottle flute, soft piano, held strings |
| Witchlight Marsh | `witchlight_marsh` | 174.5 s | 66 BPM | Floating harp, wordless choir, sparse vibraphone |
| Boss first phase | `pumpkin_battle` | 130.9 s | 88 BPM | Low string swells, distant horns, restrained pulse |
| Boss half health | `pumpkin_rage` | 112.9 s | 102 BPM | Tense swells, deeper pulse, airy melody |
| Victory | `dream_celebration` | 137.1 s | 84 BPM | Warm major-key piano, flute, harp homecoming |

The cover retains the tutorial motif at reduced gain. Completing an objective
does not restart a song. Boss awakening, half health and victory select separate
pieces using the existing 1.5-second crossfade. Pause, mute and Inspector volume
retain their behavior. No saved scene placement or gameplay balance is changed.

## Reproduce

Run `python3 tools/arrange_soundtrack.py` with NumPy and `clang++` installed.
The script downloads a pinned bank into ignored `Builds/music-work`, verifies its
SHA-256, compiles the local offline renderer, then writes WAVs, note scores and
provenance under `Assets/Resources/Audio/Music`. Rendering two complete cycles and
keeping the second captures previous-cycle instrument tails; circular reflections
and a 5 ms boundary correction remove residual seam discontinuity. Peak is kept
below full scale; RMS gain is limited so event effects have room in the mix.

Original note sequences and orchestration are project work with Codex assistance.
Instrument recordings are **GeneralUser GS 2.0.3** by **S. Christian Collins**,
not project-created samples. The creator's [license](https://github.com/mrbumpy409/GeneralUser-GS/blob/684543d5e5efaef08d02be50dcda8d552478fa60/documentation/LICENSE.txt)
permits private/commercial music creation. It also describes uncertainty about
the original provenance of some historical samples; the full unedited notice is
retained in `Assets/ThirdParty/Music/GeneralUser-GS-LICENSE.txt`.

- Bank commit: `684543d5e5efaef08d02be50dcda8d552478fa60`
- Bank SHA-256: `9575028c7a1f589f5770fccc8cff2734566af40cd26ed836944e9a5152688cfe`
- [TinySoundFont](https://github.com/schellingb/TinySoundFont) commit: `853a0a171759f1ddba0de1442133a75912bbeffa`
- Header SHA-256: `70d55963c98f60ebb81518eaa1f25d46888d5180eb5f5289fd6b74ffc177d197`

TinySoundFont and its MIT notice are vendored only for offline regeneration.
The selected GM instruments render through its supported synthesis path; no
claim of full GeneralUser GS modulator/effects emulation is made. Unity ships
rendered WAVs, not the SoundFont or the renderer. Earlier WAVs are preserved in
ignored `Builds/archive/music-before-20261016`. The first whimsical refresh is
also retained under `Builds/archive/music-whimsical-20261016`. The old oscillator scripts now
write into `Builds/archive/legacy-procedural-music` to avoid replacing this score.

## Validation

`--smoke-test --music-smoke` exercises actual chapter track selection, Boss state
changes, overlapping/final crossfade sources, playback continuity, volume zero,
mute, pause and resume. PCM checks measure clipping, channel signal, DC and loop
seams. These checks establish signal integrity and integration, not listening
quality. No independent listener review is claimed; 24-second exploration excerpts
are retained in `docs/qa/music-dreamlike` for listening feedback. Earlier whimsical-version QA
remains in `docs/qa/music-refresh`; its hashes refer to that earlier arrangement.
