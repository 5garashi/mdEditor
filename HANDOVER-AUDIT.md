<!--
ファイル名 / File: HANDOVER-AUDIT.md
概要 / Description: 宣伝資料の引継ぎ調査 / Promotion handover assessment
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.0.5
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# mdEditor 宣伝資料の引継ぎ調査 / Promotion handover assessment

判断は「部分的に引き継ぎ、修正する」。全体を完成品として引き継ぐ価値はないが、公式素材、日英の説明、短いサンプル、実演台本には再利用価値がある。実際のアプリ画面を見せる構成へ変更する。今回は調査と方針決定までで、既存19ファイルの修正・削除・公開は行っていない。

## 状態と保全 / State and preservation

- 調査日: 2026-10-12 JST。オンライン正本のIdentityヘッダーは4.12.0、本文の変更日は2026-10-12。第3・4・6〜9・11〜13条を参照した。
- 作業場所 `C:\miFfplay\mdEditorPromotion` は開始時空。親のmiFfplayリポジトリには `.gitignore`、`AGENTS.md` の未コミット変更と未追跡ファイルがある。これらに変更を加えていない。
- 隣の `C:\miFfplay\mdEditor` はmain、追跡ファイルの変更なし。samplesに未追跡の画像・PDFが18個ある。これらも変更していない。
- PR #9はopen、draft、merged=false。headは `fb118bf37fed8f23bda009c09f07b9449b73ae2e`。オンラインmainは `6440a2edbe0c570de9fffe8636e7e0233711d978`。差分は19ファイル追加、603行追加、変更・削除なし。
- `audit-source` に独立したcloneを作成。元のheadを `audit/baseline-fb118bf` に保持し、親フォルダーの `launch-kit-baseline.bundle` に完全なGit履歴を保存、bundle verify成功。調査報告は `audit/handover-review` ブランチに追加する。リモートへのpushはしていない。

## 19ファイルの判断 / File decisions

| 対象 / Files | 数 / Count | 判断 / Decision | 理由・処置 / Reason and next action |
| --- | ---: | --- | --- |
| promotion/index.html、en.html | 2 | 修正 | 日英の機能説明は活用。実画面を中心にし、認定状況を確認日付き事実へ変更。公開用とCSS・SVG埋め込み確認用を生成する。 |
| promotion/style.css | 1 | 修正 | 配色・書体の土台は活用。ロゴ切替、ヘッダー、狭幅、拡大、強制カラーを実ブラウザーで修正・検証する。 |
| promotion/README.md | 1 | 修正 | 配置構造は活用。版管理・署名・配布物のライセンス・公開検証手順を追記する。 |
| promotion/RECORDING.md | 1 | 修正 | 秒割りは参考にする。現在のWindows環境を前提に、開く→編集→図・数式→PDF保存の実操作に合わせる。 |
| promotion/IDENTITY-CHECK.md | 1 | 修正 | 未確認の明記は活用。「適用」の主張と実際の不備を区別し、証拠・環境・結果を記録する。 |
| promotion/assets/公式SVG5点 | 5 | 再利用 | 現行公式原本とGit blobのバイト一致を再確認。ローカルcheckoutはcore.autocrlf=trueによるCRLF化があり、ファイルそのもののバイト一致とは区別する。 |
| promotion/assets/mdEditor-intro-en.mp4 | 1 | 差替え・旧案保管 | 40秒、1280×720、H.264/AACを確認。実演ではないため依頼された動画として採用しない。削除候補だが実行していない。 |
| promotion/assets/video-poster.png | 1 | 差替え・旧案保管 | 新しい実演動画と一致するアプリ画面のポスターが必要。削除候補だが実行していない。 |
| samples/quickstart-workflow.md、.en.md | 2 | 修正 | 短い業務例として有用。変更する箇所と期待する表示を具体化し、実機とPDFで検証する。 |
| samples/quickstart-engineering-note.md、.en.md | 2 | 修正・主サンプル | 図・数式・表がそろい、実演の主役に適する。電圧変更後の数式・表を自分で更新する手順を付ける。自動計算とは宣伝しない。 |
| samples/quickstart-ai-notes.md、.en.md | 2 | 修正・補助サンプル | 整理用テンプレートとして活用。AI生成機能がない旨を維持し、主訴求にはしない。 |

破棄候補は旧MP4と旧ポスターの2点。現段階ではGitに保管し、物理的削除はしていない。

## 発見した問題 / Findings

1. 実際のChromeでローカル日本語ページを1280×900で表示し、CSS・SVGの読込みを確認。しかし昼夜ロゴが両方表示された。`.brand img { display:block }` の詳細度が `.logo-night { display:none }` より高いため。夜でも同じ構造の問題がある。単体埋め込みだけでは直らない。スクリーンショット: 親フォルダーの `page-ja.png`。
2. 第9-1条のヘッダー下辺ink 3px罫、右側シンプル版マークがない。第9-5条のREADME・報告書末尾署名もない。IDENTITY-CHECKの「署名」確認は不十分。
3. 大きい見出しとロゴ帯が初画面を占め、アプリの操作結果が見えない。技術サンプルの図・数式と編集前後を実画面で示すほうが、導入判断につながる。
4. サンプルリンクは相対パスであり、必ずsandboxドメインになる実装ではない。正式配布先でのHTTP応答、保存名、内容、別言語リンクの検証が必要。公開先は未決定・未検証。
5. 「認定中」は現在も正しいと確認できていない。日本語・英語のStore HTMLを取得できたが、公開アプリの版と認定状況を確定する証拠にはならない。日本語HTMLの構造化データにはoffers.price=0があるが、地域別の実際の購入画面の価格確認は未実施。未確認の認定状態や地域共通の価格を断定しない。
6. サンプル冒頭のPDFショートカットは変更可能な既定値である。初めて使う人には「PDFに保存」ボタンを主手順にし、ショートカットは既定値と説明する。
7. ロゴのライセンス記載は維持されている。音源は音声ストリームの存在だけでは、原本由来、編集範囲、利用条件、クレジットの十分さを確認できない。新動画で使用する場合は第11・12条に照合し、使用元・加工・権利表記を記録する。
8. 宣伝資料の内容を変える際には第12-7条の版更新判断が必要。別リポジトリの宣伝資料として管理するか、アプリと同じVERSIONを共有するかを整理する。宣伝の変更だけを理由に、説明なしでアプリ版を変更しない。

## 実機操作・録画の実現性 / Windows feasibility

Windowsデスクトップ1366×768、Chrome・Edge・ffmpeg・Python・pywinauto・Pillowを検出した。権限内ではウィンドウ列挙が空だったが、許可された実行環境では稼働中のmdEditorと別プレビューを検出できた。UI Automationで「開く」「PDFに保存」、文書名diagram-test.md、保存済み表示などの要素を読み取れた。操作の自動化には実現の見込みがある。

ただし、操作成功や録画成功まで確認済みではない。ウィンドウ領域を画像取得した試行では、前面のVS Codeが写り、隠れたアプリの画面を取得できなかった。`app-audit.png` はmdEditorの描画の証拠に使用しない。撮影には専用のアプリ起動、前面化、個人情報を含まない領域の確保が必要。使用中のアプリの文書を勝手に置き換えない。

初期判断: 自動化を断念する根拠はない。最初に専用サンプルの開く・編集・PDF保存と短時間録画を試し、成功した工程だけを制作計画に入れる。アプリ本体の変更が必要かはまだ判断できない。

## 次の制作順序 / Next steps

1. 専用のWindows実行環境で技術サンプルを開き、編集反映、Mermaid、KaTeX、PDF保存、保存PDFの内容を確認。短い録画で画面取得・字幕領域・読める文字サイズを確かめる。
2. 機能と公開状況を確定する。公開ページだけで認定状況が分からない場合、所有者の提供する対象版の認定・公開情報に基づく。ユーザーの現在の作業画面から無関係な情報を収集しない。
3. その実画面を使って日英紹介ページを修正。公開用一式とCSS・公式SVGを埋め込んだ単体確認用を作る。確認用のサンプル配布方式も明示する。
4. 日英・昼夜・320px・200%拡大・文字だけ200%拡大を実ブラウザーで確認し、スクリーンショットを提示。キーボード、強制カラー、グレースケール、ナレーターの確認を記録する。
5. 30〜45秒の実演を収録。字幕だけで操作が分かるようにし、必要なら日英字幕を用意する。音源使用時は停止・音量調整と権利表記を確認する。できない工程を文字スライドで完成扱いしない。
6. 日英6サンプルを実機表示・PDFで確認。配布先が決まった段階で公開環境のリンクとダウンロードを検証する。
7. 再利用・差替え、成果物、検証結果、未確認項目をまとめて提示。公開・mainへのマージ・PR/ブランチ削除はユーザー判断後に行う。

本調査は全面的なWCAG 2.2 AA準拠の検証ではない。ブラウザー確認は日本語・昼・1280×900の初画面のみ。Windowsの新サンプル描画・PDF出力、字幕の読解性、音源の加工と権利、正式公開先の配布動作は未確認。

## 参照 / Sources

- [Design Office Identity 正本](https://5garashi.github.io/5garashi-identity/) — 取得HTMLは親フォルダー `identity-current.html` に改変せず保管。CC BY-ND 4.0、第9条のコード断片はMIT。
- [PR #9](https://github.com/5garashi/mdEditor/pull/9) — 接続GitHubのメタデータと取得Git差分。
- [mdEditor](https://github.com/5garashi/mdEditor) — main SHAをGitHub APIで確認、説明・実装・プライバシー文書を参照。
- [日本語Store](https://apps.microsoft.com/detail/9nww2qrclmhc?hl=ja-JP&gl=JP)、[English Store](https://apps.microsoft.com/detail/9nww2qrclmhc?hl=en-US&gl=US) — 取得HTMLは親フォルダーに保管。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-12 JST
