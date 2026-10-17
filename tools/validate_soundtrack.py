"""Signal-integrity checks for the rendered score; not a listening review."""
from pathlib import Path
import hashlib
import json
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
MUSIC = ROOT/'Assets/Resources/Audio/Music'
QA = ROOT/'docs/qa/music-dreamlike'


def main():
    source=json.loads((MUSIC/'SOURCE.json').read_text())
    results=[]
    for record in source['tracks']:
        path=MUSIC/(record['track']+'.wav')
        with wave.open(str(path),'rb') as f:
            assert (f.getnchannels(),f.getsampwidth(),f.getframerate())==(2,2,32000)
            audio=np.frombuffer(f.readframes(f.getnframes()),'<i2').reshape(-1,2).astype(float)/32768
        peak=float(np.max(np.abs(audio)))
        rms=np.sqrt(np.mean(audio**2,axis=0))
        dc=np.mean(audio,axis=0)
        boundary=float(np.max(np.abs(audio[0]-audio[-1])))
        # Sections must carry usable signal, and a quiet/loud contrast must
        # actually exist in the recording rather than only in score metadata.
        sections=[float(np.sqrt(np.mean(s*s))) for s in np.array_split(audio,6)]
        stereo=float(np.sqrt(np.mean((audio[:,0]-audio[:,1])**2)))
        assert peak<.85 and np.all(rms>.025),record['track']+': signal/headroom'
        assert np.max(np.abs(dc))<.001,record['track']+': DC offset'
        assert boundary<2/32768,record['track']+': loop discontinuity'
        assert min(sections)>.02 and max(sections)/min(sections)>1.35,record['track']+': arrangement dynamics'
        assert stereo>.003,record['track']+': stereo information'
        assert hashlib.sha256(path.read_bytes()).hexdigest()==record['sha256']
        results.append({'track':record['track'],'peak':peak,'rms_by_channel':rms.tolist(),
                        'dc_by_channel':dc.tolist(),'loop_boundary_step':boundary,
                        'section_rms':sections,'stereo_difference_rms':stereo,'passed':True})
    before=json.loads((ROOT/'Builds/music-work/scene-hashes-before.json').read_text())
    scenes_unchanged=all(hashlib.sha256((ROOT/'Assets/Scenes'/name).read_bytes()).hexdigest()==sha for name,sha in before.items())
    assert scenes_unchanged,'A saved map changed during music work'
    QA.mkdir(parents=True,exist_ok=True)
    (QA/'signal-validation.json').write_text(json.dumps({'passed':True,'tracks':results,
        'saved_maps_unchanged_from_task_start':True,'scene_sha256':before,
        'independent_listener_review':False},indent=2)+'\n')
    print('Six scores passed PCM, hash, stereo, dynamics and seam checks; four saved maps unchanged.')


if __name__=='__main__': main()
