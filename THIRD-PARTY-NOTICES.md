<!--
  ファイル名 / File: THIRD-PARTY-NOTICES.md
  概要 / Description: Licenses of third-party software distributed with mdEditor
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-11
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# Third-party notices / 第三者ソフトウェアのライセンス

mdEditor itself is licensed under the [MIT License](./LICENSE). It includes the third-party software listed below, each under its own license. The full license texts follow the lists.

mdEditor 本体は [MIT License](./LICENSE) です。以下の第三者ソフトウェアを、それぞれのライセンスのもとで同梱しています。ライセンス本文は一覧の後にあります。

The 5garashi.com logotype and symbol mark in `wwwroot/assets/` are not covered by these licenses or by MIT; they are provided under CC BY-ND 4.0 (see each SVG file).

## Components / 同梱しているソフトウェア

| Component | Version | License | Copyright | Source |
| :--- | :--- | :--- | :--- | :--- |
| .NET Runtime (self-contained) | 10.0 | MIT | Copyright (c) .NET Foundation and Contributors | https://github.com/dotnet/runtime |
| Markdig | 1.4.0 | BSD-2-Clause | Copyright (c) 2016-2026, Alexandre Mutel | https://github.com/xoofx/markdig |
| HtmlSanitizer | 9.2.1039 | MIT | Copyright (c) 2013-2016 Michael Ganss and HtmlSanitizer contributors | https://github.com/mganss/HtmlSanitizer |
| AngleSharp | 1.7.2 | MIT | Copyright (c) 2013 - 2026 AngleSharp | https://github.com/AngleSharp/AngleSharp |
| AngleSharp.Css | 1.0.2 | MIT | Copyright (c) 2013 - 2026 AngleSharp | https://github.com/AngleSharp/AngleSharp.Css |
| ReverseMarkdown | 6.2.1 | MIT | Copyright (c) Babu Annamalai | https://github.com/mysticmind/reversemarkdown-net |
| MimeKitLite | 4.18.1 | MIT | Copyright (c) .NET Foundation and Contributors | https://github.com/jstedfast/MimeKit |
| YamlDotNet | 18.1.0 | MIT | Copyright (c) Antoine Aubry and contributors | https://github.com/aaubry/YamlDotNet |
| Mammoth (.NET) | 1.13.0 | BSD-2-Clause | Copyright 2015 - 2016 Michael Williamson | https://github.com/mwilliamson/dotnet-mammoth |
| DocumentFormat.OpenXml (Open XML SDK) | 3.5.1 | MIT | Copyright (c) Microsoft Corporation | https://github.com/dotnet/Open-XML-SDK |
| PdfPig (opening PDFs; includes the Adobe Glyph List and Zapf Dingbats Glyph List) | 0.1.16 | Apache-2.0 (glyph lists: BSD-3-Clause) | Copyright (c) 2002-2007, www.pdfbox.org; Adobe Systems Incorporated (glyph lists) | https://github.com/UglyToad/PdfPig |
| Microsoft.Web.WebView2 (SDK) | 1.0.4258.31 | BSD-3-Clause (Microsoft) | Copyright (C) Microsoft Corporation | https://www.nuget.org/packages/Microsoft.Web.WebView2 |
| Windows SDK projection for .NET (Microsoft.Windows.SDK.NET.dll and WinRT.Runtime.dll from the Microsoft.Windows.SDK.NET.Ref package; drawing PDF pages with Windows' own renderer) | 10.0.19041.55 (WinRT.Runtime 2.2.0) | Microsoft Software License Terms, Windows SDK for Windows 10 (Distributable Code, REDIST list) | Copyright (c) Microsoft Corporation | https://www.nuget.org/packages/Microsoft.Windows.SDK.NET.Ref |
| Mermaid | 11.17.2 | MIT | Copyright (c) 2014 - 2022 Knut Sveidqvist | https://github.com/mermaid-js/mermaid |
| KaTeX (including fonts) | 0.18.9 | MIT | Copyright (c) 2013-2020 Khan Academy and other contributors | https://github.com/KaTeX/KaTeX |
| highlight.js (with the PowerShell and Dockerfile grammars) | 11.12.0 | BSD-3-Clause | Copyright (c) 2006, Ivan Sagalaev | https://highlightjs.org/ |
| Viz.js (WebAssembly build of Graphviz) | 3.31.0 | MIT | Copyright (c) Michael Daines | https://github.com/mdaines/viz-js |
| Graphviz (inside Viz.js, object code) | 16.1.0 | EPL-1.0 | Copyright (c) AT&T and the Graphviz contributors | https://gitlab.com/graphviz/graphviz |
| Expat (inside Viz.js, object code) | — | MIT | Copyright (c) 1998-2000 Thai Open Source Software Center Ltd and Clark Cooper; Copyright (c) 2001-2025 Expat maintainers | https://libexpat.github.io |
| three.js (core, STLLoader, OrbitControls) | 0.186.1 | MIT | Copyright © 2010-2026 three.js authors | https://threejs.org/ |
| d3-geo | 3.1.1 | ISC | Copyright 2010-2024 Mike Bostock | https://github.com/d3/d3-geo |
| topojson-client | 3.1.0 | ISC | Copyright 2012-2019 Michael Bostock | https://github.com/topojson/topojson-client |
| world-atlas (countries-110m, from Natural Earth, public domain) | 2.0.2 | ISC | Copyright 2013-2019 Michael Bostock | https://github.com/topojson/world-atlas |
| CodeMirror 6 (view 6.43.12, state 6.7.6, commands 6.11.1) | 6 | MIT | Copyright (C) 2018-2021 by Marijn Haverbeke and others | https://codemirror.net/ |

Microsoft Edge WebView2 Runtime is not distributed with mdEditor; it is provided by Windows or installed separately under Microsoft's terms. The .NET runtime's own third-party notices are at https://github.com/dotnet/runtime/blob/main/THIRD-PARTY-NOTICES.TXT.

## Libraries bundled inside the CodeMirror build / CodeMirror に含まれるライブラリ

`wwwroot/lib/codemirror/codemirror.min.js` is built from unmodified npm packages with `tools/codemirror` (esbuild). It contains:

| Library | Version | License | Copyright |
| :--- | :--- | :--- | :--- |
| @codemirror/view | 6.43.12 | MIT | Copyright (C) 2018-2021 by Marijn Haverbeke and others |
| @codemirror/state | 6.7.6 | MIT | Copyright (C) 2018-2021 by Marijn Haverbeke and others |
| @codemirror/commands | 6.11.1 | MIT | Copyright (C) 2018-2021 by Marijn Haverbeke and others |
| @codemirror/language | 6.13.1 | MIT | Copyright (C) 2018-2021 by Marijn Haverbeke and others |
| @lezer/common | 1.5.3 | MIT | Copyright (C) 2018 by Marijn Haverbeke and others |
| @lezer/highlight | 1.2.5 | MIT | Copyright (C) 2018 by Marijn Haverbeke and others |
| @lezer/lr | 1.4.11 | MIT | Copyright (C) 2018 by Marijn Haverbeke and others |
| @marijn/find-cluster-break | 1.0.4 | MIT | Copyright (C) 2024 by Marijn Haverbeke |
| crelt | 1.0.7 | MIT | Copyright (C) 2020 by Marijn Haverbeke |
| style-mod | 4.1.4 | MIT | Copyright (C) 2018 by Marijn Haverbeke and others |
| w3c-keyname | 2.2.8 | MIT | Copyright (C) 2016 by Marijn Haverbeke and others |

## Libraries bundled inside Mermaid / Mermaid に含まれるライブラリ

`wwwroot/lib/mermaid/mermaid.min.js` is the unmodified Mermaid 11.17.2 browser bundle. It contains the following direct dependencies of Mermaid (versions as resolved by Mermaid's build) and their own dependencies:

| Library | License | Copyright |
| :--- | :--- | :--- |
| @braintree/sanitize-url | MIT | Copyright (c) 2017 Braintree |
| @iconify/utils | MIT | Copyright (c) 2021-PRESENT Vjacheslav Trushkin |
| @mermaid-js/parser | MIT | Copyright (c) 2023 Yokozuna59 |
| @upsetjs/venn.js | MIT | Copyright (c) 2013 Ben Frederickson |
| chevrotain | Apache-2.0 | Shahar Soel |
| cytoscape | MIT | Copyright (c) 2016-2026, The Cytoscape Consortium |
| cytoscape-cose-bilkent | MIT | Copyright (c) 2016-2018, The Cytoscape Consortium |
| cytoscape-fcose | MIT | Copyright (c) 2018 - present, iVis-at-Bilkent |
| d3 | ISC | Copyright 2010-2023 Mike Bostock |
| d3-sankey | BSD-3-Clause | Copyright 2015, Mike Bostock |
| dagre-d3-es | MIT | Copyright (c) 2013 Chris Pettitt (original dagre-d3) |
| dayjs | MIT | Copyright (c) 2018-present, iamkun |
| DOMPurify | Apache-2.0 (dual-licensed with MPL-2.0; used under Apache-2.0) | Cure53 and other contributors |
| es-toolkit | MIT | Copyright (c) 2024 Viva Republica, Inc. |
| fastdom | MIT | Wilson Page |
| khroma | MIT | Copyright (c) 2019-present Fabio Spampinato, Andrew Maney |
| langium | MIT | Copyright 2021 TypeFox GmbH |
| lodash-es | MIT | Copyright OpenJS Foundation and other contributors |
| marked | MIT | Copyright (c) 2018+, MarkedJS |
| roughjs | MIT | Copyright (c) 2019 Preet Shihn |
| stylis | MIT | Copyright (c) 2016-present Sultan Tarimo |
| ts-dedent | MIT | Copyright (c) 2018 Tamino Martinius |
| uuid | MIT | Copyright (c) 2010-2020 Robert Kieffer and other contributors |

---

## MIT License

Applies to: .NET Runtime, HtmlSanitizer, AngleSharp, AngleSharp.Css, ReverseMarkdown, MimeKitLite, YamlDotNet, three.js, Viz.js, Expat, DocumentFormat.OpenXml, Mermaid, KaTeX, CodeMirror, and the MIT-licensed libraries above. Each component's copyright notice is listed in the tables above.

```text
Copyright (c) <year> <copyright holders>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## BSD 2-Clause License (Markdig, Mammoth)

The same license text applies to Mammoth (.NET), Copyright 2015 - 2016 Michael Williamson. / 同じライセンス本文が Mammoth（.NET、Copyright 2015 - 2016 Michael Williamson）にも適用されます。

```text
Copyright (c) 2016-2026, Alexandre Mutel
All rights reserved.

Redistribution and use in source and binary forms, with or without modification
, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Microsoft Windows SDK for Windows 10 (Microsoft.Windows.SDK.NET.Ref)

Microsoft.Windows.SDK.NET.dll and WinRT.Runtime.dll let mdEditor call Windows' own PDF renderer (Windows.Data.Pdf), which draws a page whose letters cannot be read as a picture. Both come from the Microsoft.Windows.SDK.NET.Ref NuGet package, whose files the Windows SDK REDIST list (https://learn.microsoft.com/legal/windows-sdk/redist) allows to be distributed unmodified as part of a program so that it can call WinRT APIs. They are Microsoft's Distributable Code under the Microsoft Software License Terms for the Microsoft Windows Software Development Kit (SDK) for Windows 10 (https://aka.ms/WinSDKLicenseURL). They are not covered by mdEditor's MIT License: they stay under those terms, their copyright notices may not be altered, and they may be used only as part of mdEditor on Windows. (The source of WinRT.Runtime, the C#/WinRT project, is published under the MIT License at https://github.com/microsoft/CsWinRT.)

Microsoft.Windows.SDK.NET.dll と WinRT.Runtime.dll は、mdEditor から Windows の PDF 描画（Windows.Data.Pdf）を使うためのファイルで、文字を読み取れないページを画像にするときに使います。どちらも NuGet パッケージ Microsoft.Windows.SDK.NET.Ref のファイルで、Windows SDK の REDIST リスト（https://learn.microsoft.com/legal/windows-sdk/redist）により、WinRT API を呼ぶためにプログラムの一部として変更せずに配布できます。Microsoft の「再頒布可能コード」として、Microsoft Windows SDK for Windows 10 のライセンス条項（https://aka.ms/WinSDKLicenseURL）のもとで同梱しています。mdEditor の MIT License の対象ではなく、これらの条項に従います（著作権表示の変更は不可、Windows 上の mdEditor の一部としてのみ使用）。なお、WinRT.Runtime のソース（C#/WinRT）は MIT License で公開されています（https://github.com/microsoft/CsWinRT）。

## Microsoft.Web.WebView2 SDK License

```text
Copyright (C) Microsoft Corporation. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

   * Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above
copyright notice, this list of conditions and the following disclaimer
in the documentation and/or other materials provided with the
distribution.
   * The name of Microsoft Corporation, or the names of its contributors 
may not be used to endorse or promote products derived from this
software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Viz.js, Graphviz, and Expat (wwwroot/lib/graphviz)

`wwwroot/lib/graphviz/viz-global.js` is the unmodified browser build of Viz.js 3.31.0, which contains Graphviz 16.1.0 and Expat in object code form. Graphviz is distributed under the Eclipse Public License 1.0; its source code is available from https://gitlab.com/graphviz/graphviz. The full texts (MIT for Viz.js and Expat, EPL 1.0 for Graphviz) are in `wwwroot/lib/graphviz/LICENSE`, which is shipped with the app. / Graphviz の図を描く Viz.js 3.31.0（Graphviz 16.1.0 と Expat をオブジェクトコードで含む）を改変せずに同梱しています。Graphviz は Eclipse Public License 1.0 で、ソースコードは https://gitlab.com/graphviz/graphviz から入手できます。ライセンスの全文は、アプリと一緒に配る `wwwroot/lib/graphviz/LICENSE` にあります。

## BSD 3-Clause License (highlight.js)

`wwwroot/lib/highlight/` holds the unmodified browser build of highlight.js 11.12.0 (`@highlightjs/cdn-assets`) and its PowerShell and Dockerfile grammars. / コードの色分けに使う highlight.js 11.12.0 のブラウザー用ビルドと、PowerShell・Dockerfile の文法を改変せずに同梱しています。

```text
BSD 3-Clause License

Copyright (c) 2006, Ivan Sagalaev.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of the copyright holder nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## BSD 3-Clause License (d3-sankey)

```text
Copyright 2015, Mike Bostock
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors
   may be used to endorse or promote products derived from this software
   without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## ISC License (d3-geo)

```text
Copyright 2010-2024 Mike Bostock

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.

This license applies to GeographicLib, versions 1.12 and later.

Copyright 2008-2012 Charles Karney

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

## ISC License (topojson-client)

```text
Copyright 2012-2019 Michael Bostock

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
```

## ISC License (world-atlas)

```text
Copyright 2013-2019 Michael Bostock

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
```

## OpenStreetMap map tiles (only when chosen in Settings)

When detailed maps are loaded (Load detailed maps in the notice bar, or Settings > Internet > Detailed maps (OpenStreetMap) > Always load), map images come from tile.openstreetmap.org and are shown with the credit "© OpenStreetMap contributors" (map data under the Open Database License, https://www.openstreetmap.org/copyright). They are not bundled with the app and not kept in saved HTML. / 詳細な地図を読み込んだとき（お知らせ欄の「詳細な地図を読み込む」、または「いつも読み込む」の設定）だけ、地図の画像を tile.openstreetmap.org から読み込み、「© OpenStreetMap contributors」の表示を付けます（地図のデータは Open Database License）。アプリには含めず、保存した HTML にも入れません。

## ISC License (d3)

```text
Copyright 2010-2023 Mike Bostock

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
```

## PdfPig NOTICES

The NOTICES.txt of PdfPig (https://github.com/UglyToad/PdfPig), reproduced as it is:

```text
This product is derived from software developed at
The Apache Software Foundation (http://www.apache.org/).

Based on source code originally developed in the PDFBox and
FontBox projects.

Copyright (c) 2002-2007, www.pdfbox.org

Includes the Adobe Glyph List
Copyright 1997, 1998, 2002, 2007, 2010 Adobe Systems Incorporated.

Includes the Zapf Dingbats Glyph List
Copyright 2002, 2010 Adobe Systems Incorporated.
```

## BSD 3-Clause License (Adobe Glyph List and Zapf Dingbats Glyph List, inside PdfPig)

Copyright 1997, 1998, 2002, 2007, 2010 Adobe Systems Incorporated (Adobe Glyph List); Copyright 2002, 2010 Adobe Systems Incorporated (Zapf Dingbats Glyph List). License text from https://github.com/adobe-type-tools/agl-aglfn:

```text
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

Neither the name of Adobe nor the names of its contributors may be
used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Apache License 2.0 (DOMPurify, chevrotain, PdfPig)

```text
Apache License
                           Version 2.0, January 2004
                        http://www.apache.org/licenses/

   TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

   1. Definitions.

      "License" shall mean the terms and conditions for use, reproduction,
      and distribution as defined by Sections 1 through 9 of this document.

      "Licensor" shall mean the copyright owner or entity authorized by
      the copyright owner that is granting the License.

      "Legal Entity" shall mean the union of the acting entity and all
      other entities that control, are controlled by, or are under common
      control with that entity. For the purposes of this definition,
      "control" means (i) the power, direct or indirect, to cause the
      direction or management of such entity, whether by contract or
      otherwise, or (ii) ownership of fifty percent (50%) or more of the
      outstanding shares, or (iii) beneficial ownership of such entity.

      "You" (or "Your") shall mean an individual or Legal Entity
      exercising permissions granted by this License.

      "Source" form shall mean the preferred form for making modifications,
      including but not limited to software source code, documentation
      source, and configuration files.

      "Object" form shall mean any form resulting from mechanical
      transformation or translation of a Source form, including but
      not limited to compiled object code, generated documentation,
      and conversions to other media types.

      "Work" shall mean the work of authorship, whether in Source or
      Object form, made available under the License, as indicated by a
      copyright notice that is included in or attached to the work
      (an example is provided in the Appendix below).

      "Derivative Works" shall mean any work, whether in Source or Object
      form, that is based on (or derived from) the Work and for which the
      editorial revisions, annotations, elaborations, or other modifications
      represent, as a whole, an original work of authorship. For the purposes
      of this License, Derivative Works shall not include works that remain
      separable from, or merely link (or bind by name) to the interfaces of,
      the Work and Derivative Works thereof.

      "Contribution" shall mean any work of authorship, including
      the original version of the Work and any modifications or additions
      to that Work or Derivative Works thereof, that is intentionally
      submitted to Licensor for inclusion in the Work by the copyright owner
      or by an individual or Legal Entity authorized to submit on behalf of
      the copyright owner. For the purposes of this definition, "submitted"
      means any form of electronic, verbal, or written communication sent
      to the Licensor or its representatives, including but not limited to
      communication on electronic mailing lists, source code control systems,
      and issue tracking systems that are managed by, or on behalf of, the
      Licensor for the purpose of discussing and improving the Work, but
      excluding communication that is conspicuously marked or otherwise
      designated in writing by the copyright owner as "Not a Contribution."

      "Contributor" shall mean Licensor and any individual or Legal Entity
      on behalf of whom a Contribution has been received by Licensor and
      subsequently incorporated within the Work.

   2. Grant of Copyright License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      copyright license to reproduce, prepare Derivative Works of,
      publicly display, publicly perform, sublicense, and distribute the
      Work and such Derivative Works in Source or Object form.

   3. Grant of Patent License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      (except as stated in this section) patent license to make, have made,
      use, offer to sell, sell, import, and otherwise transfer the Work,
      where such license applies only to those patent claims licensable
      by such Contributor that are necessarily infringed by their
      Contribution(s) alone or by combination of their Contribution(s)
      with the Work to which such Contribution(s) was submitted. If You
      institute patent litigation against any entity (including a
      cross-claim or counterclaim in a lawsuit) alleging that the Work
      or a Contribution incorporated within the Work constitutes direct
      or contributory patent infringement, then any patent licenses
      granted to You under this License for that Work shall terminate
      as of the date such litigation is filed.

   4. Redistribution. You may reproduce and distribute copies of the
      Work or Derivative Works thereof in any medium, with or without
      modifications, and in Source or Object form, provided that You
      meet the following conditions:

      (a) You must give any other recipients of the Work or
          Derivative Works a copy of this License; and

      (b) You must cause any modified files to carry prominent notices
          stating that You changed the files; and

      (c) You must retain, in the Source form of any Derivative Works
          that You distribute, all copyright, patent, trademark, and
          attribution notices from the Source form of the Work,
          excluding those notices that do not pertain to any part of
          the Derivative Works; and

      (d) If the Work includes a "NOTICE" text file as part of its
          distribution, then any Derivative Works that You distribute must
          include a readable copy of the attribution notices contained
          within such NOTICE file, excluding those notices that do not
          pertain to any part of the Derivative Works, in at least one
          of the following places: within a NOTICE text file distributed
          as part of the Derivative Works; within the Source form or
          documentation, if provided along with the Derivative Works; or,
          within a display generated by the Derivative Works, if and
          wherever such third-party notices normally appear. The contents
          of the NOTICE file are for informational purposes only and
          do not modify the License. You may add Your own attribution
          notices within Derivative Works that You distribute, alongside
          or as an addendum to the NOTICE text from the Work, provided
          that such additional attribution notices cannot be construed
          as modifying the License.

      You may add Your own copyright statement to Your modifications and
      may provide additional or different license terms and conditions
      for use, reproduction, or distribution of Your modifications, or
      for any such Derivative Works as a whole, provided Your use,
      reproduction, and distribution of the Work otherwise complies with
      the conditions stated in this License.

   5. Submission of Contributions. Unless You explicitly state otherwise,
      any Contribution intentionally submitted for inclusion in the Work
      by You to the Licensor shall be under the terms and conditions of
      this License, without any additional terms or conditions.
      Notwithstanding the above, nothing herein shall supersede or modify
      the terms of any separate license agreement you may have executed
      with Licensor regarding such Contributions.

   6. Trademarks. This License does not grant permission to use the trade
      names, trademarks, service marks, or product names of the Licensor,
      except as required for reasonable and customary use in describing the
      origin of the Work and reproducing the content of the NOTICE file.

   7. Disclaimer of Warranty. Unless required by applicable law or
      agreed to in writing, Licensor provides the Work (and each
      Contributor provides its Contributions) on an "AS IS" BASIS,
      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
      implied, including, without limitation, any warranties or conditions
      of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
      PARTICULAR PURPOSE. You are solely responsible for determining the
      appropriateness of using or redistributing the Work and assume any
      risks associated with Your exercise of permissions under this License.

   8. Limitation of Liability. In no event and under no legal theory,
      whether in tort (including negligence), contract, or otherwise,
      unless required by applicable law (such as deliberate and grossly
      negligent acts) or agreed to in writing, shall any Contributor be
      liable to You for damages, including any direct, indirect, special,
      incidental, or consequential damages of any character arising as a
      result of this License or out of the use or inability to use the
      Work (including but not limited to damages for loss of goodwill,
      work stoppage, computer failure or malfunction, or any and all
      other commercial damages or losses), even if such Contributor
      has been advised of the possibility of such damages.

   9. Accepting Warranty or Additional Liability. While redistributing
      the Work or Derivative Works thereof, You may choose to offer,
      and charge a fee for, acceptance of support, warranty, indemnity,
      or other liability obligations and/or rights consistent with this
      License. However, in accepting such obligations, You may act only
      on Your own behalf and on Your sole responsibility, not on behalf
      of any other Contributor, and only if You agree to indemnify,
      defend, and hold each Contributor harmless for any liability
      incurred by, or claims asserted against, such Contributor by reason
      of your accepting any such warranty or additional liability.

   END OF TERMS AND CONDITIONS

   APPENDIX: How to apply the Apache License to your work.

      To apply the Apache License to your work, attach the following
      boilerplate notice, with the fields enclosed by brackets "[]"
      replaced with your own identifying information. (Don't include
      the brackets!)  The text should be enclosed in the appropriate
      comment syntax for the file format. We also recommend that a
      file or class name and description of purpose be included on the
      same "printed page" as the copyright notice for easier
      identification within third-party archives.

   Copyright [yyyy] [name of copyright owner]

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
```
