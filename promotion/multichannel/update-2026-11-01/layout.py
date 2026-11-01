"""File: layout.py
Description: Title, video, then text: an X thread and a LinkedIn article for mdEditor 1.0.20, with the short videos for X
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json, re, shutil
B = Path(__file__).resolve().parent; ROOT = B.parents[1]
K = ROOT / 'work/publish-2026-10-23/repo/promotion/multichannel/update-2026-11-01'; V = ROOT / 'deliverables/mdEditor-promo-1.0.20-revision-1.0.0'
C = json.loads((B / 'copy.json').read_text(encoding='utf-8')); COPY, FEATURES = C['copy'], C['features']
BASE = 'https://5garashi.github.io/mdEditor/'; LANDING = {'ja': '', 'en': 'en.html', 'zh-CN': 'zh-CN.html'}
YT = {'ja': 'AhCp_Erzfow', 'en': 'AkYYP5qnGDU', 'zh-CN': 'V60PrM1Y6pA'}
MUSIC = {'ja': 'BGM：Light Steps — 5garashi.com設計事務所（Codex）CC BY 4.0', 'en': 'Music: Light Steps — 5garashi.com Design Office (Codex), CC BY 4.0', 'zh-CN': '背景音乐：Light Steps — 5garashi.com Design Office（Codex）CC BY 4.0'}
def weight(t):
    t = re.sub(r'https?://\S+', 'x' * 23, t)
    return sum(1 if ord(c) < 0x1100 or 0x2000 <= ord(c) <= 0x201F else 2 for c in t)
for lang in COPY:
    c = COPY[lang]
    shutil.copyfile(V / f'mdEditor-promo-{lang}-short.mp4', K / f'mdEditor-short-{lang}.mp4')
    # X thread: the first post is the title with the video; the reply carries the text.
    first = c['title']
    reply = '\n'.join([{'ja': 'PDFとWordをMarkdownとして開けます。AIチャットの回答も書き方の癖を直して表示し、プレビューどおりのPDFに保存できます。Windows用・無料。',
                        'en': 'Open PDF and Word files as Markdown, fix the formatting of AI chat answers, and save a PDF that looks like the preview. Free.',
                        'zh-CN': '把 PDF 和 Word 作为 Markdown 打开。AI 聊天回答的格式问题会自动修正，并可保存与预览一致的 PDF。Windows 版，免费。'}[lang],
                       BASE + LANDING[lang], {'ja': 'AI（Claude・Codex）の支援で開発。', 'en': 'Built with AI assistance (Claude, Codex).', 'zh-CN': '开发使用了 AI 辅助（Claude、Codex）。'}[lang],
                       MUSIC[lang], '#Markdown' if lang == 'en' else '#mdEditor #Markdown'])
    for name, text in [('x-thread-1', first), ('x-thread-2', reply)]:
        assert weight(text) <= 280, (lang, name, weight(text)); (B / f'{name}-{lang}.txt').write_text(text, encoding='utf-8'); shutil.copyfile(B / f'{name}-{lang}.txt', K / f'{name}-{lang}.txt')
    # LinkedIn article: headline, the YouTube video on its own line (LinkedIn embeds it), then the text.
    sub = {'ja': ('今回の更新', '主な機能', '収録と制限', 'クレジット'), 'en': ('What is new', 'Key features', 'Recording and limits', 'Credits'), 'zh-CN': ('本次更新', '主要功能', '录制与限制', '致谢')}[lang]
    body = '\n\n'.join([f'https://www.youtube.com/watch?v={YT[lang]}', c['new'], c['strong'], c['intro'], sub[0], c['new'], sub[1], '\n'.join('• ' + f for f in FEATURES[lang]),
                        sub[2], c['limits'], c['ai'], BASE + LANDING[lang], sub[3], c['credits']])
    body = body.replace(c['new'] + '\n\n' + c['strong'] + '\n\n' + c['intro'] + '\n\n' + sub[0] + '\n\n' + c['new'], c['intro'] + '\n\n' + sub[0] + '\n\n' + c['new'] + '\n\n' + c['strong'])
    article = f'{c["title"]}\n\n{body}\n'
    (B / f'linkedin-article-{lang}.txt').write_text(article, encoding='utf-8'); shutil.copyfile(B / f'linkedin-article-{lang}.txt', K / f'linkedin-article-{lang}.txt')
    print(lang, 'x', weight(first), weight(reply), 'article', len(article))
