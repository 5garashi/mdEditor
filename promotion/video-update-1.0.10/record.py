"""
ファイル名 / File: record.py
概要 / Description: mdEditor 1.0.10の実操作収録 / Record import and export operations
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,time,json,subprocess,zipfile,winreg,hashlib
from xml.sax.saxutils import escape
import win32gui,win32con
from pywinauto import Desktop
from PIL import ImageGrab
import fitz
BASE=Path(__file__).resolve().parent
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
from autotester.win_dialogs import IDOK
VIEW="window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
REG=r'Software\mdEditor'
NAMES=['Theme','PreviewDetached','PreviewWindow','PreviewZoom','Zoom','AutoBackup','ExternalImages']

def docx(path,en):
 title='Project handover' if en else 'プロジェクト引き継ぎ'
 lines=[title,'Bring existing documents into Markdown.' if en else '既存の資料をMarkdownに取り込みます。','Review notes' if en else '確認メモ','Keep the source. Edit a copy. Share the result.' if en else '原本を残し、コピーを編集して共有します。']
 paras=''.join('<w:p>'+('<w:pPr><w:pStyle w:val="Heading1"/></w:pPr>' if i in [0,2] else '')+'<w:r><w:t>'+escape(t)+'</w:t></w:r></w:p>' for i,t in enumerate(lines))
 with zipfile.ZipFile(path,'w') as z:
  z.writestr('[Content_Types].xml','<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/><Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/></Types>')
  z.writestr('_rels/.rels','<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>')
  z.writestr('word/document.xml','<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body>'+paras+'<w:sectPr/></w:body></w:document>')
  z.writestr('word/styles.xml','<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/></w:style></w:styles>')
  z.writestr('word/_rels/document.xml.rels','<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="s" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>')

def record(lang,take):
 en=lang=='en'; work=BASE/f'{take}-{lang}'; work.mkdir(exist_ok=False)
 word=work/'handover.docx'; docx(word,en); original=hashlib.sha256(word.read_bytes()).hexdigest()
 csv=work/'budget.csv'; csv.write_text('Item,Cost,Owner\nPrototype,120,Design\nTesting,45,Quality\n' if en else '項目,費用,担当\n試作,120,設計\n試験,45,品質\n',encoding='utf-8-sig')
 sample=work/'engineering-note.md'
 title='Engineering note' if en else '技術メモ'
 sample.write_text('# '+title+'\n\n'+('Check a calculation and review the workflow.' if en else '計算条件と作業の流れを確認します。')+'\n\n$$P=\\frac{V^2}{R}=\\frac{10^2}{100}=1\\,\\mathrm{W}$$\n\n```mermaid\nflowchart LR\n A['+('Import' if en else '取り込む')+'] --> B['+('Review' if en else '整える')+'] --> C['+('Share' if en else '渡す')+']\n```\n\n| '+('Voltage | Resistance | Power' if en else '電圧 | 抵抗 | 電力')+' |\n| --- | --- | --- |\n| 10 V | 100 Ω | 1 W |\n',encoding='utf-8')
 before={}
 with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as k:
  for name in NAMES:
   try: before[name]=winreg.QueryValueEx(k,name)
   except FileNotFoundError: before[name]=None
  for name,val in [('Theme','light'),('PreviewDetached','off'),('Zoom','1'),('AutoBackup','off')]: winreg.SetValueEx(k,name,0,winreg.REG_SZ,val)
 app=MdEditor(BASE/'app/mdEditor.exe',lang=lang,user_data_dir=work/'profile')
 recorder=None; events=[]; validations={}; start=0
 def mark(key,ja,eng):
  event={'seconds':round(time.monotonic()-start,2),'key':key,'ja':ja,'en':eng}; events.append(event)
  (work/'events.json').write_text(json.dumps(events,ensure_ascii=False,indent=2),encoding='utf-8')
  print(json.dumps(event,ensure_ascii=False),flush=True)
 def hold(n=4): time.sleep(n)
 def shot(name): ImageGrab.grab(bbox=(8,0,1272,642)).save(work/f'{name}.png')
 def position(d): win32gui.SetWindowPos(d.hwnd,-1,125,15,1000,620,win32con.SWP_SHOWWINDOW)
 def openfile(path):
  app.click('#open-button'); d=app.wait_dialog(); position(d); hold(1); d.accept_file(str(path)); hold(2)
 def savemd(path):
  app.click('#save-button'); d=app.wait_dialog(); position(d); hold(1); d.accept_file(str(path)); app.wait_saved(); hold(1)
 def confirm():
  d=app.wait_dialog(timeout=30); position(d)
  ui=Desktop(backend='uia').window(handle=d.hwnd)
  text='\n'.join(x.window_text() for x in ui.descendants(control_type='Text')) or d.text()
  hold(2)
  buttons=ui.descendants(control_type='Button')
  button=next((x for x in buttons if x.window_text() in ('OK','OK(&O)','閉じる','Close')),None)
  if button is not None: button.invoke()
  try: d.wait_closed(timeout=1)
  except TimeoutError:
   ui.set_focus(); ui.type_keys('{ENTER}')
   try: d.wait_closed(timeout=2)
   except TimeoutError:
    if button is not None: button.click_input()
    d.wait_closed()
  return text
 try:
  app.start().wait_ready(); hwnd=app.main_window(); win32gui.ShowWindow(hwnd,9); win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW)
  app.click('#about-button'); app.wait_until("document.querySelector('#about-dialog').open"); version=app.text('#app-version'); assert version=='1.0.10',version; app.click('#about-close')
  raw=work/'raw.mp4'; log=(work/'capture.log').open('w')
  recorder=subprocess.Popen(['ffmpeg','-nostats','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(raw)],stdin=subprocess.PIPE,stdout=log,stderr=log)
  start=time.monotonic(); mark('intro','mdEditor：資料を取り込み、整えて渡す','mdEditor: bring it in, make it clear, share it'); hold(3)
  mark('word','Word文書を開いてMarkdownにする','Open a Word document as Markdown'); openfile(word)
  app.wait_preview("p.textContent.includes("+json.dumps('Project handover' if en else 'プロジェクト引き継ぎ')+")")
  validations['word_import']=True; shot('word-import'); hold(4)
  mark('edit-word','文章を追記して、Markdownで保存','Add a note and save the Markdown')
  app.append('\n\n'+('Reviewed and ready to share.' if en else '確認済み。共有の準備ができました。')); hold(2); savemd(work/'handover.md')
  mark('csv','CSVを開くと、編集できる表に','Open CSV as an editable Markdown table'); openfile(csv)
  app.wait_preview("p.querySelectorAll('table tbody tr').length===2"); validations['csv_import']=True; shot('csv-import'); hold(4); savemd(work/'budget.md')
  mark('url','「URLを開く」でWebページを取り込む','Open URL brings a web page into Markdown'); app.click('#url-button'); app.wait_until("document.querySelector('#url-dialog').open")
  app.click('#url-input'); app.type_text('https://5garashi.github.io/mdEditor/office-card-en.html' if en else 'https://5garashi.github.io/mdEditor/office-card-ja.html'); hold(2); app.click('#url-open')
  app.wait_preview("p.textContent.includes('mdEditor')",timeout=30); validations['url_import']=True; shot('url-import'); hold(4); savemd(work/'web-note.md')
  mark('diagrams','図と数式を見ながら、文書を編集','Edit while previewing diagrams and equations'); openfile(sample)
  app.wait_preview("!!p.querySelector('svg') && !!p.querySelector('.katex')")
  idx=app.editor_text().index(title); updated=title+(': reviewed' if en else '：確認済み')
  app.js(f'(() => {{const v={VIEW}; v.dispatch({{selection:{{anchor:{idx},head:{idx+len(title)}}}}});v.focus();}})()'); app.type_text(updated); app.wait_dirty()
  app.wait_preview('p.querySelector("h1").textContent==='+json.dumps(updated)); hold(3); app.click('#save-button'); app.wait_saved(); shot('diagrams'); validations['edited_heading']=updated
  mark('saveas','書き出す形式は「名前を付けて保存」で選ぶ','Choose the output format in Save As'); app.click('#save-as-button'); d=app.wait_dialog(); position(d)
  ui=Desktop(backend='uia').window(handle=d.hwnd); combos=ui.descendants(control_type='ComboBox'); info=[(x.window_text(),x.element_info.automation_id) for x in combos]; print('COMBOS',info,flush=True)
  combo=next((x for x in combos if x.element_info.automation_id=='FileTypeControlHost'),None)
  if combo is None: combo=next(x for x in combos if any(t in x.window_text() for t in ['Markdown','ファイルの種類','Save as type']))
  combo.set_focus(); combo.type_keys('%{DOWN}'); hold(3); shot('save-as-list'); combo.type_keys('{ESC}'); d.press(2); d.wait_closed()
  for ext,shortcut,ja,eng in [('docx','Ctrl+Shift+D','Word：図と数式も一緒に渡す','Word: share diagrams and math too'),('pdf','Ctrl+Shift+P','PDF：配布用の文書にする','PDF: prepare a document for sharing'),('html','Ctrl+Shift+M','HTML：ひとつのファイルで共有','HTML: share a single file'),('png','Ctrl+Shift+G','PNG：プレビューを画像にする','PNG: turn the preview into images')]:
   mark(ext,ja,eng); app.press(shortcut); d=app.wait_dialog(); position(d); hold(1.5); path=work/f'engineering-note.{ext}'; d.accept_file(str(path)); message=confirm(); validations[ext]={'message':message}; hold(2); shot(ext+'-done')
  mark('outro','mdEditor · Microsoft Storeで無料 / 5garashi.com','mdEditor · Free on Microsoft Store / 5garashi.com'); hold(5)
  recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=20); recorder=None
  validations['word_source_unchanged']=hashlib.sha256(word.read_bytes()).hexdigest()==original
  assert validations['word_source_unchanged']
  assert zipfile.is_zipfile(work/'engineering-note.docx')
  with zipfile.ZipFile(work/'engineering-note.docx') as z:
   xml=z.read('word/document.xml').decode(); assert 'reviewed' in xml or '確認済み' in xml; validations['docx_images']=len([n for n in z.namelist() if n.startswith(('word/media/','media/'))]); assert validations['docx_images']>=2
  pdf=fitz.open(work/'engineering-note.pdf'); text=''.join(p.get_text() for p in pdf); assert 'reviewed' in text or '確認済み' in text; validations['pdf_pages']=len(pdf); pdf[0].get_pixmap().save(work/'pdf-check.png')
  html=(work/'engineering-note.html').read_text(encoding='utf-8'); assert '<svg' in html and '<math' in html; validations['html_svg_mathml']=True
  pngs=list(work.glob('engineering-note*.png')); assert pngs; validations['png_files']=[p.name for p in pngs]
  (work/'recording.json').write_text(json.dumps({'app':'1.0.10 unmodified source build','date':'2026-10-16 JST','language':lang,'events':events,'duration':round(time.monotonic()-start,2),'validation':validations},ensure_ascii=False,indent=2),encoding='utf-8')
  print('RECORDING COMPLETE',work,flush=True)
 finally:
  if recorder and recorder.poll() is None:
   recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=20)
  app.stop()
  with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as k:
   for name,val in before.items():
    if val is None:
     try:winreg.DeleteValue(k,name)
     except FileNotFoundError:pass
    else:winreg.SetValueEx(k,name,0,val[1],val[0])

if __name__=='__main__': record(sys.argv[1],sys.argv[2])
