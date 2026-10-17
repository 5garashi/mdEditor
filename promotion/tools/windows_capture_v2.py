"""
ファイル名 / File: windows_capture_v2.py
概要 / Description: 日英の拡張実操作デモを録画 / Record extended bilingual Windows demonstrations
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import json,shutil,subprocess,sys,time,winreg
from pathlib import Path
import fitz,win32con,win32gui
from pywinauto import Desktop
from PIL import ImageGrab
ROOT=Path(__file__).resolve().parents[1]
WORK=ROOT.parent.parent/'work/demo-v2-take2'
WORK.mkdir(parents=True,exist_ok=True)
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
from autotester.cdp import CdpSession,wait_for_page
from autotester.win_dialogs import top_level_windows
VIEW="window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
REG=r'Software\mdEditor'
NAMES=['Theme','PreviewDetached','PreviewWindow','PreviewZoom']
before={}
with winreg.OpenKey(winreg.HKEY_CURRENT_USER,REG) as k:
    for name in NAMES:
        try:before[name]=winreg.QueryValueEx(k,name)
        except FileNotFoundError:before[name]=None

def place(h,x=0,width=1280):
    win32gui.ShowWindow(h,9)
    win32gui.SetWindowPos(h,-1,x,0,width,650,win32con.SWP_SHOWWINDOW)
def select(app,old,new):
    index=app.editor_text().index(old)
    app.js(f'(() => {{const v={VIEW};v.dispatch({{selection:{{anchor:{index},head:{index+len(old)}}},scrollIntoView:true}});v.focus();}})()')
    app.type_text(new);app.wait_dirty()
def screenshot(lang,stage):
    ImageGrab.grab(bbox=(8,0,1272,642)).save(WORK/f'{lang}-{stage}.png')

def record(lang):
    en=lang=='en';suffix='.en' if en else ''
    sample=WORK/f'demo-{lang}.md';shutil.copy2(ROOT/'samples'/f'quickstart-engineering-note{suffix}.md',sample)
    raw=WORK/f'raw-{lang}.mp4';pdf=WORK/f'demo-{lang}.pdf'
    assert not raw.exists() and not pdf.exists(),'Keep previous takes; use a new WORK folder'
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as k:
        winreg.SetValueEx(k,'Theme',0,winreg.REG_SZ,'light')
        winreg.SetValueEx(k,'PreviewDetached',0,winreg.REG_SZ,'off')
    app=MdEditor(ROOT.parent/'bin/Release/net10.0-windows/mdEditor.exe',lang=lang,user_data_dir=WORK/f'profile-{lang}')
    recorder=preview=None;events=[]
    try:
        app.start().wait_ready();main=app.main_window();place(main)
        recorder=subprocess.Popen(['ffmpeg','-y','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(raw)],stdin=subprocess.PIPE,stdout=(WORK/f'capture-{lang}.log').open('w'),stderr=subprocess.STDOUT)
        start=time.monotonic()
        def at(n):
            wait=n-(time.monotonic()-start)
            if wait>0:time.sleep(wait)
        def mark(action):events.append({'seconds':round(time.monotonic()-start,2),'action':action})
        at(3);app.click('#open-button');dialog=app.wait_dialog();mark('Open document dialog')
        at(5);dialog.accept_file(str(sample));app.wait_preview("!!p.querySelector('svg')&&!!p.querySelector('.katex')")
        heading='Diagrams and math' if en else '図と数式のメモ'
        updated='Diagrams and math: reviewed' if en else '図と数式の記録'
        at(9);select(app,heading,updated);mark('Heading edited')
        app.wait_preview(f'p.querySelector("h1").textContent==={json.dumps(updated)}')
        at(13);app.js("document.getElementById('preview').scrollTop=0");mark('KaTeX equation')
        at(17);select(app,'Check specifications' if en else '仕様を確認','Verify by measurement' if en else '測定で検証')
        app.wait_preview(f'p.querySelector("svg").textContent.includes({json.dumps("Verify by measurement" if en else "測定で検証")})')
        app.js("document.querySelector('#preview svg').scrollIntoView({block:'center'})");mark('Mermaid edited')
        at(23);app.click('#save-button');app.wait_saved();mark('Markdown saved')
        at(25);app.click('#help-button');app.wait_until("document.querySelector('#help-dialog').open")
        assert app.js("document.querySelectorAll('#help-rows tr').length")>5
        mark('Syntax / 書き方 reference opened');at(29);screenshot(lang,'syntax')
        at(33);app.click('#help-close');mark('Syntax closed')
        at(35);app.click('[data-detach="preview"]')
        target=wait_for_page(app.port,'https://mdeditor.local/index.html?view=preview',timeout=20)
        preview=CdpSession(target['webSocketDebuggerUrl'])
        deadline=time.monotonic()+15
        while time.monotonic()<deadline:
            if preview.evaluate("!!document.querySelector('#preview svg')&&!!document.querySelector('#preview .katex')"):break
            time.sleep(.1)
        else:raise RuntimeError('Detached content not ready')
        windows=[h for h in top_level_windows(app.pid) if h!=main and win32gui.GetClassName(h)!='#32770']
        assert len(windows)==1
        place(main,0,680);place(windows[0],664,616)
        preview.evaluate("document.getElementById('preview').scrollTop=0")
        mark('Two actual Windows windows with rendered diagram and math')
        at(42);screenshot(lang,'separate')
        at(46);app.click('#theme-button')
        app.wait_until("matchMedia('(prefers-color-scheme: dark)').matches")
        deadline=time.monotonic()+10
        while time.monotonic()<deadline:
            if preview.evaluate("matchMedia('(prefers-color-scheme: dark)').matches"):break
            time.sleep(.1)
        else:raise RuntimeError('Detached preview did not follow dark theme')
        mark('Dark mode in editor and separate preview');at(50);screenshot(lang,'dark')
        at(53);app.click('[data-detach="editor"]');app.wait_until("!document.body.classList.contains('preview-detached')")
        preview.close();preview=None;place(main);mark('Preview returned to main window')
        at(56);app.click('#pdf-button');dialog=app.wait_dialog();mark('PDF destination')
        at(59);dialog.accept_file(str(pdf))
        deadline=time.monotonic()+25
        while not pdf.exists() and time.monotonic()<deadline:time.sleep(.1)
        assert pdf.exists();confirm=app.wait_dialog();message=confirm.text()
        assert 'saved' in message.lower() or '保存しました' in message
        mark('PDF save confirmed while dark mode selected')
        at(65);screenshot(lang,'pdf-saved')
        at(68)
        for b in Desktop(backend='uia').window(handle=confirm.hwnd).descendants(control_type='Button'):
            if b.window_text() in ('OK','OK(&O)'):b.invoke();confirm.wait_closed();break
        app.js("document.getElementById('preview').scrollTop=0")
        at(70);app.click('#theme-button');app.click('#theme-button');app.wait_until("!matchMedia('(prefers-color-scheme: dark)').matches")
        at(72);screenshot(lang,'final')
        at(75);recorder.stdin.write(b'q\n');recorder.stdin.flush();recorder.wait(timeout=10);recorder=None
        doc=fitz.open(pdf);text='\n'.join(p.get_text() for p in doc)
        assert updated in text and ('Verify by measurement' if en else '測定で検証') in text
        doc[0].get_pixmap(matrix=fitz.Matrix(1.2,1.2)).save(str(WORK/f'{lang}-pdf-page1.png'))
        (WORK/f'recording-{lang}.json').write_text(json.dumps({'date':'2026-10-14 JST','source_build':'1.0.5','language':lang,'events':events,'native_pdf_confirmation':message,'pdf_pages':len(doc),'edited_heading_and_diagram_verified':True,'settings_restored_after_run':True},ensure_ascii=False,indent=2),encoding='utf-8')
        app.close();print('Recorded extended real Windows demo:',lang,flush=True)
    finally:
        if recorder and recorder.poll() is None:
            recorder.stdin.write(b'q\n');recorder.stdin.flush();recorder.wait(timeout=10)
        if preview:preview.close()
        app.stop()

try:
    for lang in ('en','ja'):record(lang)
finally:
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as k:
        for name,value in before.items():
            if value is None:
                try:winreg.DeleteValue(k,name)
                except FileNotFoundError:pass
            else:winreg.SetValueEx(k,name,0,value[1],value[0])
