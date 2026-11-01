"""File: x.py
Description: Short X posts for mdEditor 1.0.20, checked against X's weighted 280 limit, added to the posts files
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import re
B = Path(__file__).resolve().parent; K = B.parents[1] / 'work/publish-2026-10-23/repo/promotion/multichannel/update-2026-11-01'
BASE = 'https://5garashi.github.io/mdEditor/'
X = {
 'ja': f'mdEditor 1.0.20（Windows・無料）\nPDFとWordをMarkdownとして開き、AIチャットの回答も書き方の癖を直して表示。プレビューどおりのPDFに保存できます。\n{BASE}\nAI（Claude・Codex）の支援で開発しています。\n#mdEditor #Markdown',
 'en': f'mdEditor 1.0.20 for Windows (free): open PDF and Word files as Markdown, fix the formatting quirks of AI chat answers, and save a PDF that looks like the preview.\n{BASE}en.html\nBuilt with AI assistance (Claude, Codex).\n#Markdown #Windows',
 'zh-CN': f'mdEditor 1.0.20（Windows，免费）\n把 PDF 和 Word 作为 Markdown 打开，自动修正 AI 聊天回答的格式问题，保存与预览一致的 PDF。\n{BASE}zh-CN.html\n开发使用了 AI 辅助（Claude、Codex）。\n#Markdown',
}
def weight(t):
    # X counts a URL as 23 and most CJK characters as 2.
    t = re.sub(r'https?://\S+', 'x' * 23, t)
    return sum(1 if ord(c) < 0x1100 or 0x2000 <= ord(c) <= 0x201F else 2 for c in t)
for lang, text in X.items():
    n = weight(text); assert n <= 280, (lang, n)
    (B / f'x-{lang}.txt').write_text(text, encoding='utf-8')
    f = K / f'posts-{lang}.md'; s = f.read_text(encoding='utf-8')
    s = re.sub(r'## 短い投稿（X）\n\n```text\n.*?```\n', f'## 短い投稿（X、{n}/280）\n\n```text\n{text}\n```\n', s, flags=re.S)
    f.write_text(s, encoding='utf-8'); print(lang, n)
