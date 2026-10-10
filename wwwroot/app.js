/*
  File: wwwroot/app.js
  Description: Connects editor controls to the native application host
  Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  Created: 2026-10-10
  License: MIT
  SPDX-License-Identifier: MIT
*/
(() => {
  const editor = document.getElementById("editor");
  const preview = document.getElementById("preview");
  const documentName = document.getElementById("document-name");
  const saveState = document.getElementById("save-state");
  const languageButton = document.getElementById("language-button");
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
      findPlaceholder: "検索する文字列", replacePlaceholder: "置換後の文字列",
      findLabel: "検索", replaceLabel: "置換",
      findPrev: "前へ", findNext: "次へ", replaceOne: "置換", replaceAll: "すべて置換",
      matchCase: "大文字小文字を区別", close: "閉じる",
      notFound: "見つかりません", replacedCount: (n) => `${n} 件を置換しました`,
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
      findPlaceholder: "Text to find", replacePlaceholder: "Replace with",
      findLabel: "Find", replaceLabel: "Replace",
      findPrev: "Previous", findNext: "Next", replaceOne: "Replace", replaceAll: "Replace all",
      matchCase: "Match case", close: "Close",
      notFound: "Not found", replacedCount: (n) => `Replaced ${n}`,
      diagramError: "Unable to render the diagram", diagramUnavailable: "The diagram renderer could not be loaded",
      ratio: (e, p) => `Editor ${e}%, preview ${p}%`
    }
  };
  const storedLanguage = localStorage.getItem("mdeditor-language");
  let language = storedLanguage === "en" || storedLanguage === "ja"
    ? storedLanguage
    : (navigator.language || "").toLowerCase().startsWith("ja") ? "ja" : "en";
  const t = (key) => messages[language][key];

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
    saveState.textContent = pdfStarting ? t("pdfStarting") : statusText();
    setEditorRatio(editorRatio, false);
    setDisplayMode(displayMode);
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

  const norm = (s) => (findCase.checked ? s : s.toLowerCase());

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
    const hay = norm(editor.value);
    const target = norm(needle);
    let index;
    if (forward) {
      index = hay.indexOf(target, editor.selectionEnd);
      if (index < 0) index = hay.indexOf(target);
    } else {
      index = editor.selectionStart > 0
        ? hay.lastIndexOf(target, editor.selectionStart - 1)
        : -1;
      if (index < 0) index = hay.lastIndexOf(target);
    }
    if (index < 0) {
      findStatus.textContent = t("notFound");
      return false;
    }
    findStatus.textContent = "";
    editor.focus();
    editor.setSelectionRange(index, index + needle.length);
    scrollMatchIntoView(index);
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
    const selected = editor.value.slice(editor.selectionStart, editor.selectionEnd);
    if (norm(selected) === norm(needle)) {
      replaceSelection(replaceInput.value);
    }
    findText(true);
  };

  const replaceAll = () => {
    const needle = findInput.value;
    if (!needle) return;
    const hay = norm(editor.value);
    const target = norm(needle);
    let count = 0;
    let result = "";
    let last = 0;
    let index = hay.indexOf(target);
    while (index >= 0) {
      result += editor.value.slice(last, index) + replaceInput.value;
      last = index + needle.length;
      count += 1;
      index = hay.indexOf(target, last);
    }
    if (count === 0) {
      findStatus.textContent = t("notFound");
      return;
    }
    result += editor.value.slice(last);
    editor.focus();
    editor.select();
    replaceSelection(result);
    findStatus.textContent = t("replacedCount")(count);
  };

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
    if (!event.ctrlKey && !event.metaKey) {
      if (event.key === "F11") {
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

  // Rendered SVG keyed by Mermaid source, so unchanged diagrams are restored
  // synchronously on every preview update without flicker or scroll jumps.
  const diagramCache = new Map();
  let diagramCounter = 0;
  let renderGeneration = 0;

  window.mermaid?.initialize({
    startOnLoad: false,
    securityLevel: "strict",
    theme: "default",
    fontFamily: "\"Yu Gothic UI\", \"Meiryo\", sans-serif"
  });

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

  const showDiagramError = (element, source, detail) => {
    const message = document.createElement("p");
    message.className = "diagram-error-message";
    message.textContent = detail ? `${t("diagramError")}: ${detail}` : t("diagramError");
    const code = document.createElement("code");
    code.textContent = source;
    const pre = document.createElement("pre");
    pre.appendChild(code);
    element.classList.remove("diagram-pending");
    element.classList.add("diagram-error");
    element.replaceChildren(message, pre);
  };

  const renderDiagrams = async (generation, previousSvgs) => {
    // Markdig emits <pre class="mermaid">; a div keeps code-block styles out of the drawing.
    const blocks = Array.from(preview.querySelectorAll(".mermaid"), (block) => {
      const element = document.createElement("div");
      element.className = "mermaid";
      element.textContent = block.textContent;
      block.replaceWith(element);
      return { element, source: element.textContent };
    });
    const sources = new Set(blocks.map((block) => block.source));
    for (const source of diagramCache.keys()) {
      if (!sources.has(source)) diagramCache.delete(source);
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
        diagramCache.set(source, svg);
        if (generation !== renderGeneration) return;
        element.innerHTML = svg;
        element.classList.remove("diagram-pending");
        element.classList.add("diagram-rendered");
      } catch (error) {
        document.getElementById(`d${id}`)?.remove();
        if (generation !== renderGeneration) return;
        showDiagramError(element, source, error?.message ?? String(error));
      }
    }
  };

  const renderPreview = async (html) => {
    const generation = ++renderGeneration;
    const previousSvgs = Array.from(
      preview.querySelectorAll(".mermaid"),
      (element) => (element.classList.contains("diagram-rendered") ? element.innerHTML : null)
    );
    preview.innerHTML = html;
    showUnsupportedDiagrams();
    renderMath();
    await renderDiagrams(generation, previousSvgs);
  };

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
    } else if (message.type === "setText") {
      editor.value = message.text;
    } else if (message.type === "preview") {
      renderPreview(message.html).finally(() => {
        if (message.notifyRendered) post({ type: "previewRendered" });
      });
    } else if (message.type === "pdfExportStatus") {
      saveState.textContent = message.message || statusText();
    }
  });

  applyLanguage();
  post({ type: "ready", lang: language });
})();
