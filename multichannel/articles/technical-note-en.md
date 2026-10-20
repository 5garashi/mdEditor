<!--
File: promotion/multichannel/articles/technical-note-en.md
Description: mdEditor use-case article
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.12.0
License: MIT
SPDX-License-Identifier: MIT
-->
# Diagrams, 3D and video in one Markdown document

Start with an STL cube you can rotate and zoom. Then explore diagrams, math, maps and video inside the same document, and the main editing and sharing features of mdEditor, a free Windows app developed by 5garashi.com Design Office.

[Watch the source 1.0.14 demonstration](https://5garashi.github.io/mdEditor/en.html#demo) · [YouTube entry](https://5garashi.github.io/mdEditor/youtube-en.html)

![Actual mdEditor: STL source beside a 3D cube with a red 5.](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.14-en.png)

## Try the same sample

Save [diagram-test.en.md](https://5garashi.github.io/mdEditor/samples/diagram-test.en.md), then choose Open in mdEditor. Scroll from the top; drag the STL canvas to rotate and use the wheel to zoom. Internet images, video and detailed OpenStreetMap tiles load only with permission. Servers receive information such as your IP address when you permit a request. The sample deliberately contains one syntax-error example.

This sample targets mdEditor. GitHub also renders STL and GeoJSON; DOT needs a compatible renderer such as mdEditor. Rendering behavior is not identical in every environment. Use the sample in mdEditor; use Mermaid that renders in both environments for shared Markdown documents.

## Write with the result beside you

Edit a heading and check the live preview. Synchronized scrolling keeps the result near your current line. Detach the preview to put it in another window, or expand it to read the document. A second physical display was not tested in this recording.

## Drop, resize and center an image

Drop an image into the editor, then use image adjustment to set 50% width and centered alignment. Save the document and its used assets together as a sharing ZIP.

![Actual centered image adjustment.](https://5garashi.github.io/mdEditor/assets/image-adjust-1.0.14-en.png)

## Share a document

Use Save to keep the Markdown. Use Save As and choose PDF, Word, HTML, PNG or another supported format. Open the result and check diagrams, text and page breaks before sharing. Current source 1.0.14 also offers one-click PDF export. Word import does not preserve its colors or layout; notebook export omits execution output. Equations and table values are not automatically calculated.

## Main features

- Live preview beside your text, scrolling together
- Preview on a second display, following the line you edit
- Also a viewer: the preview fills the window
- Mermaid and Graphviz diagrams and math, offline
- 3D models (STL) you turn with the mouse
- Maps (GeoJSON), with OpenStreetMap when you allow it
- Colored code for about 40 languages, kept in PDF and Word
- Table of contents from your headings, updated with one press
- GitHub-style alerts, front matter, and collapsible sections
- Videos play right in the preview
- One-click PDF that looks exactly like the preview
- Opens Word, HTML, e-mail, CSV, JSON, Jupyter Notebooks, and images
- Saves as HTML, PNG, Word, CSV, JSON, Jupyter Notebook, or text; sharing ZIP
- Drop or paste images; adjust their size and centered alignment
- Safe saving and automatic crash recovery
- Opens Shift_JIS, EUC-JP, and UTF-8 files correctly
- Internet images, videos, and maps load only when you allow them
- No ads, no tracking, no account
- Everything from the keyboard, with custom shortcuts
- Screen reader support, read-aloud, and dark mode

## Scope and credits

The new video uses source build 1.0.14 (local source snapshot), with captions and Light Steps BGM. Import of seven formats, PDF export, diagrams, math, maps, video and rotating 3D were checked during recording. Full keyboard coverage, crash recovery, all export formats and screen-reader behavior were not exhaustively retested. The desktop background is covered during recording. Native file-dialog intervals are removed.

[PDFs, images and samples](https://5garashi.github.io/mdEditor/resources-en.html) · [App privacy](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md)

Design reference: [Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/). App, article and notation examples: MIT. Official logos: CC BY-ND 4.0.

---

**Author**: 5garashi.com Design Office
**Last updated**: 2026-10-20 JST
