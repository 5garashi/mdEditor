/*
  ファイル名 / File: wwwroot/app.js
  概要 / Description: Connects editor controls to the native application host
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
(() => {
  const editor = document.getElementById("editor");
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
      toolbar: "文書操作", dropHint: "ここに .md ファイルをドロップして開く",
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
      diagramError: "図を表示できません", diagramUnavailable: "図の表示機能を読み込めませんでした",
      ratio: (e, p) => `編集画面 ${e}%、プレビュー ${p}%`
    },
    en: {
      toolbar: "Document actions", dropHint: "Drop a .md file here to open it",
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
    themeButton.title = t("themeTitle");
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

  editor.addEventListener("input", () => {
    post({ type: "change", text: editor.value });
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
    const selected = editor.value.slice(editor.selectionStart, editor.selectionEnd);
    if (selected && !selected.includes("\n")) {
      findInput.value = selected;
    }
    findStatus.textContent = "";
    findInput.focus();
    findInput.select();
  };

  const closeFindBar = () => {
    findBar.hidden = true;
    editor.focus();
  };

  // Matches come from the original text, so selection positions stay right even
  // when lowercasing would change a string's length (for example "İ").
  const findMatches = () => {
    const pattern = findInput.value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const regex = new RegExp(pattern, findCase.checked ? "g" : "gi");
    return Array.from(editor.value.matchAll(regex), (match) => ({
      index: match.index,
      length: match[0].length
    }));
  };

  // Measures the wrapped position of the match with a hidden mirror of the textarea.
  const scrollMatchIntoView = (index) => {
    const style = getComputedStyle(editor);
    const mirror = document.createElement("div");
    for (const name of [
      "boxSizing", "width", "paddingTop", "paddingRight", "paddingBottom", "paddingLeft",
      "borderTopWidth", "borderRightWidth", "borderBottomWidth", "borderLeftWidth",
      "fontFamily", "fontSize", "fontWeight", "lineHeight", "letterSpacing", "tabSize",
      "whiteSpace", "overflowWrap"
    ]) {
      mirror.style[name] = style[name];
    }
    mirror.style.position = "absolute";
    mirror.style.visibility = "hidden";
    mirror.style.top = "0";
    mirror.textContent = editor.value.slice(0, index);
    const marker = document.createElement("span");
    marker.textContent = "\u200b";
    mirror.appendChild(marker);
    document.body.appendChild(mirror);
    const top = marker.offsetTop;
    mirror.remove();
    const lineHeight = Number.parseFloat(style.lineHeight) || 24;
    if (top < editor.scrollTop || top + lineHeight > editor.scrollTop + editor.clientHeight) {
      editor.scrollTop = Math.max(0, top - editor.clientHeight / 2);
    }
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
    const match = forward
      ? matches.find((m) => m.index >= editor.selectionEnd) ?? matches[0]
      : matches.findLast((m) => m.index < editor.selectionStart) ?? matches[matches.length - 1];
    findStatus.textContent = "";
    editor.focus();
    editor.setSelectionRange(match.index, match.index + match.length);
    scrollMatchIntoView(match.index);
    return true;
  };

  const replaceSelection = (text) => {
    editor.focus();
    if (!document.execCommand("insertText", false, text)) {
      editor.setRangeText(text, editor.selectionStart, editor.selectionEnd, "end");
      post({ type: "change", text: editor.value });
    }
  };

  const replaceOne = () => {
    const needle = findInput.value;
    if (!needle) return;
    const selectedIsMatch = findMatches().some((m) =>
      m.index === editor.selectionStart && m.index + m.length === editor.selectionEnd);
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
    let result = "";
    let last = 0;
    for (const match of matches) {
      result += editor.value.slice(last, match.index) + replaceInput.value;
      last = match.index + match.length;
    }
    result += editor.value.slice(last);
    editor.focus();
    editor.select();
    replaceSelection(result);
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
    const before = editor.value.slice(0, editor.selectionStart);
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

  document.addEventListener("keydown", (event) => {
    // The open dialog handles Esc itself; preventing it here would keep it open.
    if (helpDialog.open) return;
    if (!event.ctrlKey && !event.metaKey) {
      if (event.key === "F1") {
        event.preventDefault();
        openHelp();
      } else if (event.key === "F11") {
        event.preventDefault();
        post({ type: "toggleWindowFullscreen" });
      } else if (event.key === "Escape") {
        event.preventDefault();
        if (displayMode !== "normal") {
          setDisplayMode("normal");
        } else {
          post({ type: "exitWindowFullscreen" });
        }
      }
      return;
    }

    const key = event.key.toLowerCase();
    if (key === "h" && !event.shiftKey) {
      event.preventDefault();
      openFindBar();
    } else if (key === "s") {
      event.preventDefault();
      post({ type: event.shiftKey ? "saveAs" : "save" });
    } else if (key === "o") {
      event.preventDefault();
      post({ type: "open" });
    } else if (key === "p" && event.shiftKey) {
      event.preventDefault();
      post({ type: "exportPdf" });
    }
  });

  preview.addEventListener("click", (event) => {
    const link = event.target.closest("a");
    if (!link || !link.href) {
      return;
    }

    const url = new URL(link.href);
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
    if (!forPrint) lastPreviewHtml = html;
    const previousSvgs = Array.from(
      preview.querySelectorAll(".mermaid"),
      (element) => (element.classList.contains("diagram-rendered") ? element.innerHTML : null)
    );
    preview.innerHTML = html;
    showUnsupportedDiagrams();
    renderMath();
    await renderDiagrams(generation, previousSvgs, diagramCache);
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
      documentDirty = message.isDirty;
      pdfStarting = false;
      saveState.textContent = statusText();
    } else if (message.type === "language") {
      language = message.lang === "en" ? "en" : "ja";
      applyLanguage();
    } else if (message.type === "theme") {
      theme = themeOrder.includes(message.theme) ? message.theme : "auto";
      updateThemeButton();
    } else if (message.type === "setText") {
      editor.value = message.text;
    } else if (message.type === "preview") {
      renderPreview(message.html, message.forPrint).finally(() => {
        if (message.notifyRendered) post({ type: "previewRendered", requestId: message.requestId });
      });
    } else if (message.type === "pdfExportStatus") {
      saveState.textContent = message.message || statusText();
    }
  });

  applyLanguage();
  post({ type: "ready", lang: language });
})();
