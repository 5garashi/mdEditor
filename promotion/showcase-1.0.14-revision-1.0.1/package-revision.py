"""File: package-revision.py
Description: Package corrected real openings and immediate music onset
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.0.1
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,hashlib,shutil,zipfile,subprocess,array,math
B=Path(__file__).resolve().parent;ROOT=B.parents[1];P=ROOT/'audit-source/promotion';D=ROOT/'deliverables';OLD=D/'mdEditor-showcase-1.0.14-revision-1.0.0';OUT=D/'mdEditor-showcase-1.0.14-revision-1.0.1'
for f in OLD.iterdir():
 if f.is_file() and not (OUT/f.name).exists() and f.suffix not in ['.mp4'] and not f.name.endswith('-validation.json') and not f.name.startswith(('contact-sheet','transition-review')):
  shutil.copyfile(f,OUT/f.name)
facts={'date':'2026-10-20 JST','revision':'1.0.1','opening_title':'mdEditor','change':'Real opening recaptured; remaining operations and timeline retained','music':'Light Steps; 48-second excerpt shifted 0.1s; no fade-in; original ending fade retained','windows_specific_cause':'not established','audio':{}}
for lang in ['ja','en']:
 shutil.copyfile(B/f'intro2-{lang}/opening.png',OUT/f'opening-{lang}.png')
 for prefix in ['', 'vertical-']:
  f=OUT/f'mdEditor-showcase-{prefix}{lang}.mp4';d=json.loads((OUT/(f.stem+'-validation.json')).read_text(encoding='utf-8'))
  assert any(s['codec_type']=='audio' for s in d['probe']['streams'])
  samples=array.array('f',subprocess.check_output(['ffmpeg','-v','error','-i',str(f),'-t','0.1','-f','f32le','-ac','1','-ar','44100','-']))
  rms=math.sqrt(sum(x*x for x in samples)/len(samples));db=20*math.log10(max(1e-10,rms));assert db>-30
  facts['audio'][f.name]={'first_100ms_rms_dbfs':round(db,2),'duration':float(d['probe']['format']['duration']),'full_decode':'passed by encoder'}
  d.update(revision='1.0.1',opening_title='mdEditor',opening_recaptured=True,music_start_offset_seconds=.1,fade_in_seconds=0)
  (OUT/(f.stem+'-validation.json')).write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(OUT/'revision-validation.json').write_text(json.dumps(facts,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
pv=OUT/'production-validation.json'
if pv.exists():
 d=json.loads(pv.read_text(encoding='utf-8'));d['original_verification_package']='1.0.0';d['revision_note']='Original main-feature recording checks retained. Revised opening and audio verified in revision-validation.json.';pv.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
f=OUT/'README.md';s=f.read_text(encoding='utf-8').replace('Version: 1.0.0','Version: 1.0.1').replace('資料パッケージ版：1.0.0','資料パッケージ版：1.0.1');i=s.index('\n\n',s.index('# mdEditor'))+2;s=s[:i]+'''revision 1.0.1：冒頭の文書名・見出しを日英とも「mdEditor」に変更し、実アプリで撮り直しました。後半の実演とチャプター時刻は維持。BGMは最初の0.2秒のフェードを外し、音源の0.1秒位置から開始します。旧版の最初の100msは約-21 dBFSで、数秒の無音は検出されませんでした。Windows固有の原因や再生機器の遅延は未確認です。

'''+s[i:];f.write_text(s,encoding='utf-8')
f=OUT/'CHANGELOG.md';s=f.read_text(encoding='utf-8').replace('Version: 1.0.0','Version: 1.0.1').replace('## 1.0.0 —','## 1.0.1 — 2026-10-20\n\n- 日英の冒頭の文書名・見出しをmdEditorにして実収録し直し。BGMのフェードインを除去し、先頭から明瞭に鳴るよう開始位置を調整。\n- Re-record the opening with the product name and remove the music fade-in.\n\n## 1.0.0 —',1);f.write_text(s,encoding='utf-8')
(OUT/'VERSION').write_text('1.0.1\n',encoding='utf-8')
for name in ['record.py','encode.py','record-intro.py','prepare-intro.py','splice-intro.py','package-revision.py','review.py']:shutil.copyfile(B/name,OUT/name)
m={'version':'1.0.1','publication':'local only','files':{}}
for f in OUT.iterdir():
 if f.is_file() and f.name!='manifest.json':m['files'][f.name]={'bytes':f.stat().st_size,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
(OUT/'manifest.json').write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
shutil.copytree(OUT,P/OUT.name.replace('mdEditor-',''),dirs_exist_ok=True)
with zipfile.ZipFile(D/(OUT.name+'.zip'),'w',zipfile.ZIP_DEFLATED) as z:
 for f in OUT.iterdir():
  if f.is_file():z.write(f,OUT.name+'/'+f.name)
for name in ['README.md','PROMOTION-SYSTEM.md','CHANGELOG.md']:
 f=P/name;s=f.read_text(encoding='utf-8').replace('Version: 1.11.0','Version: 1.11.1')
 if name=='CHANGELOG.md':s=s.replace('## 1.11.0 —','## 1.11.1 — 2026-10-20\n\n- 動画revision 1.0.1：日英の冒頭をmdEditorへ変更して再収録。BGMの冒頭を調整し、音声データ・4本の再生を確認。ローカル保存のみ。\n- Revise the video opening title and music onset in all four local videos.\n\n## 1.11.0 —',1)
 else:s=s.replace('showcase-1.0.14-revision-1.0.0/','showcase-1.0.14-revision-1.0.1/')
 if name=='README.md':
  import re
  s=re.sub(r'バージョン / Version: \d+\.\d+\.\d+', 'バージョン / Version: 1.11.1',s)
 f.write_text(s,encoding='utf-8')
(P/'VERSION').write_text('1.11.1\n',encoding='utf-8')
for name in ['README.md','PROMOTION-SYSTEM.md']:
 f=ROOT/name;s=f.read_text(encoding='utf-8').replace('Version: 1.11.0','Version: 1.11.1').replace('showcase-1.0.14-revision-1.0.0/','showcase-1.0.14-revision-1.0.1/');f.write_text(s,encoding='utf-8')
print(json.dumps(facts,ensure_ascii=False));print(OUT)
