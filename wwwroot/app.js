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
    if (key === "s") {
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
      preview.innerHTML = message.html;
    } else if (message.type === "pdfExportStatus") {
      saveState.textContent = message.message || statusText();
    }
  });

  applyLanguage();
  post({ type: "ready", lang: language });
})();
