/*
  File: tools/markdownlint/entry.js
  Description: Exposes markdownlint's lint function as window.markdownlint for mdEditor's Markdown check
  Author: 5garashi.com Design Office
  Created: 2026-10-28
  License: MIT
  SPDX-License-Identifier: MIT
*/
// Build with: npm ci && npm run build (writes wwwroot/lib/markdownlint/markdownlint.min.js).
import { lint } from "markdownlint/sync";

window.markdownlint = { lint };
