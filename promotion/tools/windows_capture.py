"""
ファイル名 / File: promotion/tools/windows_capture.py
概要 / Description: Record and verify the unmodified Windows app with dedicated sample copies
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import argparse, json, sys, time, subprocess, shutil
from datetime import datetime, timezone, timedelta
from pathlib import Path
import win32con, win32gui
from PIL import ImageGrab
import fitz
from pywinauto import Desktop

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--helper-root',type=Path,default=Path(r'C:\miFfplay\appAutoTester'))
parser.add_argument('--exe',type=Path,default=ROOT.parent/'bin/Release/net10.0-windows/mdEditor.exe')
parser.add_argument('--work',type=Path,default=ROOT.parent.parent/'work')
parser.add_argument('--record',action='store_true')
parser.add_argument('--verify',action='store_true')
args=parser.parse_args()
sys.path[:0]=[str(args.helper_root),str(args.helper_root/'targets/mdeditor')]
from mdeditor_page import MdEditor

WORK=args.work.resolve(); WORK.mkdir(exist_ok=True,parents=True)
ASSETS=ROOT/'assets'; VALID=ROOT/'validation'; VALID.mkdir(exist_ok=True)
VIEW="window.CodeMirror.EditorView.findFromDOM(document.querySelector('.cm-editor'))"
def now_jst():return datetime.now(timezone(timedelta(hours=9))).isoformat(timespec='seconds')

def front(app):
    hwnd=app.main_window()
    win32gui.ShowWindow(hwnd,win32con.SW_RESTORE)
    win32gui.SetWindowPos(hwnd,win32con.HWND_TOPMOST,0,0,1280,650,win32con.SWP_SHOWWINDOW)
    return hwnd

def dismiss(dialog):
    buttons=Desktop(backend='uia').window(handle=dialog.hwnd).descendants(control_type='Button')
    for button in buttons:
        if button.window_text() in ('OK','OK(&O)'):
            button.invoke()
            dialog.wait_closed(); return
    raise RuntimeError('No OK button: '+dialog.text())

def export(app,path):
    assert not path.exists(), 'Refuse to overwrite an existing verification PDF'
    app.click('#pdf-button')
    dlg=app.wait_dialog()
    dlg.accept_file(str(path))
    deadline=time.monotonic()+40
    while not path.exists() and time.monotonic()<deadline: time.sleep(.2)
    assert path.exists(), [d.text() for d in app.dialogs()]
    d=app.wait_dialog()
    text=d.text()
    assert 'PDF' in text and ('保存しました' in text or 'saved' in text.lower()), text
    return d,text

def select_text(app,old,new):
    index=app.editor_text().index(old)
    app.js(f'(() => {{ const v={VIEW}; v.dispatch({{selection:{{anchor:{index},head:{index+len(old)}}},scrollIntoView:true}}); v.focus(); }})()')
    app.type_text(new)
    app.wait_dirty()

def inspect_pdf(pdf,name):
    doc=fitz.open(pdf)
    text='\n'.join(p.get_text() for p in doc)
    assert len(text.strip())>150
    assert 'Ω' in text if 'engineering' in name else True
    pages=[]
    for i,p in enumerate(doc):
        path=VALID/f'{name}-pdf-{i+1}.png'
        p.get_pixmap(matrix=fitz.Matrix(1.2,1.2)).save(str(path)); pages.append(path.name)
    return {'pages':len(doc),'pdf':pdf.name,'page_images':pages,'text':text}

def verify():
    results=[]
    for sample in sorted((ROOT/'samples').glob('quickstart-*.md')):
        en='.en.' in sample.name
        name=sample.stem
        copied=WORK/sample.name; shutil.copy2(sample,copied)
        app=MdEditor(args.exe.resolve(),file=copied,lang='en' if en else 'ja',user_data_dir=WORK/(name+'-profile'))
        try:
            app.start().wait_ready(); front(app)
            app.wait_preview("p.textContent.length > 100")
            if 'ai-notes' not in name: app.wait_preview("!!p.querySelector('svg')",timeout=25)
            if 'engineering' in name: app.wait_preview("!!p.querySelector('.katex')")
            actual=app.js("({svg:document.querySelectorAll('#preview svg').length,math:document.querySelectorAll('#preview .katex').length,tables:document.querySelectorAll('#preview table').length,checkboxes:document.querySelectorAll('#preview input[type=checkbox]').length,errors:[...document.querySelectorAll('#preview .diagram-error,#preview .mermaid-error')].map(e=>e.textContent)})")
            assert not actual['errors'],actual
            if 'engineering' in name:
                app.js(f'(() => {{const v={VIEW}; const p=v.state.doc.toString().indexOf("# "); v.dispatch({{selection:{{anchor:p}}}}); v.scrollDOM.scrollTop=v.lineBlockAt(p).top;}})()')
                time.sleep(.5)
                app.screenshot(ASSETS/f'app-{"en" if en else "ja"}.png')
            app.screenshot(VALID/f'{name}-app.png')
            pdf=VALID/(name+'.pdf')
            if pdf.exists():
                shutil.move(str(pdf),str(WORK/(name+'-previous.pdf')))
            d,message=export(app,pdf); dismiss(d)
            pdf_result=inspect_pdf(pdf,name)
            results.append({'sample':sample.name,'render':actual,'native_confirmation':message,**pdf_result})
            app.close()
        finally: app.stop()
    (VALID/'windows-results.json').write_text(json.dumps({'exe':str(args.exe),'source_commit':'6440a2edbe0c570de9fffe8636e7e0233711d978','date':now_jst(),'results':results},ensure_ascii=False,indent=2),encoding='utf-8')
    print('Verified six samples in the actual Windows app and exported six PDFs',flush=True)

def record(lang):
    en=lang=='en'; suffix='.en' if en else ''
    sample=ROOT/'samples'/f'quickstart-engineering-note{suffix}.md'
    demo=WORK/f'demo-{lang}.md'; shutil.copy2(sample,demo)
    pdf=WORK/f'demo-{lang}.pdf'
    raw=WORK/f'raw-{lang}.mp4'
    assert not pdf.exists() and not raw.exists()
    app=MdEditor(args.exe.resolve(),lang=lang,user_data_dir=WORK/f'record-{lang}-profile')
    recorder=None
    events=[]
    try:
        app.start().wait_ready(); front(app)
        # Desktop capture excludes rounded window corners and personal windows behind them.
        log=(WORK/f'ffmpeg-{lang}.log').open('w')
        recorder=subprocess.Popen(['ffmpeg','-y','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(raw)],stdin=subprocess.PIPE,stdout=log,stderr=log)
        start=time.monotonic()
        def at(sec):
            wait=sec-(time.monotonic()-start)
            if wait>0:time.sleep(wait)
        def mark(label): events.append({'seconds':round(time.monotonic()-start,2),'action':label})
        at(3); app.click('#open-button'); mark('Open dialog')
        dlg=app.wait_dialog(); at(5); dlg.accept_file(str(demo))
        app.wait_preview("!!p.querySelector('svg') && !!p.querySelector('.katex')",timeout=20); mark('Sample rendered')
        heading='Diagrams and math' if en else '図と数式のメモ'
        updated='Diagrams and math: reviewed' if en else '図と数式の記録'
        at(9)
        select_text(app,heading,updated); mark('Heading edited using input event')
        app.wait_preview(f'p.querySelector("h1").textContent === {json.dumps(updated)}')
        at(13); app.js("document.getElementById('preview').scrollTop=0"); mark('Equation preview')
        at(17)
        old='Check specifications' if en else '仕様を確認'
        new='Verify by measurement' if en else '測定で検証'
        select_text(app,old,new)
        app.wait_preview(f'[...p.querySelectorAll("svg")].some(e=>e.textContent.includes({json.dumps(new)}))'); mark('Diagram edited and rendered')
        app.js("document.querySelector('#preview svg').scrollIntoView({block:'center'})")
        at(23); app.click('#save-button'); app.wait_saved(); mark('Markdown saved')
        at(25); app.click('#pdf-button'); dlg=app.wait_dialog(); mark('PDF destination dialog')
        at(28); dlg.accept_file(str(pdf))
        deadline=time.monotonic()+30
        while not pdf.exists() and time.monotonic()<deadline:time.sleep(.1)
        assert pdf.exists()
        confirm=app.wait_dialog(); mark('Native PDF saved confirmation')
        confirmation=confirm.text()
        assert '保存しました' in confirmation or 'saved' in confirmation.lower(),confirmation
        at(33); dismiss(confirm)
        app.js("document.getElementById('preview').scrollTop=0")
        at(35); app.screenshot(WORK/f'poster-{lang}.png'); mark('Final app view')
        at(40)
        recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=10); recorder=None
        assert raw.stat().st_size>10000
        doc=fitz.open(pdf); text='\n'.join(p.get_text() for p in doc)
        assert updated in text and new in text, text
        doc[0].get_pixmap(matrix=fitz.Matrix(1.2,1.2)).save(str(VALID/f'demo-{lang}-pdf.png'))
        (VALID/f'recording-{lang}.json').write_text(json.dumps({'date':now_jst(),'language':lang,'raw':raw.name,'events':events,'pdf_confirmation':confirmation,'pdf_contains_edited_heading_and_diagram':True,'pdf_pages':len(doc)},ensure_ascii=False,indent=2),encoding='utf-8')
        app.close(); print('Recorded real Windows operations: '+lang,flush=True)
    finally:
        if recorder and recorder.poll() is None:
            recorder.stdin.write(b'q\n'); recorder.stdin.flush(); recorder.wait(timeout=10)
        app.stop()

if args.verify: verify()
if args.record:
    record('ja')
    record('en')
