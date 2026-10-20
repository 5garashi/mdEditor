<!--
ファイル名 / File: promotion/integration/tiktok/README.md
概要 / Description: TikTokプロフィールの設定記録 / TikTok profile setup record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.3.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# TikTokプロフィール / TikTok profile

2026-10-20：日英の1.0.14動画を投稿し、どちらも「誰でも」で公開確認。 [今回の公開記録](../../multichannel/update-2026-10-20/PUBLICATION.md)。


2026-10-18：[日本語動画](https://www.tiktok.com/@5garashi_com/video/7697746476302175504)と[英語動画](https://www.tiktok.com/@5garashi_com/video/7697747556910042375)を@5garashi_comから公開。ソース1.0.12、背景除去済みrevision 1.1.1。誰でも、自社宣伝・AIラベルを確認。 [詳細記録](https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/update-2026-10-18/PUBLICATION.md)。

プロフィール: https://www.tiktok.com/@5garashi_com

**以前の公開状態 / Previous publication:** 2026-10-17にソース版1.0.10の[日本語動画](https://www.tiktok.com/@5garashi_com/video/7697326216935640372)と[英語動画](https://www.tiktok.com/@5garashi_com/video/7697324847185136912)を「誰でも」で公開しました。自社宣伝とAI生成のラベルを設定済みです。制作・公開確認は[公開記録](../../video-update-1.0.10/PUBLICATION-2026-10-17.md)を参照してください。Japanese and English source-build 1.0.10 videos are public.

2026-10-14の[以前の動画](https://www.tiktok.com/@5garashi_com/video/7696190069094272263)は保持しています。当時の確認範囲は[PUBLICATION.md](PUBLICATION.md)を参照してください。以下の制作記録の「未投稿」は、公開前の工程時点の記述です。

本人の依頼に基づき、ログイン済みの新規アカウントを以下の内容へ設定し、保存後の画面で反映を確認しました。

| 項目 | 保存内容 |
| --- | --- |
| 元のユーザー名 | user6183740191017 |
| 新しいユーザー名 | 5garashi_com |
| 名前 | 5garashi Design Office |
| 自己紹介 | Electronic design · Software · Digital support / 日本語・English（改行で区切る） |
| プロフィール写真 | ../linkedin/company-logo.png。400×400、公式シンプル版マークをそのまま使用 |

profile.jsonは再設定時の入力原稿です。公式SVGは変更せず、既存のPNGを使用しました。TikTokの円形切り抜きでマークが欠けないことを目視確認しました。

2026-10-14追記: 本人がプロフィール写真を添付のクリーム色背景のマークへ変更しました。その写真を維持します。company-logo.pngには本人の未コミット変更があり、company-logo2.pngも存在するため、この工程では上書き・コミットしません。

次回変更できるときの希望値は `@5garashi.com` / `5garashi.com Design Office` です。planned-profile.jsonに保存しました。変更間隔と、その時点のユーザー名の利用可否を確認してから適用します。現在のプロフィールURLは引き続き上記のアンダースコア入りです。

画面に表示された変更間隔は、ユーザー名30日間に1度、名前7日間に1度。本人が依頼した値で変更を確定しました。

Web版の今回の編集フォームには、独立したWebsite入力欄は表示されていませんでした。アカウントの種別変更や有料機能の契約は行っていません。投稿、フォロー、メッセージ送信も行っていません。

## 縦画面の操作デモ / Vertical demo

`video/mdEditor-demo-vertical-en.mp4` は英語字幕付きの縦動画です。名称は **mdEditor Demo** とし、秒数を名称に含めません。この版の長さは49秒、1080×1920、20fps、音声なしです。字幕は映像に焼き込み、別途demo-en.vttも用意しました。

### オリジナルBGM「Light Steps」 / Original upbeat BGM

本人から公式テーマは重く感じるとの意見を受け、軽快な操作感を連想させる別のオリジナル曲を作曲しました。現在review.htmlに表示する動画は `video/mdEditor-demo-vertical-en-light-steps-v1.mp4` です。

- 5/4拍子、125 BPM。「3＋2」のまとまりを持つリズムと5拍目のアクセント。
- Dメジャー・ペンタトニックの短い旋律、軽いプラック音、短い鍵盤の伴奏、控えめなベースとシェーカー。
- 20小節で48秒、その後1秒の終止と余韻。動画と同じ49秒。
- 公式テーマの音源・MIDI・旋律を使わない新規曲。アップテンポの指定は今回の本人の依頼に基づくもので、公式テーマの改版ではありません。

作曲・音源生成はCodex（依頼: 5garashi.com設計事務所 / 5garashi.com Design Office）。曲名は **Light Steps / 軽快なステップ v1**。音楽の公開ライセンスは未指定です。コードのMITおよび公式ロゴのCC BY-ND 4.0を、新しい音楽に自動で適用しません。

`music/` にWAV原本、MP3試聴用、MIDI編集用、score-v1.jsonの音符・テンポ・制作情報、validation-v1.jsonの検証記録を保存しました。MIDIは合成WAVの音色を内包しないため、再生ソフトにより音色が変わります。シェーカーの音源は合成WAVに含まれ、MIDIには旋律・伴奏・ベースを保存しています。

再制作は `python promotion/integration/tiktok/compose_music.py`。Python、NumPy、ffmpegを使用します。既存のWAVがあれば停止するため、改作は別の版番号で保存してください。動画は映像ストリームをコピーし、無音版との全フレーム一致を検証しています。音楽は自動再生しません。公式テーマ付き版・無音版も比較用に保持し、SNSへは未投稿です。音の印象は試聴して判断してください。TikTok圧縮後の音質・音量は未確認です。

### 以前の公式テーマ付き版 / Previous official-theme version

2026-10-14の本人の依頼に基づき、`video/mdEditor-demo-vertical-en-bgm.mp4` を追加しました。公式テーマ曲 `5garashi_theme.wav`（5/4拍子、96 BPM、Dメジャー・ペンタトニック）を使っています。曲のテンポ・音色・旋律はそのまま、冒頭49秒を使用し、音量を6 dB下げ、開始0.15秒と終了1.5秒にフェードを付けました。公式原本は改変せず作業用フォルダーへ保存し、Identityへの書き込みは行いません。

音源出典: [Design Office Identity 第11条](https://5garashi.github.io/5garashi-identity/#a11)。著作者は5garashi.com設計事務所 / 5garashi.com Design Office、原本ライセンスはCC BY-ND 4.0です。同条の紹介動画・デモBGM用途と、本人のBGM追加依頼に基づく制作です。これは新しい公式テーマの改版ではありません。映像への同期・抜粋・フェードを含むこの処理を、第三者一般への改変許諾として案内しません。

再制作: 無音版を生成した後、`python promotion/integration/tiktok/add_audio.py` を実行します。公式リポジトリのコミットと原本SHA-256を固定し、取得した音源の一致を確認します。動画ストリームはコピーするため、映像・字幕は再圧縮しません。デコードした全フレームのハッシュが無音版と一致し、長さ49秒・ステレオAAC音声・ピークが0 dBTP未満であることを確認します。記録は `video/audio-validation.json`。元の無音版も保持しています。

review.htmlではBGM版を表示します。自動再生はせず、再生・一時停止・音量調整はプレーヤーで操作します。音楽には操作情報を持たせず、字幕だけでも内容が分かります。TikTokへは未投稿で、圧縮後の音量・再生・商用投稿時の音源判定は未確認です。投稿原稿に音楽のクレジットを追加しています。

専用プロファイルで未改変のWindows版ソースビルド1.0.5を操作した、新しい実録画です。Microsoft Storeからインストールしたバイナリの録画ではありません。ユーザーからStore版1.0.5公開の連絡を受けています。

1. 文書を開く。
2. アプリの全体表示ボタンで編集欄を広げ、見出しを変更する。
3. 更新された見出し・KaTeX数式・Mermaid図を表示する。
4. 実際にプレビューの別ウィンドウを開く。
5. Markdownを保存し、ネイティブの保存ダイアログでPDFを保存する。

録画中の別ウィンドウの存在、PDF保存完了の通知、PDF中の編集後の見出しと図を確認しました。video/validation.jsonに操作時刻とエンコード検証を保存しています。PDFは作業用フォルダーwork/tiktok-v3にあり、宣伝用配布には含めません。使用したサンプルは既存の英語版quickstart-engineering-note.en.mdです。最初の2回の収録は描画待ち、ウィンドウの重なり、ダイアログ位置に問題があったため、work/tiktok-v1とv2に保持し、再収録しました。

実画面は664×712で取得し、896×960へ拡大して縦キャンバスの中央に配置しました。アプリ画面の色や表示内容を合成で変更していません。上下の見出し・字幕・案内は編集時に付加しています。録画用の編集は文書のコピーのみ。別ウィンドウの配置等のアプリ設定は終了時に元へ戻します。

再制作は `python promotion/integration/tiktok/record_vertical.py` → `python promotion/integration/tiktok/encode_vertical.py` の順です。前者はWindows、ffmpeg、pywin32、pywinauto、PyMuPDFと、C:\miFfplay\appAutoTesterの補助ライブラリーを使います。既存の録画とPDFがあれば停止するため、再収録にはWORKを新しいフォルダーへ変更してください。アプリ本体や既存の横動画は変更しません。

確認画面はreview.html、投稿原稿はPOST.md。Chromeで動画のデコード・再生を確認しました。確認ページは昼夜・幅320/1280px・文字サイズ100/200%で横にはみ出さないこととロゴの読み込みを検証しました。動画の見出し、別ウィンドウの表示、保存完了通知を抽出フレームでも目視確認しました。これらはWCAG完全準拠やTikTok実機検証を意味しません。

まず動画を確認し、TikTokのアップロード画面で公開範囲・プレビュー・字幕とUIの重なりを確認してから公開します。**この工程ではTikTokへ投稿していません。** TikTok上の圧縮後の読みやすさ・実機再生・画面UIとの重なりは未確認です。PDFは2ページで、既存のサンプルで確認済みの末尾の空白ページの制約も残っています。アプリの修正はこの制作には含めません。

English: The new TikTok profile was configured as @5garashi_com, with the office name, bilingual bio and unchanged official mark. The saved profile was visually verified. No videos, follows or messages were published.

デザイン基準 / Design reference: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。本文・設定JSONはMIT、公式ロゴ部分はCC BY-ND 4.0。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST
