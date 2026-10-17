"""File: record.py
Description: Record unmodified mdEditor sample tour and new-feature operations
Author: 5garashi.com Design Office
Created: 2026-10-17
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,time,json,subprocess,winreg,hashlib,importlib.util
import win32gui,win32con
from PIL import ImageGrab
from pywinauto import Desktop
B=Path(__file__).resolve().parent
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
spec=importlib.util.spec_from_file_location('old_record',B.parent/'video-update-2026-10-16/record.py');old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)
def record(lang,take):
 en=lang=='en';w=B/f'{take}-{lang}';w.mkdir(exist_ok=False)
 sample=B/'samples'/('diagram-test.en.md' if en else 'diagram-test.md');original=hashlib.sha256(sample.read_bytes()).hexdigest()
 values={}
 with winreg.CreateKey(winreg.HKEY_CURRENT_USER,r'Software\mdEditor') as k:
  for i in range(winreg.QueryInfoKey(k)[1]):
   name,value,kind=winreg.EnumValue(k,i);values[name]=(value,kind)
  for name,value in [('Theme','light'),('PreviewDetached','off'),('Zoom','1'),('AutoBackup','off'),('ExternalImages','ask')]:winreg.SetValueEx(k,name,0,winreg.REG_SZ,value)
 a=MdEditor(B/'app/mdEditor.exe',file=sample,lang=lang,user_data_dir=w/'profile');rec=None;events=[];masks=[];checks={};start=0
 def mark(key,ja,eng):
  v={'seconds':round(time.monotonic()-start,2),'key':key,'ja':ja,'en':eng};events.append(v);(w/'events.json').write_text(json.dumps(events,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(v,ensure_ascii=False),flush=True)
 def shot(name):ImageGrab.grab(bbox=(8,0,1272,642)).save(w/f'{name}.png')
 def scroll(target,seconds):
  a.js(f'''(()=>{{let p=document.querySelector('#preview'),from=p.scrollTop,to={target},start=performance.now();function step(t){{let n=Math.min((t-start)/{seconds*1000},1);p.scrollTop=from+(to-from)*n;if(n<1)requestAnimationFrame(step);}}requestAnimationFrame(step);}})()''');time.sleep(seconds+.2)
 def locate(selector):
  return a.js(f'''(()=>{{let p=document.querySelector('#preview'),e=document.querySelector({json.dumps(selector)});return e.getBoundingClientRect().top-p.getBoundingClientRect().top+p.scrollTop-10;}})()''')
 def focus(selector):scroll(locate(selector),1.5)
 def dialogfile(path):
  t=time.monotonic()-start;d=a.wait_dialog();win32gui.SetWindowPos(d.hwnd,-1,125,15,1000,620,win32con.SWP_SHOWWINDOW);time.sleep(.7);d.accept_file(str(path));time.sleep(.8);masks.append([t,time.monotonic()-start])
 try:
  a.start().wait_ready(timeout=60);win32gui.ShowWindow(a.main_window(),9);win32gui.SetWindowPos(a.main_window(),-1,0,0,1280,650,win32con.SWP_SHOWWINDOW);time.sleep(8)
  a.click('#about-button');assert a.text('#app-version')=='1.0.12';a.click('#about-close')
  raw=w/'raw.mp4';log=(w/'capture.log').open('w');rec=subprocess.Popen(['ffmpeg','-y','-nostats','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(raw)],stdin=subprocess.PIPE,stdout=log,stderr=log)
  start=time.monotonic();mark('intro','このMarkdownが、図・3D・地図・動画に。','One Markdown document. Diagrams, 3D, maps and video.');time.sleep(2)
  mark('consent','画像・動画と詳細地図を、許可して読み込む','Allow internet media and detailed maps to load')
  if a.is_visible('#notice-action'):a.click('#notice-action');time.sleep(2)
  if a.is_visible('#notice-maps'):a.click('#notice-maps');time.sleep(4)
  # Explicit user choice for this isolated recording profile; keep all video sound muted.
  a.click('#settings-button');a.js("let s=document.querySelector('#setting-video-autoplay');if(s){s.value='always';s.dispatchEvent(new Event('change',{bubbles:true}));}");a.click('#settings-close')
  a.click('[data-fullscreen-mode="preview"]');time.sleep(1);a.js("document.querySelector('#preview').scrollTop=0");shot('opening')
  height=a.js("document.querySelector('#preview').scrollHeight");checks['sample_scroll_height']=height
  mark('tour','diagram-test.mdを上から下へ。描ける表現を一望する','A top-to-bottom tour of diagram-test.en.md')
  # Continuous travel through the entire original sample; pause at the new 3D example.
  stop=locate('#preview .stl');scroll(stop,62);time.sleep(1);shot('model-before')
  mark('3d','3Dモデルを拡大し、ドラッグで回転','Zoom the 3D model, then drag to rotate it')
  canvas=a.js("(()=>{let r=document.querySelector('#preview .stl canvas').getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2};})()")
  before=a.js("document.querySelector('#preview .stl canvas').toDataURL()")
  a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseWheel','x':canvas['x'],'y':canvas['y'],'deltaX':0,'deltaY':-190});time.sleep(1.2)
  x,y=canvas['x'],canvas['y'];a.cdp.send('Input.dispatchMouseEvent',{'type':'mousePressed','x':x,'y':y,'button':'left','clickCount':1})
  for i in range(50):a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseMoved','x':x+140*(i+1)/50,'y':y+40*(i+1)/50,'button':'left','buttons':1});time.sleep(.045)
  a.cdp.send('Input.dispatchMouseEvent',{'type':'mouseReleased','x':x+140,'y':y+40,'button':'left','clickCount':1});time.sleep(1);after=a.js("document.querySelector('#preview .stl canvas').toDataURL()");assert before!=after;checks['3d_zoom_and_rotation_changed_canvas']=True;shot('model-after')
  mark('maps','詳細地図を読み込み、地点・ルート・円を表示','Detailed maps show locations, routes and an area')
  focus('#preview .geo-map');time.sleep(4);shot('map')
  mark('video','文書の中で動画を再生。音はミュート','Video plays inside the document, with sound muted')
  focus('#preview video');a.js("document.querySelectorAll('#preview video').forEach(v=>{v.muted=true;v.play().catch(()=>{});})");time.sleep(4);shot('video');checks['videos']=a.js("[...document.querySelectorAll('#preview video')].map(v=>({ready:v.readyState,time:v.currentTime,paused:v.paused,muted:v.muted}))")
  scroll(a.js("document.querySelector('#preview').scrollHeight"),3);time.sleep(1)
  mark('github','新機能：GitHub形式の注意書き・数式・HTML','New: GitHub-style alerts, math and HTML')
  a.js("document.querySelectorAll('#preview h2').forEach(e=>{if(e.textContent.startsWith('20.')){let p=document.querySelector('#preview');p.scrollTop+=e.getBoundingClientRect().top-p.getBoundingClientRect().top;}})");time.sleep(3);shot('github-alerts');scroll(a.js("document.querySelector('#preview').scrollTop+650"),4);shot('github-html')
  mark('metadata','先頭のYAMLも、読みやすいカードとして表示','YAML front matter becomes a readable information card');scroll(0,2);time.sleep(3);shot('frontmatter')
  a.click('[data-fullscreen-mode="preview"]');time.sleep(.6)
  mark('word','Word文書を取り込み、Markdownとして編集','Import a Word document and edit it as Markdown')
  word=w/'handover.docx';old.docx(word,en);a.click('#open-button');dialogfile(word);time.sleep(2)
  a.append('\n\n'+('Reviewed in mdEditor 1.0.12.' if en else 'mdEditor 1.0.12で確認しました。'));time.sleep(2);shot('word-import')
  mark('saveas','名前を付けて保存：Word・PDF・HTML・PNG','Save As: Word, PDF, HTML and PNG')
  a.click('#save-as-button');d=a.wait_dialog();t=time.monotonic()-start;win32gui.SetWindowPos(d.hwnd,-1,125,15,1000,620,win32con.SWP_SHOWWINDOW)
  ui=Desktop(backend='uia').window(handle=d.hwnd);combo=next(x for x in ui.descendants(control_type='ComboBox') if x.element_info.automation_id=='FileTypeControlHost');combo.type_keys('%{DOWN}');time.sleep(3);shot('formats');combo.type_keys('{ESC}');d.press(2);d.wait_closed();masks.append([t,time.monotonic()-start])
  mark('export','図や文章を、相手に合う形式で渡す','Share the result in the format your reader needs')
  a.press('Ctrl+Shift+P');dialogfile(w/'handover.pdf');time.sleep(2)
  for d in a.dialogs():
   try:d.press(1);d.wait_closed(timeout=2)
   except Exception:pass
  time.sleep(2);checks['pdf_export']=(w/'handover.pdf').exists()
  mark('outro','mdEditor 1.0.12 · 描く。動かす。整えて渡す。','mdEditor 1.0.12 · Visualize. Explore. Edit and share.');time.sleep(4);shot('outro')
  rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30);rec=None
  assert hashlib.sha256(sample.read_bytes()).hexdigest()==original
  checks['sample_unchanged']=True
  (w/'recording.json').write_text(json.dumps({'version':'1.0.12','language':lang,'events':events,'privacy_intervals':masks,'checks':checks,'sample':str(sample),'sample_sha256':original,'duration':round(time.monotonic()-start,2)},ensure_ascii=False,indent=2),encoding='utf-8');print('COMPLETE',w,flush=True)
 finally:
  if rec and rec.poll() is None:rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30)
  a.stop()
  with winreg.CreateKey(winreg.HKEY_CURRENT_USER,r'Software\mdEditor') as k:
   present=[winreg.EnumValue(k,i)[0] for i in range(winreg.QueryInfoKey(k)[1])]
   for name in present:
    if name not in values:winreg.DeleteValue(k,name)
   for name,(value,kind) in values.items():winreg.SetValueEx(k,name,0,kind,value)
if __name__=='__main__':record(sys.argv[1],sys.argv[2])
