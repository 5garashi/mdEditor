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
  const preview = document.getElementById("preview");
  const documentName = document.getElementById("document-name");
  const saveState = document.getElementById("save-state");
  const languageButton = document.getElementById("language-button");
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

  const messages = {
    ja: {
      toolbar: "文書操作",
      sample: "サンプル", sampleHeading: "サンプルを開く",
      sampleIntro: "図と数式を含む短い文書を、編集欄に入れて試せます。選んだときだけ入ります。",
      sampleOpen: "開く", sampleOpenLabel: (name) => `サンプル「${name}」を編集欄に入れる`,
      sampleConfirm: "編集欄の内容をサンプルに置き換えます。よろしいですか？",
      about: "このアプリについて", aboutHeading: "このアプリについて",
      aboutIntro: "mdEditor は 5garashi.com Design Office が作った、Markdown の編集・プレビュー・PDF 書き出しのアプリです。",
      aboutCompany: "5garashi.com Design Office の紹介", aboutWorks: "作品・ほかのアプリ",
      aboutSupport: "サポート・お問い合わせ", aboutPrivacy: "プライバシーポリシー",
      aboutNote: "リンクは選んだときだけブラウザーで開きます。起動時の広告や、PDF への宣伝の挿入はありません。",
      open: "開く", openTitle: "開く (Ctrl+O)", save: "保存", saveTitle: "保存 (Ctrl+S)",
      saveAs: "名前を付けて保存", pdf: "PDFに保存",
      languageToggle: "English", languageLabel: "表示言語を English に切り替える",
      editorHeading: "Markdown を編集", editorLabel: "Markdown テキスト",
      previewHeading: "プレビュー", previewLabel: "Markdown の表示結果",
      dividerLabel: "編集画面とプレビューの幅",
      dividerTitle: "ドラッグまたは矢印キーで左右の幅を調整します",
      fullscreenEditor: "編集画面をウィンドウ全体に表示", fullscreenPreview: "プレビューをウィンドウ全体に表示",
      fullscreenExit: "元の表示に戻す", saved: "保存済み", unsaved: "未保存の変更",
      pdfStarting: "PDF出力を開始しています",
      findReplace: "検索・置換", findReplaceTitle: "検索・置換 (Ctrl+H)",
      help: "書き方", helpTitle: "Markdown の書き方 (F1)", helpHeading: "Markdown の簡単な書き方",
      shortcuts: "キー操作", shortcutHeading: "キーボードショートカット",
      settings: "設定", settingsHeading: "設定",
      settingsIntro: "読みやすさや操作のしやすさを、使う人に合わせて調整できます。設定は次回の起動でも使います。",
      settingsText: "文字（編集欄とプレビュー）", settingsFont: "書体", settingsFontStandard: "標準",
      settingsFontUd: "BIZ UD ゴシック（読みやすさ重視）", settingsSize: "文字の大きさ", settingsSize100: "標準",
      settingsSize125: "大きい（125%）", settingsSize150: "とても大きい（150%）", settingsSize200: "最大（200%）",
      settingsSpacing: "行間", settingsSpacingNormal: "標準", settingsSpacingWide: "広め", settingsSpacingWider: "とても広い",
      settingsZoom: "画面全体の表示倍率", zoomOut: "縮小", zoomIn: "拡大", zoomReset: "100% に戻す",
      zoomNote: "Ctrl + マウスホイール、Ctrl + プラス／マイナス、Ctrl + 0 でも変えられます。倍率は次回の起動でも使います。",
      settingsEditor: "編集欄", settingsActiveLine: "いまの行を強調する", settingsThickCaret: "カーソルを太くする",
      settingsReader: "読み上げ", settingsReaderOn: "読み上げ機能を使う（プレビューの見出しに操作ボタンを表示）",
      settingsReaderRate: "速さ", rateSlow: "ゆっくり", rateSlower: "少しゆっくり", rateNormal: "ふつう",
      rateFaster: "少し速い", rateFast: "速い", settingsReaderPause: "段落の間", pauseNone: "間を空けない",
      pauseShort: "0.5 秒", pauseMedium: "1 秒", pauseLong: "2 秒",
      readerVoices: (ja, en) => `この PC の音声で読み上げます（日本語: ${ja}、英語: ${en}）。文章を外部に送ることはありません。`,
      readerNoVoiceShort: "なし",
      readerNoVoice: "読み上げに使える音声がこの PC にありません。Windows の「設定」→「時刻と言語」→「音声」で音声を追加してください。",
      readerPlay: "読み上げ", readerPause: "一時停止", readerResume: "再開", readerStop: "停止",
      settingsBackup: "自動バックアップ", settingsBackupOn: "作業中の文書を自動でバックアップし、異常終了したときに復元する",
      settingsBackupNote: "保存していない変更を、この PC の中（%LOCALAPPDATA%\\mdEditor\\Recovery）に15秒ごとに控えます。元のファイルは書き換えません。保存するか、正常に終了すると控えは消えます。",
      settingsReset: "表示の設定を初期設定に戻す",
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
        open: "開く", save: "保存", saveAs: "名前を付けて保存", pdf: "PDFに保存", paper: "用紙の切り替え",
        theme: "表示テーマの切り替え", language: "表示言語の切り替え", encoding: "文字コード",
        findReplace: "検索・置換", findNext: "次を検索", findPrev: "前を検索", replaceAll: "すべて置換",
        help: "Markdown の書き方", shortcuts: "キー操作の一覧", fullscreenEditor: "編集画面をウィンドウ全体に表示",
        fullscreenPreview: "プレビューをウィンドウ全体に表示", nextPane: "次の欄へ移動", previousPane: "前の欄へ移動",
        windowFullscreen: "ウィンドウの全画面表示", settings: "設定", readAloud: "読み上げ・一時停止", stopReading: "読み上げを停止"
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
    en: {
      toolbar: "Document actions",
      sample: "Samples", sampleHeading: "Open a sample",
      sampleIntro: "Try a short document with diagrams and math. Nothing is inserted until you choose one.",
      sampleOpen: "Open", sampleOpenLabel: (name) => `Put the sample "${name}" in the editor`,
      sampleConfirm: "This replaces the editor content with the sample. Continue?",
      about: "About", aboutHeading: "About this app",
      aboutIntro: "mdEditor is a Markdown editor, previewer, and PDF exporter made by 5garashi.com Design Office.",
      aboutCompany: "About 5garashi.com Design Office", aboutWorks: "Works and other apps",
      aboutSupport: "Support and contact", aboutPrivacy: "Privacy policy",
      aboutNote: "Links open in your browser only when you choose them. The app shows no ads at startup and never inserts promotions into PDFs.",
      open: "Open", openTitle: "Open (Ctrl+O)", save: "Save", saveTitle: "Save (Ctrl+S)",
      saveAs: "Save As", pdf: "Save as PDF",
      languageToggle: "日本語", languageLabel: "Switch the display language to Japanese",
      editorHeading: "Edit Markdown", editorLabel: "Markdown text",
      previewHeading: "Preview", previewLabel: "Rendered Markdown",
      dividerLabel: "Width of the editor and preview",
      dividerTitle: "Drag or use the arrow keys to resize",
      fullscreenEditor: "Fill window with editor", fullscreenPreview: "Fill window with preview",
      fullscreenExit: "Restore layout", saved: "Saved", unsaved: "Unsaved changes",
      pdfStarting: "Starting PDF export",
      findReplace: "Find/Replace", findReplaceTitle: "Find/Replace (Ctrl+H)",
      help: "Syntax", helpTitle: "Markdown syntax (F1)", helpHeading: "Markdown quick reference",
      shortcuts: "Keys", shortcutHeading: "Keyboard shortcuts",
      settings: "Settings", settingsHeading: "Settings",
      settingsIntro: "Adjust readability and ease of use to suit you. Settings are remembered.",
      settingsText: "Text (editor and preview)", settingsFont: "Typeface", settingsFontStandard: "Standard",
      settingsFontUd: "BIZ UD Gothic (designed for legibility)", settingsSize: "Text size", settingsSize100: "Standard",
      settingsSize125: "Large (125%)", settingsSize150: "Larger (150%)", settingsSize200: "Largest (200%)",
      settingsSpacing: "Line spacing", settingsSpacingNormal: "Standard", settingsSpacingWide: "Wide", settingsSpacingWider: "Wider",
      settingsZoom: "Zoom (whole window)", zoomOut: "Zoom out", zoomIn: "Zoom in", zoomReset: "Reset to 100%",
      zoomNote: "Ctrl + mouse wheel, Ctrl + plus/minus, and Ctrl + 0 also work. The zoom level is remembered.",
      settingsEditor: "Editor", settingsActiveLine: "Highlight the current line", settingsThickCaret: "Use a thick cursor",
      settingsReader: "Read aloud", settingsReaderOn: "Use read-aloud (shows controls in the preview heading)",
      settingsReaderRate: "Speed", rateSlow: "Slow", rateSlower: "A little slow", rateNormal: "Normal",
      rateFaster: "A little fast", rateFast: "Fast", settingsReaderPause: "Pause between paragraphs", pauseNone: "No pause",
      pauseShort: "0.5 seconds", pauseMedium: "1 second", pauseLong: "2 seconds",
      readerVoices: (ja, en) => `Reads with voices on this PC (Japanese: ${ja}, English: ${en}). Text is never sent anywhere.`,
      readerNoVoiceShort: "none",
      readerNoVoice: "No voice for read-aloud is installed on this PC. Add one in Windows Settings > Time & language > Speech.",
      readerPlay: "Read aloud", readerPause: "Pause", readerResume: "Resume", readerStop: "Stop",
      settingsBackup: "Automatic backup", settingsBackupOn: "Back up the open document automatically and restore it after a crash",
      settingsBackupNote: "Unsaved changes are copied every 15 seconds to this PC (%LOCALAPPDATA%\\mdEditor\\Recovery). The original file is never changed. The copy is removed when you save or close normally.",
      settingsReset: "Restore display defaults",
      shortcutIntro: "Every command can be used from the keyboard. Change a key with Change, or remove it with None. Keys can be Ctrl with a letter or digit, or an F key.",
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
        open: "Open", save: "Save", saveAs: "Save As", pdf: "Save as PDF", paper: "Switch paper size",
        theme: "Switch theme", language: "Switch language", encoding: "Text encoding",
        findReplace: "Find/Replace", findNext: "Find next", findPrev: "Find previous", replaceAll: "Replace all",
        help: "Markdown syntax", shortcuts: "Keyboard shortcuts", fullscreenEditor: "Fill window with editor",
        fullscreenPreview: "Fill window with preview", nextPane: "Next pane", previousPane: "Previous pane",
        windowFullscreen: "Window full screen", settings: "Settings", readAloud: "Read aloud / pause", stopReading: "Stop reading"
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
  const storedLanguage = localStorage.getItem("mdeditor-language");
  let language = storedLanguage === "en" || storedLanguage === "ja"
    ? storedLanguage
    : (navigator.language || "").toLowerCase().startsWith("ja") ? "ja" : "en";
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
    document.documentElement.lang = language;
    for (const element of document.querySelectorAll("[data-i18n]")) {
      element.textContent = t(element.dataset.i18n);
    }
    for (const element of document.querySelectorAll("[data-i18n-attr]")) {
      for (const pair of element.dataset.i18nAttr.split(";")) {
        const [attribute, key] = pair.split(":");
        element.setAttribute(attribute, t(key));
      }
    }
    languageButton.lang = language === "ja" ? "en" : "ja";
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
  const { Compartment, EditorSelection, EditorState, EditorView, defaultKeymap, drawSelection, highlightActiveLine, history, historyKeymap, keymap } = window.CodeMirror;
  const displayDefaults = {
    font: "standard", size: "1", spacing: "1.7", activeLine: false, thickCaret: false, reader: false, rate: "1", pause: "0.5"
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
  // Optional editor aids: a highlighted current line, and a drawn (thicker) cursor.
  const editorExtras = new Compartment();
  const editorExtrasFor = (settings) => [
    settings.activeLine ? highlightActiveLine() : [],
    settings.thickCaret ? drawSelection() : []
  ];
  const editorAttributes = new Compartment();
  const editorAttributeValues = () => EditorView.contentAttributes.of({
    "aria-label": t("editorLabel"), spellcheck: "false", autocorrect: "off", autocapitalize: "off"
  });
  const editorExtensions = () => [
    history(),
    keymap.of([...defaultKeymap, ...historyKeymap]),
    EditorView.lineWrapping,
    editorAttributes.of(editorAttributeValues()),
    editorExtras.of(editorExtrasFor(displaySettings)),
    // Every edit goes to the host, which tracks the text and the unsaved state.
    EditorView.updateListener.of((update) => {
      if (update.docChanged) post({ type: "change", text: update.state.doc.toString() });
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
      button.textContent = active
        ? t("fullscreenExit")
        : buttonMode === "editor"
          ? t("fullscreenEditor")
          : t("fullscreenPreview");
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
  document.getElementById("save-button").addEventListener("click", () => post({ type: "save" }));
  document.getElementById("save-as-button").addEventListener("click", () => post({ type: "saveAs" }));
  document.getElementById("pdf-button").addEventListener("click", () => {
    pdfStarting = true;
    saveState.textContent = t("pdfStarting");
    post({ type: "exportPdf" });
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
    if (!noticeState) {
      notice.hidden = true;
      return;
    }
    noticeText.textContent = noticeState.kind === "message"
      ? noticeState.text
      : t(noticeState.kind === "lossy" ? "noticeLossy" : "noticeGuessed")(encodingLabel(noticeState.encoding));
    document.getElementById("notice-encoding").hidden = noticeState.kind === "message";
    notice.classList.toggle("notice-strong", noticeState.kind === "lossy");
    notice.hidden = false;
  };

  const showNoticeMessage = (text) => {
    noticeState = { kind: "message", text };
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
  document.getElementById("notice-close").addEventListener("click", () => {
    noticeState = null;
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
  languageButton.addEventListener("click", () => {
    language = language === "ja" ? "en" : "ja";
    localStorage.setItem("mdeditor-language", language);
    applyLanguage();
    post({ type: "setLanguage", lang: language });
  });
  fullscreenButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const mode = button.dataset.fullscreenMode;
      setDisplayMode(displayMode === mode ? "normal" : mode);
    });
  });

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
    { block: true,
      ja: ["見出し", "# 見出し1\n## 見出し2\n### 見出し3", '<span class="help-h1">見出し1</span><span class="help-h2">見出し2</span><span class="help-h3">見出し3</span>'],
      en: ["Headings", "# Heading 1\n## Heading 2\n### Heading 3", '<span class="help-h1">Heading 1</span><span class="help-h2">Heading 2</span><span class="help-h3">Heading 3</span>'] },
    { block: true,
      ja: ["段落", "1つ目の段落\n\n2つ目の段落", "<p>1つ目の段落</p><p>2つ目の段落</p><small>空行で段落が分かれます</small>"],
      en: ["Paragraphs", "First paragraph\n\nSecond paragraph", "<p>First paragraph</p><p>Second paragraph</p><small>A blank line starts a new paragraph</small>"] },
    { block: true,
      ja: ["改行", "1行目\\\n2行目", "<p>1行目<br>2行目</p><small>行末に \\ を付けるか、半角スペースを2つ入れると改行します。何も付けずに改行すると、前の行に続けて表示されます</small>"],
      en: ["Line break", "First line\\\nSecond line", "<p>First line<br>Second line</p><small>End a line with \\ or two spaces to break it. A plain line break joins the lines in the output</small>"] },
    { ja: ["太字", "**太字**", "<strong>太字</strong>"], en: ["Bold", "**bold**", "<strong>bold</strong>"] },
    { ja: ["斜体", "*斜体*", "<em>斜体</em>"], en: ["Italic", "*italic*", "<em>italic</em>"] },
    { ja: ["取り消し線", "~~取り消し線~~", "<del>取り消し線</del>"], en: ["Strikethrough", "~~strikethrough~~", "<del>strikethrough</del>"] },
    { ja: ["コード（文中）", "`コード`", "<code>コード</code>"], en: ["Inline code", "`code`", "<code>code</code>"] },
    { ja: ["リンク", "[リンクの文字](https://example.com)", '<span class="help-link">リンクの文字</span>'],
      en: ["Link", "[link text](https://example.com)", '<span class="help-link">link text</span>'] },
    { ja: ["画像", "![画像の説明](https://example.com/image.png)", "<small>画像が表示されます</small>"],
      en: ["Image", "![image description](https://example.com/image.png)", "<small>The image is shown</small>"] },
    { block: true,
      ja: ["箇条書き", "- 項目1\n- 項目2\n  - 入れ子の項目", "<ul><li>項目1</li><li>項目2<ul><li>入れ子の項目</li></ul></li></ul>"],
      en: ["Bulleted list", "- Item 1\n- Item 2\n  - Nested item", "<ul><li>Item 1</li><li>Item 2<ul><li>Nested item</li></ul></li></ul>"] },
    { block: true,
      ja: ["番号付きリスト", "1. 手順1\n2. 手順2", "<ol><li>手順1</li><li>手順2</li></ol>"],
      en: ["Numbered list", "1. Step 1\n2. Step 2", "<ol><li>Step 1</li><li>Step 2</li></ol>"] },
    { block: true,
      ja: ["引用", "> 引用文", "<blockquote>引用文</blockquote>"],
      en: ["Quote", "> Quoted text", "<blockquote>Quoted text</blockquote>"] },
    { block: true,
      ja: ["コードブロック", "```js\nconsole.log(\"hello\");\n```", "<pre><code>console.log(\"hello\");</code></pre>"],
      en: ["Code block", "```js\nconsole.log(\"hello\");\n```", "<pre><code>console.log(\"hello\");</code></pre>"] },
    { block: true,
      ja: ["表", "| 名前 | 数 |\n| --- | ---: |\n| りんご | 3 |", '<table><thead><tr><th>名前</th><th class="help-right">数</th></tr></thead><tbody><tr><td>りんご</td><td class="help-right">3</td></tr></tbody></table>'],
      en: ["Table", "| Name | Qty |\n| --- | ---: |\n| Apple | 3 |", '<table><thead><tr><th>Name</th><th class="help-right">Qty</th></tr></thead><tbody><tr><td>Apple</td><td class="help-right">3</td></tr></tbody></table>'] },
    { block: true, ja: ["区切り線", "---", "<hr>"], en: ["Horizontal rule", "---", "<hr>"] },
    { block: true,
      ja: ["図（Mermaid）", "```mermaid\nflowchart LR\n  A[開始] --> B[終了]\n```", "<small>フローチャートやシーケンス図などが描かれます</small>"],
      en: ["Diagram (Mermaid)", "```mermaid\nflowchart LR\n  A[Start] --> B[End]\n```", "<small>Draws flowcharts, sequence diagrams, and more</small>"] },
    { math: "E = mc^2", ja: ["数式", "$E = mc^2$", ""], en: ["Math", "$E = mc^2$", ""] }
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

  const renderHelp = () => {
    helpRows.replaceChildren();
    for (const item of helpItems) {
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
      if (item.math && window.katex) {
        window.katex.render(item.math, resultCell, { throwOnError: false });
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
    activeLine: document.getElementById("setting-active-line"),
    thickCaret: document.getElementById("setting-thick-caret"),
    reader: document.getElementById("setting-reader"),
    rate: document.getElementById("setting-reader-rate"),
    pause: document.getElementById("setting-reader-pause")
  };
  const backupCheckbox = document.getElementById("setting-backup");
  const zoomValue = document.getElementById("zoom-value");
  const readerControls = document.getElementById("reader-controls");
  const readerPlay = document.getElementById("reader-play");
  const readerStop = document.getElementById("reader-stop");
  const readerVoiceNote = document.getElementById("reader-voice-note");

  // Applies the font, size, line spacing, editor aids, and read-aloud visibility.
  const applyDisplaySettings = (rerender = true) => {
    const root = document.documentElement;
    const fontChanged = root.classList.contains("font-ud") !== (displaySettings.font === "ud");
    root.classList.toggle("font-ud", displaySettings.font === "ud");
    root.style.setProperty("--doc-scale", displaySettings.size);
    root.style.setProperty("--doc-line-height", displaySettings.spacing);
    view.dispatch({ effects: editorExtras.reconfigure(editorExtrasFor(displaySettings)) });
    readerControls.hidden = !displaySettings.reader;
    if (!displaySettings.reader) stopReading();
    updateVoiceNote();
    // Diagram labels use the UI font, so a font change redraws them.
    if (rerender && fontChanged && lastPreviewHtml !== null) renderPreview(lastPreviewHtml);
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

  const openSettings = () => {
    fillSettingControls();
    updateVoiceNote();
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
  const hasJapanese = (text) => /[぀-ヿ㐀-鿿]/.test(text);
  const localVoice = (lang) => speechSynthesis.getVoices()
    .filter((voice) => voice.localService)
    .find((voice) => voice.lang.toLowerCase().startsWith(lang));

  const updateVoiceNote = () => {
    const ja = localVoice("ja");
    const en = localVoice("en");
    readerVoiceNote.textContent = ja || en
      ? t("readerVoices")(ja?.name ?? t("readerNoVoiceShort"), en?.name ?? t("readerNoVoiceShort"))
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

  const speakNext = () => {
    if (!reader) return;
    reader.timer = 0;
    reader.waiting = false;
    const block = reader.blocks[reader.index];
    if (!block || !block.isConnected) {
      stopReading();
      return;
    }
    clearReadingMark();
    block.classList.add("reading");
    block.scrollIntoView({ block: "nearest" });
    const text = readableText(block);
    const voice = localVoice(hasJapanese(text) ? "ja" : "en") ?? localVoice(language);
    if (!voice) {
      stopReading();
      showNoticeMessage(t("readerNoVoice"));
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
    { id: "save", keys: "Ctrl+S", button: "save-button", run: () => post({ type: "save" }) },
    { id: "saveAs", keys: "Ctrl+Shift+S", button: "save-as-button", run: () => post({ type: "saveAs" }) },
    { id: "pdf", keys: "Ctrl+Shift+P", button: "pdf-button", run: () => document.getElementById("pdf-button").click() },
    { id: "paper", keys: "Ctrl+Shift+A", button: "paper-button", run: () => paperButton.click() },
    { id: "theme", keys: "Ctrl+Shift+T", button: "theme-button", run: () => themeButton.click() },
    { id: "language", keys: "Ctrl+Shift+L", button: "language-button", run: () => languageButton.click() },
    { id: "encoding", keys: "Ctrl+Shift+E", button: "encoding-button", run: () => openEncodingDialog() },
    { id: "findReplace", keys: "Ctrl+H", button: "replace-button", run: () => openFindBar() },
    { id: "findNext", keys: "F3", button: "find-next", run: () => (findBar.hidden ? openFindBar() : findText(true)) },
    { id: "findPrev", keys: "Shift+F3", button: "find-prev", run: () => (findBar.hidden ? openFindBar() : findText(false)) },
    { id: "replaceAll", keys: "Ctrl+Shift+H", button: "replace-all", run: () => (findBar.hidden || !findInput.value ? openFindBar() : replaceAll()) },
    { id: "help", keys: "F1", button: "help-button", run: () => openHelp() },
    { id: "shortcuts", keys: "Shift+F1", button: "shortcuts-button", run: () => openShortcutDialog() },
    { id: "fullscreenEditor", keys: "Ctrl+Shift+1", selector: "[data-fullscreen-mode='editor']", run: () => setDisplayMode(displayMode === "editor" ? "normal" : "editor") },
    { id: "fullscreenPreview", keys: "Ctrl+Shift+2", selector: "[data-fullscreen-mode='preview']", run: () => setDisplayMode(displayMode === "preview" ? "normal" : "preview") },
    { id: "nextPane", keys: "F6", run: () => movePaneFocus(1) },
    { id: "previousPane", keys: "Shift+F6", run: () => movePaneFocus(-1) },
    { id: "windowFullscreen", keys: "F11", run: () => post({ type: "toggleWindowFullscreen" }) },
    { id: "settings", keys: "Ctrl+Shift+O", button: "settings-button", run: () => openSettings() },
    { id: "readAloud", keys: "Ctrl+Shift+Y", button: "reader-play", run: () => toggleReading() },
    { id: "stopReading", keys: "Ctrl+Shift+U", button: "reader-stop", run: () => stopReading() }
  ];
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
    { ja: "図と数式", en: "Diagrams and math",
      text: {
        ja: "# 図と数式のサンプル\n\nMarkdown を書くと、右のプレビューにすぐ反映されます。\n\n## 図（Mermaid）\n\n```mermaid\nflowchart LR\n  A[書く] --> B[確認する]\n  B --> C[PDFに保存]\n```\n\n## 数式（KaTeX）\n\n円の面積は $S = \\pi r^2$ です。\n\n$$\\int_0^1 x^2\\,dx = \\frac{1}{3}$$\n\n## 表とチェックリスト\n\n| 名前 | 数 |\n| --- | ---: |\n| りんご | 3 |\n| みかん | 5 |\n\n- [x] 図を見る\n- [ ] PDFに保存する\n",
        en: "# Diagrams and math sample\n\nWhat you type appears in the preview on the right right away.\n\n## Diagram (Mermaid)\n\n```mermaid\nflowchart LR\n  A[Write] --> B[Check]\n  B --> C[Save as PDF]\n```\n\n## Math (KaTeX)\n\nThe area of a circle is $S = \\pi r^2$.\n\n$$\\int_0^1 x^2\\,dx = \\frac{1}{3}$$\n\n## Table and checklist\n\n| Name | Qty |\n| --- | ---: |\n| Apple | 3 |\n| Orange | 5 |\n\n- [x] See the diagram\n- [ ] Save as PDF\n" } },
    { ja: "基本の書き方", en: "Basic writing",
      text: {
        ja: "# はじめての Markdown\n\n**太字**、*斜体*、[リンク](https://example.com) が使えます。\n\n1. 見出し\n2. リスト\n3. 引用\n\n> 引用はこのように書きます。\n\n```\nコードはこのように書きます。\n```\n",
        en: "# Your first Markdown\n\nYou can use **bold**, *italic*, and [links](https://example.com).\n\n1. Headings\n2. Lists\n3. Quotes\n\n> Quotes look like this.\n\n```\nCode looks like this.\n```\n" } }
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
      button.addEventListener("click", () => {
        if (view.state.doc.length > 0 && !window.confirm(t("sampleConfirm"))) return;
        sampleDialog.close();
        view.dispatch({ changes: { from: 0, to: view.state.doc.length, insert: sample.text[language] } });
        editorScroller.scrollTop = 0;
        preview.scrollTop = 0;
        view.focus();
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

  document.addEventListener("keydown", (event) => {
    // The open dialog handles Esc itself; preventing it here would keep it open.
    if (document.querySelector("dialog[open]")) return;
    // Esc always restores the layout or leaves full screen; it is not configurable.
    if (event.key === "Escape" && !event.ctrlKey && !event.altKey && !event.shiftKey) {
      event.preventDefault();
      if (reader) {
        stopReading();
      } else if (displayMode !== "normal") {
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
    definition.run();
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
    points.push({ line: view.state.doc.lines, top: preview.scrollHeight });
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

  const syncFrom = (source) => {
    if (editorScroller.clientHeight === 0 || preview.clientHeight === 0) return;
    const target = source === editorScroller ? preview : editorScroller;
    const atTop = source.scrollTop <= 0;
    const atBottom = source.scrollTop >= source.scrollHeight - source.clientHeight - 1;
    if (atTop || atBottom) {
      scrollWithoutEcho(target, atTop ? 0 : target.scrollHeight);
      return;
    }
    const points = previewAnchors();
    if (source === editorScroller) {
      scrollWithoutEcho(preview, interpolate(points, editorLineAtTop(), "line", "top"));
    } else {
      const line = interpolate(points, preview.scrollTop, "top", "line");
      scrollWithoutEcho(editorScroller, editorTopOfLine(line));
    }
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
  let lastPreviewHtml = null;
  const darkScheme = window.matchMedia("(prefers-color-scheme: dark)");

  // PDF export always uses the day palette, so print renders use the light theme.
  // Diagrams use the same typeface as the UI language (styles.css :root and
  // :root:lang(en)), so drawings made in one language are not reused in the other.
  const useMermaidTheme = (forPrint) => {
    const theme = !forPrint && darkScheme.matches ? "dark" : "default";
    const fontFamily = getComputedStyle(document.documentElement).fontFamily;
    if (mermaidConfig?.fontFamily !== fontFamily) {
      for (const cache of Object.values(diagramCaches)) cache.clear();
    }
    if (mermaidConfig?.theme !== theme || mermaidConfig?.fontFamily !== fontFamily) {
      window.mermaid?.initialize({
        startOnLoad: false,
        securityLevel: "strict",
        theme,
        fontFamily
      });
      mermaidConfig = { theme, fontFamily };
    }
    return diagramCaches[theme];
  };

  const renderMath = () => {
    if (!window.katex) return;
    for (const element of preview.querySelectorAll(".math")) {
      const source = element.textContent.trim()
        .replace(/^\\[([]/, "")
        .replace(/\\[)\]]$/, "")
        .trim();
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

  const renderPreview = async (html, forPrint = false) => {
    const generation = ++renderGeneration;
    const diagramCache = useMermaidTheme(forPrint);
    if (reader && !forPrint && html !== lastPreviewHtml) stopReading();
    if (!forPrint) lastPreviewHtml = html;
    const previousSvgs = Array.from(
      preview.querySelectorAll(".mermaid"),
      (element) => (element.classList.contains("diagram-rendered") ? element.innerHTML : null)
    );
    preview.innerHTML = html;
    moveAnchorsOffIds();
    labelTaskCheckboxes();
    showUnsupportedDiagrams();
    renderMath();
    if (view.hasFocus) syncFrom(editorScroller);
    await renderDiagrams(generation, previousSvgs, diagramCache);
    if (generation === renderGeneration && view.hasFocus) syncFrom(editorScroller);
  };

  darkScheme.addEventListener("change", () => {
    if (lastPreviewHtml !== null) renderPreview(lastPreviewHtml);
  });

  const hasFiles = (event) =>
    Array.from(event.dataTransfer?.types ?? []).includes("Files");
  window.addEventListener("dragover", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
    document.body.classList.add("dragging");
  }, true);
  window.addEventListener("dragleave", (event) => {
    if (event.relatedTarget === null) document.body.classList.remove("dragging");
  }, true);
  window.addEventListener("drop", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    event.stopPropagation();
    document.body.classList.remove("dragging");
    const files = Array.from(event.dataTransfer.files);
    window.chrome.webview.postMessageWithAdditionalObjects(
      { type: "dropFile", count: files.length },
      files
    );
  }, true);

  window.chrome.webview.addEventListener("message", (event) => {
    const message = event.data;
    if (message.type === "documentInfo") {
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
      language = message.lang === "en" ? "en" : "ja";
      applyLanguage();
    } else if (message.type === "scrollToAnchor") {
      // Sent after another document opens from a link with #heading.
      requestAnimationFrame(() => scrollToAnchor(message.name));
    } else if (message.type === "zoom") {
      zoomValue.textContent = `${Math.round(message.factor * 100)}%`;
    } else if (message.type === "autoBackup") {
      backupCheckbox.checked = Boolean(message.enabled);
    } else if (message.type === "encodingNotice") {
      noticeState = message.kind ? { kind: message.kind, encoding: message.encoding } : null;
      updateNotice();
    } else if (message.type === "paper") {
      paper = paperOrder.includes(message.paper) ? message.paper : "auto";
      autoPaper = message.autoPaper === "letter" ? "letter" : "a4";
      updatePaperButton();
    } else if (message.type === "theme") {
      theme = themeOrder.includes(message.theme) ? message.theme : "auto";
      updateThemeButton();
    } else if (message.type === "setText") {
      view.setState(EditorState.create({ doc: message.text, extensions: editorExtensions() }));
      // A newly loaded document starts at the top, not where the last one was.
      editorScroller.scrollTop = 0;
      preview.scrollTop = 0;
    } else if (message.type === "preview") {
      renderPreview(message.html, message.forPrint).finally(() => {
        if (message.notifyRendered) post({ type: "previewRendered", requestId: message.requestId });
      });
    } else if (message.type === "pdfExportStatus") {
      saveState.textContent = message.message || statusText();
    }
  });

  applyDisplaySettings(false);
  applyLanguage();
  post({ type: "ready", lang: language });
})();
