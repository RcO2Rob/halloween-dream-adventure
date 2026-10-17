"""Original chapter scores; sampled instruments via pinned GeneralUser GS.

Run with a NumPy-enabled Python and clang++. Instrument data is downloaded to
ignored Builds/music-work, never packaged as a raw sample bank. See docs/MUSIC.md.
"""
from pathlib import Path
import hashlib
import json
import subprocess
import urllib.request
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / 'Builds/music-work'
OUT = ROOT / 'Assets/Resources/Audio/Music'
RATE = 32000
SF_COMMIT = '684543d5e5efaef08d02be50dcda8d552478fa60'
SF_HASH = '9575028c7a1f589f5770fccc8cff2734566af40cd26ed836944e9a5152688cfe'
# Program, stereo position, mix gain. All melodic presets use GM bank zero.
PARTS = [(8,.42,.28), (89,.3,.36), (73,.62,.42), (76,.68,.38),
         (53,.38,.28), (46,.72,.42), (48,.47,.24), (43,.5,.28),
         (11,.32,.32), (60,.6,.25), (0,.5,.36)]
# intro, theme, answer, contrasting bridge, developed theme, turnaround.
SECTIONS = ['Drifting into sleep', 'A distant light', 'Breath of the garden',
            'Beyond the mist', 'A remembered path', 'Floating home']
THEMES = {
    'dream_journey': [0,2,4,3,2,1,0,-1, 0,2,5,4,3,2,1,0],
    'shattered_gardens': [4,3,4,0,2,1,0,-1, 2,4,6,5,4,2,1,0],
    'witchlight_marsh': [0,4,3,2,1,-1,0,2, 5,4,2,3,1,0,-1,0],
    'pumpkin_battle': [0,0,4,3,2,0,-1,0, 2,2,5,4,3,2,1,0],
    'pumpkin_rage': [0,4,0,3,2,4,1,0, 5,4,6,5,3,2,1,0],
    'dream_celebration': [0,2,4,7,6,4,5,4, 3,5,7,6,4,2,1,0],
}
CONFIGS = [
    ('dream_journey',68,4,48,62,False,'Distant celesta, breathy flute, warm pads and slow harp'),
    ('shattered_gardens',72,4,48,60,False,'Airy bottle flute, soft piano and sustained strings'),
    ('witchlight_marsh',66,4,48,64,False,'Floating harp, wordless choir and sparse vibraphone'),
    ('pumpkin_battle',88,4,48,50,False,'Low string swells, distant horns and a restrained pulse'),
    ('pumpkin_rage',102,4,48,50,False,'Tense string swells and a deeper pulse beneath airy melody'),
    ('dream_celebration',84,4,48,60,True,'Warm major-key piano, flute and harp homecoming'),
]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def arrange(config, renderer, soundfont):
    name,bpm,meter,bars,root,major,description = config
    rng = np.random.default_rng(20261016 + bpm)
    beat = 60/bpm
    length = round(bars*meter*beat*RATE)
    events, notes = [], []
    scale = [0,2,4,5,7,9,11] if major else [0,2,3,5,7,8,10]
    def pitch(degree):
        octave,index = divmod(int(degree),7)
        return root + octave*12 + scale[index]
    def note(bar,at,degree,duration,part,velocity=.6):
        start = (bar*meter+at)*beat
        key = pitch(degree)
        duration = min(duration, meter*bars-bar*meter-at-.025)
        if duration <= 0: return
        vel = float(np.clip(velocity*rng.uniform(.92,1.06),.08,.9))
        # Subtle reproducible timing; downbeats stay aligned, phrases stay intact.
        sample = round((start + (rng.uniform(0,.012) if at else 0))*RATE)
        end = min(length-1,sample+round(duration*beat*RATE))
        events.extend([(sample,part,key,vel),(end,part,key,0)])
        notes.append([bar,round(at,3),part,key,round(duration,3),round(vel,3)])
    def drum(bar,at,key,velocity):
        sample = round((bar*meter+at)*beat*RATE)
        events.extend([(sample,11,key,velocity),(min(length-1,sample+round(.13*RATE)),11,key,0)])

    combat = name.startswith('pumpkin')
    rage = name == 'pumpkin_rage'
    garden = name == 'shattered_gardens'
    marsh = name == 'witchlight_marsh'
    victory = major
    motif = THEMES[name]
    # Eight-bar harmonic sentences, with a different route through the bridge.
    progression = [0,0,5,3,2,5,1,4] if not major else [0,5,3,4,0,2,3,4]
    bridge = [3,6,2,5,1,3,4,4]
    section_bars = bars/6
    for bar in range(bars):
        section = min(5,int(bar/section_bars))
        local = bar%8
        # Harmony breathes over two bars rather than bouncing every bar.
        harmony = (bridge if section==3 else progression)[(bar//2)%8]
        chord = [harmony,harmony+2,harmony+4]
        sparse = section in (0,5)
        energy = .66 if sparse else 1
        if section==4: energy=1.05
        # Warm sustained beds and slowly separated harp notes replace staccato
        # pizzicato. Long notes, silence and slower harmonic rhythm lead the mix.
        if bar%2==0:
            for i,degree in enumerate(chord):
                note(bar,i*.08,degree-7,7.5,1,.37*energy)
            note(bar,0,harmony-14,6.8,7,.4*energy)
            if section in (2,3,4) or combat:
                for degree in [chord[0],chord[2]]:
                    note(bar,.4,degree-7,6.7,6,.35*energy)
        if not sparse or bar%2==0:
            for i,at in enumerate([.35,2.4] if section%2 else [.8]):
                note(bar,at,chord[(i+bar)%3],2.4,5,.34*energy)
        if marsh and bar%4==0:
            note(bar,1.1,chord[1],5.5,4,.32*energy)
        if garden and section in (1,3,4) and bar%2:
            note(bar,.5,chord[0],2.6,10,.38*energy)
        # Gentle two-bar melodic breaths; every fourth bar is melody-free.
        lead = (2 if combat or victory else 3 if garden else 8 if marsh else 2)
        if section in (2,3): lead = 4 if marsh else 3
        if section==5: lead = 0 if not combat else 2
        if not (section==0 and bar<2):
            phrase_bar = bar%4
            pattern = [0,2.5] if phrase_bar%2==0 else [1]
            for i,at in enumerate(pattern):
                # Every fourth bar leaves a breathing space after its cadence.
                if phrase_bar==3: continue
                if sparse and i==1: continue
                degree = motif[((bar//4)*3+phrase_bar*2+i)%len(motif)]
                if section==3: degree=4-degree
                if section==4 and phrase_bar==2: degree+=2
                note(bar,at,degree,2.1 if i==0 else 1.3,lead,.43*energy)
        # Countermelody appears only in the reply and developed refrain.
        if section in (2,4) and bar%4==3:
            note(bar,.5,chord[1],2.4,0 if not combat else 9,.29)
        if victory and section in (1,2,4):
            if bar%2==0:
                for i,degree in enumerate(chord): note(bar,.2+i*.9,degree,2.2,10,.36)
        if combat:
            # A low heartbeat conveys danger; no snare/hi-hat groove or fills.
            for at in ([0,2] if rage else [0]): drum(bar,at,36,.32*energy)
            if bar%4==0: note(bar,.3,chord[0],4.8,9,.38*energy)
            if rage and not sparse:
                for at in [0,2]: note(bar,at,chord[2]-7,1.5,6,.38)
    # Render two complete cycles; the second contains the previous cycle's tails.
    # This preserves instrumental decay at the seam without cutting notes/fading
    # the entire piece to silence every time it repeats.
    repeated = events + [(s+length,c,k,v) for s,c,k,v in events]
    repeated.sort(key=lambda e:(e[0],e[3]>0))
    score = CACHE/(name+'.events')
    with score.open('w') as f:
        f.write(f'{RATE} {length*2} 12 {len(repeated)}\n')
        for program,pan,gain in PARTS: f.write(f'0 {program} {pan} {gain}\n')
        f.write('128 0 .5 .65\n')
        for event in repeated: f.write(' '.join(map(str,event))+'\n')
    raw = CACHE/(name+'.f32')
    subprocess.run([str(renderer),str(soundfont),str(score),str(raw)],check=True)
    audio = np.fromfile(raw,dtype='<f4').reshape(-1,2)[length:].astype(np.float64)
    # Longer, quieter circular reflections create space without a loud echo.
    wet=audio.copy()
    for seconds,gain in [(.137,.1),(.293,.085),(.461,.07),(.739,.05),(1.103,.035),(1.571,.02)]:
        wet += np.roll(audio,round(seconds*RATE),axis=0)[:,::-1]*gain
    wet -= wet.mean(axis=0)
    rms=float(np.sqrt(np.mean(wet**2)))
    gain=min(.7/float(np.max(np.abs(wet))), .08/max(rms,1e-8))
    wet*=gain
    # A 5ms interpolation removes a residual waveform step, not a musical fade.
    count=round(.005*RATE)
    delta=wet[-1]-wet[0]
    wet[:count]+=delta[None,:]*np.linspace(1,0,count)[:,None]
    assert np.max(np.abs(wet)) < .99
    path=OUT/(name+'.wav')
    with wave.open(str(path),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE)
        f.writeframes((wet*32767).astype('<i2').tobytes())
    # Four varied excerpts for a compact in-app listening comparison.
    if name in ('dream_journey','shattered_gardens','witchlight_marsh'):
        snippets=[]
        for fraction in (.17,.38,.55,.72):
            segment=wet[round(fraction*length):round(fraction*length)+RATE*6].copy()
            fade=np.linspace(0,1,round(.15*RATE))
            segment[:len(fade)]*=fade[:,None];segment[-len(fade):]*=fade[::-1,None]
            snippets.append(segment)
        preview=np.concatenate(snippets)
        preview_path=ROOT/'docs/qa/music-dreamlike'/f'{name}-preview.wav'
        preview_path.parent.mkdir(parents=True,exist_ok=True)
        with wave.open(str(preview_path),'wb') as f:
            f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE)
            f.writeframes((preview*32767).astype('<i2').tobytes())
    return {'track':name,'style':'Gentle ethereal dream atmosphere','bpm':bpm,'meter':meter,'bars':bars,'duration_seconds':length/RATE,
            'key':f'MIDI tonic {root}, '+('major' if major else 'natural minor'),
            'orchestration':description,'sections':SECTIONS,'note_count':len(notes),
            'peak':float(np.max(np.abs(wet))),'rms':float(np.sqrt(np.mean(wet**2))),
            'boundary_step':float(np.max(np.abs(wet[-1]-wet[0]))),
            'section_rms':[float(np.sqrt(np.mean(s*s))) for s in np.array_split(wet,6)],
            'sha256':digest(path),'notes':notes}


def main():
    CACHE.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
    soundfont=CACHE/'GeneralUser-GS.sf2'
    if not soundfont.exists():
        urllib.request.urlretrieve(f'https://raw.githubusercontent.com/mrbumpy409/GeneralUser-GS/{SF_COMMIT}/GeneralUser-GS.sf2',soundfont)
    if digest(soundfont)!=SF_HASH: raise ValueError('SoundFont hash mismatch')
    renderer=CACHE/'render_soundfont'
    subprocess.run(['clang++','-O2','-std=c++11',str(ROOT/'tools/render_soundfont.cpp'),'-o',str(renderer)],check=True)
    tracks=[]
    for config in CONFIGS:
        record=arrange(config,renderer,soundfont)
        (OUT/(config[0]+'.score.json')).write_text(json.dumps(record,indent=2)+'\n')
        tracks.append({k:v for k,v in record.items() if k!='notes'})
        print(config[0],round(record['duration_seconds'],2),'seconds',flush=True)
    provenance={'version':3,'style':'Gentle ethereal dream atmosphere, selected by the user',
                'composition':'Original project note sequences arranged with Codex assistance; no borrowed songs.',
                'instrument_bank':{'author':'S. Christian Collins','name':'GeneralUser GS 2.0.3','commit':SF_COMMIT,'sha256':SF_HASH,
                                   'url':'https://www.schristiancollins.com/generaluser.php','license':'Assets/ThirdParty/Music/GeneralUser-GS-LICENSE.txt'},
                'renderer':{'name':'TinySoundFont','commit':'853a0a171759f1ddba0de1442133a75912bbeffa','license':'tools/vendor/tinysoundfont/LICENSE'},
                'sample_rate':RATE,'channels':2,'tracks':tracks,'independent_listener_review':False}
    (OUT/'SOURCE.json').write_text(json.dumps(provenance,indent=2)+'\n')
    (OUT/'VICTORY_SOURCE.json').write_text(json.dumps({'source':'SOURCE.json','track':'dream_celebration','generator':'tools/arrange_soundtrack.py'},indent=2)+'\n')
    # Continuous twelve-second phrases, chapters one through three in order.
    snippets=[]
    for record in tracks[:3]:
        with wave.open(str(OUT/(record['track']+'.wav')),'rb') as f:
            data=np.frombuffer(f.readframes(f.getnframes()),'<i2').reshape(-1,2).astype(float)
        start=round(8*record['meter']*60/record['bpm']*RATE)
        segment=data[start:start+12*RATE].copy()
        fade=np.linspace(0,1,round(.3*RATE))
        segment[:len(fade)]*=fade[:,None];segment[-len(fade):]*=fade[::-1,None]
        snippets.append(segment)
    preview=ROOT/'docs/qa/music-dreamlike/three-chapters-preview.wav'
    with wave.open(str(preview),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE)
        f.writeframes(np.concatenate(snippets).astype('<i2').tobytes())

if __name__=='__main__': main()
