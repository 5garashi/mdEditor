<!--
ファイル名 / File: OFFICE-IDENTITY-HANDOVER.md
概要 / Description: オリジナル5拍子曲のIdentity掲載検討を引き継ぐ / Handover for Identity consideration
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
版 / Version: 1.0.0
ライセンス / License: MIT（本引き継ぎ文のみ / this handover text only; music license not specified）
SPDX-License-Identifier: MIT
-->
# OfficeIdentity担当AIへの引き継ぎ — Light Steps v1

以下の内容で、同梱ファイルとともに引き継いでください。

## 依頼

5garashi.com設計事務所 / 5garashi.com Design OfficeのIdentityへ、オリジナル曲 **Light Steps / 軽快なステップ v1.0.0** を掲載したいです。添付の音源・制作データを確認し、音の特徴とブランド上の位置づけを整理して掲載案を作ってください。

既存の公式テーマ曲とは別に、**ソフトウェアが軽快に動く印象を伝える、アップテンポの5拍子曲**として制作しました。既存テーマの音源・MIDIを使った編曲ではありません。まずは「軽快な操作デモ・製品紹介向けの曲」として追加する案を検討してください。既存テーマを置き換えるかどうかは、今回の制作では決定していません。

確認したIdentity版はv4.12.0です。担当作業の開始時にも [現行の正本](https://5garashi.github.io/5garashi-identity/) と変更履歴を確認し、掲載・公式採用には必要な版管理を行ってください。この引き継ぎは公式採用済みの宣言ではありません。

## 音楽の狙い・アピールポイント

1. **「5」を音でも伝える。** 5/4拍子を「3＋2」のまとまりで聴かせ、5拍目にもアクセントを置いています。5garashiの名称や5本の唐辛子のマークとの関係を説明しやすい曲です。
2. **軽快な操作感を連想させる。** 125 BPMのテンポと、短く弾む音の立ち上がり・余韻で、開く、編集する、プレビューする、といった操作の小気味よさを表現する狙いです。アプリの実際の性能・速度を証明する音楽ではありません。
3. **明るさと親しみやすさ。** Dメジャー・ペンタトニックを中心に、明るい短い旋律と、応答するような音型を組み合わせています。技術的な製品を紹介する場面にも、近寄りやすい印象を添えることを目指しました。
4. **画面の説明を主役にする。** 重いドラムや長い持続音を使わず、軽いプラック音、短い鍵盤音、控えめなベースとシェーカーを組み合わせました。動画では字幕だけでも内容が分かります。将来ナレーションを付ける場合は、声との音量バランスを改めて確認してください。
5. **あとから育てられる。** WAV・MP3に加えてMIDI、音符データ、生成コードもあります。動画の長さや用途が変わった際に、フレーズを追加して別版を制作できます。現状は49秒の終止付きで、シームレスなループ素材ではありません。

「明るい」「軽快」は制作意図です。最終的な印象とブランドへの適合は、担当AIと本人が実際に試聴して判断してください。

## 仕様と構成

| 項目 | 内容 |
| --- | --- |
| 曲名 | Light Steps / 軽快なステップ |
| 曲の版 | 1.0.0（ファイル名のv1に対応） |
| 制作日 | 2026-10-14 |
| 作曲・音源生成 | Codex、5garashi.com設計事務所の依頼に基づく |
| 拍子・テンポ | 5/4、125 BPM、3＋2のグルーヴ |
| 主な音階 | Dメジャー・ペンタトニック（D、E、F♯、A、B） |
| 長さ | 49秒。20小節が48秒、その後に終止と余韻 |
| 音色 | 合成したプラック、短い鍵盤音、ベース、シェーカー。生楽器の録音ではない |
| 展開 | 短い導入 → 4種の旋律パターン → 中盤の高音の応答 → 終止 |
| WAV | 44,100 Hz、16 bit PCM、ステレオ |
| MP3 | 192 kbps、ステレオ、試聴・配信用 |
| MIDI | Standard MIDI File形式0、480 ticks/quarter、テンポ・拍子・旋律・伴奏・ベースを収録 |
| PCM原本のサンプルピーク | 約−6.38 dBFS。ラウドネス値・AACのtrue peak値とは別 |

MIDIの楽器指定は近似です。WAVの合成音色、シェーカー、ステレオの位置、エコー、最終フェードはMIDIに再現されないため、再生ソフトの音色で印象が変わります。現在の音を確認する正本はWAVです。

## 同梱ファイル

| ファイル | 用途 |
| --- | --- |
| mdeditor-light-steps-v1.wav | 制作・保存用の音源原本 |
| mdeditor-light-steps-v1.mp3 | すぐ試聴するための圧縮音源 |
| mdeditor-light-steps-v1.mid | 楽譜化・編曲用の音符とテンポ・拍子 |
| score-v1.json | 音符、タイミング、音色の役割、制作情報 |
| validation-v1.json | WAVのSHA-256、ピーク、長さ、動画フレーム一致の記録 |
| compose_music.py | NumPyによる作曲・合成、MIDI・MP3・動画の生成コード |
| mdEditor-demo-vertical-en-light-steps-v1.mp4 | 実際の操作デモに曲を付けた使用例。英語字幕付き |
| OFFICE-IDENTITY-HANDOVER.md | この引き継ぎ文 |
| manifest.json | 引き継ぎZIPに含む各ファイルのサイズ・SHA-256 |

ZIP内のcompose_music.pyは参照用コピーです。コードの実行場所は下記の元リポジトリで、Python、NumPy（制作時2.2.6）、ffmpeg、元の無音動画が必要です。ZIPだけでスクリプトをそのまま実行する構成ではありません。

制作元: [5garashi/mdEditor — promotion/site-integration](https://github.com/5garashi/mdEditor/tree/promotion/site-integration/promotion/integration/tiktok)。音楽制作コミットは `4e2323d`。元のファイルは `promotion/integration/tiktok/music/`、コードはその親フォルダーにあります。アプリ本体の変更は行っていません。

## 掲載用の紹介文案

日本語:

> **Light Steps — 軽快なステップ**
>
> ソフトウェアの軽快な操作感をイメージした、125 BPM・5拍子のオリジナル曲。「3＋2」のリズムに弾む旋律を重ね、5garashi.comの「5」を音で表現します。製品紹介や操作デモに、明るく親しみやすい印象を添えるために制作しました。

English:

> **Light Steps**
>
> An original 125 BPM track in 5/4, inspired by responsive software interactions. A 3+2 groove and a buoyant melody give musical form to the “5” in 5garashi.com. Created to bring a bright, approachable feel to product introductions and demonstrations.

## 担当AIにお願いしたい作業

1. WAVと操作デモを試聴し、既存の公式テーマとの役割分担を提案する。例えば「既存テーマ: 落ち着いた紹介」「Light Steps: 軽快な製品・操作デモ」。印象は本人にも確認する。
2. 第11条の音のIdentityへ、日英の説明・仕様・試聴プレーヤー・楽譜またはリズムの図・ダウンロード導線を追加する案を作る。再生は本人が開始し、一時停止・音量調整を可能にする。音だけで情報を伝えない。
3. 正式採用する場合の曲名、版、原本の配置、用途を明文化する。既存テーマを無断で置き換えない。Identity側のVERSION・CHANGELOG・配布パッケージ・単一ファイル版を、そのリポジトリの手順で更新する。
4. **音楽の公開ライセンスとクレジットを本人と確定する。** 現在の曲は公開ライセンス未指定。既存IdentityのCC BY-ND 4.0や生成コードのMITを自動で適用しない。動画への同期、抜粋、フェード、ループ、再編曲を利用者にどこまで許すかも整理する。作曲・生成がCodexであるという制作経緯を保持する。
5. 選んだ配布形式の音量・クリッピング・再生、ブラウザーのキーボード操作、昼夜・狭い画面・拡大・読み上げを検証し、結果と未確認を記録する。新しい曲の再演奏・長尺版は別版として管理する。

## 現在の確認範囲と制約

音源を生成し、49秒・PCM原本のピークを計算しました。音楽付き動画は、無音版とデコードした全映像フレームが一致します。Chromeで動画再生と確認ページの昼夜・幅320/1280px・文字サイズ100/200%を確認しました。

5拍子と125 BPMは作曲データおよびMIDIの指定に基づきます。専門家による聴感評価、ナレーションとの混合、TikTokなどでの圧縮後の音質・音量、音楽認識・権利判定は未確認です。GitHubの公開作業ブランチには保存済みですが、SNSへは未投稿、Identityへも未掲載です。

デザイン基準出典: 5garashi.com設計事務所 / 5garashi.com Design Office — [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。既存のIdentity・公式ロゴのライセンスと、この新しい音楽のライセンスは別に扱います。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-14 JST
