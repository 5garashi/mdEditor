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
        WriteIndented = true,
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

    // Writes the file with the defaults when there is none, so it can be opened and changed.
    public static void WriteDefaults()
    {
        if (!File.Exists(FilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new PdfImportOptions(), Json));
        }
    }
}
