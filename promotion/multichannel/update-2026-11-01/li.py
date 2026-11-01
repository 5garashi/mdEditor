"""File: li.py
Description: Fill the LinkedIn article editor (title and formatted body) for the owner to publish; never presses Publish
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import html, time, sys, win32clipboard
import ui
B = Path(__file__).resolve().parent
lines = (B / 'linkedin-article-en.txt').read_text(encoding='utf-8').strip('\n').split('\n\n')
title, url, rest = lines[0], lines[1], lines[2:]
HEADS = {'What is new', 'Key features', 'Recording and limits', 'Credits'}

def html_body():
    out = []
    for block in rest:
        if block in HEADS: out.append(f'<h2>{html.escape(block)}</h2>')
        elif block.startswith('• '): out.append('<ul>' + ''.join(f'<li>{html.escape(x[2:])}</li>' for x in block.split('\n')) + '</ul>')
        else: out.append(''.join('<p>' + html.escape(x) + '</p>' for x in block.split('\n')))
    return ''.join(out)

def put_html(fragment, text):
    # CF_HTML with byte offsets, plus plain text as a fallback.
    head = 'Version:0.9\r\nStartHTML:{0:010d}\r\nEndHTML:{1:010d}\r\nStartFragment:{2:010d}\r\nEndFragment:{3:010d}\r\n'
    pre = '<html><body><!--StartFragment-->'; post = '<!--EndFragment--></body></html>'
    h0 = len(head.format(0, 0, 0, 0).encode()); sf = h0 + len(pre.encode()); ef = sf + len(fragment.encode()); eh = ef + len(post.encode())
    data = (head.format(h0, eh, sf, ef) + pre + fragment + post).encode('utf-8')
    win32clipboard.OpenClipboard()
    try:
        win32clipboard.EmptyClipboard(); win32clipboard.SetClipboardData(win32clipboard.RegisterClipboardFormat('HTML Format'), data)
        win32clipboard.SetClipboardText(text, win32clipboard.CF_UNICODETEXT)
    finally: win32clipboard.CloseClipboard()

edits = ui.W.descendants(control_type='Edit')
step = sys.argv[1]
if step == 'title':
    ui.fill(edits[1], title)
elif step == 'url':
    # From the end of the title, Enter moves into the body; paste the URL alone on the first line.
    win32clipboard.OpenClipboard(); win32clipboard.EmptyClipboard(); win32clipboard.SetClipboardText(url, win32clipboard.CF_UNICODETEXT); win32clipboard.CloseClipboard()
    ui.W.set_focus(); edits[1].click_input(); time.sleep(.3); ui.W.type_keys('^{END}{ENTER}'); time.sleep(.5); ui.W.type_keys('^v'); time.sleep(1); ui.W.type_keys('{ENTER}'); time.sleep(5)
elif step == 'body':
    put_html(html_body(), '\n\n'.join(rest)); ui.W.set_focus(); ui.W.type_keys('^{END}'); time.sleep(.3); ui.W.type_keys('^v'); time.sleep(3)
ui.inspect()
