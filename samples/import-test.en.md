<!--
  File: import-test.en.md
  Description: Steps and expected results for each Open and Save As format
  Author: 5garashi.com Design Office
  Created: 2026-10-16
  License: MIT
  SPDX-License-Identifier: MIT
-->
# Open and export tests

These files check every format mdEditor opens and every type in Save As. Open each file with Open or drag and drop, and compare it with the expected result. The Japanese versions are the files without `.en` ([import-test.md](import-test.md)). The steps in Chinese are in [import-test.zh-CN.md](import-test.zh-CN.md), using the English files.

> The external-image results assume Settings → Internet is set to Ask (the default).

## 1. Files

| File | Format | What it checks |
| --- | --- | --- |
| [text-test.en.txt](text-test.en.txt) | Text | Opening and saving as is, line breaks, export without marks |
| [html-test.en.html](html-test.en.html) | HTML | Conversion to Markdown, removed parts, images |
| [word-test.en.docx](word-test.en.docx) | Word | Conversion to Markdown, images taken out |
| [mail-test.en.eml](mail-test.en.eml) | E-mail (HTML) | Subject and other fields, a picture in the mail, an attachment, an external image |
| [mail-text-test.en.eml](mail-text-test.en.eml) | E-mail (text only) | Line breaks |
| [table-test.en.csv](table-test.en.csv) | CSV (UTF-8) | Conversion to a table, cells with special characters |
| [table-test.en.tsv](table-test.en.tsv) | TSV (UTF-8) | Conversion to a table |
| [json-list-test.en.json](json-list-test.en.json) | JSON (a list) | Conversion to a table, nested values |
| [json-config-test.en.json](json-config-test.en.json) | JSON (settings and such) | A code block, comments and trailing commas |
| [xml-test.en.xml](xml-test.en.xml) | XML | A code block, a value containing "```" |
| [notebook-test.en.ipynb](notebook-test.en.ipynb) | Jupyter Notebook | Cells, outputs (text, table, picture, error), pictures in cells, saving back |
| [import-test_files/sample.png](import-test_files/sample.png) | Image | Opening an image and inserting it into the editor; also used by the HTML file |

## 2. Open

### 2.1 text-test.en.txt

1. Open text-test.en.txt.
2. The file name stays text-test.en.txt and no conversion notice appears.

Expected in the preview:

- "A single line break is joined…" joins the line before it in one paragraph.
- The lines after "ends with two spaces" and "A backslash" break.
- Only "A heading" is a heading; "#Not a heading" stays plain text.
- The lists, check boxes, bold text, link, quote, table, code block, math, and footnote show.

Finally, type one character and press Save. No location is asked, and text-test.en.txt is overwritten (undo it after checking).

### 2.2 html-test.en.html

Expected:

- The notice says the HTML was converted to Markdown, and the file name becomes html-test.en.md.
- Headings, bold, italic, the link, lists (with a nested one), the table, math (`$E = mc^2$` and `$$…$$`), the quote, and the code become Markdown.
- The red text loses its color.
- "Sample image" (beside the file) shows.
- The internet image shows as an "External image (github.githubassets.com), not loaded" box, and Load images appears in the notice bar. Pressing it shows the image.
- Only the button's text remains; the iframe, object, embed, and script text do not.
- html-test.en.html itself is never written (saving asks for a .md file).

### 2.3 word-test.en.docx

Expected:

- The notice says the Word document was converted, and the file name becomes word-test.en.md.
- Headings 1 to 3, bold, italic, the link, both lists, the table, and the image become Markdown.
- The image shows in the preview and points to `word-test.en_files/image1.png` in the editor.
- Saving as Markdown with Save As creates a word-test.en_files folder with the image beside the file.

### 2.4 mail-test.en.eml

Expected:

- The notice says the e-mail was converted, and the file name becomes mail-test.en.md.
- The first line is the subject "[Test] Opening e-mail in mdEditor" as a heading.
- From (Sam Sender <sato@example.com>), To, Cc, Date (09:30 Japan time, shown in this PC's time zone), and Attachments (note.txt) are listed.
- The bold text, link, list, and table in the body become Markdown.
- The picture inside the mail (cid) shows and points to `mail-test.en_files/image1.png`.
- The internet image (a 1-pixel tracker, as an example) does not load, and Load images appears in the notice bar.
- No script remains, and the attachment is not extracted.

### 2.5 mail-text-test.en.eml

Expected:

- The three body lines show on separate lines.
- Lines starting with ">" become a quote.

### 2.6 table-test.en.csv

Expected:

- The notice says the table file was opened as a Markdown table.
- The first row (Name, Team, Extension, Note) is the header.
- "A cell with a comma, here", "A cell with a / line break" (broken with `<br>`), "A cell with "quotes"", and "A cell with a | bar" each stay in one cell.

### 2.7 table-test.en.tsv

Expected:

- The header and three tab-separated rows become a table; "Commas, stay as they are" stays in one cell.
- The empty cell in the last row stays empty.

### 2.8 json-list-test.en.json

Expected:

- The notice says the JSON list was opened as a Markdown table.
- The field names (id, name, team, extension, active, note, licenses, address) are the header row. Missing fields leave the cell empty.
- "licenses" shows as "Architect, Surveyor" and "address" as `{"city":"Yokohama","ward":"Nishi"}`, each in one cell.

### 2.9 json-config-test.en.json

Expected:

- The notice says the JSON was opened in a code block.
- The comment and trailing comma do not stop it from opening; the JSON is re-indented in a code block (the comment is dropped).

### 2.10 xml-test.en.xml

Expected:

- The notice says the XML was opened in a code block.
- The content goes into the code block as it is. The "```" in a value does not close the block early (the block uses four backticks).

### 2.11 import-test_files/sample.png (opening an image)

Expected:

- The notice says the image was opened, and the file name becomes sample.md.
- The editor shows `![sample](sample.png)` and an empty line, and the preview shows the image.
- Write a description below it and save it somewhere else (such as the desktop) with Save As; sample.png is copied there too.

### 2.12 Inserting images in the editor

1. Open a saved Markdown file (a new one you have saved is fine).
2. Drag sample.png onto the editor.
3. Take a screen capture (Windows key + Shift + S) and paste it into the editor with Ctrl+V.

Expected:

- `![sample](name_files/sample.png)` is inserted where it was dropped, and the image is copied into a "name_files" folder beside the document.
- `![](name_files/image-date-time.png)` is inserted where it was pasted.
- A new document that has not been saved asks where to save first.
- An image dropped anywhere other than the editor (such as the toolbar) opens as an image document, as in 2.11.

### 2.13 notebook-test.en.ipynb

Expected:

- The notice says the Jupyter Notebook was converted, and the file name becomes notebook-test.en.md.
- The heading and Markdown cells show as they are, with the "Sample image" pasted into a cell.
- Code is in ```python blocks with outputs below: "run 0" to "run 2" (an output block), the name and qty table, the sample image, and ZeroDivisionError (plain text without color codes).
- The raw cell's text is a code block without a language.
- Saving with Save As as Jupyter Notebook gives a notebook with only Markdown cells and the four code cells; outputs are not included.

## 3. Save As

Open [diagram-test.en.md](diagram-test.en.md) or text-test.en.txt and choose each file type in Save As. Save somewhere other than this folder (such as the desktop).

| Type | Expected |
| --- | --- |
| Markdown | The document itself is saved as .md; Save then writes to that file |
| Text, as written | The Markdown is saved unchanged as .txt |
| Text, without marks | Heading and emphasis marks are gone; lists use "•", tables are tab-separated, links become "text (address)", diagrams become "[Figure]". The file being edited does not change |
| PDF | The rendered preview as PDF |
| HTML | One self-contained HTML file that looks the same |
| PNG | The preview as pictures; a long document becomes name-1.png, name-2.png, and so on |
| Word | A .docx with Word's own styles; diagrams and math are pictures |
| CSV | The tables in the document, comma-separated; two or more tables become name-1.csv and so on. Excel shows them without garbled text |
| TSV | The tables in the document, tab-separated |
| Jupyter Notebook | Code blocks with a language become code cells and the rest Markdown cells; outputs are not included |
| JSON | The tables in the document as arrays of objects keyed by the header row. Values stay text (007 keeps its zeros) |

- When the name's extension differs from the type (for example "a.md" with the type Word), the file is saved with the type's extension (a.docx).
- Choosing CSV for a document with no tables shows "This document has no tables".
