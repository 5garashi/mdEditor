<!--
  ファイル名 / File: PRIVACY.md
  概要 / Description: Privacy policy for mdEditor (draft for the Microsoft Store listing)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-11
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# mdEditor プライバシーポリシー / Privacy Policy

最終更新 / Last updated: 2026-10-11

[日本語](#日本語) / [English](#english)

## 日本語

mdEditor（以下「本アプリ」）は、5garashi.com設計事務所が提供する Windows 用の Markdown エディタです。本アプリは、利用者の個人情報や文書の内容を収集・送信しません。

### 文書の扱い

- 開いた文書は、利用者の PC の上だけで読み込み、表示し、保存します。文書の内容を5garashi.com設計事務所や第三者に送ることはありません。
- PDF の書き出しも PC の中で行います。
- 自動バックアップ（設定でオフにできます）は、保存していない変更を15秒ごとに `%LOCALAPPDATA%\mdEditor\Recovery` に控えます。控えは保存するか正常に終了すると消え、異常終了したときだけ次の起動で復元に使います。
- 読み上げ（設定でオンにしたときだけ）は、この PC に入っている Windows の音声だけを使います。文章をインターネット上の音声サービスに送ることはありません。

### 保存する設定

本アプリは、次の設定を利用者の PC に保存します。これらが外部に送られることはありません。

- 表示テーマ（自動・ライト・ダーク）、PDF の用紙、表示倍率、自動バックアップのオン・オフ、プレビューの別ウィンドウの位置・大きさ・表示倍率と、別ウィンドウにしていたかどうか: Windows レジストリ `HKEY_CURRENT_USER\Software\mdEditor`
- 表示言語、編集欄とプレビューの幅、キーボードショートカット、設定画面の表示の設定（書体・文字の大きさ・行間・プレビューの本文の幅・編集欄・読み上げ）: 画面表示の部品（Microsoft Edge WebView2）のデータフォルダー `%LOCALAPPDATA%\mdEditor\WebView2`

### 通信が発生する場合

本アプリ自体は、利用状況の計測（テレメトリ）や広告のための通信を行いません。ただし、次の場合には通信が発生します。

- 文書に `https://` などで始まる外部の画像が含まれている場合、プレビューと PDF の表示のために、本アプリはその画像のサーバーへ直接接続します。接続先のサーバーには、通常の Web の閲覧と同じように、利用者の IP アドレスなどが伝わります。
- 文書内のリンクと、「このアプリについて」の会社紹介・サポートなどのリンクは、利用者が選んだときだけ、既定のブラウザーやメールアプリで開きます。その後の通信は、それぞれのアプリの扱いによります。
- Microsoft Store 版では、Windows の仕組みによりアプリが書き込む設定やデータの保存場所が、パッケージ専用の領域に置き換わることがあります。内容と扱いは上記と同じで、外部に送られることはありません。
- 画面表示に使う Microsoft Edge WebView2 Runtime は Microsoft が提供する部品で、その更新や診断データの送信は Microsoft の設定とプライバシーに関する声明に従います。

### お問い合わせ

本ポリシーに関するお問い合わせは、[GitHub の Issues](https://github.com/5garashi/mdEditor/issues) までお寄せください。

### 改定

本ポリシーを改定する場合は、このページを更新し、最終更新日を改めます。

## English

mdEditor (the "app") is a Markdown editor for Windows provided by 5garashi.com Design Office. The app does not collect or transmit personal information or the contents of your documents.

### Your documents

- Documents you open are read, displayed, and saved only on your PC. Their contents are never sent to 5garashi.com Design Office or to any third party.
- PDF export also happens entirely on your PC.
- Automatic backup (can be turned off in Settings) copies unsaved changes every 15 seconds to `%LOCALAPPDATA%\mdEditor\Recovery`. The copy is removed when you save or close normally, and is used only to restore your work after an abnormal exit.
- Read-aloud (only when turned on in Settings) uses only the Windows voices installed on this PC. Text is never sent to an online speech service.

### Settings stored on your PC

The app stores the following settings on your PC. They are never sent anywhere.

- Theme (Auto, Light, Dark), PDF paper size, zoom level, whether automatic backup is on, and the preview window's position, size, zoom, and whether it was open: Windows registry `HKEY_CURRENT_USER\Software\mdEditor`
- Display language, the width of the editor and preview, keyboard shortcuts, and the display options in Settings (typeface, text size, line spacing, preview text width, editor aids, read-aloud): the data folder of the display component (Microsoft Edge WebView2), `%LOCALAPPDATA%\mdEditor\WebView2`

### When network connections happen

The app itself makes no connections for usage tracking (telemetry) or advertising. Connections do happen in these cases:

- If a document contains external images (for example, addresses starting with `https://`), the app connects directly to those servers to show the images in the preview and in PDFs. As with ordinary web browsing, those servers receive information such as your IP address.
- Links in a document, and the company, support, and other links in About, open in your default browser or mail app only when you choose them. What happens after that depends on those apps.
- In the Microsoft Store version, Windows may redirect where the app writes its settings and data to a package-specific area. The contents and handling are the same as described above, and nothing is sent anywhere.
- Microsoft Edge WebView2 Runtime, the display component provided by Microsoft, is updated and may send diagnostic data according to Microsoft's settings and privacy statement.

### Contact

For questions about this policy, please use [GitHub Issues](https://github.com/5garashi/mdEditor/issues).

### Changes

If this policy changes, this page will be updated along with the date above.
