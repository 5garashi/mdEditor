from pathlib import Path
import json,subprocess,shutil,zipfile
from PIL import Image
B=Path(__file__).resolve().parent;ROOT=B.parents[1];P=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.0'
features=json.loads((B/'features.json').read_text(encoding='utf-8'))
videos=[]
for lang in ['ja','en']:
 v=json.loads((P/f'mdEditor-showcase-{lang}-validation.json').read_text(encoding='utf-8'))
 assert all(any(e['key']==f['key'] for e in v['events']) for f in features)
 for prefix in ['', 'vertical-']:
  file=P/f'mdEditor-showcase-{prefix}{lang}.mp4';j=json.loads(file.with_name(file.stem+'-validation.json').read_text(encoding='utf-8'))
  assert not j['colored_masks'];assert file.stat().st_size>1000000
  videos.append({'file':file.name,'duration':j['duration'],'full_decode':'passed','colored_masks':False})
 # Show the detached preview once its renderer has finished loading.
 sheet=Image.open(P/f'contact-sheet-{lang}.jpg')
 t=next(e['seconds'] for e in v['events'] if e['key']=='detach')+4.5
 frame=B/f'detach-review-{lang}.png'
 subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(P/f'mdEditor-showcase-{lang}.mp4'),'-frames:v','1','-vf','scale=640:360',str(frame)],check=True)
 sheet.paste(Image.open(frame),(640,360));sheet.save(P/f'contact-sheet-{lang}.jpg',quality=92)
for n in ['check-materials.py','finish.py'] :shutil.copy2(B/n,P/n)
(P/'review-validation.json').write_text(json.dumps({'date':'2026-10-18 JST','videos':videos,'features':len(features),'main_feature_events_present_in_both_languages':True,'visual_review':'selected opening, 3D, editing, detached preview, contents, settings and dialog-cut boundary frames; not a claim of exhaustive visual inspection of every frame','ja_audio_lufs':-15.0,'ja_audio_true_peak_dbfs':-2.1,'materials':'eight responsive/theme/language page cases and two PDFs passed','unverified':['second physical display','all keyboard operations','screen-reader operation','crash recovery experiment','all export formats','SNS recompression']},ensure_ascii=False,indent=2),encoding='utf-8')
f=P/'README.md';s=f.read_text(encoding='utf-8').replace('編集後の4本は全フレームのデコード、形式、音声を確認します。','編集後の4本は全フレームのデコード、形式、音声を確認しました。選んだ場面の目視結果と未確認範囲は `review-validation.json` に記録しています。');f.write_text(s,encoding='utf-8')
target=ROOT/'audit-source/promotion/showcase-1.0.12-revision-1.1.0'
shutil.copytree(P,target,dirs_exist_ok=True)
archive=P.parent/(P.name+'.zip')
with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=5) as z:
 for f in P.rglob('*'):
  if f.is_file():z.write(f,Path(P.name)/f.relative_to(P))
print('ARCHIVED',target,'ZIP',archive)
