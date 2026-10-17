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
        if language not in ('ja', 'en'):
            continue
        video_id = item['id']
        if not re.fullmatch(r'[A-Za-z0-9_-]{11}', video_id):
            raise ValueError('Invalid YouTube video ID in publication state.')
        en = language == 'en'
        filename = 'youtube-en.html' if en else 'youtube.html'
        landing = 'en.html' if en else 'index.html'
        original = (output / landing).read_text(encoding='utf-8')
        header = original[original.index('<header'):original.index('</header>') + len('</header>')]
        label = 'Watch the demo on YouTube' if en else 'YouTubeで操作デモを見る'
        title = 'mdEditor Demo' if en else 'mdEditor 操作デモ'
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
<!doctype html><html lang="{language}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{title}</title><link rel="stylesheet" href="style.css"><script src="player.js" defer></script></head><body>{header}<main><section><h1>{title}</h1><p>{html.escape(item['title'])}</p><p><a class="button" href="{url}">{label}</a></p><p><a href="{landing}">{'Guide and samples' if en else '使い方とサンプル'}</a></p><p>{'Video revision' if en else '動画の版'}: {html.escape(item['revision'])}</p></section></main></body></html>'''
        (output / filename).write_text(body, encoding='utf-8')
        # No embedded player: YouTube receives a request only after the link is followed.
        link = f'<a class="button" href="{filename}">{label}</a>'
        (output / landing).write_text(original.replace('</nav>', link + '</nav>', 1), encoding='utf-8')
