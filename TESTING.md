<!--
  ファイル名 / File: TESTING.md
  概要 / Description: mdEditor を変えたときに、どのテストを動かし、どう報告するか / Which tests to run when mdEditor changes, and how to report them
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-11-02
  版 / Version: 1.0.22
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

- 共通のルール「不具合を直したら、水平展開する」の例集めの自動テストは、`targets\mdeditor\test_written_elsewhere.py` である。ほかの環境では正しい書き方を、mdEditor が書き換えず、意図どおりに表示することを確かめる。
- 表示の読み取り・自動修正のルール・HTML などからの変換を変えたときは、見つけた例をこのファイルに足し、`run.cmd mdeditor -k test_written_elsewhere`（約 2 分）で動かす。

## 訳を変えたとき

英語・中国語の文を変えたら、`test_translations.py` を動かし、[docs/translation-review.md](docs/translation-review.md) の見直しの依頼文をユーザーに示す。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-11-02 JST
