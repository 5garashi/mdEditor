"""
ファイル名 / File: promotion/tools/capture_separate.py
概要 / Description: 専用プロファイルで別ウィンドウの実画面を取得 / Capture actual detached preview
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import base64,json,sys,time,winreg
from pathlib import Path
import win32gui,win32con
from build import ROOT
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
from autotester.cdp import CdpSession,list_targets,wait_for_page
WORK=ROOT.parent.parent/'work'
OUT=ROOT/'multichannel/illustrations';OUT.mkdir(exist_ok=True)
REG=r'Software\mdEditor'
NAMES=['PreviewDetached','PreviewWindow','PreviewZoom']
before={}
with winreg.OpenKey(winreg.HKEY_CURRENT_USER,REG) as key:
    for name in NAMES:
        try:before[name]=winreg.QueryValueEx(key,name)
        except FileNotFoundError:before[name]=None
results=[]
try:
    for lang in ['ja','en']:
        suffix='.en' if lang=='en' else ''
        app=MdEditor(ROOT.parent/'bin/Release/net10.0-windows/mdEditor.exe',file=ROOT/'samples'/f'quickstart-engineering-note{suffix}.md',lang=lang,user_data_dir=WORK/f'detached-{lang}-profile')
        preview=None
        try:
            app.start().wait_ready()
            main=app.main_window()
            win32gui.ShowWindow(main,win32con.SW_RESTORE)
            win32gui.MoveWindow(main,0,0,900,720,True)
            time.sleep(.5)
            targets=list_targets(app.port)
            if not any('?view=preview' in x.get('url','') for x in targets) and not app.js("document.body.classList.contains('preview-detached')"):
                app.click('[data-detach="preview"]')
            try:target=wait_for_page(app.port,'https://mdeditor.local/index.html?view=preview',timeout=20)
            except TimeoutError:
                app.screenshot(WORK/f'detach-debug-{lang}.png')
                print(json.dumps({'targets':list_targets(app.port),'dialogs':[d.text() for d in app.dialogs()]},ensure_ascii=True),flush=True)
                raise
            preview=CdpSession(target['webSocketDebuggerUrl'])
            # Both windows belong to this newly launched process, never the user's running app.
            from autotester.win_dialogs import top_level_windows
            for hwnd in top_level_windows(app.pid):
                if hwnd!=main and win32gui.GetClassName(hwnd)!='#32770':win32gui.MoveWindow(hwnd,700,0,650,720,True)
            deadline=time.monotonic()+20
            while time.monotonic()<deadline:
                if preview.evaluate("!!document.querySelector('#preview svg')&&!!document.querySelector('#preview .katex')"):break
                time.sleep(.1)
            else:raise RuntimeError('Detached preview did not render')
            view="window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
            app.js(f'(() => {{const v={view};const p=v.state.doc.toString().indexOf("# ");v.dispatch({{selection:{{anchor:p}}}});v.scrollDOM.scrollTop=v.lineBlockAt(p).top;}})()')
            time.sleep(.5)
            app.screenshot(OUT/f'window-editor-{lang}.png')
            (OUT/f'window-preview-{lang}.png').write_bytes(base64.b64decode(preview.send('Page.captureScreenshot',{'format':'png'})['data']))
            results.append({'language':lang,'actual_windows':2,'source_version':'1.0.5','preview_diagram_and_math':True,'files':[f'window-editor-{lang}.png',f'window-preview-{lang}.png']})
            preview.close();preview=None
            app.click('[data-detach="editor"]')
            app.wait_until("!document.body.classList.contains('preview-detached')")
            app.close()
            print('Separate windows captured: '+lang,flush=True)
        finally:
            if preview:preview.close()
            app.stop()
finally:
    # Closing the dedicated preview saves its placement. Restore only these app values.
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER,REG) as key:
        for name,value in before.items():
            if value is None:
                try:winreg.DeleteValue(key,name)
                except FileNotFoundError:pass
            else:winreg.SetValueEx(key,name,0,value[1],value[0])
(ROOT/'validation/separate-window-results.json').write_text(json.dumps({'date':'2026-10-13 JST','settings_restored':True,'results':results},indent=2),encoding='utf-8')
print('Captured separate editor and preview windows in Japanese and English; app settings restored')
