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
 intro=w/('Prototype review.md' if en else '試作品の確認.md');intro.write_text('# '+('Prototype review' if en else '試作品の確認')+'\n\n```stl\n'+stl+'\n```\n\n'+('Drag to turn the model and check each face.' if en else 'ドラッグで回して、各面を確かめます。')+'\n',encoding='utf-8')
 title='Project notes' if en else '引き継ぎメモ'
 note=w/'main-features.md';note.write_text('# '+title+'\n\n'+('Edit the source and see the result beside it.' if en else '書きながら、すぐ横で仕上がりを確認します。')+'\n\n## '+('Review steps' if en else '確認手順')+'\n\n```mermaid\nflowchart LR\n A['+('Write' if en else '書く')+'] --> B['+('Review' if en else '確認')+'] --> C['+('Share' if en else '共有')+']\n```\n\n$$P=\\frac{V^2}{R}=1\\,\\mathrm{W}$$\n\n## '+('Components' if en else '構成')+'\n\n```dot\ndigraph G { rankdir=LR; node [style=filled,fillcolor="#FFF3D6",fontname="Meiryo"]; Document -> Preview -> PDF; }\n```\n\n## '+('Code' if en else 'コード')+'\n\n```python\ndef total(items):\n    return sum(item["cost"] for item in items)\n\nprint(total([{"cost": 120}, {"cost": 45}]))\n```\n\n## '+('Notes' if en else 'お知らせ')+'\n\n> [!NOTE]\n> '+('Keep your editable source.' if en else '編集できる原本も残してください。')+'\n\n<details>\n<summary>'+('More information' if en else '詳しい情報')+'</summary>\n\n'+('Fold and unfold this part of the document.' if en else '文書の一部を折りたたんで表示できます。')+'\n\n</details>\n\n## '+('Next steps' if en else '次の作業')+'\n\n- [ ] Review\n- [ ] Share\n',encoding='utf-8')
 legacy='# '+('Legacy text' if en else '文字コードの確認')+'\n\n日本語の文章を正しく表示します。\n'
 for codec in ['cp932','euc_jp','utf-8']:(w/f'encoding-{codec}.md').write_bytes(legacy.encode(codec))
 return sample,intro,note
def record(lang,take):
 en=lang=='en';w=B/f'{take}-{lang}';w.mkdir(exist_ok=False);sample,intro,note=prepare(lang,w);before={};events=[];cuts=[];checks={};start=0;rec=None
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
  a.click('#about-button');assert a.text('#app-version')=='1.0.12';a.click('#about-close');a.wait_preview("!!p.querySelector('.stl canvas')",timeout=40)
  win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW)
  import win32api
  win32api.keybd_event(18,0,0,0);win32gui.SetForegroundWindow(hwnd);win32api.keybd_event(18,0,2,0);time.sleep(1)
  rec=subprocess.Popen(['ffmpeg','-y','-nostats','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(w/'raw.mp4')],stdin=subprocess.PIPE,stdout=(w/'capture.log').open('w'),stderr=subprocess.STDOUT)
  start=time.monotonic();mark('intro','mdEditor：書く・読む・図にする、Windowsアプリ','mdEditor: write, read and visualize on Windows');shot('opening');rotate(5)
  mark('intro-more','仕上がりを見ながら編集。3Dも、文書の中で。','Edit beside a live preview. Explore 3D inside your document.');rotate(4,True);checks['intro_rotated']=True
  openfile(sample);a.wait_preview("!!p.querySelector('.stl canvas')",timeout=40)
  mark('consent','この収録では、画像・動画・詳細地図の読み込みを許可','Allow internet media and detailed maps for this recording')
  if a.is_visible('#notice-action'):a.click('#notice-action');time.sleep(2)
  if a.is_visible('#notice-maps'):a.click('#notice-maps');time.sleep(3)
  a.click('#settings-button');setting('#setting-video-autoplay','always');a.click('#settings-close');a.click('[data-fullscreen-mode="preview"]');scroll(0,.5)
  mark('tour','diagram-test.mdを上から下へ。多彩な表現を一望','A top-to-bottom tour of diagram-test.en.md')
  n=a.js("(()=>{let p=document.querySelector('#preview'),e=p.querySelector('.stl');return e.getBoundingClientRect().top-p.getBoundingClientRect().top+p.scrollTop-10;})()");scroll(n,62)
  mark('3d','STLを拡大・回転して、立体を確かめる','Zoom and rotate an STL model to inspect it');rotate(3,True);shot('model')
  mark('maps','GeoJSONの地点・経路を、許可した詳細地図に表示','Show GeoJSON locations and routes on a permitted detailed map');focus('#preview .geo-map');time.sleep(3);shot('map')
  mark('video','ローカルとネット上の動画を、文書内で再生','Play local and permitted internet videos inside the document');focus('#preview video');a.js("document.querySelectorAll('#preview video').forEach(v=>{v.muted=true;v.play().catch(()=>{});})");time.sleep(4);checks['videos']=a.js("[...document.querySelectorAll('#preview video')].map(v=>({ready:v.readyState,playing:!v.paused,muted:v.muted}))");shot('video');scroll(a.js("document.querySelector('#preview').scrollHeight"),3);time.sleep(1)
  a.click('[data-fullscreen-mode="preview"]');openfile(note)
  mark('edit','書きながら横で仕上がりを確認。スクロールも連動','Write beside the live preview, with synchronized scrolling');a.append('\n\n'+('Reviewed and ready to share.' if en else '確認して、共有の準備を整えます。'));time.sleep(2);a.js(f'(()=>{{let v={VIEW};v.dispatch({{selection:{{anchor:0}},effects:window.CodeMirror.EditorView.scrollIntoView(0)}});v.focus();}})()');time.sleep(2);shot('editing')
  mark('detach','プレビューを別ウィンドウに。2台目の画面へ移動できる','Detach the preview and move it to a second display')
  a.click('[data-detach="preview"]');time.sleep(2);wins=[]
  def enum(h,_):
   if win32process.GetWindowThreadProcessId(h)[1]==a.pid and h!=hwnd and win32gui.IsWindowVisible(h) and win32gui.GetClassName(h)!='#32770':wins.append(h)
  win32gui.EnumWindows(enum,None)
  if wins:win32gui.SetWindowPos(hwnd,-1,0,0,735,650,win32con.SWP_SHOWWINDOW);win32gui.SetWindowPos(wins[0],-1,740,0,540,650,win32con.SWP_SHOWWINDOW)
  a.move_cursor_to_end();time.sleep(3);shot('detached');checks['detached_window']=bool(wins);a.press('Ctrl+Shift+3');time.sleep(1);win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW)
  mark('viewer','ビューアーとして、プレビューをウィンドウいっぱいに','Use the full-window preview as a document viewer');a.click('[data-fullscreen-mode="preview"]');scroll(0);time.sleep(3);a.click('[data-fullscreen-mode="preview"]')
  mark('offline','Mermaid・Graphvizの図と数式を、オフラインで表示','Bundled Mermaid, Graphviz and math render offline');scroll(0);time.sleep(2);focus('#preview .graphviz');time.sleep(3);checks['offline_figures']=a.js("({mermaid:!!document.querySelector('#preview .mermaid svg'),graphviz:!!document.querySelector('#preview .graphviz svg'),math:!!document.querySelector('#preview .katex')})")
  mark('code','約40言語を色分け。PDF・Wordにも色を残す','Highlight about 40 code languages; keep colors in PDF and Word');focus('#preview pre');time.sleep(2);a.js("document.querySelector('#preview code.hljs')?.scrollIntoView({block:'center'})");time.sleep(3);shot('code')
  mark('toc','見出しから目次を作成。もう一度押すと更新','Build a contents list from headings; press again to update');a.js(f'(()=>{{let v={VIEW};v.dispatch({{selection:{{anchor:0}}}});v.focus();}})()');a.click('#toc-button');time.sleep(2);a.click('#toc-button');time.sleep(2);checks['toc']= '<!-- toc -->' in a.editor_text();shot('toc')
  mark('github','GitHub形式の注意書き・フロントマター・折りたたみ','GitHub-style alerts, front matter and collapsible sections');focus('#preview details');a.click('#preview summary');time.sleep(2);shot('details')
  mark('pdf','プレビューの見た目を、PDFへすぐ書き出し','Export the preview directly to PDF')
  t=time.monotonic()-start;a.press('Ctrl+Shift+P');d=a.wait_dialog();position(d);d.accept_file(str(w/'features.pdf'));d=a.wait_dialog(timeout=35)
  ui=__import__('pywinauto').Desktop(backend='uia').window(handle=d.hwnd);ui.descendants(control_type='Button')[0].invoke();d.wait_closed()
  finishcut(t);time.sleep(2);checks['pdf_export']=(w/'features.pdf').exists()
  mark('import','Word・HTML・メール・CSV・JSON・Notebook・画像を開ける','Open Word, HTML, email, CSV, JSON, notebooks and images')
  suffix='.en' if en else '';imports=['word-test'+suffix+'.docx','html-test'+suffix+'.html','mail-test'+suffix+'.eml','table-test'+suffix+'.csv','json-list-test'+suffix+'.json','notebook-test'+suffix+'.ipynb','import-test-images/sample.png'];checks['imports']=[]
  for name in imports:openfile(B/'source/samples'/name);time.sleep(1.7);checks['imports'].append({'file':name,'name':a.document_name(),'textLength':len(a.editor_text())})
  mark('export','HTML・PNG・Word・CSV・JSON・Notebook・テキストに保存','Save HTML, PNG, Word, CSV, JSON, notebooks and text');time.sleep(4)
  openfile(note);mark('images','画像はドロップや貼り付けで、そのまま文書へ','Drop or paste images directly into the document')
  a.move_cursor_to_end();data=base64.b64encode((B/'source/samples/import-test-images/sample.png').read_bytes()).decode()
  a.js(f'''(()=>{{let bytes=Uint8Array.from(atob({json.dumps(data)}),c=>c.charCodeAt(0)),dt=new DataTransfer();dt.items.add(new File([bytes],'sample.png',{{type:'image/png'}}));document.querySelector('.cm-content').dispatchEvent(new ClipboardEvent('paste',{{clipboardData:dt,bubbles:true,cancelable:true}}));}})()''')
  a.wait_preview("!!p.querySelector('img')");a.js("document.querySelector('#preview img').scrollIntoView({block:'center'})");checks['image_paste']='![' in a.editor_text();time.sleep(3);shot('image-paste')
  mark('encoding','Shift_JIS・EUC-JP・UTF-8の日本語文書に対応','Open Japanese text in Shift_JIS, EUC-JP and UTF-8');openfile(w/'encoding-cp932.md');time.sleep(2);a.click('#encoding-button');time.sleep(3);shot('encoding');a.click('#encoding-close')
  mark('recovery','安全な保存と、異常終了からの自動復元','Safe saving and automatic recovery after an unexpected exit');a.click('#settings-button');a.js("document.querySelector('#setting-backup').scrollIntoView({block:'center'})");time.sleep(3);shot('backup');a.click('#settings-close')
  mark('internet','ネット上の画像・動画・地図は、許可して読み込む','Internet images, video and maps load only with permission');a.click('#settings-button');a.js("document.querySelector('#setting-external').scrollIntoView({block:'center'})");time.sleep(3);shot('internet');a.click('#settings-close')
  mark('privacy','広告なし、計測なし、アカウント不要','No ads, no tracking, no account required');time.sleep(4)
  mark('keyboard','キーボードで全操作。ショートカットも変更できる','Use the keyboard and customize shortcuts');a.click('#shortcuts-button');time.sleep(4);shot('shortcuts');a.click('#shortcut-close')
  mark('accessibility','画面読み上げ・文書の読み上げ・ダークモードに対応','Screen-reader support, document reading and dark mode');a.click('#settings-button');a.js("document.querySelector('#setting-reader').scrollIntoView({block:'center'})");setting('#setting-reader',True,'checked');time.sleep(3);shot('reader');a.click('#settings-close');a.click('#theme-button');time.sleep(2);shot('dark')
  mark('outro','mdEditor：書く。見る。整えて、渡す。','mdEditor: write, explore, refine and share.');time.sleep(4)
  rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30);rec=None
  (w/'recording.json').write_text(json.dumps({'version':'1.0.12','source_commit':'8ab7cef','language':lang,'events':events,'remove_intervals':cuts,'checks':checks,'opening_reference':'docs/images/7-3d-model.ja.png','duration':round(time.monotonic()-start,3),'no_colored_masks':True},ensure_ascii=False,indent=2),encoding='utf-8');print('COMPLETE',w,flush=True)
 finally:
  if rec and rec.poll() is None:rec.stdin.write(b'q\n');rec.stdin.flush();rec.wait(timeout=30)
  a.stop()
  with winreg.CreateKey(winreg.HKEY_CURRENT_USER,r'Software\mdEditor') as k:
   present=[winreg.EnumValue(k,i)[0] for i in range(winreg.QueryInfoKey(k)[1])]
   for name in present:
    if name not in before:winreg.DeleteValue(k,name)
   for name,(value,kind) in before.items():winreg.SetValueEx(k,name,0,kind,value)
if __name__=='__main__':record(sys.argv[1],sys.argv[2])
