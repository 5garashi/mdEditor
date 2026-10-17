"""
ファイル名 / File: promotion/integration/tiktok/record_vertical.py
概要 / Description: 専用プロファイルで縦動画用の実操作を録画 / Capture real Windows operations
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import json, shutil, subprocess, sys, time, winreg
from pathlib import Path
import fitz, win32con, win32gui
from pywinauto import Desktop

PROMO = Path(__file__).resolve().parents[2]
WORK = PROMO.parent.parent / 'work' / 'tiktok-v3'
WORK.mkdir(parents=True, exist_ok=True)
sys.path[:0] = [r'C:\miFfplay\appAutoTester', r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
from autotester.cdp import CdpSession, wait_for_page
from autotester.win_dialogs import top_level_windows

VIEW = "window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
REG = r'Software\mdEditor'
NAMES = ['PreviewDetached', 'PreviewWindow', 'PreviewZoom']
before = {}
with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REG) as key:
    for name in NAMES:
        try: before[name] = winreg.QueryValueEx(key, name)
        except FileNotFoundError: before[name] = None

def place(hwnd):
    win32gui.ShowWindow(hwnd, win32con.SW_RESTORE)
    win32gui.SetWindowPos(hwnd, win32con.HWND_TOPMOST, 0, 0, 680, 720, win32con.SWP_SHOWWINDOW)

def select(app, old, new):
    index = app.editor_text().index(old)
    app.js(f'(() => {{const v={VIEW};v.dispatch({{selection:{{anchor:{index},head:{index + len(old)}}},scrollIntoView:true}});v.focus();}})()')
    app.type_text(new)
    app.wait_dirty()

def position_dialog(dialog):
    left,top,right,bottom=win32gui.GetWindowRect(dialog.hwnd)
    win32gui.SetWindowPos(dialog.hwnd,win32con.HWND_TOPMOST,12,100,min(right-left,650),min(bottom-top,580),win32con.SWP_SHOWWINDOW)

sample = WORK / 'diagrams-and-math.md'
pdf = WORK / 'diagrams-and-math.pdf'
raw = WORK / 'raw-en.mp4'
assert not raw.exists() and not pdf.exists(), 'Preserve existing capture; use a new work folder for another take.'
shutil.copy2(PROMO / 'samples/quickstart-engineering-note.en.md', sample)
app = MdEditor(PROMO.parent / 'bin/Release/net10.0-windows/mdEditor.exe', lang='en', user_data_dir=WORK / 'profile')
recorder = preview = None
events = []
try:
    app.start().wait_ready()
    main = app.main_window()
    place(main)
    log = (WORK / 'capture.log').open('w')
    recorder = subprocess.Popen(['ffmpeg','-y','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','664x712','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(raw)], stdin=subprocess.PIPE, stdout=log, stderr=log)
    start = time.monotonic()
    def at(seconds):
        wait = seconds - (time.monotonic() - start)
        if wait > 0: time.sleep(wait)
    def mark(action): events.append({'seconds':round(time.monotonic()-start,2),'action':action})
    at(3); app.click('#open-button'); mark('Open native file dialog')
    dialog = app.wait_dialog(); position_dialog(dialog)
    at(5); dialog.accept_file(str(sample))
    app.wait_preview("!!p.querySelector('svg') && !!p.querySelector('.katex')")
    app.click('[data-fullscreen-mode="editor"]'); mark('Expand editor using app control')
    at(9); select(app,'Diagrams and math','Diagrams and math: reviewed'); mark('Type heading edit')
    app.js(f'(() => {{const v={VIEW};const p=v.state.doc.toString().indexOf("# ");v.scrollDOM.scrollTop=v.lineBlockAt(p).top;}})()')
    at(14); app.click('[data-fullscreen-mode="editor"]')
    app.click('[data-fullscreen-mode="preview"]')
    app.js("document.getElementById('preview').scrollTop=0")
    app.wait_preview("p.querySelector('h1').textContent==='Diagrams and math: reviewed'")
    mark('Preview updated heading and equation')
    at(20); app.js("document.querySelector('#preview svg').scrollIntoView({block:'center'})"); mark('Mermaid diagram preview')
    at(25); app.click('[data-fullscreen-mode="preview"]')
    app.click('[data-detach="preview"]')
    target = wait_for_page(app.port,'https://mdeditor.local/index.html?view=preview',timeout=20)
    preview = CdpSession(target['webSocketDebuggerUrl'])
    deadline=time.monotonic()+15
    while time.monotonic()<deadline:
        if preview.evaluate("!!document.querySelector('#preview svg') && !!document.querySelector('#preview .katex')"): break
        time.sleep(.1)
    else: raise RuntimeError('Detached preview content not ready')
    windows = [h for h in top_level_windows(app.pid) if h != main and win32gui.GetClassName(h) != '#32770']
    assert len(windows) == 1, 'Expected one actual detached preview window'
    # The app deliberately keeps editor focus when detaching. Move that editor
    # outside the capture area so its topmost activation cannot cover the preview.
    win32gui.SetWindowPos(main,win32con.HWND_NOTOPMOST,700,0,680,720,win32con.SWP_SHOWWINDOW)
    place(windows[0])
    try: win32gui.SetForegroundWindow(windows[0])
    except Exception: pass
    mark('Actual separate preview window opened')
    at(31); place(main)
    app.click('[data-detach="editor"]')
    app.wait_until("!document.body.classList.contains('preview-detached')")
    preview.close(); preview = None
    app.click('#save-button'); app.wait_saved(); mark('Markdown saved')
    at(34); app.click('#pdf-button'); dialog = app.wait_dialog(); position_dialog(dialog); mark('Native PDF destination dialog')
    at(37); dialog.accept_file(str(pdf))
    deadline=time.monotonic()+30
    while not pdf.exists() and time.monotonic()<deadline: time.sleep(.1)
    assert pdf.exists()
    confirm=app.wait_dialog(); position_dialog(confirm); confirmation=confirm.text()
    assert 'saved' in confirmation.lower(), confirmation
    mark('Native PDF save confirmation')
    at(43)
    for button in Desktop(backend='uia').window(handle=confirm.hwnd).descendants(control_type='Button'):
        if button.window_text() in ('OK','OK(&O)'): button.invoke(); confirm.wait_closed(); break
    app.click('[data-fullscreen-mode="preview"]')
    app.js("document.getElementById('preview').scrollTop=0")
    at(49)
    recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=10); recorder=None
    doc=fitz.open(pdf)
    text='\n'.join(p.get_text() for p in doc)
    assert 'Diagrams and math: reviewed' in text and 'Check specifications' in text
    doc[0].get_pixmap(matrix=fitz.Matrix(1.2,1.2)).save(str(WORK / 'export-preview.png'))
    (WORK/'recording.json').write_text(json.dumps({'date':'2026-10-14 JST','app':'Unmodified Windows source build 1.0.5','events':events,'pdf_confirmation':confirmation,'pdf_pages':len(doc),'pdf_edited_heading_verified':True,'separate_preview_window_verified':True},indent=2),encoding='utf-8')
    app.close()
    print('Real Windows vertical capture and PDF verification completed',flush=True)
finally:
    if recorder and recorder.poll() is None:
        recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=10)
    if preview: preview.close()
    app.stop()
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as key:
        for name,value in before.items():
            if value is None:
                try: winreg.DeleteValue(key,name)
                except FileNotFoundError: pass
            else: winreg.SetValueEx(key,name,0,value[1],value[0])
