<!--
File: samples/quickstart-engineering-note.en.md
Description: mdEditor promotion material
Author: 5garashi.com Design Office
Created: 2026-10-12
Version: 1.0.5
License: MIT
SPDX-License-Identifier: MIT
-->
# Technical note with diagrams and math

Open in mdEditor, edit, then press Ctrl+Shift+P to save as PDF.

## Power dissipated by a resistor

Example: apply $V=10\,\mathrm{V}$ to $R=100\,\Omega$.

$$P=\frac{V^2}{R}=\frac{10^2}{100}=1\,\mathrm{W}$$

| Voltage V | Resistance Ω | Power W |
| --- | --- | --- |
| 5 | 100 | 0.25 |
| 10 | 100 | 1.00 |

Check temperature and derating when selecting a real component.

```mermaid
flowchart LR
 A[Define conditions] --> B[Calculate]
 B --> C[Check specifications]
 C --> D[Verify by measurement]
```
