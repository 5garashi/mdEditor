"""ファイル名 / File: prepare.py
概要 / Description: mdEditor 1.0.22 の日英中の公開素材を用意する（紹介ページ、YouTube revision 7.0.0、投稿文、短い動画、縦型動画）
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-11-07
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json, re, shutil, subprocess
from PIL import Image, ImageDraw, ImageFont
from bs4 import BeautifulSoup

B = Path(__file__).resolve().parent; ROOT = B.parents[1]
R = ROOT / 'work/publish-2026-10-23/repo'; P = R / 'promotion'; K = P / 'multichannel/update-2026-11-07'; K.mkdir(parents=True, exist_ok=True)
V = ROOT / 'deliverables/mdEditor-promo-1.0.22-revision-1.1.0'; T = ROOT / 'local/promo-1.0.22-claude'
TAKE = {'ja': 'take2', 'en': 'take-en', 'zh-CN': 'take-zh-CN'}
MUSIC = T / '5garashi_light_steps.mp3'
BASE = 'https://5garashi.github.io/mdEditor/'; DATE = '2026-11-07'; KIT = '1.15.0'; REV = '7.0.0'
LANGS = ['ja', 'en', 'zh-CN']
LANDING = {'ja': '', 'en': 'en.html', 'zh-CN': 'zh-CN.html'}
FONT = {'ja': 'C:/Windows/Fonts/YuGothB.ttc', 'en': 'C:/Windows/Fonts/segoeuib.ttf', 'zh-CN': 'C:/Windows/Fonts/msyhbd.ttc'}
GREEN = '#385723'; PAPER = '#F9EFCB'; HEAD = '#FFE699'; VOLT = '#B92613'; STEEL = '#7B4A21'
ZERO = 'https://5garashi.github.io/zeroPhase/'

COPY = {
 'ja': {
  'title': 'mdEditor 1.0.22 — AIが書く数式も、図も3Dも、Markdownで',
  'new': 'AIのチャットが書く数式の表示を強化しました。$$ の行から書き始めた複数行の数式、閉じる $ の前に空白がある $x $、HTMLの中に文字で書いた数式も、数式として表示します。Markdownの中に書いたSVGの図も表示し、PDF・Wordにも入れます。',
  'strong': 'PDFとWordをMarkdownとして開き、直したらPDFやWordで渡せます。Mermaid・Graphviz・数式・3Dモデル・地図をオフラインで表示。広告なし・計測なし・アカウント不要です。',
  'intro': '3Dモデルの拡大・回転から始まり、英語の論文の数式を、TeXの原文とプレビューを並べてスクロール。そのあと、主な機能（key-features.md）と図のサンプル（diagram-test.md）を上から下へ紹介します。',
  'limits': 'Windows用・無料。AIのチャットの回答は、できる範囲で再現します（すべての書き方を直せるわけではありません）。PDFの取り込みもできる範囲での変換で、段組みやスキャンしたPDFは元に戻りません。収録は、Microsoft Storeに出した1.0.22と同じコミットからのビルドです。',
  'ai': '開発と動画の制作に、AI（Claude・Codex）の支援を受けています。BGMはCodexで作曲・音源生成したものです。',
  'lines': ['AIが書く数式の表示を強化', 'PDFとWordを、Markdownとして開く', '図・数式・3D・地図をオフラインで', '広告なし・計測なし・アカウント不要'],
  'head': 'AIが書く数式も、Markdownで。',
  'store': 'Microsoft Storeで無料',
  'outline': ['3Dモデルを拡大して回す。', 'zerothOrderPhase.en.md の数式を、TeXの原文とプレビューを並べてスクロールする。', 'key-features.md を上から下へ。', 'diagram-test.md を上から下へ。'],
  'section': '今回のアップデート（1.0.22）',
  'note': 'Microsoft Store版1.0.22と同じコミットからビルドした実画面です。',
  'credits': 'BGM：「Light Steps」5garashi.com設計事務所（作曲・音源生成：Codex）、CC BY 4.0。抜粋してくり返し、最初と最後をフェード。\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\n数式の文書：zeroPhase「Observation Limits and the Undifferentiated Origin」5garashi.com設計事務所\n' + ZERO + '\nサンプルの映画：「Big Buck Bunny」© 2008 Blender Foundation、CC BY 3.0。サンプル文書の中で再生（編集なし）。\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\n地図データ：© OpenStreetMap contributors（ODbL）https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #KaTeX',
  'reply': 'AIのチャットが書く数式の表示を強化。PDFとWordをMarkdownとして開き、図・数式・3Dをオフラインで表示します。Windows用・無料。',
  'disclose': 'AI（Claude・Codex）の支援で開発。',
  'music': 'BGM：Light Steps — 5garashi.com設計事務所（Codex）CC BY 4.0',
 },
 'en': {
  'title': 'mdEditor 1.0.22 — Math from AI Chats, Diagrams and 3D in Markdown',
  'new': 'Better display of the math that AI chats write. Math spread over several lines that starts on the $$ line, $x $ with a space before the closing $, and math written as text inside HTML now show as math. SVG drawings written in Markdown are shown too, and go into PDF and Word.',
  'strong': 'Open PDF and Word files as Markdown, fix them, and share them as PDF or Word. Mermaid, Graphviz, math, 3D models and maps, offline. No ads, no tracking, no account.',
  'intro': 'Starts by zooming and turning a 3D model, then scrolls through the math of an English paper with the TeX beside the preview, followed by a top-to-bottom tour of the key features (key-features.en.md) and the diagram samples (diagram-test.en.md).',
  'limits': 'Windows only, free. Markdown from AI chats is reproduced as closely as possible; not every way of writing can be fixed. PDF import is best effort too: multi-column layouts and scanned PDFs do not come back. Recorded from a build of the same commit as the 1.0.22 release on Microsoft Store.',
  'ai': 'Developed and produced with AI assistance (Claude, Codex). The music was composed and generated with Codex.',
  'lines': ['Better math from AI chats', 'Open PDF and Word files as Markdown', 'Diagrams, math, 3D and maps, offline', 'No ads. No tracking. No account.'],
  'head': 'Math from AI chats, in Markdown.',
  'store': 'Free on Microsoft Store',
  'outline': ['Zoom and turn a 3D model.', 'Scroll through the math in zerothOrderPhase.en.md, with the TeX beside the preview.', 'Tour key-features.en.md from top to bottom.', 'Tour diagram-test.en.md from top to bottom.'],
  'section': 'New in this update (1.0.22)',
  'note': 'The actual app screen, built from the same commit as the 1.0.22 release on Microsoft Store.',
  'credits': 'Music: "Light Steps" by 5garashi.com Design Office (composed and generated with Codex), CC BY 4.0. Changes: excerpt, looped, faded in and out.\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\nMath document: zeroPhase, "Observation Limits and the Undifferentiated Origin", 5garashi.com Design Office\n' + ZERO + '\nSample film: "Big Buck Bunny" © 2008 Blender Foundation, CC BY 3.0. Shown as it plays inside a sample document; not edited.\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\nMap data: © OpenStreetMap contributors (ODbL) https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #KaTeX',
  'reply': 'Better display of the math AI chats write. Opens PDF and Word as Markdown; diagrams, math and 3D offline. Windows, free.',
  'disclose': 'Built with AI assistance (Claude, Codex).',
  'music': 'Music: Light Steps — 5garashi.com Design Office (Codex), CC BY 4.0',
 },
 'zh-CN': {
  'title': 'mdEditor 1.0.22 — AI 写的公式、图表和 3D，都用 Markdown 显示',
  'new': '加强了 AI 聊天所写公式的显示。从 $$ 行开始的多行公式、结束 $ 前有空格的 $x $，以及 HTML 中以文字写成的公式，都会显示为公式。Markdown 中写的 SVG 图形也会显示，并可放入 PDF 和 Word。',
  'strong': '把 PDF 和 Word 作为 Markdown 打开，修改后导出为 PDF 或 Word。离线显示 Mermaid、Graphviz、公式、3D 模型和地图。无广告、无跟踪、无需账户。',
  'intro': '从放大和旋转 3D 模型开始，然后并排显示英文论文的 TeX 原文和预览，同步滚动浏览公式，接着从上到下浏览主要功能（key-features.zh-CN.md）和图表示例（diagram-test.zh-CN.md）。',
  'limits': '仅支持 Windows，免费。AI 聊天中的 Markdown 会尽量还原，并非所有写法都能修正。PDF 导入也会尽量转换，分栏排版和扫描 PDF 无法还原。录制所用的版本，由与 Microsoft Store 上发布的 1.0.22 相同的提交构建。',
  'ai': '开发和视频制作使用了 AI 辅助（Claude、Codex）。背景音乐由 Codex 作曲并生成音频。',
  'lines': ['加强 AI 所写公式的显示', '把 PDF 和 Word 作为 Markdown 打开', '离线显示图表、公式、3D 和地图', '无广告、无跟踪、无需账户'],
  'head': 'AI 写的公式，也用 Markdown 显示。',
  'store': '在 Microsoft Store 免费获取',
  'outline': ['放大并旋转 3D 模型。', '并排显示 TeX 原文和预览，滚动浏览 zerothOrderPhase.en.md 中的公式。', '从上到下浏览 key-features.zh-CN.md。', '从上到下浏览 diagram-test.zh-CN.md。'],
  'section': '本次更新（1.0.22）',
  'note': '实际界面，由与 Microsoft Store 上发布的 1.0.22 相同的提交构建。',
  'credits': '背景音乐：“Light Steps”，5garashi.com Design Office（作曲与音频生成：Codex），CC BY 4.0。节选、循环播放，开头和结尾淡入淡出。\nhttps://5garashi.github.io/5garashi-identity/ / https://creativecommons.org/licenses/by/4.0/\n公式文档：zeroPhase“Observation Limits and the Undifferentiated Origin”，5garashi.com Design Office\n' + ZERO + '\n示例影片：“Big Buck Bunny” © 2008 Blender Foundation，CC BY 3.0。在示例文档中播放，未经编辑。\nhttps://peach.blender.org/ / https://creativecommons.org/licenses/by/3.0/\n地图数据：© OpenStreetMap contributors（ODbL）https://www.openstreetmap.org/copyright',
  'tags': '#mdEditor #Markdown #Windows #KaTeX',
  'reply': '加强了 AI 聊天所写公式的显示。把 PDF 和 Word 作为 Markdown 打开，离线显示图表、公式和 3D。Windows 版，免费。',
  'disclose': '开发使用了 AI 辅助（Claude、Codex）。',
  'music': '背景音乐：Light Steps — 5garashi.com Design Office（Codex）CC BY 4.0',
 },
}
# Twenty features, from store/STORE-LISTING.md of 1.0.22.
STORE = (Path(r'C:\miFfplay\mdEditor\store\STORE-LISTING.md')).read_text(encoding='utf-8')
def bullets(heading):
    part = STORE.replace('\r\n', '\n').split(heading, 1)[1].strip('\n').split('\n\n', 1)[0]
    return [x[2:].strip() for x in part.splitlines() if x.startswith('- ')]
FEATURES = {'en': bullets('**Features (bullets):**'), 'ja': bullets('**製品の機能（箇条書き）:**'), 'zh-CN': bullets('**产品功能（要点）:**')}
assert all(len(v) == 20 for v in FEATURES.values()), {k: len(v) for k, v in FEATURES.items()}

def duration(path):
    return float(subprocess.check_output(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'csv=p=0', str(path)]).strip())
def events(lang): return {e['key']: e['seconds'] for e in json.loads((V / f'validation-{lang}.json').read_text(encoding='utf-8'))['events']}
def chapters(lang): return (V / f'chapters-{lang}.txt').read_text(encoding='utf-8').strip().splitlines()
def font(lang, size): return ImageFont.truetype(FONT[lang], size)
def frame(lang, seconds, out):
    # A frame of the cut recording, without captions.
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-ss', str(seconds), '-i', str(T / TAKE[lang] / 'clean.mp4'), '-frames:v', '1', str(out)], check=True)
    return Image.open(out).convert('RGB')
def weight(t):
    t = re.sub(r'https?://\S+', 'x' * 23, t)
    return sum(1 if ord(c) < 0x1100 or 0x2000 <= ord(c) <= 0x201F else 2 for c in t)

def images(lang):
    c = COPY[lang]; url = BASE + LANDING[lang]; e = events(lang)
    model = frame(lang, e['3d'] + 3, B / f'model-{lang}.png'); math = frame(lang, e['math-2.'] + 2, B / f'math-{lang}.png')
    model.save(P / f'assets/hero-3d-1.0.22-{lang}.png'); shutil.copyfile(V / f'poster-{lang}.png', P / f'assets/video-poster-1.0.22-{lang}.png')
    # YouTube thumbnail, 1280x720.
    t = Image.new('RGB', (1280, 720), PAPER); d = ImageDraw.Draw(t); d.rectangle((0, 0, 1280, 96), fill=HEAD); d.line((0, 96, 1280, 96), fill=GREEN, width=3)
    d.text((36, 18), 'mdEditor 1.0.22', font=font(lang, 48), fill=GREEN)
    shot = math.copy(); shot.thumbnail((1180, 530)); t.paste(shot, ((1280 - shot.width) // 2, 108))
    d.text((640, 680), c['head'], font=font(lang, 48), fill=VOLT, anchor='mm'); t.save(P / f'assets/youtube-1.0.22-{lang}.png')
    # Social image, 1600x1100.
    s = Image.new('RGB', (1600, 1100), PAPER); d = ImageDraw.Draw(s); d.rectangle((0, 0, 1600, 140), fill=HEAD)
    d.text((45, 30), 'mdEditor 1.0.22', font=font(lang, 60), fill=GREEN); d.text((45, 168), c['head'], font=font(lang, 50), fill=VOLT)
    for i, shot in enumerate([math, model]):
        shot = shot.copy(); shot.thumbnail((740, 400)); s.paste(shot, (45 + i * 770, 270))
    for i, line in enumerate(c['lines']): d.text((45, 710 + i * 62), line, font=font(lang, 34), fill=GREEN)
    d.text((45, 975), url, font=font(lang, 28), fill=GREEN); d.text((45, 1035), f'mdEditor 1.0.22 | 5garashi.com Design Office | {DATE}', font=font(lang, 22), fill=STEEL)
    s.save(K / f'social-{lang}.png')

def short(lang):
    # For X (2:20 at most on a free account): the opening, the math and key-features, then the closing card; the music is laid again.
    e = events(lang); full = V / f'mdEditor-promo-{lang}.mp4'; end = duration(full); a, b = e['dt'], e['outro']; length = a + (end - b)
    assert length <= 140, length
    graph = (f'[0:v]trim=0:{a},setpts=PTS-STARTPTS[v1];[0:v]trim={b}:{end},setpts=PTS-STARTPTS[v2];[v1][v2]concat=n=2:v=1:a=0[v];'
             f'[1:a]atrim=0.1:48.1,asetpts=PTS-STARTPTS,aloop=loop=-1:size=2116800,atrim=0:{length},afade=t=in:d=0.3,afade=t=out:st={length - 3}:d=3[a]')
    out = K / f'mdEditor-short-{lang}.mp4'
    subprocess.run(['ffmpeg', '-y', '-v', 'error', '-i', str(full), '-i', str(MUSIC), '-filter_complex', graph, '-map', '[v]', '-map', '[a]', '-c:v', 'libx264', '-preset', 'medium', '-crf', '20',
                    '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '192k', '-movflags', '+faststart', str(out)], check=True)
    return out

def vertical(lang, src):
    # 1080x1920 for TikTok: the title burned in at the top (text cannot sit above a TikTok video), the app enlarged, the landscape video with its captions, then the Store line.
    c = COPY[lang]; bg = Image.new('RGB', (1080, 1920), PAPER); d = ImageDraw.Draw(bg)
    d.rectangle((0, 0, 1080, 190), fill=HEAD); d.line((0, 190, 1080, 190), fill=GREEN, width=4)
    d.text((40, 30), 'mdEditor 1.0.22', font=font(lang, 56), fill=GREEN); d.text((40, 112), c['head'], font=font(lang, 42), fill=VOLT)
    d.text((40, 1700), c['store'], font=font(lang, 48), fill=GREEN); d.text((40, 1790), BASE + LANDING[lang], font=font(lang, 30), fill=GREEN)
    d.text((40, 1850), c['music'], font=font(lang, 22), fill=STEEL)
    f = B / f'vertical-{lang}.png'; bg.save(f); out = K / f'mdEditor-vertical-{lang}.mp4'
    graph = ('[0:v]split=2[full][detail];[detail]crop=900:560:190:12,scale=1080:-2[zoom];[full]scale=1080:-2[small];'
             '[1:v][zoom]overlay=0:230[tmp];[tmp][small]overlay=0:1000,format=yuv420p[v]')
    subprocess.run(['ffmpeg', '-y', '-v', 'error', '-i', str(src), '-loop', '1', '-framerate', '30', '-i', str(f), '-filter_complex', graph, '-map', '[v]', '-map', '0:a', '-shortest', '-r', '30',
                    '-c:v', 'libx264', '-preset', 'medium', '-crf', '21', '-c:a', 'copy', '-movflags', '+faststart', str(out)], check=True)
    f.unlink()

def texts(lang):
    c = COPY[lang]; url = BASE + LANDING[lang]
    desc = '\n\n'.join([c['new'], c['strong'], c['intro'], '\n'.join(chapters(lang)), '\n'.join('- ' + f for f in FEATURES[lang]), url, c['limits'], c['ai'], c['credits']])
    (P / f'integration/youtube/automation/description-{lang}.txt').write_text(desc, encoding='utf-8')
    post = '\n\n'.join([c['title'], c['new'], c['intro'], url, c['limits'], c['ai'], c['credits'], c['tags']])
    (K / f'post-{lang}.txt').write_text(post, encoding='utf-8')
    # X thread: the first post is the title with the short video; the reply carries the text.
    reply = '\n'.join([c['reply'], url, c['disclose'], c['music'], '#mdEditor #Markdown'])
    for name, text in [('x-thread-1', c['title']), ('x-thread-2', reply)]:
        assert weight(text) <= 280, (lang, name, weight(text)); (K / f'{name}-{lang}.txt').write_text(text, encoding='utf-8')
    # TikTok: the title on the first line, then what the video shows, the link, the disclosure, the credit and the tags.
    tiktok = '\n'.join([c['title'], c['reply'], url, c['disclose'], c['music'], c['tags']])
    (K / f'tiktok-{lang}.txt').write_text(tiktok, encoding='utf-8')

def article(lang, yt):
    # LinkedIn article: headline, the YouTube video (embedded with the toolbar's Add embed), then the text.
    c = COPY[lang]; sub = {'ja': ('今回の更新', '主な機能', '収録と制限', 'クレジット'), 'en': ('What is new', 'Key features', 'Recording and limits', 'Credits'), 'zh-CN': ('本次更新', '主要功能', '录制与限制', '致谢')}[lang]
    body = '\n\n'.join([f'https://www.youtube.com/watch?v={yt}', c['intro'], sub[0], c['new'], c['strong'], sub[1], '\n'.join('• ' + f for f in FEATURES[lang]),
                        sub[2], c['limits'], c['ai'], BASE + LANDING[lang], sub[3], c['credits']])
    (K / f'linkedin-article-{lang}.txt').write_text(f'{c["title"]}\n\n{body}\n', encoding='utf-8')

def page(lang, name):
    c = COPY[lang]; f = P / name; s = f.read_text(encoding='utf-8')
    for a, b in [('mdEditor-demo-1.0.20', 'mdEditor-demo-1.0.22'), ('demo-1.0.20', 'demo-1.0.22'), ('hero-3d-1.0.20', 'hero-3d-1.0.22'), ('video-poster-1.0.20', 'video-poster-1.0.22'),
                 ('Windows用mdEditor 1.0.20の実演と20機能。PDFもAIチャットの回答もMarkdownで。', 'Windows用mdEditor 1.0.22の実演と20機能。AIが書く数式も、図も3Dも、Markdownで。'),
                 ('Open PDFs and AI chat answers as Markdown. Explore mdEditor 1.0.20 for Windows.', 'Math from AI chats, diagrams and 3D in Markdown. Explore mdEditor 1.0.22 for Windows.'),
                 ('ソース版1.0.20の実画面です。', c['note']), ('Actual source build 1.0.20.', c['note']), ('The actual app, built from the commit released on Microsoft Store as 1.0.22.', c['note'])]:
        s = s.replace(a, b)
    soup = BeautifulSoup(s, 'html.parser')
    old = soup.find(id='new-1-0-20') or soup.find(id='new-1-0-22'); new = soup.new_tag('section', id='new-1-0-22'); h = soup.new_tag('h2'); h.string = c['section']; new.append(h)
    for t in [c['new'], c['strong']]:
        p = soup.new_tag('p'); p.string = t; new.append(p)
    p = soup.new_tag('p', attrs={'class': 'note'}); p.string = c['limits'] + ' ' + c['ai']; new.append(p)
    old.replace_with(new)
    lst = soup.select_one('#feature-list ol') or next(s2.find('ol') for s2 in soup.find_all('section') if s2.find('h2') and s2.find('h2').get_text().startswith('20'))
    lst.clear()
    for x in FEATURES[lang]:
        li = soup.new_tag('li'); li.string = x; lst.append(li)
    for img in soup.select('img[src*="hero-3d-1.0.22"]'):
        img['alt'] = {'ja': 'mdEditorの実画面。プレビューに、赤い台の上の緑の星形の3Dモデルを拡大して表示。', 'en': 'mdEditor screen: a green star-shaped 3D model on a red base, zoomed in the preview.', 'zh-CN': 'mdEditor 实际界面：预览中放大显示红色底座上的绿色星形 3D 模型。'}[lang]
        img['width'], img['height'] = '1264', '584'
    secs = round(duration(V / f'mdEditor-promo-{lang}.mp4'))
    for o in soup.select('output.player-clock'): o.string = f'00:00 / {secs // 60:02}:{secs % 60:02}'
    for r in soup.select('input[data-seek]'): r['max'] = str(secs)
    det = soup.select_one('section.demo details ol')
    if det:
        det.clear()
        for x in c['outline']:
            li = soup.new_tag('li'); li.string = x; det.append(li)
    out = redate(str(soup))
    assert '1.0.20' not in out, (name, [m.start() for m in re.finditer(r'1\.0\.20', out)])
    f.write_text(out, encoding='utf-8')

def redate(s):
    # The kit version and the date of the page; a date inside a folder name (update-2026-11-01/...) is a path and stays.
    return re.sub(r'(?<!update-)2026-11-01', DATE, s.replace('1.14.0', KIT))
def links(name):
    # Every local link on the page must point to a file in promotion/.
    s = (P / name).read_text(encoding='utf-8')
    missing = [u for u in re.findall(r'(?:src|href)="([^"#:?]+)"', s) if not (P / u).exists()]
    assert not missing, (name, missing)

def header(name, what):
    return f'<!--\nファイル名 / File: {name}\n概要 / Description: {what}\n作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n作成日 / Created: {DATE}\n版 / Version: {KIT}\nライセンス / License: MIT\nSPDX-License-Identifier: MIT\n-->\n'

if __name__ == '__main__':
    old = json.loads((ROOT / 'work/publish-2026-11-01/copy.json').read_text(encoding='utf-8'))['copy']['zh-CN']
    for lang in LANGS:
        shutil.copyfile(V / f'mdEditor-promo-{lang}.mp4', P / 'assets' / f'mdEditor-demo-1.0.22-{lang}.mp4')
        shutil.copyfile(V / f'mdEditor-promo-{lang}.vtt', P / 'assets' / f'demo-1.0.22-{lang}.vtt')
        images(lang); vertical(lang, short(lang)); texts(lang)
    for lang, name in [('ja', 'index.html'), ('en', 'en.html'), ('zh-CN', 'zh-CN.html')]:
        page(lang, name)
    f = P / 'resources-zh-CN.html'; s = f.read_text(encoding='utf-8')
    s = re.sub(r'仅支持 Windows，免费。[^<]*', lambda m: COPY['zh-CN']['limits'], s, count=1).replace('mdEditor-demo-1.0.20', 'mdEditor-demo-1.0.22').replace('demo-1.0.20', 'demo-1.0.22')
    s = redate(s); assert '1.0.20' not in s; f.write_text(s, encoding='utf-8')
    for name in ['index.html', 'en.html', 'zh-CN.html', 'resources-zh-CN.html']: links(name)
    # YouTube: revision 7.0.0 goes up as new non-public uploads first (sync); making them public is a separate, later push.
    A = P / 'integration/youtube/automation'; cfg = json.loads((A / 'release.json').read_text(encoding='utf-8'))
    cfg.update(operation='sync', request_id=f'{DATE}-store-1.0.22-{REV}-sync')
    for v in cfg['videos']:
        l = v['language']
        v.update(revision=REV, media=f'promotion/assets/mdEditor-demo-1.0.22-{l}.mp4', captions=f'promotion/assets/demo-1.0.22-{l}.vtt', thumbnail=f'promotion/assets/youtube-1.0.22-{l}.png',
                 title=COPY[l]['title'], target_privacy='public', tags=['Markdown', 'Windows', 'KaTeX', 'Mermaid', 'AI', 'mdEditor'])
    (A / 'release.json').write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    (B / 'copy.json').write_text(json.dumps({'copy': COPY, 'features': FEATURES}, ensure_ascii=False, indent=2), encoding='utf-8')
    for name in ['prepare.py']: shutil.copyfile(B / name, K / name)
    for lang in LANGS:
        print(lang, 'x', [weight((K / f'x-thread-{i}-{lang}.txt').read_text(encoding='utf-8')) for i in (1, 2)], 'short', round(duration(K / f'mdEditor-short-{lang}.mp4'), 1), 'vertical', round(duration(K / f'mdEditor-vertical-{lang}.mp4'), 1))
