/*
  ファイル名 / File: PdfImport.cs
  概要 / Description: Opens a PDF as Markdown: text in reading order, headings guessed from size, lists, and images
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-24
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace MdEditor;

// A PDF records where each letter goes, not what is a heading, a paragraph, or a table.
// PdfPig (Apache 2.0, managed code only) reads the letters; text blocks are put in reading
// order, lines are joined into paragraphs, headings are guessed from text size
// (HeadingGuess), lines starting with a bullet or number become list items, lines in a
// monospaced typeface become code blocks, and pictures are written to "name_files". Ruled
// tables, figures, and bullets drawn as shapes are found from the drawings (PdfStructure.cs).
// Figures and math are drawn again as SVG pictures (PdfFigure.cs), and pictures keep their
// size and place (Picture). Column layout comes back only as its text.
// Nothing is sent anywhere, and the PDF itself is never written.
internal static partial class PdfImport
{
    public const int MaxPages = 500;

    public sealed record Result(string Markdown, int Images, int Pages, int ReadPages, bool HasText, int Headings);

    private sealed record Line(string Text, double Size, bool Bold, double Top, double Right, bool MathFont, string Family, double Left = 0, bool Rejoined = false);

    // A picture also keeps where it was (Left, Right, Top, Bottom), the width it has at its
    // own size in CSS pixels (Natural), its description (Alt), and the side text flows
    // around it on (Float); ClearAfter ends the flow after a paragraph.
    private sealed record Item(string Text, double Size, bool Bold, ItemKind Kind, double Top, int Page, bool Math = false, string Family = "", bool Toc = false, double Left = 0, double Advance = 0, int Frame = -1,
        double Right = 0, double Bottom = 0, double Natural = 0, string Alt = "", string Float = "", bool ClearAfter = false);

    // Code is one item per line; a Table item holds its Markdown.
    private enum ItemKind { Paragraph, Bullet, Numbered, Image, Code, Table }

    public static Result ToMarkdown(string path, string assets, string imagesName)
    {
        using var document = PdfDocument.Open(path);
        var items = new List<Item>();
        var images = 0;
        // Formulas in a line of text: each stands in the text as one private-use character,
        // replaced with its picture at the end.
        var inlines = new List<string>();
        // The body's column, for the size and place of pictures.
        double columnLeft = double.MaxValue, columnRight = double.MinValue;
        string SaveSvg(string svg)
        {
            images++;
            var file = $"image{images}.svg";
            Directory.CreateDirectory(Path.Combine(assets, imagesName));
            File.WriteAllText(Path.Combine(assets, imagesName, file), svg);
            return file;
        }

        var readPages = Math.Min(document.NumberOfPages, MaxPages);
        for (var number = 1; number <= readPages; number++)
        {
            var page = document.GetPage(number);
            var pageItems = new List<Item>();
            var rejoined = new HashSet<Word>(ReferenceEqualityComparer.Instance);
            var shapes = FindShapes(page);
            // The text of tables, figures, and formulas is read with them, not in the running text.
            // Some PDFs map glyphs to control characters (NUL); they show nothing.
            var letters = page.Letters.Where(letter => !letter.Value.All(char.IsControl) && !shapes.Tables.Any(table => Contains(table.Box, letter))
                && !shapes.Figures.Any(figure => Contains(figure, letter))
                && !shapes.Formulas.Any(formula => Contains(formula.Box, letter))).ToList();
            foreach (var formula in shapes.Formulas.Where(formula => !formula.Display))
            {
                var file = SaveSvg(FigureSvg(page, formula.Box, formula: true));
                inlines.Add($"![{EscapeAlt(FormulaText(formula))}]({Uri.EscapeDataString(imagesName)}/{file})");
                letters.Add(Placeholder(formula, (char)(0xE000 + inlines.Count - 1), page.Letters));
            }

            var blocks = Blocks(letters, rejoined).ToList();
            // A bullet set apart from its text, in a block of its own, marks the line it is
            // level with, as a bullet drawn as a shape does (even in the middle of a block).
            foreach (var bullet in blocks.Where(block => LoneBulletPattern().IsMatch(block.Text)).ToList())
            {
                shapes.Marks.Add(new Mark(bullet.BoundingBox, false, false));
                blocks.Remove(bullet);
            }

            var textWidth = blocks.Count == 0 ? 0 : blocks.Max(block => block.BoundingBox.Width);
            foreach (var block in blocks)
            {
                pageItems.AddRange(Paragraphs(block, number, textWidth, rejoined, shapes));
                columnLeft = Math.Min(columnLeft, block.BoundingBox.Left);
                columnRight = Math.Max(columnRight, block.BoundingBox.Right);
            }

            // A line on the same background as code (one in Japanese only, say) is code too,
            // and the code lines of one background are kept together.
            for (var i = 0; i < pageItems.Count; i++)
            {
                var item = pageItems[i];
                if (item.Kind != ItemKind.Paragraph || item.Toc)
                {
                    continue;
                }

                var frame = FrameAt(shapes, item.Left + 1, item.Top - item.Size / 2);
                if (frame >= 0 && pageItems.FirstOrDefault(other => other.Kind == ItemKind.Code && other.Frame == frame) is { } code)
                {
                    pageItems[i] = item with { Kind = ItemKind.Code, Advance = code.Advance, Frame = frame, Top = item.Top - item.Size * 0.85 };
                }
            }

            foreach (var frame in pageItems.Where(item => item.Kind == ItemKind.Code && item.Frame >= 0).Select(item => item.Frame).Distinct().ToList())
            {
                var first = pageItems.FindIndex(item => item.Kind == ItemKind.Code && item.Frame == frame);
                var group = pageItems.Where(item => item.Kind == ItemKind.Code && item.Frame == frame).ToList();
                pageItems.RemoveAll(item => item.Kind == ItemKind.Code && item.Frame == frame);
                pageItems.InsertRange(Math.Min(first, pageItems.Count), group);
            }

            // Each table and figure goes before the first text that starts below its top.
            void Place(Item item)
            {
                var at = pageItems.FindIndex(other => other.Kind != ItemKind.Image && other.Top < item.Top - 1);
                if (at < 0) pageItems.Add(item); else pageItems.Insert(at, item);
            }

            foreach (var table in shapes.Tables)
            {
                Place(new Item(TableMarkdown(table, page.Letters), 0, false, ItemKind.Table, table.Box.Top, number));
            }

            // A figure, and a formula on a line of its own, is drawn again as an SVG picture
            // (PdfFigure.cs) at the size it has on the page.
            foreach (var (box, alt, isFormula) in shapes.Figures.Select(figure => (figure, string.Empty, false))
                         .Concat(shapes.Formulas.Where(formula => formula.Display).Select(formula => (formula.Box, FormulaText(formula), true))))
            {
                Place(new Item(SaveSvg(FigureSvg(page, box, isFormula)), 0, false, ItemKind.Image, box.Top, number,
                    Left: box.Left, Right: box.Right, Bottom: box.Bottom, Natural: box.Width * 4 / 3, Alt: alt));
            }

            // A page number sits alone near the top or bottom edge; numbers elsewhere (a
            // table's figures) stay.
            var margin = page.Height * 0.08;
            pageItems.RemoveAll(item => IsPageNumber(item.Text) && (item.Top > page.Height - margin || item.Top < margin));

            // A long contents entry wraps: its first line has no leader dots and comes out as
            // a paragraph right above, starting at the same place. It is joined to its entry.
            for (var i = pageItems.Count - 1; i > 0; i--)
            {
                var entry = pageItems[i];
                var above = pageItems[i - 1];
                if (entry.Toc && !above.Toc && above.Kind == ItemKind.Paragraph && !above.Math
                    && Math.Abs(above.Left - entry.Left) <= 4
                    && above.Top - entry.Top > 0 && above.Top - entry.Top < Math.Max(entry.Size, 1) * 2.6)
                {
                    var cut = entry.Text.LastIndexOf(" …… ", StringComparison.Ordinal);
                    var title = JoinLines(new[] { above.Text, entry.Text[..cut] }.Where(part => part.Trim().Length > 0)).Trim();
                    pageItems[i] = entry with { Text = title + entry.Text[cut..], Top = above.Top, Left = above.Left };
                    pageItems.RemoveAt(i - 1);
                    i--;
                }
            }

            // A bullet set apart from its text can land in a block of its own; it goes back to
            // the paragraph that starts on the same line, which becomes a list item.
            foreach (var bullet in pageItems.Where(item => LoneBulletPattern().IsMatch(item.Text)).ToList())
            {
                var owner = pageItems.FindIndex(item => item.Kind == ItemKind.Paragraph && !item.Toc && !ReferenceEquals(item, bullet)
                    && !LoneBulletPattern().IsMatch(item.Text) && Math.Abs(item.Top - bullet.Top) < Math.Max(bullet.Size, 1) * 0.6);
                if (owner >= 0)
                {
                    pageItems[owner] = pageItems[owner] with { Kind = ItemKind.Bullet };
                }

                pageItems.Remove(bullet);
            }

            // A picture inside a figure is part of its SVG.
            foreach (var image in page.GetImages().Where(image => !shapes.Figures.Any(figure => Contains(figure, image.BoundingBox))))
            {
                if (SaveImage(image, assets, imagesName, images + 1) is { } file)
                {
                    images++;
                    // Placed before the first text that starts below the picture's top.
                    var bounds = image.BoundingBox;
                    var at = pageItems.FindIndex(item => item.Kind != ItemKind.Image && item.Top < bounds.Top - 1);
                    var picture = new Item(file, 0, false, ItemKind.Image, bounds.Top, number,
                        Left: bounds.Left, Right: bounds.Right, Bottom: bounds.Bottom, Natural: image.WidthInSamples);
                    if (at < 0) pageItems.Add(picture); else pageItems.Insert(at, picture);
                }
            }

            // Text beside a narrow picture flows around it: the picture keeps to the other
            // side, and the flow ends after the last paragraph that starts beside it.
            for (var i = 0; i < pageItems.Count; i++)
            {
                var picture = pageItems[i];
                if (picture.Kind != ItemKind.Image || picture.Right - picture.Left <= 0 || IsIcon(picture)
                    || picture.Right - picture.Left > (columnRight - columnLeft) * 0.6)
                {
                    continue;
                }

                // A paragraph's first line starts level with the picture (a little above it when
                // the picture has a margin) and beside it.
                var beside = pageItems.Select((item, index) => (item, index)).Where(pair => pair.item.Kind is ItemKind.Paragraph or ItemKind.Bullet or ItemKind.Numbered
                    && !pair.item.Toc && pair.item.Top <= picture.Top + Math.Max(pair.item.Size, 1) * 1.5 && pair.item.Top > picture.Bottom
                    && (pair.item.Left >= picture.Right - 2 || (pair.item.Right > 0 && pair.item.Right <= picture.Left + 2))).ToList();
                if (beside.Count == 0)
                {
                    continue;
                }

                // The picture goes right before the text that flows around it.
                pageItems.RemoveAt(i);
                var first = beside[0].index > i ? beside[0].index - 1 : beside[0].index;
                pageItems.Insert(first, picture with { Float = beside[0].item.Left >= picture.Right - 2 ? "left" : "right" });
                var last = pageItems.FindLastIndex(item => beside.Any(pair => ReferenceEquals(pair.item, item)));
                pageItems[last] = pageItems[last] with { ClearAfter = true };
            }

            items.AddRange(pageItems);
        }

        var text = items.Where(item => item.Kind != ItemKind.Image).ToList();
        // The body typeface is the one most of the text is set in; a heading set in another
        // (Gothic over Mincho, for example) stands out even at the body size.
        var bodyFamily = text.Where(item => item.Kind is not (ItemKind.Code or ItemKind.Table)).GroupBy(item => item.Family)
            .OrderByDescending(group => group.Sum(item => item.Text.Length))
            .Select(group => group.Key).FirstOrDefault() ?? string.Empty;
        var levels = HeadingGuess.Levels(text.Select(item => new SizedParagraph(
            item.Text, item.Size, item.Bold,
            item.Kind is ItemKind.Bullet or ItemKind.Code or ItemKind.Table || item.Math || item.Toc,
            item.Family.Length > 0 && item.Family != bodyFamily)).ToList());
        var headingOf = new Dictionary<Item, int>(ReferenceEqualityComparer.Instance);
        for (var i = 0; i < text.Count; i++)
        {
            headingOf[text[i]] = levels[i];
        }

        var tocDepth = TocDepths(items);
        var markdown = new StringBuilder();
        ItemKind? previous = null;
        var previousToc = false;
        // The open list items, outermost first: where each starts, and how far its text is
        // indented (a nested item is indented to its parent's text).
        var lists = new List<(double Left, int Indent)>();
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var level = headingOf.GetValueOrDefault(item);
            var kind = level > 0 ? ItemKind.Paragraph : item.Kind;
            if (kind == ItemKind.Code)
            {
                var end = index;
                while (end + 1 < items.Count && items[end + 1].Kind == ItemKind.Code && SameCodeBlock(items[end], items[end + 1]))
                {
                    end++;
                }

                if (markdown.Length > 0)
                {
                    markdown.Append("\n\n");
                }

                markdown.Append(CodeBlock(items.GetRange(index, end - index + 1)));
                index = end;
                previous = ItemKind.Code;
                previousToc = false;
                lists.Clear();
                continue;
            }

            // A video on the internet that mdEditor had not loaded is printed as a note with
            // its address; the address alone on a line shows it again.
            if (level == 0 && kind is ItemKind.Paragraph or ItemKind.Bullet && ExternalVideoPattern().Match(item.Text) is { Success: true } video)
            {
                kind = ItemKind.Paragraph;
                item = item with { Text = video.Groups["url"].Value };
            }

            if (kind is not (ItemKind.Bullet or ItemKind.Numbered))
            {
                lists.Clear();
            }
            var listContinues = ((previous is ItemKind.Bullet or ItemKind.Numbered) && kind == previous) || (item.Toc && previousToc);
            if (markdown.Length > 0)
            {
                markdown.Append(listContinues ? "\n" : "\n\n");
            }

            previousToc = item.Toc;
            if (item.Toc)
            {
                // A table of contents is one list, nested as the PDF indents it. "1." is
                // written "1\." so Markdown keeps it as text, not as a numbered list.
                markdown.Append(new string(' ', 2 * tocDepth[item])).Append("- ").Append(EscapeListNumber(item.Text));
                previous = ItemKind.Paragraph;
                continue;
            }

            switch (kind)
            {
                case ItemKind.Image:
                    markdown.Append(Picture(item, $"{Uri.EscapeDataString(imagesName)}/{item.Text}", columnLeft, columnRight));
                    break;
                case ItemKind.Bullet:
                case ItemKind.Numbered:
                    while (lists.Count > 0 && lists[^1].Left >= item.Left - 4)
                    {
                        lists.RemoveAt(lists.Count - 1);
                    }

                    var indent = lists.Count == 0 ? 0 : lists[^1].Indent;
                    var marker = kind == ItemKind.Bullet ? "- " : item.Text[..(item.Text.IndexOf('.') + 2)];
                    markdown.Append(new string(' ', indent))
                        .Append(kind == ItemKind.Bullet ? "- " + Escape(item.Text) : item.Text);
                    lists.Add((item.Left, indent + marker.Length));
                    break;
                case ItemKind.Table:
                    markdown.Append(item.Text);
                    break;
                default:
                    markdown.Append(level > 0 ? new string('#', level) + " " + item.Text : Escape(item.Text));
                    break;
            }

            if (item.ClearAfter)
            {
                markdown.Append("\n\n<br clear=\"all\">");
            }

            previous = kind;
        }

        for (var i = 0; i < inlines.Count; i++)
        {
            markdown.Replace(((char)(0xE000 + i)).ToString(), inlines[i]);
        }

        var headings = levels.Count(level => level > 0);
        return new Result(markdown.Length > 0 ? markdown.Append('\n').ToString() : string.Empty,
            images, document.NumberOfPages, readPages, text.Count > 0, headings);
    }

    // A picture as Markdown, or as HTML when it is shown at another size than its own (the
    // width as a share of the body's column, or in pixels), centered, at the right, or with
    // text flowing around it.
    private static string Picture(Item item, string source, double columnLeft, double columnRight)
    {
        var column = columnRight - columnLeft;
        var width = item.Right - item.Left;
        if (width <= 0 || column <= 0 || IsIcon(item))
        {
            return $"![{EscapeAlt(item.Alt)}]({source})";
        }

        var shown = width * 4 / 3;
        var percent = width / column * 100;
        var nearest = Math.Round(percent / 5) * 5;
        var size = item.Natural <= 0 || Math.Abs(shown - item.Natural) <= item.Natural * 0.05 || percent >= 90 ? null
            : Math.Abs(percent - nearest) < 1.5 ? $"{nearest}%" : $"{Math.Round(shown)}";
        var (left, right) = (item.Left - columnLeft, columnRight - item.Right);
        var align = item.Float.Length > 0 ? null
            : left > column * 0.05 && Math.Abs(left - right) < column * 0.05 ? "center"
            : left > column * 0.1 && right < column * 0.03 ? "right"
            : null;
        if (size is null && align is null && item.Float.Length == 0)
        {
            return $"![{EscapeAlt(item.Alt)}]({source})";
        }

        // Only what HTML needs escaped (µ and the like stay readable).
        var alt = item.Alt.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        var image = $"<img src=\"{source}\" alt=\"{alt}\"" + (size is null ? string.Empty : $" width=\"{size}\"")
            + (item.Float.Length > 0 ? $" align=\"{item.Float}\" hspace=\"16\"" : string.Empty) + ">";
        return align is null ? image : $"<p align=\"{align}\">{image}</p>";
    }

    // A picture no taller than a line or so (an icon, an emoji drawn as a picture) stays in
    // the text as it is.
    private static bool IsIcon(Item picture) => picture.Top - picture.Bottom < 24 && picture.Alt.Length == 0;

    private static string EscapeAlt(string alt) => alt.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");

    // A formula's letters left to right (zero-width ones left out), as its description.
    private static string FormulaText(Formula formula) => Normalize(string.Concat(formula.Letters
        .Where(letter => letter.BoundingBox.Width > 0.5 && !IsBlank(letter))
        .OrderBy(letter => letter.StartBaseLine.X).Select(letter => letter.Value)));

    // The one letter an inline formula stands as: as wide as the formula, on the baseline
    // and in the typeface and size of the text next to it.
    private static Letter Placeholder(Formula formula, char mark, IEnumerable<Letter> letters)
    {
        var box = formula.Box;
        var near = letters.Where(letter => !IsMathLetter(letter) && letter.BoundingBox.Width > 0.5 && !IsBlank(letter)
                && letter.GetFont() is not null && !Contains(box, letter) && letter.BoundingBox.Bottom < box.Top && letter.BoundingBox.Top > box.Bottom)
            .OrderBy(letter => Math.Min(Math.Abs(letter.BoundingBox.Left - box.Right), Math.Abs(box.Left - letter.BoundingBox.Right)))
            .FirstOrDefault() ?? formula.Letters.Where(letter => letter.GetFont() is not null).OrderByDescending(letter => letter.PointSize).FirstOrDefault()
            ?? formula.Letters[0];
        var baseline = near.StartBaseLine.Y;
        var glyph = new PdfRectangle(box.Left, baseline, box.Right, baseline + near.PointSize * 0.7);
        return new Letter(mark.ToString(), glyph, glyph, new PdfPoint(box.Left, baseline), new PdfPoint(box.Right, baseline),
            box.Width, near.FontSize, near.GetFont()!, near.RenderingMode, near.StrokeColor, near.FillColor, near.PointSize, near.TextSequence);
    }

    // Text blocks of a page in reading order (columns are read one after another).
    private static IEnumerable<TextBlock> Blocks(IReadOnlyList<Letter> letters, HashSet<Word> rejoined)
    {
        if (letters.Count == 0)
        {
            return [];
        }

        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters).ToList();
        var blocks = JoinSplitLines(DocstrumBoundingBoxes.Instance.GetBlocks(words).ToList(), rejoined);
        return UnsupervisedReadingOrderDetector.Instance.Get(blocks);
    }

    // A line with a wide gap ("File Number: E46769   Type dsg: 1494V-0") can be cut into two
    // blocks, and its right part would then be read after the lines below its left part. A
    // block of one line that goes on from a line of another block, on the same baseline and
    // across a gap narrower than a column gap (2.5 line heights), goes back into that block.
    // Its words are added to rejoined.
    private static List<TextBlock> JoinSplitLines(List<TextBlock> blocks, HashSet<Word> rejoined)
    {
        for (var i = 0; i < blocks.Count; i++)
        {
            var piece = SplitLines(blocks[i]);
            // A lone bullet is put back with its text later, as a list item.
            if (piece.Count != 1 || LoneBulletPattern().IsMatch(ReadLine(piece[0]).Text))
            {
                continue;
            }

            var bottom = piece[0][0].BoundingBox.Bottom;
            var left = piece[0].Min(word => word.BoundingBox.Left);
            var height = Math.Max(piece[0].Max(word => word.BoundingBox.Height), 1);
            var owner = blocks.FindIndex(block => !ReferenceEquals(block, blocks[i]) && SplitLines(block).Any(line =>
            {
                var gap = left - line.Max(word => word.BoundingBox.Right);
                return Math.Abs(line[0].BoundingBox.Bottom - bottom) < height * 0.5 && gap > 0 && gap < height * 2.5
                    && !LoneBulletPattern().IsMatch(ReadLine(line).Text);
            }));
            if (owner < 0)
            {
                continue;
            }

            // Only a piece that reaches past the block's right edge makes a form line (or a
            // table row); a piece inside a paragraph's width is part of a wrapped line.
            if (piece[0].Max(word => word.BoundingBox.Right) > blocks[owner].BoundingBox.Right + height)
            {
                rejoined.UnionWith(piece[0]);
            }

            blocks[owner] =new TextBlock(blocks[owner].TextLines.Concat(blocks[i].TextLines).ToList());
            blocks.RemoveAt(i);
            i = -1;
        }

        return blocks;
    }

    // The paragraphs of a block: a new one starts where the size changes, a line starts a
    // list item, or the line before ended well short of the block's right edge (so it was
    // not wrapped: the last line of a paragraph, or a short item such as a checklist line).
    // In a block much narrower than the page's text (a list of short items, table cells),
    // every line stands alone. Wrapped lines are joined.
    // A line in a monospaced typeface is a line of code: it keeps its spaces and stands
    // alone (CodeBlock puts the lines back together).
    private static IEnumerable<Item> Paragraphs(TextBlock block, int page, double textWidth, HashSet<Word> rejoined, PageShapes shapes)
    {
        var split = SplitLines(block);
        var lines = split.Select(words => ReadLine(words) with { Rejoined = words.Any(rejoined.Contains) }).ToList();
        // On a code background, a line of Japanese with a monospaced start (a diff's "- 保存") is code.
        var code = split.Select(words => CodeLine(words, shapes.CodeFrames.Contains(FrameAt(shapes, words.Min(word => word.BoundingBox.Left) + 1, words.Max(word => word.BoundingBox.Top) - 1)))).ToList();
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Text.Length == 0)
            {
                lines.RemoveAt(i);
                code.RemoveAt(i);
            }
        }
        var blockRight = block.BoundingBox.Right;
        var shortLine = block.BoundingBox.Width < textWidth * 0.5 ? -1 : block.BoundingBox.Width * 0.2;
        Line? previousLine = null;
        // A formula is set in pieces (a fraction's numerator and denominator are separate
        // lines, larger than the text): a line with math, and a same-sized line right next to
        // one, is never a heading.
        var own = lines.Select(line => line.MathFont || LooksLikeMath(line.Text)).ToList();
        bool SameSize(int a, int b) => Math.Abs(lines[a].Size - lines[b].Size) <= lines[a].Size * 0.1;
        for (var i = 0; i < lines.Count; i++)
        {
            var math = own[i] || (i > 0 && own[i - 1] && SameSize(i, i - 1)) || (i + 1 < lines.Count && own[i + 1] && SameSize(i, i + 1));
            lines[i] = lines[i] with { MathFont = math };
        }
        var current = new List<Line>();
        var kind = ItemKind.Paragraph;
        double firstSize = 0, firstTop = 0;
        var firstBold = false;
        var firstFamily = string.Empty;
        double firstLeft = 0;

        IEnumerable<Item> Flush()
        {
            if (current.Count == 0)
            {
                yield break;
            }

            var text = JoinLines(current.Select(line => line.Text));
            var mathInItem = current.Any(line => line.MathFont);
            // Where the first line ends, for text that flows around a picture.
            var right = current[0].Right;
            current.Clear();
            yield return new Item(text, firstSize, firstBold, kind, firstTop, page, mathInItem, firstFamily, Left: firstLeft, Right: right);
        }

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            if (code[index] is { } codeLine)
            {
                foreach (var item in Flush()) yield return item;
                // Code lines are spaced by their baselines (letters of other typefaces stand taller).
                yield return new Item(codeLine.Text, line.Size, false, ItemKind.Code, codeLine.Baseline, page, Family: line.Family, Left: line.Left, Advance: codeLine.Advance,
                    Frame: FrameAt(shapes, line.Left + 1, line.Top - line.Size / 2));
                previousLine = null;
                continue;
            }

            // A table-of-contents line ("3.1 Definitions ........ 6") reaches the right edge
            // because of its leader dots, so it would look wrapped; it stands alone, with the
            // dots shortened, and is never a heading.
            var leader = LeaderPattern().Match(line.Text);
            if (leader.Success)
            {
                foreach (var item in Flush()) yield return item;
                var entry = line.Text[..leader.Index].TrimEnd() + " …… " + leader.Groups[1].Value;
                yield return new Item(entry, line.Size, line.Bold, ItemKind.Paragraph, line.Top, page, Family: line.Family, Toc: true, Left: line.Left);
                previousLine = null;
                continue;
            }

            var (lineKind, rest) = ListMarker(line.Text);
            // A bullet or a check box drawn as a shape.
            if (lineKind == ItemKind.Paragraph && MarkBefore(shapes, line.Left, line.Top, line.Size) is { } mark)
            {
                (lineKind, rest) = (ItemKind.Bullet, mark.CheckBox ? (mark.Checked ? "[x] " : "[ ] ") + line.Text : line.Text);
            }
            var sizeChanged = current.Count > 0 && Math.Abs(line.Size - firstSize) > firstSize * 0.1;
            var fontChanged = current.Count > 0 && line.Family != firstFamily;
            var endedEarly = previousLine is not null && blockRight - previousLine.Right > shortLine;
            // A line put back together across a wide gap is a form line or a table row, not
            // prose: it stands alone.
            var standsAlone = line.Rejoined || previousLine?.Rejoined == true;
            if (current.Count > 0 && (sizeChanged || fontChanged || endedEarly || standsAlone || lineKind != ItemKind.Paragraph))
            {
                foreach (var item in Flush()) yield return item;
            }

            previousLine = line;

            if (current.Count == 0)
            {
                kind = lineKind;
                firstSize = line.Size;
                firstBold = line.Bold;
                firstTop = line.Top;
                firstFamily = line.Family;
                firstLeft = line.Left;
            }

            current.Add(line with { Text = lineKind == ItemKind.Bullet ? rest : line.Text });
        }

        foreach (var item in Flush()) yield return item;
    }

    // The lines of a block, from its words: words whose baselines are within half a line
    // height of each other share a line, read left to right. PdfPig's own text lines can
    // run a heading and the lines below it together in Japanese, where each character
    // may count as a word.
    private static List<List<Word>> SplitLines(TextBlock block)
    {
        var lines = new List<List<Word>>();
        foreach (var word in block.TextLines.SelectMany(line => line.Words)
                     .Where(word => word.Text.Trim().Length > 0)
                     .OrderByDescending(word => word.BoundingBox.Bottom))
        {
            var height = Math.Max(word.BoundingBox.Height, 1);
            var line = lines.LastOrDefault();
            if (line is not null && Math.Abs(line[0].BoundingBox.Bottom - word.BoundingBox.Bottom) < height * 0.5)
            {
                line.Add(word);
            }
            else
            {
                lines.Add([word]);
            }
        }

        foreach (var line in lines)
        {
            line.Sort((a, b) => a.BoundingBox.Left.CompareTo(b.BoundingBox.Left));
        }

        return lines;
    }

    private static Line ReadLine(List<Word> words)
    {
        var builder = new StringBuilder();
        string? last = null;
        foreach (var word in words)
        {
            var text = word.Text.Trim();
            if (text.Length == 0)
            {
                continue;
            }

            // No space between Japanese or Chinese characters, before closing punctuation,
            // or after an opening bracket.
            if (last is not null && !(IsCjk(last[^1]) && IsCjk(text[0])) && !".,;:!?)]}%".Contains(text[0]) && !"([{".Contains(last[^1]))
            {
                builder.Append(' ');
            }

            builder.Append(text);
            last = text;
        }

        var letters = words.SelectMany(word => word.Letters).Where(letter => !IsBlank(letter)).ToList();
        var size = letters.Count == 0 ? 0 : letters.Select(letter => letter.PointSize).OrderBy(value => value).ElementAt(letters.Count / 2);
        var bold = letters.Count > 0 && letters.Count(IsBold) * 2 > letters.Count;
        var mathFont = letters.Any(letter => (letter.FontName ?? string.Empty).Contains("KaTeX", StringComparison.OrdinalIgnoreCase)
            || (letter.FontName ?? string.Empty).Contains("Math", StringComparison.OrdinalIgnoreCase));
        var family = letters.Count == 0 ? string.Empty : letters.GroupBy(letter => Family(letter.FontName))
            .OrderByDescending(group => group.Count()).First().Key;
        return new Line(Normalize(builder.ToString()), size, bold, words.Max(word => word.BoundingBox.Top), words.Max(word => word.BoundingBox.Right), mathFont, family,
            words.Min(word => word.BoundingBox.Left));
    }

    private sealed record CodeText(string Text, double Advance, double Baseline);

    // A line set (almost) all in a monospaced typeface, with its spaces put back from where
    // the letters stand (one space per letter width); null for any other line.
    private static CodeText? CodeLine(List<Word> words, bool framed)
    {
        var letters = words.SelectMany(word => word.Letters).Where(letter => !IsBlank(letter))
            .OrderBy(letter => letter.StartBaseLine.X).ToList();
        // Japanese and Chinese in code (comments, strings) are set in another typeface. A line
        // of prose with code words in it starts with prose or is mostly Japanese or Chinese (a
        // long comment on a code background is code all the same; see ToMarkdown).
        var latin = letters.Where(letter => !letter.Value.Any(IsCjk)).ToList();
        if (latin.Count == 0 || latin.Count(letter => IsMonospaced(letter.FontName)) < latin.Count * 0.8
            || !IsMonospaced(letters[0].FontName) || (!framed && letters.Count - latin.Count > latin.Count))
        {
            return null;
        }

        var widths = letters.Where(letter => letter.Value.Length == 1 && letter.Value[0] < '⺀')
            .Select(letter => letter.EndBaseLine.X - letter.StartBaseLine.X).Where(width => width > 0).OrderBy(width => width).ToList();
        var advance = widths.Count > 0 ? widths[widths.Count / 2] : letters[0].PointSize * 0.55;
        var builder = new StringBuilder();
        double? end = null;
        foreach (var letter in letters)
        {
            if (end is { } x && (int)Math.Round((letter.StartBaseLine.X - x) / advance) is var spaces and > 0)
            {
                builder.Append(' ', spaces);
            }

            builder.Append(letter.Value);
            end = letter.EndBaseLine.X;
        }

        return new CodeText(Normalize(builder.ToString()), advance, letters[0].StartBaseLine.Y);
    }

    private static bool IsMonospaced(string? fontName)
    {
        var name = Family(fontName).Replace(" ", string.Empty);
        return name.Contains("mono") || name.Contains("consolas") || name.Contains("courier") || name.Contains("menlo")
            || name.Contains("monaco") || name.Contains("lucidaconsole") || name.Contains("sourcecode") || name.Contains("cascadia")
            || name.Contains("inconsolata") || name.Contains("firacode") || name.Contains("jetbrains");
    }

    // Code lines belong to one block when they are on the same background, follow each
    // other closely (or are pieces of one line), or go on at the top of the next page.
    private static bool SameCodeBlock(Item above, Item below) =>
        below.Page == above.Page + 1
        || (below.Page == above.Page && (above.Frame >= 0 ? below.Frame == above.Frame
            : above.Top - below.Top is > -1 and var gap && gap < Math.Max(above.Size, 1) * 3.2));

    // The lines of a code block in a fence, indented by how far right each starts; a wider
    // gap between lines is an empty line. The fence is "~~~~" when the code holds "```".
    private static string CodeBlock(List<Item> lines)
    {
        var left = lines.Min(line => line.Left);
        var advance = Math.Max(lines.Select(line => line.Advance).Where(width => width > 0).DefaultIfEmpty(1).Min(), 0.1);
        var fence = lines.Any(line => line.Text.Contains("```")) ? "~~~~" : "```";
        var builder = new StringBuilder(fence).Append('\n');
        // Pieces of one line (cut apart at wide gaps) go back together, each at its column.
        var rows = new List<List<Item>>();
        foreach (var line in lines.OrderBy(line => line.Page).ThenByDescending(line => line.Top))
        {
            if (rows.Count > 0 && rows[^1][0].Page == line.Page && Math.Abs(rows[^1][0].Top - line.Top) < 1)
            {
                rows[^1].Add(line);
            }
            else
            {
                rows.Add([line]);
            }
        }

        for (var i = 0; i < rows.Count; i++)
        {
            if (i > 0 && rows[i][0].Page == rows[i - 1][0].Page && rows[i - 1][0].Top - rows[i][0].Top > Math.Max(rows[i][0].Size, 1) * 1.9)
            {
                builder.Append('\n');
            }

            var row = new StringBuilder();
            foreach (var piece in rows[i].OrderBy(piece => piece.Left))
            {
                var column = Math.Max(0, (int)Math.Round((piece.Left - left) / advance));
                row.Append(' ', Math.Max(row.Length == 0 ? column : 1, column - row.Length)).Append(piece.Text);
            }

            builder.Append(row).Append('\n');
        }

        return builder.Append(fence).ToString();
    }

    // A typeface without its subset prefix ("ABCDEF+") and weight or style ("-Bold",
    // ",Italic", "MT"), so "Arial-BoldMT" and "ArialMT" are both "arial".
    private static string Family(string? fontName)
    {
        var name = fontName ?? string.Empty;
        var plus = name.IndexOf('+');
        if (plus is > 0 and <= 7) name = name[(plus + 1)..];
        name = FontStylePattern().Replace(name, string.Empty);
        return name.ToLowerInvariant();
    }

    private static bool IsBold(Letter letter) =>
        letter.FontDetails?.IsBold == true
        || (letter.FontName ?? string.Empty).Contains("Bold", StringComparison.OrdinalIgnoreCase)
        || (letter.FontName ?? string.Empty).Contains("Heavy", StringComparison.OrdinalIgnoreCase)
        || (letter.FontName ?? string.Empty).Contains("Black", StringComparison.OrdinalIgnoreCase);

    // Wrapped lines become one paragraph: Japanese and Chinese join directly, other text
    // with a space, and a word broken with a hyphen at the line end is put back together.
    private static string JoinLines(IEnumerable<string> lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            if (builder.Length == 0)
            {
                builder.Append(line);
            }
            else if (builder[^1] == '-' && builder.Length > 1 && char.IsLetter(builder[^2]) && char.IsLower(line[0]))
            {
                builder.Length--;
                builder.Append(line);
            }
            else
            {
                builder.Append(IsCjk(builder[^1]) && IsCjk(line[0]) ? string.Empty : " ").Append(line);
            }
        }

        return builder.ToString();
    }

    private static (ItemKind Kind, string Remainder) ListMarker(string text)
    {
        var bullet = BulletPattern().Match(text);
        if (bullet.Success)
        {
            return (ItemKind.Bullet, text[bullet.Length..]);
        }

        var numbered = NumberedPattern().Match(text);
        return numbered.Success ? (ItemKind.Numbered, $"{numbered.Groups[1].Value}. {text[numbered.Length..]}") : (ItemKind.Paragraph, text);
    }

    // PDFs made by browsers often map kanji to the look-alike radicals of the Kangxi and
    // CJK Radicals blocks (方 as ⽅, U+2F45); compatibility normalization puts the usual
    // character back. Other text (full-width digits, for example) is left as it is.
    // Ligatures (ﬁ, ﬃ) are spelled out the same way. A few radicals of the CJK Radicals
    // Supplement block have no compatibility mapping; they are mapped by hand.
    private static string Normalize(string text)
    {
        if (!text.Any(c => c is >= '⺀' and <= '⿟' or >= 'ﬀ' and <= 'ﬆ'))
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (Radicals.TryGetValue(c, out var kanji))
            {
                builder.Append(kanji);
            }
            else if (c is >= '⺀' and <= '⿟' or >= 'ﬀ' and <= 'ﬆ')
            {
                builder.Append(c.ToString().Normalize(NormalizationForm.FormKC));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private static readonly Dictionary<char, char> Radicals = new()
    {
        ['⻄'] = '西', ['⻆'] = '角', ['⻌'] = '辶', ['⻑'] = '長', ['⻘'] = '青',
        ['⻝'] = '食', ['⻣'] = '骨', ['⻤'] = '鬼', ['⻨'] = '麦', ['⻩'] = '黄',
        ['⻫'] = '斉', ['⻭'] = '歯', ['⻯'] = '竜', ['⻲'] = '亀'
    };

    // Large text with math symbols is a formula, not a heading.
    private static bool LooksLikeMath(string text) => text.IndexOfAny(['=', '−', '±', '×', '÷', '√', '∑', '∫', '≤', '≥', '≠', '≈']) >= 0;

    // A line that is only a page number ("3", "- 3 -", "3 / 10") is left out.
    private static bool IsPageNumber(string text) => PageNumberPattern().IsMatch(text);

    // The depth of each table-of-contents line within its run of consecutive lines: the
    // leftmost lines are 0, and each further step to the right one more (positions within
    // 4 points count as the same step).
    private static Dictionary<Item, int> TocDepths(List<Item> items)
    {
        var depths = new Dictionary<Item, int>(ReferenceEqualityComparer.Instance);
        for (var start = 0; start < items.Count; start++)
        {
            if (!items[start].Toc)
            {
                continue;
            }

            var end = start;
            while (end + 1 < items.Count && items[end + 1].Toc)
            {
                end++;
            }

            var run = items.GetRange(start, end - start + 1);
            var steps = new List<double>();
            foreach (var left in run.Select(item => item.Left).OrderBy(left => left))
            {
                if (steps.Count == 0 || left - steps[^1] > 4)
                {
                    steps.Add(left);
                }
            }

            foreach (var item in run)
            {
                depths[item] = Math.Max(0, steps.FindLastIndex(step => item.Left >= step - 4));
            }

            start = end;
        }

        return depths;
    }

    // "1. Introduction" would start a numbered list; "1\." keeps the number as text.
    private static string EscapeListNumber(string text)
    {
        var match = ListNumberPattern().Match(text);
        return match.Success ? text[..(match.Length - 1)] + "\\" + text[(match.Length - 1)..] : Escape(text);
    }

    // Markdown would read these at the start of a line as syntax.
    private static string Escape(string text) =>
        text.Length > 0 && "#>-+*|".Contains(text[0]) ? "\\" + text : text;

    private static bool IsCjk(char c) =>
        c is >= '　' and <= 'ヿ' or >= '㐀' and <= '䶿' or >= '一' and <= '鿿'
            or >= '豈' and <= '﫿' or >= '＀' and <= '￯';

    private static string? SaveImage(IPdfImage image, string assets, string imagesName, int number)
    {
        string extension;
        byte[] bytes;
        if (image.TryGetPng(out var png))
        {
            (extension, bytes) = (".png", png);
        }
        else if (image.RawMemory.Length > 3 && image.RawMemory.Span[0] == 0xFF && image.RawMemory.Span[1] == 0xD8)
        {
            (extension, bytes) = (".jpg", image.RawMemory.ToArray());
        }
        else
        {
            return null;
        }

        var file = $"image{number}{extension}";
        Directory.CreateDirectory(Path.Combine(assets, imagesName));
        File.WriteAllBytes(Path.Combine(assets, imagesName, file), bytes);
        return file;
    }

    [GeneratedRegex(@"^\s*[•●○◦▪■□・·‣⁃\-–*]\s+")]
    private static partial Regex BulletPattern();

    // mdEditor's note for an internet video it has not loaded, in each language.
    [GeneratedRegex(@"^\s*[［\[](?:外部の動画|外部视频|Internet video)\s*[:：]\s*(?<url>https?://[^\s（(]+)\s*[（(][^）)]*[）)][。，,]?\s*(?:読み込んでいません|未加载|not loaded)\s*[］\]]\s*$")]
    private static partial Regex ExternalVideoPattern();

    [GeneratedRegex(@"^\s*(\d{1,3})[.)．）]\s+")]
    private static partial Regex NumberedPattern();

    [GeneratedRegex(@"^[\s\-–—]*\d{1,4}(\s*/\s*\d{1,4})?[\s\-–—]*$")]
    private static partial Regex PageNumberPattern();

    // A number, a period or parenthesis, and a space: Markdown's numbered-list marker.
    [GeneratedRegex(@"^[0-9]{1,9}[.)](?=\s)")]
    private static partial Regex ListNumberPattern();

    [GeneratedRegex(@"^\s*[•●○◦▪■□・·‣⁃]\s*$")]
    private static partial Regex LoneBulletPattern();

    // Leader dots (or ・ ‥ …) and a page number at the end of a table-of-contents line.
    [GeneratedRegex(@"\s*(?:[.．・‥…]\s*){4,}([0-9０-９]{1,4})\s*$")]
    private static partial Regex LeaderPattern();

    [GeneratedRegex(@"([-,]?(Bold|Italic|Oblique|Regular|Medium|Light|Semibold|SemiBold|Black|Heavy|Book|Roman)|PSMT|MT|PS)+$", RegexOptions.IgnoreCase)]
    private static partial Regex FontStylePattern();
}
