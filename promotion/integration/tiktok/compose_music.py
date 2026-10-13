"""
ファイル名 / File: compose_music.py
概要 / Description: 軽快なオリジナル5拍子曲を作曲・合成 / Compose an original upbeat 5/4 cue
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office（音楽生成 / music generation: Codex）
作成日 / Created: 2026-10-14
ライセンス / License: MIT（コードのみ / code only; music license not specified）
SPDX-License-Identifier: MIT
"""
import hashlib,json,struct,subprocess,wave
from pathlib import Path
import numpy as np

HERE=Path(__file__).resolve().parent
OUT=HERE/'music';OUT.mkdir(exist_ok=True)
SR=44100;BPM=125;BEAT=60/BPM;DURATION=49
audio=np.zeros((SR*DURATION,2),dtype=np.float64)
notes=[]
rng=np.random.default_rng(514)

def add(midi,beat,length,level,voice='pluck',pan=0):
    start=int(beat*BEAT*SR)
    seconds=length*BEAT
    tail=.38 if voice=='pluck' else .12
    t=np.arange(int((seconds+tail)*SR))/SR
    freq=440*2**((midi-69)/12)
    attack=1-np.exp(-t*650)
    if voice=='pluck':
        sound=(np.sin(2*np.pi*freq*t)+.24*np.sin(2*np.pi*freq*2*t)+.08*np.sin(2*np.pi*freq*3*t))*attack*np.exp(-t/ .19)
    elif voice=='bass':
        sound=(np.sin(2*np.pi*freq*t)+.12*np.sin(2*np.pi*freq*2*t))*attack*np.minimum(1,np.maximum(0,(seconds+.1-t)/.1))*np.exp(-t/.5)
    else:
        sound=(np.sin(2*np.pi*freq*t)+.18*np.sin(2*np.pi*freq*2*t))*attack*np.exp(-t/.33)
    for channel,gain in enumerate((np.sqrt((1-pan)/2),np.sqrt((1+pan)/2))):
        n=min(len(sound),len(audio)-start)
        if n>0: audio[start:start+n,channel]+=sound[:n]*level*gain
    notes.append({'note':midi,'beat':beat,'length':length,'voice':voice,'velocity':min(110,int(level*200)+35)})

def tick(beat,level):
    start=int(beat*BEAT*SR);t=np.arange(int(.045*SR))/SR
    noise=rng.standard_normal(len(t));noise[1:]-=.75*noise[:-1].copy()
    sound=noise*np.exp(-t/.008)*(1-np.exp(-t*1800))*level
    audio[start:start+len(t)]+=sound[:,None]

# Original melody: no official theme audio or MIDI is imported.
# A 3+2 groove, short eighth-note replies, and a gentle accent on beat five.
patterns=[
 [78,81,83,81,78,76,78,81],
 [83,81,78,74,78,81,78,76],
 [76,78,81,83,86,83,81,78],
 [81,78,76,74,76,78,81,74]]
positions=[0,.5,1.5,2,3,3.5,4,4.5]
harmonies=[(50,[62,66,69]),(47,[59,62,66]),(52,[64,69,71]),(45,[57,64,69])]
for bar in range(20):
    base=bar*5;root,chord=harmonies[bar%4]
    for b,l in [(0,.55),(2,.4),(3,.6),(4,.35)]:add(root,base+b,l,.17,'bass')
    for b in [0,1.5,3,4]:
        for j,n in enumerate(chord):add(n,base+b,.28,.035,'keys',pan=(j-1)*.35)
    for b in np.arange(0,5,.5):tick(base+float(b),.010 if b not in (0,3,4) else .019)
    melody=patterns[bar%4]
    if bar<2: melody=melody[:5]
    for i,n in enumerate(melody):
        if bar==19 and positions[i]>3:continue
        add(n,base+positions[i],.32,.105 if positions[i]!=4 else .14,'pluck',pan=(-.15 if i%2 else .15))
    if 8<=bar<16:
        for i,b in enumerate([1,2.5,4]):add(chord[i]+12,base+b,.2,.045,'pluck',pan=.35)
# Resolve after twenty bars (48 seconds), leaving a one-second musical tail.
for n in (50,62,66,69,74):add(n,100,.5,.08 if n<60 else .055,'keys')
# Small stereo echoes add space without long pads or heavy percussion.
dry=audio.copy()
for delay,gain in [(.12,.12),(.24,.055)]:
    k=int(delay*SR);audio[k:]+=dry[:-k,::-1]*gain
fade=int(.025*SR);audio[:fade]*=np.linspace(0,1,fade)[:,None]
fade=int(.65*SR);audio[-fade:]*=np.linspace(1,0,fade)[:,None]
audio*=.48/np.max(np.abs(audio))
wav=OUT/'mdeditor-light-steps-v1.wav'
assert not wav.exists(),'Preserve the existing composition; use a new version to revise it'
with wave.open(str(wav),'wb') as f:
    f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR)
    f.writeframes((audio*32767).astype('<i2').tobytes())
score={'title':'Light Steps / 軽快なステップ','version':'1.0.0','date':'2026-10-14','composition_and_synthesis':'Codex, commissioned by 5garashi.com Design Office','music_license':'not specified','original_composition':True,'official_theme_used':False,'bpm':BPM,'meter':'5/4','grouping':'3+2','duration_seconds':DURATION,'sample_rate':SR,'notes':notes}
(OUT/'score-v1.json').write_text(json.dumps(score,ensure_ascii=False,indent=2),encoding='utf-8')

def vlq(value):
    data=[value&127];value>>=7
    while value:data.insert(0,(value&127)|128);value>>=7
    return bytes(data)
events=[(0,bytes([0xff,0x51,3])+int(60000000/BPM).to_bytes(3,'big')),(0,bytes([0xff,0x58,4,5,2,24,8]))]
programs={'pluck':(0,10),'bass':(1,33),'keys':(2,4)}
for ch,program in programs.values():events.append((0,bytes([0xc0+ch,program])))
for n in notes:
    ch=programs[n['voice']][0]
    events.append((round(n['beat']*480),bytes([0x90+ch,n['note'],n['velocity']])))
    events.append((round((n['beat']+n['length'])*480),bytes([0x80+ch,n['note'],0])))
track=b'';previous=0
for tick_at,event in sorted(events,key=lambda e:e[0]):track+=vlq(tick_at-previous)+event;previous=tick_at
track+=b'\x00\xff\x2f\x00'
(OUT/'mdeditor-light-steps-v1.mid').write_bytes(b'MThd'+struct.pack('>IHHH',6,0,1,480)+b'MTrk'+struct.pack('>I',len(track))+track)
subprocess.run(['ffmpeg','-y','-v','error','-i',str(wav),'-c:a','libmp3lame','-b:a','192k',str(OUT/'mdeditor-light-steps-v1.mp3')],check=True)
silent=HERE/'video/mdEditor-demo-vertical-en.mp4'
video=HERE/'video/mdEditor-demo-vertical-en-light-steps-v1.mp4'
assert not video.exists()
subprocess.run(['ffmpeg','-v','error','-i',str(silent),'-i',str(wav),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-t','49','-movflags','+faststart','-metadata','title=mdEditor Demo — Light Steps v1','-metadata','copyright=Original music generated by Codex for 5garashi.com Design Office; music license not specified; captions MIT; official logo CC BY-ND 4.0','-metadata','comment=Original 125 BPM 5/4 music, 3+2 groove; official theme not used; actual Windows operations',str(video)],check=True)
def frames(p):return hashlib.sha256(subprocess.check_output(['ffmpeg','-v','error','-i',str(p),'-map','0:v:0','-f','framemd5','-'])).hexdigest()
assert frames(silent)==frames(video)
(OUT/'validation-v1.json').write_text(json.dumps({'video_frames_unchanged':True,'wav_sha256':hashlib.sha256(wav.read_bytes()).hexdigest(),'peak_dbfs':float(20*np.log10(np.max(np.abs(audio)))),'duration_seconds':49,'bpm':125,'meter':'5/4','published':False},indent=2),encoding='utf-8')
print('Composed Light Steps v1: original 5/4, 125 BPM, WAV/MIDI/MP3 and captioned demo video')
