/*
  File: tools/codemirror/entry.js
  Description: Exposes the CodeMirror 6 pieces mdEditor uses as window.CodeMirror
  Author: 5garashi.com Design Office
  Created: 2026-10-11
  License: MIT
  SPDX-License-Identifier: MIT
*/
// Build with: npm ci && npm run build (writes wwwroot/lib/codemirror/codemirror.min.js).
import { EditorView, drawSelection, highlightActiveLine, keymap } from "@codemirror/view";
import { Compartment, EditorSelection, EditorState } from "@codemirror/state";
import { defaultKeymap, history, historyKeymap } from "@codemirror/commands";

window.CodeMirror = {
  Compartment, EditorSelection, EditorState, EditorView,
  defaultKeymap, drawSelection, highlightActiveLine, history, historyKeymap, keymap
};
