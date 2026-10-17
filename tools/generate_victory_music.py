"""Generate only the original major-key victory loop, preserving all prior scores."""
import json
from generate_music import OUT, render

if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    track = render('dream_celebration', 128, 1, celebration=True)
    record = {
        'source': 'Original project-authored algorithmic victory composition',
        'generator': 'tools/generate_victory_music.py and tools/generate_music.py',
        'samples': 'Synthesized oscillators and deterministic noise; no external samples or recordings',
        'harmony': 'C major, I-IV-V-I, ascending bell melody, plucked arpeggios and light drums',
        'format': '22050 Hz stereo PCM16 WAV', 'track': track,
    }
    (OUT / 'VICTORY_SOURCE.json').write_text(json.dumps(record, indent=2) + '\n')
    print(json.dumps(record, indent=2))
