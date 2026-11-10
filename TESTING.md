<!--
  ファイル名 / File: TESTING.md
  概要 / Description: mdEditor を変えたときに、どのテストを動かし、どう報告するか / Which tests to run when mdEditor changes, and how to report them
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-11-02
  版 / Version: 1.0.24
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# mdEditor のテスト

mdEditor のファイルを変えたら、この文書に従ってテストを選び、動かし、結果を報告する。変更を始める前に読む。

## テストの置き場所

- テストは、隣のフォルダー `..\appAutoTester`（非公開のリポジトリ 5garashi/appAutoTester）の `targets\mdeditor` にある。本物の画面を操作して確かめる。
- **どのテストを動かすかの決まりの正本**は、`..\appAutoTester\docs\test-selection-proposal.md` である。決まりは改版されるので、表や件数をこの文書に書き写さず、作業のたびに正本を読む。

## 手順

1. **範囲を決めて伝える**: 変えたファイルと差分から、正本の表で範囲を決める。動かす前に、どのファイルがどの行に当たり、どの範囲になるかを伝える。
2. **全体テストの前に確認する**: 全体テスト（`..\appAutoTester\run.cmd mdeditor`、約 15 分、画面が開いたり閉じたりする）を動かす前に、理由と時間を伝えて、ユーザーの返事を待つ（正本の規則 8）。ユーザーがその作業で全体テストを明示的に頼んだときだけ、確認を省く。
3. **動かす**: `run.cmd` は、フルパスで呼ぶ。テストのファイルだけを変えたときは、appAutoTester のフォルダーで `.venv\Scripts\python.exe -m pytest targets\mdeditor\<ファイル名>` で動かす（`run.cmd` にファイル名を渡すと、全体が選ばれる）。
4. **報告する**: 動かしたテストと結果（例: 「サンプルのテスト 7 件が通った。全体テストは動かしていない」）を書く。ビルドが通っただけで、確かめたと書かない。
5. **版を出す前**: リリースの PR を出す前には、全体テストを 1 回動かす（これも、始める前に確認する）。

## 不具合を直したとき

- 表示を合わせる書き方か、自動修正のルールを足すかは、README の「対応する書き方の方針」で決める。ほかの環境では正しい書き方と、主流の AI チャットが出す Markdown だけに合わせ、どこでも正しくない書き方や、前の版の不具合で崩れて保存された文書には合わせない。

- 共通のルール「不具合を直したら、水平展開する」の例集めの自動テストは、`targets\mdeditor\test_written_elsewhere.py` である。ほかの環境では正しい書き方を、mdEditor が書き換えず、意図どおりに表示することを確かめる。
- 表示の読み取り・自動修正のルール・HTML などからの変換を変えたときは、見つけた例をこのファイルに足し、`run.cmd mdeditor -k test_written_elsewhere`（約 2 分）で動かす。
- 複数行の `$$ … $$` は、区切りと式が同じ行にあるもの、引用・リスト内、従来の区切りだけの行をコーパスで確認する。コード内・金額・閉じていない式・空行をまたぐ式は、数式と取り違えず本文を保つことも確認する。`test_multiline_dollar_math_exports` では HTML・Word・PDF に数式として保存されることを確認する。

## GitHub 変換と GitHub 表示を変えたとき

- `targets\mdeditor\test_release_1023.py` と `test_release_1024.py`（GitHub 変換が先に当てる自動修正のルール）を動かす（`run.cmd mdeditor -k "test_release_1023 or test_release_1024"`、約 2 分）。
- GitHub 表示のテストは、GitHub に送らない。環境変数 `MDEDITOR_GITHUB_RESPONSE_FILE` に、GitHub の答えを保存したファイルを指定すると、アプリはそれを GitHub の答えとして使う（テストのためだけの仕組み）。
- 本物の GitHub に送って確かめるテストは、ふだんは動かない。GitHub の答えの形が変わっていないかを確かめるときだけ、環境変数 `MDEDITOR_LIVE_GITHUB=1` を付けて動かす（短い見本の文字を api.github.com に送る）。
- 書き換え方の根拠は、GitHub の Markdown API で確かめた結果である。書き換え方を足す・変えるときは、変換の前と後を API で確かめ、結果をテストの見本に足す。
- 別ウィンドウのプレビューでの GitHub 表示は、テストの道具が別ウィンドウを操作できないため、自動では確かめていない。

## 目次を変えたとき

- 目次の文は `TextFormats.cs` の `TableOfContents` が作る（呼び出し元は `MainWindow.xaml.cs` の「目次」の 1 か所）。`test_release_1011.py` の目次のテスト（`-k toc`）、`test_release_1018.py::test_section_numbers_key`、`test_release_1024.py` の `test_contents_` で始まる 2 件（見出しの数式・コード・文字参照・括弧・アドレス）と、`test_startup.py` 全体を動かす。
- 見出しに新しい種類の書き方を足して確かめたときは、`test_release_1024.py` の 2 件に例を足す。

## 訳を変えたとき

英語・中国語の文を変えたら、`test_translations.py` を動かし、[docs/translation-review.md](docs/translation-review.md) の手順 4・5 のとおり、見直しをほかの AI に共通の道具で頼み、指摘を判断して直し、報告する。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-11-11 JST
