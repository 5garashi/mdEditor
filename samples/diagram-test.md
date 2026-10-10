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

    classDef ok fill:#d4f7d4,stroke:#2a7a2a;
    classDef ng fill:#ffd6d6,stroke:#a22;
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

## 確認チェックリスト

- [ ] すべての Mermaid 図がコードではなく図として表示される
- [ ] 日本語のラベルが文字化け・はみ出しせずに表示される
- [ ] 構文エラーの図があっても他の図は表示される
- [ ] 編集中に図が再描画され、ちらつきやスクロール位置の飛びがない
- [ ] 幅の狭いプレビューでも図が横にはみ出さない（またはスクロールできる）
- [ ] 「PDFに保存」で図が PDF に含まれる
- [ ] オフライン環境でも図が表示される
