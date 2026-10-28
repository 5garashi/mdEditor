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
using Markdig.Renderers.Html;
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

// A colored piece of a code line, read from the preview (Color is RRGGBB).
internal sealed record CodeRun(string Text, string Color, bool Bold, bool Italic);

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
    // The colored lines of each code block in document order (from the preview).
    private readonly IReadOnlyList<IReadOnlyList<IReadOnlyList<CodeRun>>> _codeColors;
    private int _codeIndex;
    // Heading ids (as links write them) and the Word bookmark each heading gets,
    // so links such as a table of contents jump to the heading in Word too.
    private readonly Dictionary<string, string> _bookmarks = new(StringComparer.Ordinal);
    private int _bookmarkId;
    private MainDocumentPart _main = null!;
    private int _figureIndex;
    private uint _drawingId;
    private int _numberingId;

    public DocxExport(MarkdownPipeline pipeline, IReadOnlyList<ExportFigure> figures, Func<string, string?> resolveImage,
        IReadOnlyList<IReadOnlyList<IReadOnlyList<CodeRun>>>? codeColors = null)
    {
        _pipeline = pipeline;
        _figures = figures;
        _resolveImage = resolveImage;
        _codeColors = codeColors ?? [];
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
        var parsed = Markdown.Parse(markdown, _pipeline);
        foreach (var heading in parsed.Descendants<HeadingBlock>())
        {
            if (heading.GetAttributes().Id is { Length: > 0 } id && !_bookmarks.ContainsKey(id))
            {
                _bookmarks[id] = $"h{_bookmarks.Count + 1}";
            }
        }

        foreach (var block in parsed)
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
                var headingParagraph = Paragraph($"Heading{Math.Clamp(heading.Level, 1, 6)}", Inlines(heading.Inline));
                if (heading.GetAttributes().Id is { } headingId && _bookmarks.TryGetValue(headingId, out var bookmark))
                {
                    var number = (++_bookmarkId).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    headingParagraph.InsertAfter(new BookmarkStart { Id = number, Name = bookmark }, headingParagraph.ParagraphProperties);
                    headingParagraph.Append(new BookmarkEnd { Id = number });
                }

                yield return headingParagraph;
                break;
            case ParagraphBlock paragraph:
                yield return Paragraph(style, Inlines(paragraph.Inline));
                break;
            case MathBlock:
                yield return FigureParagraph(style);
                break;
            case FencedCodeBlock fenced when DiagramFences.IsDiagram(fenced.Info) || DiagramFences.IsMath(fenced.Info):
                yield return FigureParagraph(style);
                break;
            case Markdig.Extensions.Yaml.YamlFrontMatterBlock yaml:
                // Front matter as a table of its keys, as the preview shows it.
                var matterLines = yaml.Lines.Lines.Take(yaml.Lines.Count).Select(line => line.Slice.ToString()).ToList();
                if (FrontMatter.Read(matterLines) is { } matter)
                {
                    yield return YamlTable(matter);
                    yield return new Paragraph();
                }
                else
                {
                    foreach (var line in matterLines.Where(line => line.Trim() != "---"))
                    {
                        yield return Paragraph("Code", [Run(line, code: true)]);
                    }
                }
                break;
            case HtmlBlock html when html.Lines.ToString().Contains("<video", StringComparison.OrdinalIgnoreCase):
                // A video cannot play in Word: its name stands in its place.
                var videoSource = System.Text.RegularExpressions.Regex.Match(html.Lines.ToString(), @"srcs*=s*[""']([^""']+)").Groups[1].Value;
                yield return Paragraph(style, [Run($"▶ {VideoName(videoSource)}", bold: true)]);
                break;
            case HtmlBlock html when ImageTag.IsMatch(html.Lines.ToString()):
                // Pictures written as HTML (<p align="center"><img width="50%"></p>, as GitHub
                // READMEs size and place them) in one paragraph, aligned as the HTML says.
                var htmlSource = html.Lines.ToString();
                var tags = ImageTag.Matches(htmlSource).Select(match => match.Value).ToList();
                var placed = Paragraph(style, tags.Select(tag => (OpenXmlElement?)HtmlImage(tag) ?? Run($"［{HtmlAttribute(tag, "alt")}］")));
                var wrapper = System.Text.RegularExpressions.Regex.Match(htmlSource,
                    @"^\s*<(?:p|div)\b[^>]*\balign\s*=\s*[""']?(center|right|left)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var alignment = (wrapper.Success ? wrapper.Groups[1].Value : tags.Count == 1 ? HtmlAttribute(tags[0], "align") : null)?.ToLowerInvariant();
                if (alignment is "center" or "right")
                {
                    placed.ParagraphProperties ??= new ParagraphProperties();
                    placed.ParagraphProperties.Justification = new Justification
                    {
                        Val = alignment == "center" ? JustificationValues.Center : JustificationValues.Right
                    };
                }

                yield return placed;
                if (PlainTextExport.StripHtml(ImageTag.Replace(htmlSource, string.Empty)) is { } besideText)
                {
                    foreach (var line in besideText.Split('\n').Where(line => line.Trim().Length > 0))
                    {
                        yield return Paragraph(style, [Run(line.Trim())]);
                    }
                }
                break;
            case HtmlBlock html when AlignedText.Match(html.Lines.ToString()) is { Success: true } aligned
                                     && PlainTextExport.StripHtml(aligned.Groups["text"].Value) is { } alignedText:
                // A paragraph or heading placed with HTML (<p align="right">, <h1 align="center">,
                // as a PDF's letterhead or title opens) keeps its place, and a heading its level.
                var tag = aligned.Groups["tag"].Value.ToLowerInvariant();
                foreach (var line in alignedText.Split('\n').Where(line => line.Trim().Length > 0))
                {
                    var placedText = Paragraph(tag[0] == 'h' ? $"Heading{tag[1]}" : style, [Run(line.Trim())]);
                    placedText.ParagraphProperties ??= new ParagraphProperties();
                    placedText.ParagraphProperties.Justification = new Justification
                    {
                        Val = aligned.Groups["align"].Value.Equals("center", StringComparison.OrdinalIgnoreCase) ? JustificationValues.Center : JustificationValues.Right
                    };
                    yield return placedText;
                }

                break;
            case HtmlBlock html:
                // HTML written in Markdown keeps its text (Word cannot show the HTML itself).
                if (PlainTextExport.StripHtml(string.Join("\n", html.Lines.Lines.Take(html.Lines.Count).Select(line => line.Slice.ToString()))) is { } htmlText)
                {
                    foreach (var line in htmlText.Split('\n').Where(line => line.Trim().Length > 0))
                    {
                        yield return Paragraph(style, [Run(line.Trim())]);
                    }
                }
                break;
            case CodeBlock code:
                var lines = code.Lines.Lines.Take(code.Lines.Count).Select(line => line.Slice.ToString()).ToList();
                // The preview's colors are used only when the block reads the same, line by line.
                var colored = _codeIndex < _codeColors.Count ? _codeColors[_codeIndex] : null;
                _codeIndex++;
                var matches = colored is not null && colored.Count == lines.Count
                    && colored.Select(runs => string.Concat(runs.Select(run => run.Text))).SequenceEqual(lines);
                for (var i = 0; i < lines.Count; i++)
                {
                    yield return Paragraph("Code", matches
                        ? colored![i].Select(run => (OpenXmlElement)Run(run.Text, run.Bold, run.Italic, code: true, color: run.Color)).ToList()
                        : [Run(lines[i], code: true)]);
                }
                break;
            case ThematicBreakBlock:
                yield return new Paragraph(new ParagraphProperties(new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Size = 8, Space = 1, Color = "D0D0D0" })));
                break;
            case Markdig.Extensions.Alerts.AlertBlock alert:
                yield return Paragraph("Quote", [Run(DiagramFences.AlertTitle(alert), bold: true)]);
                foreach (var child in alert)
                {
                    foreach (var element in Blocks(child, level, "Quote"))
                    {
                        yield return element;
                    }
                }
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

    // Front matter as the preview shows it: a card of "key  value" rows (keys
    // in bold gray #59636E, 6.11:1 on white; a gray line on the left of the outer card,
    // since volt marks errors (Identity 4-3, 4-4 (3))), a list of words
    // joined with " · ", and nested maps and lists as cards inside the value
    // cell (each cell ends with a paragraph, as Word requires).
    private static WTable YamlTable(YamlValue value, bool nested = false)
    {
        DocumentFormat.OpenXml.Wordprocessing.TableCell Cell(YamlValue content, bool key = false)
        {
            var properties = new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto, Width = "0" });
            return content switch
            {
                YamlText text => new DocumentFormat.OpenXml.Wordprocessing.TableCell(properties,
                    Paragraph(null, [key ? Run(text.Text, bold: true, color: "59636E") : Run(text.Text.Replace('\n', ' '))])),
                YamlList list when list.Items.All(item => item is YamlText) => new DocumentFormat.OpenXml.Wordprocessing.TableCell(properties,
                    Paragraph(null, [Run(string.Join(" · ", list.Items.Cast<YamlText>().Select(item => item.Text.Replace('\n', ' '))))])),
                _ => new DocumentFormat.OpenXml.Wordprocessing.TableCell(properties, YamlTable(content, nested: true), new Paragraph())
            };
        }

        var rows = value switch
        {
            YamlMap map => map.Entries.Select(entry => new List<DocumentFormat.OpenXml.Wordprocessing.TableCell> { Cell(new YamlText(entry.Key), key: true), Cell(entry.Value) }).ToList(),
            YamlList list => list.Items.Select(item => new List<DocumentFormat.OpenXml.Wordprocessing.TableCell> { Cell(item) }).ToList(),
            _ => [[Cell(value)]]
        };
        var none = new Func<BorderType, BorderType>(border => { border.Val = BorderValues.None; return border; });
        var line = new Func<BorderType, BorderType>(border => { border.Val = BorderValues.Single; border.Size = 4; border.Color = "D0D0D0"; return border; });
        var table = new WTable(new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Auto, Width = "0" },
            new TableBorders(
                nested ? none(new TopBorder()) : line(new TopBorder()),
                nested ? none(new LeftBorder()) : new LeftBorder { Val = BorderValues.Single, Size = 24, Color = "59636E" },
                nested ? none(new BottomBorder()) : line(new BottomBorder()),
                nested ? none(new RightBorder()) : line(new RightBorder()),
                nested ? none(new InsideHorizontalBorder()) : line(new InsideHorizontalBorder()),
                none(new InsideVerticalBorder())),
            new TableCellMarginDefault(
                new TopMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
                new TableCellLeftMargin { Width = (short)(nested ? 0 : 140), Type = TableWidthValues.Dxa },
                new BottomMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
                new TableCellRightMargin { Width = 140, Type = TableWidthValues.Dxa })));
        var columns = Math.Max(1, rows.Max(row => row.Count));
        table.Append(new TableGrid(columns == 2
            ? [new GridColumn { Width = "2000" }, new GridColumn { Width = "7638" }]
            : [new GridColumn { Width = "9638" }]));
        foreach (var row in rows.Where(row => row.Count > 0))
        {
            table.Append(new DocumentFormat.OpenXml.Wordprocessing.TableRow(row));
        }

        return table;
    }

    private static string VideoName(string address) =>
        Uri.UnescapeDataString(address.Split('#', '?')[0].Split('/').Last());

    // A plain table of text, the first row bold.
    private static WTable TextTable(List<List<string>> rows)
    {
        var columns = Math.Max(1, rows.Max(row => row.Count));
        var result = new WTable(new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Auto, Width = "0" },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "1A1A1A" })));
        result.Append(new TableGrid(Enumerable.Range(0, columns).Select(_ => new GridColumn { Width = (9638 / columns).ToString() })));
        for (var r = 0; r < rows.Count; r++)
        {
            var tableRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            foreach (var text in rows[r])
            {
                tableRow.Append(new DocumentFormat.OpenXml.Wordprocessing.TableCell(Paragraph(null, [Run(text, bold: r == 0)])));
            }

            result.Append(tableRow);
        }

        return result;
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
                    result.Add(Image(image) ?? Run(System.Text.RegularExpressions.Regex.IsMatch(image.Url ?? string.Empty, @"\.(mp4|m4v|webm|ogv|mov)([?#].*)?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                        ? $"▶ {VideoName(image.Url!)}"
                        : $"［{PlainText(image)}］", bold, italic, strike));
                    break;
                case LinkInline link:
                    result.Add(Hyperlink(link.Url, Inlines(link, bold, italic, strike)));
                    break;
                case AutolinkInline autolink:
                    result.Add(Hyperlink(autolink.IsEmail ? "mailto:" + autolink.Url : autolink.Url, [Run(autolink.Url, bold, italic, strike)]));
                    break;
                case HtmlInline html when html.Tag.StartsWith("<video", StringComparison.OrdinalIgnoreCase):
                    // A video cannot play in Word: its name stands in its place.
                    result.Add(Run($"▶ {VideoName(System.Text.RegularExpressions.Regex.Match(html.Tag, @"srcs*=s*[""']([^""']+)").Groups[1].Value)}", bold: true));
                    break;
                case HtmlInline html when ImageTag.IsMatch(html.Tag):
                    result.Add(HtmlImage(html.Tag) ?? Run($"［{HtmlAttribute(html.Tag, "alt")}］", bold, italic, strike));
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

    private static Run Run(string text, bool bold = false, bool italic = false, bool strike = false, bool code = false, string? color = null)
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
        if (color is not null) properties.Color = new Color { Val = color };

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
        // A link to a heading in this document goes to that heading's bookmark.
        if (url is { Length: > 1 } && url[0] == (char)35 && _bookmarks.TryGetValue(Uri.UnescapeDataString(url[1..]), out var anchor))
        {
            var internalLink = new Hyperlink { Anchor = anchor, History = true };
            foreach (var run in content.OfType<Run>())
            {
                run.RunProperties ??= new RunProperties();
                run.RunProperties.RunStyle = new RunStyle { Val = "Hyperlink" };
                internalLink.Append(run);
            }

            return internalLink;
        }

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

    private static readonly System.Text.RegularExpressions.Regex ImageTag =
        new(@"<img\b[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    // One paragraph or heading of text placed with HTML: <p align="right">…</p>, <h1 align="center">…</h1>.
    private static readonly System.Text.RegularExpressions.Regex AlignedText =
        new(@"^\s*<(?<tag>p|h[1-6])\s+align\s*=\s*[""']?(?<align>center|right)[""']?\s*>(?<text>(?:(?!</?(?:p|h[1-6]|img)\b)[\s\S])*)</\k<tag>>\s*$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    // An attribute of an HTML tag, decoded ("a&amp;b" is "a&b"), or null.
    private static string? HtmlAttribute(string tag, string name)
    {
        var match = System.Text.RegularExpressions.Regex.Match(tag,
            $@"\s{name}\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'>]+))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success
            ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value)
            : null;
    }

    private Run? HtmlImage(string tag) =>
        HtmlAttribute(tag, "src") is { Length: > 0 } source
            ? Image(source, HtmlAttribute(tag, "alt") ?? string.Empty, HtmlAttribute(tag, "width"), HtmlAttribute(tag, "height"))
            : null;

    // A width or height as the HTML gives it: "50%" of the text width, or pixels ("300", "300px").
    private static long? HtmlLength(string? value)
    {
        var match = System.Text.RegularExpressions.Regex.Match(value?.Trim() ?? string.Empty, @"^(\d+(?:\.\d+)?)\s*(%|px)?$");
        if (!match.Success || !double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) || number <= 0)
        {
            return null;
        }

        return match.Groups[2].Value == "%" ? (long)(MaxWidthEmu * Math.Min(number, 100) / 100) : (long)(number * EmuPerPixel);
    }

    private Run? Image(LinkInline image) => Image(image.Url ?? string.Empty, PlainText(image), null, null);

    private Run? Image(string url, string description, string? widthAttribute, string? heightAttribute)
    {
        if (_resolveImage(url) is not { } path)
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
            // The size the HTML asks for; one side alone keeps the picture's proportions.
            long width = frame.PixelWidth * EmuPerPixel, height = frame.PixelHeight * EmuPerPixel;
            var askedWidth = HtmlLength(widthAttribute);
            var askedHeight = HtmlLength(heightAttribute is { } h && h.Trim().EndsWith('%') ? null : heightAttribute);
            if (askedWidth is { } w && askedHeight is { } hh)
            {
                (width, height) = (w, hh);
            }
            else if (askedWidth is { } onlyWidth)
            {
                (width, height) = (onlyWidth, height * onlyWidth / Math.Max(1, width));
            }
            else if (askedHeight is { } onlyHeight)
            {
                (width, height) = (width * onlyHeight / Math.Max(1, height), onlyHeight);
            }

            return Picture(bytes, type.Value, width, height, description);
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
