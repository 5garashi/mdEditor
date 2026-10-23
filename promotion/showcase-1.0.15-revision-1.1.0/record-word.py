"""File: record-word.py
Description: Record a corrected localized Word import scene
Author: 5garashi.com Design Office
Created: 2026-10-23
Version: 1.1.0
License: MIT
SPDX-License-Identifier: MIT
"""
﻿from pathlib import Path
import zipfile,xml.etree.ElementTree as ET,time,sys,subprocess,json
import win32gui,win32con
B=Path(__file__).resolve().parent
f=B/'source/samples/word-test.zh-CN.docx'
with zipfile.ZipFile(B/'source/samples/word-test.en.docx') as z,zipfile.ZipFile(f,'w') as o:
 for item in z.infolist():
  d=z.read(item.filename)
  if item.filename=='word/document.xml':
   root=ET.fromstring(d)
   ns='{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
   body=root.find(ns+'body');body.clear()
   for text,style in [('项目笔记','Heading1'),('将已有的 Word 文档导入，继续用 Markdown 整理。',None),('审阅流程','Heading2'),('编写文档，检查图表，然后分享成果。',None),('下一步','Heading2'),('插入图片，调整大小，并导出为 PDF。',None)]:
    p=ET.SubElement(body,ns+'p')
    if style:ET.SubElement(ET.SubElement(p,ns+'pPr'),ns+'pStyle',{ns+'val':style})
    ET.SubElement(ET.SubElement(p,ns+'r'),ns+'t').text=text
   d=ET.tostring(root,encoding='utf-8',xml_declaration=True)
  o.writestr(item,d)
sys.path[:0]=[r'C:\miFfplay\appAutoTester',r'C:\miFfplay\appAutoTester\targets\mdeditor']
from mdeditor_page import MdEditor
w=B/'word-fix-zh-CN';w.mkdir(exist_ok=True)
shield=subprocess.Popen([sys.executable,str(B/'shield.py')]);a=MdEditor(B/'app/mdEditor.exe',file=f.resolve(),lang='zh-CN',user_data_dir=w/'profile');rec=None
try:
 a.start();a.file=None;a.wait_ready(timeout=60);hwnd=a.main_window();win32gui.ShowWindow(hwnd,9);win32gui.SetWindowPos(hwnd,-1,0,0,1280,650,win32con.SWP_SHOWWINDOW)
 for i in range(30):
  for d in a.dialogs():
   ui=__import__('pywinauto').Desktop(backend='uia').window(handle=d.hwnd);buttons=ui.descendants(control_type='Button')
   if len(buttons)==1:buttons[0].invoke();d.wait_closed()
  if a.document_name().startswith('word-test.zh-CN'):break
  time.sleep(1)
 assert a.document_name().startswith('word-test.zh-CN'),(a.document_name(),a.editor_text())
 time.sleep(3)
 from PIL import ImageGrab
 ImageGrab.grab(bbox=(8,0,1272,642)).save(w/'word.png')
 rec=subprocess.Popen(['ffmpeg','-y','-nostats','-f','gdigrab','-framerate','20','-draw_mouse','0','-offset_x','8','-offset_y','0','-video_size','1264x642','-i','desktop','-t','4','-an','-c:v','libx264','-preset','ultrafast','-crf','18',str(w/'raw.mp4')],stdout=subprocess.DEVNULL,stderr=(w/'capture.log').open('w'));rec.wait(timeout=20)
 (w/'check.json').write_text(json.dumps({'file':a.document_name(),'text':a.editor_text(),'source':'corrected Chinese recording fixture XML'},ensure_ascii=False,indent=2),encoding='utf-8');print('WORD FIX RECORDED',flush=True)
finally:
 if rec and rec.poll() is None:rec.terminate()
 a.stop();shield.terminate();shield.wait(timeout=10)
