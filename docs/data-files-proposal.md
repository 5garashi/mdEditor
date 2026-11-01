<!--
  ファイル名 / File: docs/data-files-proposal.md
  概要 / Description: 一覧や初期値をデータのファイルに出し、変更量とテストを減らす提案 / Proposal to move lists and defaults into data files to shrink changes and test runs
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-23
  版 / Version: 0.1.0
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 一覧や初期値をデータのファイルに出す提案

- 状態: 検討用の提案。実装は、5garashi.com から依頼があるまで保留する。
- 版: 0.1.0（2026-10-23）
- 対象: mdEditor 1.0.15。取りかかるのは 1.0.15 を出したあとの版を想定する。
- 関連: [表示言語の構成の提案](localization-structure-proposal.md)（言葉・「書き方」の例・サンプルの一覧・ボタンの表示名と読み）。本書は、言葉以外の一覧と初期値を扱う。

## 提案の要点

プログラムの中に直接書いてある一覧や初期値を、データのファイルに出す。よくある変更（初期値を変える、キーを変える、扱う拡張子を足す）が、プログラムを直さずにデータのファイルだけで済むようにする。

合わせて、appAutoTester のテストの範囲の決め方に、データのファイルごとに流すテストを決めた行を足す。データのファイルを置いただけでは、今の規則では `wwwroot` の変更として全件になるため、ルール側の対応がないとテストは減らない。

目的は、変更の量と、変更ごとに流すテストの量を減らすことである。動きそのもの（変換や保存の手順）をデータで決める作りにはしない。かえって複雑になるためである。

## 1. 候補

効果の大きい順に並べる。

| 候補 | 今の置き場所 | データにした場合 | 効果 |
| :--- | :--- | :--- | :--- |
| 1. 設定の初期値 | `wwwroot/app.js` の `displayDefaults`（書体・文字の大きさ・行間・読み上げ・地図・動画など 14 項目）、`MainWindow.xaml.cs` の `DefaultZoom`（90%）と倍率の段階 `ZoomSteps`、自動バックアップの間隔（15 秒）、ページの上限（`MaxPageBytes`、10 MB） | 1.0.15 の「倍率の初期値を 90% に」は、データ 1 行の変更で済んだ | 大 |
| 2. キーボードショートカットの初期値 | `wwwroot/app.js` に 30 件。1 件に操作の名前・キー・ボタン・処理が並ぶ | 操作の名前とキーの初期値だけをデータに出す。処理はコードに残す | 中 |
| 3. 扱うファイルの種類（拡張子） | `MainWindow.xaml.cs` の数か所（`IsDocumentPath`、`OpenFilter`、`ImageExtensions`、`VideoContentTypes` など）、`setup.ps1` の `$fileTypes`（9 種類）、`store/AppxManifest.template.xml` の `FileType`（18 件）、README | 同じ一覧を 4〜5 か所に重ねて書いている。1 つのファイルにまとめ、それぞれがそこから読む。マニフェストの部分は `build-msix.ps1` が作る | 中（食い違いの防止が主） |
| 4. 図とコードの言語名 | `wwwroot/app.js` の Graphviz の名前（`dot`・`graphviz`・`gv`）、3D（`stl`）・地図（`geojson`・`topojson`）の名前、`wwwroot/index.html` で読み込むコードの色分けの言語（PowerShell・Dockerfile） | 名前の追加がデータだけで済む | 小 |
| 5. リンク先の URL | `wwwroot/index.html` の「このアプリについて」のリンク（会社・サポート・プライバシー） | URL の変更がデータだけで済む | 小（めったに変えない） |

### データに出さないもの

- **HTML の許可リスト**（文書の中で使ってよいタグと属性。`MainWindow.xaml.cs` の HtmlSanitizer の設定）: データにはできるが、安全のための決まりである。変えたら全件を流すべきなので、テストを軽くする対象にしない。
- **処理そのもの**（ファイルの変換、保存、書き出しの手順）: データで動きを決めると、かえって複雑になる。
- **ショートカットの処理**（2 の「処理」の部分）: キーを押したときに何をするかは、コードに残す。

## 2. 置き場所と形の例

```
wwwroot/data/
  defaults.json     設定の初期値（画面側と C# 側の両方が読む）
  shortcuts.json    ショートカットの初期値（操作の名前とキー）
  filetypes.json    扱うファイルの種類（C#、setup.ps1、build-msix.ps1 が読む）
  fences.json       図とコードの言語名
  links.json        「このアプリについて」のリンク先
```

`defaults.json` の例:

```json
{
  "display": { "font": "standard", "size": "1", "spacing": "1.7", "pause": "0.5", "mapBackground": "ask", "videoAutoplay": "auto" },
  "zoom": { "default": 0.9, "steps": [0.5, 0.67, 0.75, 0.8, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3] },
  "backupSeconds": 15,
  "maxPageMegabytes": 10
}
```

`filetypes.json` の例:

```json
[
  { "id": "markdown", "name": "Markdown document", "extensions": [".md", ".markdown"], "opens": "document" },
  { "id": "image", "name": "Image", "extensions": [".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"], "opens": "media" }
]
```

形は例であり、項目の名前と値はまだ決めていない。言語ファイル（[表示言語の構成の提案](localization-structure-proposal.md)の `wwwroot/lang/`）と同じく `wwwroot` の中に置き、アプリと一緒に配る。

## 3. テストの範囲の決め方への追加案

データのファイルごとに、流すテストを決める。サンプルの行と同じ考え方である。

| データのファイル | 流すテスト（例） |
| :--- | :--- |
| 言葉の辞書・ボタンの表示名と読み（表示言語の構成の提案） | `test_startup.py` |
| `defaults.json` | `test_startup.py` と `test_settings.py` |
| `shortcuts.json` | `test_ui.py` と `test_settings.py` |
| `filetypes.json` | `test_files.py`（`setup.ps1` と Store のマニフェストも変わるので、Store に出す前は全件） |
| `fences.json` | `run.cmd mdeditor -k "sample or diagram_test"` |
| `links.json` | `test_startup.py` |

合わせて、データの書き方の誤りを見つける短い確認を 1 つ足す。たとえば次のことを確かめる。

- JSON として読めること、必要な項目がそろっていること。
- ショートカットのキーが重なっていないこと。
- 拡張子が 2 つの種類に重なっていないこと。
- 初期値が、選べる値の中にあること（例: 倍率の初期値が `steps` に含まれる）。

データのファイルを足す変更や、データを読む処理を変える変更は、今までどおり全件とする。規則の改訂は 5garashi.com が判断する。

## 4. 進め方の案

効果が大きく、手間が小さい順に、1 つずつ段階に分ける。どの段階でも、画面の見た目と動きは変えない。

1. **設定の初期値**（`defaults.json`）と、書き方の確認。
2. **ショートカットの初期値**（`shortcuts.json`）。
3. **扱うファイルの種類**（`filetypes.json`）。`setup.ps1` と `build-msix.ps1` も読むようにする。
4. **図とコードの言語名**と**リンク先**（`fences.json`、`links.json`）。必要になったときでよい。

段階ごとにデータを読む処理を足すので、その段階では全件を流す。表示言語の構成の提案と同時に進める場合は、言語ファイルを読む仕組みとデータのファイルを読む仕組みを共通にする。

## English summary

This is a proposal; implementation waits for a request from 5garashi.com. It complements the localization structure proposal (text, Syntax examples, samples, and button names) and covers other lists and defaults that are written directly in the code.

Candidates, most useful first:

1. setting defaults (`displayDefaults` in app.js, the default zoom and zoom steps, the backup interval, the page size limit in C#); the 1.0.15 change of the default zoom to 90% would have been one data line;
2. the default keys of the 30 keyboard shortcuts (the actions stay in code);
3. the supported file types, now repeated in C#, `setup.ps1`, the Store manifest, and the README;
4. the fence names for diagrams and code colors;
5. the About link addresses.

Not moved: the HTML allowlist (a safety rule; changing it should run the full suite), the conversion and saving logic, and what each shortcut does.

Data would live in `wwwroot/data/` (`defaults.json`, `shortcuts.json`, `filetypes.json`, `fences.json`, `links.json`). The formats shown are examples only.

Data files only cut test time if the test-selection rule names the tests for each file, as it does for samples. For example: defaults run `test_startup.py` and `test_settings.py`; shortcuts run `test_ui.py` and `test_settings.py`; file types run `test_files.py`. A short check validates the data (well-formed JSON, required items, no duplicate keys or extensions, defaults within the allowed values). Adding a data file or changing the code that reads it still runs the full suite. Revising the rule is for 5garashi.com to decide.

The work goes one step at a time, in the order above, with no visible change at any step. If done together with the localization proposal, both should share one loader.

## 出典と確認範囲

- 候補の置き場所と件数は、2026-10-23 に mdEditor のソース（`wwwroot/app.js` の `displayDefaults`・ショートカットの一覧・`graphvizLanguages`、`wwwroot/index.html`、`MainWindow.xaml.cs` の `DefaultZoom`・`ZoomSteps`・`IsDocumentPath`・`OpenFilter`・`ImageExtensions`・`VideoContentTypes`・`MaxPageBytes`・自動バックアップの間隔、`setup.ps1` の `$fileTypes`、`store/AppxManifest.template.xml`）で確かめた。
- データの形とファイルの名前は例であり、まだ決めていない。
- テストの範囲の決め方は、appAutoTester の `docs/test-selection-proposal.md`（v1.1.0）を基にした。
- 本書は文書の追加だけで、アプリの起動・テストの実行は行っていない。

## 変更の記録

- 0.1.0（2026-10-23）: 最初の版。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-23 JST
