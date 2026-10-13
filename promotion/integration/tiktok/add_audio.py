"""
ファイル名 / File: promotion/integration/tiktok/add_audio.py
概要 / Description: 公式5拍子テーマを縦動画へ追加 / Add official 5/4 theme to the vertical demo
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
ライセンス / License: MIT（コードのみ / code only）
SPDX-License-Identifier: MIT
"""
import argparse, hashlib, json, subprocess, urllib.request
from pathlib import Path

HERE=Path(__file__).resolve().parent
parser=argparse.ArgumentParser()
parser.add_argument('--validate-only',action='store_true',help='Verify an existing render without overwriting it')
args=parser.parse_args()
WORK=HERE.parents[3]/'work/tiktok-audio-v1'
WORK.mkdir(parents=True,exist_ok=True)
COMMIT='c53f255c75b258c5ad320f56e2310cefd6a71b10'
URL=f'https://raw.githubusercontent.com/5garashi/5garashi-identity/{COMMIT}/assets/5garashi_theme.wav'
SHA256='2cb1ed95fab38404aa4c592185ab5c8cd26462d6f9b1e08323bec6d5952fb2de'
source=WORK/'5garashi_theme.wav'
if not source.exists():
    data=urllib.request.urlopen(URL,timeout=60).read()
    assert hashlib.sha256(data).hexdigest()==SHA256,'Official audio checksum differs'
    source.write_bytes(data)
assert hashlib.sha256(source.read_bytes()).hexdigest()==SHA256
silent=HERE/'video/mdEditor-demo-vertical-en.mp4'
output=HERE/'video/mdEditor-demo-vertical-en-bgm.mp4'
if not args.validate_only:
    assert not output.exists(),'Keep an existing BGM render; use --validate-only to verify it.'
    subprocess.run(['ffmpeg','-i',str(silent),'-i',str(source),'-map','0:v:0','-map','1:a:0',
    '-c:v','copy','-c:a','aac','-b:a','192k','-ar','44100',
    '-af','volume=-6dB,afade=t=in:st=0:d=0.15,afade=t=out:st=47.5:d=1.5',
    '-t','49','-movflags','+faststart',
    '-metadata','title=mdEditor Demo — official 5/4 theme BGM',
    '-metadata','artist=5garashi.com Design Office',
    '-metadata','copyright=Music: 5garashi.com Design Office, Identity v4.12.0, CC BY-ND 4.0; captions MIT; official logo CC BY-ND 4.0',
    '-metadata','comment=Official theme, 5/4, 96 BPM; first 49 seconds, -6 dB, endpoint fades; actual Windows source build 1.0.5; video stream copied',
    str(output)],check=True,stdout=subprocess.DEVNULL,stderr=(WORK/'mux.log').open('w'))
def video_hash(path):
    data=subprocess.check_output(['ffmpeg','-v','error','-i',str(path),'-map','0:v:0','-f','framemd5','-'])
    return hashlib.sha256(data).hexdigest()
original_hash=video_hash(silent)
assert video_hash(output)==original_hash,'Decoded video frames changed'
probe=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration:stream=codec_type,codec_name,width,height,sample_rate,channels','-of','json',str(output)]))
assert float(probe['format']['duration'])==49
assert any(s['codec_type']=='audio' and s['channels']==2 for s in probe['streams'])
levels=subprocess.run(['ffmpeg','-hide_banner','-i',str(output),'-map','0:a:0','-af','loudnorm=I=-20:TP=-2:LRA=7:print_format=json','-f','null','NUL'],capture_output=True,text=True,encoding='utf-8',check=True).stderr
measure=json.loads(levels[levels.rfind('{'):])
assert float(measure['input_tp'])<0,'Audio exceeds full scale'
report={'date':'2026-10-14 JST','source_url':URL,'source_sha256':SHA256,'identity_version':'4.12.0','meter':'5/4','tempo_bpm':96,'processing':'49-second excerpt; -6 dB; 0.15-second fade-in; 1.5-second fade-out','video_decoded_frames_unchanged':True,'video_framemd5_sha256':original_hash,'encoding':probe,'audio_integrated_lufs':float(measure['input_i']),'audio_true_peak_dbtp':float(measure['input_tp']),'published':False}
(HERE/'video/audio-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('Created BGM video; original video frames unchanged; audio',report['audio_integrated_lufs'],'LUFS, peak',report['audio_true_peak_dbtp'],'dBTP')
