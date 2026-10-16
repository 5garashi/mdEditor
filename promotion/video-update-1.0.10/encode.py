"""
ファイル名 / File: encode.py
概要 / Description: 実録画に字幕と音楽を付ける / Caption and score actual recordings
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,json,subprocess,hashlib
from PIL import Image,ImageDraw,ImageFont
import fitz
BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[1]
OUT=ROOT/'deliverables/mdEditor-video-1.0.10'
OUT.mkdir(exist_ok=True)
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
def encode(lang,take):
 work=BASE/f'{take}-{lang}'; record=json.loads((work/'recording.json').read_text(encoding='utf-8')); raw=work/'raw.mp4'; duration=float(probe(raw)['format']['duration'])
 events=record['events']; ass=ass_header(1280,720,26,32)
 # The dialog title bar is pale while the app header is yellow. Detect its
 # actual frames, then hide only the file picker's private navigation column.
 pixels=subprocess.check_output(['ffmpeg','-v','error','-i',str(raw),'-vf','crop=2:2:500:40:exact=1,format=rgb24','-an','-f','rawvideo','-'])
 flags=[(pixels[i]>190 and pixels[i+1]>210 and pixels[i+2]>210) for i in range(0,len(pixels),12)]
 intervals=[]; begin=None
 for i,flag in enumerate(flags+[False]):
  if flag and begin is None: begin=i
  if not flag and begin is not None:
   intervals.append([max(0,begin/20-.05),min(duration,i/20+.05)]); begin=None
 assert intervals
 open_intervals=[]
 for event in events:
  if event['key'] in ('word','csv','diagrams'):
   open_intervals.append(next(pair for pair in intervals if pair[0]>=event['seconds']-.1))
 (OUT/f'privacy-mask-{lang}.json').write_text(json.dumps({'region_in_raw_pixels':[132,128,150,355],'intervals_seconds':intervals,'open_dialog_lower_region':[132,483,150,65],'open_dialog_intervals':open_intervals,'reason':'Hide PC names and network shares in native file-picker navigation'},indent=2),encoding='utf-8')
 enabled='+'.join(f'between(t,{a:.2f},{b:.2f})' for a,b in intervals)
 privacy=f"drawbox=x=132:y=128:w=150:h=355:color=0xF9EFCB:t=fill:enable='{enabled}'"
 enabled_lower='+'.join(f'between(t,{a:.2f},{b:.2f})' for a,b in open_intervals)
 privacy+=f",drawbox=x=132:y=483:w=150:h=65:color=0xF9EFCB:t=fill:enable='{enabled_lower}'"
 vtt='WEBVTT\n\nNOTE\nFile: update-'+lang+'.vtt\nAuthor: 5garashi.com Design Office\nCreated: 2026-10-16\nLicense: MIT\n\n'
 if lang=='ja':
  vtt=vtt.replace('File:','ファイル名 / File:').replace('Author: 5garashi.com Design Office','作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office').replace('Created:','作成日 / Created:').replace('License: MIT','ライセンス / License: MIT')
 for i,event in enumerate(events):
  a=event['seconds']; b=events[i+1]['seconds'] if i+1<len(events) else duration
  text=event[lang]; ass+=f'Dialogue: 0,{stamp(a,True)},{stamp(b,True)},Default,,0,0,0,,{text}\n'
  vtt+=f'{stamp(a)} --> {stamp(b)}\n{text}\n\n'
 ass+=f'Dialogue: 1,0:00:00.00,{stamp(duration,True)},Credit,,0,0,0,,Windows source build 1.0.10 | Music: Light Steps — CC BY 4.0 | 5garashi.com\n'
 (work/'landscape.ass').write_text(ass,encoding='utf-8-sig'); (OUT/f'mdEditor-update-{lang}.vtt').write_text(vtt,encoding='utf-8')
 # A 48-second musical phrase is repeated; an ending fade is added. No narration.
 audio=f'[1:a]atrim=0:48,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{duration},afade=t=in:d=0.2,afade=t=out:st={duration-2}:d=2[a]'
 output=OUT/f'mdEditor-update-{lang}.mp4'
 cmd=['ffmpeg','-y','-i',str(raw),'-i',str(BASE/'light-steps.mp3'),'-filter_complex',f'[0:v]pad=1280:720:8:0:color=0xF9EFCB,ass=landscape.ass[v];'+audio,'-map','[v]','-map','[a]','-t',str(duration),'-r','20','-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-movflags','+faststart','-metadata','title=mdEditor Update — Import, Edit and Export','-metadata','artist=5garashi.com Design Office','-metadata','comment=Actual operations in unmodified source build 1.0.10; captured and edited with OpenAI Codex; music Light Steps, CC BY 4.0, excerpt repeated and faded',str(output)]
 idx=cmd.index('-filter_complex')+1
 cmd[idx]=cmd[idx].replace('[0:v]pad=','[0:v]'+privacy+',pad=')
 cmd[1:1]=['-filter_complex_threads','2']; cmd[-1:-1]=['-threads','4']
 subprocess.run(cmd,cwd=work,check=True,stdout=subprocess.DEVNULL,stderr=(work/'encode-landscape.log').open('w')); print('ENCODED',output,flush=True)
 # Portrait has the entire app plus a magnified view of its actual preview.
 canvas=Image.new('RGB',(1080,1920),'#F9EFCB'); d=ImageDraw.Draw(canvas); d.rectangle((0,0,1080,165),fill='#FFE699'); d.line((0,165,1080,165),fill='#385723',width=5)
 font='C:/Windows/Fonts/meiryob.ttc'
 def txt(x,y,t,size): d.text((x,y),t,font=ImageFont.truetype(font,size),fill='#385723')
 svg=fitz.open(ROOT/f'official-assets/5garashi_logotype{"_en" if lang=="en" else ""}_outlined.svg'); pix=svg[0].get_pixmap(matrix=fitz.Matrix(3,3),alpha=True); logo=Image.frombytes('RGBA',(pix.width,pix.height),pix.samples); logo.thumbnail((310,110)); canvas.paste(logo,(55,25),logo)
 txt(730,52,'mdEditor',42); txt(55,210,'Bring it in.' if lang=='en' else '取り込む。整える。',60); txt(55,295,'Make it clear. Share it.' if lang=='en' else '必要な形式で、渡す。',52)
 txt(42,420,'Actual Windows app / 実際の操作画面',25); txt(42,1045,'Screen detail / 画面の拡大',25)
 txt(60,1700,'Windows source build 1.0.10',25); txt(60,1830,'Music: Light Steps — CC BY 4.0',24); txt(60,1870,'5garashi.com Design Office',24)
 canvas.save(work/'portrait-base.png')
 portrait_ass=ass_header(1080,1920,38,140)
 for i,event in enumerate(events):
  a=event['seconds']; b=events[i+1]['seconds'] if i+1<len(events) else duration
  portrait_ass+=f'Dialogue: 0,{stamp(a,True)},{stamp(b,True)},Default,,0,0,0,,{event[lang]}\n'
 (work/'portrait.ass').write_text(portrait_ass,encoding='utf-8-sig')
 output2=OUT/f'mdEditor-update-vertical-{lang}.mp4'
 filters='[0:v]split=2[full][detail];[full]scale=1000:-2[screen];[detail]crop=610:345:645:260,scale=1000:-2[zoom];[2:v][screen]overlay=40:475[tmp];[tmp][zoom]overlay=40:1100,ass=portrait.ass[v];'+audio
 cmd2=['ffmpeg','-y','-i',str(raw),'-i',str(BASE/'light-steps.mp3'),'-loop','1','-i',str(work/'portrait-base.png'),'-filter_complex',filters,'-map','[v]','-map','[a]','-t',str(duration),'-r','20','-c:v','libx264','-preset','fast','-crf','21','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-movflags','+faststart','-metadata','title=mdEditor Update — portrait','-metadata','comment=Real Windows operations; lower panel magnifies the same preview region; Light Steps CC BY 4.0 excerpt repeated and faded',str(output2)]
 idx=cmd2.index('-filter_complex')+1
 cmd2[idx]=cmd2[idx].replace('[0:v]split=','[0:v]'+privacy+',split=')
 idx=cmd2.index('-loop'); cmd2[idx+2:idx+2]=['-framerate','20']
 cmd2[1:1]=['-filter_complex_threads','2']; cmd2[-1:-1]=['-threads','4']
 subprocess.run(cmd2,cwd=work,check=True,stdout=subprocess.DEVNULL,stderr=(work/'encode-portrait.log').open('w')); print('ENCODED',output2,flush=True)
 for f in [output,output2]:
  data=probe(f); assert any(s['codec_type']=='audio' and s['channels']==2 for s in data['streams']); assert abs(float(data['format']['duration'])-duration)<.2
  subprocess.run(['ffmpeg','-v','error','-i',str(f),'-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=(work/(f.stem+'-decode.log')).open('w'))
  (OUT/(f.stem+'-validation.json')).write_text(json.dumps({'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'duration':duration,'source_recording':record,'encoding':data},ensure_ascii=False,indent=2),encoding='utf-8')
 # Review frames show the imported documents and actual exports.
 frames=[]
 for i,event in enumerate(events):
  frame=work/f'frame-{i:02}.jpg'; t=min(event['seconds']+2,duration-.2); subprocess.run(['ffmpeg','-y','-v','error','-ss',str(t),'-i',str(output),'-frames:v','1',str(frame)],check=True)
  image=Image.open(frame); image.thumbnail((480,270)); tile=Image.new('RGB',(480,300),'#F9EFCB'); tile.paste(image,(0,0)); ImageDraw.Draw(tile).text((8,273),event['key'],fill='#385723'); frames.append(tile)
 sheet=Image.new('RGB',(480*3,300*((len(frames)+2)//3)),'#F9EFCB')
 for i,im in enumerate(frames): sheet.paste(im,((i%3)*480,(i//3)*300))
 sheet.save(OUT/f'contact-sheet-{lang}.jpg')
 print('VALIDATED',lang,flush=True)
if __name__=='__main__': encode(sys.argv[1],sys.argv[2])
