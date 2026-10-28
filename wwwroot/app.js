/*
  ファイル名 / File: wwwroot/app.js
  概要 / Description: Connects editor controls to the native application host
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
(() => {
  const editorHost = document.getElementById("editor");
  let applyingHostEdits = false;
  const preview = document.getElementById("preview");
  const documentName = document.getElementById("document-name");
  const saveState = document.getElementById("save-state");
  const languageSelect = document.getElementById("language-select");
  const themeButton = document.getElementById("theme-button");
  const workspace = document.querySelector(".workspace");
  const divider = document.getElementById("pane-divider");
  const fullscreenButtons = document.querySelectorAll(".pane-fullscreen-button");
  const compactLayout = window.matchMedia("(max-width: 760px)");
  let editorRatio = Number.parseFloat(localStorage.getItem("mdeditor-editor-ratio"));
  let displayMode = "normal";
  let documentDirty = false;
  let pdfStarting = false;
  let pointerId = null;
  // The preview can open in its own window (for example on a second display).
  // That window loads this page with ?view=preview and shows only the preview;
  // the host relays the preview, the scroll position, and commands between them.
  const previewWindow = new URLSearchParams(window.location.search).get("view") === "preview";
  if (previewWindow) document.documentElement.classList.add("preview-window");
  let previewDetached = false;
  let previewLines = 0;

  const messages = {
    ja: {
      toolbar: "文書操作",
      sample: "サンプル", sampleHeading: "サンプルを開く",
      sampleIntro: "図と数式を含む文書を、編集欄に入れて試せます。短い例のほかに、使えるすべての図・チャート・数式を確かめる長い文書（日本語・英語・中国語）もあります。選んだときだけ入ります。",
      settingsPdf: "PDF の取り込み",
      settingsPdfFigureText: "図のまわりの文字",
      pdfFigureTextPicture: "図に入れる（見た目優先・初期設定）",
      pdfFigureTextText: "文字として残す（編集優先）",
      settingsPdfFigureTextNote: "図の範囲にかかる文字（イラストの横の説明など）を、図の画像に入れて見た目を保つか、文字として残して編集できるようにするかを選びます。PDF を開いたあと、お知らせ欄のボタンで、もう一方の方法で開き直すこともできます。",
      settingsPdfAlign: "右寄せ・中央寄せの文を HTML（align）で書いて、位置を残す",
      settingsPdfPagePictures: "文字を読み取れないページ（縦書き・文字化けする書体・図形で描いた文字）を、ページ全体の画像にする",
      settingsPdfFile: "詳細な数値を開く",
      settingsPdfFileNote: "取り込みの細かい数値（図とみなす線の間隔、ラベルの文字数、右寄せとみなす位置など）は、詳細設定ファイルで変えられます。メモ帳で開きます。変えた数値は、次に PDF を開いたときから使います。ファイル:",
      settingsExport: "書き出し",
      settingsMarkdown: "Markdown の書き方",
      settingsNearGitHub: "GitHub での表示に近づける（mdEditor 独自の書き方を使わない）",
      settingsNearGitHubNote: "オンにすると、GitHub にない mdEditor 独自の書き方（x^2^ の上付き、H~2~O の下付き、==強調==、++挿入++、定義リスト、::: の囲み、{#id} などの属性、略語、罫線の表、a. の番号）を、GitHub と同じく書いたとおりの文字で表示し、~文字~ を取り消し線にします。PDF などの書き出しも同じです。表示を GitHub に近づけるための設定で、GitHub と完全に同じにはなりません（図・地図・3D モデル、コードの色分け、数式の描き方などは mdEditor のままです）。",
      settingsHtmlSource: "HTMLに保存するとき、元の Markdown も入れる（mdEditor で開くと元どおりに戻せる）",
      settingsHtmlSourceNote: "HTML の中に、元の文章がそのまま入ります。画面に出ないコメント（<!-- -->）も含まれるので、人に渡す前に確かめてください。入れない場合も、mdEditor で開くと Markdown に変換して開けます（色や配置、図の絵は戻りません）。",
      sampleLoadFailed: "サンプルを読み込めませんでした。mdEditor を入れ直してから、もう一度お試しください。",
      sampleOpen: "開く", sampleOpenLabel: (name) => `サンプル「${name}」を新しい文書として開く`,
      about: "このアプリについて", aboutHeading: "このアプリについて", aboutVersion: "バージョン",
      aboutIntro: "mdEditor は 5garashi.com Design Office が作った、Markdown の編集・プレビュー・PDF 書き出しのアプリです。",
      aboutCompany: "5garashi.com Design Office の紹介", aboutWorks: "作品・ほかのアプリ",
      aboutSupport: "サポート・お問い合わせ", aboutPrivacy: "プライバシーポリシー",
      aboutNote: "リンクは選んだときだけブラウザーで開きます。起動時の広告や、PDF への宣伝の挿入はありません。",
      open: "開く", openTitle: "開く (Ctrl+O)", save: "保存", saveTitle: "保存 (Ctrl+S)",
      restoreDiscarded: "前の文書に戻す",
      restoreDiscardedTitle: (name) => `前の文書「${name}」に戻します。保存しなかった変更があったときは、閉じたときの状態に戻します`,
      saveAs: "名前を付けて保存",
      languageToggle: "Language", languageLabel: "表示言語",
      editorHeading: "Markdown を編集", editorLabel: "Markdown テキスト",
      previewHeading: "プレビュー", previewLabel: "Markdown の表示結果",
      dividerLabel: "編集画面とプレビューの幅",
      dividerTitle: "ドラッグまたは矢印キーで左右の幅を調整します",
      fullscreenEditor: "編集画面をウィンドウ全体に表示", fullscreenPreview: "プレビューをウィンドウ全体に表示",
      fullscreenShort: "ウィンドウ全体に表示",
      fullscreenExit: "元の表示に戻す", saved: "保存済み", unsaved: "未保存の変更",
      settingsReaderLang: "言語", readerLangAuto: "自動（段落ごとに判定）", readerLangJa: "日本語", readerLangEn: "英語", readerLangZh: "中国語（簡体字）",
      settingsReaderVoice: "音声", readerVoiceAuto: "自動（最初に見つかった音声）",
      readerNoVoiceFor: (lang) => `${{ ja: "日本語", en: "英語", zh: "中国語" }[lang] ?? ""}の読み上げに使える音声がこの PC にありません。Windows の「設定」→「時刻と言語」→「音声」で音声を追加してください。`,
      openUrl: "URLを開く", urlHeading: "URLを開く",
      urlIntro: "https:// で始まる Web ページを、Markdown に変換して開きます。スクリプトは実行せず、文書の内容をどこかへ送ることもありません。ページの画像は、そのページから読み込みます。",
      urlLabel: "アドレス", urlOpen: "開く",
      loadExternal: "画像・動画を読み込む",
      noticeExternal: (count) => `この文書には、インターネット上の画像・動画が ${count} 件あります。読み込むと、そのサーバーに IP アドレスなどが伝わります。`,
      externalVideo: (name, host) => `［外部の動画${name ? `: ${name}` : ""}（${host}）。読み込んでいません］`,
      videoLabel: (name) => `動画: ${name}`, videoStill: (name) => `▶ 動画: ${name}`,
      externalImage: (alt, host) => `［外部の画像${alt ? `: ${alt}` : ""}（${host}）。読み込んでいません］`,
      settingsInternet: "インターネット", settingsExternal: "ネット上の画像",
      externalAsk: "確認してから読み込む（初期設定）", externalAlways: "いつも読み込む", externalNever: "読み込まない",
      settingsExternalNote: "文書の中のネット上の画像（https のものだけ）を読み込むかどうかです。読み込むと、その画像のサーバーに IP アドレスや時刻が伝わります。スクリプトは実行せず、文書の内容を送ることもありません。",
      detachPreview: "別ウィンドウで表示", attachPreview: "本体に戻す", attachPreviewMain: "プレビューを本体に戻す",
      pdfStarting: "PDF出力を開始しています",
      findReplace: "検索・置換", findReplaceTitle: "検索・置換 (Ctrl+H)",
      settingsVideo: "動画", settingsVideoAutoplay: "自動再生",
      videoAutoplayAuto: "Windows の動きの設定に合わせる（初期設定）", videoAutoplayAlways: "いつも自動で再生する", videoAutoplayNever: "自動で再生しない",
      settingsVideoNote: "文書の中の動画は、音を消して自動で再生し、繰り返します（音を出す・止めるは動画のボタンで）。初期設定では、Windows の「アニメーション効果」がオフのとき、自動で再生せずに最初の画面と再生ボタンを表示します。",
      settingsMapTiles: "詳細な地図（OpenStreetMap）",
      mapTilesAsk: "確認してから読み込む（初期設定）", mapTilesAlways: "いつも読み込む", mapTilesNever: "読み込まない（簡単な世界地図だけ）",
      settingsMapTilesNote: "```geojson と ```topojson の地図は、まず PC の中の簡単な世界地図の上に描きます。道路や地名の入った詳細な地図は OpenStreetMap からインターネットで読み込み、そのとき地図のサーバーに、表示している場所と IP アドレスが伝わります。保存した HTML には入れません。",
      noticeMaps: (count) => `この文書には地図が ${count} 件あります。詳細な地図（OpenStreetMap）を読み込むと、地図のサーバーに表示している場所と IP アドレスなどが伝わります。`,
      loadMaps: "詳細な地図を読み込む",
      modelLabel: "3D モデル（STL）。ドラッグで回転、ホイールで拡大・縮小", modelEmpty: "三角形がありません",
      mapLabel: "地図（GeoJSON）", mapEmpty: "地図に描く図形がありません",
      toc: "目次", tocNoHeadings: "目次にする見出しがありません。「## 見出し」のように見出しを書いてから、もう一度押してください。",
      dropInsert: "カーソルの位置に入れる", dropOpen: "ファイルとして開く",
      imageButton: "画像の調整", imageHeading: "画像の大きさと位置",
      imageTarget: (name) => `対象の画像: ${name}`,
      imageSize: "大きさ", imageSizeOriginal: "元の大きさ（指定しない）", imageSize25: "本文の幅の 25%", imageSize50: "本文の幅の 50%",
      imageSize75: "本文の幅の 75%", imageSize100: "本文の幅いっぱい（100%）", imageSizePixels: "幅をピクセルで指定", imagePixels: "幅（ピクセル）",
      imagePosition: "位置", imagePositionNone: "指定しない（左）", imagePositionCenter: "中央", imagePositionRight: "右",
      imagePositionFloatLeft: "左に置き、右側に文章を回り込ませる", imagePositionFloatRight: "右に置き、左側に文章を回り込ませる",
      imageAloneNote: "この画像は、同じ行にほかの文字や画像があるため、「中央」と「右」は選べません。選ぶときは、画像だけの行にしてください。",
      imageApply: "変更する",
      imageNote: "GitHub と同じ書き方（HTML の <img width=\"50%\"> や <p align=\"center\">）に書き換えます。「元の大きさ」と「指定しない」に戻すと、Markdown の ![説明](ファイル) に戻ります。Ctrl+Z で取り消せます。",
      imageNone: "画像が見つかりません。編集欄で画像（![説明](ファイル) か <img>）の行にカーソルを置くか、プレビューの画像を右クリックしてから、もう一度お試しください。",
      imageMoved: "プレビューの画像の場所が、編集欄と合いませんでした。プレビューが新しくなってから、もう一度右クリックしてください。",
      imagePixelsInvalid: "幅は 1 から 10000 までの数で入れてください。",
      imageChanged: (size, position) => `画像を「${size}・${position}」にしました。Ctrl+Z で取り消せます。`,
      imageSizeWords: { original: "元の大きさ", pixels: (px) => `幅 ${px} ピクセル`, percent: (p) => `本文の幅の ${p}%` },
      imagePositionWords: { none: "位置の指定なし", center: "中央", right: "右", floatLeft: "左に置いて回り込み", floatRight: "右に置いて回り込み" },
      tocUpdated: "目次を新しくしました。", tocInserted: "カーソルの位置に目次を入れました。見出しを変えたら、もう一度「目次」を押すと新しくなります。",
      help: "書き方", helpTitle: "Markdown の書き方 (F1)", helpHeading: "Markdown の簡単な書き方",
      shortcuts: "キー操作", shortcutHeading: "キーボードショートカット",
      brandLogo: "assets/5garashi_logotype_outlined.svg", brandLogoDark: "assets/5garashi_logotype_reversed_outlined.svg",
      brandLogoAlt: "5garashi.com 設計事務所 / 5garashi.com Design Office",
      settings: "設定", settingsHeading: "設定",
      settingsIntro: "読みやすさや操作のしやすさを、使う人に合わせて調整できます。設定は次回の起動でも使います。",
      settingsText: "文字（編集欄とプレビュー）", settingsFont: "書体", settingsFontStandard: "標準",
      settingsFontUd: "BIZ UD ゴシック（読みやすさ重視）", settingsSize: "文字の大きさ", settingsSize100: "標準",
      settingsSize125: "大きい（125%）", settingsSize150: "とても大きい（150%）", settingsSize200: "最大（200%）",
      settingsSpacing: "行間", settingsSpacingNormal: "標準", settingsSpacingWide: "広め", settingsSpacingWider: "とても広い",
      settingsThemePaper: "表示テーマと PDF の用紙",
      settingsThemePaperNote: "押すたびに切り替わります。表示テーマは 自動（Windows に合わせる）→ ライト → ダーク、用紙は 自動 → A4 → Letter の順です。Ctrl+Shift+T と Ctrl+Shift+A でも、設定を開かずに切り替えられます。",
      settingsZoom: "画面全体の表示倍率",zoomOut: "縮小", zoomIn: "拡大", zoomReset: "初期値（90%）に戻す",
      zoomNote: "Ctrl + マウスホイール、Ctrl + プラス／マイナス、Ctrl + 0 でも変えられます。倍率は次回の起動でも使います。",
      settingsFullWidth: "プレビューの本文の幅を制限しない（広い画面で1行を長くする）",
      settingsEditor: "編集欄", settingsActiveLine: "いまの行を強調する", settingsThickCaret: "カーソルを太くする",
      settingsLint: "Markdown の書き方を確かめる（問題のある所に波線を引く）",
      lintStatus: (count) => `書き方の問題 ${count} 件（押すと一覧を表示）`,
      lintPhrases: {"Diagnostics": "書き方の確認", "No diagnostics": "問題は見つかりませんでした", "close": "閉じる"},
      lint: {"MD001": "見出しの段階が飛んでいます（# の次に ### など）。1 段ずつ下げてください。", "MD011": "リンクの書き方が逆です。[文字](アドレス) の順に書きます。", "MD018": "見出しの # の後に空白がありません（#見出し → # 見出し）。", "MD023": "見出しが行の先頭から始まっていません。前の空白を消してください。", "MD024": "同じ階層に同じ名前の見出しがあります。目次やリンクで見分けられません。", "MD037": "強調の記号（* や _）の内側に空白があり、強調になりません。", "MD038": "コード（`）の内側の前後に空白があります。", "MD039": "リンクの文字の前後に空白があります。", "MD042": "リンク先が空です。", "MD045": "画像に説明（代替テキスト）がありません。![説明](…) と書くと、画面読み上げでも伝わります。", "MD051": "ページ内リンク（#…）の行き先の見出しが見つかりません。", "MD052": "参照リンク [文字][名前] の名前が、どこにも定義されていません。", "MD056": "表のこの行の列の数が、見出しの行と違います。", "MD058": "表の前後に空行がありません。表として表示されないことがあります。", "fence": "コードブロック（``` や ~~~）が閉じられていません。文書の最後までコードになります。", "missing": "リンク先のファイルが見つかりません: "},
      settingsReader: "読み上げ", settingsReaderOn: "読み上げ機能を使う（プレビューの見出しに操作ボタンを表示）",
      settingsReaderRate: "速さ", rateSlow: "ゆっくり", rateSlower: "少しゆっくり", rateNormal: "ふつう",
      rateFaster: "少し速い", rateFast: "速い", settingsReaderPause: "段落の間", pauseNone: "間を空けない",
      pauseShort: "0.5 秒", pauseMedium: "1 秒", pauseLong: "2 秒",
      readerVoices: (ja, en, zh) => `この PC の音声で読み上げます（日本語: ${ja}、英語: ${en}、中国語: ${zh}）。文章を外部に送ることはありません。`,
      readerNoVoiceShort: "なし",
      readerNoVoice: "読み上げに使える音声がこの PC にありません。Windows の「設定」→「時刻と言語」→「音声」で音声を追加してください。",
      readerPlay: "読み上げ", readerPause: "一時停止", readerResume: "再開", readerStop: "停止",
      settingsBackup: "自動バックアップ", settingsBackupOn: "作業中の文書を自動でバックアップし、異常終了したときに復元する",
      settingsBackupNote: "保存していない変更を、この PC の中（%LOCALAPPDATA%\\mdEditor\\Recovery）に15秒ごとに控えます。元のファイルは書き換えません。保存するか、正常に終了すると控えは消えます。",
      settingsReset: "表示の設定を初期設定に戻す",
      settingsPasteKey: "クリップボードから新規作成", pasteKeyLabel: "起動するキー", pasteKeyNone: "なし",
      pasteKeyNote: "キーを選ぶと、確認のうえデスクトップにショートカットを作ります。テキストや画像をコピーしてからそのキーを押すと、mdEditor が起動して新しい文書に入れます。",
      pasteKeyReady: (key) => `${key} で使えます（デスクトップのショートカット）。テキストや画像をコピーしてから押してください。「なし」にするとショートカットを削除します。`,
      pasteKeyMissing: (key) => `前に作ったショートカット（${key}）がデスクトップに見つかりません。キーを選び直すと作り直します。`,
      shortcutIntro: "すべての操作はキーボードで行えます。キーは「変更」で好きな組み合わせに変えられ、「なし」で外せます。割り当てられるのは Ctrl と英字・数字の組み合わせ、または F キーです。",
      shortcutEnabled: "キーボードショートカットを使う", shortcutAction: "操作", shortcutKeys: "キー",
      shortcutChange: "変更", shortcutRemove: "なし", shortcutNone: "（なし）",
      shortcutPress: "キーを押してください", shortcutReset: "初期設定に戻す",
      shortcutChangeLabel: (name) => `「${name}」のキーを変更`,
      shortcutRemoveLabel: (name) => `「${name}」のキーを外す`,
      shortcutPressFor: (name) => `「${name}」に割り当てるキーを押してください。Esc で取り消します。`,
      shortcutCancelled: "変更を取り消しました。",
      shortcutInvalid: "Ctrl と英字・数字の組み合わせ、または F キーを押してください（Ctrl+Alt は使えません）。",
      shortcutReserved: (keys) => `${keys} は文字の編集や検索に使うため、割り当てられません。`,
      shortcutConflict: (keys, name) => `${keys} は「${name}」に使われています。先にそちらを変更してください。`,
      shortcutSaved: (name, keys) => `「${name}」を ${keys} にしました。`,
      shortcutRemoved: (name) => `「${name}」のキーを外しました。`,
      shortcutTurnedOn: "キーボードショートカットを使います。", shortcutTurnedOff: "キーボードショートカットを使いません（Esc とダイアログ内の操作は使えます）。",
      shortcutResetDone: "すべてのキーを初期設定に戻しました。",
      shortcutNames: {
        open: "開く", restoreDiscarded: "前の文書に戻す", save: "保存", saveAs: "名前を付けて保存", pdf: "PDFに保存", paper: "用紙の切り替え",
        theme: "表示テーマの切り替え", language: "表示言語の切り替え", encoding: "文字コード",
        toc: "目次を入れる・新しくする", findReplace: "検索・置換", findNext: "次を検索", findPrev: "前を検索", replaceAll: "すべて置換",
        help: "Markdown の書き方", shortcuts: "キー操作の一覧", fullscreenEditor: "編集画面をウィンドウ全体に表示",
        fullscreenPreview: "プレビューをウィンドウ全体に表示", nextPane: "次の欄へ移動", previousPane: "前の欄へ移動",
        windowFullscreen: "ウィンドウの全画面表示", settings: "設定", readAloud: "読み上げ・一時停止", stopReading: "読み上げを停止", html: "HTMLに保存", png: "PNGに保存", docx: "Wordに保存", openUrl: "URLを開く",
        detachPreview: "プレビューを別ウィンドウで表示・本体に戻す", image: "画像の大きさと位置"
      },
      helpIntro: "左の書き方で入力すると、右のように表示されます。「挿入」を押すと、編集欄のカーソル位置に例を入れます。",
      helpItem: "項目", helpSyntax: "書き方", helpResult: "表示", helpInsert: "挿入",
      helpInsertLabel: (name) => `「${name}」の例を挿入`,
      findPlaceholder: "検索する文字列", replacePlaceholder: "置換後の文字列",
      findLabel: "検索", replaceLabel: "置換",
      findPrev: "前へ", findNext: "次へ", replaceOne: "置換", replaceAll: "すべて置換",
      matchCase: "大文字小文字を区別", close: "閉じる",
      notFound: "見つかりません", replacedCount: (n) => `${n} 件を置換しました`,
      themeLabels: { auto: "表示: 自動", light: "表示: ライト", dark: "表示: ダーク" },
      themeTitle: "表示テーマを切り替えます（自動 → ライト → ダーク）",
      paperLabels: (paper, auto) => paper === "auto"
        ? `用紙: 自動（${auto === "letter" ? "Letter" : "A4"}）`
        : `用紙: ${paper === "letter" ? "Letter" : "A4"}`,
      paperTitle: "PDFの用紙サイズを切り替えます（自動 → A4 → Letter）。自動は Windows の地域の設定に従います",
      encodingLabels: {
        "utf-8": "UTF-8", "utf-8-bom": "UTF-8（BOM付き）", "utf-16le": "UTF-16 LE", "utf-16be": "UTF-16 BE",
        "shift_jis": "Shift_JIS", "euc-jp": "EUC-JP", "iso-2022-jp": "ISO-2022-JP（JIS）", "windows-1252": "Windows-1252（西欧）"
      },
      encodingGuessed: (label) => `${label}（推定）`,
      encodingButtonLabel: (label) => `文字コード: ${label}。押すと文字コードを選べます`,
      encodingButtonTitle: "文字コードを確認・変更します",
      encodingHeading: "文字コード", encodingLegend: "文字コードを選ぶ",
      encodingCurrent: (label) => `いまの文字コード: ${label}`,
      encodingReopen: "この文字コードで開き直す", encodingSave: "次の保存からこの文字コードにする",
      encodingHelp: "文字化けしているときは「この文字コードで開き直す」を選びます。内容はそのままで保存する形式だけを変えるときは「次の保存からこの文字コードにする」を選びます。",
      noticeGuessed: (label) => `この文書は UTF-8 ではないため、${label} と推定して開きました。文字化けしている場合は、保存する前に文字コードを選び直してください。`,
      noticeLossy: (label) => `${label} では読めない文字があり、「�」で表示しています。保存する前に、正しい文字コードを選んで開き直してください。`,
      noticeChoose: "文字コードを選ぶ",
      diagramError: "図を表示できません", diagramUnavailable: "図の表示機能を読み込めませんでした",
      ratio: (e, p) => `編集画面 ${e}%、プレビュー ${p}%`
    },
    zh: {
      toolbar: "文档操作",
      sample: "示例", sampleHeading: "打开示例",
      sampleIntro: "可以把包含图表和公式的文档放入编辑区试用。除了简短的示例，还有用来检查所有图表、图形和公式的长文档（日语、英语、中文）。只有选择时才会放入。",
      settingsPdf: "PDF 导入",
      settingsPdfFigureText: "图表周围的文字",
      pdfFigureTextPicture: "放入图片（外观优先・默认）",
      pdfFigureTextText: "保留为文字（编辑优先）",
      settingsPdfFigureTextNote: "选择把延伸到图表范围内的文字（插图旁边的说明等）放入图表的图片以保持外观，还是保留为文字以便编辑。打开 PDF 后，也可以用通知栏中的按钮以另一种方式重新打开。",
      settingsPdfAlign: "用 HTML（align）写出靠右和居中的文字，保留位置",
      settingsPdfPagePictures: "将无法读取文字的页面（竖排、乱码字体、以图形绘制的文字）作为整页图片",
      settingsPdfFile: "打开详细数值",
      settingsPdfFileNote: "导入时的细节数值（视为图表的线条间距、标签的字数、视为靠右的位置等）可以在详细设置文件中更改。用记事本打开。更改的数值从下次打开 PDF 时开始使用。文件：",
      settingsExport: "导出",
      settingsMarkdown: "Markdown 写法",
      settingsNearGitHub: "显示效果接近 GitHub（不使用 mdEditor 独有的写法）",
      settingsNearGitHubNote: "开启后，GitHub 没有的 mdEditor 独有写法（x^2^ 上标、H~2~O 下标、==强调==、++插入++、定义列表、::: 容器、{#id} 等属性、缩写、网格表格、a. 编号）会像在 GitHub 上一样按原文显示，~文字~ 显示为删除线。导出 PDF 等时也一样。此设置只是让显示接近 GitHub，并不会与 GitHub 完全相同（图表、地图、3D 模型、代码着色和公式的绘制方式仍是 mdEditor 的方式）。",
      settingsHtmlSource: "保存为 HTML 时，同时放入原来的 Markdown（用 mdEditor 打开时可以恢复原样）",
      settingsHtmlSourceNote: "原文会原样放入 HTML 中，其中也包括不在画面上显示的注释（<!-- -->），交给别人之前请先检查。不放入时，用 mdEditor 打开也能转换为 Markdown（颜色、排版和图表的图形不会恢复）。",
      sampleLoadFailed: "无法加载示例。请重新安装 mdEditor 后再试。",
      sampleOpen: "打开", sampleOpenLabel: (name) => `将示例“${name}”作为新文档打开`,
      about: "关于此应用", aboutHeading: "关于此应用", aboutVersion: "版本",
      aboutIntro: "mdEditor 是 5garashi.com Design Office 开发的应用，用于编辑和预览 Markdown 并导出 PDF。",
      aboutCompany: "5garashi.com Design Office 简介", aboutWorks: "作品和其他应用",
      aboutSupport: "支持和联系", aboutPrivacy: "隐私政策",
      aboutNote: "只有在选择链接时才会用浏览器打开。启动时不显示广告，也不会在 PDF 中插入宣传内容。",
      open: "打开", openTitle: "打开 (Ctrl+O)", save: "保存", saveTitle: "保存 (Ctrl+S)",
      restoreDiscarded: "返回上一个文档",
      restoreDiscardedTitle: (name) => `返回上一个文档“${name}”。如果有未保存的更改，会恢复到关闭时的状态`,
      saveAs: "另存为",
      languageToggle: "Language", languageLabel: "显示语言",
      editorHeading: "编辑 Markdown", editorLabel: "Markdown 文本",
      previewHeading: "预览", previewLabel: "Markdown 的显示效果",
      dividerLabel: "编辑区和预览的宽度",
      dividerTitle: "拖动或按方向键调整左右宽度",
      fullscreenEditor: "编辑区填满窗口", fullscreenPreview: "预览填满窗口",
      fullscreenShort: "填满窗口",
      fullscreenExit: "恢复原来的布局", saved: "已保存", unsaved: "有未保存的更改",
      settingsReaderLang: "语言", readerLangAuto: "自动（按段落判断）", readerLangJa: "日语", readerLangEn: "英语", readerLangZh: "中文（简体）",
      settingsReaderVoice: "语音", readerVoiceAuto: "自动（最先找到的语音）",
      readerNoVoiceFor: (lang) => `这台电脑上没有可用于朗读${{ ja: "日语", en: "英语", zh: "中文" }[lang] ?? ""}的语音。请在 Windows 的“设置”→“时间和语言”→“语音”中添加语音。`,
      openUrl: "打开 URL", urlHeading: "打开 URL",
      urlIntro: "将以 https:// 开头的网页转换为 Markdown 打开。不执行脚本，也不会把文档内容发送到任何地方。页面中的图像从该页面加载。",
      urlLabel: "地址", urlOpen: "打开",
      loadExternal: "加载图像和视频",
      noticeExternal: (count) => `此文档中有 ${count} 个网络上的图像或视频。加载后，这些服务器会得知你的 IP 地址等信息。`,
      externalVideo: (name, host) => `［外部视频${name ? `：${name}` : ""}（${host}），未加载］`,
      videoLabel: (name) => `视频：${name}`, videoStill: (name) => `▶ 视频：${name}`,
      externalImage: (alt, host) => `［外部图像${alt ? `：${alt}` : ""}（${host}），未加载］`,
      settingsInternet: "互联网", settingsExternal: "网络上的图像",
      externalAsk: "确认后再加载（默认）", externalAlways: "始终加载", externalNever: "不加载",
      settingsExternalNote: "是否加载文档中网络上的图像（仅限 https）。加载后，该图像的服务器会得知你的 IP 地址和时间。不执行脚本，也不会发送文档内容。",
      detachPreview: "在单独窗口中显示", attachPreview: "返回主窗口", attachPreviewMain: "将预览返回主窗口",
      pdfStarting: "正在开始导出 PDF",
      findReplace: "查找和替换", findReplaceTitle: "查找和替换 (Ctrl+H)",
      settingsVideo: "视频", settingsVideoAutoplay: "自动播放",
      videoAutoplayAuto: "跟随 Windows 的动画设置（默认）", videoAutoplayAlways: "始终自动播放", videoAutoplayNever: "不自动播放",
      settingsVideoNote: "文档中的视频会静音自动播放并循环（声音和暂停用视频的按钮控制）。默认情况下，当 Windows 的“动画效果”关闭时，不自动播放，而是显示第一帧和播放按钮。",
      settingsMapTiles: "详细地图（OpenStreetMap）",
      mapTilesAsk: "确认后再加载（默认）", mapTilesAlways: "始终加载", mapTilesNever: "不加载（仅简单的世界地图）",
      settingsMapTilesNote: "```geojson 和 ```topojson 的地图会先绘制在电脑中的简单世界地图上。带有道路和地名的详细地图需要从 OpenStreetMap 通过互联网加载，此时地图服务器会得知显示的地点和你的 IP 地址。保存的 HTML 中不包含详细地图。",
      noticeMaps: (count) => `此文档中有 ${count} 个地图。加载详细地图（OpenStreetMap）后，地图服务器会得知显示的地点和你的 IP 地址等信息。`,
      loadMaps: "加载详细地图",
      modelLabel: "3D 模型（STL）。拖动旋转，滚轮缩放", modelEmpty: "没有三角形",
      mapLabel: "地图（GeoJSON）", mapEmpty: "没有可绘制在地图上的图形",
      toc: "目录", tocNoHeadings: "没有可以作为目录的标题。请先写出“## 标题”这样的标题，然后再按一次。",
      dropInsert: "插入到光标位置", dropOpen: "作为文件打开",
      imageButton: "调整图像", imageHeading: "图像的大小和位置",
      imageTarget: (name) => `目标图像：${name}`,
      imageSize: "大小", imageSizeOriginal: "原始大小（不指定）", imageSize25: "正文宽度的 25%", imageSize50: "正文宽度的 50%",
      imageSize75: "正文宽度的 75%", imageSize100: "正文全宽（100%）", imageSizePixels: "按像素指定宽度", imagePixels: "宽度（像素）",
      imagePosition: "位置", imagePositionNone: "不指定（左）", imagePositionCenter: "居中", imagePositionRight: "右",
      imagePositionFloatLeft: "放在左边，文字环绕在右侧", imagePositionFloatRight: "放在右边，文字环绕在左侧",
      imageAloneNote: "此图像所在的行还有其他文字或图像，因此不能选择“居中”和“右”。需要时，请让图像单独占一行。",
      imageApply: "更改",
      imageNote: "将改写为与 GitHub 相同的写法（HTML 的 <img width=\"50%\"> 或 <p align=\"center\">）。恢复为“原始大小”和“不指定”时，会变回 Markdown 的 ![说明](文件)。可以用 Ctrl+Z 撤销。",
      imageNone: "找不到图像。请在编辑区中把光标放在图像（![说明](文件) 或 <img>）所在的行，或在预览中右键单击图像，然后再试。",
      imageMoved: "预览中图像的位置与编辑区不一致。请等预览更新后，再右键单击一次。",
      imagePixelsInvalid: "请输入 1 到 10000 之间的宽度。",
      imageChanged: (size, position) => `已将图像设为“${size}、${position}”。可以用 Ctrl+Z 撤销。`,
      imageSizeWords: { original: "原始大小", pixels: (px) => `宽 ${px} 像素`, percent: (p) => `正文宽度的 ${p}%` },
      imagePositionWords: { none: "不指定位置", center: "居中", right: "右", floatLeft: "放在左边并环绕文字", floatRight: "放在右边并环绕文字" },
      tocUpdated: "已更新目录。", tocInserted: "已在光标位置插入目录。更改标题后，再按一次“目录”即可更新。",
      help: "写法", helpTitle: "Markdown 写法 (F1)", helpHeading: "Markdown 简明写法",
      shortcuts: "按键操作", shortcutHeading: "键盘快捷键",
      brandLogo: "assets/5garashi_logotype_en_outlined.svg", brandLogoDark: "assets/5garashi_logotype_en_reversed_outlined.svg",
      brandLogoAlt: "5garashi.com Design Office",
      settings: "设置", settingsHeading: "设置",
      settingsIntro: "可以根据使用者调整易读性和易用性。下次启动时会保留这些设置。",
      settingsText: "文字（编辑区和预览）", settingsFont: "字体", settingsFontStandard: "标准",
      settingsFontUd: "BIZ UD Gothic（注重易读性）", settingsSize: "文字大小", settingsSize100: "标准",
      settingsSize125: "大（125%）", settingsSize150: "很大（150%）", settingsSize200: "最大（200%）",
      settingsSpacing: "行距", settingsSpacingNormal: "标准", settingsSpacingWide: "较宽", settingsSpacingWider: "很宽",
      settingsThemePaper: "显示主题和 PDF 纸张",
      settingsThemePaperNote: "每按一次切换一次。显示主题按 自动（跟随 Windows）→ 白天 → 夜间 的顺序，纸张按 自动 → A4 → Letter 的顺序切换。也可以不打开设置，用 Ctrl+Shift+T 和 Ctrl+Shift+A 切换。",
      settingsZoom: "整个画面的缩放比例",zoomOut: "缩小", zoomIn: "放大", zoomReset: "恢复为默认值（90%）",
      zoomNote: "也可以用 Ctrl + 鼠标滚轮、Ctrl + 加号／减号、Ctrl + 0 更改。下次启动时会保留缩放比例。",
      settingsFullWidth: "不限制预览正文的宽度（在宽屏上每行更长）",
      settingsEditor: "编辑区", settingsActiveLine: "突出显示当前行", settingsThickCaret: "加粗光标",
      settingsLint: "检查 Markdown 写法（在有问题的地方画波浪线）",
      lintStatus: (count) => `写法问题 ${count} 个（按下显示列表）`,
      lintPhrases: {"Diagnostics": "写法检查", "No diagnostics": "没有发现问题", "close": "关闭"},
      lint: {"MD001": "标题级别跳跃了（例如 # 之后是 ###）。请逐级降低。", "MD011": "链接的写法反了。请按 [文字](地址) 的顺序书写。", "MD018": "标题的 # 后面没有空格（#标题 → # 标题）。", "MD023": "标题没有从行首开始。请删除前面的空格。", "MD024": "同一级别中有同名的标题，目录和链接无法区分。", "MD037": "强调符号（* 或 _）内侧有空格，不会显示为强调。", "MD038": "代码（`）内侧的前后有空格。", "MD039": "链接文字的前后有空格。", "MD042": "链接目标为空。", "MD045": "图像没有说明（替代文字）。写成 ![说明](…)，屏幕阅读器也能读出。", "MD051": "找不到页内链接（#…）指向的标题。", "MD052": "引用链接 [文字][名称] 的名称没有定义。", "MD056": "表格这一行的列数与标题行不同。", "MD058": "表格前后没有空行，可能不会显示为表格。", "fence": "代码块（``` 或 ~~~）没有结束，文档末尾之前都会成为代码。", "missing": "找不到链接的文件："},
      settingsReader: "朗读", settingsReaderOn: "使用朗读功能（在预览的标题栏显示操作按钮）",
      settingsReaderRate: "速度", rateSlow: "慢", rateSlower: "稍慢", rateNormal: "正常",
      rateFaster: "稍快", rateFast: "快", settingsReaderPause: "段落间隔", pauseNone: "不停顿",
      pauseShort: "0.5 秒", pauseMedium: "1 秒", pauseLong: "2 秒",
      readerVoices: (ja, en, zh) => `使用这台电脑的语音朗读（日语：${ja}，英语：${en}，中文：${zh}）。不会把文字发送到外部。`,
      readerNoVoiceShort: "无",
      readerNoVoice: "这台电脑上没有可用于朗读的语音。请在 Windows 的“设置”→“时间和语言”→“语音”中添加语音。",
      readerPlay: "朗读", readerPause: "暂停", readerResume: "继续", readerStop: "停止",
      settingsBackup: "自动备份", settingsBackupOn: "自动备份正在编辑的文档，异常退出时恢复",
      settingsBackupNote: "每 15 秒将未保存的更改备份到这台电脑中（%LOCALAPPDATA%\\mdEditor\\Recovery）。不会改写原文件。保存或正常退出后，备份会被删除。",
      settingsReset: "将显示设置恢复为默认",
      settingsPasteKey: "从剪贴板新建", pasteKeyLabel: "启动按键", pasteKeyNone: "无",
      pasteKeyNote: "选择按键后，经你确认会在桌面上创建快捷方式。复制文本或图像后按该键，mdEditor 会启动并把内容放入新文档。",
      pasteKeyReady: (key) => `可以使用 ${key}（桌面快捷方式）。请先复制文本或图像，再按该键。选择“无”会删除快捷方式。`,
      pasteKeyMissing: (key) => `在桌面上找不到之前创建的快捷方式（${key}）。重新选择按键即可重新创建。`,
      shortcutIntro:"所有操作都可以用键盘完成。可以用“更改”把按键改为喜欢的组合，用“无”取消。可以分配的是 Ctrl 加字母或数字的组合，或 F 键。",
      shortcutEnabled: "使用键盘快捷键", shortcutAction: "操作", shortcutKeys: "按键",
      shortcutChange: "更改", shortcutRemove: "无", shortcutNone: "（无）",
      shortcutPress: "请按下按键", shortcutReset: "恢复默认",
      shortcutChangeLabel: (name) => `更改“${name}”的按键`,
      shortcutRemoveLabel: (name) => `取消“${name}”的按键`,
      shortcutPressFor: (name) => `请按下要分配给“${name}”的按键。按 Esc 取消。`,
      shortcutCancelled: "已取消更改。",
      shortcutInvalid: "请按 Ctrl 加字母或数字的组合，或 F 键（不能使用 Ctrl+Alt）。",
      shortcutReserved: (keys) => `${keys} 用于编辑文字或查找，不能分配。`,
      shortcutConflict: (keys, name) => `${keys} 已用于“${name}”。请先更改那一项。`,
      shortcutSaved: (name, keys) => `已将“${name}”设为 ${keys}。`,
      shortcutRemoved: (name) => `已取消“${name}”的按键。`,
      shortcutTurnedOn: "已启用键盘快捷键。", shortcutTurnedOff: "已停用键盘快捷键（Esc 和对话框中的操作仍可使用）。",
      shortcutResetDone: "已将所有按键恢复为默认。",
      shortcutNames: {
        open: "打开", restoreDiscarded: "返回上一个文档", save: "保存", saveAs: "另存为", pdf: "保存为 PDF", paper: "切换纸张",
        theme: "切换显示主题", language: "切换显示语言", encoding: "编码",
        toc: "插入或更新目录", findReplace: "查找和替换", findNext: "查找下一个", findPrev: "查找上一个", replaceAll: "全部替换",
        help: "Markdown 写法", shortcuts: "按键操作一览", fullscreenEditor: "编辑区填满窗口",
        fullscreenPreview: "预览填满窗口", nextPane: "移到下一个区域", previousPane: "移到上一个区域",
        windowFullscreen: "窗口全屏显示", settings: "设置", readAloud: "朗读／暂停", stopReading: "停止朗读", html: "保存为 HTML", png: "保存为 PNG", docx: "保存为 Word", openUrl: "打开 URL",
        detachPreview: "在单独窗口中显示预览／返回主窗口", image: "图像的大小和位置"
      },
      helpIntro: "按左边的写法输入，就会显示为右边的样子。按“插入”可以把示例放到编辑区的光标位置。",
      helpItem: "项目", helpSyntax: "写法", helpResult: "显示", helpInsert: "插入",
      helpInsertLabel: (name) => `插入“${name}”的示例`,
      findPlaceholder: "要查找的文字", replacePlaceholder: "替换为",
      findLabel: "查找", replaceLabel: "替换",
      findPrev: "上一个", findNext: "下一个", replaceOne: "替换", replaceAll: "全部替换",
      matchCase: "区分大小写", close: "关闭",
      notFound: "找不到", replacedCount: (n) => `已替换 ${n} 处`,
      themeLabels: { auto: "显示：自动", light: "显示：浅色", dark: "显示：深色" },
      themeTitle: "切换显示主题（自动 → 浅色 → 深色）",
      paperLabels: (paper, auto) => paper === "auto"
        ? `纸张：自动（${auto === "letter" ? "Letter" : "A4"}）`
        : `纸张：${paper === "letter" ? "Letter" : "A4"}`,
      paperTitle: "切换 PDF 的纸张大小（自动 → A4 → Letter）。自动时跟随 Windows 的区域设置",
      encodingLabels: {
        "utf-8": "UTF-8", "utf-8-bom": "UTF-8（带 BOM）", "utf-16le": "UTF-16 LE", "utf-16be": "UTF-16 BE",
        "shift_jis": "Shift_JIS", "euc-jp": "EUC-JP", "iso-2022-jp": "ISO-2022-JP（JIS）", "windows-1252": "Windows-1252（西欧）"
      },
      encodingGuessed: (label) => `${label}（推测）`,
      encodingButtonLabel: (label) => `编码：${label}。按下可以选择编码`,
      encodingButtonTitle: "查看或更改编码",
      encodingHeading: "编码", encodingLegend: "选择编码",
      encodingCurrent: (label) => `当前编码：${label}`,
      encodingReopen: "用此编码重新打开", encodingSave: "从下次保存起使用此编码",
      encodingHelp: "出现乱码时，请选择“用此编码重新打开”。只想改变保存格式而不改变内容时，请选择“从下次保存起使用此编码”。",
      noticeGuessed: (label) => `此文档不是 UTF-8，因此推测为 ${label} 打开。如果出现乱码，请在保存前重新选择编码。`,
      noticeLossy: (label) => `有用 ${label} 无法读取的字符，已显示为“�”。保存前，请选择正确的编码重新打开。`,
      noticeChoose: "选择编码",
      diagramError: "无法显示图表", diagramUnavailable: "无法加载图表显示功能",
      ratio: (e, p) => `编辑区 ${e}%，预览 ${p}%`
    },
    en: {
      toolbar: "Document actions",
      sample: "Samples", sampleHeading: "Open a sample",
      sampleIntro: "Try a document with diagrams and math. Besides the short examples, there are long tests of every supported diagram, chart, and formula (Japanese, English, and Chinese). Nothing is inserted until you choose one.",
      settingsPdf: "Opening PDFs",
      settingsPdfFigureText: "Text around figures",
      pdfFigureTextPicture: "Put in the picture (looks like the page, default)",
      pdfFigureTextText: "Keep as text (can be edited)",
      settingsPdfFigureTextNote: "Choose whether text that reaches into a figure (a description beside an illustration, say) goes into the figure's picture, keeping the look, or stays text you can edit. After a PDF opens, the button in the notice bar opens it again the other way.",
      settingsPdfAlign: "Keep right-aligned and centered text in place, written as HTML (align)",
      settingsPdfPagePictures: "Make a page whose text cannot be read (vertical writing, garbled fonts, text drawn as shapes) one picture of the whole page",
      settingsPdfFile: "Open detailed values",
      settingsPdfFileNote: "The finer values of PDF import (how close lines must be to make a figure, how long a label may be, where text counts as right-aligned, and so on) are in an advanced settings file, opened in Notepad. Changed values are used from the next PDF you open. File:",
      settingsExport: "Export",
      settingsMarkdown: "Markdown syntax",
      settingsNearGitHub: "Show documents closer to GitHub (no mdEditor-only syntax)",
      settingsNearGitHubNote: "When on, mdEditor's own syntax that GitHub does not have (x^2^ superscript, H~2~O subscript, ==highlight==, ++insert++, definition lists, ::: containers, attributes such as {#id}, abbreviations, grid tables, and a. numbering) shows as the plain text you wrote, as on GitHub, and ~text~ is struck through. Exports such as PDF follow it too. This brings the display closer to GitHub but does not make it identical (diagrams, maps, 3D models, code colors, and how math is drawn stay mdEditor's own).",
      settingsHtmlSource: "Keep the original Markdown in saved HTML (opens back exactly in mdEditor)",
      settingsHtmlSourceNote: "The original text goes into the HTML file as it is, including comments (<!-- -->) that are never shown, so check it before you share the file. Without it, mdEditor still opens the HTML by converting it to Markdown (colors, layout, and drawn diagrams do not come back).",
      sampleLoadFailed: "The sample could not be loaded. Reinstall mdEditor, then try again.",
      sampleOpen: "Open", sampleOpenLabel: (name) => `Open the sample "${name}" as a new document`,
      about: "About", aboutHeading: "About this app", aboutVersion: "Version",
      aboutIntro: "mdEditor is a Markdown editor, previewer, and PDF exporter made by 5garashi.com Design Office.",
      aboutCompany: "About 5garashi.com Design Office", aboutWorks: "Works and other apps",
      aboutSupport: "Support and contact", aboutPrivacy: "Privacy policy",
      aboutNote: "Links open in your browser only when you choose them. The app shows no ads at startup and never inserts promotions into PDFs.",
      open: "Open", openTitle: "Open (Ctrl+O)", save: "Save", saveTitle: "Save (Ctrl+S)",
      restoreDiscarded: "Back to previous document",
      restoreDiscardedTitle: (name) => `Goes back to the previous document, ${name}. Changes it had that were not saved come back as they were`,
      saveAs: "Save As",
      languageToggle: "Language", languageLabel: "Display language",
      editorHeading: "Edit Markdown", editorLabel: "Markdown text",
      previewHeading: "Preview", previewLabel: "Rendered Markdown",
      dividerLabel: "Width of the editor and preview",
      dividerTitle: "Drag or use the arrow keys to resize",
      fullscreenEditor: "Fill window with editor", fullscreenPreview: "Fill window with preview",
      fullscreenShort: "Fill window",
      fullscreenExit: "Restore layout", saved: "Saved", unsaved: "Unsaved changes",
      settingsReaderLang: "Language", readerLangAuto: "Automatic (per paragraph)", readerLangJa: "Japanese", readerLangEn: "English", readerLangZh: "Chinese (Simplified)",
      settingsReaderVoice: "Voice", readerVoiceAuto: "Automatic (first voice found)",
      readerNoVoiceFor: (lang) => `No ${{ ja: "Japanese", en: "English", zh: "Chinese" }[lang] ?? ""} voice for read-aloud is installed on this PC. Add one in Windows Settings > Time & language > Speech.`,
      openUrl: "Open URL", urlHeading: "Open a URL",
      urlIntro: "Opens a web page that starts with https:// as Markdown. Scripts do not run, and nothing about the document is sent anywhere. Images on the page load from that page.",
      urlLabel: "Address", urlOpen: "Open",
      loadExternal: "Load images and videos",
      noticeExternal: (count) => `This document has ${count} image(s) or video(s) on the internet. Loading them lets those servers see information such as your IP address.`,
      externalVideo: (name, host) => `[Internet video${name ? `: ${name}` : ""} (${host}), not loaded]`,
      videoLabel: (name) => `Video: ${name}`, videoStill: (name) => `▶ Video: ${name}`,
      externalImage: (alt, host) => `[Internet image${alt ? `: ${alt}` : ""} (${host}), not loaded]`,
      settingsInternet: "Internet", settingsExternal: "Images on the internet",
      externalAsk: "Ask before loading (default)", externalAlways: "Always load", externalNever: "Never load",
      settingsExternalNote: "Whether images in a document that are on the internet (https only) load. Loading one lets its server see your IP address and the time. Scripts never run, and the document's content is never sent.",
      detachPreview: "Open in its own window", attachPreview: "Return to main window", attachPreviewMain: "Return preview here",
      pdfStarting: "Starting PDF export",
      findReplace: "Find/Replace", findReplaceTitle: "Find/Replace (Ctrl+H)",
      settingsVideo: "Videos", settingsVideoAutoplay: "Autoplay",
      videoAutoplayAuto: "Follow the Windows animation setting (default)", videoAutoplayAlways: "Always play automatically", videoAutoplayNever: "Never play automatically",
      settingsVideoNote: "Videos in a document start by themselves without sound and loop (the video's controls turn the sound on or pause it). By default, when Windows animation effects are off, they do not start and show the first frame with a play button instead.",
      settingsMapTiles: "Detailed maps (OpenStreetMap)",
      mapTilesAsk: "Ask before loading (default)", mapTilesAlways: "Always load", mapTilesNever: "Never load (simple world map only)",
      settingsMapTilesNote: "```geojson and ```topojson maps are first drawn on a simple world map on your PC. A detailed map with roads and place names comes from OpenStreetMap over the internet, which lets the map server see the area shown and your IP address. It is not kept in saved HTML.",
      noticeMaps: (count) => `This document has ${count} map(s). Loading detailed maps (OpenStreetMap) lets the map server see the area shown and information such as your IP address.`,
      loadMaps: "Load detailed maps",
      modelLabel: "3D model (STL). Drag to turn, use the wheel to zoom", modelEmpty: "There are no triangles",
      mapLabel: "Map (GeoJSON)", mapEmpty: "There are no shapes to draw on the map",
      toc: "Contents", tocNoHeadings: "There are no headings for a table of contents. Write headings such as \"## Heading\", then press it again.",
      dropInsert: "Insert at the cursor", dropOpen: "Open as a file",
      imageButton: "Adjust image", imageHeading: "Image size and position",
      imageTarget: (name) => `Image: ${name}`,
      imageSize: "Size", imageSizeOriginal: "Original size (not set)", imageSize25: "25% of the text width", imageSize50: "50% of the text width",
      imageSize75: "75% of the text width", imageSize100: "Full text width (100%)", imageSizePixels: "Width in pixels", imagePixels: "Width (pixels)",
      imagePosition: "Position", imagePositionNone: "Not set (left)", imagePositionCenter: "Center", imagePositionRight: "Right",
      imagePositionFloatLeft: "Left, with text flowing on its right", imagePositionFloatRight: "Right, with text flowing on its left",
      imageAloneNote: "Center and Right are not available because this image shares its line with other text or images. Put the image on a line of its own to use them.",
      imageApply: "Change",
      imageNote: "The image is rewritten the way GitHub does it (HTML such as <img width=\"50%\"> and <p align=\"center\">). Choosing Original size and Not set turns it back into Markdown, ![description](file). Ctrl+Z undoes the change.",
      imageNone: "No image was found. Put the cursor on a line with an image (![description](file) or <img>) in the editor, or right-click an image in the preview, then try again.",
      imageMoved: "The image in the preview no longer matches the editor. Wait for the preview to update, then right-click it again.",
      imagePixelsInvalid: "Enter a width from 1 to 10000.",
      imageChanged: (size, position) => `The image is now ${size}, ${position}. Ctrl+Z undoes it.`,
      imageSizeWords: { original: "original size", pixels: (px) => `${px} pixels wide`, percent: (p) => `${p}% of the text width` },
      imagePositionWords: { none: "position not set", center: "centered", right: "on the right", floatLeft: "on the left with text flowing", floatRight: "on the right with text flowing" },
      tocUpdated: "The table of contents was updated.", tocInserted: "A table of contents was inserted at the cursor. After changing headings, press Contents again to update it.",
      help: "Syntax", helpTitle: "Markdown syntax (F1)", helpHeading: "Markdown quick reference",
      shortcuts: "Keys", shortcutHeading: "Keyboard shortcuts",
      brandLogo: "assets/5garashi_logotype_en_outlined.svg", brandLogoDark: "assets/5garashi_logotype_en_reversed_outlined.svg",
      brandLogoAlt: "5garashi.com Design Office",
      settings: "Settings", settingsHeading: "Settings",
      settingsIntro: "Adjust readability and ease of use to suit you. Settings are remembered.",
      settingsText: "Text (editor and preview)", settingsFont: "Typeface", settingsFontStandard: "Standard",
      settingsFontUd: "BIZ UD Gothic (designed for legibility)", settingsSize: "Text size", settingsSize100: "Standard",
      settingsSize125: "Large (125%)", settingsSize150: "Larger (150%)", settingsSize200: "Largest (200%)",
      settingsSpacing: "Line spacing", settingsSpacingNormal: "Standard", settingsSpacingWide: "Wide", settingsSpacingWider: "Wider",
      settingsThemePaper: "Theme and PDF paper",
      settingsThemePaperNote: "Each press switches to the next choice: the theme goes Auto (follows Windows), Light, Dark, and the paper Auto, A4, Letter. Ctrl+Shift+T and Ctrl+Shift+A switch them without opening Settings.",
      settingsZoom: "Zoom (whole window)",zoomOut: "Zoom out", zoomIn: "Zoom in", zoomReset: "Reset to default (90%)",
      zoomNote: "Ctrl + mouse wheel, Ctrl + plus/minus, and Ctrl + 0 also work. The zoom level is remembered.",
      settingsFullWidth: "Don't limit the preview text width (longer lines on wide screens)",
      settingsEditor: "Editor", settingsActiveLine: "Highlight the current line", settingsThickCaret: "Use a thick cursor",
      settingsLint: "Check how Markdown is written (underline problems)",
      lintStatus: (count) => `${count} Markdown problem${count === 1 ? "" : "s"} (press to list them)`,
      lintPhrases: {},
      lint: {"MD001": "A heading level is skipped (### after #, for example). Go down one level at a time.", "MD011": "The link is written the wrong way round. Write [text](address).", "MD018": "There is no space after the heading's # (#Heading → # Heading).", "MD023": "The heading does not start at the beginning of the line. Remove the spaces before it.", "MD024": "Another heading at the same level has the same name; contents and links cannot tell them apart.", "MD037": "There are spaces inside the emphasis marks (* or _), so the text is not emphasized.", "MD038": "There are spaces just inside the code marks (`).", "MD039": "There are spaces around the link text.", "MD042": "The link goes nowhere.", "MD045": "The image has no description (alternative text). Write ![description](…) so screen readers can say what it shows.", "MD051": "The heading this link on the page (#…) points to cannot be found.", "MD052": "The name of the reference link [text][name] is not defined anywhere.", "MD056": "This row of the table has a different number of columns from the header row.", "MD058": "There is no empty line before or after the table, so it may not show as a table.", "fence": "The code block (``` or ~~~) is never closed; everything to the end becomes code.", "missing": "The linked file cannot be found: "},
      settingsReader: "Read aloud", settingsReaderOn: "Use read-aloud (shows controls in the preview heading)",
      settingsReaderRate: "Speed", rateSlow: "Slow", rateSlower: "A little slow", rateNormal: "Normal",
      rateFaster: "A little fast", rateFast: "Fast", settingsReaderPause: "Pause between paragraphs", pauseNone: "No pause",
      pauseShort: "0.5 seconds", pauseMedium: "1 second", pauseLong: "2 seconds",
      readerVoices: (ja, en, zh) => `Reads with voices on this PC (Japanese: ${ja}, English: ${en}, Chinese: ${zh}). Text is never sent anywhere.`,
      readerNoVoiceShort: "none",
      readerNoVoice: "No voice for read-aloud is installed on this PC. Add one in Windows Settings > Time & language > Speech.",
      readerPlay: "Read aloud", readerPause: "Pause", readerResume: "Resume", readerStop: "Stop",
      settingsBackup: "Automatic backup", settingsBackupOn: "Back up the open document automatically and restore it after a crash",
      settingsBackupNote: "Unsaved changes are copied every 15 seconds to this PC (%LOCALAPPDATA%\\mdEditor\\Recovery). The original file is never changed. The copy is removed when you save or close normally.",
      settingsReset: "Restore display defaults",
      settingsPasteKey: "New from clipboard", pasteKeyLabel: "Start key", pasteKeyNone: "None",
      pasteKeyNote: "Choosing a key creates a desktop shortcut once you confirm. Copy text or a picture, then press the key: mdEditor starts and puts it in a new document.",
      pasteKeyReady: (key) => `${key} is ready (desktop shortcut). Copy text or a picture first, then press it. Choosing None deletes the shortcut.`,
      pasteKeyMissing: (key) => `The shortcut made earlier (${key}) is no longer on the desktop. Choose a key again to make it again.`,
      shortcutIntro:"Every command can be used from the keyboard. Change a key with Change, or remove it with None. Keys can be Ctrl with a letter or digit, or an F key.",
      shortcutEnabled: "Use keyboard shortcuts", shortcutAction: "Command", shortcutKeys: "Key",
      shortcutChange: "Change", shortcutRemove: "None", shortcutNone: "(none)",
      shortcutPress: "Press a key", shortcutReset: "Restore defaults",
      shortcutChangeLabel: (name) => `Change the key for ${name}`,
      shortcutRemoveLabel: (name) => `Remove the key for ${name}`,
      shortcutPressFor: (name) => `Press the key for ${name}. Esc cancels.`,
      shortcutCancelled: "Change cancelled.",
      shortcutInvalid: "Press Ctrl with a letter or digit, or an F key (Ctrl+Alt is not allowed).",
      shortcutReserved: (keys) => `${keys} is used for editing or find and cannot be assigned.`,
      shortcutConflict: (keys, name) => `${keys} is already used for ${name}. Change that one first.`,
      shortcutSaved: (name, keys) => `${name} is now ${keys}.`,
      shortcutRemoved: (name) => `Removed the key for ${name}.`,
      shortcutTurnedOn: "Keyboard shortcuts are on.", shortcutTurnedOff: "Keyboard shortcuts are off (Esc and keys inside dialogs still work).",
      shortcutResetDone: "All keys are back to their defaults.",
      shortcutNames: {
        open: "Open", restoreDiscarded: "Back to previous document", save: "Save", saveAs: "Save As", pdf: "Save as PDF", paper: "Switch paper size",
        theme: "Switch theme", language: "Switch language", encoding: "Text encoding",
        toc: "Insert or update the table of contents", findReplace: "Find/Replace", findNext: "Find next", findPrev: "Find previous", replaceAll: "Replace all",
        help: "Markdown syntax", shortcuts: "Keyboard shortcuts", fullscreenEditor: "Fill window with editor",
        fullscreenPreview: "Fill window with preview", nextPane: "Next pane", previousPane: "Previous pane",
        windowFullscreen: "Window full screen", settings: "Settings", readAloud: "Read aloud / pause", stopReading: "Stop reading", html: "Save as HTML", png: "Save as PNG", docx: "Save as Word", openUrl: "Open URL",
        detachPreview: "Preview in its own window / return", image: "Image size and position"
      },
      helpIntro: "Type the syntax on the left to get the result on the right. Insert puts the example at the cursor in the editor.",
      helpItem: "Item", helpSyntax: "Syntax", helpResult: "Result", helpInsert: "Insert",
      helpInsertLabel: (name) => `Insert the ${name} example`,
      findPlaceholder: "Text to find", replacePlaceholder: "Replace with",
      findLabel: "Find", replaceLabel: "Replace",
      findPrev: "Previous", findNext: "Next", replaceOne: "Replace", replaceAll: "Replace all",
      matchCase: "Match case", close: "Close",
      notFound: "Not found", replacedCount: (n) => `Replaced ${n}`,
      themeLabels: { auto: "Theme: Auto", light: "Theme: Light", dark: "Theme: Dark" },
      themeTitle: "Switch the theme (Auto, Light, Dark)",
      paperLabels: (paper, auto) => paper === "auto"
        ? `Paper: Auto (${auto === "letter" ? "Letter" : "A4"})`
        : `Paper: ${paper === "letter" ? "Letter" : "A4"}`,
      paperTitle: "Switch the PDF paper size (Auto, A4, Letter). Auto follows the Windows region",
      encodingLabels: {
        "utf-8": "UTF-8", "utf-8-bom": "UTF-8 with BOM", "utf-16le": "UTF-16 LE", "utf-16be": "UTF-16 BE",
        "shift_jis": "Shift_JIS", "euc-jp": "EUC-JP", "iso-2022-jp": "ISO-2022-JP (JIS)", "windows-1252": "Windows-1252 (Western)"
      },
      encodingGuessed: (label) => `${label} (guessed)`,
      encodingButtonLabel: (label) => `Encoding: ${label}. Press to choose an encoding`,
      encodingButtonTitle: "Check or change the text encoding",
      encodingHeading: "Text encoding", encodingLegend: "Choose an encoding",
      encodingCurrent: (label) => `Current encoding: ${label}`,
      encodingReopen: "Reopen in this encoding", encodingSave: "Use this encoding from the next save",
      encodingHelp: "If the text looks garbled, choose Reopen in this encoding. To keep the text and change only how it is saved, choose Use this encoding from the next save.",
      noticeGuessed: (label) => `This document is not UTF-8, so it was opened as ${label} (a guess). If the text looks garbled, choose another encoding before saving.`,
      noticeLossy: (label) => `Some characters cannot be read as ${label} and are shown as "�". Choose the right encoding and reopen the file before saving.`,
      noticeChoose: "Choose encoding",
      diagramError: "Unable to render the diagram", diagramUnavailable: "The diagram renderer could not be loaded",
      ratio: (e, p) => `Editor ${e}%, preview ${p}%`
    }
  };
  // The display languages, in the order of the Language list. "zh" is Simplified
  // Chinese; the page's lang attribute is zh-CN so Chinese typefaces are used.
  const languages = ["ja", "en", "zh"];
  const htmlLang = { ja: "ja", en: "en", zh: "zh-CN" };
  const normalizeLanguage = (value) => {
    const code = (value || "").toLowerCase();
    return code.startsWith("ja") ? "ja" : code.startsWith("zh") ? "zh" : code.startsWith("en") ? "en" : null;
  };
  let language = normalizeLanguage(localStorage.getItem("mdeditor-language"))
    ?? (normalizeLanguage(navigator.language) || "en");
  const t = (key) => messages[language][key];

  // "auto" follows the Windows app mode; the host applies the choice to WebView2,
  // so prefers-color-scheme (CSS, logos, Mermaid) switches with it.
  const themeOrder = ["auto", "light", "dark"];
  let theme = "auto";
  const updateThemeButton = () => {
    themeButton.textContent = t("themeLabels")[theme];
  };

  const statusText = () => (documentDirty ? t("unsaved") : t("saved"));

  const applyLanguage = () => {
    document.documentElement.lang = htmlLang[language];
    for (const element of document.querySelectorAll("[data-i18n]")) {
      element.textContent = t(element.dataset.i18n);
    }
    for (const element of document.querySelectorAll("[data-i18n-attr]")) {
      for (const pair of element.dataset.i18nAttr.split(";")) {
        const [attribute, key] = pair.split(":");
        element.setAttribute(attribute, t(key));
      }
    }
    languageSelect.value = language;
    updateThemeButton();
    view.dispatch({ effects: editorAttributes.reconfigure(editorAttributeValues()) });
    applyShortcutHints();
    updateReaderButtons();
    updateVoiceNote();
    updatePaperButton();
    updateEncodingButton();
    updateNotice();
    saveState.textContent = pdfStarting ? t("pdfStarting") : statusText();
    setEditorRatio(editorRatio, false);
    setDisplayMode(displayMode);
    updateDetachButtons();
    // The typeface follows the UI language, so redraw diagrams with it.
    if (lastPreviewHtml !== null) renderPreview(lastPreviewHtml);
  };

  if (!Number.isFinite(editorRatio)) {
    editorRatio = 50;
  }
  editorRatio = Math.min(80, Math.max(20, editorRatio));

  const post = (message) => window.chrome.webview.postMessage(message);

  // The editor is CodeMirror 6 rather than a textarea: it keeps only the visible
  // lines in the DOM, so screen readers stay responsive on long documents (a
  // textarea made Narrator stall for 20+ seconds on each selection change).
  const { Compartment, EditorSelection, EditorState, EditorView, closeLintPanel, defaultKeymap, drawSelection, forceLinting, highlightActiveLine, history, historyKeymap, keymap, linter, openLintPanel } = window.CodeMirror;
  const displayDefaults = {
    font: "standard", size: "1", spacing: "1.7", fullWidth: false, htmlSource: false, activeLine: false, thickCaret: false, lint: true,
    reader: false, rate: "1", pause: "0.5", readerLang: "auto", readerVoice: "", mapBackground: "ask", videoAutoplay: "auto"
  };
  const displayStorageKey = "mdeditor-display";
  let displaySettings = (() => {
    try {
      return { ...displayDefaults, ...JSON.parse(localStorage.getItem(displayStorageKey) || "{}") };
    } catch {
      return { ...displayDefaults };
    }
  })();
  const saveDisplaySettings = () => {
    try { localStorage.setItem(displayStorageKey, JSON.stringify(displaySettings)); } catch { /* kept for this session */ }
  };
  // Markdown check: markdownlint (loaded the first time it is needed) with the rules that
  // point at mistakes rather than at style, plus two checks of mdEditor's own: a code fence
  // never closed, and a linked file that is not there (the host looks, beside the document).
  // Problems are underlined, and the strip under the editor lists them. Rules that would flag what mdEditor itself
  // writes (HTML, long lines, several top headings in an imported PDF) are left out.
  const lintRules = {
    default: false, MD001: true, MD011: true, MD018: true, MD023: true, MD024: { siblings_only: true }, MD037: true, MD038: true,
    MD039: true, MD042: true, MD045: true, MD051: true, MD052: true, MD056: true, MD058: true
  };
  // Shown as hints rather than warnings: worth knowing, often on purpose.
  const lintHints = new Set(["MD024", "MD038", "MD045"]);
  let markdownlintLoading = null;
  const loadMarkdownlint = () => markdownlintLoading ??= loadScript("lib/markdownlint/markdownlint.min.js").then(() => window.markdownlint);
  let linkCheckId = 0;
  const linkChecks = new Map();
  // The text with code blanked out (fenced blocks and `code` spans, kept the same length),
  // so examples written as code are not checked as links.
  const withoutCode = (text) => {
    let fence = null;
    const lines = text.split("\n").map((line) => {
      const mark = /^ {0,3}(`{3,}|~{3,})/.exec(line)?.[1];
      if (fence) {
        if (mark && mark[0] === fence[0] && mark.length >= fence.length && /^ {0,3}[`~]+\s*$/.test(line)) fence = null;
        return " ".repeat(line.length);
      }
      if (mark) { fence = mark; return " ".repeat(line.length); }
      return line;
    });
    return lines.join("\n").replace(/(`+)[^`\n]*?\1/g, (code) => " ".repeat(code.length));
  };
  // Relative links and pictures, which the host checks are there: [text](path), ![](path),
  // and src="path" / href="path" in HTML. Addresses, mail links, and #anchors are left out.
  const localTargets = (text) => {
    text = withoutCode(text);
    const targets = [];
    const pattern = /\]\(\s*<?([^)\s>]+)>?(?:\s+["'(][^)]*)?\)|\b(?:src|href)\s*=\s*["']([^"']+)["']/g;
    for (const match of text.matchAll(pattern)) {
      const target = match[1] ?? match[2];
      if (!target || /^([a-z][a-z0-9+.-]*:|#|\/\/)/i.test(target)) continue;
      const at = match.index + match[0].indexOf(target);
      targets.push({ target, from: at, to: at + target.length });
    }
    return targets;
  };
  const missingTargets = (targets) => new Promise((resolve) => {
    if (targets.length === 0) { resolve([]); return; }
    const id = ++linkCheckId;
    linkChecks.set(id, resolve);
    post({ type: "checkLinks", id, targets: [...new Set(targets.map((item) => item.target))] });
    setTimeout(() => { if (linkChecks.delete(id)) resolve([]); }, 5000);
  });
  // The first code fence that is never closed, as its line, or null.
  const unclosedFence = (doc) => {
    let open = null;
    for (let number = 1; number <= doc.lines; number++) {
      const text = doc.line(number).text;
      const fence = /^ {0,3}(`{3,}|~{3,})/.exec(text);
      if (!fence) continue;
      if (!open) open = { mark: fence[1], number };
      else if (fence[1][0] === open.mark[0] && fence[1].length >= open.mark.length && /^ {0,3}[`~]+\s*$/.test(text)) open = null;
    }
    return open && doc.line(open.number);
  };
  const decodeTarget = (value) => { try { return decodeURIComponent(value); } catch { return value; } };
  const markdownDiagnostics = async (editor) => {
    const doc = editor.state.doc;
    const text = doc.toString();
    const messages = t("lint");
    const diagnostics = [];
    try {
      const markdownlint = await loadMarkdownlint();
      const results = markdownlint.lint({ strings: { document: text }, config: lintRules }).document ?? [];
      for (const result of results) {
        const rule = result.ruleNames[0];
        if (result.lineNumber > doc.lines) continue;
        const line = doc.line(result.lineNumber);
        const from = result.errorRange ? Math.min(line.from + result.errorRange[0] - 1, line.to) : line.from;
        const to = result.errorRange ? Math.min(from + result.errorRange[1], line.to) : line.to;
        diagnostics.push({ from, to: Math.max(to, from), severity: lintHints.has(rule) ? "info" : "warning", source: rule, message: messages[rule] ?? result.ruleDescription });
      }
    } catch {
      // Without markdownlint the checks of mdEditor's own still run.
    }
    const fence = unclosedFence(doc);
    if (fence) diagnostics.push({ from: fence.from, to: fence.to, severity: "warning", message: messages.fence });
    const targets = localTargets(text);
    const missing = new Set(await missingTargets(targets));
    for (const item of targets) {
      if (missing.has(item.target)) diagnostics.push({ from: item.from, to: item.to, severity: "warning", message: messages.missing + decodeTarget(item.target) });
    }
    showLintCount(diagnostics.length);
    return diagnostics;
  };
  // The strip under the editor says how many problems there are; it opens the list.
  let lintCount = 0;
  const showLintCount = (count) => {
    lintCount = count;
    const status = document.getElementById("lint-status");
    status.textContent = t("lintStatus")(count);
    status.hidden = count === 0 || !displaySettings.lint;
  };
  const markdownCheck = () => linter(markdownDiagnostics, { delay: 800 });

  // Optional editor aids: a highlighted current line, a drawn (thicker) cursor, and the
  // Markdown check.
  const editorExtras = new Compartment();
  const editorExtrasFor = (settings) => [
    settings.activeLine ? highlightActiveLine() : [],
    settings.thickCaret ? drawSelection() : [],
    settings.lint ? markdownCheck() : []
  ];
  const editorAttributes = new Compartment();
  // The editor's label, and the words of the problem list in the page's language.
  const editorAttributeValues = () => [
    EditorView.contentAttributes.of({
      "aria-label": t("editorLabel"), spellcheck: "false", autocorrect: "off", autocapitalize: "off"
    }),
    EditorState.phrases.of(t("lintPhrases"))
  ];
  const editorExtensions = () => [
    history(),
    keymap.of([...defaultKeymap, ...historyKeymap]),
    EditorView.lineWrapping,
    editorAttributes.of(editorAttributeValues()),
    editorExtras.of(editorExtrasFor(displaySettings)),
    // Every edit goes to the host, which tracks the text and the unsaved state
    // (edits the host itself asked for, such as links rewritten on Save As, are marked).
    EditorView.updateListener.of((update) => {
      if (update.docChanged) post({ type: "change", text: update.state.doc.toString(), host: applyingHostEdits });
    })
  ];
  const view = new EditorView({ parent: editorHost, state: EditorState.create({ doc: "", extensions: editorExtensions() }) });
  const editorScroller = view.scrollDOM;
  const selectRange = (from, to) => {
    view.dispatch({
      selection: EditorSelection.single(from, to),
      effects: EditorView.scrollIntoView(from, { y: "center" })
    });
    view.focus();
  };

  const setEditorRatio = (ratio, persist = true) => {
    editorRatio = Math.min(80, Math.max(20, ratio));
    const firstTrack = `minmax(0, ${editorRatio}fr)`;
    const lastTrack = `minmax(0, ${100 - editorRatio}fr)`;

    if (compactLayout.matches) {
      workspace.style.gridTemplateColumns = "";
      workspace.style.gridTemplateRows = `${firstTrack} 44px ${lastTrack}`;
      divider.setAttribute("aria-orientation", "horizontal");
    } else {
      workspace.style.gridTemplateRows = "";
      workspace.style.gridTemplateColumns = `${firstTrack} 44px ${lastTrack}`;
      divider.setAttribute("aria-orientation", "vertical");
    }

    divider.setAttribute("aria-valuenow", String(editorRatio));
    divider.setAttribute(
      "aria-valuetext",
      t("ratio")(editorRatio, 100 - editorRatio)
    );
    if (persist) {
      localStorage.setItem("mdeditor-editor-ratio", String(editorRatio));
    }
  };

  const setDisplayMode = (mode) => {
    displayMode = mode;
    document.body.classList.toggle("editor-focused", mode === "editor");
    document.body.classList.toggle("preview-focused", mode === "preview");
    if (mode === "preview" || mode === "editor") {
      workspace.style.gridTemplateColumns = "";
      workspace.style.gridTemplateRows = "";
    } else {
      setEditorRatio(editorRatio, false);
    }

    for (const button of fullscreenButtons) {
      const buttonMode = button.dataset.fullscreenMode;
      const active = mode === buttonMode;
      // The two buttons show the same short text (their pane tells them apart); screen
      // readers get the full name, which contains the shown text (WCAG 2.5.3).
      button.textContent = active ? t("fullscreenExit") : t("fullscreenShort");
      if (active) {
        button.removeAttribute("aria-label");
      } else {
        button.setAttribute("aria-label", buttonMode === "editor" ? t("fullscreenEditor") : t("fullscreenPreview"));
      }
      button.setAttribute("aria-pressed", String(active));
    }
  };

  const getPointerRatio = (event) => {
    const bounds = workspace.getBoundingClientRect();
    const padding = window.getComputedStyle(workspace);
    const compact = compactLayout.matches;
    const dividerSize = compact ? divider.offsetHeight : divider.offsetWidth;
    const startPadding = Number.parseFloat(
      compact ? padding.paddingTop : padding.paddingLeft
    ) || 0;
    const endPadding = Number.parseFloat(
      compact ? padding.paddingBottom : padding.paddingRight
    ) || 0;
    const extent = (compact ? bounds.height : bounds.width) - startPadding - endPadding - dividerSize;
    if (extent <= 0) {
      return editorRatio;
    }

    const start = compact ? bounds.top : bounds.left;
    const position = (compact ? event.clientY : event.clientX) - start - startPadding;
    return (position / extent) * 100;
  };

  document.getElementById("open-button").addEventListener("click", () => post({ type: "open" }));
  // Shown only while a document closed with "No" to saving can be brought back.
  const restoreButton = document.getElementById("restore-button");
  let discardedName = null;
  const updateRestoreButton = () => {
    restoreButton.hidden = discardedName === null;
    restoreButton.title = discardedName === null ? "" : t("restoreDiscardedTitle")(discardedName);
  };
  restoreButton.addEventListener("click", () => post({ type: "restoreDiscarded" }));
  document.getElementById("save-button").addEventListener("click", () => post({ type: "save" }));
  // Save As lists every format (Markdown, text, PDF, HTML, PNG, Word), like Open.
  // Keeping the original Markdown in the HTML is a choice in Settings (off by default).
  const saveAs = () => post({ type: "saveAs", embedSource: Boolean(displaySettings.htmlSource) });
  document.getElementById("save-as-button").addEventListener("click", () => saveAs());
  const exportPdf = () => {
    pdfStarting = true;
    saveState.textContent = t("pdfStarting");
    post({ type: "exportPdf" });
  };
  const exportHtml = () => post({ type: "exportHtml", embedSource: Boolean(displaySettings.htmlSource) });

  // PNG and Word output: the preview alone at a fixed width in the day
  // palette, so the host can capture it (and each diagram or formula) as pictures.
  const openAllDetails = () => {
    for (const details of preview.querySelectorAll("details")) details.open = true;
  };
  window.mdeditorCaptureLayout = (on) => {
    document.body.classList.toggle("capture-export", on);
    if (on) {
      openAllDetails();
      stillVideos();
    }
    if (on) window.scrollTo(0, 0);
    return true;
  };
  window.mdeditorCaptureSize = () => {
    const rect = preview.getBoundingClientRect();
    return { width: Math.ceil(rect.width), height: Math.ceil(preview.scrollHeight) };
  };
  // Diagrams and formulas in document order, the order the Word output meets them.
  // Only the outer boxes count: shapes inside a drawing can carry the class
  // "mermaid" too (git graph commits do), and they are not figures of their own.
  window.mdeditorFigureRects = () => Array.from(preview.querySelectorAll("div.mermaid, div.graphviz, div.stl, div.geo-map, span.math, div.math"))
    .filter((element) => !element.parentElement?.closest(".mermaid, .graphviz, .stl, .geo-map, .math")).map((element) => {
    const rect = element.getBoundingClientRect();
    const pad = element.classList.contains("math") ? 1 : 4;
    return {
      x: Math.max(0, rect.left + window.scrollX - pad),
      y: Math.max(0, rect.top + window.scrollY - pad),
      width: Math.max(1, rect.width + pad * 2),
      height: Math.max(1, rect.height + pad * 2)
    };
    });

  // Encoding: shown beside the file name, chosen in a dialog, and announced in a
  // notice when it had to be guessed or some bytes could not be read.
  const encodingIds = ["utf-8", "utf-8-bom", "utf-16le", "utf-16be", "shift_jis", "euc-jp", "iso-2022-jp", "windows-1252"];
  const encodingButton = document.getElementById("encoding-button");
  const encodingDialog = document.getElementById("encoding-dialog");
  const encodingList = document.getElementById("encoding-list");
  const encodingReopen = document.getElementById("encoding-reopen");
  const notice = document.getElementById("notice");
  const noticeText = document.getElementById("notice-text");
  let documentEncoding = "utf-8";
  let encodingSource = "detected";
  let canReopen = false;
  let noticeState = null;

  const encodingLabel = (id) => t("encodingLabels")[id] ?? id;

  const updateEncodingButton = () => {
    const label = encodingLabel(documentEncoding);
    encodingButton.textContent = encodingSource === "guessed" ? t("encodingGuessed")(label) : label;
    encodingButton.setAttribute("aria-label", t("encodingButtonLabel")(encodingButton.textContent));
  };

  const updateNotice = () => {
    const askMaps = mapsWaiting > 0 && !mapNoticeClosed;
    document.getElementById("notice-maps").hidden = !askMaps;
    if (!noticeState && !askMaps) {
      notice.hidden = true;
      return;
    }
    const main = !noticeState ? ""
      : noticeState.kind === "message"
        ? noticeState.text
        : noticeState.kind === "external"
          ? [noticeState.prefix, t("noticeExternal")(noticeState.count)].filter(Boolean).join(language === "en" ? " " : "")
          : t(noticeState.kind === "lossy" ? "noticeLossy" : "noticeGuessed")(encodingLabel(noticeState.encoding));
    // Sentences are joined with a space in English only (Japanese and Chinese end with 。).
    noticeText.textContent = [main, askMaps ? t("noticeMaps")(mapsWaiting) : ""].filter(Boolean).join(language === "en" ? " " : "");
    document.getElementById("notice-encoding").hidden = !noticeState || noticeState.kind === "message" || noticeState.kind === "external";
    document.getElementById("notice-action").hidden = noticeState?.kind !== "external";
    const reopen = document.getElementById("notice-reopen");
    reopen.hidden = !(noticeState?.kind === "message" && noticeState.reopen);
    reopen.textContent = noticeState?.reopen ?? "";
    notice.classList.toggle("notice-strong", noticeState?.kind === "lossy");
    notice.hidden = false;
  };

  // reopen: the label of a button that opens the PDF again the other way (see the host).
  const showNoticeMessage = (text, reopen) => {
    noticeState = { kind: "message", text, reopen };
    updateNotice();
  };

  const renderEncodingList = () => {
    encodingList.replaceChildren();
    for (const id of encodingIds) {
      const label = document.createElement("label");
      label.className = "encoding-option";
      const input = document.createElement("input");
      input.type = "radio";
      input.name = "encoding";
      input.value = id;
      input.checked = id === documentEncoding;
      label.append(input, document.createTextNode(encodingLabel(id)));
      encodingList.appendChild(label);
    }
  };

  const selectedEncoding = () => encodingList.querySelector("input:checked")?.value ?? documentEncoding;

  const openEncodingDialog = () => {
    renderEncodingList();
    document.getElementById("encoding-current").textContent = t("encodingCurrent")(encodingButton.textContent);
    encodingReopen.disabled = !canReopen;
    encodingDialog.showModal();
    encodingList.querySelector("input:checked")?.focus();
  };

  encodingButton.addEventListener("click", openEncodingDialog);
  document.getElementById("notice-encoding").addEventListener("click", openEncodingDialog);
  document.getElementById("notice-action").addEventListener("click", () => {
    noticeState = noticeState?.prefix ? { kind: "message", text: noticeState.prefix } : null;
    updateNotice();
    post({ type: "allowExternalImages" });
  });

  // Open URL: an https:// web page opens as Markdown (the host fetches it).
  const urlDialog = document.getElementById("url-dialog");
  const urlInput = document.getElementById("url-input");
  const openUrlDialog = () => {
    urlDialog.showModal();
    urlInput.select();
  };
  document.getElementById("url-button").addEventListener("click", openUrlDialog);
  document.getElementById("url-close").addEventListener("click", () => urlDialog.close());
  document.getElementById("url-form").addEventListener("submit", (event) => {
    event.preventDefault();
    const url = urlInput.value.trim();
    if (!url) return;
    urlDialog.close();
    post({ type: "openUrl", url });
  });

  // Image size and position, written as GitHub READMEs do it: <img width="50%">,
  // <img align="left"> to let text flow, and <p align="center"><img></p>. It works on
  // the image at the cursor, or the one right-clicked in the preview (data-source is
  // where it starts in the editor). The change is one edit, so Ctrl+Z undoes it.
  const imageDialog = document.getElementById("image-dialog");
  const imagePixels = document.getElementById("image-pixels");
  const imageAloneNote = document.getElementById("image-alone-note");
  let imageTarget = null;
  const markdownImagePattern = /!\[((?:\\.|[^\]\\])*)\]\(\s*(<[^>\n]*>|(?:[^\s()\\]|\\.|\((?:[^\s()\\]|\\.)*\))+)(?:\s+("(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\)))?\s*\)/g;
  const htmlImagePattern = /<img\b[^>]*>/gi;
  const unescapeMarkdown = (text) => text.replace(/\\([!-/:-@[-`{-~])/g, "$1");
  const escapeAttribute = (text) => text.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  const decodeAttribute = (text) => {
    const area = document.createElement("textarea");
    area.innerHTML = text;
    return area.value;
  };
  // The attributes of an HTML tag, in order, as [name, value] pairs.
  const tagAttributes = (tag) => [...tag.replace(/^<[a-z]+/i, "").replace(/\/?>$/, "")
    .matchAll(/([^\s"'>/=]+)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s"'>]+)))?/g)]
    .map((match) => [match[1].toLowerCase(), decodeAttribute(match[2] ?? match[3] ?? match[4] ?? "")]);
  const attributeOf = (attributes, name) => attributes.find(([key]) => key === name)?.[1];

  // Images on one editor line: Markdown ![alt](src "title") and HTML <img>.
  const imagesInLine = (line) => {
    const found = [];
    for (const match of line.text.matchAll(markdownImagePattern)) {
      const destination = match[2].startsWith("<") ? match[2].slice(1, -1) : match[2];
      const title = match[3] ? unescapeMarkdown(match[3].slice(1, -1)) : "";
      found.push({
        from: line.from + match.index, to: line.from + match.index + match[0].length,
        attributes: [["src", unescapeMarkdown(destination).replace(/ /g, "%20")], ["alt", unescapeMarkdown(match[1])], ...(title ? [["title", title]] : [])]
      });
    }
    for (const match of line.text.matchAll(htmlImagePattern)) {
      found.push({ from: line.from + match.index, to: line.from + match.index + match[0].length, attributes: tagAttributes(match[0]) });
    }
    return found.sort((a, b) => a.from - b.from);
  };

  // The image at pos (or the only one on its line), and whether it is the whole
  // line, alone or in <p align>/<div align>: only then can it be centered or put right.
  const findImage = (pos, startsAt = false) => {
    const line = view.state.doc.lineAt(pos);
    const images = imagesInLine(line);
    const image = startsAt
      ? images.find((item) => item.from === pos)
      : images.find((item) => item.from <= pos && pos <= item.to) ?? (images.length === 1 ? images[0] : null);
    if (!image) return null;
    const indent = line.text.match(/^\s*/)[0];
    const rest = line.text.slice(indent.length).trimEnd();
    const wrapper = rest.match(/^<(p|div)\b[^>]*>\s*(<img\b[^>]*>)\s*<\/\1>$/i);
    const tag = view.state.sliceDoc(image.from, image.to);
    const alone = indent.length < 4 && (rest === tag || Boolean(wrapper && wrapper[2] === tag));
    const wrapperAlign = wrapper && wrapper[2] === tag ? (attributeOf(tagAttributes(rest.match(/^<[^>]*>/)[0]), "align") ?? "").toLowerCase() : "";
    return { ...image, line, indent, alone, wrapperAlign };
  };

  const imageSizeOf = (width) => {
    const value = (width ?? "").trim();
    if (!value) return { size: "original" };
    const percent = value.match(/^(25|50|75|100)%$/);
    if (percent) return { size: percent[1] };
    const pixels = value.match(/^(\d+)(?:px)?$/);
    return pixels ? { size: "pixels", pixels: pixels[1] } : { size: "original" };
  };

  const updateImagePixels = () => {
    imagePixels.disabled = imageDialog.querySelector("input[name='image-size']:checked")?.value !== "pixels";
  };
  for (const input of imageDialog.querySelectorAll("input[name='image-size']")) input.addEventListener("change", updateImagePixels);

  const openImageDialog = (at) => {
    if (previewWindow) {
      post({ type: "command", id: "image", at });
      return;
    }
    const fromPreview = Number.isInteger(at);
    const image = fromPreview
      ? (at <= view.state.doc.length ? findImage(at, true) : null)
      : findImage(view.state.selection.main.head);
    if (!image) {
      showNoticeMessage(t(fromPreview ? "imageMoved" : "imageNone"));
      view.focus();
      return;
    }
    imageTarget = image;
    selectRange(image.from, image.to);
    const name = attributeOf(image.attributes, "alt") || decodeURIComponent((attributeOf(image.attributes, "src") ?? "").split(/[?#]/)[0].split("/").pop() || "");
    document.getElementById("image-name").textContent = t("imageTarget")(name);
    const { size, pixels } = imageSizeOf(attributeOf(image.attributes, "width"));
    imageDialog.querySelector(`input[name='image-size'][value='${size}']`).checked = true;
    imagePixels.value = pixels ?? "";
    updateImagePixels();
    const align = (attributeOf(image.attributes, "align") ?? "").toLowerCase();
    const position = image.wrapperAlign === "center" || image.wrapperAlign === "right" ? image.wrapperAlign
      : align === "left" ? "floatLeft" : align === "right" ? "floatRight" : "none";
    for (const input of imageDialog.querySelectorAll("input[name='image-position']")) {
      input.disabled = !image.alone && (input.value === "center" || input.value === "right");
      input.checked = input.value === position;
    }
    imageAloneNote.hidden = image.alone;
    imageDialog.showModal();
    imageDialog.querySelector("input[name='image-size']:checked").focus();
  };

  const applyImage = () => {
    const image = imageTarget;
    const size = imageDialog.querySelector("input[name='image-size']:checked").value;
    const position = imageDialog.querySelector("input[name='image-position']:checked").value;
    const pixels = Number(imagePixels.value);
    if (size === "pixels" && !(Number.isInteger(pixels) && pixels >= 1 && pixels <= 10000)) {
      showNoticeMessage(t("imagePixelsInvalid"));
      imagePixels.focus();
      return false;
    }
    // The text may have changed under the dialog; the image must still be where it was.
    if (!image || view.state.sliceDoc(image.from, image.to) !== image.line.text.slice(image.from - image.line.from, image.to - image.line.from)
        || view.state.doc.lineAt(image.from).text !== image.line.text) {
      showNoticeMessage(t("imageNone"));
      return true;
    }

    let attributes = image.attributes.filter(([name]) => !["width", "align", "data-source"].includes(name));
    if (size !== "original") {
      // A new width keeps the picture's proportions, so an old height goes.
      attributes = attributes.filter(([name]) => name !== "height");
      attributes.push(["width", size === "pixels" ? String(pixels) : `${size}%`]);
    }
    if (position === "floatLeft" || position === "floatRight") attributes.push(["align", position === "floatLeft" ? "left" : "right"]);
    if (!attributes.some(([name]) => name === "alt")) attributes.splice(1, 0, ["alt", ""]);
    const wrapped = position === "center" || position === "right";
    const plain = !wrapped && attributes.every(([name]) => name === "src" || name === "alt" || name === "title");
    let text;
    if (plain) {
      // Nothing GitHub needs HTML for: back to Markdown.
      const source = attributeOf(attributes, "src") ?? "";
      const title = attributeOf(attributes, "title");
      const destination = /[\s()<>]/.test(source) ? `<${source.replace(/[<>]/g, (c) => encodeURIComponent(c))}>` : source;
      text = `![${(attributeOf(attributes, "alt") ?? "").replace(/[[\]\\]/g, "\\$&")}](${destination}${title ? ` "${title.replace(/["\\]/g, "\\$&")}"` : ""})`;
    } else {
      const ordered = [...attributes.filter(([name]) => name === "src"), ...attributes.filter(([name]) => name === "alt"),
        ...attributes.filter(([name]) => name !== "src" && name !== "alt")];
      text = `<img ${ordered.map(([name, value]) => `${name}="${escapeAttribute(value)}"`).join(" ")}>`;
      if (wrapped) text = `<p align="${position}">${text}</p>`;
    }

    let from = image.from;
    let to = image.to;
    if (image.alone) {
      // The whole line: an HTML block ends at a blank line, so text right below gets one.
      from = image.line.from + image.indent.length;
      to = image.line.to;
      const next = image.line.number < view.state.doc.lines ? view.state.doc.line(image.line.number + 1) : null;
      if (!plain && next && next.text.trim() !== "") text += "\n";
    }
    view.dispatch({ changes: { from, to, insert: text }, selection: EditorSelection.single(from, from + text.length), scrollIntoView: true });
    const words = t("imageSizeWords");
    showNoticeMessage(t("imageChanged")(
      size === "original" ? words.original : size === "pixels" ? words.pixels(pixels) : words.percent(size),
      t("imagePositionWords")[position]));
    return true;
  };

  document.getElementById("image-button").addEventListener("click", () => openImageDialog());
  document.getElementById("image-close").addEventListener("click", () => imageDialog.close());
  imageDialog.addEventListener("close", () => {
    imageTarget = null;
    if (!previewWindow) view.focus();
  });
  document.getElementById("image-form").addEventListener("submit", (event) => {
    event.preventDefault();
    if (applyImage()) imageDialog.close();
  });
  // Right-click on a picture in the preview opens the same dialog for it.
  preview.addEventListener("contextmenu", (event) => {
    const image = event.target instanceof Element ? event.target.closest("img[data-source]") : null;
    if (!image) return;
    event.preventDefault();
    openImageDialog(Number(image.dataset.source));
  });

  // Internet images (ask, always, never) belong to the host, which renders the preview.
  const externalSelect = document.getElementById("setting-external");
  externalSelect.addEventListener("change", () => post({ type: "setExternalImages", value: externalSelect.value }));
  document.getElementById("notice-maps").addEventListener("click", () => {
    mapsWaiting = 0;
    updateNotice();
    post({ type: "allowMaps" });
  });
  document.getElementById("notice-close").addEventListener("click", () => {
    noticeState = null;
    mapNoticeClosed = true;
    updateNotice();
    view.focus();
  });
  document.getElementById("encoding-close").addEventListener("click", () => encodingDialog.close());
  encodingDialog.addEventListener("click", (event) => {
    if (event.target === encodingDialog) encodingDialog.close();
  });
  encodingReopen.addEventListener("click", () => {
    encodingDialog.close();
    post({ type: "reopenWithEncoding", encoding: selectedEncoding() });
  });
  document.getElementById("encoding-save").addEventListener("click", () => {
    encodingDialog.close();
    post({ type: "setSaveEncoding", encoding: selectedEncoding() });
  });

  // Paper size for PDF export: "auto" follows the Windows region (shown in brackets).
  const paperOrder = ["auto", "a4", "letter"];
  const paperButton = document.getElementById("paper-button");
  let paper = "auto";
  let autoPaper = "a4";
  const updatePaperButton = () => {
    paperButton.textContent = t("paperLabels")(paper, autoPaper);
  };
  paperButton.addEventListener("click", () => {
    paper = paperOrder[(paperOrder.indexOf(paper) + 1) % paperOrder.length];
    updatePaperButton();
    post({ type: "setPaper", paper });
  });

  themeButton.addEventListener("click", () => {
    theme = themeOrder[(themeOrder.indexOf(theme) + 1) % themeOrder.length];
    updateThemeButton();
    post({ type: "setTheme", theme });
  });
  const setLanguage = (value) => {
    language = normalizeLanguage(value) ?? language;
    try { localStorage.setItem("mdeditor-language", language); } catch { /* kept for this session */ }
    applyLanguage();
    post({ type: "setLanguage", lang: language });
  };
  languageSelect.addEventListener("change", () => setLanguage(languageSelect.value));
  fullscreenButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const mode = button.dataset.fullscreenMode;
      setDisplayMode(displayMode === mode ? "normal" : mode);
    });
  });

  // The preview pane's button opens the preview in its own window; the editor
  // pane's button (or that window's own button) brings it back.
  const detachButtons = document.querySelectorAll("[data-detach]");
  const toggleDetachedPreview = () => {
    post({ type: previewWindow || previewDetached ? "attachPreview" : "detachPreview" });
  };
  const updateDetachButtons = () => {
    for (const button of detachButtons) {
      const inEditor = button.dataset.detach === "editor";
      button.hidden = previewWindow ? inEditor : inEditor !== previewDetached;
      button.textContent = previewWindow ? t("attachPreview") : inEditor ? t("attachPreviewMain") : t("detachPreview");
    }
  };
  const setPreviewDetached = (detached) => {
    previewDetached = detached;
    document.body.classList.toggle("preview-detached", detached);
    if (detached && displayMode === "preview") setDisplayMode("normal");
    // Reading runs in the window that shows the preview, so it can be marked on screen.
    if (detached) stopReading();
    updateDetachButtons();
    requestAnimationFrame(() => syncFrom(editorScroller));
  };
  detachButtons.forEach((button) => button.addEventListener("click", toggleDetachedPreview));

  divider.addEventListener("pointerdown", (event) => {
    if (displayMode === "preview") {
      return;
    }

    pointerId = event.pointerId;
    divider.setPointerCapture(pointerId);
    document.body.classList.add("is-resizing");
    setEditorRatio(getPointerRatio(event));
    event.preventDefault();
  });

  divider.addEventListener("pointermove", (event) => {
    if (event.pointerId === pointerId) {
      setEditorRatio(getPointerRatio(event));
    }
  });

  const stopDragging = (event) => {
    if (event.pointerId !== pointerId) {
      return;
    }

    pointerId = null;
    document.body.classList.remove("is-resizing");
  };
  divider.addEventListener("pointerup", stopDragging);
  divider.addEventListener("pointercancel", stopDragging);
  divider.addEventListener("lostpointercapture", () => {
    pointerId = null;
    document.body.classList.remove("is-resizing");
  });

  divider.addEventListener("keydown", (event) => {
    const compact = compactLayout.matches;
    const forward = compact ? "ArrowDown" : "ArrowRight";
    const backward = compact ? "ArrowUp" : "ArrowLeft";

    if (event.key === forward) {
      event.preventDefault();
      setEditorRatio(editorRatio + 5);
    } else if (event.key === backward) {
      event.preventDefault();
      setEditorRatio(editorRatio - 5);
    } else if (event.key === "Home") {
      event.preventDefault();
      setEditorRatio(20);
    } else if (event.key === "End") {
      event.preventDefault();
      setEditorRatio(80);
    }
  });

  window.addEventListener("resize", () => {
    if (displayMode !== "preview") {
      setEditorRatio(editorRatio, false);
    }
  });
  compactLayout.addEventListener("change", () => {
    if (displayMode !== "preview") {
      setEditorRatio(editorRatio, false);
    }
  });

  const findBar = document.getElementById("find-bar");
  const findInput = document.getElementById("find-input");
  const replaceInput = document.getElementById("replace-input");
  const findCase = document.getElementById("find-case");
  const findStatus = document.getElementById("find-status");

  const openFindBar = () => {
    findBar.hidden = false;
    const { from, to } = view.state.selection.main;
    const selected = view.state.sliceDoc(from, to);
    if (selected && !selected.includes("\n")) {
      findInput.value = selected;
    }
    findStatus.textContent = "";
    findInput.focus();
    findInput.select();
  };

  const closeFindBar = () => {
    findBar.hidden = true;
    view.focus();
  };

  // Matches come from the original text, so selection positions stay right even
  // when lowercasing would change a string's length (for example "İ").
  const findMatches = () => {
    const pattern = findInput.value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const regex = new RegExp(pattern, findCase.checked ? "g" : "gi");
    return Array.from(view.state.doc.toString().matchAll(regex), (match) => ({
      index: match.index,
      length: match[0].length
    }));
  };

  const findText = (forward) => {
    const needle = findInput.value;
    if (!needle) {
      return false;
    }
    const matches = findMatches();
    if (matches.length === 0) {
      findStatus.textContent = t("notFound");
      return false;
    }
    const selection = view.state.selection.main;
    const match = forward
      ? matches.find((m) => m.index >= selection.to) ?? matches[0]
      : matches.findLast((m) => m.index < selection.from) ?? matches[matches.length - 1];
    findStatus.textContent = "";
    selectRange(match.index, match.index + match.length);
    return true;
  };

  const replaceSelection = (text) => {
    view.dispatch({ ...view.state.replaceSelection(text), scrollIntoView: true });
    view.focus();
  };

  const replaceOne = () => {
    const needle = findInput.value;
    if (!needle) return;
    const selection = view.state.selection.main;
    const selectedIsMatch = findMatches().some((m) =>
      m.index === selection.from && m.index + m.length === selection.to);
    if (selectedIsMatch) {
      replaceSelection(replaceInput.value);
    }
    findText(true);
  };

  const replaceAll = () => {
    const needle = findInput.value;
    if (!needle) return;
    const matches = findMatches();
    const count = matches.length;
    if (count === 0) {
      findStatus.textContent = t("notFound");
      return;
    }
    // One transaction, so a single undo restores every replacement.
    view.dispatch({
      changes: matches.map((m) => ({ from: m.index, to: m.index + m.length, insert: replaceInput.value }))
    });
    view.focus();
    findStatus.textContent = t("replacedCount")(count);
  };

  // Markdown quick reference. Results are fixed markup written here, not user input.
  const helpItems = [
    { group: { ja: "基本", en: "Basics", zh: "基本" } },
    { block: true,
      ja: ["見出し", "# 見出し1\n## 見出し2\n### 見出し3", '<span class="help-h1">見出し1</span><span class="help-h2">見出し2</span><span class="help-h3">見出し3</span>'],
      en: ["Headings", "# Heading 1\n## Heading 2\n### Heading 3", '<span class="help-h1">Heading 1</span><span class="help-h2">Heading 2</span><span class="help-h3">Heading 3</span>'],
      zh: ["标题", "# 标题1\n## 标题2\n### 标题3", '<span class="help-h1">标题1</span><span class="help-h2">标题2</span><span class="help-h3">标题3</span>'] },
    { block: true,
      ja: ["段落", "1つ目の段落\n\n2つ目の段落", "<p>1つ目の段落</p><p>2つ目の段落</p><small>空行で段落が分かれます</small>"],
      en: ["Paragraphs", "First paragraph\n\nSecond paragraph", "<p>First paragraph</p><p>Second paragraph</p><small>A blank line starts a new paragraph</small>"],
      zh: ["段落", "第一段\n\n第二段", "<p>第一段</p><p>第二段</p><small>空一行即分成新的段落</small>"] },
    { block: true,
      ja: ["改行", "1行目\\\n2行目", "<p>1行目<br>2行目</p><small>行末に \\ を付けるか、半角スペースを2つ入れると改行します。何も付けずに改行すると、前の行に続けて表示されます</small>"],
      en: ["Line break", "First line\\\nSecond line", "<p>First line<br>Second line</p><small>End a line with \\ or two spaces to break it. A plain line break joins the lines in the output</small>"],
      zh: ["换行", "第一行\\\n第二行", "<p>第一行<br>第二行</p><small>在行末加 \\ 或两个半角空格即可换行。不加任何东西直接换行时，会接在上一行后面显示</small>"] },
    { ja: ["太字", "**太字**", "<strong>太字</strong>"], en: ["Bold", "**bold**", "<strong>bold</strong>"], zh: ["粗体", "**粗体**", "<strong>粗体</strong>"] },
    { ja: ["斜体", "*斜体*", "<em>斜体</em>"], en: ["Italic", "*italic*", "<em>italic</em>"], zh: ["斜体", "*斜体*", "<em>斜体</em>"] },
    { ja: ["取り消し線", "~~取り消し線~~", "<del>取り消し線</del>"], en: ["Strikethrough", "~~strikethrough~~", "<del>strikethrough</del>"], zh: ["删除线", "~~删除线~~", "<del>删除线</del>"] },
    { ja: ["コード（文中）", "`コード`", "<code>コード</code>"], en: ["Inline code", "`code`", "<code>code</code>"], zh: ["行内代码", "`代码`", "<code>代码</code>"] },
    { ja: ["リンク", "[リンクの文字](https://example.com)", '<span class="help-link">リンクの文字</span>'],
      en: ["Link", "[link text](https://example.com)", '<span class="help-link">link text</span>'],
      zh: ["链接", "[链接文字](https://example.com)", '<span class="help-link">链接文字</span>'] },
    { ja: ["画像", "![画像の説明](https://example.com/image.png)", "<small>画像が表示されます</small>"],
      en: ["Image", "![image description](https://example.com/image.png)", "<small>The image is shown</small>"],
      zh: ["图像", "![图像说明](https://example.com/image.png)", "<small>会显示图像</small>"] },
    { block: true,
      ja: ["画像の大きさと位置", '<p align="center"><img src="https://example.com/image.png" alt="画像の説明" width="50%"></p>',
        "<small>本文の幅の半分の大きさで、中央に表示されます（GitHub と同じ書き方）。画像の行で「画像の調整」を押すか、プレビューの画像を右クリックしても書き換えられます</small>"],
      en: ["Image size and position", '<p align="center"><img src="https://example.com/image.png" alt="image description" width="50%"></p>',
        "<small>Shown at half the text width, centered (written as on GitHub). Adjust image on the image's line, or right-clicking the image in the preview, rewrites it for you</small>"],
      zh: ["图像的大小和位置", '<p align="center"><img src="https://example.com/image.png" alt="图像说明" width="50%"></p>',
        "<small>以正文宽度的一半居中显示（与 GitHub 相同的写法）。在图像所在行按“调整图像”，或在预览中右键单击图像，也可以改写</small>"] },
    { block: true,
      ja: ["箇条書き", "- 項目1\n- 項目2\n  - 入れ子の項目", "<ul><li>項目1</li><li>項目2<ul><li>入れ子の項目</li></ul></li></ul>"],
      en: ["Bulleted list", "- Item 1\n- Item 2\n  - Nested item", "<ul><li>Item 1</li><li>Item 2<ul><li>Nested item</li></ul></li></ul>"],
      zh: ["无序列表", "- 项目1\n- 项目2\n  - 嵌套项目", "<ul><li>项目1</li><li>项目2<ul><li>嵌套项目</li></ul></li></ul>"] },
    { block: true,
      ja: ["番号付きリスト", "1. 手順1\n2. 手順2", "<ol><li>手順1</li><li>手順2</li></ol>"],
      en: ["Numbered list", "1. Step 1\n2. Step 2", "<ol><li>Step 1</li><li>Step 2</li></ol>"],
      zh: ["有序列表", "1. 步骤1\n2. 步骤2", "<ol><li>步骤1</li><li>步骤2</li></ol>"] },
    { block: true,
      ja: ["引用", "> 引用文", "<blockquote>引用文</blockquote>"],
      en: ["Quote", "> Quoted text", "<blockquote>Quoted text</blockquote>"],
      zh: ["引用", "> 引用的文字", "<blockquote>引用的文字</blockquote>"] },
    { block: true,
      ja: ["コードブロック", "```js\nconsole.log(\"hello\");\n```", "<pre><code>console.log(\"hello\");</code></pre>"],
      en: ["Code block", "```js\nconsole.log(\"hello\");\n```", "<pre><code>console.log(\"hello\");</code></pre>"],
      zh: ["代码块", "```js\nconsole.log(\"hello\");\n```", "<pre><code>console.log(\"hello\");</code></pre>"] },
    { block: true,
      ja: ["表", "| 名前 | 数 |\n| --- | ---: |\n| りんご | 3 |", '<table><thead><tr><th>名前</th><th class="help-right">数</th></tr></thead><tbody><tr><td>りんご</td><td class="help-right">3</td></tr></tbody></table>'],
      en: ["Table", "| Name | Qty |\n| --- | ---: |\n| Apple | 3 |", '<table><thead><tr><th>Name</th><th class="help-right">Qty</th></tr></thead><tbody><tr><td>Apple</td><td class="help-right">3</td></tr></tbody></table>'],
      zh: ["表格", "| 名称 | 数量 |\n| --- | ---: |\n| 苹果 | 3 |", '<table><thead><tr><th>名称</th><th class="help-right">数量</th></tr></thead><tbody><tr><td>苹果</td><td class="help-right">3</td></tr></tbody></table>'] },
    { block: true, ja: ["区切り線", "---", "<hr>"], en: ["Horizontal rule", "---", "<hr>"], zh: ["分隔线", "---", "<hr>"] },
    { block: true,
      ja: ["チェックリスト", "- [x] 済んだこと\n- [ ] これからのこと", '<ul class="help-tasks"><li>☑ 済んだこと</li><li>☐ これからのこと</li></ul>'],
      en: ["Checklist", "- [x] Done\n- [ ] To do", '<ul class="help-tasks"><li>☑ Done</li><li>☐ To do</li></ul>'],
      zh: ["任务列表", "- [x] 已完成\n- [ ] 待完成", '<ul class="help-tasks"><li>☑ 已完成</li><li>☐ 待完成</li></ul>'] },
    // Diagrams: the result column draws each example with Mermaid.
    { group: { ja: "図（Mermaid）", en: "Diagrams (Mermaid)", zh: "图表（Mermaid）" } },
    { block: true, mermaid: true,
      ja: ["フローチャート", "```mermaid\nflowchart TD\n  A[開始] --> B{条件}\n  B -->|はい| C[処理する]\n  B -->|いいえ| D[終了]\n```", ""],
      en: ["Flowchart", "```mermaid\nflowchart TD\n  A[Start] --> B{Condition}\n  B -->|Yes| C[Process]\n  B -->|No| D[End]\n```", ""],
      zh: ["流程图", "```mermaid\nflowchart TD\n  A[开始] --> B{条件}\n  B -->|是| C[处理]\n  B -->|否| D[结束]\n```", ""] },
    { block: true, mermaid: true,
      ja: ["シーケンス図", "```mermaid\nsequenceDiagram\n  participant U as 利用者\n  participant A as アプリ\n  U->>A: 保存する\n  A-->>U: 保存しました\n```", ""],
      en: ["Sequence diagram", "```mermaid\nsequenceDiagram\n  participant U as User\n  participant A as App\n  U->>A: Save\n  A-->>U: Saved\n```", ""],
      zh: ["时序图", "```mermaid\nsequenceDiagram\n  participant U as 用户\n  participant A as 应用\n  U->>A: 保存\n  A-->>U: 已保存\n```", ""] },
    { block: true, mermaid: true,
      ja: ["ガントチャート", "```mermaid\ngantt\n  dateFormat YYYY-MM-DD\n  section 準備\n  設計 :a1, 2026-01-05, 5d\n  section 作業\n  開発 :after a1, 7d\n```", ""],
      en: ["Gantt chart", "```mermaid\ngantt\n  dateFormat YYYY-MM-DD\n  section Plan\n  Design :a1, 2026-01-05, 5d\n  section Build\n  Develop :after a1, 7d\n```", ""],
      zh: ["甘特图", "```mermaid\ngantt\n  dateFormat YYYY-MM-DD\n  section 准备\n  设计 :a1, 2026-01-05, 5d\n  section 实施\n  开发 :after a1, 7d\n```", ""] },
    { block: true, mermaid: true,
      ja: ["円グラフ", "```mermaid\npie title 時間の使い方\n  \"書く\" : 60\n  \"見直す\" : 40\n```", ""],
      en: ["Pie chart", "```mermaid\npie title How time is spent\n  \"Writing\" : 60\n  \"Reviewing\" : 40\n```", ""],
      zh: ["饼图", "```mermaid\npie title 时间分配\n  \"写作\" : 60\n  \"校对\" : 40\n```", ""] },
    { block: true, mermaid: true,
      ja: ["クラス図", "```mermaid\nclassDiagram\n  class Document {\n    +title\n    +save()\n  }\n  Document <|-- Report\n```", ""],
      en: ["Class diagram", "```mermaid\nclassDiagram\n  class Document {\n    +title\n    +save()\n  }\n  Document <|-- Report\n```", ""],
      zh: ["类图", "```mermaid\nclassDiagram\n  class Document {\n    +title\n    +save()\n  }\n  Document <|-- Report\n```", ""] },
    { block: true, mermaid: true,
      ja: ["状態遷移図", "```mermaid\nstateDiagram-v2\n  state \"編集中\" as Editing\n  state \"保存済み\" as Saved\n  [*] --> Editing\n  Editing --> Saved: 保存\n  Saved --> Editing: 書き直す\n```", ""],
      en: ["State diagram", "```mermaid\nstateDiagram-v2\n  [*] --> Editing\n  Editing --> Saved: save\n  Saved --> Editing: edit\n```", ""],
      zh: ["状态图", "```mermaid\nstateDiagram-v2\n  state \"编辑中\" as Editing\n  state \"已保存\" as Saved\n  [*] --> Editing\n  Editing --> Saved: 保存\n  Saved --> Editing: 修改\n```", ""] },
    { block: true, mermaid: true,
      ja: ["ER図", "```mermaid\nerDiagram\n  CUSTOMER ||--o{ ORDER : places\n  ORDER ||--|{ ITEM : contains\n```", ""],
      en: ["ER diagram", "```mermaid\nerDiagram\n  CUSTOMER ||--o{ ORDER : places\n  ORDER ||--|{ ITEM : contains\n```", ""],
      zh: ["ER 图", "```mermaid\nerDiagram\n  CUSTOMER ||--o{ ORDER : places\n  ORDER ||--|{ ITEM : contains\n```", ""] },
    { block: true, mermaid: true,
      ja: ["マインドマップ", "```mermaid\nmindmap\n  root((mdEditor))\n    書く\n    見る\n    渡す\n```", ""],
      en: ["Mind map", "```mermaid\nmindmap\n  root((mdEditor))\n    Write\n    View\n    Share\n```", ""],
      zh: ["思维导图", "```mermaid\nmindmap\n  root((mdEditor))\n    写\n    看\n    分享\n```", ""] },
    { block: true, mermaid: true,
      ja: ["棒グラフ・折れ線グラフ", "```mermaid\nxychart-beta\n  title \"月別の件数\"\n  x-axis [\"1月\", \"2月\", \"3月\", \"4月\"]\n  y-axis \"件数\" 0 --> 40\n  bar [12, 18, 25, 31]\n  line [12, 18, 25, 31]\n```", ""],
      en: ["Bar and line chart", "```mermaid\nxychart-beta\n  title \"Documents per month\"\n  x-axis [Jan, Feb, Mar, Apr]\n  y-axis \"Count\" 0 --> 40\n  bar [12, 18, 25, 31]\n  line [12, 18, 25, 31]\n```", ""],
      zh: ["柱状图和折线图", "```mermaid\nxychart-beta\n  title \"每月文档数\"\n  x-axis [\"1月\", \"2月\", \"3月\", \"4月\"]\n  y-axis \"件数\" 0 --> 40\n  bar [12, 18, 25, 31]\n  line [12, 18, 25, 31]\n```", ""] },
    { block: true, mermaid: true,
      ja: ["タイムライン", "```mermaid\ntimeline\n  title 作業の流れ\n  10月1日 : 書き始める\n  10月5日 : 見直す\n          : 図を足す\n  10月10日 : 公開する\n```", ""],
      en: ["Timeline", "```mermaid\ntimeline\n  title How the work goes\n  Oct 1 : Start writing\n  Oct 5 : Review\n        : Add diagrams\n  Oct 10 : Publish\n```", ""],
      zh: ["时间线", "```mermaid\ntimeline\n  title 工作流程\n  10月1日 : 开始写作\n  10月5日 : 校对\n          : 添加图表\n  10月10日 : 发布\n```", ""] },
    // Math: the result column renders each example with KaTeX.
    { group: { ja: "数式（KaTeX）", en: "Math (KaTeX)", zh: "公式（KaTeX）" } },
    { math: "E = mc^2", ja: ["文中の数式", "$E = mc^2$", ""], en: ["Inline math", "$E = mc^2$", ""], zh: ["行内公式", "$E = mc^2$", ""] },
    { block: true, math: "\\int_0^1 x^2\\,dx = \\frac{1}{3}", display: true,
      ja: ["別の行の数式", "$$\n\\int_0^1 x^2\\,dx = \\frac{1}{3}\n$$", ""],
      en: ["Display math", "$$\n\\int_0^1 x^2\\,dx = \\frac{1}{3}\n$$", ""],
      zh: ["独立公式", "$$\n\\int_0^1 x^2\\,dx = \\frac{1}{3}\n$$", ""] },
    { math: "\\frac{a}{b}", ja: ["分数", "$\\frac{a}{b}$", ""], en: ["Fraction", "$\\frac{a}{b}$", ""], zh: ["分数", "$\\frac{a}{b}$", ""] },
    { math: "\\sqrt{x^2 + 1}", ja: ["平方根", "$\\sqrt{x^2 + 1}$", ""], en: ["Square root", "$\\sqrt{x^2 + 1}$", ""], zh: ["平方根", "$\\sqrt{x^2 + 1}$", ""] },
    { math: "x_i^2", ja: ["上付き・下付き", "$x_i^2$", ""], en: ["Superscript and subscript", "$x_i^2$", ""], zh: ["上标和下标", "$x_i^2$", ""] },
    { math: "\\sum_{i=1}^{n} i = \\frac{n(n+1)}{2}", ja: ["総和", "$\\sum_{i=1}^{n} i = \\frac{n(n+1)}{2}$", ""], en: ["Sum", "$\\sum_{i=1}^{n} i = \\frac{n(n+1)}{2}$", ""], zh: ["求和", "$\\sum_{i=1}^{n} i = \\frac{n(n+1)}{2}$", ""] },
    { math: "\\alpha,\\ \\beta,\\ \\pi,\\ \\theta", ja: ["ギリシャ文字", "$\\alpha,\\ \\beta,\\ \\pi,\\ \\theta$", ""], en: ["Greek letters", "$\\alpha,\\ \\beta,\\ \\pi,\\ \\theta$", ""], zh: ["希腊字母", "$\\alpha,\\ \\beta,\\ \\pi,\\ \\theta$", ""] },
    { block: true, math: "\\begin{pmatrix} a & b \\\\ c & d \\end{pmatrix}", display: true,
      ja: ["行列", "$$\n\\begin{pmatrix} a & b \\\\ c & d \\end{pmatrix}\n$$", ""],
      en: ["Matrix", "$$\n\\begin{pmatrix} a & b \\\\ c & d \\end{pmatrix}\n$$", ""],
      zh: ["矩阵", "$$\n\\begin{pmatrix} a & b \\\\ c & d \\end{pmatrix}\n$$", ""] },
    { block: true, math: "f(x) = \\begin{cases} 1 & (x > 0) \\\\ 0 & (x \\le 0) \\end{cases}", display: true,
      ja: ["場合分け", "$$\nf(x) = \\begin{cases} 1 & (x > 0) \\\\ 0 & (x \\le 0) \\end{cases}\n$$", ""],
      en: ["Cases", "$$\nf(x) = \\begin{cases} 1 & (x > 0) \\\\ 0 & (x \\le 0) \\end{cases}\n$$", ""],
      zh: ["分段函数", "$$\nf(x) = \\begin{cases} 1 & (x > 0) \\\\ 0 & (x \\le 0) \\end{cases}\n$$", ""] },
    { block: true, math: "\\begin{aligned} y &= (x + 1)^2 \\\\ &= x^2 + 2x + 1 \\end{aligned}", display: true,
      ja: ["複数行の式", "$$\n\\begin{aligned} y &= (x + 1)^2 \\\\ &= x^2 + 2x + 1 \\end{aligned}\n$$", ""],
      en: ["Aligned equations", "$$\n\\begin{aligned} y &= (x + 1)^2 \\\\ &= x^2 + 2x + 1 \\end{aligned}\n$$", ""],
      zh: ["多行公式", "$$\n\\begin{aligned} y &= (x + 1)^2 \\\\ &= x^2 + 2x + 1 \\end{aligned}\n$$", ""] },
    // GitHub's own additions (each also in samples/diagram-test, 20).
    { group: { ja: "GitHub と同じ書き方", en: "Written as on GitHub", zh: "与 GitHub 相同的写法" } },
    { block: true,
      ja: ["注意書き", "> [!NOTE]\n> 補足の情報です。", '<blockquote><strong>Note</strong><br>補足の情報です。</blockquote><small>[!TIP]・[!IMPORTANT]・[!WARNING]・[!CAUTION] もあります</small>'],
      en: ["Alert", "> [!NOTE]\n> Useful information.", '<blockquote><strong>Note</strong><br>Useful information.</blockquote><small>[!TIP], [!IMPORTANT], [!WARNING], and [!CAUTION] work too</small>'],
      zh: ["提示框", "> [!NOTE]\n> 补充信息。", '<blockquote><strong>Note</strong><br>补充信息。</blockquote><small>还有 [!TIP]、[!IMPORTANT]、[!WARNING]、[!CAUTION]</small>'] },
    { block: true,
      ja: ["折りたたみ", "<details>\n<summary>クリックで開く</summary>\n\n折りたたまれた本文です。\n\n</details>", "<details><summary>クリックで開く</summary>折りたたまれた本文です。</details>"],
      en: ["Collapsible section", "<details>\n<summary>Click to open</summary>\n\nThe collapsed text.\n\n</details>", "<details><summary>Click to open</summary>The collapsed text.</details>"],
      zh: ["折叠", "<details>\n<summary>点击展开</summary>\n\n折叠的正文。\n\n</details>", "<details><summary>点击展开</summary>折叠的正文。</details>"] },
    { ja: ["キーの表示", "<kbd>Ctrl</kbd>+<kbd>S</kbd>", "<kbd>Ctrl</kbd>+<kbd>S</kbd>"],
      en: ["Keys", "<kbd>Ctrl</kbd>+<kbd>S</kbd>", "<kbd>Ctrl</kbd>+<kbd>S</kbd>"],
      zh: ["按键", "<kbd>Ctrl</kbd>+<kbd>S</kbd>", "<kbd>Ctrl</kbd>+<kbd>S</kbd>"] },
    { ja: ["絵文字", ":rocket: :memo: :warning:", "🚀 📝 ⚠️"], en: ["Emoji", ":rocket: :memo: :warning:", "🚀 📝 ⚠️"], zh: ["表情符号", ":rocket: :memo: :warning:", "🚀 📝 ⚠️"] },
    // Other drawings and media (samples/diagram-test, 16 and 21).
    { group: { ja: "そのほかの図と動画", en: "Other drawings and video", zh: "其他图表和视频" } },
    { block: true,
      ja: ["Graphviz の図", "```dot\ndigraph {\n  開く -> 編集 -> 保存\n}\n```", "<small>矢印でつないだ図が描かれます（GitHub ではコードのまま）</small>"],
      en: ["Graphviz diagram", "```dot\ndigraph {\n  Open -> Edit -> Save\n}\n```", "<small>Drawn as boxes joined by arrows (GitHub shows the code)</small>"],
      zh: ["Graphviz 图", "```dot\ndigraph {\n  打开 -> 编辑 -> 保存\n}\n```", "<small>绘制为用箭头连接的图（在 GitHub 上显示为代码）</small>"] },
    { block: true,
      ja: ["地図", "```geojson\n{ \"type\": \"Point\", \"coordinates\": [139.767, 35.681] }\n```", "<small>世界地図の上に点が描かれます（経度・緯度の順）</small>"],
      en: ["Map", "```geojson\n{ \"type\": \"Point\", \"coordinates\": [139.767, 35.681] }\n```", "<small>A point is drawn on a world map (longitude, then latitude)</small>"],
      zh: ["地图", "```geojson\n{ \"type\": \"Point\", \"coordinates\": [116.397, 39.908] }\n```", "<small>在世界地图上绘制一个点（顺序为经度、纬度）</small>"] },
    { ja: ["動画", "![動画の説明](clip.mp4)", "<small>文書と同じフォルダーの動画を再生します（.mp4・.webm・.mov など）</small>"],
      en: ["Video", "![video description](clip.mp4)", "<small>Plays a video beside the document (.mp4, .webm, .mov, and so on)</small>"],
      zh: ["视频", "![视频说明](clip.mp4)", "<small>播放与文档在同一文件夹中的视频（.mp4、.webm、.mov 等）</small>"] }
  ];
  const helpDialog = document.getElementById("help-dialog");
  const helpButton = document.getElementById("help-button");
  const helpRows = document.getElementById("help-rows");

  // Block examples start on their own line so they do not join the current text.
  const insertExample = (syntax, block) => {
    helpDialog.close();
    const before = view.state.sliceDoc(0, view.state.selection.main.from);
    const prefix = block && before && !before.endsWith("\n") ? "\n" : "";
    replaceSelection(prefix + syntax + (block ? "\n" : ""));
  };

  // The diagram examples are drawn after the table is built (Mermaid renders asynchronously).
  let helpDiagramCounter = 0;
  const renderHelpDiagrams = async (pending) => {
    if (!pending.length) return;
    if (!window.mermaid) {
      try {
        await loadMermaid();
      } catch {
        for (const { cell } of pending) cell.textContent = t("diagramError");
        return;
      }
    }
    useMermaidTheme(false);
    for (const { cell, source } of pending) {
      try {
        // A Gantt chart takes the whole page width; the small example is drawn narrower.
        const drawn = source.startsWith("gantt") ? `%%{init: {"gantt": {"useWidth": 420}}}%%\n${source}` : source;
        const { svg } = await window.mermaid.render(`help-diagram-${++helpDiagramCounter}`, drawn);
        cell.innerHTML = svg;
      } catch {
        cell.textContent = t("diagramError");
      }
    }
  };

  const renderHelpMath = async (pending) => {
    if (!pending.length) return;
    if (!window.katex) {
      try {
        await loadKatex();
      } catch {
        return;
      }
    }
    for (const { cell, item } of pending) {
      window.katex.render(item.math, cell, { throwOnError: false, displayMode: Boolean(item.display) });
    }
  };

  const renderHelp = () => {
    helpRows.replaceChildren();
    const diagrams = [];
    const formulas = [];
    for (const item of helpItems) {
      if (item.group) {
        const row = document.createElement("tr");
        row.className = "help-group";
        const heading = document.createElement("th");
        heading.colSpan = 4;
        heading.scope = "colgroup";
        heading.textContent = item.group[language];
        row.appendChild(heading);
        helpRows.appendChild(row);
        continue;
      }
      const [name, syntax, result] = item[language];
      const row = document.createElement("tr");
      const nameCell = document.createElement("th");
      nameCell.scope = "row";
      nameCell.textContent = name;
      const syntaxCell = document.createElement("td");
      const pre = document.createElement("pre");
      const code = document.createElement("code");
      code.textContent = syntax;
      pre.appendChild(code);
      syntaxCell.appendChild(pre);
      const resultCell = document.createElement("td");
      resultCell.className = "help-result";
      if (item.math) {
        // The TeX shows until KaTeX has loaded.
        resultCell.textContent = item.math;
        formulas.push({ cell: resultCell, item });
      } else if (item.mermaid) {
        resultCell.classList.add("help-diagram");
        diagrams.push({ cell: resultCell, source: syntax.replace(/^```mermaid\n/, "").replace(/\n```$/, "") });
      } else {
        resultCell.innerHTML = result || syntax;
      }
      const insertCell = document.createElement("td");
      const insert = document.createElement("button");
      insert.type = "button";
      insert.className = "help-insert";
      insert.textContent = t("helpInsert");
      insert.setAttribute("aria-label", t("helpInsertLabel")(name));
      insert.addEventListener("click", () => insertExample(syntax, item.block));
      insertCell.appendChild(insert);
      row.append(nameCell, syntaxCell, resultCell, insertCell);
      helpRows.appendChild(row);
    }
    renderHelpMath(formulas);
    renderHelpDiagrams(diagrams);
  };

  const openHelp = () => {
    renderHelp();
    helpDialog.showModal();
  };

  helpButton.addEventListener("click", openHelp);
  document.getElementById("help-close").addEventListener("click", () => helpDialog.close());
  // A click on the backdrop lands on the dialog element itself.
  helpDialog.addEventListener("click", (event) => {
    if (event.target === helpDialog) helpDialog.close();
  });

  document.getElementById("replace-button").addEventListener("click", openFindBar);
  document.getElementById("find-close").addEventListener("click", closeFindBar);
  document.getElementById("find-next").addEventListener("click", () => findText(true));
  document.getElementById("find-prev").addEventListener("click", () => findText(false));
  document.getElementById("replace-one").addEventListener("click", replaceOne);
  document.getElementById("replace-all").addEventListener("click", replaceAll);
  findBar.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      if (event.target === replaceInput) replaceOne();
      else findText(!event.shiftKey);
    } else if (event.key === "Escape") {
      event.preventDefault();
      event.stopPropagation();
      closeFindBar();
    }
  });

  // Settings dialog: reading comfort options that most people leave alone, so they
  // all start at the standard values and are opt-in. Kept per user in localStorage.
  const settingsDialog = document.getElementById("settings-dialog");
  const settingControls = {
    font: document.getElementById("setting-font"),
    size: document.getElementById("setting-size"),
    spacing: document.getElementById("setting-spacing"),
    fullWidth: document.getElementById("setting-full-width"),
    htmlSource: document.getElementById("setting-html-source"),
    activeLine: document.getElementById("setting-active-line"),
    thickCaret: document.getElementById("setting-thick-caret"),
    lint: document.getElementById("setting-lint"),
    reader: document.getElementById("setting-reader"),
    rate: document.getElementById("setting-reader-rate"),
    pause: document.getElementById("setting-reader-pause"),
    readerLang: document.getElementById("setting-reader-lang"),
    readerVoice: document.getElementById("setting-reader-voice"),
    mapBackground: document.getElementById("setting-map-tiles"),
    videoAutoplay: document.getElementById("setting-video-autoplay")
  };
  const backupCheckbox = document.getElementById("setting-backup");
  const zoomValue = document.getElementById("zoom-value");
  const readerControls = document.getElementById("reader-controls");
  const readerPlay = document.getElementById("reader-play");
  const readerStop = document.getElementById("reader-stop");
  const readerVoiceNote = document.getElementById("reader-voice-note");

  // Applies the font, size, line spacing, editor aids, and read-aloud visibility.
  let drawnMapTiles = displaySettings.mapBackground;
  let drawnAutoplay = displaySettings.videoAutoplay;
  // Maps waiting for "Load detailed maps", the consent the host keeps for this
  // document, and whether the notice was closed for this document.
  let mapsWaiting = 0;
  let mapsAllowed = false;
  let mapNoticeClosed = false;
  const applyDisplaySettings = (rerender = true) => {
    const root = document.documentElement;
    const fontChanged = root.classList.contains("font-ud") !== (displaySettings.font === "ud");
    root.classList.toggle("font-ud", displaySettings.font === "ud");
    root.style.setProperty("--doc-scale", displaySettings.size);
    root.style.setProperty("--doc-line-height", displaySettings.spacing);
    root.classList.toggle("preview-full-width", Boolean(displaySettings.fullWidth));
    view.dispatch({ effects: editorExtras.reconfigure(editorExtrasFor(displaySettings)) });
    showLintCount(displaySettings.lint ? lintCount : 0);
    readerControls.hidden = !displaySettings.reader;
    if (!displaySettings.reader) stopReading();
    updateVoiceNote();
    // Diagram labels use the UI font, so a font change redraws them; so does
    // a change of map background.
    const tilesChanged = drawnMapTiles !== displaySettings.mapBackground;
    drawnMapTiles = displaySettings.mapBackground;
    const autoplayChanged = drawnAutoplay !== displaySettings.videoAutoplay;
    drawnAutoplay = displaySettings.videoAutoplay;
    if (rerender && (fontChanged || tilesChanged || autoplayChanged) && lastPreviewHtml !== null) renderPreview(lastPreviewHtml);
  };

  const fillSettingControls = () => {
    for (const [key, control] of Object.entries(settingControls)) {
      if (control.type === "checkbox") control.checked = Boolean(displaySettings[key]);
      else control.value = String(displaySettings[key]);
    }
  };

  for (const [key, control] of Object.entries(settingControls)) {
    control.addEventListener("change", () => {
      displaySettings[key] = control.type === "checkbox" ? control.checked : control.value;
      saveDisplaySettings();
      applyDisplaySettings();
    });
  }
  backupCheckbox.addEventListener("change", () => post({ type: "setAutoBackup", enabled: backupCheckbox.checked }));
  // The strip opens the list of problems the Markdown check found (closes it when open).
  document.getElementById("lint-status").addEventListener("click", () => {
    if (view.dom.querySelector(".cm-panel-lint")) {
      closeLintPanel(view);
      view.focus();
    } else {
      forceLinting(view);
      openLintPanel(view);
    }
  });
  // PDF import: the host keeps these and uses them when a PDF is opened.
  const pdfFigureTextSelect = document.getElementById("setting-pdf-figure-text");
  const pdfAlignCheckbox = document.getElementById("setting-pdf-align");
  const pdfPagePicturesCheckbox = document.getElementById("setting-pdf-page-pictures");
  pdfFigureTextSelect.addEventListener("change", () => post({ type: "setPdfFigureText", value: pdfFigureTextSelect.value }));
  pdfAlignCheckbox.addEventListener("change", () => post({ type: "setPdfAlign", enabled: pdfAlignCheckbox.checked }));
  pdfPagePicturesCheckbox.addEventListener("change", () => post({ type: "setPdfPagePictures", enabled: pdfPagePicturesCheckbox.checked }));
  document.getElementById("setting-pdf-file").addEventListener("click", () => post({ type: "openPdfOptionsFile" }));
  document.getElementById("notice-reopen").addEventListener("click", () => {
    noticeState = null;
    updateNotice();
    post({ type: "reopenPdf" });
  });
  // Closer to GitHub changes how the host reads Markdown, so the host keeps it.
  const nearGitHubCheckbox = document.getElementById("setting-near-github");
  nearGitHubCheckbox.addEventListener("change", () => post({ type: "setNearGitHub", enabled: nearGitHubCheckbox.checked }));
  document.getElementById("zoom-in").addEventListener("click", () => post({ type: "zoom", action: "in" }));
  document.getElementById("zoom-out").addEventListener("click", () => post({ type: "zoom", action: "out" }));
  document.getElementById("zoom-reset").addEventListener("click", () => post({ type: "zoom", action: "reset" }));
  document.getElementById("settings-reset").addEventListener("click", () => {
    displaySettings = { ...displayDefaults };
    saveDisplaySettings();
    fillSettingControls();
    applyDisplaySettings();
    post({ type: "zoom", action: "reset" });
  });

  // Settings > New from clipboard: Ctrl+Alt+letter or digit, given to a desktop shortcut
  // the host makes (after asking) to start mdEditor with --paste.
  const pasteKeySelect = document.getElementById("setting-paste-key");
  const pasteKeyNote = document.getElementById("paste-key-note");
  let pasteKey = { key: "", exists: false };
  for (const character of "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") {
    const option = document.createElement("option");
    option.value = character;
    option.textContent = `Ctrl+Alt+${character}`;
    pasteKeySelect.appendChild(option);
  }
  const showPasteKey = () => {
    // A shortcut the user deleted shows as None, with a note, so choosing the key again remakes it.
    pasteKeySelect.value = pasteKey.exists ? pasteKey.key : "";
    const key = `Ctrl+Alt+${pasteKey.key}`;
    pasteKeyNote.textContent = pasteKey.exists ? t("pasteKeyReady")(key)
      : pasteKey.key ? t("pasteKeyMissing")(key) : t("pasteKeyNote");
  };
  pasteKeySelect.addEventListener("change", () => post({ type: "setPasteHotkey", key: pasteKeySelect.value }));

  const openSettings = () => {
    fillVoiceOptions();
    fillSettingControls();
    updateVoiceNote();
    showPasteKey();
    post({ type: "getPasteHotkey" });
    settingsDialog.showModal();
  };
  document.getElementById("settings-button").addEventListener("click", openSettings);
  document.getElementById("settings-close").addEventListener("click", () => settingsDialog.close());
  settingsDialog.addEventListener("click", (event) => {
    if (event.target === settingsDialog) settingsDialog.close();
  });

  // Read-aloud (optional): reads the preview from the first visible block and
  // highlights what is being read, so sound and screen match (Identity 7-4).
  // Only voices installed on this PC are used, so the text never leaves it.
  const readableSelector = "h1, h2, h3, h4, h5, h6, p, li, blockquote, pre, th, td";
  let reader = null;
  // Kana means Japanese; Han characters without kana are read as Chinese.
  const textLanguage = (text) => /[぀-ヿ]/.test(text) ? "ja" : /[㐀-鿿]/.test(text) ? "zh" : "en";
  const localVoices = (lang) => speechSynthesis.getVoices()
    .filter((voice) => voice.localService && voice.lang.toLowerCase().startsWith(lang));
  const localVoice = (lang) => localVoices(lang)[0];
  // The voice for a paragraph: the chosen language (or the paragraph's own in
  // Automatic), and the chosen voice when it is installed for that language.
  const voiceFor = (text) => {
    const fixed = languages.includes(displaySettings.readerLang);
    const lang = fixed ? displaySettings.readerLang : textLanguage(text);
    const voices = localVoices(lang);
    const voice = voices.find((item) => item.name === displaySettings.readerVoice) ?? voices[0];
    return { lang, voice: voice ?? (fixed ? undefined : localVoice(language)) };
  };
  const readerVoiceSelect = document.getElementById("setting-reader-voice");
  const fillVoiceOptions = () => {
    const lang = displaySettings.readerLang;
    const voices = languages.includes(lang) ? localVoices(lang) : languages.flatMap((code) => localVoices(code));
    readerVoiceSelect.replaceChildren();
    const automatic = document.createElement("option");
    automatic.value = "";
    automatic.textContent = t("readerVoiceAuto");
    readerVoiceSelect.appendChild(automatic);
    for (const voice of voices) {
      const option = document.createElement("option");
      option.value = voice.name;
      option.textContent = `${voice.name}（${voice.lang}）`;
      readerVoiceSelect.appendChild(option);
    }
    readerVoiceSelect.value = voices.some((voice) => voice.name === displaySettings.readerVoice) ? displaySettings.readerVoice : "";
  };
  speechSynthesis.addEventListener("voiceschanged", fillVoiceOptions);
  document.getElementById("setting-reader-lang").addEventListener("change", () => {
    displaySettings.readerVoice = "";
    saveDisplaySettings();
    fillVoiceOptions();
  });

  const updateVoiceNote = () => {
    const ja = localVoice("ja");
    const en = localVoice("en");
    const zh = localVoice("zh");
    readerVoiceNote.textContent = ja || en || zh
      ? t("readerVoices")(ja?.name ?? t("readerNoVoiceShort"), en?.name ?? t("readerNoVoiceShort"), zh?.name ?? t("readerNoVoiceShort"))
      : t("readerNoVoice");
  };
  speechSynthesis.addEventListener("voiceschanged", updateVoiceNote);

  const readableBlocks = () => Array.from(preview.querySelectorAll(readableSelector)).filter((element) =>
    !element.querySelector(readableSelector) && !element.closest(".mermaid, .katex-display") && element.textContent.trim());

  // Text to speak: rendered math keeps only its plain-text form.
  const readableText = (element) => {
    const copy = element.cloneNode(true);
    copy.querySelectorAll(".katex-html, svg").forEach((node) => node.remove());
    return copy.textContent.replace(/\s+/g, " ").trim();
  };

  const updateReaderButtons = () => {
    readerPlay.textContent = !reader ? t("readerPlay") : reader.paused ? t("readerResume") : t("readerPause");
    readerPlay.setAttribute("aria-pressed", String(Boolean(reader && !reader.paused)));
    readerStop.disabled = !reader;
  };

  const clearReadingMark = () => preview.querySelectorAll(".reading").forEach((element) => element.classList.remove("reading"));

  const stopReading = () => {
    if (reader) clearTimeout(reader.timer);
    reader = null;
    speechSynthesis.cancel();
    clearReadingMark();
    updateReaderButtons();
  };

  // Read-aloud reads the selection when there is one: text selected in the preview, or
  // in the editor when it has the focus (or had it just before the button was pressed).
  // Without a selection it reads paragraphs from the first one in view.
  let editorFocusedBeforeButton = false;
  readerPlay.addEventListener("pointerdown", () => { editorFocusedBeforeButton = view.hasFocus; });

  // Markdown written in the editor, as words to say: marks, link addresses, and code
  // fences are left out.
  const spokenMarkdown = (markdown) => markdown
    .split("\n")
    .filter((line) => !/^\s*(```|~~~)/.test(line))
    .map((line) => line
      .replace(/^\s{0,3}#{1,6}\s+/, "")
      .replace(/^\s*>\s?/, "")
      .replace(/^\s*(?:[-*+]|\d+[.)])\s+(?:\[[ xX]\]\s+)?/, "")
      .replace(/!\[([^\]]*)\]\([^)]*\)/g, "$1")
      .replace(/\[([^\]]+)\]\([^)]*\)/g, "$1")
      .replace(/<[^>]+>/g, "")
      .replace(/(\*\*|__|~~|==|\*|_|`)/g, "")
      .replace(/\|/g, " ")
      .trim())
    .filter((line) => line && !/^[-:\s]+$/.test(line))
    .join("\n");

  const selectedText = () => {
    const selection = window.getSelection();
    if (selection && !selection.isCollapsed && selection.rangeCount > 0
        && preview.contains(selection.getRangeAt(0).commonAncestorContainer)) {
      const holder = document.createElement("div");
      for (let i = 0; i < selection.rangeCount; i++) holder.appendChild(selection.getRangeAt(i).cloneContents());
      return readableText(holder);
    }
    const range = view.state.selection.main;
    if (!range.empty && (view.hasFocus || editorFocusedBeforeButton)) {
      return spokenMarkdown(view.state.sliceDoc(range.from, range.to));
    }
    return "";
  };

  // A long selection is spoken a sentence or two at a time: speech engines may stop
  // partway through very long text.
  const spokenChunks = (text) => {
    const sentences = text.replace(/\s+/g, " ").match(/[^。．！？!?.]+[。．！？!?.]*\s*/g) ?? [text];
    const chunks = [];
    for (const sentence of sentences) {
      if (chunks.length > 0 && (chunks[chunks.length - 1] + sentence).length <= 200) {
        chunks[chunks.length - 1] += sentence;
      } else {
        chunks.push(sentence);
      }
    }
    return chunks.map((chunk) => chunk.trim()).filter(Boolean);
  };

  const speakNext = () => {
    if (!reader) return;
    reader.timer = 0;
    reader.waiting = false;
    const block = reader.blocks[reader.index];
    if (!block || (typeof block !== "string" && !block.isConnected)) {
      stopReading();
      return;
    }
    let text = block;
    if (typeof block !== "string") {
      // A paragraph of the preview is marked while it is read; a selection stays as selected.
      clearReadingMark();
      block.classList.add("reading");
      block.scrollIntoView({ block: "nearest" });
      text = readableText(block);
    }
    const { lang, voice } = voiceFor(text);
    if (!voice) {
      stopReading();
      showNoticeMessage(t("readerNoVoiceFor")(lang));
      return;
    }
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.voice = voice;
    utterance.lang = voice.lang;
    utterance.rate = Number(displaySettings.rate);
    utterance.onend = () => {
      if (!reader) return;
      reader.index += 1;
      reader.waiting = true;
      reader.timer = setTimeout(speakNext, Number(displaySettings.pause) * 1000);
    };
    speechSynthesis.speak(utterance);
  };

  const toggleReading = () => {
    if (!displaySettings.reader) return;
    if (!reader) {
      const selected = selectedText();
      editorFocusedBeforeButton = false;
      if (selected) {
        reader = { blocks: spokenChunks(selected), index: 0, paused: false, timer: 0, waiting: false, selection: true };
        speakNext();
        updateReaderButtons();
        return;
      }
      const blocks = readableBlocks();
      if (blocks.length === 0) return;
      const top = preview.getBoundingClientRect().top;
      const first = blocks.findIndex((element) => element.getBoundingClientRect().bottom > top + 4);
      reader = { blocks, index: Math.max(0, first), paused: false, timer: 0, waiting: false };
      speakNext();
    } else if (reader.paused) {
      reader.paused = false;
      if (reader.waiting) speakNext(); else speechSynthesis.resume();
    } else {
      reader.paused = true;
      if (reader.waiting) clearTimeout(reader.timer); else speechSynthesis.pause();
    }
    updateReaderButtons();
  };
  readerPlay.addEventListener("click", toggleReading);
  readerStop.addEventListener("click", stopReading);

  // Keyboard shortcuts for every command (Identity 7-3 (2)): each can be changed
  // or removed in the "Key operations" dialog, and all of them can be turned off.
  // Only Ctrl + letter/digit and F keys are allowed so typing is never captured.
  const shortcutDefinitions = [
    { id: "open", keys: "Ctrl+O", button: "open-button", run: () => post({ type: "open" }) },
    { id: "restoreDiscarded", keys: "Ctrl+Shift+B", button: "restore-button", run: () => post({ type: "restoreDiscarded" }) },
    { id: "openUrl", keys: "Ctrl+Shift+K", button: "url-button", run: () => openUrlDialog() },
    { id: "save", keys: "Ctrl+S", button: "save-button", run: () => post({ type: "save" }) },
    { id: "saveAs", keys: "Ctrl+Shift+S", button: "save-as-button", run: () => saveAs() },
    // These save straight to one format (the same formats are in Save As).
    { id: "pdf", keys: "Ctrl+Shift+P", run: () => exportPdf() },
    { id: "html", keys: "Ctrl+Shift+M", run: () => exportHtml() },
    { id: "png", keys: "Ctrl+Shift+G", run: () => post({ type: "exportPng" }) },
    { id: "docx", keys: "Ctrl+Shift+D", run: () => post({ type: "exportDocx" }) },
    { id: "paper", keys: "Ctrl+Shift+A", button: "paper-button", run: () => paperButton.click() },
    { id: "theme", keys: "Ctrl+Shift+T", button: "theme-button", run: () => themeButton.click() },
    // Ctrl+Shift+L moves to the next language in the list (Japanese, English, Chinese).
    { id: "language", keys: "Ctrl+Shift+L", button: "language-select", run: () => setLanguage(languages[(languages.indexOf(language) + 1) % languages.length]) },
    { id: "encoding", keys: "Ctrl+Shift+E", button: "encoding-button", run: () => openEncodingDialog() },
    { id: "findReplace", keys: "Ctrl+H", button: "replace-button", run: () => openFindBar() },
    { id: "toc", keys: "Ctrl+Shift+J", button: "toc-button", run: () => post({ type: "toc" }) },
    { id: "image", keys: "Ctrl+Shift+F", button: "image-button", run: (at) => openImageDialog(at) },
    { id: "findNext", keys: "F3", button: "find-next", run: () => (findBar.hidden ? openFindBar() : findText(true)) },
    { id: "findPrev", keys: "Shift+F3", button: "find-prev", run: () => (findBar.hidden ? openFindBar() : findText(false)) },
    { id: "replaceAll", keys: "Ctrl+Shift+H", button: "replace-all", run: () => (findBar.hidden || !findInput.value ? openFindBar() : replaceAll()) },
    { id: "help", keys: "F1", button: "help-button", run: () => openHelp() },
    { id: "shortcuts", keys: "Shift+F1", button: "shortcuts-button", run: () => openShortcutDialog() },
    { id: "fullscreenEditor", keys: "Ctrl+Shift+1", selector: "[data-fullscreen-mode='editor']", run: () => setDisplayMode(displayMode === "editor" ? "normal" : "editor") },
    { id: "fullscreenPreview", keys: "Ctrl+Shift+2", selector: "[data-fullscreen-mode='preview']", run: () => { if (!previewDetached) setDisplayMode(displayMode === "preview" ? "normal" : "preview"); } },
    { id: "nextPane", keys: "F6", run: () => movePaneFocus(1) },
    { id: "previousPane", keys: "Shift+F6", run: () => movePaneFocus(-1) },
    { id: "windowFullscreen", keys: "F11", run: () => post({ type: "toggleWindowFullscreen" }) },
    { id: "settings", keys: "Ctrl+Shift+O", button: "settings-button", run: () => openSettings() },
    { id: "readAloud", keys: "Ctrl+Shift+Y", button: "reader-play", run: () => (previewDetached ? post({ type: "popupCommand", id: "readAloud" }) : toggleReading()) },
    { id: "stopReading", keys: "Ctrl+Shift+U", button: "reader-stop", run: () => (previewDetached ? post({ type: "popupCommand", id: "stopReading" }) : stopReading()) },
    { id: "detachPreview", keys: "Ctrl+Shift+3", selector: "[data-detach]", run: () => toggleDetachedPreview() }
  ];
  // In the preview window these run there; every other command goes to the main window.
  const previewWindowCommands = new Set(["readAloud", "stopReading", "detachPreview", "windowFullscreen", "nextPane", "previousPane"]);
  // Editing keys and the window's find stay with the editor and the browser.
  const reservedShortcuts = new Set(["Ctrl+A", "Ctrl+C", "Ctrl+F", "Ctrl+V", "Ctrl+X", "Ctrl+Y", "Ctrl+Z", "Ctrl+Shift+Z"]);
  const shortcutStorageKey = "mdeditor-shortcuts";
  const loadShortcutSettings = () => {
    try {
      const saved = JSON.parse(localStorage.getItem(shortcutStorageKey) || "{}");
      return { enabled: saved.enabled !== false, keys: saved.keys && typeof saved.keys === "object" ? saved.keys : {} };
    } catch {
      return { enabled: true, keys: {} };
    }
  };
  let shortcutSettings = loadShortcutSettings();
  const saveShortcutSettings = () => {
    try { localStorage.setItem(shortcutStorageKey, JSON.stringify(shortcutSettings)); } catch { /* kept for this session */ }
  };
  const shortcutKeys = (definition) =>
    Object.hasOwn(shortcutSettings.keys, definition.id) ? shortcutSettings.keys[definition.id] : definition.keys;

  // "Ctrl+Shift+E" style name for a key press, or null when it cannot be a shortcut.
  const shortcutFromEvent = (event) => {
    let key = null;
    if (/^Key[A-Z]$/.test(event.code)) key = event.code.slice(3);
    else if (/^Digit[0-9]$/.test(event.code)) key = event.code.slice(5);
    else if (/^F([1-9]|1[0-2])$/.test(event.key)) key = event.key;
    // On-screen keyboards and voice input can send keys without a physical key
    // code; fall back to the key number so their shortcuts still work.
    else if (!event.code && event.keyCode >= 65 && event.keyCode <= 90) key = String.fromCharCode(event.keyCode);
    else if (!event.code && event.keyCode >= 48 && event.keyCode <= 57) key = String.fromCharCode(event.keyCode);
    if (!key) return null;
    const parts = [];
    if (event.ctrlKey) parts.push("Ctrl");
    if (event.altKey) parts.push("Alt");
    if (event.shiftKey) parts.push("Shift");
    return [...parts, key].join("+");
  };
  // aria-keyshortcuts uses "Control" for Ctrl.
  const ariaShortcut = (keys) => keys.replace(/\bCtrl\b/g, "Control");

  const movePaneFocus = (step) => {
    const stops = [document.getElementById("open-button"), view.contentDOM, preview]
      .filter((element) => element.getClientRects().length > 0);
    const current = stops.findIndex((element) => element === document.activeElement || element.contains(document.activeElement));
    const next = stops[(current + step + stops.length) % stops.length];
    if (next === view.contentDOM) view.focus(); else next.focus();
  };

  const shortcutButtons = (definition) => definition.button
    ? [document.getElementById(definition.button)]
    : definition.selector ? Array.from(document.querySelectorAll(definition.selector)) : [];

  // Buttons show their key in the tooltip and tell screen readers through aria-keyshortcuts.
  const applyShortcutHints = () => {
    for (const definition of shortcutDefinitions) {
      const keys = shortcutSettings.enabled ? shortcutKeys(definition) : "";
      for (const button of shortcutButtons(definition)) {
        if (!button) continue;
        const name = t("shortcutNames")[definition.id];
        button.title = keys ? `${name} (${keys})` : name;
        if (keys) button.setAttribute("aria-keyshortcuts", ariaShortcut(keys));
        else button.removeAttribute("aria-keyshortcuts");
      }
    }
  };

  const shortcutDialog = document.getElementById("shortcut-dialog");
  const shortcutRows = document.getElementById("shortcut-rows");
  const shortcutStatus = document.getElementById("shortcut-status");
  const shortcutEnabled = document.getElementById("shortcut-enabled");
  let capturing = null;

  const renderShortcutRows = () => {
    shortcutRows.replaceChildren();
    for (const definition of shortcutDefinitions) {
      const name = t("shortcutNames")[definition.id];
      const keys = shortcutKeys(definition);
      const row = document.createElement("tr");
      const nameCell = document.createElement("th");
      nameCell.scope = "row";
      nameCell.textContent = name;
      const keyCell = document.createElement("td");
      const kbd = document.createElement("kbd");
      kbd.textContent = capturing === definition.id ? t("shortcutPress") : keys || t("shortcutNone");
      keyCell.appendChild(kbd);
      const actionCell = document.createElement("td");
      actionCell.className = "shortcut-buttons";
      const change = document.createElement("button");
      change.type = "button";
      change.textContent = t("shortcutChange");
      change.setAttribute("aria-label", t("shortcutChangeLabel")(name));
      change.addEventListener("click", () => {
        capturing = definition.id;
        shortcutStatus.textContent = t("shortcutPressFor")(name);
        renderShortcutRows();
        shortcutRows.querySelector(`[data-capture="${definition.id}"]`)?.focus();
      });
      change.dataset.capture = definition.id;
      const remove = document.createElement("button");
      remove.type = "button";
      remove.textContent = t("shortcutRemove");
      remove.setAttribute("aria-label", t("shortcutRemoveLabel")(name));
      remove.disabled = !keys;
      remove.addEventListener("click", () => {
        shortcutSettings.keys[definition.id] = "";
        saveShortcutSettings();
        shortcutStatus.textContent = t("shortcutRemoved")(name);
        renderShortcutRows();
        applyShortcutHints();
      });
      actionCell.append(change, remove);
      row.append(nameCell, keyCell, actionCell);
      shortcutRows.appendChild(row);
    }
  };

  // While capturing, the next key press becomes the shortcut (Esc cancels).
  shortcutDialog.addEventListener("keydown", (event) => {
    if (!capturing) return;
    if (["Control", "Shift", "Alt", "Meta"].includes(event.key)) return;
    event.preventDefault();
    event.stopPropagation();
    const definition = shortcutDefinitions.find((item) => item.id === capturing);
    const name = t("shortcutNames")[definition.id];
    if (event.key === "Escape") {
      capturing = null;
      shortcutStatus.textContent = t("shortcutCancelled");
      renderShortcutRows();
      shortcutRows.querySelector(`[data-capture="${definition.id}"]`)?.focus();
      return;
    }
    const keys = shortcutFromEvent(event);
    const isFunctionKey = keys && /\bF([1-9]|1[0-2])$/.test(keys);
    if (!keys || (!isFunctionKey && !event.ctrlKey) || (event.ctrlKey && event.altKey)) {
      shortcutStatus.textContent = t("shortcutInvalid");
      return;
    }
    if (reservedShortcuts.has(keys)) {
      shortcutStatus.textContent = t("shortcutReserved")(keys);
      return;
    }
    const other = shortcutDefinitions.find((item) => item.id !== definition.id && shortcutKeys(item) === keys);
    if (other) {
      shortcutStatus.textContent = t("shortcutConflict")(keys, t("shortcutNames")[other.id]);
      return;
    }
    shortcutSettings.keys[definition.id] = keys;
    saveShortcutSettings();
    capturing = null;
    shortcutStatus.textContent = t("shortcutSaved")(name, keys);
    renderShortcutRows();
    applyShortcutHints();
    shortcutRows.querySelector(`[data-capture="${definition.id}"]`)?.focus();
  }, true);

  const samples = [
    { ja: "図と数式", en: "Diagrams and math", zh: "图表和公式",
      text: {
        ja: "# 図と数式のサンプル\n\nMarkdown を書くと、右のプレビューにすぐ反映されます。\n\n## 図（Mermaid）\n\n```mermaid\nflowchart LR\n  A[書く] --> B[確認する]\n  B --> C[PDFに保存]\n```\n\n## 数式（KaTeX）\n\n円の面積は $S = \\pi r^2$ です。\n\n$$\\int_0^1 x^2\\,dx = \\frac{1}{3}$$\n\n## 表とチェックリスト\n\n| 名前 | 数 |\n| --- | ---: |\n| りんご | 3 |\n| みかん | 5 |\n\n- [x] 図を見る\n- [ ] PDFに保存する\n",
        en: "# Diagrams and math sample\n\nWhat you type appears in the preview on the right right away.\n\n## Diagram (Mermaid)\n\n```mermaid\nflowchart LR\n  A[Write] --> B[Check]\n  B --> C[Save as PDF]\n```\n\n## Math (KaTeX)\n\nThe area of a circle is $S = \\pi r^2$.\n\n$$\\int_0^1 x^2\\,dx = \\frac{1}{3}$$\n\n## Table and checklist\n\n| Name | Qty |\n| --- | ---: |\n| Apple | 3 |\n| Orange | 5 |\n\n- [x] See the diagram\n- [ ] Save as PDF\n",
        zh: "# 图表和公式示例\n\n写下 Markdown，右边的预览会立即更新。\n\n## 图表（Mermaid）\n\n```mermaid\nflowchart LR\n  A[写作] --> B[检查]\n  B --> C[保存为 PDF]\n```\n\n## 公式（KaTeX）\n\n圆的面积是 $S = \\pi r^2$。\n\n$$\\int_0^1 x^2\\,dx = \\frac{1}{3}$$\n\n## 表格和任务列表\n\n| 名称 | 数量 |\n| --- | ---: |\n| 苹果 | 3 |\n| 橘子 | 5 |\n\n- [x] 查看图表\n- [ ] 保存为 PDF\n" } },
    { ja: "基本の書き方", en: "Basic writing", zh: "基本写法",
      text: {
        ja: "# はじめての Markdown\n\n**太字**、*斜体*、[リンク](https://example.com) が使えます。\n\n1. 見出し\n2. リスト\n3. 引用\n\n> 引用はこのように書きます。\n\n```\nコードはこのように書きます。\n```\n",
        en: "# Your first Markdown\n\nYou can use **bold**, *italic*, and [links](https://example.com).\n\n1. Headings\n2. Lists\n3. Quotes\n\n> Quotes look like this.\n\n```\nCode looks like this.\n```\n",
        zh: "# 第一次使用 Markdown\n\n可以使用**粗体**、*斜体*和[链接](https://example.com)。\n\n1. 标题\n2. 列表\n3. 引用\n\n> 引用这样写。\n\n```\n代码这样写。\n```\n" } },
    // The full rendering tests from samples/ (bundled with the app as wwwroot/samples/).
    { ja: "図・チャート表示テスト（すべての種類・日本語）", en: "Diagram and chart test (every type, Japanese)", zh: "图表显示测试（所有类型，日语）", file: "samples/diagram-test.md" },
    { ja: "図・チャート表示テスト（すべての種類・英語）", en: "Diagram and chart test (every type, English)", zh: "图表显示测试（所有类型，英语）", file: "samples/diagram-test.en.md" },
    { ja: "図・チャート表示テスト（すべての種類・中国語）", en: "Diagram and chart test (every type, Chinese)", zh: "图表显示测试（所有类型，中文）", file: "samples/diagram-test.zh-CN.md" }
  ];
  const sampleDialog = document.getElementById("sample-dialog");
  const sampleList = document.getElementById("sample-list");
  const openSampleDialog = () => {
    sampleList.replaceChildren();
    for (const sample of samples) {
      const name = sample[language];
      const item = document.createElement("li");
      const label = document.createElement("span");
      label.textContent = name;
      const button = document.createElement("button");
      button.type = "button";
      button.textContent = t("sampleOpen");
      button.setAttribute("aria-label", t("sampleOpenLabel")(name));
      if (sample.file) button.dataset.file = sample.file;
      button.addEventListener("click", async () => {
        sampleDialog.close();
        let text = sample.text?.[language];
        if (sample.file) {
          try {
            const response = await fetch(sample.file);
            if (!response.ok) throw new Error(String(response.status));
            text = await response.text();
          } catch {
            showNoticeMessage(t("sampleLoadFailed"));
            return;
          }
        }
        // A sample opens as a new, unsaved document under its own name (the app
        // asks to save changes to the open document first), never over the open file.
        const fileName = sample.file ? sample.file.split("/").pop() : `${name}.md`;
        post({ type: "openSample", text, name: fileName });
      });
      item.append(label, button);
      sampleList.appendChild(item);
    }
    sampleDialog.showModal();
  };
  document.getElementById("sample-button").addEventListener("click", openSampleDialog);
  document.getElementById("sample-close").addEventListener("click", () => sampleDialog.close());
  sampleDialog.addEventListener("click", (event) => {
    if (event.target === sampleDialog) sampleDialog.close();
  });

  const aboutDialog = document.getElementById("about-dialog");
  document.getElementById("about-button").addEventListener("click", () => aboutDialog.showModal());
  document.getElementById("about-close").addEventListener("click", () => aboutDialog.close());
  aboutDialog.addEventListener("click", (event) => {
    if (event.target === aboutDialog) aboutDialog.close();
    const link = event.target.closest?.("a");
    if (link) {
      event.preventDefault();
      post({ type: "externalLink", url: link.href });
    }
  });

  const openShortcutDialog = () => {
    capturing = null;
    shortcutStatus.textContent = "";
    shortcutEnabled.checked = shortcutSettings.enabled;
    renderShortcutRows();
    shortcutDialog.showModal();
  };
  document.getElementById("shortcuts-button").addEventListener("click", openShortcutDialog);
  document.getElementById("shortcut-close").addEventListener("click", () => shortcutDialog.close());
  shortcutDialog.addEventListener("click", (event) => {
    if (event.target === shortcutDialog) shortcutDialog.close();
  });
  shortcutDialog.addEventListener("close", () => { capturing = null; });
  shortcutEnabled.addEventListener("change", () => {
    shortcutSettings.enabled = shortcutEnabled.checked;
    saveShortcutSettings();
    shortcutStatus.textContent = t(shortcutEnabled.checked ? "shortcutTurnedOn" : "shortcutTurnedOff");
    applyShortcutHints();
  });
  document.getElementById("shortcut-reset").addEventListener("click", () => {
    shortcutSettings = { enabled: true, keys: {} };
    saveShortcutSettings();
    shortcutEnabled.checked = true;
    shortcutStatus.textContent = t("shortcutResetDone");
    renderShortcutRows();
    applyShortcutHints();
  });

  // Ctrl + 0 goes back to the app's default zoom (90%) instead of the browser's 100%.
  document.addEventListener("keydown", (event) => {
    if (event.ctrlKey && !event.altKey && !event.shiftKey && (event.code === "Digit0" || event.code === "Numpad0")) {
      event.preventDefault();
      post({ type: "zoom", action: "reset" });
    }
  }, true);

  document.addEventListener("keydown", (event) => {
    // The open dialog handles Esc itself; preventing it here would keep it open.
    if (document.querySelector("dialog[open]")) return;
    // Esc always restores the layout or leaves full screen; it is not configurable.
    if (event.key === "Escape" && !event.ctrlKey && !event.altKey && !event.shiftKey) {
      event.preventDefault();
      if (reader) {
        stopReading();
      } else if (displayMode !== "normal" && !previewWindow) {
        setDisplayMode("normal");
      } else {
        post({ type: "exitWindowFullscreen" });
      }
      return;
    }
    if (!shortcutSettings.enabled) return;
    const keys = shortcutFromEvent(event);
    const definition = keys && shortcutDefinitions.find((item) => shortcutKeys(item) === keys);
    if (!definition) return;
    event.preventDefault();
    if (previewWindow && !previewWindowCommands.has(definition.id)) post({ type: "command", id: definition.id });
    else definition.run();
  });

  // Scroll sync between the editor and the preview. The host marks each preview
  // block with data-line (its source line); CodeMirror knows where each source
  // line sits in the editor, wrapped lines included.
  let ignoreScrollOf = null;
  let ignoreTimer = 0;
  let syncFrame = 0;

  // Fractional source line (0-based) at the editor's top edge, and the reverse.
  const editorLineAtTop = () => {
    const y = Math.max(0, editorScroller.scrollTop - view.documentPadding.top);
    const block = view.lineBlockAtHeight(y);
    const line = view.state.doc.lineAt(block.from).number - 1;
    return line + Math.min(1, Math.max(0, (y - block.top) / Math.max(1, block.height)));
  };
  const editorTopOfLine = (line) => {
    const doc = view.state.doc;
    const index = Math.min(doc.lines - 1, Math.max(0, Math.floor(line)));
    const block = view.lineBlockAt(doc.line(index + 1).from);
    return block.top + (line - index) * block.height + view.documentPadding.top;
  };

  // Preview blocks as (line, top) points, kept in increasing order.
  const previewAnchors = () => {
    const base = preview.getBoundingClientRect().top - preview.scrollTop;
    const points = [{ line: 0, top: 0 }];
    for (const element of preview.querySelectorAll("[data-line]")) {
      const rect = element.getBoundingClientRect();
      if (rect.width === 0 && rect.height === 0) continue;
      const point = { line: Number(element.dataset.line), top: rect.top - base };
      const last = points[points.length - 1];
      if (point.line >= last.line && point.top >= last.top) points.push(point);
    }
    points.push({ line: previewLines || view.state.doc.lines, top: preview.scrollHeight });
    return points;
  };
  const interpolate = (points, value, from, to) => {
    let index = 0;
    while (index < points.length - 2 && points[index + 1][from] <= value) index += 1;
    const a = points[index];
    const b = points[index + 1];
    const span = b[from] - a[from];
    return span > 0 ? a[to] + ((value - a[from]) / span) * (b[to] - a[to]) : a[to];
  };

  const scrollWithoutEcho = (element, top) => {
    ignoreScrollOf = element;
    element.scrollTop = Math.max(0, top);
    clearTimeout(ignoreTimer);
    ignoreTimer = setTimeout(() => { ignoreScrollOf = null; }, 120);
  };

  // A pane at its top or bottom edge follows to the same edge.
  const scrollEdge = (element) => {
    if (element.scrollTop <= 0) return "top";
    if (element.scrollTop >= element.scrollHeight - element.clientHeight - 1) return "bottom";
    return null;
  };
  const scrollPreviewToLine = (line, edge) => {
    if (edge) scrollWithoutEcho(preview, edge === "top" ? 0 : preview.scrollHeight);
    else scrollWithoutEcho(preview, interpolate(previewAnchors(), line, "line", "top"));
  };
  const scrollEditorToLine = (line, edge) => {
    if (edge) scrollWithoutEcho(editorScroller, edge === "top" ? 0 : editorScroller.scrollHeight);
    else scrollWithoutEcho(editorScroller, editorTopOfLine(line));
  };

  // When the preview has its own window, the position (a source line, not
  // pixels, so different window sizes and displays still match) goes through
  // the host to the other window.
  let lastSyncedLine = { line: 0, edge: "top" };
  const syncFrom = (source) => {
    if (source.clientHeight === 0) return;
    const edge = scrollEdge(source);
    if (previewWindow || previewDetached) {
      const line = edge ? 0
        : source === editorScroller ? editorLineAtTop()
          : interpolate(previewAnchors(), preview.scrollTop, "top", "line");
      lastSyncedLine = { line, edge };
      post({ type: "syncLine", line, edge });
      return;
    }
    if (editorScroller.clientHeight === 0 || preview.clientHeight === 0) return;
    if (source === editorScroller) scrollPreviewToLine(editorLineAtTop(), edge);
    else scrollEditorToLine(interpolate(previewAnchors(), preview.scrollTop, "top", "line"), edge);
  };

  const onPaneScroll = (event) => {
    if (event.currentTarget === ignoreScrollOf) return;
    const source = event.currentTarget;
    cancelAnimationFrame(syncFrame);
    syncFrame = requestAnimationFrame(() => syncFrom(source));
  };
  editorScroller.addEventListener("scroll", onPaneScroll);
  preview.addEventListener("scroll", onPaneScroll);

  // Links to headings in the same document (#heading) scroll the preview there.
  const scrollToAnchor = (name) => {
    const target = Array.from(preview.querySelectorAll("[data-anchor]"))
      .find((element) => element.dataset.anchor === name);
    if (!target) return;
    target.scrollIntoView({ block: "start" });
    target.setAttribute("tabindex", "-1");
    target.focus({ preventScroll: true });
  };

  preview.addEventListener("click", (event) => {
    const link = event.target.closest("a");
    if (!link || !link.href) {
      return;
    }

    const href = link.getAttribute("href") ?? "";
    if (href.startsWith("#")) {
      event.preventDefault();
      try {
        scrollToAnchor(decodeURIComponent(href.slice(1)));
      } catch {
        // A malformed escape in the link: leave the preview where it is.
      }
      return;
    }

    const url = new URL(link.href);
    // A relative link (such as ./LICENSE or other.md) points to a file beside the
    // document. Navigating the app page there would leave it on an error page, so
    // the host opens the file instead.
    if (url.origin === window.location.origin) {
      event.preventDefault();
      post({ type: "documentLink", href });
      return;
    }

    const isExternalWebLink = (url.protocol === "http:" || url.protocol === "https:")
      && url.origin !== window.location.origin;
    if (!isExternalWebLink && url.protocol !== "mailto:") {
      return;
    }

    event.preventDefault();
    post({ type: "externalLink", url: url.href });
  });

  // Rendered SVG per Mermaid theme, keyed by source, so unchanged diagrams are
  // restored synchronously on every preview update without flicker or scroll jumps.
  const diagramCaches = { default: new Map(), dark: new Map() };
  let diagramCounter = 0;
  let renderGeneration = 0;
  let mermaidConfig = null;
  let mermaidWanted = null;
  let lastPreviewHtml = null;
  const darkScheme = window.matchMedia("(prefers-color-scheme: dark)");

  // Mermaid (about 3.5 MB) and KaTeX are loaded the first time a document (or the
  // Syntax list) needs them, so a launch does not wait for them.
  const loadLibrary = (src) => new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = src;
    script.onload = resolve;
    script.onerror = () => reject(new Error(src));
    document.head.appendChild(script);
  });
  let mermaidLoading = null;
  let katexLoading = null;
  const loadMermaid = () => mermaidLoading ??= loadLibrary("lib/mermaid/mermaid.min.js")
    .then(() => applyMermaidConfig())
    .catch((error) => {
      mermaidLoading = null;
      throw error;
    });
  const loadKatex = () => katexLoading ??= loadLibrary("lib/katex/katex.min.js").catch((error) => {
    katexLoading = null;
    throw error;
  });

  const applyMermaidConfig = () => {
    if (!window.mermaid || !mermaidWanted) return;
    const { theme, fontFamily } = mermaidWanted;
    if (mermaidConfig?.theme !== theme || mermaidConfig?.fontFamily !== fontFamily) {
      window.mermaid.initialize({
        startOnLoad: false,
        securityLevel: "strict",
        theme,
        fontFamily
      });
      mermaidConfig = { theme, fontFamily };
    }
  };

  // PDF export always uses the day palette, so print renders use the light theme.
  // Diagrams use the same typeface as the UI language (styles.css :root and
  // :root:lang(en)), so drawings made in one language are not reused in the other.
  const useMermaidTheme = (forPrint) => {
    const theme = !forPrint && darkScheme.matches ? "dark" : "default";
    const fontFamily = getComputedStyle(document.documentElement).fontFamily;
    if (mermaidWanted?.fontFamily !== fontFamily) {
      for (const cache of Object.values(diagramCaches)) cache.clear();
    }
    mermaidWanted = { theme, fontFamily };
    applyMermaidConfig();
    return diagramCaches[theme];
  };

  // GitHub draws ```math blocks as display math.
  const convertMathFences = () => {
    for (const code of preview.querySelectorAll("pre > code.language-math")) {
      const pre = code.parentElement;
      const element = document.createElement("div");
      element.className = "math";
      if (pre.dataset.line) element.dataset.line = pre.dataset.line;
      element.textContent = code.textContent;
      pre.replaceWith(element);
    }
  };

  const renderMath = async (generation) => {
    if (!preview.querySelector(".math")) return;
    if (!window.katex) {
      try {
        await loadKatex();
      } catch {
        return;
      }
      if (generation !== renderGeneration) return;
    }
    for (const element of preview.querySelectorAll(".math")) {
      const source = element.textContent.trim()
        .replace(/^\\[([]/, "")
        .replace(/\\[)\]]$/, "")
        .trim()
        // GitHub also writes inline math as $`x`$; the backticks are not part of it.
        .replace(/^`([\s\S]*)`$/, "$1");
      try {
        window.katex.render(source, element, {
          displayMode: element.tagName === "DIV",
          throwOnError: false
        });
      } catch {
        // Leave the TeX source visible when KaTeX cannot render it.
      }
    }
  };

  // An internet image that is not loaded (consent, setting, or not https) shows
  // what it is instead; its address stays on the element for Save as HTML.
  // Videos (an image or a lone link to .mp4, .webm, .mov and so on, or <video>)
  // play in the preview. An internet video waits for consent like an image.
  const videoPattern = /\.(mp4|m4v|webm|ogv|mov)(?:[?#].*)?$/i;
  const videoName = (address) => {
    try {
      return decodeURIComponent(new URL(address, "https://document.mdeditor.local/").pathname.split("/").pop() ?? address);
    } catch {
      return address;
    }
  };
  const heldVideo = (address, name) => {
    let host = address;
    try { host = new URL(address).host; } catch { /* keep the address */ }
    const holder = document.createElement("span");
    holder.className = "external-image external-video";
    holder.dataset.externalSrc = address;
    holder.title = address;
    holder.textContent = t("externalVideo")(name, host);
    return holder;
  };
  const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
  const showVideos = () => {
    for (const image of preview.querySelectorAll("img")) {
      const address = image.getAttribute("src") || image.dataset.externalSrc || "";
      if (!videoPattern.test(address)) continue;
      const name = image.alt || videoName(address);
      if (image.dataset.externalSrc && !image.getAttribute("src")) {
        image.replaceWith(heldVideo(image.dataset.externalSrc, name));
        continue;
      }
      const video = document.createElement("video");
      video.src = address;
      video.setAttribute("aria-label", t("videoLabel")(name));
      image.replaceWith(video);
    }
    for (const video of preview.querySelectorAll("video")) {
      const held = video.dataset.externalSrc && !video.getAttribute("src")
        ? video.dataset.externalSrc
        : !video.getAttribute("src") ? video.querySelector("source[data-external-src]")?.dataset.externalSrc : null;
      if (held) {
        video.replaceWith(heldVideo(held, videoName(held)));
        continue;
      }
      video.controls = true;
      video.preload = "metadata";
      video.playsInline = true;
      // Videos start by themselves, without sound and over and over, like a
      // moving picture (sound needs a press, as browsers require); the controls
      // pause them or turn the sound on. With "reduce motion" set in Windows they
      // wait for a press, unless autoplay is chosen in Settings (Identity 7-5).
      if (displaySettings.videoAutoplay === "always" || (displaySettings.videoAutoplay === "auto" && !reduceMotion.matches)) {
        video.muted = true;
        video.loop = true;
        video.autoplay = true;
      }
      if (!video.getAttribute("aria-label")) video.setAttribute("aria-label", t("videoLabel")(videoName(video.currentSrc || video.getAttribute("src") || video.querySelector("source")?.getAttribute("src") || "")));
    }
  };

  // Saved pages cannot play: PDF, PNG, and Word show the video's name instead.
  const stillVideos = () => {
    for (const video of preview.querySelectorAll("video")) {
      const source = video.currentSrc || video.getAttribute("src") || video.querySelector("source")?.getAttribute("src") || "";
      const box = document.createElement("span");
      box.className = "video-still";
      box.dataset.src = source;
      box.textContent = t("videoStill")(videoName(source));
      video.replaceWith(box);
    }
  };

  const showHeldBackImages = () => {
    for (const image of preview.querySelectorAll("img[data-external-src]")) {
      const address = image.dataset.externalSrc;
      let host = address;
      try { host = new URL(address).host; } catch { /* keep the address */ }
      const holder = document.createElement("span");
      holder.className = "external-image";
      holder.dataset.externalSrc = address;
      holder.dataset.alt = image.alt;
      holder.title = address;
      holder.textContent = t("externalImage")(image.alt, host);
      image.replaceWith(holder);
    }
  };

  // Heading ids from the document would share the page's id namespace and could
  // shadow the app's own elements (such as "help-dialog"), so keep them as data-anchor.
  const moveAnchorsOffIds = () => {
    for (const element of preview.querySelectorAll("[id]")) {
      element.dataset.anchor = element.id;
      element.removeAttribute("id");
    }
  };

  // Markdig puts task-list checkboxes before the item text without a label, so
  // screen readers would announce only "checkbox"; name each one after its item.
  const labelTaskCheckboxes = () => {
    for (const input of preview.querySelectorAll(".task-list-item > input[type=\"checkbox\"]")) {
      input.setAttribute("aria-label", input.parentElement.textContent.trim());
    }
  };

  // Markdig marks nomnoml blocks as diagrams, but no renderer is bundled for them.
  const showUnsupportedDiagrams = () => {
    for (const element of preview.querySelectorAll(".nomnoml")) {
      const code = document.createElement("code");
      code.textContent = element.textContent;
      const pre = document.createElement("pre");
      pre.appendChild(code);
      element.replaceWith(pre);
    }
  };

  // Mermaid centres each sankey label on its node, so labels of neighbouring
  // columns can land on the same line and overlap; move colliding pairs apart.
  const separateSankeyLabels = (container) => {
    const svg = container.querySelector('svg[aria-roledescription="sankey"]');
    if (!svg) return;
    const labels = Array.from(svg.querySelectorAll("text"));
    for (let pass = 0; pass < 4; pass++) {
      let moved = false;
      for (let i = 0; i < labels.length; i++) {
        for (let j = i + 1; j < labels.length; j++) {
          const a = labels[i].getBBox();
          const b = labels[j].getBBox();
          const overlapX = a.x < b.x + b.width && b.x < a.x + a.width;
          const overlapY = a.y < b.y + b.height && b.y < a.y + a.height;
          if (!overlapX || !overlapY) continue;
          const shift = (Math.min(a.y + a.height, b.y + b.height) - Math.max(a.y, b.y)) / 2 + 2;
          const [upper, lower] = a.y <= b.y ? [labels[i], labels[j]] : [labels[j], labels[i]];
          upper.setAttribute("y", String(Number.parseFloat(upper.getAttribute("y")) - shift));
          lower.setAttribute("y", String(Number.parseFloat(lower.getAttribute("y")) + shift));
          moved = true;
        }
      }
      if (!moved) break;
    }
  };

  const showDiagramError = (element, source, detail) => {
    const message = document.createElement("p");
    message.className = "diagram-error-message";
    // Errors carry a symbol and wording, not color alone (Identity 7-1, 7-2).
    message.textContent = `⚠ ${detail ? `${t("diagramError")}: ${detail}` : t("diagramError")}`;
    const code = document.createElement("code");
    code.textContent = source;
    const pre = document.createElement("pre");
    pre.appendChild(code);
    element.classList.remove("diagram-pending");
    element.classList.add("diagram-error");
    element.replaceChildren(message, pre);
  };

  const renderDiagrams = async (generation, previousSvgs, diagramCache) => {
    // Markdig emits <pre class="mermaid">; a div keeps code-block styles out of the drawing.
    const blocks = Array.from(preview.querySelectorAll(".mermaid"), (block) => {
      const element = document.createElement("div");
      element.className = "mermaid";
      if (block.dataset.line) element.dataset.line = block.dataset.line;
      element.textContent = block.textContent;
      block.replaceWith(element);
      return { element, source: element.textContent };
    });
    const sources = new Set(blocks.map((block) => block.source));
    for (const cache of Object.values(diagramCaches)) {
      for (const source of cache.keys()) {
        if (!sources.has(source)) cache.delete(source);
      }
    }

    const pending = [];
    blocks.forEach((block, index) => {
      const cached = diagramCache.get(block.source);
      if (cached) {
        block.element.innerHTML = cached;
        block.element.classList.add("diagram-rendered");
      } else if (previousSvgs[index]) {
        // Keep the last drawing at this position while the edited diagram renders.
        block.element.innerHTML = previousSvgs[index];
        block.element.classList.add("diagram-rendered");
        pending.push(block);
      } else {
        block.element.classList.add("diagram-pending");
        pending.push(block);
      }
    });

    if (pending.length && !window.mermaid) {
      try {
        await loadMermaid();
      } catch {
        // Shown below as "diagrams unavailable".
      }
      if (generation !== renderGeneration) return;
      applyMermaidConfig();
    }
    for (const { element, source } of pending) {
      if (generation !== renderGeneration) return;
      if (!window.mermaid) {
        showDiagramError(element, source, t("diagramUnavailable"));
        continue;
      }

      const id = `mermaid-diagram-${++diagramCounter}`;
      try {
        const { svg } = await window.mermaid.render(id, source);
        // A newer render may have switched the theme while this one was drawing.
        if (generation !== renderGeneration) return;
        element.innerHTML = svg;
        separateSankeyLabels(element);
        // Cache the adjusted markup so restored diagrams keep the fix.
        diagramCache.set(source, element.innerHTML);
        element.classList.remove("diagram-pending");
        element.classList.add("diagram-rendered");
      } catch (error) {
        document.getElementById(`d${id}`)?.remove();
        if (generation !== renderGeneration) return;
        showDiagramError(element, source, error?.message ?? String(error));
      }
    }
  };

  // Graphviz (```dot, ```graphviz, ```gv) is drawn offline with the bundled Viz.js,
  // loaded the first time a document has one. Links, pictures, and anything that
  // could run are removed from the drawing, so a diagram can only be looked at.
  const graphvizLanguages = ["dot", "graphviz", "gv"];
  const graphvizCache = new Map();
  let vizInstance = null;
  const loadViz = () => vizInstance ??= new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = "lib/graphviz/viz-global.js";
    script.onload = () => (window.Viz ? window.Viz.instance().then(resolve, reject) : reject(new Error("Viz.js")));
    script.onerror = () => { vizInstance = null; reject(new Error("Viz.js")); };
    document.head.appendChild(script);
  });
  const prepareGraphviz = () => Array.from(preview.querySelectorAll("pre > code"), (code) => {
    const language = Array.from(code.classList).find((name) => name.startsWith("language-"))?.slice(9).toLowerCase();
    if (!graphvizLanguages.includes(language)) return null;
    const pre = code.parentElement;
    const element = document.createElement("div");
    element.className = "graphviz";
    const line = pre.dataset.line ?? code.dataset.line;
    if (line) element.dataset.line = line;
    element.textContent = code.textContent;
    pre.replaceWith(element);
    return element;
  }).filter(Boolean);
  const safeSvg = (markup) => {
    const svg = new DOMParser().parseFromString(markup, "image/svg+xml").documentElement;
    if (svg.nodeName !== "svg") return null;
    for (const element of svg.querySelectorAll("script, foreignObject, image, use, style")) element.remove();
    for (const link of svg.querySelectorAll("a")) link.replaceWith(...link.childNodes);
    for (const element of [svg, ...svg.querySelectorAll("*")]) {
      for (const attribute of Array.from(element.attributes)) {
        if (/^on/i.test(attribute.name) || /href$/i.test(attribute.name)) element.removeAttribute(attribute.name);
      }
    }
    svg.setAttribute("role", "img");
    return svg;
  };
  const loadScript = (src) => new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = src;
    script.onload = resolve;
    script.onerror = () => reject(new Error(src));
    document.head.appendChild(script);
  });

  // A fenced block in one of these languages becomes an empty <div> of this class
  // holding its source, ready to be drawn.
  const takeFences = (languages, className) => Array.from(preview.querySelectorAll("pre > code"), (code) => {
    const language = Array.from(code.classList).find((name) => name.startsWith("language-"))?.slice(9).toLowerCase();
    if (!languages.includes(language)) return null;
    const pre = code.parentElement;
    const element = document.createElement("div");
    element.className = className;
    element.dataset.language = language;
    const line = pre.dataset.line ?? code.dataset.line;
    if (line) element.dataset.line = line;
    element.textContent = code.textContent;
    pre.replaceWith(element);
    return element;
  }).filter(Boolean);

  // 3D models (```stl, ASCII STL as on GitHub) are drawn offline with the bundled
  // three.js: drag to turn, wheel to zoom. A model keeps its view while you type
  // elsewhere, and its picture goes into PDF, PNG, HTML, and Word.
  const modelViews = new Map();
  let threeLoaded = null;
  const loadThree = () => threeLoaded ??= loadScript("lib/three/stl-viewer.min.js").catch((error) => {
    threeLoaded = null;
    throw error;
  });
  const createModelView = (source) => {
    const T = window.MdThree;
    const geometry = new T.STLLoader().parse(source);
    if (!geometry.attributes.position?.count) throw new Error(t("modelEmpty"));
    geometry.computeVertexNormals();
    geometry.center();
    const box = new T.Box3().setFromBufferAttribute(geometry.attributes.position);
    const size = Math.max(box.getSize(new T.Vector3()).length(), 1e-6);
    const scene = new T.Scene();
    // A dark stage, so the cream body stands out (11.0:1; Identity 4-4 (4)).
    scene.background = new T.Color("#2B3A20");
    // Colors from the 5garashi palette: the first solid is cream, the others
    // (letters on a body, say) volt. A solid whose name has a color in it
    // ("solid letters #0969DA") takes that color.
    const names = geometry.userData.groupNames ?? [];
    const materials = (geometry.groups.length > 1 ? geometry.groups : [null]).map((_, index) => new T.MeshStandardMaterial({
      color: names[index]?.match(/#[0-9a-f]{6}/i)?.[0] ?? (index === 0 ? "#FFF3D6" : "#B92613"),
      metalness: 0.05,
      roughness: 0.6
    }));
    const mesh = new T.Mesh(geometry, materials.length > 1 ? materials : materials[0]);
    // STL is Z-up; the view is Y-up.
    mesh.rotation.x = -Math.PI / 2;
    scene.add(mesh);
    scene.add(new T.AmbientLight(0xffffff, 0.9));
    const camera = new T.PerspectiveCamera(40, 640 / 384, size / 100, size * 100);
    camera.position.set(size * 0.95, size * 0.75, size * 1.45);
    const light = new T.DirectionalLight(0xffffff, 1.6);
    light.position.set(1, 2, 3);
    camera.add(light);
    scene.add(camera);
    const renderer = new T.WebGLRenderer({ antialias: true, preserveDrawingBuffer: true });
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setSize(640, 384, false);
    const canvas = renderer.domElement;
    canvas.setAttribute("role", "img");
    canvas.setAttribute("aria-label", t("modelLabel"));
    canvas.tabIndex = 0;
    const controls = new T.OrbitControls(camera, canvas);
    controls.listenToKeyEvents(canvas);
    const draw = () => renderer.render(scene, camera);
    controls.addEventListener("change", draw);
    draw();
    const element = document.createElement("div");
    element.className = "stl diagram-rendered";
    element.appendChild(canvas);
    return {
      element,
      dispose: () => {
        controls.dispose();
        geometry.dispose();
        materials.forEach((material) => material.dispose());
        renderer.dispose();
        renderer.forceContextLoss();
      }
    };
  };
  const renderModels = async (elements, generation) => {
    const used = new Set();
    // The same model written twice gets a view for each (a view can be in one place only).
    const seen = new Map();
    for (const element of elements) {
      const text = element.textContent;
      const nth = (seen.get(text) ?? 0) + 1;
      seen.set(text, nth);
      const source = nth > 1 ? `${text}\n#${nth}` : text;
      let view = modelViews.get(source);
      if (!view) {
        element.classList.add("diagram-pending");
        try {
          await loadThree();
          if (generation !== renderGeneration) return;
          view = createModelView(text);
          modelViews.set(source, view);
        } catch (error) {
          showDiagramError(element, text, error?.message ?? String(error));
          continue;
        }
      }
      used.add(source);
      if (view.element.dataset.line !== element.dataset.line) {
        if (element.dataset.line) view.element.dataset.line = element.dataset.line;
      }
      element.replaceWith(view.element);
    }
    // Models no longer in the document give their drawing context back.
    for (const [source, view] of modelViews) {
      if (!used.has(source)) {
        view.dispose();
        modelViews.delete(source);
      }
    }
  };

  // Maps (```geojson, ```topojson) are drawn with the bundled d3-geo on a simple
  // world outline (Natural Earth), offline. With "OpenStreetMap" chosen in
  // Settings, map tiles are loaded behind the shapes instead (Web Mercator, so
  // the two line up), with the required credit.
  const mapCache = new Map();
  let geoLoaded = null;
  let worldOutline = null;
  const loadGeo = () => geoLoaded ??= Promise.all([
    loadScript("lib/geo/geo.min.js"),
    fetch("lib/geo/countries-110m.json").then((response) => response.json())
  ]).then(([, world]) => {
    worldOutline = window.MdGeo.feature(world, world.objects.countries);
  }).catch((error) => {
    geoLoaded = null;
    throw error;
  });
  const mapFeatures = (language, source) => {
    const data = JSON.parse(source);
    const G = window.MdGeo;
    const asFeatures = (value) => {
      if (!value) return [];
      if (value.type === "FeatureCollection") return value.features ?? [];
      if (value.type === "Feature") return [value];
      if (value.type === "GeometryCollection" && !value.geometries) return [];
      return [{ type: "Feature", properties: {}, geometry: value }];
    };
    if (language === "topojson" || data.type === "Topology") {
      return Object.values(data.objects ?? {}).flatMap((object) => asFeatures(G.feature(data, object)));
    }
    return asFeatures(data);
  };
  const svgNamespace = "http://www.w3.org/2000/svg";
  const svgElement = (name, attributes = {}) => {
    const element = document.createElementNS(svgNamespace, name);
    for (const [key, value] of Object.entries(attributes)) element.setAttribute(key, String(value));
    return element;
  };
  const drawMap = (language, source, tiles) => {
    const G = window.MdGeo;
    // GeoJSON winds outer rings the other way round from d3-geo, which would read
    // a small area as the whole world outside it; such areas are turned around.
    const rewind = (feature) => {
      const polygons = feature.geometry.type === "Polygon" ? [feature.geometry.coordinates]
        : feature.geometry.type === "MultiPolygon" ? feature.geometry.coordinates : [];
      for (const rings of polygons) {
        if (G.geoArea({ type: "Polygon", coordinates: rings }) > 2 * Math.PI) rings.forEach((ring) => ring.reverse());
      }
      return feature;
    };
    const features = mapFeatures(language, source).filter((feature) => feature?.geometry).map(rewind);
    if (features.length === 0) throw new Error(t("mapEmpty"));
    const collection = { type: "FeatureCollection", features };
    const width = 640;
    const height = 400;
    const [[west, south], [east, north]] = G.geoBounds(collection);
    if (![west, south, east, north].every(Number.isFinite)) throw new Error(t("mapEmpty"));
    // Web Mercator at a tile zoom level, chosen so the shapes fit with a margin.
    const worldX = (longitude) => (longitude + 180) / 360 * 256;
    const worldY = (latitude) => {
      const sine = Math.sin(Math.max(-85, Math.min(85, latitude)) * Math.PI / 180);
      return (0.5 - Math.log((1 + sine) / (1 - sine)) / (4 * Math.PI)) * 256;
    };
    const spanX = Math.abs(worldX(east) - worldX(west));
    const spanY = Math.abs(worldY(south) - worldY(north));
    const fit = Math.log2(Math.min((width - 80) / Math.max(spanX, 1e-9), (height - 80) / Math.max(spanY, 1e-9)));
    // The offline outline is coarse, so it never zooms past country scale.
    const zoom = Math.max(0, Math.min(tiles ? 16 : 6, Math.floor(fit)));
    const worldSize = 256 * 2 ** zoom;
    const centerX = (worldX(west) + worldX(east)) / 2 * 2 ** zoom;
    const centerY = (worldY(north) + worldY(south)) / 2 * 2 ** zoom;
    const projection = G.geoMercator()
      .scale(worldSize / (2 * Math.PI))
      .translate([width / 2 - (centerX - worldSize / 2), height / 2 - (centerY - worldSize / 2)])
      .clipExtent([[-20, -20], [width + 20, height + 20]]);
    const path = G.geoPath(projection).pointRadius(6);

    const svg = svgElement("svg", { viewBox: `0 0 ${width} ${height}`, width, height, role: "img", "aria-label": t("mapLabel") });
    if (tiles) {
      const group = svgElement("g", { class: "map-tiles" });
      const left = centerX - width / 2;
      const top = centerY - height / 2;
      const count = 2 ** zoom;
      for (let tileX = Math.floor(left / 256); tileX * 256 < left + width; tileX++) {
        for (let tileY = Math.max(0, Math.floor(top / 256)); tileY * 256 < top + height && tileY < count; tileY++) {
          const wrapped = ((tileX % count) + count) % count;
          group.appendChild(svgElement("image", {
            href: `https://tile.openstreetmap.org/${zoom}/${wrapped}/${tileY}.png`,
            x: tileX * 256 - left, y: tileY * 256 - top, width: 256, height: 256
          }));
        }
      }
      svg.appendChild(group);
    } else {
      svg.appendChild(svgElement("rect", { width, height, fill: "#DCEBF5" }));
      svg.appendChild(svgElement("path", { d: path(worldOutline) ?? "", fill: "#F3EFE2", stroke: "#B9B2A0", "stroke-width": 0.6 }));
    }
    // The shapes, with GitHub's simplestyle properties (stroke, fill, marker-color).
    for (const feature of features) {
      const style = feature.properties ?? {};
      const point = /Point$/.test(feature.geometry.type);
      const line = /LineString$/.test(feature.geometry.type);
      const color = style["marker-color"] ?? style.stroke ?? "#B92613";
      const shape = svgElement("path", {
        d: path(feature) ?? "",
        fill: point ? color : line ? "none" : style.fill ?? "#B92613",
        "fill-opacity": point ? 1 : style["fill-opacity"] ?? 0.25,
        stroke: point ? "#FFFFFF" : style.stroke ?? "#B92613",
        "stroke-width": point ? 2 : style["stroke-width"] ?? 2,
        "stroke-opacity": style["stroke-opacity"] ?? 1
      });
      const name = style.name ?? style.title;
      if (name) {
        const title = svgElement("title");
        title.textContent = String(name);
        shape.appendChild(title);
      }
      svg.appendChild(shape);
    }
    if (tiles) {
      // OpenStreetMap's tiles must carry this credit.
      const credit = svgElement("g", { class: "map-credit" });
      credit.appendChild(svgElement("rect", { x: width - 196, y: height - 18, width: 196, height: 18, fill: "#FFFFFF", "fill-opacity": 0.85 }));
      const text = svgElement("text", { x: width - 6, y: height - 5, "text-anchor": "end", "font-size": 11, fill: "#1A1A1A" });
      text.textContent = "© OpenStreetMap contributors";
      credit.appendChild(text);
      svg.appendChild(credit);
    }
    return svg;
  };
  // Map tiles finish loading before a PDF, PNG, or Word export takes its picture.
  const tilesLoaded = (svg) => Promise.race([
    Promise.all(Array.from(svg.querySelectorAll(".map-tiles image"), (image) =>
      new Promise((resolve) => { image.addEventListener("load", resolve, { once: true }); image.addEventListener("error", resolve, { once: true }); }))),
    new Promise((resolve) => setTimeout(resolve, 6000))
  ]);
  const renderMaps = async (elements, generation) => {
    const tiles = displaySettings.mapBackground === "always" || (displaySettings.mapBackground === "ask" && mapsAllowed);
    const waits = [];
    let drawn = 0;
    for (const element of elements) {
      const source = element.textContent;
      const language = element.dataset.language;
      const key = `${tiles}\n${language}\n${source}`;
      try {
        let markup = mapCache.get(key);
        if (!markup) {
          element.classList.add("diagram-pending");
          await loadGeo();
          if (generation !== renderGeneration) return;
          markup = drawMap(language, source, tiles).outerHTML;
          if (mapCache.size > 50) mapCache.clear();
          mapCache.set(key, markup);
        }
        element.innerHTML = markup;
        element.classList.remove("diagram-pending");
        element.classList.add("diagram-rendered");
        drawn++;
        if (tiles) waits.push(tilesLoaded(element.firstElementChild));
      } catch (error) {
        showDiagramError(element, source, error?.message ?? String(error));
      }
    }
    await Promise.all(waits);
    // Only the window that shows the notice asks (not the preview window).
    const waiting = displaySettings.mapBackground === "ask" && !mapsAllowed && !previewWindow ? drawn : 0;
    if (waiting !== mapsWaiting) {
      mapsWaiting = waiting;
      updateNotice();
    }
  };

  // Graphviz sizes boxes with Latin font measurements, so Japanese labels come out
  // wider than their box; a label that does not fit is made smaller to fit.
  const fitGraphvizText = (svg) => {
    for (const group of svg.querySelectorAll("g.node, g.cluster")) {
      const shape = group.querySelector("polygon, ellipse, path, polyline");
      if (!shape) continue;
      const room = shape.getBBox().width - 8;
      for (const text of group.querySelectorAll("text")) {
        const width = text.getBBox().width;
        if (room > 0 && width > room) {
          const size = parseFloat(text.getAttribute("font-size") || "14");
          text.setAttribute("font-size", (size * room / width).toFixed(2));
        }
      }
    }
  };

  const renderGraphviz = async (elements, generation) => {
    for (const element of elements) {
      const source = element.textContent;
      let result = graphvizCache.get(source);
      if (!result) {
        element.classList.add("diagram-pending");
        let viz;
        try {
          viz = await loadViz();
        } catch {
          showDiagramError(element, source, t("diagramUnavailable"));
          continue;
        }
        if (generation !== renderGeneration) return;
        const output = viz.render(source, { format: "svg" });
        result = output.status === "success"
          ? { svg: output.output }
          : { error: output.errors.map((item) => item.message).join(" ") };
        if (graphvizCache.size > 100) graphvizCache.clear();
        graphvizCache.set(source, result);
      }
      const svg = result.svg ? safeSvg(result.svg) : null;
      if (!svg) {
        showDiagramError(element, source, result.error ?? "");
        continue;
      }
      element.replaceChildren(document.importNode(svg, true));
      fitGraphvizText(element.firstElementChild);
      element.classList.remove("diagram-pending");
      element.classList.add("diagram-rendered");
    }
  };

  // Code blocks with a language (```python and so on) are colored with the bundled
  // highlight.js; blocks without a known language stay plain. The colors come from
  // CSS variables, so PDF, PNG, HTML, and Word get the day palette. Results are
  // kept by language and text, since the preview is redrawn as you type.
  const highlighted = new Map();
  const highlightCode = () => {
    if (!window.hljs) return;
    for (const code of preview.querySelectorAll("pre > code")) {
      const language = Array.from(code.classList).find((name) => name.startsWith("language-"))?.slice(9);
      if (!language || !window.hljs.getLanguage(language)) continue;
      const text = code.textContent;
      const key = `${language}\n${text}`;
      let html = highlighted.get(key);
      if (html === undefined) {
        html = window.hljs.highlight(text, { language, ignoreIllegals: true }).value;
        if (highlighted.size > 300) highlighted.clear();
        highlighted.set(key, html);
      }
      code.innerHTML = html;
      code.classList.add("hljs");
    }
  };

  // Save as Word: the colors of every code block (in document order), as runs per
  // line, read from the preview while it shows the day palette.
  window.mdeditorCodeColors = () => Array.from(preview.querySelectorAll("pre > code"))
    .filter((code) => !code.closest(".mermaid, .graphviz, .stl, .geo-map")).map((code) => {
    const lines = [[]];
    let baseWeight = 400;
    const walk = (node, style) => {
      for (const child of node.childNodes) {
        if (child.nodeType === Node.TEXT_NODE) {
          const parts = child.textContent.split("\n");
          parts.forEach((part, index) => {
            if (index > 0) lines.push([]);
            if (part) lines[lines.length - 1].push({ text: part, ...style });
          });
        } else if (child.nodeType === Node.ELEMENT_NODE) {
          const computed = getComputedStyle(child);
          walk(child, { color: computed.color, bold: Number(computed.fontWeight) > baseWeight, italic: computed.fontStyle === "italic" });
        }
      }
    };
    const base = getComputedStyle(code);
    baseWeight = Number(base.fontWeight);
    walk(code, { color: base.color, bold: false, italic: false });
    if (lines.length > 1 && lines[lines.length - 1].length === 0) lines.pop();
    return lines;
  });

  const renderPreview = async (html, forPrint = false, lines = 0) => {
    const generation = ++renderGeneration;
    if (!forPrint && lines > 0) previewLines = lines;
    const diagramCache = useMermaidTheme(forPrint);
    if (reader && !forPrint && html !== lastPreviewHtml) stopReading();
    if (!forPrint) lastPreviewHtml = html;
    const previousSvgs = Array.from(
      preview.querySelectorAll(".mermaid"),
      (element) => (element.classList.contains("diagram-rendered") ? element.innerHTML : null)
    );
    preview.innerHTML = html;
    showVideos();
    showHeldBackImages();
    moveAnchorsOffIds();
    labelTaskCheckboxes();
    showUnsupportedDiagrams();
    const graphs = prepareGraphviz();
    const models = takeFences(["stl"], "stl");
    const maps = takeFences(["geojson", "topojson"], "geo-map");
    convertMathFences();
    await renderMath(generation);
    if (generation !== renderGeneration) return;
    highlightCode();
    const keepPlace = () => {
      if (previewWindow) scrollPreviewToLine(lastSyncedLine.line, lastSyncedLine.edge);
      else if (view.hasFocus) syncFrom(editorScroller);
    };
    keepPlace();
    await renderDiagrams(generation, previousSvgs, diagramCache);
    await renderGraphviz(graphs, generation);
    await renderModels(models, generation);
    await renderMaps(maps, generation);
    if (forPrint) {
      openAllDetails();
      stillVideos();
    }
    if (generation === renderGeneration) keepPlace();
  };

  // Save as HTML: the host renders the preview in the day palette (as for a PDF)
  // and asks for its markup. Math becomes MathML, which browsers show without
  // the KaTeX fonts; heading ids come back; app-only attributes are dropped.
  window.mdeditorExportHtml = () => {
    const copy = preview.cloneNode(true);
    const canvases = Array.from(preview.querySelectorAll(".stl canvas"));
    copy.querySelectorAll(".stl canvas").forEach((canvas, index) => {
      const image = document.createElement("img");
      image.src = canvases[index]?.toDataURL("image/png") ?? "";
      image.alt = canvas.getAttribute("aria-label") ?? "";
      canvas.replaceWith(image);
    });
    copy.querySelectorAll(".map-tiles, .map-credit").forEach((element) => element.remove());
    for (const video of copy.querySelectorAll("video, .external-video, .video-still")) {
      const source = video.dataset.externalSrc || video.dataset.src || video.getAttribute("src") || video.querySelector("source")?.getAttribute("src") || "";
      const link = document.createElement("a");
      // Local videos are linked by their path beside the document.
      link.href = source.startsWith("https://document.mdeditor.local/") ? decodeURIComponent(new URL(source).pathname.slice(1)) : source;
      link.textContent = t("videoStill")(videoName(source));
      video.replaceWith(link);
    }
    for (const holder of copy.querySelectorAll(".external-image[data-external-src]:not(.external-video)")) {
      const image = document.createElement("img");
      image.src = holder.dataset.externalSrc;
      image.alt = holder.dataset.alt ?? "";
      holder.replaceWith(image);
    }
    for (const selector of [".katex-display", ".katex"]) {
      for (const element of copy.querySelectorAll(selector)) {
        const math = element.querySelector(".katex-mathml math");
        if (math) element.replaceWith(math);
      }
    }
    for (const element of copy.querySelectorAll("*")) {
      if (element.dataset.anchor) element.id = element.dataset.anchor;
      element.removeAttribute("data-line");
      element.removeAttribute("data-source");
      element.removeAttribute("data-anchor");
      if (element.getAttribute("tabindex") === "-1") element.removeAttribute("tabindex");
      element.classList.remove("reading", "diagram-rendered", "diagram-pending");
      if (element.classList.length === 0) element.removeAttribute("class");
    }
    return {
      title: copy.querySelector("h1")?.textContent.trim() ?? "",
      html: copy.innerHTML
    };
  };

  darkScheme.addEventListener("change", () => {
    if (lastPreviewHtml !== null) renderPreview(lastPreviewHtml);
  });

  const hasFiles = (event) =>
    Array.from(event.dataTransfer?.types ?? []).includes("Files");
  const hasLink = (event) =>
    Array.from(event.dataTransfer?.types ?? []).includes("text/uri-list");
  // While files are dragged, a label by the pointer says what dropping there does:
  // on the editor they go in at the cursor, anywhere else a file opens. Going in at the
  // cursor is set apart in red (volt), the label and the editor's frame both.
  const dropHint = document.createElement("div");
  dropHint.className = "drop-hint";
  dropHint.hidden = true;
  document.body.append(dropHint);
  const hideDropHint = () => {
    dropHint.hidden = true;
    document.body.classList.remove("dragging", "drop-insert");
  };
  window.addEventListener("dragover", (event) => {
    if (!hasFiles(event) && !hasLink(event)) return;
    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
    document.body.classList.add("dragging");
    if (!hasFiles(event)) return;
    const insert = editorHost.contains(event.target);
    dropHint.textContent = t(insert ? "dropInsert" : "dropOpen");
    document.body.classList.toggle("drop-insert", insert);
    dropHint.hidden = false;
    // Beside the picture Windows draws around the pointer (about 90px wide): to its
    // right, or to its left when the window has no room there.
    const right = event.clientX + 56;
    const x = right + dropHint.offsetWidth <= window.innerWidth - 8 ? right : event.clientX - 56 - dropHint.offsetWidth;
    const y = Math.min(event.clientY - dropHint.offsetHeight / 2, window.innerHeight - dropHint.offsetHeight - 8);
    dropHint.style.left = `${Math.max(8, x)}px`;
    dropHint.style.top = `${Math.max(8, y)}px`;
  }, true);
  window.addEventListener("dragleave", (event) => {
    if (event.relatedTarget === null) hideDropHint();
  }, true);
  window.addEventListener("dragend", hideDropHint, true);
  window.addEventListener("drop", (event) => {
    const link = event.dataTransfer?.getData("text/uri-list")?.split("\n").find((line) => line && !line.startsWith("#"));
    if (!hasFiles(event) && link?.trim().startsWith("https://")) {
      event.preventDefault();
      event.stopPropagation();
      hideDropHint();
      post({ type: "openUrl", url: link.trim() });
      return;
    }
    if (!hasFiles(event)) {
      hideDropHint();
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    hideDropHint();
    const files = Array.from(event.dataTransfer.files);
    // On the editor, the files go in at the drop point (the cursor moves there
    // first); anywhere else the app opens the file.
    const onEditor = editorHost.contains(event.target);
    if (onEditor) {
      const position = view.posAtCoords({ x: event.clientX, y: event.clientY });
      if (position !== null) view.dispatch({ selection: { anchor: position } });
    }
    window.chrome.webview.postMessageWithAdditionalObjects(
      { type: "dropFile", count: files.length, onEditor },
      files
    );
  }, true);

  // A pasted picture (such as a screen capture) is saved beside the document
  // by the app, which then inserts it at the cursor.
  editorHost.addEventListener("paste", (event) => {
    const image = Array.from(event.clipboardData?.files ?? []).find((file) => file.type.startsWith("image/"));
    if (!image) return;
    event.preventDefault();
    event.stopPropagation();
    if (image.size > 20 * 1024 * 1024) return;
    const reader = new FileReader();
    reader.onload = () => post({ type: "pasteImage", mime: image.type, data: String(reader.result).replace(/^data:[^,]*,/, "") });
    reader.readAsDataURL(image);
  }, true);

  document.getElementById("toc-button").addEventListener("click", () => post({ type: "toc" }));
  // The table of contents sits between <!-- toc --> and <!-- /toc --> (not shown in
  // the preview), so pressing Contents again replaces it with the current headings.
  // Only marks on lines of their own count, and not inside a code block, so a
  // sentence that mentions <!-- toc --> is never taken for a table of contents.
  const tocPattern = /^<!-- toc -->[ \t]*$[\s\S]*?^<!-- \/toc -->[ \t]*$/gm;
  const findToc = (doc) => {
    for (const match of doc.matchAll(tocPattern)) {
      const fences = doc.slice(0, match.index).match(/^[ \t]{0,3}(```|~~~)/gm)?.length ?? 0;
      if (fences % 2 === 0) return match;
    }
    return null;
  };
  const placeToc = (text) => {
    const doc = view.state.doc.toString();
    const found = findToc(doc);
    if (found) {
      view.dispatch({
        changes: { from: found.index, to: found.index + found[0].length, insert: text },
        selection: { anchor: found.index },
        scrollIntoView: true
      });
      showNoticeMessage(t("tocUpdated"));
    } else {
      let { from, to } = view.state.selection.main;
      // With the cursor still at the very start, the table goes after the title.
      const title = from === 0 && to === 0 ? /^# .*$/m.exec(doc) : null;
      if (title) from = to = title.index + title[0].length;
      // On its own lines, with exactly one blank line before and after.
      const head = view.state.sliceDoc(0, from);
      const tail = view.state.sliceDoc(to);
      const before = head === "" || head.endsWith("\n\n") ? "" : head.endsWith("\n") ? "\n" : "\n\n";
      const after = tail.startsWith("\n\n") ? "" : tail.startsWith("\n") ? "\n" : "\n\n";
      const insert = `${before}${text}${after}`;
      view.dispatch({ changes: { from, to, insert }, selection: { anchor: from + insert.length }, scrollIntoView: true });
      showNoticeMessage(t("tocInserted"));
    }
    view.focus();
  };

  window.chrome.webview.addEventListener("message", (event) => {
    const message = event.data;
    if (message.type === "tocText") {
      if (message.text) placeToc(message.text);
      else showNoticeMessage(t("tocNoHeadings"));
    } else if (message.type === "applyEdits") {
      // One transaction, so Ctrl+Z undoes them together; the cursor keeps its place.
      applyingHostEdits = true;
      try {
        view.dispatch({ changes: message.edits.map(({ from, to, insert }) => ({ from, to, insert })) });
      } finally {
        applyingHostEdits = false;
      }
    } else if (message.type === "discarded") {
      discardedName = message.name ?? null;
      updateRestoreButton();
    } else if (message.type === "insertText") {
      const { from, to } = view.state.selection.main;
      // The editor keeps "\n" line breaks; "\r\n" would make the length below wrong.
      let insert = message.text.replace(/\r\n?/g, "\n");
      if (message.block) {
        // A file's content goes in as blocks of its own, with blank lines around it.
        const head = view.state.sliceDoc(0, from);
        const tail = view.state.sliceDoc(to);
        const before = head === "" || head.endsWith("\n\n") ? "" : head.endsWith("\n") ? "\n" : "\n\n";
        const after = tail.startsWith("\n") ? "" : "\n";
        insert = `${before}${insert}${after}`;
      }
      view.dispatch({ changes: { from, to, insert }, selection: { anchor: from + insert.length }, scrollIntoView: true });
      view.focus();
    } else if (message.type === "documentInfo") {
      documentName.textContent = message.name;
      documentName.title = message.name;
      documentDirty = message.isDirty;
      documentEncoding = message.encoding ?? "utf-8";
      encodingSource = message.encodingSource ?? "detected";
      canReopen = Boolean(message.canReopen);
      updateEncodingButton();
      pdfStarting = false;
      saveState.textContent = statusText();
    } else if (message.type === "language") {
      language = normalizeLanguage(message.lang) ?? language;
      applyLanguage();
    } else if (message.type === "previewDetached") {
      setPreviewDetached(Boolean(message.detached));
    } else if (message.type === "syncLine") {
      lastSyncedLine = { line: message.line, edge: message.edge };
      if (previewWindow) scrollPreviewToLine(message.line, message.edge);
      else scrollEditorToLine(message.line, message.edge);
    } else if (message.type === "runCommand") {
      shortcutDefinitions.find((item) => item.id === message.id)?.run(message.at ?? undefined);
    } else if (message.type === "pasteHotkey") {
      pasteKey = { key: message.key ?? "", exists: Boolean(message.exists) };
      showPasteKey();
    } else if (message.type === "externalImages") {
      externalSelect.value = message.value;
    } else if (message.type === "externalNotice") {
      if (message.count > 0) {
        noticeState = { kind: "external", count: message.count, prefix: message.prefix };
      } else if (noticeState?.kind === "external") {
        noticeState = noticeState.prefix ? { kind: "message", text: noticeState.prefix } : null;
      }
      updateNotice();
    } else if (message.type === "notice") {
      showNoticeMessage(message.text, message.reopen);
    } else if (message.type === "appInfo") {
      document.getElementById("app-version").textContent = message.version;
    } else if (message.type === "scrollToAnchor") {
      // Sent after another document opens from a link with #heading.
      requestAnimationFrame(() => scrollToAnchor(message.name));
    } else if (message.type === "zoom") {
      zoomValue.textContent = `${Math.round(message.factor * 100)}%`;
    } else if (message.type === "autoBackup") {
      backupCheckbox.checked = Boolean(message.enabled);
    } else if (message.type === "missingLinks") {
      const resolve = linkChecks.get(message.id);
      if (linkChecks.delete(message.id)) resolve(message.targets ?? []);
    } else if (message.type === "pdfOptions") {
      pdfFigureTextSelect.value = message.figureText;
      pdfAlignCheckbox.checked = Boolean(message.align);
      pdfPagePicturesCheckbox.checked = Boolean(message.pagePictures);
      document.getElementById("setting-pdf-file-path").textContent = message.file;
    } else if (message.type === "nearGitHub") {
      nearGitHubCheckbox.checked = Boolean(message.enabled);
    } else if (message.type === "encodingNotice") {
      // No encoding to announce clears the line, but never the held-back image
      // notice: that one changes only with the images (externalNotice).
      if (message.kind) noticeState = { kind: message.kind, encoding: message.encoding };
      else if (noticeState?.kind !== "external") noticeState = null;
      updateNotice();
    } else if (message.type === "paper") {
      paper = paperOrder.includes(message.paper) ? message.paper : "auto";
      autoPaper = message.autoPaper === "letter" ? "letter" : "a4";
      updatePaperButton();
    } else if (message.type === "theme") {
      theme = themeOrder.includes(message.theme) ? message.theme : "auto";
      updateThemeButton();
    } else if (message.type === "setText") {
      mapNoticeClosed = false;
      view.setState(EditorState.create({ doc: message.text, extensions: editorExtensions() }));
      // A newly loaded document starts at the top, not where the last one was.
      editorScroller.scrollTop = 0;
      preview.scrollTop = 0;
      if (previewDetached) post({ type: "syncLine", line: 0, edge: "top" });
    } else if (message.type === "preview") {
      mapsAllowed = Boolean(message.mapsAllowed);
      renderPreview(message.html, message.forPrint, message.lines).finally(() => {
        if (message.notifyRendered) post({ type: "previewRendered", requestId: message.requestId });
      });
    } else if (message.type === "pdfExportStatus") {
      saveState.textContent = message.message || statusText();
    }
  });

  // Both windows use the same storage, so a setting changed in one shows in the other.
  window.addEventListener("storage", (event) => {
    if (event.key === displayStorageKey) {
      try {
        displaySettings = { ...displayDefaults, ...JSON.parse(event.newValue || "{}") };
      } catch {
        return;
      }
      applyDisplaySettings();
    } else if (event.key === "mdeditor-language" && normalizeLanguage(event.newValue)) {
      language = normalizeLanguage(event.newValue);
      applyLanguage();
    } else if (event.key === shortcutStorageKey) {
      shortcutSettings = loadShortcutSettings();
      applyShortcutHints();
    }
  });

  if (previewWindow) displayMode = "preview";
  applyDisplaySettings(false);
  applyLanguage();
  post({ type: "ready", lang: language });
})();
