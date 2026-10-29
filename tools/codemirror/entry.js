/*
  File: tools/codemirror/entry.js
  Description: Exposes the CodeMirror 6 pieces mdEditor uses as window.CodeMirror
  Author: 5garashi.com Design Office
  Created: 2026-10-11
  License: MIT
  SPDX-License-Identifier: MIT
*/
// Build with: npm ci && npm run build (writes wwwroot/lib/codemirror/codemirror.min.js).
import { Decoration, EditorView, ViewPlugin, drawSelection, highlightActiveLine, keymap } from "@codemirror/view";
import { Compartment, EditorSelection, EditorState, RangeSetBuilder } from "@codemirror/state";
import { defaultKeymap, history, historyKeymap } from "@codemirror/commands";
import { closeLintPanel, diagnosticCount, forEachDiagnostic, forceLinting, linter, lintGutter, lintKeymap, openLintPanel } from "@codemirror/lint";
import { HighlightStyle, Language, defineLanguageFacet, syntaxHighlighting } from "@codemirror/language";
import { styleTags, tags } from "@lezer/highlight";
import { GFM, parser } from "@lezer/markdown";

// Math as mdEditor writes it: $…$, $$…$$, \(…\) and \[…\]. A single $ needs no space inside
// its ends and no digit after it, so prices ($5 and $10) stay text; \$ never closes one.
const mathDelimiters = [["$$", "$$"], ["\\[", "\\]"], ["\\(", "\\)"], ["$", "$"]];
const MathSyntax = {
  defineNodes: [
    { name: "Math", style: tags.special(tags.string) },
    { name: "MathMark", style: tags.processingInstruction }
  ],
  parseInline: [{
    name: "Math",
    before: "Escape",
    parse(cx, next, pos) {
      if (next !== 36 && next !== 92) return -1;
      const rest = cx.slice(pos, cx.end);
      for (const [open, close] of mathDelimiters) {
        if (!rest.startsWith(open)) continue;
        const single = open === "$";
        if (single && /^\$\s/.test(rest)) return -1;
        let end = rest.indexOf(close, open.length);
        while (end > 0 && rest[end - 1] === "\\" && open !== "\\(" && open !== "\\[") end = rest.indexOf(close, end + 1);
        if (single && end > 0 && (/\s/.test(rest[end - 1]) || /\d/.test(rest[end + 1] ?? ""))) return -1;
        if (end <= open.length || (open !== "$$" && open !== "\\[" && rest.slice(0, end).includes("\n"))) return -1;
        return cx.addElement(cx.elt("Math", pos, pos + end + close.length, [
          cx.elt("MathMark", pos, pos + open.length),
          cx.elt("MathMark", pos + end, pos + end + close.length)
        ]));
      }
      return -1;
    }
  }]
};
// GitHub's Markdown (tables, task lists, strikethrough, bare addresses), math, and HTML tags.
const markdownLanguage = new Language(
  defineLanguageFacet({ commentTokens: { block: { open: "<!--", close: "-->" } } }),
  parser.configure([GFM, MathSyntax, { props: [styleTags({ "HTMLTag HTMLBlock": tags.tagName })] }]),
  [],
  "markdown"
);

window.CodeMirror = {
  Compartment, Decoration, EditorSelection, EditorState, EditorView, RangeSetBuilder, ViewPlugin,
  defaultKeymap, drawSelection, highlightActiveLine, history, historyKeymap, keymap,
  closeLintPanel, diagnosticCount, forEachDiagnostic, forceLinting, linter, lintGutter, lintKeymap, openLintPanel,
  HighlightStyle, markdownLanguage, syntaxHighlighting, tags
};
