<!--
  ファイル名 / File: import-test.zh-CN.md
  概要 / Description: Steps and expected results for each Open and Save As format (Simplified Chinese)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-21
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 打开和导出测试

这些文件用来检查 mdEditor 能打开的所有格式和“另存为”中的所有类型。请用“打开”或拖放打开每个文件，并与预期结果对照。没有专用的中文测试文件，因此使用英文版的文件（文件名中带 `.en`）。日语版的步骤见 [import-test.md](import-test.md)，英语版见 [import-test.en.md](import-test.en.md)。

> 关于外部图像的结果，以“设置”→“互联网”为“确认后再加载”（默认）为前提。界面语言为中文时，通知栏和对话框的文字为中文。

## 1. 文件

| 文件 | 格式 | 检查内容 |
| --- | --- | --- |
| [text-test.en.txt](text-test.en.txt) | 文本 | 按原样打开和保存、换行、去掉标记的导出 |
| [html-test.en.html](html-test.en.html) | HTML | 转换为 Markdown、被移除的部分、图像 |
| [word-test.en.docx](word-test.en.docx) | Word | 转换为 Markdown、取出图像 |
| [word-headings-test.en.docx](word-headings-test.en.docx) | Word（没有标题样式） | 根据文字大小推测标题 |
| [pdf-test.en.pdf](pdf-test.en.pdf) | PDF | 转换为 Markdown、推测标题、取出图像 |
| [pdf-toc-test.en.pdf](pdf-toc-test.en.pdf) | PDF（目录、带编号的标题） | 目录的行、字体不同的带编号标题、分开的“•” |
| [mail-test.en.eml](mail-test.en.eml) | 电子邮件（HTML） | 主题等项目、邮件中的图片、附件、外部图像 |
| [mail-text-test.en.eml](mail-text-test.en.eml) | 电子邮件（仅文本） | 换行 |
| [table-test.en.csv](table-test.en.csv) | CSV（UTF-8） | 转换为表格、含有特殊字符的单元格 |
| [table-test.en.tsv](table-test.en.tsv) | TSV（UTF-8） | 转换为表格 |
| [json-list-test.en.json](json-list-test.en.json) | JSON（列表） | 转换为表格、嵌套的值 |
| [json-config-test.en.json](json-config-test.en.json) | JSON（设置等） | 代码块、注释和末尾的逗号 |
| [xml-test.en.xml](xml-test.en.xml) | XML | 代码块、含有“```”的值 |
| [notebook-test.en.ipynb](notebook-test.en.ipynb) | Jupyter Notebook | 单元格、运行结果（文本、表格、图片、错误）、单元格中的图片、保存回去 |
| [import-test_files/sample.png](import-test_files/sample.png) | 图像 | 打开图像、插入到编辑区；HTML 文件也会用到 |

## 2. 打开

### 2.1 text-test.en.txt

1. 打开 text-test.en.txt。
2. 文件名仍是 text-test.en.txt，不显示转换的通知。

预览中的预期显示：

- “A single line break is joined…”接在上一行后面，成为同一段。
- “ends with two spaces”和“A backslash”之后的行会换行。
- 只有“A heading”是标题；“#Not a heading”保持为普通文字。
- 列表、复选框、粗体、链接、引用、表格、代码块、公式和脚注都会显示。

最后输入一个字符并按“保存”。不会询问保存位置，text-test.en.txt 会被覆盖（检查后请撤销）。

### 2.2 html-test.en.html

预期结果：

- 通知栏说明已将 HTML 转换为 Markdown，文件名变为 html-test.en.md。
- 标题、粗体、斜体、链接、列表（含嵌套）、表格、公式（`$E = mc^2$` 和 `$$…$$`）、引用和代码都变为 Markdown。
- 红色文字的颜色会去掉。
- 显示“Sample image”（文件旁边的图像）。
- 网络上的图像显示为“外部图像（github.githubassets.com），未加载”的框，通知栏中出现“加载图像和视频”。按下后显示图像。
- 只保留按钮的文字；iframe、object、embed 和脚本的文字都不会留下。
- 不会写入 html-test.en.html 本身（保存时会要求保存为 .md 文件）。

### 2.3 word-test.en.docx

预期结果：

- 通知栏说明已转换 Word 文档，文件名变为 word-test.en.md。
- 标题 1 到 3、粗体、斜体、链接、两种列表、表格和图像都变为 Markdown。
- 预览中显示图像，编辑区中指向 `word-test.en_files/image1.png`。
- 用“另存为”保存为 Markdown 时，会在文件旁边创建放有图像的 word-test.en_files 文件夹。

### 2.4 mail-test.en.eml

预期结果：

- 通知栏说明已转换电子邮件，文件名变为 mail-test.en.md。
- 第一行是作为标题的主题“[Test] Opening e-mail in mdEditor”。
- 列出发件人（Sam Sender <sato@example.com>）、收件人、抄送、日期（日本时间 09:30，按这台电脑的时区显示）和附件（note.txt）。界面语言为中文时，项目名为“发件人”“收件人”“抄送”“日期”“附件”。
- 正文中的粗体、链接、列表和表格变为 Markdown。
- 邮件中的图片（cid）会显示，并指向 `mail-test.en_files/image1.png`。
- 网络上的图像（例如 1 像素的跟踪图像）不会加载，通知栏中出现“加载图像和视频”。
- 不会留下脚本，也不会取出附件。

### 2.5 mail-text-test.en.eml

预期结果：

- 正文的三行分别显示在不同的行。
- 以“>”开头的行变为引用。

### 2.6 table-test.en.csv

预期结果：

- 通知栏说明已将表格文件转换为 Markdown 表格打开。
- 第一行（Name、Team、Extension、Note）是表头。
- “A cell with a comma, here”、“A cell with a / line break”（用 `<br>` 换行）、“A cell with "quotes"”和“A cell with a | bar”各自保持在一个单元格中。

### 2.7 table-test.en.tsv

预期结果：

- 表头和以制表符分隔的三行变为表格；“Commas, stay as they are”保持在一个单元格中。
- 最后一行的空单元格保持为空。

### 2.8 json-list-test.en.json

预期结果：

- 通知栏说明已将 JSON 列表转换为 Markdown 表格打开。
- 字段名（id、name、team、extension、active、note、licenses、address）作为表头行。缺少的字段对应的单元格为空。
- “licenses”显示为“Architect, Surveyor”，“address”显示为 `{"city":"Yokohama","ward":"Nishi"}`，各自在一个单元格中。

### 2.9 json-config-test.en.json

预期结果：

- 通知栏说明已将 JSON 放入代码块中打开。
- 注释和末尾的逗号不影响打开；JSON 在代码块中重新缩进（注释会去掉）。

### 2.10 xml-test.en.xml

预期结果：

- 通知栏说明已将 XML 放入代码块中打开。
- 内容按原样放入代码块。值中的“```”不会提前结束代码块（代码块使用四个反引号）。

### 2.11 import-test_files/sample.png（打开图像）

预期结果：

- 通知栏说明已打开图像，文件名变为 sample.md。
- 编辑区显示 `![sample](sample.png)` 和一个空行，预览中显示图像。
- 在下方写上说明，用“另存为”保存到其他位置（如桌面）时，sample.png 也会复制过去。

### 2.12 在编辑区中插入图像

1. 打开一个已保存的 Markdown 文件（新建并保存的文件也可以）。
2. 把 sample.png 拖到编辑区。
3. 截取屏幕（Windows 键 + Shift + S），在编辑区中按 Ctrl+V 粘贴。

预期结果：

- 在拖放的位置插入 `![sample](名称_files/sample.png)`，图像复制到文档旁边的“名称_files”文件夹中。
- 在粘贴的位置插入 `![](名称_files/image-日期-时间.png)`。
- 尚未保存的新文档会先询问保存位置。
- 拖放到编辑区以外（如工具栏）的图像，会像 2.11 一样作为图像文档打开。

### 2.13 notebook-test.en.ipynb

预期结果：

- 通知栏说明已转换 Jupyter Notebook，文件名变为 notebook-test.en.md。
- 标题和 Markdown 单元格按原样显示，单元格中粘贴的“Sample image”也会显示。
- 代码在 ```python 代码块中，下面是运行结果：“run 0”到“run 2”（output 代码块）、name 和 qty 的表格、示例图像，以及 ZeroDivisionError（不带颜色代码的纯文本）。
- raw 单元格的文字成为没有语言名的代码块。
- 用“另存为”保存为 Jupyter Notebook 时，得到只有 Markdown 单元格和四个代码单元格的笔记本；不包含运行结果。

### 2.14 word-headings-test.en.docx（没有标题样式的 Word）

预期结果：

- 通知栏在“已将 Word 文档转换为 Markdown 打开”之后，显示“由于文档没有标题样式，已根据文字大小把 3 个段落设为标题。”。
- “A Word document without heading styles”（20 磅粗体）成为 `#` 标题，“A large bold heading”（14 磅粗体）成为 `##`，“A large heading, not bold”（14 磅）成为 `###`。
- “Bold at the body size”不会成为标题，保持为粗体段落（`**…**`）。
- 使用了标题样式的 word-test.en.docx 不会显示这条通知（与 2.3 的结果相同）。

### 2.15 pdf-test.en.pdf

预期结果：

- 通知栏显示“已将 PDF 转换为 Markdown 打开”，文件名变为 pdf-test.en.md。
- “Opening a PDF”成为 `#` 标题，“Headings and paragraphs”“Image”“Table”“Code”“Bullets”“To do”“Figure”成为 `##`，“A smaller heading”成为 `###`。
- 以“A long paragraph”开头的段落，无论在 PDF 中怎样折行，都会合成一个段落，句号前没有空格。
- 示例图像显示在预览中，编辑区中指向 `pdf-test.en_files/image1.png`。
- 表格恢复为 Markdown 表格（`| Item | Cost |`）。
- “Code”下的 Python 成为用 ``` 围起来的代码块，缩进也会保留。
- “Bullets”成为 `- The first item`，其下为缩进的 `  - A nested item`。“To do”成为 `- [x] A finished task` 和 `- [ ] A task still to do`。
- “Figure”下的流程图被重新绘制为 SVG 图片（`<p align="center"><img src="pdf-test.en_files/image….svg" …></p>`），预览中可以看到与 PDF 相同布局的方框、箭头和“Write”“Save”“Share”文字。图中的文字不会出现在正文中。
- “Math”中句子里的 $E = mc^2$ 成为原位置的图片（`![E=mc2](pdf-test.en_files/image….svg)`），单独一行的分数成为居中的图片。
- “Image size and position”成为 `<p align="center">…width="50%"…</p>`，以及 `width="25%" align="left"` 的图片、其后环绕的段落和 `<br clear="all">`。
- “Video”恢复为单独一行的地址（`https://www.w3schools.com/html/mov_bbb.mp4`）；PDF 中是“未加载”的注记。
- 把 pdf-test.en.pdf 拖放到编辑区时，插入的是复制到“名称_files”文件夹中的 PDF 的链接，而不是其内容。

### 2.16 pdf-toc-test.en.pdf（目录和带编号的标题）

仿照官方指南制作的 PDF：正文为 Times，带编号的标题为同样大小的 Helvetica。

预期结果：

- 目录按 PDF 的缩进成为嵌套的列表（`- 1\. Introduction …… 2`，其下为 `  - 2.1 Definitions …… 2`，再下为 `    - (1) First condition …… 3`）。目录的行不会成为标题。
- “1. Introduction”“2. Requirements”成为 `##` 标题，“2.1 Definitions”成为 `###`，“(1) First condition”“(2) Second condition”成为 `####`。
- 封面的标题、日期和“Contents”成为 `#`。“1 April 2026”的数字后面没有点，不会成为带编号的标题。
- “•”与文字分开的行成为列表项 `- A definition item, …`。
- 在同一行上隔着较宽间距的“File Number: E12345”和“Type: A-1”成为一行 `File Number: E12345 Type: A-1`，其下的“Main board”保持为单独的段落。
- 在文件资源管理器中复制此文件，然后按“从剪贴板新建”的按键，会打开此文件。

## 3. 另存为

打开 [diagram-test.zh-CN.md](diagram-test.zh-CN.md) 或 text-test.en.txt，在“另存为”中选择各个文件类型。请保存到此文件夹以外的位置（如桌面）。

| 类型 | 预期结果 |
| --- | --- |
| Markdown | 将文档本身保存为 .md；之后“保存”会写入该文件 |
| 文本（原样） | 将 Markdown 按原样保存为 .txt |
| 文本（去掉标记） | 去掉标题和强调的标记；界面语言为中文时，列表用“•”，表格以制表符分隔，链接变为“文字（地址）”，图表变为“［图］”。正在编辑的文件不变 |
| PDF | 将排版后的预览保存为 PDF |
| HTML | 外观相同的单个 HTML 文件 |
| PNG | 将预览保存为图片；长文档分为 名称-1.png、名称-2.png 等 |
| Word | 使用 Word 自带样式的 .docx；图表和公式为图片 |
| CSV | 文档中的表格，以逗号分隔；有两个以上表格时分为 名称-1.csv 等。在 Excel 中不会乱码 |
| TSV | 文档中的表格，以制表符分隔 |
| Jupyter Notebook | 带语言名的代码块成为代码单元格，其余成为 Markdown 单元格；不包含运行结果 |
| JSON | 将文档中的表格保存为以表头行为键的对象数组。值保持为文字（007 的 0 不会消失） |
| 用于分发的 ZIP | 文档和“名称_files”文件夹打包为一个 ZIP；文档没有引用的文件不放入 |

- 文件名的扩展名与类型不同时（例如名称为“a.md”而类型为 Word），会以类型的扩展名保存（a.docx）。
- 对没有表格的文档选择 CSV 时，会显示“此文档中没有表格”。
