from pathlib import Path
import re,json,shutil
B=Path(__file__).resolve().parent;P=B/'repo/promotion';K=P/'multichannel/update-2026-10-18'
f=P/'integration/build.py';s=f.read_text(encoding='utf-8').replace('1.0.10','1.0.12').replace('2026-10-17','2026-10-18').replace('Identity 4.15.0','Identity 4.20.0').replace('multichannel/update-2026-10-17/','multichannel/update-2026-10-18/').replace("'multichannel/social/read-'+lang+'-landscape.png'","'multichannel/update-2026-10-18/social-'+lang+'.png'")
s=s.replace('QR付き1枚PDFを開く','1枚紹介PDFを開く').replace('Open the one-page PDF','Open the one-page PDF')
s=s.replace("link('https://github.com/5garashi/mdEditor/blob/main/promotion/multichannel/social/posts-'+lang+'.md'","link('https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/update-2026-10-18/posts-'+lang+'.md'")
s=s.replace("body += '<p class=\"note\">'+t('主画面", "body += section(t('主な20機能を読む','Read the twenty main features'),link('multichannel/update-2026-10-18/features-'+lang+'.pdf',t('機能一覧PDFを開く','Open the feature guide PDF')))+ '<p class=\"note\">'+t('主画面")
s=s.replace('mdEditor — 取り込み・編集・書き出しをWindowsで。','mdEditor — 図も、3Dも、動画も。').replace('mdEditor — Import, edit and export on Windows.','mdEditor — Diagrams, 3D and video.')
s=s.replace('ソース1.0.12はWord・CSV・Webを取り込み、Word・PDF・HTML・PNGへ書き出せます。','ソース1.0.12：図・数式・回転する3D・地図・動画を文書の中で。書きながら確認し、PDF・Wordなどで共有。').replace('Source 1.0.12: import Word, CSV and web pages; export Word, PDF, HTML and PNG.','Source 1.0.12: diagrams, math, interactive 3D, maps and video in your document. Edit with a live preview and share PDF or Word.')
s=s.replace('図と数式の技術メモを作り、PDFで渡す','図・3Dから始めて、文書を共有する').replace('Create a technical note and share a PDF','Explore diagrams and 3D, then share a document')
s=s.replace("link('samples/'+name+('.en' if en else '')+'.md',label)","link('samples/'+name+('.en' if en else '')+'.md',label)")
s=s.replace("article = 'technical-note-'+lang+'.md'", "body += section(t('動画と同じサンプルを見る','Try the sample from the video'),link('samples/diagram-test'+('.en' if en else '')+'.md',t('diagram-testを開く','Open diagram-test'))+'<p>'+t('STL・DOT・GeoJSONを含むmdEditor向けサンプルです。ネット上の画像・動画・詳細地図は、許可した場合に読み込みます。','A product sample including STL, DOT and GeoJSON. Internet media and detailed maps load only with permission.')+'</p>')\n        article = 'technical-note-'+lang+'.md'")
f.write_text(s,encoding='utf-8')
# Preserve sample image references without copying unrelated exports.
src=Path('C:/miFfplay/mdEditor/samples')
for name in ['import-test-images','darkMode.png','darkMode-en.png','darkMode-jp.png']:
 f=src/name
 if f.is_dir():shutil.copytree(f,P/'samples'/name,dirs_exist_ok=True)
 elif f.exists():shutil.copy2(f,P/'samples'/name)
header='<!--\nFile: README.md\nDescription: Source 1.0.12 publication materials\nAuthor: 5garashi.com Design Office\nCreated: 2026-10-18\nVersion: 1.8.0\nLicense: MIT\nSPDX-License-Identifier: MIT\n-->\n'
(K/'README.md').write_text(header+'# mdEditor — 図も、3Dも、動画も / Visual-first promotion\n\nVersion: 1.8.0 · [CHANGELOG](../../CHANGELOG.md)\n\n回転する3Dで用途を見せ、主な20機能を紹介する日英資料です。ソース版1.0.12・動画パッケージrevision 1.1.1を使用。\n\n| 素材 | 日本語 | English |\n| --- | --- | --- |\n| 投稿画像 | [PNG](social-ja.png) | [PNG](social-en.png) |\n| 1枚紹介PDF | [PDF](one-page-ja.pdf) | [PDF](one-page-en.pdf) |\n| 20機能のガイド | [PDF](features-ja.pdf) | [PDF](features-en.pdf) |\n| 投稿原稿 | [Markdown](posts-ja.md) | [Markdown](posts-en.md) |\n\n公開URLと確認結果は [PUBLICATION.md](PUBLICATION.md) にまとめます。Store配信版番号、全キー操作、実2台目画面、読み上げ、全形式出力・復元の再試験は未確認。PDFはタグなし。画像・動画・地図は収録時に許可しました。\n\nDesign reference: [Identity 4.20.0](https://5garashi.github.io/5garashi-identity/)。ロゴ CC BY-ND 4.0、文章とコード MIT。サンプル映像・地図・音楽の権利は [素材の出典](../../THIRD-PARTY-NOTICES.md) に従います。\n\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: 2026-10-18 JST\n',encoding='utf-8')
(K/'PUBLICATION.md').write_text(header+'# 公開記録 / Publication record\n\n2026-10-18：公開作業中。公開先ごとの確認後に更新します。\n',encoding='utf-8')
for n in ['README.md','CHANGELOG.md','PROMOTION-SYSTEM.md']:
 f=P/n;s=f.read_text(encoding='utf-8').replace('Version: 1.7.1','Version: 1.8.0')
 if n=='README.md':
  i=s.index('2026-10-18 改訂：');e=s.index('\n\n',i);s=s[:i]+'2026-10-18：[日英の紹介画像・PDF・20機能ガイド・投稿原稿](multichannel/update-2026-10-18/README.md)と紹介・資料ページを、ソース1.0.12の3Dから始まる構成へ更新。動画は背景の映り込みを除いたrevision 1.1.1です。[公開記録](multichannel/update-2026-10-18/PUBLICATION.md)に各媒体の確認結果をまとめます。'+s[e:]
 if n=='CHANGELOG.md':
  i=s.index('## 1.7.1');s=s[:i]+'## 1.8.0 — 2026-10-18\n\n- 日英の紹介・資料・制作事例・埋め込みカード、画像、PDF、20機能ガイド、サンプル、記事・投稿原稿をソース1.0.12へ更新。YouTube動画revision 3.0.0として背景除去済み動画を同期し、各媒体へ公開する工程と結果を記録。\n- Update bilingual visual-first materials and publish the corrected source 1.0.12 showcase across channels.\n\n'+s[i:]
 f.write_text(s,encoding='utf-8')
f=P/'integration/README.md';s=f.read_text(encoding='utf-8');s=s.replace('Version: 1.1.0','Version: 1.2.0').replace('Version: 1.1.1','Version: 1.2.0');i=s.index('2026-10-17:');s=s[:i]+'2026-10-18：[1.0.12の日英資料と公開記録](../multichannel/update-2026-10-18/README.md)。3Dから始まる紹介ページ、20機能ガイド、資料・制作事例・埋め込みカードを更新。\n\n'+s[i:];f.write_text(s,encoding='utf-8')
(P/'integration/CHANGELOG.md').write_text('# Changelog\n\n## 1.2.0 — 2026-10-18\n\n- Source 1.0.12: visual-first landing, resource links, twenty-feature PDFs and office-site cards.\n\n'+(P/'integration/CHANGELOG.md').read_text(encoding='utf-8'),encoding='utf-8')
print('Prepared page generator and publication records')
