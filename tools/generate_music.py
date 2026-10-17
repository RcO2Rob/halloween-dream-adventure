"""Archive the legacy v0.10 oscillator scores. Run with Python 3 and NumPy.

No recordings, samples, external compositions or downloaded music are used.
The orchestration and note sequences below are project source.
"""
from pathlib import Path
import hashlib
import json
import wave
import numpy as np

RATE = 22050
OUT = Path(__file__).resolve().parents[1] / "Builds/archive/legacy-procedural-music"


def frequency(note):
    return 440 * 2 ** ((note - 69) / 12)


def render(name, bpm, intensity, celebration=False):
    beat = 60 / bpm
    duration = 64 * beat
    length = round(duration * RATE)
    score = np.zeros((length, 2), dtype=np.float64)
    rng = np.random.default_rng(20261011 + intensity)

    def add(start, note, seconds, gain, instrument="bell", pan=0):
        count = max(2, round(seconds * RATE))
        t = np.arange(count) / RATE
        f = frequency(note)
        attack = np.minimum(1, t / .025)
        release = np.minimum(1, (seconds - t) / .1)
        if instrument == "bell":
            signal = (np.sin(2*np.pi*f*t) * np.exp(-t*2.3) +
                      .28*np.sin(2*np.pi*f*2*t)*np.exp(-t*4) +
                      .13*np.sin(2*np.pi*f*3*t)*np.exp(-t*6))
        elif instrument == "pad":
            signal = sum(np.sin(2*np.pi*(f*(1 + detune))*t) for detune in [-.0015, 0, .0015]) / 3
            signal += .18*np.sin(2*np.pi*f*2*t)
            attack = np.minimum(1, t / .35)
        elif instrument == "bass":
            signal = (np.sin(2*np.pi*f*t) + .23*np.sin(2*np.pi*f*2*t))*np.exp(-t*1.6)
        else:  # soft plucked strings
            signal = sum(np.sin(2*np.pi*f*h*t)*np.exp(-t*(2+h*.7))/h**1.8 for h in range(1, 7))
        signal *= gain * attack * np.clip(release, 0, 1)
        stereo = signal[:, None] * np.array([np.sqrt((1-pan)/2), np.sqrt((1+pan)/2)])
        # Wrap tails into the start of the bar cycle for a seamless loop.
        indexes = (round(start * RATE) + np.arange(count)) % length
        np.add.at(score, indexes, stereo)

    def drum(start, kind, gain):
        seconds = .24 if kind == "kick" else .11
        count = round(seconds * RATE)
        t = np.arange(count) / RATE
        if kind == "kick":
            phase = 2*np.pi*(49*t + 55*(1-np.exp(-t*25))/25)
            signal = np.sin(phase)*np.exp(-t*19)
        else:
            noise = rng.normal(0, 1, count)
            signal = np.diff(noise, prepend=noise[0]) * np.exp(-t*(45 if kind == "hat" else 27))
            if kind == "snare": signal = .55*signal + .4*np.sin(2*np.pi*180*t)*np.exp(-t*25)
        signal *= gain*np.minimum(1,t/.004)*np.minimum(1,(seconds-t)/.015)
        indexes = (round(start*RATE) + np.arange(count)) % length
        np.add.at(score, indexes, signal[:, None]*np.array([.707, .707]))

    # A minor: Am(add9), Fmaj7, C(add9), E minor; tension comes from rhythm.
    chords = [(45, [57, 60, 64, 71]), (41, [57, 60, 64, 65]),
              (48, [55, 60, 64, 74]), (40, [55, 59, 64, 67])]
    melody = [[76, 79, 83, 81, 79, 76], [77, 81, 84, 83, 81, 79],
              [79, 76, 74, 72, 76, 79], [78, 76, 74, 71, 74, 76]]
    if celebration:
        # Bright C-major I-IV-V-I harmony and a new ascending victory melody.
        chords = [(48, [60, 64, 67, 72]), (41, [57, 60, 65, 69]),
                  (43, [59, 62, 67, 74]), (48, [60, 64, 67, 76])]
        melody = [[76, 79, 84, 83, 81, 79], [77, 81, 84, 86, 84, 81],
                  [79, 83, 86, 84, 83, 79], [84, 79, 76, 79, 83, 84]]
    for bar in range(16):
        start = bar*4*beat
        bass, tones = chords[(bar//2) % 4]
        for index, tone in enumerate(tones):
            add(start, tone, 4.4*beat, .042 if intensity == 0 else .028, "pad", (index-1.5)*.2)
        for tick in range(8):
            note = tones[[0, 2, 1, 3, 2, 1, 3, 2][tick]] + (12 if intensity == 0 else 0)
            add(start + tick*.5*beat, note, 1.2*beat, .072 if intensity == 0 else .095,
                "bell" if intensity == 0 else "pluck", -.32 if tick % 2 == 0 else .32)
        for tick in ([0, 2] if intensity == 0 else [0, 1.5, 2, 3]):
            add(start + tick*beat, bass, 1.15*beat, .105 if intensity == 0 else .17, "bass")
        if bar % 2 == 0:
            phrase = melody[(bar//2) % 4]
            for index, note in enumerate(phrase):
                add(start + [.0, 1, 2, 3, 4.5, 6][index]*beat, note,
                    1.7*beat, .105 if intensity == 0 else .11, "bell", .13)
        if intensity > 0:
            for tick in [0, 2]: drum(start + tick*beat, "kick", .23)
            for tick in [1, 3]: drum(start + tick*beat, "snare", .052)
            for tick in range(8): drum(start + tick*.5*beat, "hat", .019 if tick % 2 else .026)
            if intensity == 2:
                drum(start+3.5*beat, "kick", .17)
                for tick in range(16):
                    add(start+tick*.25*beat, tones[tick % 4]+12, .55*beat, .042, "pluck", -.3)

    # Circular room echoes preserve the seam instead of cutting off reverb tails.
    dry = score.copy()
    for delay, amount in [(.137,.16),(.293,.11),(.431,.07)]:
        score += np.roll(dry[:, ::-1], round(delay*RATE), axis=0)*amount
    score -= score.mean(axis=0)
    peak = np.abs(score).max()
    score *= .78 / max(peak, 1e-9)
    if celebration:
        # A six-millisecond seam envelope removes the final oscillator discontinuity.
        edge = round(RATE * .006)
        score[:edge] *= np.linspace(0, 1, edge)[:, None]
        score[-edge:] *= np.linspace(1, 0, edge)[:, None]
    pcm = np.round(score*32767).astype('<i2')
    path = OUT / (name+'.wav')
    with wave.open(str(path),'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(RATE); f.writeframes(pcm.tobytes())
    return {'file':path.name,'bpm':bpm,'bars':16,'seconds':length/RATE,
            'peak':float(np.abs(score).max()),'rms':float(np.sqrt(np.mean(score**2))),
            'loop_edge_delta':float(np.max(np.abs(score[0]-score[-1]))),
            'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    tracks = [render('dream_journey', 82, 0), render('pumpkin_battle', 112, 1), render('pumpkin_rage', 124, 2)]
    manifest = {'source':'Original algorithmic compositions authored for this project with Codex assistance',
                'samples':'Synthesized oscillators and deterministic noise only; no external samples or music',
                'format':'22050 Hz stereo PCM16 WAV', 'generator':'tools/generate_music.py','tracks':tracks}
    (OUT/'SOURCE.json').write_text(json.dumps(manifest,indent=2)+'\n')
    print(json.dumps(manifest,indent=2))
