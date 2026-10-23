"""File: latest_pages.py
Description: Build stable bilingual demo links from confirmed public YouTube state.
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import html
import json
import re


def generate_latest(output, state_file):
    if not state_file.exists():
        return
    latest = json.loads(state_file.read_text(encoding='utf-8')).get('latest', {})
    for language, item in latest.items():
        if language not in ('ja', 'en', 'zh-CN'):
            continue
        video_id = item['id']
        if not re.fullmatch(r'[A-Za-z0-9_-]{11}', video_id):
            raise ValueError('Invalid YouTube video ID in publication state.')
        en = language == 'en'
        filename = {'ja':'youtube.html','en':'youtube-en.html','zh-CN':'youtube-zh-CN.html'}[language]
        landing = {'ja':'index.html','en':'en.html','zh-CN':'zh-CN.html'}[language]
        original = (output / landing).read_text(encoding='utf-8')
        header = original[original.index('<header'):original.index('</header>') + len('</header>')]
        label = {'ja':'YouTubeで操作デモを見る','en':'Watch the demo on YouTube','zh-CN':'在 YouTube 上观看实际操作'}[language]
        title = {'ja':'mdEditor 操作デモ','en':'mdEditor Demo','zh-CN':'mdEditor 实际操作'}[language]
        url = 'https://www.youtube.com/watch?v=' + video_id
        body = f'''<!--
File: {filename}
Description: Latest confirmed public mdEditor demo; generated from publication state
Author: 5garashi.com Design Office
Created: 2026-10-13
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
-->
<!doctype html><html lang="{language}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{title}</title><link rel="stylesheet" href="style.css"><script src="player.js" defer></script></head><body>{header}<main><section><h1>{title}</h1><p>{html.escape(item['title'])}</p><p><a class="button" href="{url}">{label}</a></p><p><a href="{landing}">{dict(en='Guide and samples',ja='使い方とサンプル',**{'zh-CN':'使用方法与示例'})[language]}</a></p><p>{dict(en='Video revision',ja='動画の版',**{'zh-CN':'视频版本'})[language]}: {html.escape(item['revision'])}</p></section></main></body></html>'''
        (output / filename).write_text(body, encoding='utf-8')
        # No embedded player: YouTube receives a request only after the link is followed.
        link = f'<a class="button" href="{filename}">{label}</a>'
        (output / landing).write_text(original.replace('</nav>', link + '</nav>', 1), encoding='utf-8')
