/*
  ファイル名 / File: wwwroot/fix-worker.js
  概要 / Description: 自動修正のルールの正規表現を、画面を止めずに当てる / Runs a fix rule's regular expression off the main thread
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-30
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
// One rule over one text. The page sends the parts it must not touch (code, math, HTML, link
// addresses, as the rule says) and stops this worker when a pattern takes too long, so a
// pattern that runs away never freezes the editor.
self.onmessage = ({ data }) => {
  const { id, text, pattern, replacement, ignoreCase, multiline, skipped, limit } = data;
  const flags = `${ignoreCase ? "i" : ""}${multiline ? "m" : ""}u`;
  let regex;
  let sticky;
  try {
    regex = new RegExp(pattern, `g${flags}`);
    sticky = new RegExp(pattern, `y${flags}`);
  } catch (error) {
    self.postMessage({ id, error: String(error.message || error) });
    return;
  }
  const isSkipped = (from, to) => skipped.some(([skipFrom, skipTo]) => skipFrom < to && from < skipTo);
  // The replacement of one match, made by String.prototype.replace itself (so $1, $<name>,
  // $&, and $$ mean just what they mean there): the same pattern, held to where it matched.
  const expand = (match, input) => {
    sticky.lastIndex = match.index;
    const replaced = input.replace(sticky, replacement);
    return replaced.slice(match.index, replaced.length - (input.length - match.index - match[0].length));
  };
  const pieces = [];
  // One line at a time, unless the rule reads across lines (then ^ and $ still mean a line).
  const parts = multiline ? [[0, text]] : [];
  if (!multiline) {
    let at = 0;
    for (const line of text.split("\n")) {
      parts.push([at, line]);
      at += line.length + 1;
    }
  }
  for (const [offset, input] of parts) {
    regex.lastIndex = 0;
    for (let match = regex.exec(input); match; match = regex.exec(input)) {
      if (match[0] === "") {
        regex.lastIndex += 1;
        continue;
      }
      const from = offset + match.index;
      const to = from + match[0].length;
      // A match that runs into a part not to touch is tried again one character later, so a
      // real match that starts inside it is not passed over.
      if (isSkipped(from, to)) {
        regex.lastIndex = match.index + (input.codePointAt(match.index) > 0xFFFF ? 2 : 1);
        continue;
      }
      const insert = expand(match, input);
      if (insert === match[0]) continue;
      pieces.push({ from, to, insert });
      if (pieces.length > limit) {
        self.postMessage({ id, many: true });
        return;
      }
    }
  }
  self.postMessage({ id, pieces });
};
