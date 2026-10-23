"""File: encode.py
Description: Cut native dialogs from real footage without colored masking rectangles
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.1.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,json,subprocess,importlib.util,hashlib,os
from PIL import Image,ImageDraw,ImageFont
TOUR={'structures':'用图表呈现结构、状态和交互','charts':'将计划和数值转化为直观的图表','ideas':'一眼看清关系和想法','intentional-error':'测试文档也包含一个故意设置的语法错误示例','math-tour':'在同一份文档中查看公式、表格和图表','code-tour':'代码语法高亮，让阅读更轻松','3d-tour':'从 GitHub 风格的内容，到可交互的 3D 模型'}
import fitz
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.15-revision-1.1.0';OUT.mkdir(exist_ok=True)
revision='1.0.0'
OUT=ROOT/'deliverables/mdEditor-showcase-1.0.15-revision-1.1.0';OUT.mkdir(exist_ok=True)
def stamp(n,ass=False):
 if ass: return f'{int(n)//3600}:{int(n)//60%60:02}:{int(n)%60:02}.{int(n*100)%100:02}'
 return f'{int(n)//3600:02}:{int(n)//60%60:02}:{int(n)%60:02}.{int(n*1000)%1000:03}'
def probe(path): return json.loads(subprocess.check_output(['ffprobe','-v','error','-show_format','-show_streams','-of','json',str(path)]))
def ass_header(width,height,font,margin):
 return f'''[Script Info]
ScriptType: v4.00+
PlayResX: {width}
PlayResY: {height}
WrapStyle: 0
[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Meiryo,{font},&H00235738,&H00235738,&H00CBEFF9,&H00CBEFF9,-1,0,0,0,100,100,0,0,1,0,0,2,35,35,{margin},1
Style: Credit,Meiryo,14,&H00214A7B,&H00214A7B,&H00CBEFF9,&H00CBEFF9,0,0,0,0,100,100,0,0,1,0,0,2,20,20,3,1
[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
'''
class Helpers:
 stamp=staticmethod(stamp)
 probe=staticmethod(probe)
 ass_header=staticmethod(ass_header)
old=Helpers()

def encode(lang,take):
 w=B/f'{take}-{lang}';r=json.loads((w/'recording.json').read_text(encoding='utf-8'));raw=w/'raw.mp4';rawduration=float(old.probe(raw)['format']['duration'])
 cuts=[]
 for a,b in sorted(r['remove_intervals']):
  if cuts and a<=cuts[-1][1]:cuts[-1][1]=max(cuts[-1][1],b)
  else:cuts.append([a,b])
 def mapped(t):return max(0,t-sum(max(0,min(t,b)-a) for a,b in cuts if t>a))
 events=[dict(e,seconds=round(mapped(e['seconds']),3)) for e in r['events']]
 tour=next(e['seconds'] for e in events if e['key']=='tour')
 for off,key,ja,en in [(8,'structures','構造や状態、やり取りを図に','Visualize structure, states and interactions'),(16,'charts','予定と数値を、見える形に','Make schedules and numbers visible'),(24,'ideas','つながりとアイデアを、ひと目で','Explore relationships and ideas at a glance'),(32,'intentional-error','テスト用の意図的な構文エラー例も含みます','The test sample includes an intentional syntax-error example'),(39,'math-tour','数式・表と図を、ひとつの文書に','Keep equations, tables and diagrams together'),(47,'code-tour','コードも色分けして読みやすく','Read code with syntax highlighting'),(55,'3d-tour','GitHub形式の表現から、3Dへ','From GitHub-style content to interactive 3D')]:events.append({'seconds':tour+off,'key':key,'ja':ja,'en':en,'zh-CN':TOUR[key]})
 events.sort(key=lambda e:e['seconds'])
 # Remove complete native-dialog intervals, not selected screen regions.
 clean=w/('clean-word-fixed.mp4' if lang=='zh-CN' and (w/'clean-word-fixed.mp4').exists() else 'clean.mp4');expr='+'.join(f'between(t,{a:.3f},{b:.3f})' for a,b in cuts)
 if revision!='1.0.0':clean=w/f'clean-revision-{revision}.mp4';assert clean.exists()
 if not clean.exists() or clean.stat().st_mtime<raw.stat().st_mtime:subprocess.run(['ffmpeg','-y','-i',str(raw),'-vf',f"select='not({expr})',setpts=N/20/TB",'-an','-r','20','-c:v','libx264','-preset','veryfast','-crf','16','-threads','6',str(clean)],check=True,stdout=subprocess.DEVNULL,stderr=(w/'cut.log').open('w'))
 duration=float(old.probe(clean)['format']['duration']);ass=old.ass_header(1280,720,24,34).replace('Meiryo','Microsoft YaHei' if lang=='zh-CN' else 'Segoe UI' if lang=='en' else 'Meiryo');vtt='WEBVTT\n\n'
 for i,e in enumerate(events):
  end=events[i+1]['seconds'] if i+1<len(events) else duration
  if end<=e['seconds']:continue
  ass+=f'Dialogue: 0,{old.stamp(e["seconds"],True)},{old.stamp(end,True)},Default,,0,0,0,,{e[lang]}\n';vtt+=f'{old.stamp(e["seconds"])} --> {old.stamp(end)}\n{e[lang]}\n\n'
 ass+=f'Dialogue: 1,0:00:00.00,{old.stamp(duration,True)},Credit,,0,0,0,,mdEditor source 1.0.15 | Light Steps: CC BY 4.0 | 5garashi.com\n'
 va=next(e['seconds'] for e in events if e['key']=='video');vb=next(e['seconds'] for e in events if e['key']=='edit');ass+=f'Dialogue: 1,{old.stamp(va,True)},{old.stamp(vb,True)},Credit,,0,0,18,,Big Buck Bunny (c) 2008 Blender Foundation | CC BY 3.0 | bigbuckbunny.org\n'
 (w/'captions.ass').write_text(ass,encoding='utf-8-sig');(OUT/f'mdEditor-showcase-{lang}.vtt').write_text(vtt.rstrip()+'\n',encoding='utf-8');(OUT/f'chapters-{lang}.txt').write_text('\n'.join(f'{int(e["seconds"])//60:02}:{int(e["seconds"])%60:02} {e[lang]}' for e in events)+'\n',encoding='utf-8')
 music=B.parent/'video-update-2026-10-16/light-steps.mp3';audio=f'[1:a]atrim=0.1:48.1,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=out:st={duration-2}:d=2[a]'
 if revision!='1.0.0':audio=f'[1:a]atrim=0.1:48.1,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=out:st={duration-2}:d=2[a]'
 def run(prefix,filters,extra=[]):
  f=OUT/f'mdEditor-showcase-{prefix}{lang}.mp4';cmd=['ffmpeg','-y','-filter_complex_threads','4','-i',str(clean),'-i',str(music),*extra,'-filter_complex',filters+';'+audio,'-map','[v]','-map','[a]','-t',str(duration),'-r','20','-c:v','libx264','-preset','veryfast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-threads','6','-movflags','+faststart','-metadata','title=mdEditor — 3D opening and main features','-metadata','artist=5garashi.com Design Office','-metadata','comment=Actual source 1.0.15; complete file-dialog scenes cut with no colored masking rectangles; Light Steps CC BY 4.0 excerpt repeated and faded; sample film Big Buck Bunny (c) 2008 Blender Foundation CC BY 3.0',str(f)]
  if revision!='1.0.0':cmd[cmd.index('-preset')+1]='veryfast'
  subprocess.run(cmd,cwd=w,check=True,stdout=subprocess.DEVNULL,stderr=(w/f'encode-{prefix or "landscape"}.log').open('w'));subprocess.run(['ffmpeg','-v','error','-i',str(f),'-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=(w/f'decode-{prefix or "landscape"}.log').open('w'))
  info=old.probe(f);assert any(x['codec_type']=='audio' for x in info['streams']);(OUT/(f.stem+'-validation.json')).write_text(json.dumps({'source':r,'duration':duration,'cut_intervals_raw_seconds':cuts,'colored_masks':False,'events':events,'probe':info,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()},ensure_ascii=False,indent=2),encoding='utf-8');print('ENCODED',f,flush=True);return f
 landscape=run('', '[0:v]pad=1280:720:8:0:color=0xF9EFCB,ass=captions.ass[v]')
 im=Image.new('RGB',(1080,1920),'#F9EFCB');d=ImageDraw.Draw(im);font='C:/Windows/Fonts/msyhbd.ttc' if lang=='zh-CN' else 'C:/Windows/Fonts/segoeuib.ttf' if lang=='en' else 'C:/Windows/Fonts/meiryob.ttc'
 def text(x,y,t,size=36):d.text((x,y),t,font=ImageFont.truetype(font,size),fill='#385723')
 d.rectangle((0,0,1080,165),fill='#FFE699');d.line((0,165,1080,165),fill='#385723',width=4)
 doc=fitz.open(ROOT/f'official-assets/5garashi_logotype{"_en" if lang!="ja" else ""}_outlined.svg');pix=doc[0].get_pixmap(matrix=fitz.Matrix(3,3),alpha=True);logo=Image.frombytes('RGBA',(pix.width,pix.height),pix.samples);logo.thumbnail((310,110));im.paste(logo,(50,25),logo)
 text(735,50,'mdEditor',42);text(50,215,'编写、查看、分享。' if lang=='zh-CN' else 'Write. Explore. Share.' if lang=='en' else '書く。見る。整えて渡す。',50);text(50,300,'通过实际操作了解主要功能。' if lang=='zh-CN' else 'See the main features.' if lang=='en' else '主な機能を、実画面で。',42);text(42,425,'实际操作画面' if lang=='zh-CN' else 'Actual app screen' if lang=='en' else '実際の操作画面',25);text(42,1035,'同一画面的局部放大' if lang=='zh-CN' else 'Detail view' if lang=='en' else '同じ画面の中央を拡大',25);text(50,1815,'Windows source build 1.0.15',24);text(50,1840,'Light Steps — CC BY 4.0',24);text(50,1880,'5garashi.com Design Office',24);im.save(w/'portrait.png')
 pa=old.ass_header(1080,1920,34,120).replace('Meiryo','Microsoft YaHei' if lang=='zh-CN' else 'Segoe UI' if lang=='en' else 'Meiryo')
 for i,e in enumerate(events):
  end=events[i+1]['seconds'] if i+1<len(events) else duration
  if end>e['seconds']:pa+=f'Dialogue: 0,{old.stamp(e["seconds"],True)},{old.stamp(end,True)},Default,,0,0,0,,{e[lang]}\n'
 (w/'portrait.ass').write_text(pa,encoding='utf-8-sig');run('vertical-','[0:v]split=2[full][detail];[full]scale=1000:-2[screen];[detail]crop=720:440:450:120,scale=1000:-2[zoom];[2:v][screen]overlay=40:470[tmp];[tmp][zoom]overlay=40:1080,ass=portrait.ass[v]',['-loop','1','-framerate','20','-i',str(w/'portrait.png')])
 sheet=Image.new('RGB',(1280,1080),'#F9EFCB')
 for i,key in enumerate(['intro','3d','detach','images','image-adjust','accessibility']):
  t=next(e['seconds'] for e in events if e['key']==key)+2;f=w/f'review-{i}.png';subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(landscape),'-frames:v','1','-vf','scale=640:360',str(f)],check=True);sheet.paste(Image.open(f),((i%2)*640,(i//2)*360))
 sheet.save(OUT/f'contact-sheet-{lang}.jpg',quality=92)
if __name__=='__main__':encode(sys.argv[1],sys.argv[2])
