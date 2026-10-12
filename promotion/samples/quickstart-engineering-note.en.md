<!--
File: promotion/samples/quickstart-engineering-note.en.md
Description: mdEditor promotion material
Author: 5garashi.com Design Office
Created: 2026-10-12
Version: 1.1.0
License: MIT
Last updated: 2026-10-13
SPDX-License-Identifier: MIT
-->
# Diagrams and math

Example: resistance $R=100\,\Omega$ and voltage $V=10\,\mathrm{V}$.

$$P=\frac{V^2}{R}=\frac{10^2}{100}=1\,\mathrm{W}$$

```mermaid
flowchart LR
 A[Set conditions] --> B[Calculate] --> C[Check specifications]
 classDef default fill:#FFFDF2,stroke:#385723,color:#385723
 linkStyle default stroke:#385723
```

| Voltage | Resistance | Power |
| --- | --- | --- |
| 5 V | 100 Ω | 0.25 W |
| 10 V | 100 Ω | 1.00 W |

## Try it

1. Open a copy of this document in mdEditor.
2. Change the first heading to “Diagrams and math: reviewed”. Check the preview.
3. Change “Check specifications” in the diagram to “Verify by measurement”. The diagram shows the new label.
4. Select “Save” to save the Markdown, then “Save as PDF” to export a PDF.
5. Open the PDF and check the heading, diagram, equation and table.

Recalculate and update the equation and table yourself when changing the values; mdEditor does not calculate them. Actual component selection also requires checking temperature and derating.
