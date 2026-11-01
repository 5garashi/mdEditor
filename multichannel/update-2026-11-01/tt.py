"""File: tt.py
Description: Put a caption into the TikTok Studio description box at a window position; never presses 投稿
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys, time, win32clipboard, win32api
import ui
B = Path(__file__).resolve().parent
text, x, y = (B / sys.argv[1]).read_text(encoding='utf-8'), int(sys.argv[2]), int(sys.argv[3])
win32clipboard.OpenClipboard(); win32clipboard.EmptyClipboard(); win32clipboard.SetClipboardText(text, win32clipboard.CF_UNICODETEXT); win32clipboard.CloseClipboard()
ui.W.set_focus(); r = ui.W.rectangle(); win32api.SetCursorPos((r.left + x, r.top + y)); win32api.mouse_event(2, 0, 0); win32api.mouse_event(4, 0, 0); time.sleep(.5)
ui.W.type_keys('^a'); time.sleep(.3); ui.W.type_keys('^v'); time.sleep(2); ui.W.type_keys('{ESC}'); time.sleep(1)
