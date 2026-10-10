# Diagram and Chart Rendering Test

This test document checks that diagrams and charts render correctly in the mdEditor preview.
Compare the "Expected" note in each section with the actual preview.

---

## 1. Flowchart (Mermaid flowchart)

Expected: a top-to-bottom flowchart with branches, a diamond decision node, and colored nodes.

```mermaid
flowchart TD
    A([Start]) --> B[Open file]
    B --> C{Extension is .md?}
    C -- Yes --> D[Load into editor]
    C -- No --> E[Show error]
    D --> F[/Update preview/]
    F --> G[(Save)]
    E --> H([End])
    G --> H

    classDef ok fill:#d4f7d4,stroke:#2a7a2a;
    classDef ng fill:#ffd6d6,stroke:#a22;
    class D,F,G ok;
    class E ng;
```

### 1-2. Left to right with subgraphs

```mermaid
flowchart LR
    subgraph UI[Window]
        ed[Editor] --> pv[Preview]
    end
    subgraph Host[WPF host]
        md[Markdig] --> san[HtmlSanitizer]
    end
    ed -- Text --> md
    san -- HTML --> pv
```

---

## 2. Sequence diagram (Mermaid sequenceDiagram)

Expected: four participants, synchronous and asynchronous arrows, loop and alt frames, and a note.

```mermaid
sequenceDiagram
    autonumber
    actor User as User
    participant Page as WebView2 (app.js)
    participant Host as MainWindow
    participant FS as File system

    User->>Page: Type text
    Page-)Host: postMessage(text)
    Note right of Host: Wait briefly, then<br/>update the preview
    loop While typing continues
        Host->>Host: Reset timer
    end
    Host-->>Page: preview HTML
    User->>Page: Save button
    Page->>Host: save
    alt Existing file
        Host->>FS: Overwrite
    else New file
        Host->>User: Save As dialog
        User->>Host: File name
        Host->>FS: Save new file
    end
    FS-->>Host: Done
    Host-->>Page: Saved
```

---

## 3. Class diagram (Mermaid classDiagram)

```mermaid
classDiagram
    class MainWindow {
        -string _text
        -MarkdownPipeline _markdownPipeline
        +LoadFile(path) void
        +Save() bool
        -SendPreview() void
    }
    class Document {
        +string Path
        +bool IsDirty
    }
    class Renderer {
        <<interface>>
        +ToHtml(md) string
    }
    MainWindow "1" *-- "1" Document : edits
    MainWindow ..> Renderer : uses
    Renderer <|.. MarkdigRenderer
```

---

## 4. State diagram (Mermaid stateDiagram)

```mermaid
stateDiagram-v2
    [*] --> Saved
    Saved --> Unsaved : Edit
    Unsaved --> Saving : Ctrl+S
    Saving --> Saved : Success
    Saving --> Unsaved : Failure
    Unsaved --> [*] : Discard and quit
    state Unsaved {
        [*] --> Typing
        Typing --> Idle : Stop typing
        Idle --> Typing : Type again
    }
```

---

## 5. ER diagram (Mermaid erDiagram)

```mermaid
erDiagram
    CUSTOMER ||--o{ ORDER : places
    ORDER ||--|{ LINE_ITEM : contains
    PRODUCT ||--o{ LINE_ITEM : "ordered in"
    CUSTOMER {
        int id PK
        string name
        string email
    }
    ORDER {
        int id PK
        int customer_id FK
        date ordered_at
    }
```

---

## 6. Gantt chart (Mermaid gantt)

Expected: dates on the horizontal axis, bars colored by section, and done, active, and critical tasks.

```mermaid
gantt
    title mdEditor release plan
    dateFormat YYYY-MM-DD
    axisFormat %m/%d
    section Design
        Requirements    :done,    a1, 2026-10-01, 3d
        UI design       :done,    a2, after a1, 4d
    section Build
        Editor          :active,  b1, 2026-10-08, 6d
        Diagram support :crit,    b2, after b1, 5d
    section Release
        Testing         :         c1, after b2, 3d
        Launch          :milestone, m1, after c1, 0d
```

---

## 7. Pie chart (Mermaid pie)

```mermaid
pie showData
    title Files by type
    "Markdown" : 62
    "Images" : 21
    "PDF" : 11
    "Other" : 6
```

---

## 8. Bar and line chart (Mermaid xychart-beta)

```mermaid
xychart-beta
    title "Documents created per month"
    x-axis [Jan, Feb, Mar, Apr, May, Jun]
    y-axis "Count" 0 --> 50
    bar [12, 18, 25, 31, 28, 40]
    line [12, 18, 25, 31, 28, 40]
```

---

## 9. Quadrant chart (Mermaid quadrantChart)

```mermaid
quadrantChart
    title Feature priority
    x-axis Low cost --> High cost
    y-axis Low impact --> High impact
    quadrant-1 Plan and do
    quadrant-2 Do now
    quadrant-3 Later
    quadrant-4 Reconsider
    Diagrams: [0.35, 0.85]
    Math: [0.55, 0.6]
    Spell check: [0.75, 0.3]
    Themes: [0.2, 0.35]
```

---

## 10. User journey (Mermaid journey)

```mermaid
journey
    title Write a document and save it as PDF
    section Write
        Open file: 5: User
        Write text: 4: User
    section Export
        Check preview: 4: User
        Save as PDF: 3: User, System
```

---

## 11. Git graph (Mermaid gitGraph)

```mermaid
gitGraph
    commit id: "init"
    commit id: "UI"
    branch feature/diagram
    checkout feature/diagram
    commit id: "mermaid"
    commit id: "test"
    checkout main
    commit id: "i18n"
    merge feature/diagram
    commit id: "release"
```

---

## 12. Mind map (Mermaid mindmap)

```mermaid
mindmap
  root((mdEditor))
    Edit
      Find and replace
      Drag and drop
    View
      Preview
      Diagrams
        Flowchart
        Sequence diagram
    Output
      Save
      PDF
```

---

## 13. Timeline (Mermaid timeline)

```mermaid
timeline
    title mdEditor history
    2026-10-01 : First release
    2026-10-05 : Japanese/English UI
               : --lang option
    2026-10-10 : Bilingual README
```

---

## 14. Sankey diagram (Mermaid sankey-beta)

Mermaid sankey supports only ASCII node names.

```mermaid
sankey-beta
Input,Markdig,100
Markdig,HtmlSanitizer,100
HtmlSanitizer,Preview,90
HtmlSanitizer,Removed,10
```

---

## 15. Mermaid syntax error (error display check)

Expected: the app does not freeze, an error message and the source are shown, and other diagrams are unaffected.

```mermaid
flowchart TD
    A --> B --
    this is broken syntax {{{
```

---

## 16. Other diagram notations (rendered only if supported)

### 16-1. Graphviz (dot)

```dot
digraph G {
    rankdir=LR;
    node [shape=box];
    Editor -> Markdig -> Sanitizer -> Preview;
    Preview -> PDF [style=dashed];
}
```

### 16-2. PlantUML

```plantuml
@startuml
actor User
User -> mdEditor : Open
mdEditor --> User : Show
@enduml
```

### 16-3. nomnoml

```nomnoml
[Editor]->[Preview]
[Preview]->[PDF]
```

---

## 17. Math (KaTeX / MathJax)

Inline math: $E = mc^2$, $\sum_{i=1}^{n} i = \frac{n(n+1)}{2}$

Display math:

$$
\int_{-\infty}^{\infty} e^{-x^2}\,dx = \sqrt{\pi}
$$

$$
\begin{pmatrix} a & b \\ c & d \end{pmatrix}
\begin{pmatrix} x \\ y \end{pmatrix}
=
\begin{pmatrix} ax + by \\ cx + dy \end{pmatrix}
$$

---

## 18. For comparison: not diagrams (check that nothing breaks)

### 18-1. Regular code block

```javascript
// Mentions mermaid, but a different language must not become a diagram
const text = "flowchart TD; A --> B";
console.log(text);
```

### 18-2. Code block without a language

```
graph TD
    A --> B
```

### 18-3. Text diagram (columns should line up in monospace)

```text
+--------+      +---------+      +----------+
| Editor | ---> | Markdig | ---> | Preview  |
+--------+      +---------+      +----------+
```

### 18-4. Table

| Diagram type | Syntax | Result |
|:---|:---:|---:|
| Flowchart | `flowchart` | ☐ |
| Sequence diagram | `sequenceDiagram` | ☐ |
| Class diagram | `classDiagram` | ☐ |
| State diagram | `stateDiagram-v2` | ☐ |
| ER diagram | `erDiagram` | ☐ |
| Gantt | `gantt` | ☐ |
| Pie chart | `pie` | ☐ |
| Math | `$...$` | ☐ |

---

## Checklist

- [ ] Every Mermaid diagram renders as a drawing, not as code
- [ ] Labels render without garbled characters or overflow
- [ ] A diagram with a syntax error does not stop the others from rendering
- [ ] Diagrams redraw while editing without flicker or scroll jumps
- [ ] Diagrams do not overflow a narrow preview (or can be scrolled)
- [ ] "Save as PDF" includes the diagrams in the PDF
- [ ] Diagrams render offline
