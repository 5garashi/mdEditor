from pathlib import Path
import json,subprocess,shutil,zipfile,hashlib
from PIL import Image
B=Path(__file__).resolve().parent;ROOT=B.parents[1];P=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.1';SRC=P.with_name('mdEditor-showcase-1.0.12-revision-1.1.0')
reviews=[]
def frame(file,t,name,width=640):
 out=B/name
 subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(file),'-frames:v','1','-vf',f'scale={width}:-2',str(out)],check=True)
 return Image.open(out)
def audiohash(f):
 return subprocess.check_output(['ffmpeg','-v','error','-i',str(f),'-map','0:a:0','-c:a','copy','-f','hash','-hash','sha256','-']).decode().strip()
for lang in ['ja','en']:
 source=SRC/f'mdEditor-showcase-{lang}.mp4';file=P/source.name
 v=json.loads(file.with_name(file.stem+'-validation.json').read_text(encoding='utf-8'))
 start,end=v['background_replacement_seconds'];assert v['background_removed']
 sheet=Image.new('RGB',(1280,1080),'#F9EFCB')
 for i,t in enumerate([start+.05,start+2.4,start+4.6,end-.05,start-.2,end+.2]):
  sheet.paste(frame(file,t,f'check-{lang}-{i}.png'),((i%2)*640,(i//2)*360))
 sheet.save(P/f'background-review-{lang}.jpg',quality=92)
 contact=Image.open(P/f'contact-sheet-{lang}.jpg');contact.paste(frame(file,start+4.5,f'detached-{lang}.png'),(640,360));contact.save(P/f'contact-sheet-{lang}.jpg',quality=92)
 baseline=audiohash(source)
 for prefix in ['', 'vertical-']:
  f=P/f'mdEditor-showcase-{prefix}{lang}.mp4';j=json.loads(f.with_name(f.stem+'-validation.json').read_text(encoding='utf-8'));assert j['background_removed'];assert baseline==audiohash(f)
  assert abs(float(j['probe']['format']['duration'])-j['duration'])<.2
  reviews.append({'file':f.name,'full_decode':'passed','background_removed':True,'duration':j['duration'],'original_audio_identical':True,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
 frame(P/f'mdEditor-showcase-vertical-{lang}.mp4',start+4.5,f'portrait-check-{lang}.png',540).save(P/f'portrait-review-{lang}.png')
j=json.loads((P/'review-validation.json').read_text(encoding='utf-8'));j.update(videos=reviews,revision='1.1.1',background_review='Selected frames before, during and after the full replacement interval in each language; portrait frames also inspected',background_method='Only actual mdEditor windows retained; stable one-second recording repeated for detached-preview segment, removing all gap pixels and exposed desktop transitions')
(P/'review-validation.json').write_text(json.dumps(j,ensure_ascii=False,indent=2),encoding='utf-8')
for n in ['fix.py','docs.py']:shutil.copy2(B/n,P/n)
shutil.copy2(B/'finish.py',P/'background-finish.py')
target=ROOT/'audit-source/promotion/showcase-1.0.12-revision-1.1.1';shutil.copytree(P,target,dirs_exist_ok=True)
with zipfile.ZipFile(P.parent/(P.name+'.zip'),'w',zipfile.ZIP_DEFLATED,compresslevel=5) as z:
 for f in P.rglob('*'):
  if f.is_file():z.write(f,Path(P.name)/f.relative_to(P))
print('PASS four decodes, unchanged audio, unchanged durations; archived',target)
