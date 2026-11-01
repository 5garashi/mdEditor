<!--
  ファイル名 / File: docs/localization-structure-proposal.md
  概要 / Description: 言語を足しやすくするための、表示言語の文字の置き場所の提案 / Proposal for a localization structure that makes adding languages simple
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-23
  版 / Version: 0.2.0
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 表示言語の構成の提案

- 状態: 検討用の提案。実装は、5garashi.com から依頼があるまで保留する。
- 版: 0.2.0（2026-10-23）
- 関連: 言葉以外の一覧と初期値（設定の初期値、ショートカット、ファイルの種類など）は、[一覧や初期値をデータのファイルに出す提案](data-files-proposal.md)で扱う。
- 対象: mdEditor 1.0.15（日本語・英語・簡体字中国語）。取りかかるのは 1.0.15 を出したあとの版を想定する。

## 提案の要点

表示言語の文字と言語ごとの決まりを、言語ごとのファイル 1 つと、言語の一覧 1 つにまとめる。言語を足すときは、ファイルを 1 つ足し、一覧に 1 行足せば済むようにする。画面の見た目と動きは変えない。

## 1. 今の置き場所

今は、表示言語の文字が 3 か所に分かれている。

| 場所 | 訳しているもの | 持ち方 |
| :--- | :--- | :--- |
| `Strings.cs`（英語）・`Strings.Chinese.cs`（中国語） | ダイアログ・確認・エラーの文、ファイルの種類を選ぶ欄 | 日本語の文を見出しにした辞書（119 個） |
| `wwwroot/app.js` の `messages`・`helpItems`・`samples` | 画面の中の文字（ボタン、設定画面、お知らせ、「書き方」、「サンプル」の一覧） | 言語ごとのまとまり（`ja`・`en`・`zh`）か、1 項目に 3 言語 |
| `TextFormats.cs` の `Pick(ja, en, zh)` | ファイルの中に書き込む言葉（記号なしのテキストの「［図］」、メールを開いたときの「差出人」など） | 引数で 3 つの言葉を渡す |

このほか、`wwwroot/index.html` に起動直後の日本語の文字があり、「Language」の一覧の 日本語・English・简体中文 も直接書いてある。

### 今、言語を足すと直す場所

| 場所 | 足すもの |
| :--- | :--- |
| `Strings.cs` | 新しい辞書の追加、`Tr`・`Normalize`・`DefaultLanguage`・`NameList`・`SentenceJoin` の分岐 |
| 新しい `Strings.Xxx.cs` | 119 個の訳 |
| `TextFormats.cs` | `Pick(ja, en, zh)` の引数を、すべての呼び出し（5 か所）で 1 つずつ増やす |
| `wwwroot/app.js` | `messages` に 1 ブロック、`helpItems`（49 項目）と `samples` に 1 言語ずつ、言語の一覧、`htmlLang`、`normalizeLanguage`、読み上げの言語の見分け方、`language === "en"` のような分岐（8 か所） |
| `wwwroot/index.html` | 「Language」の一覧に 1 行、読み上げの言語に 1 行 |
| `wwwroot/styles.css`・`wwwroot/export.css` | 書体の `:lang()` |
| `store/AppxManifest.template.xml` | `<Resource Language>` |

困るのは、`Pick` の引数が言語の数だけ増え続けることと、言語ごとの決まり（文の間の空白、かぎかっこ、箇条書きの記号）がコードの分岐に散らばっていることである。

## 2. 提案する構成

```
wwwroot/lang/
  languages.json      言語の一覧と、言語ごとの決まり
  ja.json             日本語のすべての文字（元になる言語）
  en.json
  zh-CN.json
```

### languages.json

言語ごとの決まりを、コードの分岐ではなくデータで持つ。

```json
[
  { "code": "ja", "name": "日本語", "htmlLang": "ja", "match": ["ja"],
    "sentenceJoin": "", "quote": ["「", "」"], "listMarker": "・",
    "logo": "ja", "readerLang": "ja-JP" },
  { "code": "en", "name": "English", "htmlLang": "en", "match": ["en"],
    "sentenceJoin": " ", "quote": ["\"", "\""], "listMarker": "• ",
    "logo": "en", "readerLang": "en-US" },
  { "code": "zh", "name": "简体中文", "htmlLang": "zh-CN", "match": ["zh"],
    "sentenceJoin": "", "quote": ["“", "”"], "listMarker": "・",
    "logo": "en", "readerLang": "zh-CN" }
]
```

| 項目 | 意味 | 今の場所 |
| :--- | :--- | :--- |
| `code` | 言語の名前（設定の保存、`--lang` で使う） | `Strings.cs`・`app.js` |
| `name` | 「Language」の一覧に出す名前 | `index.html` |
| `htmlLang` | `<html lang>` の値（書体の選び分けに使う） | `app.js` の `htmlLang` |
| `match` | Windows の表示言語や `--lang` の値と合わせる先頭の文字 | `Normalize`・`normalizeLanguage`・`DefaultLanguage` |
| `sentenceJoin` | 文と文の間に入れる文字 | `SentenceJoin` と `app.js` の分岐 |
| `quote` | ファイル名などを囲むかっこ | `NameList` |
| `listMarker` | 記号なしのテキストの箇条書きの記号 | `TextFormats.cs` の `Pick` |
| `logo` | 使うロゴタイプ（日本語版か英語版か） | `app.js` |
| `readerLang` | 読み上げで使う音声の言語 | `app.js` の `readerVoices` |

読み上げで段落の言語を見分ける処理（かながあれば日本語、漢字だけなら中国語）は、文字の種類による判定なので、コードに残す。

### 言語ファイル（ja.json など）

今 3 か所に分かれている文字を、区分けして 1 ファイルに入れる。

```json
{
  "ui":      { "open": "開く", "save": "保存" },
  "host":    { "この文書には表がありません。…": "この文書には表がありません。…" },
  "formats": { "figure": "［図］", "image": "［画像］", "mailFrom": "差出人" },
  "help":    [ { "id": "heading", "name": "見出し", "example": "# 見出し" } ],
  "samples": [ { "file": "samples/diagram-test.md", "name": "図・チャート表示テスト…" } ]
}
```

| 区分 | 今の場所 | 読むところ |
| :--- | :--- | :--- |
| `ui` | `app.js` の `messages` | app.js（読み込んで使う） |
| `host` | `Strings.cs`・`Strings.Chinese.cs` | C#（起動時に読む） |
| `formats` | `TextFormats.cs` の `Pick` | C#。`Pick("［図］", "[Figure]", "［图］")` は `Text("figure")` になる |
| `help` | `app.js` の `helpItems` | app.js |
| `samples` | `app.js` の `samples` | app.js |

ファイルは `wwwroot` の中に置くので、今の `wwwroot` と同じくアプリと一緒に配られる。C# はアプリのフォルダーから読む。

### 言語を足す作業

1. `wwwroot/lang/xx.json` を 1 つ作る（`ja.json` を写して訳す）。
2. `languages.json` に 1 行足す。
3. 必要なら `styles.css`・`export.css` に書体の `:lang()` を 1 つ足す。
4. `samples/` に、その言語のサンプルを足す（任意）。
5. Microsoft Store に掲載するなら、`store/AppxManifest.template.xml` に `<Resource Language>` を 1 行足す。

`index.html` の「Language」の一覧と読み上げの言語の一覧は、`languages.json` から作る。コードの言語ごとの分岐は残らない。

## 3. 決めておきたいこと

1. **C# の文字の見出しは、日本語の文のままにする**: 今の `Tr("日本語の文")` の書き方は、`MainWindow.xaml.cs` の 85 か所などの呼び出しを書き換えずに済む。その代わり、日本語の文を直すと訳が見つからず、その文だけ日本語で出る。これは 2 の確認で見つける。`ui`・`formats` は、今の `messages` と同じく短い名前を見出しにする。
2. **抜けを見つける確認を 1 つ足す**: 次の 3 つを確かめる、小さな確認である。appAutoTester のテストの範囲の決め方 v1.1.0 の「訳文だけの変更」の行に、そのまま当てはまる。
   - すべての言語ファイルで、見出しがそろっている。
   - `{0}` などの置き換える部分と、ファイルの種類を選ぶ欄で使う `|` の数がそろっている。
   - コードの中の `Tr("…")` の文が、すべて `host` にある。
3. **日本語も 1 つの言語として扱う**: `index.html` の日本語も `ja.json` から入れる。日本語を直す場所も 1 か所になる。起動直後の一瞬は文字がない状態になるが、今も言語を書き換える処理は同じ時点で動いているので、見た目はほぼ変わらない見込み（未確認）。
4. **しないこと**: 右から左に書く言語や、数による語形の変化（1 file / 2 files）への対応は、今の 3 言語では要らないので入れない。

## 4. ボタンの表示名と読み上げ用の名前

### 今の作り

ボタンの文字は、表示と読み上げ（読み上げソフトに伝える名前）が同じなら、訳文を書き換えるだけで変えられる。表示と読みを分けたいときは、ボタンの種類によって直す場所が変わる。

| 変えたいこと | 直す場所 | テストの範囲（v1.1.0） |
| :--- | :--- | :--- |
| 表示名を変える（読みも同じに変わる） | `messages` の訳文だけ | 規則 4: `test_startup.py` |
| 表示と読みを分ける（ふつうのボタン） | `index.html` のボタンに `data-i18n-attr="aria-label:キー名"` を書き、`messages` に訳文を足す | HTML の属性とキーの追加なので全件 |
| 表示と読みを分ける（処理が文字を入れ直すボタン） | 上に加えて、その処理（`app.js`）も直す | 全件 |

1.0.15 では、「編集画面をウィンドウ全体に表示」「プレビューをウィンドウ全体に表示」のボタンを、表示は「ウィンドウ全体に表示」、読みは元の名前に分けた。この 2 つは押すと「元の表示に戻す」に変わり、`setDisplayMode` が毎回文字を入れ直すので、3 行目に当たった。新しいキー `fullscreenShort`、処理の変更、`index.html` の `aria-label` を足し、全件（約 15 分）を流した。

困るのは次の 2 点である。

- ふつうのボタンでも、表示と読みを分けるだけで全件になる。
- ボタンが処理で文字を入れ直すかどうかで直す場所が変わり、外から見て分かりにくい。

### 提案

辞書（言語ファイルの `ui`）だけで、表示名と読みを決められる決まりにする。

- キー `x` を表示名、`xLabel` があれば読み（`aria-label`）とする。`xLabel` がなければ、読みは表示名と同じにする（`aria-label` を付けない）。
- 画面の部品にも、状態で文字が変わるボタンにも、表示名と読みを設定する同じ関数（例: `setButtonText(button, key)`）を使う。`data-i18n` を処理する部分と、`setDisplayMode` などの文字を入れ直す処理は、どちらもこの関数を呼ぶ。
- 読みは、表示する文字を含むようにする（WCAG 2.5.3。音声操作で表示どおりに言えば押せるように）。2 の確認に、「`xLabel` が `x` の文字を含む」ことを足す。

例（1.0.15 のボタンをこの決まりで書くと）:

```json
"fullscreenEditor": "ウィンドウ全体に表示",
"fullscreenEditorLabel": "編集画面をウィンドウ全体に表示",
"fullscreenPreview": "ウィンドウ全体に表示",
"fullscreenPreviewLabel": "プレビューをウィンドウ全体に表示"
```

ショートカット一覧とボタンのヒントは、読み（`xLabel`、なければ `x`）を操作の名前として使う。そうすれば、表示名を短くしても一覧で区別できる。

この決まりにすると、表示名の短縮も読みの追加も、言語ファイルを直すだけで済む。合わせて、appAutoTester のテストの範囲の決め方の規則 4 を「この決まりに沿った `xLabel` の追加・変更も、文章だけの修正として `test_startup.py`」と改めることを提案する。規則の改訂は 5garashi.com が判断する。

## 5. 進め方の案

一度にすべて移すと変更が大きいので、4 つの段階に分ける。どの段階でも、画面の見た目と動きは変えない。

1. **C# 側**: `Strings.cs`・`Strings.Chinese.cs` の辞書と、`TextFormats.cs` の言葉を、`host`・`formats` として JSON に移す。3 の 2 の確認を足す。
2. **画面側**: `app.js` の `messages`・`helpItems`・`samples` を JSON に移す。
3. **ボタンの表示名と読み**: 4 の決まり（`x` と `xLabel`、`setButtonText`）を入れ、今ある `aria-label` の付け方（サンプルの「開く」、「書き方」の「挿入」、ショートカット一覧の「変更」「なし」、文字コードのボタン、1.0.15 の「ウィンドウ全体に表示」）をそろえる。
4. **言語の一覧**: `languages.json` を作り、言語ごとの分岐をなくす。

段階ごとにアプリのコードが変わるので、テストの範囲の決め方では全件を流す。

## English summary

This is a proposal; implementation waits for a request from 5garashi.com.

Today the display-language text lives in three places:

- the C# dictionaries in `Strings.cs` and `Strings.Chinese.cs`, keyed by the Japanese sentence;
- the `messages`, `helpItems`, and `samples` sections of `wwwroot/app.js`;
- the `Pick(ja, en, zh)` calls in `TextFormats.cs`.

Rules such as sentence spacing, quotation marks, and list markers are spread across code branches. Adding a language touches about ten places.

The proposal moves everything into `wwwroot/lang/`:

- **`languages.json`**: one entry per language, with its code, native name, `htmlLang`, matching prefixes, sentence join, quotes, list marker, logotype, and read-aloud language.
- **One file per language** (`ja.json`, `en.json`, `zh-CN.json`), with these sections:
  - `ui`: the screen text;
  - `host`: the C# dialogs, still keyed by the Japanese sentence, so the 85-plus `Tr` calls stay unchanged;
  - `formats`: the words written into files, replacing `Pick`;
  - `help`: the Syntax list;
  - `samples`: the Samples list.

Adding a language then means one new language file, one line in `languages.json`, and optionally a font rule, samples, and a Store manifest entry.

A small check makes sure that every language file has the same keys and the same placeholders and `|` separators, and that every `Tr` string in the code has an entry. Right-to-left scripts and plural forms are out of scope.

Button text and screen-reader names: today, shortening a button's visible text while keeping a longer screen-reader name needs an `aria-label` in `index.html`, and for buttons whose text the code rewrites (such as the 1.0.15 "Fill window" buttons) a code change too, so the full suite runs. The proposal lets the language file decide both: key `x` is the visible text and an optional `xLabel` is the screen-reader name (it must contain the visible text, WCAG 2.5.3), applied by one shared function for static and code-updated buttons alike. The shortcut list and tooltips use `xLabel` when present. The selection rule's rule 4 could then treat adding or changing an `xLabel` as a text-only change (`test_startup.py`); that revision is for 5garashi.com to decide.

The migration goes in four steps, each without visible change and each with a full test run:

1. the C# dictionaries and file words;
2. the app.js text;
3. button text and screen-reader names (`x` / `xLabel`), bringing the existing `aria-label` uses in line;
4. the language list.

## 出典と確認範囲

- 置き場所と件数は、2026-10-23 に mdEditor のソース（`Strings.cs`・`Strings.Chinese.cs`・`TextFormats.cs`・`MainWindow.xaml.cs`・`wwwroot/app.js`・`wwwroot/index.html`・`store/AppxManifest.template.xml`）で確かめた。
- 構成案の JSON は例であり、項目の名前と値はまだ決めていない。
- 4 のボタンの作りは、2026-10-23 に `wwwroot/app.js`（`setDisplayMode`、`data-i18n-attr` を処理する部分、`aria-label` を付ける箇所）と `wwwroot/index.html` で確かめた。
- 本書は文書の追加・追記だけで、アプリの起動・テストの実行は行っていない。

## 変更の記録

- 0.2.0（2026-10-23）: 「4. ボタンの表示名と読み上げ用の名前」を追記した（表示名と読みを言語ファイルだけで決める決まり `x`・`xLabel` と、規則 4 の改訂案）。進め方を 4 段階にした。
- 0.1.0（2026-10-23）: 言語ファイルへの集約の提案。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-23 JST
