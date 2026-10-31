/*
  ファイル名 / File: PdfOptions.cs
  概要 / Description: The choices and numbers that tune PDF import, with the advanced settings file pdf-import.json
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-28
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdEditor;

// How a PDF is opened. The three choices come from Settings; the numbers can be changed in
// pdf-import.json (in %LOCALAPPDATA%\mdEditor) without a new build. A value left out of the
// file, or a file that cannot be read, keeps the default. Shares are of the page's text
// width (or of a page's letters, for the page tests); lengths are in points.
internal sealed class PdfImportOptions
{
    // Settings: text around a figure goes into its picture (true, the default, looks like the
    // page) or stays text (false, can be edited); right-aligned and centered text is written
    // as HTML; a page that cannot be read as text becomes a picture of the whole page.
    [JsonIgnore] public bool FigureTextInPicture { get; set; } = true;
    // Settings: a table becomes a Markdown table (false, the default, can be edited) or a
    // picture of itself (true, looks like the page).
    [JsonIgnore] public bool TablesAsPictures { get; set; }
    [JsonIgnore] public bool Align { get; set; } = true;
    [JsonIgnore] public bool PagePictures { get; set; } = true;

    // Pages made into pictures: the fewest letters a page needs to be tested; the share of
    // its letters in fonts that cannot be read, of steps that go down (vertical writing), of
    // letters turned, and of letters over other letters; the fewest shapes on a page with no
    // letters (text drawn as outlines); how many times wider than shown PDFium draws a page,
    // and the most pixels it draws.
    public int PageMinLetters { get; set; } = 20;
    public double UnreadableShare { get; set; } = 0.3;
    public double VerticalShare { get; set; } = 0.5;
    public double TurnedShare { get; set; } = 0.3;
    public double OverlapShare { get; set; } = 0.15;
    public int OutlineMinShapes { get; set; } = 200;
    public double RenderScale { get; set; } = 2;
    public long RenderMaxPixels { get; set; } = 40_000_000;

    // Figures: how close lines and shapes must be to belong to one figure, the fewest parts
    // and the smallest width and height of a figure, the share of the page a box must be wider
    // than to be a frame around text (not part of a figure), the longest label taken into a
    // figure, the longest callout line taken into a picture with callouts, and how many line
    // heights away from it a callout may be, and the share of a block of text that must lie in
    // a figure for it to go into the figure's picture (when FigureTextInPicture).
    public double FigureGap { get; set; } = 6;
    public int FigureMinParts { get; set; } = 3;
    public double FigureMinSize { get; set; } = 30;
    public double FrameWidthShare { get; set; } = 0.45;
    public int LabelMaxLetters { get; set; } = 16;
    public int CalloutMaxLetters { get; set; } = 32;
    public double CalloutReachLines { get; set; } = 3;
    public double FigureTextShare { get; set; } = 0.3;

    // Lines: the widest gap (in line heights) across which a line goes on; a value (up to
    // ValueMaxLetters letters, within ValueEdgeShare of the right edge) goes on a label's line
    // (up to ValueLabelMaxLetters letters) across a gap of up to ValueGapShare. The share of a
    // page's letters the tags must hold to be read, and the share of the page's height at the
    // top and bottom where page numbers are left out.
    public double JoinGapLines { get; set; } = 2.5;
    public int ValueMaxLetters { get; set; } = 16;
    public int ValueLabelMaxLetters { get; set; } = 24;
    public double ValueEdgeShare { get; set; } = 0.1;
    public double ValueGapShare { get; set; } = 0.4;
    public double TagCoverage { get; set; } = 0.9;
    public double PageNumberMargin { get; set; } = 0.08;

    // Alignment: how far in a right-aligned line must start, how close to the right edge it
    // must end, and how close to the middle a centered line must be.
    public double AlignRightIndent { get; set; } = 0.25;
    public double AlignRightTolerance { get; set; } = 0.06;
    public double AlignCenterTolerance { get; set; } = 0.03;

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mdEditor", "pdf-import.json");

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    // The numbers from pdf-import.json, or the defaults.
    public static PdfImportOptions Load()
    {
        try
        {
            return File.Exists(FilePath) ? JsonSerializer.Deserialize<PdfImportOptions>(File.ReadAllText(FilePath), Json) ?? new() : new();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return new();
        }
    }

    // Writes the file with the defaults (when there is none, or always with overwrite, for
    // Settings' "初期設定に戻す"), each value with what it means in Japanese and English
    // above it as comments, which Load skips (JSON with comments, as Notepad shows it).
    public static void WriteDefaults(bool overwrite = false)
    {
        if (File.Exists(FilePath) && !overwrite)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var defaults = new PdfImportOptions();
        var text = new System.Text.StringBuilder();
        text.Append("// mdEditor: PDF の取り込みの詳細な数値 / Finer values of opening PDFs\n")
            .Append("// 値を変えて保存すると、次に PDF を開いたときから使います。書かなかった項目と、読めないファイルは初期値になります。\n")
            .Append("// 「設定」→「初期設定に戻す」で、この内容に戻ります。割合は 0〜1（ページの文字の幅や、ページの文字の数に対して）、長さはポイント（1/72 インチ）です。\n")
            .Append("// Changed values are used from the next PDF opened. A value left out, or a file that cannot be read, keeps the default.\n")
            .Append("// Settings > Restore defaults writes this again. Shares are 0 to 1 (of the page's text width, or of its letters); lengths are in points (1/72 inch).\n")
            .Append("{\n");
        var properties = typeof(PdfImportOptions).GetProperties()
            .Where(property => property.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length == 0).ToList();
        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            if (Descriptions.TryGetValue(property.Name, out var description))
            {
                text.Append("\n  // ").Append(description.Ja).Append("\n  // ").Append(description.En).Append('\n');
            }

            text.Append("  \"").Append(property.Name).Append("\": ").Append(JsonSerializer.Serialize(property.GetValue(defaults)))
                .Append(i < properties.Count - 1 ? ",\n" : "\n");
        }

        File.WriteAllText(FilePath, text.Append("}\n").ToString());
    }

    // What each value means, for the comments in pdf-import.json (and in the README).
    private static readonly Dictionary<string, (string Ja, string En)> Descriptions = new()
    {
        [nameof(PageMinLetters)] = ("ページ全体を画像にするか調べる、ページの文字の数の下限。これより少ないページは文字として読みます。",
            "The fewest letters a page needs before it is tested for becoming a picture of itself; a page with fewer is read as text."),
        [nameof(UnreadableShare)] = ("文字を読み取れない書体（文字化けする書体）の文字がこの割合以上なら、ページを PDFium で画像（PNG）にします。",
            "When this share of a page's letters is in fonts that cannot be read (garbled), PDFium draws the page as a PNG."),
        [nameof(VerticalShare)] = ("文字から次の文字へ下に進む割合がこれ以上なら縦書きとみなし、ページ全体を画像（SVG）にします。",
            "When this share of steps from a letter to the next goes down, the page is taken for vertical writing and becomes one SVG picture."),
        [nameof(TurnedShare)] = ("回転した文字がこの割合以上なら、ページ全体を画像（SVG）にします。",
            "When this share of the letters is turned, the page becomes one SVG picture."),
        [nameof(OverlapShare)] = ("ほかの文字の上に重なった文字がこの割合以上なら、ページ全体を画像（SVG）にします（ポスターのようなページ）。",
            "When this share of the letters lies over other letters (a poster-like page), the page becomes one SVG picture."),
        [nameof(OutlineMinShapes)] = ("文字が 1 つもないページで、図形がこの数以上あれば、文字を図形で描いたページとみなし、PDFium で画像（PNG）にします。",
            "A page with no letters and at least this many shapes is taken for text drawn as shapes, and PDFium draws it as a PNG."),
        [nameof(RenderScale)] = ("PDFium でページを画像にするときの倍率（表示の大きさに対して）。大きいほど細かく、ファイルが大きくなります。",
            "How many times larger than shown PDFium draws a page; larger is sharper and makes bigger files."),
        [nameof(RenderMaxPixels)] = ("PDFium で描く画像の画素数の上限。これを超えるページは画像にしません。",
            "The most pixels PDFium draws for a page; a page that would need more is not made a picture."),
        [nameof(FigureGap)] = ("線や図形を 1 つの図にまとめる距離（ポイント）。これより近いものは同じ図です。",
            "How close (in points) lines and shapes must be to belong to one figure."),
        [nameof(FigureMinParts)] = ("図とみなすのに必要な線・図形の数の下限（四角い枠と表の線は数えません）。",
            "The fewest lines and shapes that make a figure (boxes and table lines do not count)."),
        [nameof(FigureMinSize)] = ("図とみなす幅と高さの下限（ポイント）。",
            "The smallest width and height (in points) of a figure."),
        [nameof(FrameWidthShare)] = ("ページの幅に対してこの割合より広い四角は、図ではなく文字の枠（コードの背景・注意書き・カード）とみなします。",
            "A box wider than this share of the page is a frame around text (a code background, a note, a card), not part of a figure."),
        [nameof(LabelMaxLetters)] = ("図のまわりのラベルとして図の画像に入れる行の、文字の数の上限。長い行は本文に残します。",
            "The longest line (in letters) taken into a figure's picture as a label; longer lines stay text."),
        [nameof(CalloutMaxLetters)] = ("線で名前を付けたイラスト（間取り図など）で、図の画像に入れる説明の行の、文字の数の上限。",
            "In a picture with its parts named around it (a floor plan, say), the longest line of a callout taken into the picture."),
        [nameof(CalloutReachLines)] = ("そのような説明を、イラストから何行分の距離まで図に入れるか。",
            "How many line heights from such a picture a callout may be and still go into it."),
        [nameof(FigureTextShare)] = ("「図のまわりの文字を図に入れる」とき、文字のまとまりのこの割合以上が図の範囲にあれば、まとまりごと図の画像に入れます。",
            "With \"put text around figures in the picture\", a block of text with this share or more of its letters in a figure goes into the figure's picture whole."),
        [nameof(JoinGapLines)] = ("同じ高さの文字を 1 行につなぐ間隔の上限（行の高さの何倍か）。",
            "The widest gap (in line heights) across which letters on one baseline are one line."),
        [nameof(ValueMaxLetters)] = ("「小計　¥300,000」のように、離れて置かれた値をラベルの行につなぐときの、値の文字の数の上限。",
            "The longest value (in letters) set apart on a label's line (\"小計  ¥300,000\") that is joined to it."),
        [nameof(ValueLabelMaxLetters)] = ("そのときのラベルの文字の数の上限。",
            "The longest label (in letters) such a value is joined to."),
        [nameof(ValueEdgeShare)] = ("値が文字の右端からこの割合（文字の幅に対して）以内にあるときだけ、ラベルの行につなぎます。",
            "Such a value is joined only when it ends within this share of the text's width from its right edge."),
        [nameof(ValueGapShare)] = ("ラベルと値の間隔の上限（文字の幅に対する割合）。",
            "The widest gap between such a label and its value, as a share of the text's width."),
        [nameof(TagCoverage)] = ("タグ付きの PDF で、ページの文字のこの割合以上がタグに入っていれば、タグのとおりに読みます。",
            "In a tagged PDF, a page is read by its tags when they hold this share or more of its letters."),
        [nameof(PageNumberMargin)] = ("ページの上下の、この割合（ページの高さに対して）の範囲にあるページ番号を省きます。",
            "Page numbers within this share of the page's height from its top or bottom are left out."),
        [nameof(AlignRightIndent)] = ("右寄せとみなす行の、左端からの位置の下限（文字の幅に対する割合）。",
            "How far in (as a share of the text's width) a line must start to count as right-aligned."),
        [nameof(AlignRightTolerance)] = ("右寄せとみなす、右端からのずれの上限（文字の幅に対する割合）。",
            "How close to the right edge (as a share of the text's width) a right-aligned line must end."),
        [nameof(AlignCenterTolerance)] = ("中央寄せとみなす、中央からのずれの上限（文字の幅に対する割合）。",
            "How close to the middle (as a share of the text's width) a centered line must be."),
    };
}
