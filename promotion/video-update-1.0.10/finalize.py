"""
ファイル名 / File: finalize.py
概要 / Description: 音量・字幕・配布物の検証 / Verify audio, captions and deliverables
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import subprocess,json,re,hashlib,zipfile
BASE=Path(__file__).resolve().parent
OUT=BASE.parents[1]/'deliverables/mdEditor-video-1.0.10'
results=[]
for video in sorted(OUT.glob('*.mp4')):
 p=subprocess.run(['ffmpeg','-hide_banner','-nostats','-i',str(video),'-vn','-af','loudnorm=I=-15:TP=-1:LRA=11:print_format=json','-f','null','-'],capture_output=True,check=True)
 log=p.stderr.decode('utf-8',errors='replace'); match=re.search(r'\{\s*"input_i".*?\}',log,re.S); assert match,video
 metrics=json.loads(match.group()); assert float(metrics['input_tp'])<=-1,metrics
 assert -18<float(metrics['input_i'])<-12,metrics
 results.append({'file':video.name,'input_lufs':float(metrics['input_i']),'input_true_peak_dbtp':float(metrics['input_tp']),'sha256':hashlib.sha256(video.read_bytes()).hexdigest()})
 print('Audio verified',video.name,metrics['input_i'],metrics['input_tp'],flush=True)
assert len(results)==4
def luminance(color):
 values=[int(color[i:i+2],16)/255 for i in (1,3,5)]
 linear=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in values]
 return sum(a*b for a,b in zip(linear,[.2126,.7152,.0722]))
ratios={}
for color in ['#385723','#7B4A21']:
 ratio=(luminance('#F9EFCB')+.05)/(luminance(color)+.05); ratios[color+'/paper']=ratio
assert ratios['#385723/paper']>=7
(OUT/'audio-and-contrast-validation.json').write_text(json.dumps({'date':'2026-10-16 JST','audio':results,'contrast':ratios,'method':'ffmpeg loudnorm input measurements; WCAG relative luminance formula'},indent=2),encoding='utf-8')
for lang in ['ja','en']:
 caption=OUT/f'mdEditor-update-{lang}.vtt'
 text=caption.read_text(encoding='utf-8')
 if 'SPDX-License-Identifier' not in text:
  added=('版 / Version: 2.0.0\n' if lang=='ja' else 'Version: 2.0.0\n')+'SPDX-License-Identifier: MIT\n'
  text=text.replace('2026-10-16\n','2026-10-16\n'+added,1); caption.write_text(text,encoding='utf-8')
 assert text.startswith('WEBVTT') and text.count('-->')==12
archive=OUT.parent/'mdEditor-video-1.0.10-revision-2.0.0.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
 for file in OUT.rglob('*'):
  if file.is_file(): z.write(file,str(Path(OUT.name)/file.relative_to(OUT)))
assert zipfile.ZipFile(archive).testzip() is None
print('Packaged',archive,flush=True)
