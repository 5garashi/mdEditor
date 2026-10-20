"""File: record.py
Description: Record a rotating-3D opening, sample tour and main-feature walkthrough
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,time,json,re,subprocess,winreg,hashlib,base64
import win32gui,win32con,win32process
from PIL import ImageGrab
B=Path(__file__).resolve().parent
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
VIEW="window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
def prepare(lang,w):
 en=lang=='en';sample=B/'source/samples'/('diagram-test.en.md' if en else 'diagram-test.md');text=sample.read_text(encoding='utf-8');stl=re.search(r'```stl\n(.*?)\n```',text,re.S).group(1)
 intro=w/'mdEditor.md';intro.write_text('# mdEditor'+'\n\n```stl\n'+stl+'\n```\n\n'+('Drag to turn the model and check each face.' if en else 'ドラッグで回して、各面を確かめます。')+'\n',encoding='utf-8')
 title='Project notes' if en else '引き継ぎメモ'
 note=w/'main-features.md';note.write_text('# '+title+'\n\n'+('Edit the source and see the result beside it.' if en else '書きながら、すぐ横で仕上がりを確認します。')+'\n\n## '+('Review steps' if en else '確認手順')+'\n\n```mermaid\nflowchart LR\n A['+('Write' if en else '書く')+'] --> B['+('Review' if en else '確認')+'] --> C['+('Share' if en else '共有')+']\n```\n\n$$P=\\frac{V^2}{R}=1\\,\\mathrm{W}$$\n\n## '+('Components' if en else '構成')+'\n\n```dot\ndigraph G { rankdir=LR; node [style=filled,fillcolor="#FFF3D6",fontname="Meiryo"]; Document -> Preview -> PDF; }\n```\n\n## '+('Code' if en else 'コード')+'\n\n```python\ndef total(items):\n    return sum(item["cost"] for item in items)\n\nprint(total([{"cost": 120}, {"cost": 45}]))\n```\n\n## '+('Notes' if en else 'お知らせ')+'\n\n> [!NOTE]\n> '+('Keep your editable source.' if en else '編集できる原本も残してください。')+'\n\n<details>\n<summary>'+('More information' if en else '詳しい情報')+'</summary>\n\n'+('Fold and unfold this part of the document.' if en else '文書の一部を折りたたんで表示できます。')+'\n\n</details>\n\n## '+('Next steps' if en else '次の作業')+'\n\n- [ ] Review\n- [ ] Share\n',encoding='utf-8')
 legacy='# '+('Legacy text' if en else '文字コードの確認')+'\n\n日本語の文章を正しく表示します。\n'
 for codec in ['cp932','euc_jp','utf-8']:(w/f'encoding-{codec}.md').write_bytes(legacy.encode(codec))
 return sample,intro,note
def record(lang,take):
 en=lang=='en';w=B/f'{take}-{lang}';w.mkdir(exist_ok=False);sample,intro,note=prepare(lang,w);before={};events=[];cuts=[];checks={};start=0;rec=None
 shield=subprocess.Popen([sys.executable,str(B/'shield.py')]);time.sleep(1)
 with winreg.CreateKey(winreg.HKEY_CURRENT_USER,r'Software\mdEditor') as k:
  for i in range(winreg.QueryInfoKey(k)[1]):
   name,value,kind=winreg.EnumValue(k,i);before[name]=(value,kind)
  for name,value in [('Theme','light'),('PreviewDetached','off'),('Zoom','0.9'),('AutoBackup','off'),('ExternalImages','ask')]:winreg.SetValueEx(k,name,0,winreg.REG_SZ,value)
 a=MdEditor(B/'app/mdEditor.exe',file=intro,lang=lang,user_data_dir=w/'profile')
 def mark(key,ja,eng):
  e={'seconds':round(time.monotonic()-start,3),'key':key,'ja':ja,'en':eng};events.append(e);(w/'events.json').write_text(json.dumps(events,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(e,ensure_ascii=False),flush=True)
 def shot(name):ImageGrab.grab(bbox=(8,0,1272,642)).save(w/f'{name}.png')
 def position(d):win32gui.SetWindowPos(d.hwnd,-1,125,15,1000,620,win32con.SWP_SHOWWINDOW)
 def finishcut(t):
  time.sleep(.65);cuts.append([max(0,t-.25),time.monotonic()-start]);(w/'cuts.json').write_text(json.dumps(cuts),encoding='utf-8')
 def openfile(path):
  t=time.monotonic()-start
  for pending in a.dialogs():
   ui=__import__('pywinauto').Desktop(backend='uia').window(handle=pending.hwnd)
   buttons=ui.descendants(control_type='Button')
   if len(buttons)==1:buttons[0].invoke();pending.wait_closed()
  a.click('#open-button')
  for attempt in range(5):
   d=a.wait_dialog();position(d)
   try:d.file_name();break
   except LookupError:
    if d.has_button(7) and d.has_button(6):d.press(7)
    else:
     ui=__import__('pywinauto').Desktop(backend='uia').window(handle=d.hwnd);ui.descendants(control_type='Button')[0].invoke()
    d.wait_closed();time.sleep(.2)
  else:raise RuntimeError('No file picker after closing confirmation')
  d.accept_file(str(path));time.sleep(1.5);finishcut(t)
 def scroll(target,seconds=1.5):
  a.js(f'''(()=>{{let p=document.querySelector('#preview'),v=p.scrollTop,t=performance.now();function s(n){{let k=Math.min((n-t)/{seconds*1000},1);p.scrollTop=v+({target}-v)*k;if(k<1)requestAnimationFrame(s);}}requestAnimationFrame(s);}})()''');time.sleep(seconds+.2)
 def focus(selector):
  n=a.js(f'''(()=>{{let p=document.querySelector('#preview'),e=document.querySelector({json.dumps(selector)});return e.getBoundingClientRect().top-p.getBoundingClientRect().top+p.scrollTop-10;}})()''');scroll(n)
 def rotate(seconds=4,zoom=False):
  canvas=a.js("(()=>{let e=document.querySelector('#preview .stl canvas');e.scrollIntoView({block:'center'});let r=e.getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2,before:e.toDataURL()};})()")
  x,y=canvas['x'],canvas['y']
  if zoom:a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseWheel','x':x,'y':y,'deltaX':0,'deltaY':-100});time.sleep(.6)
  a.cdp.send('Input.dispatchMouseEvent',{'type':'mousePressed','x':x,'y':y,'button':'left','clickCount':1})
  for i in range(70):a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseMoved','x':x+125*(i+1)/70,'y':y+25*(i+1)/70,'button':'left','buttons':1});time.sleep(seconds/70)
  a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseReleased','x':x+125,'y':y+25,'button':'left','clickCount':1})
  assert canvas['before']!=a.js("document.querySelector('#preview .stl canvas').toDataURL()")
 def setting(selector,value,kind='value'):
  a.js(f'''(()=>{{let e=document.querySelector({json.dumps(selector)});e.{kind}={json.dumps(value)};e.dispatchEvent(new Event('change',{{bubbles:true}}));}})()''')
 try:
  a.start().wait_ready(timeout=60);hwnd=a.main_window();win32gui.ShowWindow(hwnd,9);win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW);time.sleep(5)
  a.click('#about-button');assert a.text('#app-version')=='1.0.14';a.click('#about-close');a.wait_preview("!!p.querySelector('.stl canvas')",timeout=40)
  win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW)
  import win32api
  win32api.keybd_event(18,0,0,0);win32gui.SetForegroundWindow(hwnd);win32api.keybd_event(18,0,2,0);time.sleep(1)
  rec=subprocess.Popen(['ffmpeg','-y','-nostats','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(w/'raw.mp4')],stdin=subprocess.PIPE,stdout=(w/'capture.log').open('w'),stderr=subprocess.STDOUT)
  start=time.monotonic();mark('intro','mdEditor：書く・読む・図にする、Windowsアプリ','mdEditor: write, read and visualize on Windows');shot('opening');rotate(5)
  mark('intro-more','仕上がりを見ながら編集。3Dも、文書の中で。','Edit beside a live preview. Explore 3D inside your document.');rotate(4,True);checks['intro_rotated']=True
  time.sleep(3);rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30);rec=None
  shot('opening');print('Replacement intro complete',lang,flush=True)

 finally:
  if rec and rec.poll() is None:rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30)
  a.stop()
  shield.terminate();shield.wait(timeout=10)
  with winreg.CreateKey(winreg.HKEY_CURRENT_USER,r'Software\mdEditor') as k:
   present=[winreg.EnumValue(k,i)[0] for i in range(winreg.QueryInfoKey(k)[1])]
   for name in present:
    if name not in before:winreg.DeleteValue(k,name)
   for name,(value,kind) in before.items():winreg.SetValueEx(k,name,0,kind,value)
if __name__=='__main__':record(sys.argv[1],sys.argv[2])
