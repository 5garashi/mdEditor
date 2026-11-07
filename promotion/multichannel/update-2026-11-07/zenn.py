"""ファイル名 / File: zenn.py
概要 / Description: 連携している Zenn の記事を、mdEditor 1.0.22 と公開した YouTube revision 7.0.0 に合わせて更新する
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-11-07
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json, re, subprocess
B = Path(__file__).resolve().parent; ROOT = B.parents[1]; Z = ROOT / 'work/publish-2026-10-23/zenn'; R = ROOT / 'work/publish-2026-10-23/repo'
subprocess.run(['git', '-C', str(R), 'fetch', '-q', 'origin', 'promotion/youtube-state'], check=True)
state = json.loads(subprocess.check_output(['git', '-C', str(R), 'show', 'FETCH_HEAD:state.json'], encoding='utf-8'))
ids = {l: state['videos'][f'demo-{l.lower()}/7.0.0']['id'] for l in ['ja', 'en', 'zh-CN']}
assert all(state['videos'][f'demo-{l.lower()}/7.0.0']['phase'] == 'published' for l in ids), 'publish YouTube first'
C = json.loads((B / 'copy.json').read_text(encoding='utf-8')); COPY, FEATURES = C['copy']['ja'], C['features']['ja']
def code(t):
    # Zenn draws $…$ as math; the ways of writing math that the text names are shown as code.
    return t.replace('$$ の行', '`$$` の行').replace('閉じる $ の前', '閉じる `$` の前').replace('ある $x $、', 'ある `$x $`、')
f = Z / 'articles/mdeditor-diagrams-math-pdf.md'; s = f.read_text(encoding='utf-8')
s = s.replace('@[youtube](AhCp_Erzfow)', f'@[youtube]({ids["ja"]})', 1)
start = s.index('## 2026-11-01更新'); end = s.index('図・数式・地図・動画を一つの文書で確認し')
update = (f'## 2026-11-07更新：AIが書く数式も、Markdownで\n\n'
          f'[English demo](https://www.youtube.com/watch?v={ids["en"]}) · [简体中文演示](https://www.youtube.com/watch?v={ids["zh-CN"]})\n\n'
          + '\n\n'.join([code(COPY['new']), COPY['strong'], COPY['intro'], COPY['limits'], COPY['ai']]) + '\n\n')
s = s[:start] + update + s[end:]
s = s.replace('[ソース1.0.20の実演を見る]', '[1.0.22の実演を見る]')
s = s.replace('![mdEditorの実画面。プレビューに、赤い台の上の緑の星形の3Dモデル。](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.20-ja.png)',
              '![mdEditorの実画面。プレビューに、赤い台の上の緑の星形の3Dモデルを拡大して表示。](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.22-ja.png)')
s = s.replace('1.0.20はワンクリックのPDF保存', '1.0.22はワンクリックのPDF保存')
i = s.index('## 主な機能\n\n') + len('## 主な機能\n\n'); j = s.index('\n\n## 確認範囲と出典')
s = s[:i] + '\n'.join('- ' + x for x in FEATURES) + s[j:]
i = s.index('## 確認範囲と出典\n\n') + len('## 確認範囲と出典\n\n'); j = s.index('\n\n[PDF・画像・サンプル]')
s = s[:i] + ('新動画は、Microsoft Storeに出した1.0.22と同じコミットからのビルドを収録し、字幕とBGM「Light Steps」を付けています。3Dモデルの拡大・回転、数式の表示と左右のスクロールの連動、図・数式・3D・地図・動画の表示を、録画時に確認しました。全キー操作、異常終了からの復元、全形式の書き出し、読み上げ環境を網羅的に再試験したものではありません。背景を覆って収録し、OSのファイル選択ダイアログは編集で除いています。\n\n'
             + COPY['credits'].replace('\n', '  \n')) + s[j:]
s = s.replace('# 版 / Version: 1.9.0', '# 版 / Version: 1.10.0').replace('**最終更新 / Last updated**: 2026-11-01 JST', '**最終更新 / Last updated**: 2026-11-07 JST')
s = s.replace('[Design Office Identity 4.20.0]', '[Design Office Identity 4.25.0]')
assert '1.0.20' not in s, [s[m.start() - 40:m.end() + 10] for m in re.finditer(r'1\.0\.20', s)]
# Outside code spans, a $ may appear only in pairs that Zenn should draw as math; the update section has none.
plain = re.sub(r'`[^`]*`', '', s[s.index('## 2026-11-07'):s.index('図・数式・地図・動画を一つの文書で確認し')])
assert '$' not in plain, plain
f.write_text(s, encoding='utf-8'); (R / 'promotion/integration/zenn/mdeditor-diagrams-math-pdf.md').write_text(s, encoding='utf-8')
print('Zenn article ready', ids)
