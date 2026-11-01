"""File: zenn.py
Description: Update the linked Zenn article for mdEditor 1.0.20 with the published YouTube revision 6.0.0
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json, re, subprocess
B = Path(__file__).resolve().parent; ROOT = B.parents[1]; Z = ROOT / 'work/publish-2026-10-23/zenn'; R = ROOT / 'work/publish-2026-10-23/repo'
subprocess.run(['git', '-C', str(R), 'fetch', '-q', 'origin', 'promotion/youtube-state'], check=True)
state = json.loads(subprocess.check_output(['git', '-C', str(R), 'show', 'FETCH_HEAD:state.json'], encoding='utf-8'))
ids = {l: state['videos'][f'demo-{l.lower()}/6.0.0']['id'] for l in ['ja', 'en', 'zh-CN']}
assert all(state['videos'][f'demo-{l.lower()}/6.0.0']['phase'] == 'published' for l in ids), 'publish YouTube first'
COPY = json.loads((B / 'copy.json').read_text(encoding='utf-8'))['copy']['ja']; FEATURES = json.loads((B / 'copy.json').read_text(encoding='utf-8'))['features']['ja']
f = Z / 'articles/mdeditor-diagrams-math-pdf.md'; s = f.read_text(encoding='utf-8')
start = s.index('## 2026-10-23更新'); end = s.index('まず、文書内のSTL立方体を回転・拡大。')
update = (f'## 2026-11-01更新：PDFもAIチャットの回答も、Markdownで\n\n@[youtube]({ids["ja"]})\n\n'
          f'[English demo](https://www.youtube.com/watch?v={ids["en"]}) · [简体中文演示](https://www.youtube.com/watch?v={ids["zh-CN"]})\n\n'
          + '\n\n'.join([COPY['new'], COPY['strong'], COPY['intro'], COPY['limits'], COPY['ai']]) + '\n\n')
s = s[:start] + update + s[end:]
s = s.replace('まず、文書内のSTL立方体を回転・拡大。そのあと図・数式・地図・動画を一つの文書で確認し、編集と共有へ進みます。', '図・数式・地図・動画を一つの文書で確認し、そのまま編集・共有できます。')
s = s.replace('[ソース1.0.15の実演を見る]', '[ソース1.0.20の実演を見る]')
s = s.replace('![mdEditorの実画面。左にSTLソース、右に赤い5が付いたクリーム色の3D立方体。](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.15-ja.png)', '![mdEditorの実画面。プレビューに、赤い台の上の緑の星形の3Dモデル。](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.20-ja.png)')
s = s.replace('現行ソース1.0.15はワンクリックのPDF保存にも対応しています。', '1.0.20はワンクリックのPDF保存（ページ番号と画面読み上げ用のタグ付き）に対応し、PDFやWordをMarkdownとして開くこともできます（できる範囲での変換）。')
i = s.index('## 主な機能\n\n') + len('## 主な機能\n\n'); j = s.index('\n\n## 確認範囲と出典')
s = s[:i] + '\n'.join('- ' + x for x in FEATURES) + s[j:]
i = s.index('## 確認範囲と出典\n\n') + len('## 確認範囲と出典\n\n'); j = s.index('\n\n[PDF・画像・サンプル]')
s = s[:i] + ('新動画はソース版1.0.20を収録し、字幕とBGM「Light Steps」を付けています。AIチャットの回答の修正、PDFの取り込みと書き出し、書き方の確認、表の整形、図・数式・3D・地図・動画の表示を、録画時に確認しました。全キー操作、異常終了からの復元、全形式の書き出し、読み上げ環境を網羅的に再試験したものではありません。背景を覆って収録し、OSのファイル選択ダイアログは編集で除いています。\n\n'
             + COPY['credits'].replace('\n', '  \n')) + s[j:]
s = s.replace('# 版 / Version: 1.7.0', '# 版 / Version: 1.8.0').replace('**最終更新 / Last updated**: 2026-10-23 JST', '**最終更新 / Last updated**: 2026-11-01 JST')
assert '1.0.15' not in s.replace('image-adjust-1.0.15-ja.png', ''), [m.start() for m in re.finditer(r'1\.0\.15', s)]
f.write_text(s, encoding='utf-8'); (R / 'promotion/integration/zenn/mdeditor-diagrams-math-pdf.md').write_text(s, encoding='utf-8')
print('Zenn article ready', ids)
