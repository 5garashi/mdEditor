"""File: encode.py
Description: Caption real showcase footage and add the existing Light Steps music
Author: 5garashi.com Design Office
Created: 2026-10-17
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,json,subprocess,hashlib,importlib.util
from PIL import Image,ImageDraw,ImageFont
import fitz
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.12';OUT.mkdir(exist_ok=True)
spec=importlib.util.spec_from_file_location('previous_encode',B.parent/'video-update-2026-10-16/encode.py');old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)
def encode(lang,take):
 w=B/f'{take}-{lang}';record=json.loads((w/'recording.json').read_text(encoding='utf-8'));raw=w/'raw.mp4';duration=float(old.probe(raw)['format']['duration']);events=list(record['events'])
 tour=next(e['seconds'] for e in events if e['key']=='tour')
 for offset,key,ja,en in [(8,'structures','構造と状態、やり取りを図に','Visualize structure, states and interactions'),(16,'charts','予定と数値も、見える形に','Make schedules and numbers visible'),(24,'ideas','つながりとアイデアを、ひと目で','Explore relationships and ideas at a glance'),(32,'test-error','テスト用の意図的な構文エラー例も含みます','The test sample includes an intentional syntax-error example'),(39,'math','数式・表と図を、ひとつの文書に','Keep equations, tables and diagrams together'),(47,'code','コードも色分けして読みやすく','Read code with syntax highlighting'),(55,'new-syntax','GitHub形式の表現から、3Dへ','From GitHub-style content to interactive 3D')]:
  events.append({'seconds':tour+offset,'key':key,'ja':ja,'en':en})
 events.sort(key=lambda e:e['seconds'])
 ass=old.ass_header(1280,720,25,34);vtt='WEBVTT\n\n'
 for i,e in enumerate(events):
  a=e['seconds'];b=events[i+1]['seconds'] if i+1<len(events) else duration
  ass+=f'Dialogue: 0,{old.stamp(a,True)},{old.stamp(b,True)},Default,,0,0,0,,{e[lang]}\n';vtt+=f'{old.stamp(a)} --> {old.stamp(b)}\n{e[lang]}\n\n'
 ass+=f'Dialogue: 1,0:00:00.00,{old.stamp(duration,True)},Credit,,0,0,0,,Source 1.0.12 | Light Steps: CC BY 4.0 | 5garashi.com\n'
 video_start=next(e['seconds'] for e in events if e['key']=='video');video_end=next(e['seconds'] for e in events if e['key']=='github')
 ass+=f'Dialogue: 1,{old.stamp(video_start,True)},{old.stamp(video_end,True)},Credit,,0,0,18,,Big Buck Bunny (c) 2008 Blender Foundation | CC BY 3.0 | bigbuckbunny.org\n'
 (w/'captions.ass').write_text(ass,encoding='utf-8-sig');(OUT/f'mdEditor-showcase-{lang}.vtt').write_text(vtt.rstrip()+'\n',encoding='utf-8')
 # Cover the native picker's private navigation pane only while a picker is open.
 intervals=record['privacy_intervals'];enabled='+'.join(f'between(t,{a:.2f},{b:.2f})' for a,b in intervals)
 privacy=f"drawbox=x=132:y=128:w=150:h=420:color=0xF9EFCB:t=fill:enable='{enabled}'" if intervals else 'null'
 audio=f'[1:a]atrim=0:48,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=in:d=0.2,afade=t=out:st={duration-2}:d=2[a]'
 music=B.parent/'video-update-2026-10-16/light-steps.mp3'
 def run(name,filters,extra=[]):
  dest=OUT/f'mdEditor-showcase-{name}{lang}.mp4'
  cmd=['ffmpeg','-y','-filter_complex_threads','2','-i',str(raw),'-i',str(music),*extra,'-filter_complex',filters+';'+audio,'-map','[v]','-map','[a]','-t',str(duration),'-r','20','-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-threads','4','-movflags','+faststart','-metadata','title=mdEditor 1.0.12 — Visualize. Explore. Edit and share.','-metadata','artist=5garashi.com Design Office','-metadata','comment=Actual unmodified Windows source build; sample tour, 3D zoom and rotation, maps and video; music Light Steps CC BY 4.0 excerpt repeated and faded; external sample video Big Buck Bunny (c) 2008 Blender Foundation CC BY 3.0; map data OpenStreetMap contributors',str(dest)]
  subprocess.run(cmd,cwd=w,check=True,stdout=subprocess.DEVNULL,stderr=(w/f'encode-{name or "landscape"}.log').open('w'));print('ENCODED',dest,flush=True)
  subprocess.run(['ffmpeg','-v','error','-i',str(dest),'-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=(w/f'decode-{name or "landscape"}.log').open('w'))
  info=old.probe(dest);assert any(s['codec_type']=='audio' and s['channels']==2 for s in info['streams']);assert abs(float(info['format']['duration'])-duration)<.3
  (OUT/(dest.stem+'-validation.json')).write_text(json.dumps({'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'probe':info,'recording':record},ensure_ascii=False,indent=2),encoding='utf-8')
  return dest
 landscape=run('',f'[0:v]{privacy},pad=1280:720:8:0:color=0xF9EFCB,ass=captions.ass[v]')
 canvas=Image.new('RGB',(1080,1920),'#F9EFCB');d=ImageDraw.Draw(canvas);font='C:/Windows/Fonts/meiryob.ttc'
 def text(x,y,s,size=38):d.text((x,y),s,font=ImageFont.truetype(font,size),fill='#385723')
 d.rectangle((0,0,1080,165),fill='#FFE699');d.line((0,165,1080,165),fill='#385723',width=4)
 svg=fitz.open(ROOT/f'official-assets/5garashi_logotype{"_en" if lang=="en" else ""}_outlined.svg');pix=svg[0].get_pixmap(matrix=fitz.Matrix(3,3),alpha=True);logo=Image.frombytes('RGBA',(pix.width,pix.height),pix.samples);logo.thumbnail((310,110));canvas.paste(logo,(50,25),logo)
 text(730,50,'mdEditor',42);text(50,215,'One Markdown.' if lang=='en' else 'ひとつのMarkdown。',56);text(50,300,'A visual world.' if lang=='en' else '図も、3Dも、動画も。',52)
 text(42,425,'Actual app screen' if lang=='en' else '実際の操作画面',25);text(42,1035,'Center detail' if lang=='en' else '同じ画面の中央を拡大',25)
 text(50,1750,'Windows source build 1.0.12',24);text(50,1840,'Light Steps — CC BY 4.0',24);text(50,1880,'5garashi.com Design Office',24);canvas.save(w/'portrait.png')
 pa=old.ass_header(1080,1920,35,190)
 for i,e in enumerate(events):
  end=events[i+1]['seconds'] if i+1<len(events) else duration;pa+=f'Dialogue: 0,{old.stamp(e["seconds"],True)},{old.stamp(end,True)},Default,,0,0,0,,{e[lang]}\n'
 (w/'portrait.ass').write_text(pa,encoding='utf-8-sig')
 run('vertical-',f'[0:v]{privacy},split=2[full][detail];[full]scale=1000:-2[screen];[detail]crop=720:440:256:120,scale=1000:-2[zoom];[2:v][screen]overlay=40:470[tmp];[tmp][zoom]overlay=40:1080,ass=portrait.ass[v]',['-loop','1','-framerate','20','-i',str(w/'portrait.png')])
 # Review sheet from actual encoded output.
 sheet=Image.new('RGB',(1280,720),'#F9EFCB')
 for i,t in enumerate([15,35,next(e['seconds'] for e in events if e['key']=='3d')+3,next(e['seconds'] for e in events if e['key']=='video')+2]):
  f=w/f'review-{i}.png';subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(landscape),'-frames:v','1','-vf','scale=640:360',str(f)],check=True);sheet.paste(Image.open(f),((i%2)*640,(i//2)*360))
 sheet.save(OUT/f'contact-sheet-{lang}.jpg',quality=92)
if __name__=='__main__':encode(sys.argv[1],sys.argv[2])
