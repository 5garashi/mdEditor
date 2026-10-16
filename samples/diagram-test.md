<!--
  ファイル名 / File: samples/diagram-test.md
  概要 / Description: Test document for diagram, chart, and math rendering (Japanese)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 図・チャート表示テスト

mdEditor のプレビューで、各種ダイアグラムやチャートが正しく表示されるかを確認するためのテスト文書です。
各セクションの「期待される表示」と実際のプレビューを見比べてください。

---

## 1. フローチャート（Mermaid flowchart）

期待される表示：上から下へ流れる分岐付きのフローチャート。ひし形の判定ノードと、色付きのノードがある。

```mermaid
flowchart TD
    A([開始]) --> B[ファイルを開く]
    B --> C{拡張子は .md？}
    C -- はい --> D[エディタに読み込む]
    C -- いいえ --> E[エラーを表示]
    D --> F[/プレビューを更新/]
    F --> G[(保存)]
    E --> H([終了])
    G --> H

    classDef ok fill:#d4f7d4,stroke:#2a7a2a,color:#1a1a1a;
    classDef ng fill:#ffd6d6,stroke:#a22,color:#1a1a1a;
    class D,F,G ok;
    class E ng;
```

### 1-2. 左から右・サブグラフ

```mermaid
flowchart LR
    subgraph UI[画面]
        ed[エディタ] --> pv[プレビュー]
    end
    subgraph Host[WPF ホスト]
        md[Markdig] --> san[HtmlSanitizer]
    end
    ed -- テキスト --> md
    san -- HTML --> pv
```

---

## 2. シーケンス図（Mermaid sequenceDiagram）

期待される表示：4 人の参加者、同期/非同期の矢印、ループ・分岐の枠、注記。

```mermaid
sequenceDiagram
    autonumber
    actor User as ユーザー
    participant Page as WebView2 (app.js)
    participant Host as MainWindow
    participant FS as ファイルシステム

    User->>Page: 文字を入力
    Page-)Host: postMessage(text)
    Note right of Host: 一定時間待ってから<br/>プレビューを更新
    loop 入力が続く間
        Host->>Host: タイマーをリセット
    end
    Host-->>Page: preview HTML
    User->>Page: 保存ボタン
    Page->>Host: save
    alt 既存ファイル
        Host->>FS: 上書き保存
    else 新規ファイル
        Host->>User: 名前を付けて保存ダイアログ
        User->>Host: ファイル名
        Host->>FS: 新規保存
    end
    FS-->>Host: 完了
    Host-->>Page: 保存済み
```

---

## 3. クラス図（Mermaid classDiagram）

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
    MainWindow "1" *-- "1" Document : 編集中
    MainWindow ..> Renderer : 使用
    Renderer <|.. MarkdigRenderer
```

---

## 4. 状態遷移図（Mermaid stateDiagram）

```mermaid
stateDiagram-v2
    [*] --> 保存済み
    保存済み --> 未保存 : 編集
    未保存 --> 保存中 : Ctrl+S
    保存中 --> 保存済み : 成功
    保存中 --> 未保存 : 失敗
    未保存 --> [*] : 破棄して終了
    state 未保存 {
        [*] --> 入力中
        入力中 --> 待機 : 入力停止
        待機 --> 入力中 : 再入力
    }
```

---

## 5. ER 図（Mermaid erDiagram）

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

## 6. ガントチャート（Mermaid gantt）

期待される表示：横軸が日付、セクションごとに色分けされたバー。完了・進行中・重要タスクの表現。

```mermaid
gantt
    title mdEditor リリース計画
    dateFormat YYYY-MM-DD
    axisFormat %m/%d
    section 設計
        要件整理        :done,    a1, 2026-10-01, 3d
        UI 設計         :done,    a2, after a1, 4d
    section 実装
        エディタ        :active,  b1, 2026-10-08, 6d
        図の表示対応    :crit,    b2, after b1, 5d
    section リリース
        テスト          :         c1, after b2, 3d
        公開            :milestone, m1, after c1, 0d
```

---

## 7. 円グラフ（Mermaid pie）

```mermaid
pie showData
    title ファイル種別の内訳
    "Markdown" : 62
    "画像" : 21
    "PDF" : 11
    "その他" : 6
```

---

## 8. 棒グラフ・折れ線グラフ（Mermaid xychart-beta）

```mermaid
xychart-beta
    title "月別の文書作成数"
    x-axis ["1月", "2月", "3月", "4月", "5月", "6月"]
    y-axis "件数" 0 --> 50
    bar [12, 18, 25, 31, 28, 40]
    line [12, 18, 25, 31, 28, 40]
```

---

## 9. 4 象限チャート（Mermaid quadrantChart）

```mermaid
quadrantChart
    title 機能の優先度
    x-axis 低コスト --> 高コスト
    y-axis 低効果 --> 高効果
    quadrant-1 計画して実施
    quadrant-2 すぐやる
    quadrant-3 後回し
    quadrant-4 再検討
    図の表示: [0.35, 0.85]
    数式表示: [0.55, 0.6]
    スペルチェック: [0.75, 0.3]
    テーマ切替: [0.2, 0.35]
```

---

## 10. ユーザージャーニー（Mermaid journey）

```mermaid
journey
    title 文書を作成して PDF にする
    section 作成
        ファイルを開く: 5: ユーザー
        本文を書く: 4: ユーザー
    section 出力
        プレビュー確認: 4: ユーザー
        PDF に保存: 3: ユーザー, システム
```

---

## 11. Git グラフ（Mermaid gitGraph）

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

## 12. マインドマップ（Mermaid mindmap）

```mermaid
mindmap
  root((mdEditor))
    編集
      検索・置換
      ドラッグ＆ドロップ
    表示
      プレビュー
      図
        フローチャート
        シーケンス図
    出力
      保存
      PDF
```

---

## 13. タイムライン（Mermaid timeline）

```mermaid
timeline
    title mdEditor の歩み
    2026-10-01 : 初回リリース
    2026-10-05 : 日英バイリンガル UI
               : --lang オプション
    2026-10-10 : README 二か国語化
```

---

## 14. サンキーダイアグラム（Mermaid sankey-beta）

Mermaid のサンキーは日本語などの非 ASCII 文字の名前に対応していないため、英数字の名前を使います。

```mermaid
sankey-beta
Input,Markdig,100
Markdig,HtmlSanitizer,100
HtmlSanitizer,Preview,90
HtmlSanitizer,Removed,10
```

---

## 15. Mermaid の構文エラー（エラー表示の確認）

期待される表示：アプリが固まらず、エラーメッセージまたは元のコードが表示される。他の図の表示に影響しない。

```mermaid
flowchart TD
    A --> B --
    これは壊れた構文です {{{
```

---

## 16. その他の図の記法（対応していれば表示）

### 16-1. Graphviz（dot）

期待される表示：上の「開く」から、mdEditor の中の流れ（編集画面 → 変換 → 安全化 → プレビュー）を通って、「名前を付けて保存」へ進む色付きの図。表の形の箱、角の丸い箱、点線の双方向の矢印がある。

```dot
digraph mdEditor {
    graph [rankdir=TB, pad=0.2, nodesep=0.3, ranksep=0.32, fontname="Helvetica", bgcolor="#FFFDF2"];
    node  [shape=box, style="rounded,filled", fontname="Helvetica", fontsize=12,
           color="#385723", fillcolor="#FFFFFF", fontcolor="#1A1A1A", penwidth=1.4, margin="0.2,0.06"];
    edge  [color="#385723", penwidth=1.4, arrowsize=0.8, fontname="Helvetica", fontsize=10, fontcolor="#7B4A21"];

    open [shape=plain, label=<
      <TABLE BORDER="1" CELLBORDER="0" CELLSPACING="0" CELLPADDING="5" COLOR="#385723" BGCOLOR="#FFFFFF" STYLE="rounded">
        <TR><TD COLSPAN="3" BGCOLOR="#FFE699"><B>開く</B></TD></TR>
        <TR><TD>Markdown・テキスト</TD><TD>HTML・Web ページ</TD><TD>Word・メール</TD></TR>
        <TR><TD>CSV・TSV</TD><TD>JSON・XML</TD><TD>Jupyter・画像</TD></TR>
      </TABLE>>];

    subgraph cluster_app {
      label=<<B>mdEditor</B>>; labeljust=l; fontsize=13; fontcolor="#385723";
      style="rounded,filled"; color="#385723"; fillcolor="#F9EFCB"; penwidth=1.6; margin=14;
      editor  [label="編集画面\nCodeMirror"];
      convert [label="変換\nMarkdig"];
      guard   [label="安全化\nHtmlSanitizer", color="#B92613", fillcolor="#FDECEA"];
      preview [label="プレビュー\nMermaid · KaTeX · Graphviz · highlight.js", fillcolor="#FFE699", penwidth=2];
      { rank=same; editor; convert; }
      { rank=same; guard; preview; }
      editor -> convert;
      convert -> guard;
      guard -> preview;
      preview -> editor [label="スクロール連動", style=dashed, dir=both, constraint=false, color="#7B4A21"];
    }

    save [shape=plain, label=<
      <TABLE BORDER="1" CELLBORDER="0" CELLSPACING="0" CELLPADDING="5" COLOR="#385723" BGCOLOR="#FFFFFF" STYLE="rounded">
        <TR><TD COLSPAN="3" BGCOLOR="#385723"><FONT COLOR="#FFFFFF"><B>名前を付けて保存</B></FONT></TD></TR>
        <TR><TD>PDF・PNG</TD><TD>HTML</TD><TD>Word</TD></TR>
        <TR><TD>CSV・TSV・JSON</TD><TD>Jupyter Notebook</TD><TD>テキスト</TD></TR>
      </TABLE>>];

    open -> editor;
    preview -> save;
}
```

### 16-2. PlantUML

```plantuml
@startuml
actor ユーザー
ユーザー -> mdEditor : 開く
mdEditor --> ユーザー : 表示
@enduml
```

### 16-3. nomnoml

```nomnoml
[Editor]->[Preview]
[Preview]->[PDF]
```

---

## 17. 数式（KaTeX / MathJax）

インライン数式： $E = mc^2$ 、 $\sum_{i=1}^{n} i = \frac{n(n+1)}{2}$

ブロック数式：

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

## 18. 比較用：図ではないもの（崩れないことの確認）

### 18-1. 普通のコードブロック

```javascript
// mermaid と書かれていても、言語が違えば図にしない
const text = "flowchart TD; A --> B";
console.log(text);
```

### 18-2. 言語指定なしのコードブロック

```
graph TD
    A --> B
```

### 18-3. テキストの図（等幅で揃っていること）

```text
+--------+      +---------+      +----------+
| Editor | ---> | Markdig | ---> | Preview  |
+--------+      +---------+      +----------+
```

### 18-4. 表

| 図の種類 | 記法 | 確認結果 |
|:---|:---:|---:|
| フローチャート | `flowchart` | ☐ |
| シーケンス図 | `sequenceDiagram` | ☐ |
| クラス図 | `classDiagram` | ☐ |
| 状態遷移図 | `stateDiagram-v2` | ☐ |
| ER 図 | `erDiagram` | ☐ |
| ガント | `gantt` | ☐ |
| 円グラフ | `pie` | ☐ |
| 数式 | `$...$` | ☐ |

---

## 19. コードの色分け（highlight.js）

期待される表示：言語名を書いたコードブロックは、キーワード（赤）・文字列（紺）・数値（青）・関数名（紫）・属性（茶）・タグ（緑）・コメント（灰色の斜体）に色分けされる。夜の配色では明るい色になる。言語名のないブロックは色が付かない。PDF・PNG・HTML・Word に保存しても色が残る。

### 19-1. Python

```python
from pathlib import Path

def count_headings(path: Path) -> int:
    """見出しの数を数える"""
    lines = path.read_text(encoding="utf-8").splitlines()
    return sum(1 for line in lines if line.startswith("#"))  # コメント

print(count_headings(Path("README.md")), 3.14, True, None)
```

### 19-2. JavaScript / TypeScript

```javascript
const formats = ["pdf", "png", "html", "docx"];
export async function save(name, format = "pdf") {
  if (!formats.includes(format)) throw new Error(`unknown: ${format}`);
  return { name, format, size: 1024 * 2 };
}
```

```ts
interface Heading { level: number; text: string }
const toc = (items: Heading[]): string[] =>
  items.map((h) => "  ".repeat(h.level - 2) + "- " + h.text);
```

### 19-3. C#

```csharp
public sealed record Document(string Path, int Version);

public static class Saver
{
    // 保存に失敗しても元のファイルは残る
    public static async Task SaveAsync(Document doc, string text)
    {
        await File.WriteAllTextAsync(doc.Path + ".tmp", text);
        File.Move(doc.Path + ".tmp", doc.Path, overwrite: true);
    }
}
```

### 19-4. PowerShell・シェル

```powershell
Get-ChildItem -Path $env:USERPROFILE\Documents -Filter *.md |
    Where-Object { $_.Length -gt 1KB } |
    ForEach-Object { Write-Host "$($_.Name): $($_.Length) bytes" }
```

```bash
for file in docs/*.md; do
  echo "checking $file" && grep -c "^#" "$file"
done
```

### 19-5. SQL・JSON・YAML

```sql
SELECT name, COUNT(*) AS pages
FROM documents
WHERE saved_at >= '2026-10-01' AND format IN ('pdf', 'docx')
GROUP BY name
ORDER BY pages DESC;
```

```json
{
  "app": "mdEditor",
  "version": "1.0.11",
  "formats": ["pdf", "html"],
  "offline": true,
  "zoom": 0.9
}
```

```yaml
app: mdEditor
theme: auto   # auto, light, dark
paper: A4
backup:
  enabled: true
  interval: 15
```

### 19-6. HTML・CSS

```html
<!-- コメント -->
<button id="save" class="primary"
        aria-keyshortcuts="Control+S">保存</button>
```

```css
.preview-content pre {
  padding: 12px;
  border: 1px solid #1A1A1A;
  border-radius: 6px;
}
```

### 19-7. diff

```diff
- PDFに保存ボタン
+ 名前を付けて保存で PDF を選ぶ
  変わらない行
```

### 19-8. 短い言語名と、言語名なし

`py`・`js`・`cs`・`sh`・`ps1` のような短い書き方でも色が付く。最後のブロックは言語名がないので色が付かない。

```py
print("short name")
```

```ps1
Write-Output "short name"
```

```
no language: def print const SELECT
```

---

## 確認チェックリスト

- [ ] すべての Mermaid 図がコードではなく図として表示される
- [ ] 日本語のラベルが文字化け・はみ出しせずに表示される
- [ ] 構文エラーの図があっても他の図は表示される
- [ ] 編集中に図が再描画され、ちらつきやスクロール位置の飛びがない
- [ ] 幅の狭いプレビューでも図が横にはみ出さない（またはスクロールできる）
- [ ] 「PDFに保存」で図が PDF に含まれる
- [ ] オフライン環境でも図が表示される
- [ ] Graphviz の図（16-1）が色付きの図として表示される
- [ ] 言語名を書いたコードブロック（19）が色分けされ、言語名のないものは色が付かない
- [ ] 「名前を付けて保存」で PDF・PNG・HTML・Word にしても、図とコードの色が残る

---

**作成者**: 5garashi.com設計事務所\
**ライセンス**: [MIT License](../LICENSE)
