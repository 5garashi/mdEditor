"""File: prepare.py
Description: Prepare the mdEditor 1.0.20 trilingual release: site pages, YouTube revision 6.0.0, posts, social images and vertical videos
Author: 5garashi.com Design Office
Created: 2026-11-01
Version: 1.14.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json, re, shutil, subprocess
from PIL import Image, ImageDraw, ImageFont
from bs4 import BeautifulSoup

B = Path(__file__).resolve().parent; ROOT = B.parents[1]
R = ROOT / 'work/publish-2026-10-23/repo'; P = R / 'promotion'; K = P / 'multichannel/update-2026-11-01'; K.mkdir(parents=True, exist_ok=True)
V = ROOT / 'deliverables/mdEditor-promo-1.0.20-revision-1.0.0'
BASE = 'https://5garashi.github.io/mdEditor/'; DATE = '2026-11-01'; KIT = '1.14.0'
LANGS = ['ja', 'en', 'zh-CN']
LANDING = {'ja': '', 'en': 'en.html', 'zh-CN': 'zh-CN.html'}
FONT = {'ja': 'C:/Windows/Fonts/YuGothB.ttc', 'en': 'C:/Windows/Fonts/segoeuib.ttf', 'zh-CN': 'C:/Windows/Fonts/msyhbd.ttc'}
GREEN = '#385723'; PAPER = '#F9EFCB'; HEAD = '#FFE699'

COPY = {
 'ja': {
  'title': 'mdEditor 1.0.20 — PDFもAIチャットの回答も、Markdownで開いて直す',
  'new': 'PDFとWordをMarkdownとして開けます。AIチャットの回答も、書き方の癖を直して表示。プレビューどおりのPDFに、ワンクリックで保存できます。',
  'strong': '書きながら横で仕上がりを確認。Mermaid・Graphviz・数式・3Dモデル・地図をオフラインで表示。書き間違いには波線、表は Ctrl+Shift+W で整えます。',
  'intro': '回転する3Dモデルから始まり、AIチャットの回答の修正、PDFの取り込みと書き出し、書き方の確認を実演。そのあと、主な機能と図のサンプルを上から下へ紹介します。',
  'limits': 'Windows用・無料。PDFの取り込みはできる範囲での変換で、段組みやスキャンしたPDFは元に戻りません。AIチャットの回答は、できる範囲で再現します。収録はソースビルド1.0.20です。',
  'ai': '開発と動画の制作に、AI（Claude・Codex）の支援を受けています。BGMはCodexで作曲・音源生成したものです。',
  'lines': ['PDFとWordを、Markdownとして開く', 'AIチャットの回答も、できる範囲で再現', 'プレビューどおりのPDFに保存', '広告なし・計測なし・アカウント不要'],
  'head': 'PDFもAIの回答も、Markdownで。',
  'store': 'Microsoft Storeで無料',
  'chapters': ['はじめに・3Dモデル', 'AIチャットの回答を直す', 'PDFの取り込みと書き出し', '書き方の確認と表の整形', '主な機能', '図と詳細機能'],
  'outline': ['3Dモデルを拡大して回す。', 'AIチャットの回答を開き、直す前と直した後を切り替える。', 'PDFをMarkdownとして開き、元のPDFと並べる。PDFに書き出して開き直す。', '書き間違いの波線と一覧、Ctrl+Shift+Wでの表の整形。', 'key-features.mdとdiagram-test.mdを上から下へ。'],
  'section': '今回のアップデート（1.0.20）',
  'credits': 'BGM：「Light Steps」5garashi.com設計事務所（作曲・音源生成：Codex）、CC BY 4.0。抜粋してくり返し、最後をフェードアウト。\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\nサンプルの映画：「Big Buck Bunny」© 2008 Blender Foundation、CC BY 3.0。サンプル文書の中で再生（編集なし）。\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\n地図データ：© OpenStreetMap contributors（ODbL）https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #PDF',
 },
 'en': {
  'title': 'mdEditor 1.0.20 — Open PDFs and AI Chat Answers as Markdown',
  'new': 'Open PDF and Word files as Markdown. AI chat answers open with their formatting quirks fixed. Save a PDF that looks like the preview, in one click.',
  'strong': 'Write beside a live preview. Mermaid, Graphviz, math, 3D models and maps, offline. Markdown mistakes are underlined, and Ctrl+Shift+W aligns table columns.',
  'intro': 'Starts with a rotating 3D model, then shows an AI chat answer being fixed, PDF import and export, and the Markdown check, followed by a top-to-bottom tour of the key features and diagram samples.',
  'limits': 'Windows only, free. PDF import is best effort: multi-column layouts and scanned PDFs do not come back. AI chat answers are reproduced as closely as possible. Recorded from source build 1.0.20.',
  'ai': 'Developed and produced with AI assistance (Claude, Codex). The music was composed and generated with Codex.',
  'lines': ['Open PDF and Word files as Markdown', 'AI chat answers, reproduced as closely as possible', 'Save a PDF that looks like the preview', 'No ads. No tracking. No account.'],
  'head': 'PDFs and AI answers, as Markdown.',
  'store': 'Free on Microsoft Store',
  'chapters': ['Intro and 3D model', 'Fixing an AI chat answer', 'PDF import and export', 'Markdown check and tables', 'Key features', 'Diagrams and more features'],
  'outline': ['Zoom and rotate a 3D model.', 'Open an AI chat answer and switch between before and after the fixes.', 'Open a PDF as Markdown beside the original PDF; save a PDF and open it again.', 'Underlined Markdown mistakes with their list, and Ctrl+Shift+W for tables.', 'Tour key-features.en.md and diagram-test.en.md from top to bottom.'],
  'section': 'New in this update (1.0.20)',
  'credits': 'Music: "Light Steps" by 5garashi.com Design Office (composed and generated with Codex), CC BY 4.0. Changes: excerpt, looped, faded out at the end.\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\nSample film: "Big Buck Bunny" © 2008 Blender Foundation, CC BY 3.0. Shown as it plays inside a sample document; not edited.\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\nMap data: © OpenStreetMap contributors (ODbL) https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #PDF',
 },
 'zh-CN': {
  'title': 'mdEditor 1.0.20 — 把 PDF 和 AI 聊天回答作为 Markdown 打开',
  'new': '把 PDF 和 Word 作为 Markdown 打开。AI 聊天回答的格式问题也会自动修正。可以一键保存与预览一致的 PDF。',
  'strong': '一边编写，一边实时预览。离线显示 Mermaid、Graphviz、公式、3D 模型和地图。波浪线标出 Markdown 语法问题，Ctrl+Shift+W 对齐表格列。',
  'intro': '从旋转的 3D 模型开始，演示修正 AI 聊天回答、导入和导出 PDF、检查 Markdown 写法，然后从上到下浏览主要功能和图表示例。',
  'limits': '仅支持 Windows，免费。PDF 导入会尽量转换，分栏排版和扫描 PDF 无法还原。AI 聊天回答会尽量还原。使用源代码构建版 1.0.20 录制。',
  'ai': '开发和视频制作使用了 AI 辅助（Claude、Codex）。背景音乐由 Codex 作曲并生成音源。',
  'lines': ['把 PDF 和 Word 作为 Markdown 打开', 'AI 聊天回答，也尽量还原', '保存与预览一致的 PDF', '无广告、无跟踪、无需账户'],
  'head': 'PDF 和 AI 回答，都用 Markdown 打开。',
  'store': '在 Microsoft Store 免费获取',
  'chapters': ['开场与 3D 模型', '修正 AI 聊天回答', 'PDF 导入与导出', '检查写法与对齐表格', '主要功能', '图表与更多功能'],
  'outline': ['放大并旋转 3D 模型。', '打开 AI 聊天回答，切换修正前后。', '把 PDF 作为 Markdown 打开，与原 PDF 并排；保存为 PDF 后重新打开。', '波浪线标出写法问题和列表，用 Ctrl+Shift+W 对齐表格。', '从上到下浏览 key-features.zh-CN.md 和 diagram-test.zh-CN.md。'],
  'section': '本次更新（1.0.20）',
  'credits': '背景音乐：“Light Steps”，5garashi.com Design Office（作曲与音源生成：Codex），CC BY 4.0。节选、循环播放，结尾淡出。\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\n示例影片：“Big Buck Bunny” © 2008 Blender Foundation，CC BY 3.0。在示例文档中播放，未经编辑。\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\n地图数据：© OpenStreetMap contributors（ODbL）https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #PDF',
 },
}
# Twenty features, from store/STORE-LISTING.md of 1.0.20 (the Chinese PDF line says 一致, as the English and Japanese do).
STORE = (ROOT.parent / 'mdEditor/store/STORE-LISTING.md').read_text(encoding='utf-8')
def bullets(heading):
    part = STORE.replace('\r\n', '\n').split(heading, 1)[1].strip('\n').split('\n\n', 1)[0]
    return [x[2:].strip() for x in part.splitlines() if x.startswith('- ')]
FEATURES = {'en': bullets('**Features (bullets):**'), 'ja': bullets('**製品の機能（箇条書き）:**'), 'zh-CN': bullets('**产品功能（要点）:**')}
FEATURES['zh-CN'] = [x.replace('外观与预览完全相同', '外观与预览一致') for x in FEATURES['zh-CN']]
assert all(len(v) == 20 for v in FEATURES.values()), {k: len(v) for k, v in FEATURES.items()}

def duration(path):
    return float(subprocess.check_output(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'csv=p=0', str(path)]).strip())
def chapters(lang):
    # YouTube chapters need at least three, each 10 s or longer; use the six sections.
    lines = (V / f'chapters-{lang}.txt').read_text(encoding='utf-8').splitlines()
    return [lines[i][:5] + ' ' + t for i, t in zip([0, 2, 6, 11, 13, 26], COPY[lang]['chapters'])]
def font(lang, size): return ImageFont.truetype(FONT[lang], size)

def images(lang):
    c = COPY[lang]; url = BASE + LANDING[lang]; opening = Image.open(V / f'opening-{lang}.png').convert('RGB')
    for name in ['hero-3d', 'video-poster']:
        opening.save(P / f'assets/{name}-1.0.20-{lang}.png')
    # YouTube thumbnail, 1280x720.
    t = Image.new('RGB', (1280, 720), PAPER); d = ImageDraw.Draw(t); d.rectangle((0, 0, 1280, 96), fill=HEAD); d.line((0, 96, 1280, 96), fill=GREEN, width=3)
    d.text((36, 18), 'mdEditor 1.0.20', font=font(lang, 48), fill=GREEN)
    shot = opening.copy(); shot.thumbnail((1180, 520)); t.paste(shot, ((1280 - shot.width) // 2, 112))
    d.text((40, 648), c['head'], font=font(lang, 44), fill='#B92613'); t.save(P / f'assets/youtube-1.0.20-{lang}.png')
    # Social image, 1600x1100.
    s = Image.new('RGB', (1600, 1100), PAPER); d = ImageDraw.Draw(s); d.rectangle((0, 0, 1600, 140), fill=HEAD)
    d.text((45, 30), 'mdEditor 1.0.20', font=font(lang, 60), fill=GREEN); d.text((45, 168), c['head'], font=font(lang, 50), fill='#B92613')
    for i, name in enumerate(['ai-after', 'pdf-compare']):
        shot = Image.open(V / f'{name}-{lang}.png').convert('RGB'); shot.thumbnail((740, 400)); s.paste(shot, (45 + i * 770, 270))
    for i, line in enumerate(c['lines']): d.text((45, 710 + i * 62), line, font=font(lang, 34), fill=GREEN)
    d.text((45, 975), url, font=font(lang, 28), fill=GREEN); d.text((45, 1035), f'Source 1.0.20 | 5garashi.com Design Office | {DATE}', font=font(lang, 22), fill='#7B4A21')
    s.save(K / f'social-{lang}.png')

def vertical(lang):
    # 1080x1920: heading band, the app enlarged, then the landscape video with its captions, then the Store line.
    c = COPY[lang]; frame = Image.new('RGB', (1080, 1920), PAPER); d = ImageDraw.Draw(frame)
    d.rectangle((0, 0, 1080, 190), fill=HEAD); d.line((0, 190, 1080, 190), fill=GREEN, width=4)
    d.text((40, 30), 'mdEditor 1.0.20', font=font(lang, 56), fill=GREEN); d.text((40, 112), c['head'], font=font(lang, 42), fill='#B92613')
    d.text((40, 1700), c['store'], font=font(lang, 48), fill=GREEN); d.text((40, 1790), BASE + LANDING[lang], font=font(lang, 30), fill=GREEN)
    d.text((40, 1850), 'Music: Light Steps — 5garashi.com Design Office (Codex), CC BY 4.0', font=font('en', 22), fill='#7B4A21')
    bg = B / f'vertical-{lang}.png'; frame.save(bg); out = K / f'mdEditor-vertical-{lang}.mp4'
    graph = ('[0:v]split=2[full][detail];[detail]crop=900:560:190:40,scale=1080:-2[zoom];[full]scale=1080:-2[small];'
             '[1:v][zoom]overlay=0:230[tmp];[tmp][small]overlay=0:1000,format=yuv420p[v]')
    subprocess.run(['ffmpeg', '-y', '-v', 'error', '-i', str(V / f'mdEditor-promo-{lang}.mp4'), '-loop', '1', '-framerate', '20', '-i', str(bg),
                    '-filter_complex', graph, '-map', '[v]', '-map', '0:a', '-shortest', '-r', '20', '-c:v', 'libx264', '-preset', 'medium', '-crf', '21',
                    '-c:a', 'copy', '-movflags', '+faststart', str(out)], check=True)
    bg.unlink()

def texts(lang):
    c = COPY[lang]; url = BASE + LANDING[lang]
    desc = '\n\n'.join([c['new'], c['strong'], c['intro'], '\n'.join(chapters(lang)), '\n'.join('- ' + f for f in FEATURES[lang]), url, c['limits'], c['ai'], c['credits']])
    (P / f'integration/youtube/automation/description-{lang}.txt').write_text(desc, encoding='utf-8')
    post = '\n\n'.join([c['title'], c['new'], c['intro'], url, c['limits'], c['ai'], c['credits'], c['tags']])
    short = '\n\n'.join([c['title'], c['new'], url, c['ai'], c['tags']])
    (K / f'posts-{lang}.md').write_text(header(f'posts-{lang}.md') + f'# mdEditor 1.0.20 — {lang}\n\n## 長い投稿（LinkedIn・TikTok・YouTube の説明）\n\n```text\n{post}\n```\n\n## 短い投稿（X）\n\n```text\n{short}\n```\n', encoding='utf-8')
    (B / f'post-{lang}.txt').write_text(post, encoding='utf-8'); (B / f'post-short-{lang}.txt').write_text(short, encoding='utf-8')

def header(name):
    return f'<!--\nファイル名 / File: {name}\n概要 / Description: mdEditor 1.0.20 trilingual release material\n作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n作成日 / Created: {DATE}\n版 / Version: {KIT}\nライセンス / License: MIT\nSPDX-License-Identifier: MIT\n-->\n'

def page(lang, name):
    c = COPY[lang]; f = P / name; s = f.read_text(encoding='utf-8')
    for a, b in [('mdEditor-demo-1.0.15', 'mdEditor-demo-1.0.20'), ('demo-1.0.15', 'demo-1.0.20'), ('hero-3d-1.0.15', 'hero-3d-1.0.20'), ('video-poster-1.0.15', 'video-poster-1.0.20'),
                 ('Windows用mdEditor 1.0.15の実演と20機能。', 'Windows用mdEditor 1.0.20の実演と20機能。PDFもAIチャットの回答もMarkdownで。'),
                 ('Explore mdEditor 1.0.15 for Windows.', 'Open PDFs and AI chat answers as Markdown. Explore mdEditor 1.0.20 for Windows.'),
                 ('1.0.15の実演では、画像のドロップから50％幅・中央揃えまでを紹介。', '画像はドロップや貼り付けで入れ、50％幅・中央揃えなどに調整できます（画面は1.0.15）。'),
                 ('The 1.0.15 demonstration shows image drag-and-drop, 50% width and centered alignment.', 'Drop or paste an image, then set it to 50% width and center it (screen from 1.0.15).'),
                 ('ソース版1.0.15の実画面です。', 'ソース版1.0.20の実画面です。'), ('Actual source build 1.0.15. ', 'Actual source build 1.0.20.')]:
        s = s.replace(a, b)
    soup = BeautifulSoup(s, 'html.parser')
    old = soup.find(id='new-1-0-15'); new = soup.new_tag('section', id='new-1-0-20'); h = soup.new_tag('h2'); h.string = c['section']; new.append(h)
    for t in [c['new'], c['strong']]:
        p = soup.new_tag('p'); p.string = t; new.append(p)
    p = soup.new_tag('p', attrs={'class': 'note'}); p.string = c['limits'] + ' ' + c['ai']; new.append(p)
    if old: old.replace_with(new)
    else: soup.find('main').insert(1, new)
    # The Chinese page carried the 1.0.15 copy as loose paragraphs; take those out.
    oldcopy = json.loads((ROOT / 'work/publish-2026-10-23/copy.json').read_text(encoding='utf-8'))[lang]
    for p in soup.find('main').find_all('p'):
        if p.get_text().strip() in (oldcopy['new'], oldcopy['strong'], oldcopy['limits']) or '剪贴板快捷键为可选设置' in p.get_text():
            p.decompose()
    lst = soup.select_one('#feature-list ol') or next(s2.find('ol') for s2 in soup.find_all('section') if s2.find('h2') and s2.find('h2').get_text().startswith('20'))
    lst.clear()
    for x in FEATURES[lang]:
        li = soup.new_tag('li'); li.string = x; lst.append(li)
    for img in soup.select('img[src*="hero-3d-1.0.20"]'):
        img['alt'] = {'ja': 'mdEditorの実画面。プレビューに、赤い台の上の緑の星形の3Dモデル。', 'en': 'mdEditor screen: a green star-shaped 3D model on a red base in the preview.', 'zh-CN': 'mdEditor 实际界面：预览中红色底座上的绿色星形 3D 模型。'}[lang]
        img['width'], img['height'] = '1264', '624'
    secs = round(duration(V / f'mdEditor-promo-{lang}.mp4'))
    for o in soup.select('output.player-clock'): o.string = f'00:00 / {secs // 60:02}:{secs % 60:02}'
    for r in soup.select('input[data-seek]'): r['max'] = str(secs)
    det = soup.select_one('section.demo details ol')
    if det:
        det.clear()
        for x in c['outline']:
            li = soup.new_tag('li'); li.string = x; det.append(li)
    out = str(soup).replace('1.13.0', KIT).replace('2026-10-23', DATE)
    f.write_text(out, encoding='utf-8')

for lang in LANGS:
    for ext, dst in [('mp4', f'mdEditor-demo-1.0.20-{lang}.mp4'), ('vtt', f'demo-1.0.20-{lang}.vtt')]:
        shutil.copyfile(V / f'mdEditor-promo-{lang}.{ext}', P / 'assets' / dst)
    images(lang); vertical(lang); texts(lang)
for lang, name in [('ja', 'index.html'), ('en', 'en.html'), ('zh-CN', 'zh-CN.html')]:
    page(lang, name)
f = P / 'resources-zh-CN.html'; oldzh = json.loads((ROOT / 'work/publish-2026-10-23/copy.json').read_text(encoding='utf-8'))['zh-CN']
f.write_text(f.read_text(encoding='utf-8').replace(oldzh['new'], COPY['zh-CN']['new']).replace(oldzh['strong'], COPY['zh-CN']['strong']).replace(oldzh['limits'], COPY['zh-CN']['limits']).replace('mdEditor-demo-1.0.15', 'mdEditor-demo-1.0.20').replace('demo-1.0.15', 'demo-1.0.20').replace('1.13.0', KIT).replace('2026-10-23', DATE), encoding='utf-8')
# YouTube: add revision 6.0.0 as new non-public uploads first (sync); publishing is a separate, later push.
A = P / 'integration/youtube/automation'; cfg = json.loads((A / 'release.json').read_text(encoding='utf-8'))
cfg.update(operation='sync', request_id=f'{DATE}-source-1.0.20-6.0.0-sync')
for v in cfg['videos']:
    l = v['language']
    v.update(revision='6.0.0', media=f'promotion/assets/mdEditor-demo-1.0.20-{l}.mp4', captions=f'promotion/assets/demo-1.0.20-{l}.vtt', thumbnail=f'promotion/assets/youtube-1.0.20-{l}.png', title=COPY[l]['title'], target_privacy='public', tags=['Markdown', 'Windows', 'PDF', 'Mermaid', 'KaTeX', 'mdEditor'])
(A / 'release.json').write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
(P / 'VERSION').write_text(KIT + '\n', encoding='utf-8'); (P / 'integration/VERSION').write_text('1.5.0\n', encoding='utf-8')
for file, version, text in [(P / 'CHANGELOG.md', KIT, '1.0.20 の日英中の動画・縦型動画・紹介ページ・投稿文。PDF と Word の取り込み、AI チャットの回答の修正、PDF の書き出し、書き方の確認を紹介。 / Trilingual 1.0.20 videos, vertical cuts, pages and posts.'),
                            (P / 'integration/CHANGELOG.md', '1.5.0', '日英中の紹介ページの動画・機能一覧と、YouTube revision 6.0.0 を 1.0.20 に更新。 / Pages and YouTube revision 6.0.0 for 1.0.20.')]:
    s = file.read_text(encoding='utf-8'); i = s.index('\n## '); s = s[:i] + f'\n## {version} — {DATE}\n\n- ' + text + '\n' + s[i:]
    s = re.sub(r'(Version: )[^\n]+', r'\g<1>' + version, s, count=1); file.write_text(s, encoding='utf-8')
(K / 'README.md').write_text(header('README.md') + '# mdEditor 1.0.20 / Trilingual release\n\n[公開記録 / Publication](PUBLICATION.md)\n\n' + ''.join(f'- {l}: [投稿文 / Posts](posts-{l}.md) · [画像 / Image](social-{l}.png) · [縦型動画 / Vertical video](mdEditor-vertical-{l}.mp4)\n' for l in LANGS) + f'\nDesign Office Identity 4.20.0, verified {DATE}.\n', encoding='utf-8')
(K / 'PUBLICATION.md').write_text(header('PUBLICATION.md') + f'# 公開記録 / Publication register\n\n{DATE}。ソース 1.0.20・動画 revision 1.0.0（mdEditor-promo-1.0.20）・YouTube revision 6.0.0・資料キット {KIT}。\n\n| 媒体 | 状態 |\n| --- | --- |\n| GitHub Pages | 準備中 |\n| YouTube 日英中 | 非公開アップロード準備中 |\n| Zenn | 準備中 |\n| X・LinkedIn・TikTok | 規約で画面の自動操作が禁止されているため、所有者が手で投稿 |\n| Reddit | アカウント BAN の異議申し立ての結果待ちのため対象外 |\n', encoding='utf-8')
(B / 'copy.json').write_text(json.dumps({'copy': COPY, 'features': FEATURES}, ensure_ascii=False, indent=2), encoding='utf-8')
print('prepared', [p.name for p in sorted(K.iterdir())])
