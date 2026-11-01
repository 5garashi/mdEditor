"""File: manual.py
Description: Write the hand-posting guide for X, LinkedIn and TikTok (their terms forbid scripted posting through the website)
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
B = Path(__file__).resolve().parent; K = B.parents[1] / 'work/publish-2026-10-23/repo/promotion/multichannel/update-2026-11-01'
def t(name): return (B / name).read_text(encoding='utf-8')
ALT = {
 'ja': 'mdEditor 1.0.20の紹介画像。左はAIチャットの回答を直した画面、右はPDFをMarkdownとして開き元のPDFと並べた画面。PDFとWordをMarkdownとして開く、AIチャットの回答もできる範囲で再現、プレビューどおりのPDFに保存、広告なし・計測なし・アカウント不要。',
 'en': 'mdEditor 1.0.20 introduction image. Left: an AI chat answer after its fixes. Right: a PDF opened as Markdown beside the original PDF. Open PDF and Word files as Markdown, reproduce AI chat answers as closely as possible, save a PDF that looks like the preview; no ads, no tracking, no account.',
 'zh-CN': 'mdEditor 1.0.20 介绍图片。左：修正后的 AI 聊天回答。右：把 PDF 作为 Markdown 打开，与原 PDF 并排。把 PDF 和 Word 作为 Markdown 打开，尽量还原 AI 聊天回答，保存与预览一致的 PDF；无广告、无跟踪、无需账户。',
}
F = str(K).replace('/', '\\')
md = f'''# mdEditor 1.0.20 — 手で投稿する媒体

X・LinkedIn・TikTok は、規約でウェブサイトの自動操作による投稿が禁止されているため（2026-11-01 確認）、所有者が手で投稿します。ファイルはすべて次のフォルダーにあります。

`{F}`

公開済み：YouTube 日英中、紹介サイト、Zenn。リンク先はすべて紹介サイトです。

## X（@5garashi_com）

1. 画像 `social-ja.png` を添付し、代替テキスト（ALT）に下の文を入れる。
2. 本文を貼って投稿する。

本文：

```text
{t('x-ja.txt')}
```

代替テキスト：

```text
{ALT['ja']}
```

英語・中国語でも出す場合は `x-en.txt`・`x-zh-CN.txt` と `social-en.png`・`social-zh-CN.png` を使います。

## LinkedIn（本人のプロフィール、公開範囲 Anyone）

1. 画像 `social-en.png` を添付し、代替テキストを入れる。
2. 本文を貼って投稿する。

本文：

```text
{t('post-en.txt')}
```

代替テキスト：

```text
{ALT['en']}
```

日本語で出す場合は `post-ja.txt` と `social-ja.png` を使います。

## TikTok（@5garashi_com、3 本）

各言語の縦型動画をアップロードし、説明文を貼ります。BGM は AI で作ったものなので、「AI 生成コンテンツ」の表示をオンにします。公開範囲は「誰でも」。

| 動画 | 説明文 |
| :--- | :--- |
| `mdEditor-vertical-ja.mp4` | `post-ja.txt`（下） |
| `mdEditor-vertical-en.mp4` | `post-en.txt`（LinkedIn と同じ） |
| `mdEditor-vertical-zh-CN.mp4` | `post-zh-CN.txt`（下） |

日本語：

```text
{t('post-ja.txt')}
```

简体中文：

```text
{t('post-zh-CN.txt')}
```

## 投稿したら

投稿の URL を教えてください。公開記録（PUBLICATION.md）に残します。
'''
(B / 'MANUAL-POSTING.md').write_text(md, encoding='utf-8')
for l in ALT: (B / f'alt-{l}.txt').write_text(ALT[l], encoding='utf-8')
for n in ['x-ja.txt', 'x-en.txt', 'x-zh-CN.txt', 'post-ja.txt', 'post-en.txt', 'post-zh-CN.txt', 'alt-ja.txt', 'alt-en.txt', 'alt-zh-CN.txt']:
    (K / n).write_text((B / n).read_text(encoding='utf-8'), encoding='utf-8')
print('written')
