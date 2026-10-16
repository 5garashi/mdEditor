/*
  File: DocxExport.cs
  Description: Writes a Markdown document as a Word (.docx) file with Word's own styles
  Author: 5garashi.com Design Office
  Created: 2026-10-16
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using MdTable = Markdig.Extensions.Tables.Table;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using MdTableCell = Markdig.Extensions.Tables.TableCell;
using WTable = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace MdEditor;

// A picture taken from the preview: a diagram or a formula, in document order.
internal sealed record ExportFigure(byte[] Png, int PixelWidth, int PixelHeight, double Scale);

// Markdown to Word: headings, lists, quotes, code, and tables use Word's own
// styles (so the navigation pane and a table of contents work); diagrams and
// math are pictures taken from the preview, since Word cannot draw them.
internal sealed class DocxExport
{
    private const long EmuPerPixel = 9525;
    private const long MaxWidthEmu = 5760720; // 6.3 inches, inside A4 and Letter margins
    private readonly MarkdownPipeline _pipeline;
    private readonly IReadOnlyList<ExportFigure> _figures;
    private readonly Func<string, string?> _resolveImage;
    private MainDocumentPart _main = null!;
    private int _figureIndex;
    private uint _drawingId;
    private int _numberingId;

    public DocxExport(MarkdownPipeline pipeline, IReadOnlyList<ExportFigure> figures, Func<string, string?> resolveImage)
    {
        _pipeline = pipeline;
        _figures = figures;
        _resolveImage = resolveImage;
    }

    public void Write(string markdown, string path, string title)
    {
        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        _main = document.AddMainDocumentPart();
        _main.Document = new Document(new Body());
        AddStyles();
        _main.AddNewPart<NumberingDefinitionsPart>().Numbering = new Numbering(BulletDefinition(1), DecimalDefinition(2));
        document.PackageProperties.Title = title;
        document.PackageProperties.Creator = "mdEditor";

        var body = _main.Document.Body!;
        foreach (var block in Markdown.Parse(markdown, _pipeline))
        {
            foreach (var element in Blocks(block, 0, null))
            {
                body.Append(element);
            }
        }

        body.Append(new SectionProperties(
            new PageMargin { Top = 1134, Bottom = 1134, Left = 1134, Right = 1134, Header = 720, Footer = 720, Gutter = 0 }));
        _main.Document.Save();
    }

    // --- Blocks ---------------------------------------------------------------

    private IEnumerable<OpenXmlElement> Blocks(Block block, int level, string? style)
    {
        switch (block)
        {
            case HeadingBlock heading:
                yield return Paragraph($"Heading{Math.Clamp(heading.Level, 1, 6)}", Inlines(heading.Inline));
                break;
            case ParagraphBlock paragraph:
                yield return Paragraph(style, Inlines(paragraph.Inline));
                break;
            case MathBlock:
                yield return FigureParagraph(style);
                break;
            case FencedCodeBlock fenced when string.Equals(fenced.Info, "mermaid", StringComparison.OrdinalIgnoreCase):
                yield return FigureParagraph(style);
                break;
            case CodeBlock code:
                foreach (var line in code.Lines.Lines.Take(code.Lines.Count))
                {
                    yield return Paragraph("Code", [Run(line.Slice.ToString(), code: true)]);
                }
                break;
            case ThematicBreakBlock:
                yield return new Paragraph(new ParagraphProperties(new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Size = 8, Space = 1, Color = "D0D0D0" })));
                break;
            case QuoteBlock quote:
                foreach (var child in quote)
                {
                    foreach (var element in Blocks(child, level, "Quote"))
                    {
                        yield return element;
                    }
                }
                break;
            case ListBlock list:
                var numbering = NewNumbering(list.IsOrdered, int.TryParse(list.OrderedStart, out var start) ? start : 1);
                foreach (var item in list.OfType<ListItemBlock>())
                {
                    var first = true;
                    foreach (var child in item)
                    {
                        if (child is ListBlock)
                        {
                            foreach (var element in Blocks(child, level + 1, style))
                            {
                                yield return element;
                            }
                            continue;
                        }

                        foreach (var element in Blocks(child, level, style))
                        {
                            if (first && element is Paragraph listParagraph)
                            {
                                listParagraph.ParagraphProperties ??= new ParagraphProperties();
                                listParagraph.ParagraphProperties.NumberingProperties = new NumberingProperties(
                                    new NumberingLevelReference { Val = Math.Min(level, 8) },
                                    new NumberingId { Val = numbering });
                                first = false;
                            }
                            else if (element is Paragraph continuation)
                            {
                                continuation.ParagraphProperties ??= new ParagraphProperties();
                                continuation.ParagraphProperties.Indentation = new Indentation { Left = (720 * (level + 1)).ToString() };
                            }

                            yield return element;
                        }
                    }
                }
                break;
            case MdTable table:
                yield return Table(table);
                yield return new Paragraph();
                break;
            case ContainerBlock container:
                foreach (var child in container)
                {
                    foreach (var element in Blocks(child, level, style))
                    {
                        yield return element;
                    }
                }
                break;
        }
    }

    private WTable Table(MdTable table)
    {
        var border = new Func<OpenXmlElement>[]
        {
            () => new TopBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
            () => new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
            () => new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
            () => new RightBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
            () => new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
            () => new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" }
        };
        var result = new WTable(new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Auto, Width = "0" },
            new TableBorders(border.Select(make => make()))));
        // Word requires the column grid; the columns share the text width (9,638 twips).
        var columns = Math.Max(1, table.ColumnDefinitions.Count);
        result.Append(new TableGrid(Enumerable.Range(0, columns)
            .Select(_ => new GridColumn { Width = (9638 / columns).ToString() })));
        foreach (var row in table.OfType<MdTableRow>())
        {
            var tableRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            if (row.IsHeader)
            {
                tableRow.Append(new TableRowProperties(new TableHeader()));
            }

            var column = 0;
            foreach (var cell in row.OfType<MdTableCell>())
            {
                var alignment = column < table.ColumnDefinitions.Count ? table.ColumnDefinitions[column].Alignment : null;
                column++;
                var paragraphs = cell.SelectMany(child => Blocks(child, 0, null)).OfType<Paragraph>().ToList();
                if (paragraphs.Count == 0)
                {
                    paragraphs.Add(new Paragraph());
                }

                foreach (var paragraph in paragraphs)
                {
                    paragraph.ParagraphProperties ??= new ParagraphProperties();
                    paragraph.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines { Before = "40", After = "40" };
                    if (alignment is TableColumnAlign.Center or TableColumnAlign.Right)
                    {
                        paragraph.ParagraphProperties.Justification = new Justification
                        {
                            Val = alignment == TableColumnAlign.Center ? JustificationValues.Center : JustificationValues.Right
                        };
                    }

                    if (row.IsHeader)
                    {
                        foreach (var run in paragraph.Descendants<Run>())
                        {
                            run.RunProperties ??= new RunProperties();
                            run.RunProperties.Bold = new Bold();
                        }
                    }
                }

                tableRow.Append(new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                    new[] { new TableCellProperties(new TableCellMargin(
                        new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa })) }
                        .Cast<OpenXmlElement>().Concat(paragraphs)));
            }

            result.Append(tableRow);
        }

        return result;
    }

    private Paragraph FigureParagraph(string? style)
    {
        var paragraph = Paragraph(style, Figure(display: true) is { } run ? [run] : [Run("［図 / Figure］")]);
        paragraph.ParagraphProperties ??= new ParagraphProperties();
        paragraph.ParagraphProperties.Justification = new Justification { Val = JustificationValues.Center };
        return paragraph;
    }

    private static Paragraph Paragraph(string? style, IEnumerable<OpenXmlElement> content)
    {
        var paragraph = new Paragraph();
        if (style is not null)
        {
            paragraph.Append(new ParagraphProperties(new ParagraphStyleId { Val = style }));
        }

        paragraph.Append(content);
        return paragraph;
    }

    // --- Inlines --------------------------------------------------------------

    private List<OpenXmlElement> Inlines(ContainerInline? container, bool bold = false, bool italic = false, bool strike = false)
    {
        var result = new List<OpenXmlElement>();
        if (container is null)
        {
            return result;
        }

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    result.Add(Run(literal.Content.ToString(), bold, italic, strike));
                    break;
                case EmphasisInline emphasis:
                    var isStrike = emphasis.DelimiterChar == '~';
                    result.AddRange(Inlines(emphasis,
                        bold || (!isStrike && emphasis.DelimiterCount >= 2),
                        italic || (!isStrike && emphasis.DelimiterCount % 2 == 1),
                        strike || isStrike));
                    break;
                case CodeInline code:
                    result.Add(Run(code.Content, bold, italic, strike, code: true));
                    break;
                case LineBreakInline lineBreak:
                    result.Add(lineBreak.IsHard ? new Run(new Break()) : Run(" "));
                    break;
                case TaskList task:
                    result.Add(Run(task.Checked ? "☑ " : "☐ "));
                    break;
                case MathInline:
                    result.Add(Figure(display: false) ?? Run("［式］"));
                    break;
                case LinkInline { IsImage: true } image:
                    result.Add(Image(image) ?? Run($"［{PlainText(image)}］", bold, italic, strike));
                    break;
                case LinkInline link:
                    result.Add(Hyperlink(link.Url, Inlines(link, bold, italic, strike)));
                    break;
                case AutolinkInline autolink:
                    result.Add(Hyperlink(autolink.IsEmail ? "mailto:" + autolink.Url : autolink.Url, [Run(autolink.Url, bold, italic, strike)]));
                    break;
                case HtmlEntityInline entity:
                    result.Add(Run(entity.Transcoded.ToString(), bold, italic, strike));
                    break;
                case ContainerInline nested:
                    result.AddRange(Inlines(nested, bold, italic, strike));
                    break;
            }
        }

        return result;
    }

    private static string PlainText(ContainerInline container) =>
        string.Concat(container.Descendants<LiteralInline>().Select(literal => literal.Content.ToString()));

    private static Run Run(string text, bool bold = false, bool italic = false, bool strike = false, bool code = false)
    {
        // Typed properties keep the order the Word schema requires.
        var properties = new RunProperties();
        if (code)
        {
            properties.RunFonts = new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", EastAsia = "Yu Gothic" };
            properties.Shading = new Shading { Val = ShadingPatternValues.Clear, Fill = "F2F2F2" };
        }

        if (bold) properties.Bold = new Bold();
        if (italic) properties.Italic = new Italic();
        if (strike) properties.Strike = new Strike();

        var run = new Run();
        if (properties.HasChildren)
        {
            run.Append(properties);
        }

        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private OpenXmlElement Hyperlink(string? url, IEnumerable<OpenXmlElement> content)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https" or "mailto"))
        {
            return new Run(content.SelectMany(element => element.Descendants<Text>()).Select(text => (OpenXmlElement)text.CloneNode(true)));
        }

        var relationship = _main.AddHyperlinkRelationship(uri, true);
        var hyperlink = new Hyperlink { Id = relationship.Id };
        foreach (var run in content.OfType<Run>())
        {
            run.RunProperties ??= new RunProperties();
            run.RunProperties.RunStyle = new RunStyle { Val = "Hyperlink" };
            hyperlink.Append(run);
        }

        return hyperlink;
    }

    // --- Pictures -------------------------------------------------------------

    private Run? Figure(bool display)
    {
        if (_figureIndex >= _figures.Count)
        {
            return null;
        }

        var figure = _figures[_figureIndex++];
        return Picture(figure.Png, ImagePartType.Png,
            (long)(figure.PixelWidth / figure.Scale * EmuPerPixel), (long)(figure.PixelHeight / figure.Scale * EmuPerPixel),
            display ? "Figure" : "Formula");
    }

    private Run? Image(LinkInline image)
    {
        if (_resolveImage(image.Url ?? string.Empty) is not { } path)
        {
            return null;
        }

        var type = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" or ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            _ => (PartTypeInfo?)null
        };
        if (type is null)
        {
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(path);
            var frame = BitmapDecoder.Create(new MemoryStream(bytes), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
            return Picture(bytes, type.Value, frame.PixelWidth * EmuPerPixel, frame.PixelHeight * EmuPerPixel, PlainText(image));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or FileFormatException)
        {
            return null;
        }
    }

    private Run Picture(byte[] bytes, PartTypeInfo type, long width, long height, string description)
    {
        if (width > MaxWidthEmu)
        {
            height = height * MaxWidthEmu / width;
            width = MaxWidthEmu;
        }

        var part = _main.AddImagePart(type);
        using (var stream = new MemoryStream(bytes))
        {
            part.FeedData(stream);
        }

        var id = ++_drawingId;
        var relationship = _main.GetIdOfPart(part);
        return new Run(new Drawing(new DW.Inline(
            new DW.Extent { Cx = width, Cy = height },
            new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
            new DW.DocProperties { Id = id, Name = $"Picture {id}", Description = description },
            new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
            new A.Graphic(new A.GraphicData(new PIC.Picture(
                new PIC.NonVisualPictureProperties(
                    new PIC.NonVisualDrawingProperties { Id = id, Name = $"Picture {id}", Description = description },
                    new PIC.NonVisualPictureDrawingProperties()),
                new PIC.BlipFill(new A.Blip { Embed = relationship }, new A.Stretch(new A.FillRectangle())),
                new PIC.ShapeProperties(
                    new A.Transform2D(new A.Offset { X = 0, Y = 0 }, new A.Extents { Cx = width, Cy = height }),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })))
            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
        {
            DistanceFromTop = 0, DistanceFromBottom = 0, DistanceFromLeft = 0, DistanceFromRight = 0
        }));
    }

    // --- Styles and numbering --------------------------------------------------

    private void AddStyles()
    {
        // Identity Art. 6: Segoe UI for Latin text and Yu Gothic for Japanese on Windows.
        var fonts = () => new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI", EastAsia = "Yu Gothic", ComplexScript = "Segoe UI" };
        var styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(new RunPropertiesBaseStyle(fonts(), new FontSize { Val = "22" }, new Languages { Val = "ja-JP", EastAsia = "ja-JP" })),
                new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(new SpacingBetweenLines { After = "120", Line = "300", LineRule = LineSpacingRuleValues.Auto }))),
            new Style(new StyleName { Val = "Normal" }, new PrimaryStyle()) { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true },
            new Style(new StyleName { Val = "Hyperlink" }, new StyleRunProperties(new Color { Val = "1A1A1A" }, new Underline { Val = UnderlineValues.Single }))
            { Type = StyleValues.Character, StyleId = "Hyperlink" },
            new Style(new StyleName { Val = "Code" }, new BasedOn { Val = "Normal" },
                new StyleParagraphProperties(new Shading { Val = ShadingPatternValues.Clear, Fill = "F2F2F2" },
                    new SpacingBetweenLines { After = "0", Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                new StyleRunProperties(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", EastAsia = "Yu Gothic" }, new FontSize { Val = "20" }))
            { Type = StyleValues.Paragraph, StyleId = "Code" },
            new Style(new StyleName { Val = "Quote" }, new BasedOn { Val = "Normal" },
                new StyleParagraphProperties(
                    new ParagraphBorders(new LeftBorder { Val = BorderValues.Single, Size = 18, Space = 8, Color = "1A1A1A" }),
                    new Indentation { Left = "567" }))
            { Type = StyleValues.Paragraph, StyleId = "Quote" });
        int[] sizes = [32, 28, 26, 24, 23, 22];
        for (var level = 1; level <= 6; level++)
        {
            var paragraph = new StyleParagraphProperties(new KeepNext());
            // Levels 1 and 2 keep the gray rule of the preview (Identity 4-3).
            if (level <= 2)
            {
                paragraph.Append(new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 8, Space = 1, Color = "D0D0D0" }));
            }

            paragraph.Append(new SpacingBetweenLines { Before = level <= 2 ? "360" : "240", After = "120" });
            paragraph.Append(new OutlineLevel { Val = level - 1 });
            styles.Append(new Style(
                new StyleName { Val = $"heading {level}" }, new BasedOn { Val = "Normal" }, new NextParagraphStyle { Val = "Normal" }, new PrimaryStyle(),
                paragraph,
                new StyleRunProperties(new Bold(), new FontSize { Val = sizes[level - 1].ToString() }))
            { Type = StyleValues.Paragraph, StyleId = $"Heading{level}" });
        }

        _main.AddNewPart<StyleDefinitionsPart>().Styles = styles;
    }

    private int NewNumbering(bool ordered, int start)
    {
        var numbering = _main.NumberingDefinitionsPart!.Numbering;
        var id = ++_numberingId;
        var instance = new NumberingInstance(new AbstractNumId { Val = ordered ? 2 : 1 }) { NumberID = id };
        if (ordered)
        {
            instance.Append(new LevelOverride(new StartOverrideNumberingValue { Val = start }) { LevelIndex = 0 });
        }

        numbering.Append(instance);
        return id;
    }

    private static AbstractNum BulletDefinition(int id) => new(Enumerable.Range(0, 9).Select(level => new Level(
        new StartNumberingValue { Val = 1 },
        new NumberingFormat { Val = NumberFormatValues.Bullet },
        new LevelText { Val = (level % 3) switch { 0 => "•", 1 => "◦", _ => "▪" } },
        new LevelJustification { Val = LevelJustificationValues.Left },
        new PreviousParagraphProperties(new Indentation { Left = (720 * (level + 1)).ToString(), Hanging = "360" }))
    { LevelIndex = level })) { AbstractNumberId = id };

    private static AbstractNum DecimalDefinition(int id) => new(Enumerable.Range(0, 9).Select(level => new Level(
        new StartNumberingValue { Val = 1 },
        new NumberingFormat { Val = NumberFormatValues.Decimal },
        new LevelText { Val = $"%{level + 1}." },
        new LevelJustification { Val = LevelJustificationValues.Left },
        new PreviousParagraphProperties(new Indentation { Left = (720 * (level + 1)).ToString(), Hanging = "360" }))
    { LevelIndex = level })) { AbstractNumberId = id };
}
