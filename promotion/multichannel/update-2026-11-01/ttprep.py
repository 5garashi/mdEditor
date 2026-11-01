"""File: ttprep.py
Description: Prepare one TikTok Studio upload in the current tab (video, caption, brand and AI labels); never presses 投稿
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys, time, win32api, win32clipboard
import ui
B = Path(__file__).resolve().parent; K = Path(r'C:\miFfplay\mdEditorPromotion\work\publish-2026-10-23\repo\promotion\multichannel\update-2026-11-01')
lang = sys.argv[1]
def texts(): return [(x.window_text(), x) for x in ui.W.descendants(control_type='Text')]
def wheel(n):
    r = ui.W.rectangle(); win32api.SetCursorPos((r.left + 600, r.top + 300)); win32api.mouse_event(0x0800, 0, 0, n, 0); time.sleep(1.2)
def click_at(el):
    r = el.rectangle(); win32api.SetCursorPos(((r.left + r.right) // 2, (r.top + r.bottom) // 2)); win32api.mouse_event(2, 0, 0); win32api.mouse_event(4, 0, 0); time.sleep(1.2)
def visible(name, kind='Text'):
    # Scroll until the element is inside the visible part of the window (the screen is shorter than the window).
    for _ in range(12):
        el = [x for x in ui.W.descendants(control_type=kind) if x.window_text() == name]
        if el and 200 < el[0].rectangle().top < 650: return el[0]
        if el: wheel(-240 if el[0].rectangle().top >= 650 else 240)
        else: wheel(-240)
    raise RuntimeError(name)
ui.W.set_focus()
click_at(visible('動画を選択', 'Button'))
d = ui.W.child_window(class_name='#32770'); d.wait('visible', timeout=10)
e = next(x for x in d.descendants(control_type='Edit') if x.window_text() in ['ファイル名(N):', 'File name:'])
e.set_edit_text(str(K / f'mdEditor-vertical-{lang}.mp4')); e.type_keys('{ENTER}')
for _ in range(90):
    if any('アップロード完了' in t for t, _ in texts()): break
    time.sleep(2)
else: raise RuntimeError('upload not finished')
# Caption: the box shows the file name until it is replaced.
box = visible(f'mdEditor-vertical-{lang}')
win32clipboard.OpenClipboard(); win32clipboard.EmptyClipboard(); win32clipboard.SetClipboardText((B / f'post-{lang}.txt').read_text(encoding='utf-8'), win32clipboard.CF_UNICODETEXT); win32clipboard.CloseClipboard()
click_at(box); ui.W.type_keys('^a'); time.sleep(.3); ui.W.type_keys('^v'); time.sleep(2); ui.W.type_keys('{ESC}'); time.sleep(1)
click_at(visible('さらに表示'))
for label in ['投稿コンテンツを開示', 'AI生成コンテンツ']:
    t = visible(label); r = t.rectangle()
    # The switch sits just right of its label.
    win32api.SetCursorPos((r.right + 28, (r.top + r.bottom) // 2)); win32api.mouse_event(2, 0, 0); win32api.mouse_event(4, 0, 0); time.sleep(1.2)
click_at(visible('あなたのブランド'))
wheel(-1200); print('prepared', lang)
