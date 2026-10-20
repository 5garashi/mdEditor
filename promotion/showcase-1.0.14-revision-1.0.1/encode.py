"""File: encode.py
Description: Cut native dialogs from real footage without colored masking rectangles
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,json,subprocess,importlib.util,hashlib,os
from PIL import Image,ImageDraw,ImageFont
import fitz
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.14-revision-1.0.0';OUT.mkdir(exist_ok=True)
revision=os.environ.get('PROMO_VIDEO_REVISION','1.0.0')
if revision!='1.0.0':OUT=ROOT/f'deliverables/mdEditor-showcase-1.0.14-revision-{revision}';OUT.mkdir(exist_ok=True)
spec=importlib.util.spec_from_file_location('original_encode',B.parent/'video-update-2026-10-16/encode.py');old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)
def encode(lang,take):
 w=B/f'{take}-{lang}';r=json.loads((w/'recording.json').read_text(encoding='utf-8'));raw=w/'raw.mp4';rawduration=float(old.probe(raw)['format']['duration'])
 cuts=[]
 for a,b in sorted(r['remove_intervals']):
  if cuts and a<=cuts[-1][1]:cuts[-1][1]=max(cuts[-1][1],b)
  else:cuts.append([a,b])
 def mapped(t):return max(0,t-sum(max(0,min(t,b)-a) for a,b in cuts if t>a))
 events=[dict(e,seconds=round(mapped(e['seconds']),3)) for e in r['events']]
 tour=next(e['seconds'] for e in events if e['key']=='tour')
 for off,key,ja,en in [(8,'structures','構造や状態、やり取りを図に','Visualize structure, states and interactions'),(16,'charts','予定と数値を、見える形に','Make schedules and numbers visible'),(24,'ideas','つながりとアイデアを、ひと目で','Explore relationships and ideas at a glance'),(32,'intentional-error','テスト用の意図的な構文エラー例も含みます','The test sample includes an intentional syntax-error example'),(39,'math-tour','数式・表と図を、ひとつの文書に','Keep equations, tables and diagrams together'),(47,'code-tour','コードも色分けして読みやすく','Read code with syntax highlighting'),(55,'3d-tour','GitHub形式の表現から、3Dへ','From GitHub-style content to interactive 3D')]:events.append({'seconds':tour+off,'key':key,'ja':ja,'en':en})
 events.sort(key=lambda e:e['seconds'])
 # Remove complete native-dialog intervals, not selected screen regions.
 clean=w/'clean.mp4';expr='+'.join(f'between(t,{a:.3f},{b:.3f})' for a,b in cuts)
 if revision!='1.0.0':clean=w/f'clean-revision-{revision}.mp4';assert clean.exists()
 if not clean.exists() or clean.stat().st_mtime<raw.stat().st_mtime:subprocess.run(['ffmpeg','-y','-i',str(raw),'-vf',f"select='not({expr})',setpts=N/20/TB",'-an','-r','20','-c:v','libx264','-preset','fast','-crf','16','-threads','3',str(clean)],check=True,stdout=subprocess.DEVNULL,stderr=(w/'cut.log').open('w'))
 duration=float(old.probe(clean)['format']['duration']);ass=old.ass_header(1280,720,24,34);vtt='WEBVTT\n\n'
 for i,e in enumerate(events):
  end=events[i+1]['seconds'] if i+1<len(events) else duration
  if end<=e['seconds']:continue
  ass+=f'Dialogue: 0,{old.stamp(e["seconds"],True)},{old.stamp(end,True)},Default,,0,0,0,,{e[lang]}\n';vtt+=f'{old.stamp(e["seconds"])} --> {old.stamp(end)}\n{e[lang]}\n\n'
 ass+=f'Dialogue: 1,0:00:00.00,{old.stamp(duration,True)},Credit,,0,0,0,,mdEditor source 1.0.14 | Light Steps: CC BY 4.0 | 5garashi.com\n'
 va=next(e['seconds'] for e in events if e['key']=='video');vb=next(e['seconds'] for e in events if e['key']=='edit');ass+=f'Dialogue: 1,{old.stamp(va,True)},{old.stamp(vb,True)},Credit,,0,0,18,,Big Buck Bunny (c) 2008 Blender Foundation | CC BY 3.0 | bigbuckbunny.org\n'
 (w/'captions.ass').write_text(ass,encoding='utf-8-sig');(OUT/f'mdEditor-showcase-{lang}.vtt').write_text(vtt.rstrip()+'\n',encoding='utf-8');(OUT/f'chapters-{lang}.txt').write_text('\n'.join(f'{int(e["seconds"])//60:02}:{int(e["seconds"])%60:02} {e[lang]}' for e in events)+'\n',encoding='utf-8')
 music=B.parent/'video-update-2026-10-16/light-steps.mp3';audio=f'[1:a]atrim=0:48,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=in:d=0.2,afade=t=out:st={duration-2}:d=2[a]'
 if revision!='1.0.0':audio=f'[1:a]atrim=0.1:48.1,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=out:st={duration-2}:d=2[a]'
 def run(prefix,filters,extra=[]):
  f=OUT/f'mdEditor-showcase-{prefix}{lang}.mp4';cmd=['ffmpeg','-y','-filter_complex_threads','2','-i',str(clean),'-i',str(music),*extra,'-filter_complex',filters+';'+audio,'-map','[v]','-map','[a]','-t',str(duration),'-r','20','-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-threads','3','-movflags','+faststart','-metadata','title=mdEditor — 3D opening and main features','-metadata','artist=5garashi.com Design Office','-metadata','comment=Actual source 1.0.14; complete file-dialog scenes cut with no colored masking rectangles; Light Steps CC BY 4.0 excerpt repeated and faded; sample film Big Buck Bunny (c) 2008 Blender Foundation CC BY 3.0',str(f)]
  if revision!='1.0.0':cmd[cmd.index('-preset')+1]='veryfast'
  subprocess.run(cmd,cwd=w,check=True,stdout=subprocess.DEVNULL,stderr=(w/f'encode-{prefix or "landscape"}.log').open('w'));subprocess.run(['ffmpeg','-v','error','-i',str(f),'-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=(w/f'decode-{prefix or "landscape"}.log').open('w'))
  info=old.probe(f);assert any(x['codec_type']=='audio' for x in info['streams']);(OUT/(f.stem+'-validation.json')).write_text(json.dumps({'source':r,'duration':duration,'cut_intervals_raw_seconds':cuts,'colored_masks':False,'events':events,'probe':info,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()},ensure_ascii=False,indent=2),encoding='utf-8');print('ENCODED',f,flush=True);return f
 landscape=run('', '[0:v]pad=1280:720:8:0:color=0xF9EFCB,ass=captions.ass[v]')
 im=Image.new('RGB',(1080,1920),'#F9EFCB');d=ImageDraw.Draw(im);font='C:/Windows/Fonts/meiryob.ttc'
 def text(x,y,t,size=36):d.text((x,y),t,font=ImageFont.truetype(font,size),fill='#385723')
 d.rectangle((0,0,1080,165),fill='#FFE699');d.line((0,165,1080,165),fill='#385723',width=4)
 doc=fitz.open(ROOT/f'official-assets/5garashi_logotype{"_en" if lang=="en" else ""}_outlined.svg');pix=doc[0].get_pixmap(matrix=fitz.Matrix(3,3),alpha=True);logo=Image.frombytes('RGBA',(pix.width,pix.height),pix.samples);logo.thumbnail((310,110));im.paste(logo,(50,25),logo)
 text(735,50,'mdEditor',42);text(50,215,'Write. Explore. Share.' if lang=='en' else '書く。見る。整えて渡す。',50);text(50,300,'See the main features.' if lang=='en' else '主な機能を、実画面で。',42);text(42,425,'Actual app screen' if lang=='en' else '実際の操作画面',25);text(42,1035,'Detail view' if lang=='en' else '同じ画面の中央を拡大',25);text(50,1750,'Windows source build 1.0.14',24);text(50,1840,'Light Steps — CC BY 4.0',24);text(50,1880,'5garashi.com Design Office',24);im.save(w/'portrait.png')
 pa=old.ass_header(1080,1920,34,190)
 for i,e in enumerate(events):
  end=events[i+1]['seconds'] if i+1<len(events) else duration
  if end>e['seconds']:pa+=f'Dialogue: 0,{old.stamp(e["seconds"],True)},{old.stamp(end,True)},Default,,0,0,0,,{e[lang]}\n'
 (w/'portrait.ass').write_text(pa,encoding='utf-8-sig');run('vertical-','[0:v]split=2[full][detail];[full]scale=1000:-2[screen];[detail]crop=720:440:450:120,scale=1000:-2[zoom];[2:v][screen]overlay=40:470[tmp];[tmp][zoom]overlay=40:1080,ass=portrait.ass[v]',['-loop','1','-framerate','20','-i',str(w/'portrait.png')])
 sheet=Image.new('RGB',(1280,1080),'#F9EFCB')
 for i,key in enumerate(['intro','3d','detach','images','image-adjust','accessibility']):
  t=next(e['seconds'] for e in events if e['key']==key)+2;f=w/f'review-{i}.png';subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(landscape),'-frames:v','1','-vf','scale=640:360',str(f)],check=True);sheet.paste(Image.open(f),((i%2)*640,(i//2)*360))
 sheet.save(OUT/f'contact-sheet-{lang}.jpg',quality=92)
if __name__=='__main__':encode(sys.argv[1],sys.argv[2])
