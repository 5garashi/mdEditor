"""File: ui.py
Description: Operate the authorized promotion account in the existing Chrome profile
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,time,json,os,win32clipboard,win32api
from pywinauto import Desktop
from PIL import ImageGrab
B=Path(__file__).resolve().parent
W=Desktop(backend='uia').window(handle=int(os.environ.get('PROMO_CHROME_HANDLE','14027864')))
def fill(c,text):
 old=[];win32clipboard.OpenClipboard()
 try:
  f=0
  while True:
   f=win32clipboard.EnumClipboardFormats(f)
   if not f:break
   try:old.append((f,win32clipboard.GetClipboardData(f)))
   except Exception:pass
  win32clipboard.EmptyClipboard();win32clipboard.SetClipboardText(text,win32clipboard.CF_UNICODETEXT)
 finally:win32clipboard.CloseClipboard()
 try:W.set_focus();c.click_input();c.type_keys('^a^v');time.sleep(.5)
 finally:
  win32clipboard.OpenClipboard()
  try:
   win32clipboard.EmptyClipboard()
   for f,v in old:
    try:win32clipboard.SetClipboardData(f,v)
    except Exception:pass
  finally:win32clipboard.CloseClipboard()
def inspect():
 es=W.descendants();types=['Edit','Button','Hyperlink','Text','ComboBox','CheckBox','TabItem']
 print(json.dumps([(e.element_info.control_type,e.window_text()[:250]) for e in es if e.element_info.control_type in types and e.element_info.control_type!='TabItem'][-95:],ensure_ascii=False))
if __name__=='__main__':
 action=sys.argv[1]
 if action=='nav':
  next(x for x in W.descendants(control_type='Button') if x.window_text()=='新しいタブ').invoke()
  e=W.descendants(control_type='Edit')[0];fill(e,sys.argv[2]);e.type_keys('{ENTER}');time.sleep(5)
 elif action=='tab':
  next(e for e in W.descendants(control_type='TabItem') if sys.argv[2] in e.window_text()).select();time.sleep(1)
 elif action=='click':
  kind=sys.argv[3] if len(sys.argv)>3 else 'Button'
  es=[e for e in W.descendants(control_type=kind) if e.window_text()==sys.argv[2]];assert len(es)==1,[(e.window_text(),str(e.rectangle())) for e in es]
  if kind=='Text':es[0].click_input()
  else:
   try:es[0].invoke()
   except Exception:W.set_focus();es[0].click_input()
  time.sleep(2)
 elif action=='fill':
  es=W.descendants(control_type=sys.argv[2]);e=es[int(sys.argv[3])];fill(e,(B/sys.argv[4]).read_text(encoding='utf-8'))
 elif action=='file':
  d=W.child_window(class_name='#32770',title_re='.*(開く|Open).*');d.wait('visible',timeout=10);e=next(x for x in d.descendants(control_type='Edit') if x.window_text() in ['ファイル名(N):','File name:']);e.set_edit_text(str(Path(sys.argv[2]).resolve()));e.type_keys('{ENTER}');time.sleep(4)
 elif action=='shot':
  W.set_focus();r=W.rectangle();ImageGrab.grab(bbox=(r.left,r.top,r.right,r.bottom)).save(B/'current.png')
 elif action=='scroll':
  W.set_focus();r=W.rectangle();win32api.SetCursorPos((r.left+110,r.top+500));win32api.mouse_event(0x0800,0,0,int(sys.argv[2]),0);time.sleep(1)
 elif action=='point':
  W.set_focus();r=W.rectangle();win32api.SetCursorPos((r.left+int(sys.argv[2]),r.top+int(sys.argv[3])));win32api.mouse_event(2,0,0);win32api.mouse_event(4,0,0);time.sleep(1)
 inspect()
