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
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig.Graphics.Colors;

namespace MdEditor;

// A PDF records where each letter goes, not what is a heading, a paragraph, or a table.
// PdfPig (Apache 2.0, managed code only) reads the letters; text blocks are put in reading
// order, lines are joined into paragraphs, headings are guessed from text size
// (HeadingGuess), lines starting with a bullet or number become list items, lines in a
// monospaced typeface become code blocks, and pictures are written to "name_files". Ruled
// tables, figures, and bullets drawn as shapes are found from the drawings (PdfStructure.cs).
// Figures and math are drawn again as SVG pictures (PdfFigure.cs), and pictures keep their
// size and place (Picture). A tagged PDF's tags decide the reading order, paragraphs,
// headings, list items, and tables without borders where they hold a page's text
// (PdfTags.cs). Column layout comes back only as its text. A page whose text cannot be
// read in order (vertical writing, text set over text) comes back as one picture of the
// whole page (PicturePage).
// Nothing is sent anywhere, and the PDF itself is never written.
internal static partial class PdfImport
{
    public const int MaxPages = 500;

    // PicturePages: the pages that came back as a picture of the whole page.
    public sealed record Result(string Markdown, int Images, int Pages, int ReadPages, bool HasText, int Headings, IReadOnlyList<int> PicturePages);

    private sealed record Line(string Text, double Size, bool Bold, double Top, double Right, bool MathFont, string Family, double Left = 0, bool Rejoined = false);

    // A picture also keeps where it was (Left, Right, Top, Bottom), the width it has at its
    // own size in CSS pixels (Natural), its description (Alt), and the side text flows
    // around it on (Float); ClearAfter ends the flow after a paragraph. Heading is the level
    // the tags give (0 for text the tags say is no heading, -1 on a page without tags).
    private sealed record Item(string Text, double Size, bool Bold, ItemKind Kind, double Top, int Page, bool Math = false, string Family = "", bool Toc = false, double Left = 0, double Advance = 0, int Frame = -1,
        double Right = 0, double Bottom = 0, double Natural = 0, string Alt = "", string Float = "", bool ClearAfter = false, int Heading = -1);

    // Code is one item per line; a Table item holds its Markdown.
    private enum ItemKind { Paragraph, Bullet, Numbered, Image, Code, Table }

    public static Result ToMarkdown(string path, string assets, string imagesName)
    {
        using var document = PdfDocument.Open(path);
        var structure = ReadStructure(document);
        var items = new List<Item>();
        var picturePages = new List<int>();
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
            // How each shape was drawn (see-through, scaling), read only for a page with figures or formulas.
            var states = new Lazy<IReadOnlyList<PaintState>?>(() => PathStates(document, page));
            var tags = structure is null ? null : ReadPageTags(structure, page);
            // The text of tables, figures, and formulas is read with them, not in the running text.
            // Some PDFs map glyphs to control characters (NUL); they show nothing. What the tags
            // mark as not the text (running titles, page numbers) is left out.
            var letters = page.Letters.Where(letter => !letter.Value.All(char.IsControl) && !shapes.Tables.Any(table => Contains(table.Box, letter))
                && !shapes.Figures.Any(figure => Contains(figure, letter))
                && !shapes.Formulas.Any(formula => Contains(formula.Box, letter)) && tags?.IsFurniture(letter) != true).ToList();
            if (PicturePage(letters))
            {
                var whole = new PdfRectangle(0, 0, page.Width, page.Height);
                items.Add(new Item(SaveSvg(FigureSvg(page, whole, states.Value)), 0, false, ItemKind.Image, page.Height, number,
                    Left: 0, Right: page.Width, Bottom: 0, Natural: page.Width * 4 / 3));
                picturePages.Add(number);
                continue;
            }

            var taggedTables = tags is null ? [] : TaggedTables(tags, letters);
            foreach (var table in taggedTables)
            {
                var cells = new HashSet<Letter>(table.Letters, ReferenceEqualityComparer.Instance);
                letters.RemoveAll(cells.Contains);
            }

            letters = InlineMarkup(letters, page);
            foreach (var formula in shapes.Formulas.Where(formula => !formula.Display))
            {
                var file = SaveSvg(FigureSvg(page, formula.Box, states.Value, formula: true));
                inlines.Add($"![{EscapeAlt(FormulaText(formula))}]({Uri.EscapeDataString(imagesName)}/{file})");
                letters.Add(Placeholder(formula, (char)(0xE000 + inlines.Count - 1), page.Letters));
            }

            // A card's frame, not a code background or one as tall as the page (a browser's page background).
            var cards = shapes.Frames.Where((frame, index) => !shapes.CodeFrames.Contains(index) && frame.Height < page.Height * 0.5).ToList();
            var blocks = Blocks(letters, rejoined, tags, cards).ToList();
            // A bullet set apart from its text, in a block of its own, marks the line it is
            // level with, as a bullet drawn as a shape does (even in the middle of a block).
            foreach (var bullet in blocks.Where(block => LoneBulletPattern().IsMatch(block.Text)).ToList())
            {
                shapes.Marks.Add(new Mark(bullet.BoundingBox, false, false));
                blocks.Remove(bullet);
            }

            foreach (var label in TakeLabels(shapes.Figures, blocks, shapes.Tables.Select(table => table.Box).Concat(shapes.Formulas.Select(formula => formula.Box)), page.Letters))
            {
                blocks.Remove(label);
            }

            var textWidth = blocks.Count == 0 ? 0 : blocks.Max(block => block.BoundingBox.Width);
            foreach (var block in blocks)
            {
                pageItems.AddRange(Paragraphs(block, number, textWidth, rejoined, shapes, tags));
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

            // A page number sits alone near the top or bottom edge, or a little farther in
            // with nothing beyond it (LaTeX sets it an inch and more from the edge); numbers
            // elsewhere (a table's figures) stay. It goes first: one at the foot of the page
            // can come first in reading order, and the figures would be placed before it.
            var margin = page.Height * 0.08;
            var highest = pageItems.Count == 0 ? 0 : pageItems.Max(item => item.Top);
            var lowest = pageItems.Count == 0 ? 0 : pageItems.Min(item => item.Top);
            // A running title can share the line at the top.
            pageItems.RemoveAll(item => IsPageNumber(item.Text) && (item.Top > page.Height - margin || item.Top < margin
                || item.Top > page.Height - margin * 2 && item.Top >= highest - Math.Max(item.Size, 1) * 0.5
                    && pageItems.Count(other => other.Top >= item.Top - Math.Max(item.Size, 1) * 0.5) <= 2
                || item.Top < margin * 2 && item.Top <= lowest + Math.Max(item.Size, 1) * 0.5
                    && pageItems.Count(other => other.Top <= item.Top + Math.Max(item.Size, 1) * 0.5) == 1));

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

            foreach (var (box, table, _) in taggedTables)
            {
                Place(new Item(table, 0, false, ItemKind.Table, box.Top, number));
            }

            // A figure, and a formula on a line of its own, is drawn again as an SVG picture
            // (PdfFigure.cs) at the size it has on the page.
            // Figures with no text between them keep the page's order: top to bottom, then
            // left to right.
            foreach (var (box, alt, isFormula) in shapes.Figures.Select(figure => (figure, string.Empty, false))
                         .Concat(shapes.Formulas.Where(formula => formula.Display).Select(formula => (formula.Box, FormulaText(formula), true)))
                         .OrderByDescending(entry => Math.Round(entry.Item1.Top)).ThenBy(entry => entry.Item1.Left))
            {
                Place(new Item(SaveSvg(FigureSvg(page, box, states.Value, isFormula)), 0, false, ItemKind.Image, box.Top, number,
                    Left: box.Left, Right: box.Right, Bottom: box.Bottom, Natural: box.Width * 4 / 3, Alt: alt));
            }

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
        // In a PDF tagged with headings, a tagged page's headings are those the tags name, at
        // their levels ("H" without one keeps the guess); text set larger than all of them (a
        // title the tags call a paragraph) stays a heading too.
        if (structure?.HasHeadings == true)
        {
            var largest = text.Where(item => item.Heading > 0).Select(item => item.Size).DefaultIfEmpty(double.MaxValue).Max();
            for (var i = 0; i < text.Count; i++)
            {
                var item = text[i];
                levels[i] = item.Heading < 0 || item.Toc || item.Kind is ItemKind.Code or ItemKind.Table ? levels[i]
                    : item.Heading == HeadingTag ? Math.Max(levels[i], 1)
                    : item.Heading > 0 ? item.Heading
                    : levels[i] > 0 && item.Size > largest * 1.05 ? 1 : 0;
            }
        }

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
            images, document.NumberOfPages, readPages, text.Count > 0, headings, picturePages);
    }

    // Whether a page's running text (figures, tables, and formulas apart) cannot be read in
    // order, so the page is better kept as a picture of itself: it is written in vertical
    // lines (half the steps from a letter to the next go down), much of it is turned, or
    // much of it lies over other text on another baseline (a poster's lettering over its
    // background words). A page of under 20 letters is read.
    private static bool PicturePage(IReadOnlyList<Letter> letters)
    {
        var text = letters.Where(letter => !IsBlank(letter)).ToList();
        if (text.Count < 20)
        {
            return false;
        }

        var down = 0;
        for (var i = 1; i < text.Count; i++)
        {
            var (from, to) = (text[i - 1].StartBaseLine, text[i].StartBaseLine);
            var size = Math.Max(text[i].PointSize, 1);
            if (from.Y - to.Y > size * 0.5 && from.Y - to.Y < size * 2 && Math.Abs(to.X - from.X) < size * 0.3)
            {
                down++;
            }
        }

        if (down * 2 >= text.Count - 1 || text.Count(letter => letter.TextOrientation != TextOrientation.Horizontal) * 10 >= text.Count * 3)
        {
            return true;
        }

        var over = 0;
        var sorted = text.OrderBy(letter => letter.BoundingBox.Left).ToList();
        for (var i = 0; i < sorted.Count; i++)
        {
            var a = sorted[i].BoundingBox;
            for (var j = i + 1; j < sorted.Count && sorted[j].BoundingBox.Left < a.Right; j++)
            {
                var b = sorted[j].BoundingBox;
                var width = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
                var height = Math.Min(a.Top, b.Top) - Math.Max(a.Bottom, b.Bottom);
                // Letters on one baseline overlap where they are set tight (their boxes are wider
                // than their advance), and a letter drawn twice for bold is on its own baseline.
                var sameLine = Math.Abs(sorted[i].StartBaseLine.Y - sorted[j].StartBaseLine.Y) < Math.Min(sorted[i].PointSize, sorted[j].PointSize) * 0.3;
                if (width > 0 && height > 0 && !sameLine && width * height > Math.Min(a.Width * a.Height, b.Width * b.Height) * 0.4)
                {
                    over++;
                    break;
                }
            }
        }

        return over * 100 >= text.Count * 15;
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

    // Inline markup as a browser draws it: a highlight (<mark>) is a colored box just around
    // its letters, a key (<kbd>) a gray ring around monospaced letters, and a superscript or
    // subscript (<sup>, <sub>) smaller letters raised or lowered from the line. The letters
    // of each run carry its tags (the first opens, the last closes), and raised or lowered
    // letters are set back on the line, so they are read with it.
    private static List<Letter> InlineMarkup(List<Letter> letters, Page page)
    {
        var shapes = page.Paths.Where(path => !path.IsClipping && path.IsFilled && path.GetBoundingRectangle() is not null)
            .Select(path => (Box: path.GetBoundingRectangle()!.Value, Path: path)).ToList();
        var tags = new string?[letters.Count];
        var shift = new double[letters.Count];
        bool Inside(PdfRectangle box, Letter letter) => Contains(box, letter) && box.Height <= letter.PointSize * 2.2;
        for (var i = 0; i < letters.Count; i++)
        {
            var letter = letters[i];
            if (IsBlank(letter) || letter.GetFont() is null)
            {
                continue;
            }

            // Raised or lowered: smaller than the letter before (or after) it on the line, and
            // close to it; letters of one run follow the first.
            var size = letter.PointSize;
            Letter? Beside(int at) => at >= 0 && at < letters.Count && !IsBlank(letters[at])
                && Math.Min(Math.Abs(letters[at].EndBaseLine.X - letter.StartBaseLine.X), Math.Abs(letter.EndBaseLine.X - letters[at].StartBaseLine.X)) < size ? letters[at] : null;
            if (i > 0 && tags[i - 1] is "sup" or "sub" && Beside(i - 1) is { } run && Math.Abs(run.PointSize - size) < 0.1
                && Math.Abs(run.StartBaseLine.Y - letter.StartBaseLine.Y) < 0.1)
            {
                (tags[i], shift[i]) = (tags[i - 1], shift[i - 1]);
                continue;
            }

            if ((Beside(i - 1) ?? Beside(i + 1)) is { } line && line.PointSize > size * 1.1
                && (letter.StartBaseLine.Y - line.StartBaseLine.Y) / line.PointSize is var raised && Math.Abs(raised) is > 0.15 and < 0.7)
            {
                (tags[i], shift[i]) = (raised > 0 ? "sup" : "sub", line.StartBaseLine.Y - letter.StartBaseLine.Y);
                continue;
            }

            foreach (var (box, path) in shapes)
            {
                if (!Inside(box, letter))
                {
                    continue;
                }

                // Only a box that fits its letters closely (not a banner or a code background).
                var within = letters.Where(other => !IsBlank(other) && Contains(box, other)).ToList();
                if (within.Count == 0 || box.Width > within.Max(other => other.BoundingBox.Right) - within.Min(other => other.BoundingBox.Left) + size * 1.6)
                {
                    continue;
                }

                var (r, g, b) = path.FillColor is null || path.FillColor is PatternColor ? (0.0, 0.0, 0.0) : path.FillColor.ToRGBValues();
                var colorful = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b)) >= 0.25 && Math.Max(r, Math.Max(g, b)) >= 0.6;
                if (colorful && IsRectangle(path, box))
                {
                    tags[i] = "mark";
                }
                else if (!colorful && IsRing(path, box) && within.All(other => IsMonospaced(other.FontName)))
                {
                    tags[i] = "kbd";
                }
            }
        }

        var result = new List<Letter>(letters.Count);
        for (var i = 0; i < letters.Count; i++)
        {
            var letter = letters[i];
            if (tags[i] is not { } tag)
            {
                result.Add(letter);
                continue;
            }

            // A run is the letters next to each other with the same tag (blanks between the
            // letters of a highlight stay in it).
            bool Same(int at) => at >= 0 && at < letters.Count && tags[at] == tag && Math.Abs(shift[at] - shift[i]) < 0.1;
            var open = !Same(i - 1) ? $"<{tag}>" : string.Empty;
            var close = !Same(i + 1) ? $"</{tag}>" : string.Empty;
            var dy = shift[i];
            var box = new PdfRectangle(letter.BoundingBox.Left, letter.BoundingBox.Bottom + dy, letter.BoundingBox.Right, letter.BoundingBox.Top + dy);
            result.Add(new Letter(open + letter.Value + close, box, box, new PdfPoint(letter.StartBaseLine.X, letter.StartBaseLine.Y + dy),
                new PdfPoint(letter.EndBaseLine.X, letter.EndBaseLine.Y + dy), letter.Width, letter.FontSize, letter.GetFont()!, letter.RenderingMode,
                letter.StrokeColor, letter.FillColor, letter.PointSize, letter.TextSequence));
        }

        return result;
    }

    // A box of straight lines along its edges (not a letter drawn as a shape).
    private static bool IsRectangle(PdfPath path, PdfRectangle box) =>
        path.Count == 1 && path[0].Commands.All(command => command switch
        {
            PdfSubpath.Move move => OnEdge(move.Location, box),
            PdfSubpath.Line line => OnEdge(line.From, box) && OnEdge(line.To, box),
            PdfSubpath.Close => true,
            _ => false,
        });

    private static bool OnEdge(PdfPoint point, PdfRectangle box) =>
        Math.Min(Math.Abs(point.X - box.Left), Math.Abs(point.X - box.Right)) < 0.5 || Math.Min(Math.Abs(point.Y - box.Bottom), Math.Abs(point.Y - box.Top)) < 0.5;

    // A border drawn as a filled ring: an outline and one just inside it.
    private static bool IsRing(PdfPath path, PdfRectangle box) =>
        path.Count == 2 && path[1].GetBoundingRectangle() is { } inner
        && inner.Left - box.Left is >= 0 and < 3 && box.Right - inner.Right is >= 0 and < 3
        && inner.Bottom - box.Bottom is >= 0 and < 3 && box.Top - inner.Top is >= 0 and < 3;

    // Text blocks of a page in reading order (columns are read one after another).
    // On a tagged page the blocks come from the tags: each paragraph-like element's letters
    // make its own block, read in the order of the tree (PdfPig's blocks would cut one
    // paragraph into lines, having too few lines to measure their spacing; an element with
    // a wide gap in a line, a form line or text across columns, is cut into blocks as an
    // untagged page is). A letter in no element (a formula set in the text, an icon a style
    // sheet adds) goes with the nearest letter on its line that is in one; others make
    // blocks placed by where they stand.
    private static IEnumerable<TextBlock> Blocks(IReadOnlyList<Letter> letters, HashSet<Word> rejoined, PageTags? tags, IReadOnlyList<PdfRectangle> frames)
    {
        if (letters.Count == 0)
        {
            return [];
        }

        if (tags is null)
        {
            var ordered = new List<TextBlock>();
            foreach (var block in TextBlocks(letters, rejoined, frames))
            {
                InsertByPlace(ordered, block);
            }

            return ordered;
        }

        var tagged = letters.Where(letter => tags.Of(letter) is not null && !IsBlank(letter)).ToList();
        Tag? Near(Letter letter) => tagged.Where(other => Math.Abs(other.StartBaseLine.Y - letter.StartBaseLine.Y) < Math.Max(letter.PointSize, 1) * 0.5)
            .Select(other => (other, gap: Math.Max(other.BoundingBox.Left - letter.BoundingBox.Right, letter.BoundingBox.Left - other.BoundingBox.Right)))
            .Where(pair => pair.gap < Math.Max(letter.PointSize, 1) * 3).OrderBy(pair => pair.gap).Select(pair => pair.other).FirstOrDefault() is { } near ? tags.Of(near) : null;
        var groups = letters.GroupBy(letter => (tags.Of(letter) ?? Near(letter))?.Block ?? -1).ToList();
        var result = groups.Where(group => group.Key >= 0).OrderBy(group => group.Min(letter => tags.Of(letter)?.Order ?? int.MaxValue))
            .SelectMany(group => ElementBlock(group.ToList()) is { } block ? [block] : TextBlocks(group.ToList(), rejoined, frames)).ToList();
        foreach (var block in groups.Where(group => group.Key < 0).SelectMany(group => TextBlocks(group.ToList(), rejoined, frames)))
        {
            InsertByPlace(result, block);
        }

        // A browser tags each key and value of a front matter card as an element of its own.
        return KeyValueRows(result, rejoined, frames);
    }

    // An element's letters as one block of lines, or null when a line has a gap wider than
    // three line heights.
    private static TextBlock? ElementBlock(IReadOnlyList<Letter> letters)
    {
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters).Where(word => word.Text.Trim().Length > 0).ToList();
        if (words.Count == 0)
        {
            return null;
        }

        var lines = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(word => word.BoundingBox.Bottom))
        {
            var line = lines.LastOrDefault();
            if (line is not null && Math.Abs(line[0].BoundingBox.Bottom - word.BoundingBox.Bottom) < LineHeight(word) * 0.5)
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
            var height = Math.Max(line.Max(word => word.BoundingBox.Height), 1);
            if (line.Zip(line.Skip(1)).Any(pair => pair.Second.BoundingBox.Left - pair.First.BoundingBox.Right > height * 3))
            {
                return null;
            }
        }

        return new TextBlock(lines.Select(line => new TextLine(line)).ToList());
    }

    // Letters in blocks, in PdfPig's reading order. PdfPig follows the order the page draws
    // its text in where places leave it open, and a page can draw a title after the text
    // below it (a chapter's first page): see InsertByPlace.
    private static IEnumerable<TextBlock> TextBlocks(IReadOnlyList<Letter> letters, HashSet<Word> rejoined, IReadOnlyList<PdfRectangle> frames)
    {
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters).ToList();
        var blocks = KeyValueRows(JoinSplitLines(DocstrumBoundingBoxes.Instance.GetBlocks(words).ToList(), rejoined), rejoined, frames);
        var ordered = new List<TextBlock>();
        foreach (var block in UnsupervisedReadingOrderDetector.Instance.Get(blocks))
        {
            InsertByPlace(ordered, block);
        }

        return ordered;
    }

    // A block wholly above one read before it, and over most of its width or wholly to its
    // left, is read first (a block of another column to the right stays after): it goes
    // after every block that stands above it, before the first one below it.
    private static void InsertByPlace(List<TextBlock> ordered, TextBlock block)
    {
        var box = block.BoundingBox;
        var above = ordered.FindLastIndex(other => other.BoundingBox.Bottom > box.Top);
        var below = ordered.FindIndex(above + 1, other => other.BoundingBox.Top < box.Bottom
            && (Math.Min(other.BoundingBox.Right, box.Right) - Math.Max(other.BoundingBox.Left, box.Left) > Math.Min(other.BoundingBox.Width, box.Width) * 0.5
                || box.Right <= other.BoundingBox.Left));
        ordered.Insert(below < 0 ? ordered.Count : below, block);
    }

    // Labels around a figure (a chart's "男児" and "0", a title set beside it) cannot keep
    // their place in running text: one beside a figure comes before or after all of it, and
    // labels between charts lose the chart they belong to. Short text beside a figure, and a
    // short line close to it, goes into the figure's picture instead: the figure grows to
    // take it in, and figures that then meet, or stand side by side, become one. Headings
    // and longer text stay text, and a figure that would grow over other text, a table, or a
    // formula stays as it is. The blocks taken are returned.
    private static List<TextBlock> TakeLabels(List<PdfRectangle> figures, List<TextBlock> blocks, IEnumerable<PdfRectangle> keepOut, IReadOnlyList<Letter> letters)
    {
        var taken = new HashSet<TextBlock>(ReferenceEqualityComparer.Instance);
        var others = keepOut.ToList();
        int Longest(TextBlock block) => block.TextLines.Max(line => line.Text.Count(c => !char.IsWhiteSpace(c)));
        double BlockSize(TextBlock block) => block.TextLines.SelectMany(line => line.Words).SelectMany(word => word.Letters).Max(letter => letter.PointSize);
        double LabelSize(PdfRectangle figure)
        {
            var sizes = letters.Where(letter => Contains(figure, letter) && !IsBlank(letter)).Select(letter => letter.PointSize).Order().ToList();
            return sizes.Count > 0 ? sizes[sizes.Count / 2] : 0;
        }

        // Inline formulas stand in the text as private-use letters.
        bool IsLabel(TextBlock block) => Longest(block) <= 16 && !block.Text.Any(c => c >= (char)0xE000 && c <= (char)0xF8FF);

        // A grown box takes along the labels it reaches (axis numbers stand side by side, each
        // in reach of the next); it fails if it reaches other text, a table, or a formula.
        List<TextBlock>? Reach(ref PdfRectangle grown, PdfRectangle[] before)
        {
            var along = new List<TextBlock>();
            while (true)
            {
                var box = grown;
                // Text that already overlapped the figure is not the growing's doing.
                var hit = blocks.Where(other => !taken.Contains(other) && !along.Contains(other) && Overlaps(box, other.BoundingBox)
                    && (IsLabel(other) || !before.Any(old => Overlaps(old, other.BoundingBox)))).ToList();
                if (hit.Count == 0)
                {
                    return others.Any(other => Overlaps(box, other) && !before.Any(old => Overlaps(old, other))) ? null : along;
                }

                if (!hit.All(IsLabel))
                {
                    return null;
                }

                along.AddRange(hit);
                grown = Union(hit.Select(other => other.BoundingBox).Append(box));
            }
        }

        for (var changed = figures.Count > 0; changed;)
        {
            changed = false;
            foreach (var block in blocks.Where(block => !taken.Contains(block) && IsLabel(block)))
            {
                var box = block.BoundingBox;
                var longest = Longest(block);
                var height = Math.Max(block.TextLines.Max(line => line.BoundingBox.Height), 1);
                var at = figures.FindIndex(figure =>
                {
                    var gapX = Math.Max(0, Math.Max(figure.Left - box.Right, box.Left - figure.Right));
                    var gapY = Math.Max(0, Math.Max(figure.Bottom - box.Top, box.Bottom - figure.Top));
                    var level = Math.Min(figure.Top, box.Top) - Math.Max(figure.Bottom, box.Bottom) > 0;
                    // A short line close to a figure (a legend, a chart's title) belongs to it
                    // when set no larger than the figure's own labels; a heading is larger.
                    var near = gapX <= height * 1.2 && gapY <= height * 1.2
                        && (longest <= 4 || longest <= 8 && LabelSize(figure) is var size && size > 0 && BlockSize(block) <= size * 1.5);
                    return (level && gapX <= height * 3) || near;
                });
                if (at < 0)
                {
                    continue;
                }

                var grown = Union([figures[at], box]);
                if (Reach(ref grown, [figures[at], box]) is not { } along)
                {
                    continue;
                }

                taken.Add(block);
                taken.UnionWith(along);
                figures[at] = grown;
                changed = true;
                break;
            }

            for (var i = 0; i < figures.Count; i++)
            {
                for (var j = figures.Count - 1; j > i; j--)
                {
                    // Charts set side by side stay side by side, however far apart, when only
                    // their labels stand between them (Reach fails on other text).
                    var (a, b) = (figures[i], figures[j]);
                    var row = Math.Min(a.Top, b.Top) - Math.Max(a.Bottom, b.Bottom) > Math.Min(a.Height, b.Height) * 0.5;
                    var joined = Union([a, b]);
                    if ((Overlaps(a, b) || row) && Reach(ref joined, [a, b]) is { } along)
                    {
                        taken.UnionWith(along);
                        figures[i] = joined;
                        figures.RemoveAt(j);
                        changed = true;
                    }
                }
            }
        }

        return blocks.Where(taken.Contains).ToList();
    }

    // A line with a wide gap ("File Number: E46769   Type dsg: 1494V-0") can be cut into two
    // blocks, and its right part would then be read after the lines below its left part. A
    // block of one line that goes on from a line of another block, on the same baseline and
    // across a gap narrower than a column gap (2.5 line heights), goes back into that block.
    // So does a column of page numbers that goes on from contents lines ending in leader
    // dots, one number to a line. Its words are added to rejoined.
    private static List<TextBlock> JoinSplitLines(List<TextBlock> blocks, HashSet<Word> rejoined)
    {
        static bool GoesOn(List<Word> line, List<Word> part)
        {
            var height = Math.Max(part.Max(word => word.BoundingBox.Height), 1);
            var gap = part.Min(word => word.BoundingBox.Left) - line.Max(word => word.BoundingBox.Right);
            return Math.Abs(line[0].BoundingBox.Bottom - part[0].BoundingBox.Bottom) < height * 0.5 && gap > 0 && gap < height * 2.5
                && !LoneBulletPattern().IsMatch(ReadLine(line).Text);
        }

        for (var i = 0; i < blocks.Count; i++)
        {
            var piece = SplitLines(blocks[i]);
            // A lone bullet is put back with its text later, as a list item.
            if (piece.Count == 0 || piece.Count == 1 && LoneBulletPattern().IsMatch(ReadLine(piece[0]).Text)
                || piece.Count > 1 && !piece.All(part => IsPageNumber(ReadLine(part).Text)))
            {
                continue;
            }

            var owner = blocks.FindIndex(block => !ReferenceEquals(block, blocks[i]) && (piece.Count == 1
                ? SplitLines(block).Any(line => GoesOn(line, piece[0]))
                : piece.All(part => SplitLines(block).Any(line => GoesOn(line, part) && LeaderDotsPattern().IsMatch(ReadLine(line).Text)))));
            if (owner < 0)
            {
                continue;
            }

            // Only a piece that reaches past the block's right edge makes a form line (or a
            // table row); a piece inside a paragraph's width is part of a wrapped line.
            var height = Math.Max(piece[0].Max(word => word.BoundingBox.Height), 1);
            if (piece.Count == 1 && piece[0].Max(word => word.BoundingBox.Right) > blocks[owner].BoundingBox.Right + height)
            {
                rejoined.UnionWith(piece[0]);
            }

            blocks[owner] =new TextBlock(blocks[owner].TextLines.Concat(blocks[i].TextLines).ToList());
            blocks.RemoveAt(i);
            i = -1;
        }

        return blocks;
    }

    // Keys and values set side by side (a front matter card: "title" beside its value,
    // "tags" beside a row of tags) come apart into a column of keys and blocks of values,
    // and each word would stand alone. A narrow block of short lines (twelve letters at
    // most) with blocks to its right on the same baselines is one: it, those blocks, and
    // more keys under it at the same left edge become one block whose rows are lines of
    // their own (their words are added to rejoined), when two rows or more have a key. It
    // must all lie in one frame (a card; chart labels and labels around a picture, set
    // the same way, are not framed), not a code background.
    private static List<TextBlock> KeyValueRows(List<TextBlock> blocks, HashSet<Word> rejoined, IReadOnlyList<PdfRectangle> frames)
    {
        if (blocks.Count < 2)
        {
            return blocks;
        }

        var width = blocks.Max(block => block.BoundingBox.Right) - blocks.Min(block => block.BoundingBox.Left);
        bool Keys(TextBlock block) => SplitLines(block) is var lines && lines.Count > 0
            && lines.All(line => ReadLine(line).Text.Count(c => !char.IsWhiteSpace(c)) <= 12) && block.BoundingBox.Width < width * 0.25;
        // The leftmost column of short lines holds the keys (the values can be short too).
        foreach (var keys in blocks.OrderBy(block => block.BoundingBox.Left).ToList())
        {
            var i = blocks.IndexOf(keys);
            if (i < 0 || !Keys(keys))
            {
                continue;
            }

            var height = Math.Max(SplitLines(keys).Max(line => line.Max(word => word.BoundingBox.Height)), 1);
            var group = new List<TextBlock> { keys };
            for (var grown = true; grown;)
            {
                grown = false;
                var (top, bottom) = (group.Max(block => block.BoundingBox.Top), group.Min(block => block.BoundingBox.Bottom));
                foreach (var block in blocks.Where(block => !group.Contains(block)))
                {
                    var box = block.BoundingBox;
                    // Rows can stand a few line heights apart.
                    var level = box.Top <= top + height && box.Top >= bottom - height * 3;
                    var values = box.Left >= keys.BoundingBox.Right - 1;
                    // A key already put back with its value across a wide gap (JoinSplitLines) is one too.
                    var moreKeys = Math.Abs(box.Left - keys.BoundingBox.Left) <= 2
                        && (Keys(block) || block.TextLines.SelectMany(line => line.Words).Any(rejoined.Contains));
                    if (level && (values || moreKeys))
                    {
                        group.Add(block);
                        grown = true;
                    }
                }
            }

            // Two rows or more with a key and a value on one baseline.
            var keyLines = group.Where(block => Math.Abs(block.BoundingBox.Left - keys.BoundingBox.Left) <= 2).SelectMany(SplitLines).ToList();
            var valueLines = group.Where(block => block.BoundingBox.Left >= keys.BoundingBox.Right - 1).SelectMany(SplitLines).ToList();
            if (valueLines.Count == 0 || keyLines.Count(key => valueLines.Any(value => Math.Abs(value[0].BoundingBox.Bottom - key[0].BoundingBox.Bottom) < height * 0.5)) < 2)
            {
                continue;
            }

            var area = Union(group.Select(block => block.BoundingBox));
            if (!frames.Any(frame => frame.Left <= area.Left + 2 && frame.Right >= area.Right - 2 && frame.Bottom <= area.Bottom + 2 && frame.Top >= area.Top - 2))
            {
                continue;
            }

            // Values that wrap (a line reaching the right edge, the next starting where its
            // value does) are prose beside a label, read as it was. A nested key ("url" under
            // "author") starts left of the value before it.
            var rows = SplitLines(new TextBlock(group.SelectMany(block => block.TextLines).ToList()));
            var right = rows.Max(row => row.Max(word => word.BoundingBox.Right));
            double ValueStart(List<Word> row)
            {
                var value = row.Where(word => word.BoundingBox.Left > keys.BoundingBox.Right + 1).ToList();
                if (value.Count == 0)
                {
                    return row[0].BoundingBox.Left;
                }

                // After the widest gap: a nested key's value.
                var start = value[0];
                var widest = 0.0;
                for (var w = 1; w < value.Count; w++)
                {
                    var gap = value[w].BoundingBox.Left - value[w - 1].BoundingBox.Right;
                    if (gap > widest && gap > height * 1.5)
                    {
                        (widest, start) = (gap, value[w]);
                    }
                }

                return start.BoundingBox.Left;
            }

            if (rows.Zip(rows.Skip(1)).Any(pair => pair.First.Max(word => word.BoundingBox.Right) >= right - height * 3
                    && Math.Abs(pair.Second[0].BoundingBox.Left - ValueStart(pair.First)) <= 3))
            {
                continue;
            }


            var merged = new TextBlock(group.SelectMany(block => block.TextLines).ToList());
            rejoined.UnionWith(group.SelectMany(block => block.TextLines).SelectMany(line => line.Words));
            blocks.RemoveAll(group.Contains);
            blocks.Insert(Math.Min(i, blocks.Count), merged);
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
    // On a tagged page, a paragraph also ends where its element in the tags does, and a line
    // that holds the start of a list item in the tags starts a list item.
    private static IEnumerable<Item> Paragraphs(TextBlock block, int page, double textWidth, HashSet<Word> rejoined, PageShapes shapes, PageTags? tags)
    {
        var split = SplitLines(block);
        var lines = split.Select(words => ReadLine(words) with { Rejoined = words.Any(rejoined.Contains) }).ToList();
        // On a code background, a line of Japanese with a monospaced start (a diff's "- 保存") is code.
        var code = split.Select(words => CodeLine(words, shapes.CodeFrames.Contains(FrameAt(shapes, words.Min(word => word.BoundingBox.Left) + 1, words.Max(word => word.BoundingBox.Top) - 1)))).ToList();
        var lineTags = split.Select(words => tags is null ? null : TagOf(tags, words.SelectMany(word => word.Letters))).ToList();
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Text.Length == 0)
            {
                lines.RemoveAt(i);
                code.RemoveAt(i);
                lineTags.RemoveAt(i);
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
        var firstHeading = -1;
        Tag? previousTag = null;

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
            yield return new Item(text, firstSize, firstBold, kind, firstTop, page, mathInItem, firstFamily, Left: firstLeft, Right: right, Heading: firstHeading);
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

            var lineTag = lineTags[index];
            var newElement = lineTag is not null && previousTag is not null && lineTag.Block != previousTag.Block;
            if (lineKind == ItemKind.Paragraph && lineTag is not null && lineTag.ListItem != previousTag?.ListItem && tags!.StartsListItem(lineTag))
            {
                (lineKind, rest) = (ItemKind.Bullet, line.Text.TrimStart('・', '･', '•', '●', '○', '◦', '▪', '■', '□', '◆', '◇', '‣', '⁃', '-', ' ', '\u3000'));
            }

            previousTag = lineTag ?? previousTag;
            var sizeChanged = current.Count > 0 && Math.Abs(line.Size - firstSize) > firstSize * 0.1;
            var fontChanged = current.Count > 0 && line.Family != firstFamily;
            var endedEarly = previousLine is not null && blockRight - previousLine.Right > shortLine;
            // A line put back together across a wide gap is a form line or a table row, not
            // prose: it stands alone.
            var standsAlone = line.Rejoined || previousLine?.Rejoined == true;
            if (current.Count > 0 && (sizeChanged || fontChanged || endedEarly || standsAlone || newElement || lineKind != ItemKind.Paragraph))
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
                firstHeading = tags is null ? -1 : lineTag?.Heading ?? -1;
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
            var height = LineHeight(word);
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

    // How tall a word stands, for telling lines apart: its box, or half its letters' size
    // when the box is flatter (letters in a Type 3 font, as a browser printing without the
    // fonts writes them, can have boxes of no height).
    private static double LineHeight(Word word) =>
        Math.Max(Math.Max(word.BoundingBox.Height, word.Letters.Count == 0 ? 0 : word.Letters.Max(letter => letter.PointSize) * 0.5), 1);

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

            var bare = InlineTagPattern().Replace(text, string.Empty) is { Length: > 0 } stripped ? stripped : text;

            // No space between Japanese or Chinese characters, before closing punctuation,
            // or after an opening bracket (inline tags aside).
            if (last is not null && !(IsCjk(last[^1]) && IsCjk(bare[0])) && !".,;:!?)]}%".Contains(bare[0]) && !"([{".Contains(last[^1]))
            {
                builder.Append(' ');
            }

            builder.Append(text);
            last = bare;
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
            else if (builder[^1] == '-' && builder.Length > 1 && char.IsAsciiDigit(builder[^2]) && char.IsAsciiDigit(line[0]))
            {
                // A range of numbers wrapped after its hyphen ("8-" and "9") keeps the hyphen.
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

    // The tags InlineMarkup puts in the text.
    [GeneratedRegex("</?(?:mark|kbd|sup|sub)>")]
    private static partial Regex InlineTagPattern();

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

    // Leader dots at the end of a line, before its page number.
    [GeneratedRegex(@"(?:[.．・‥…]\s*){4,}$")]
    private static partial Regex LeaderDotsPattern();

    // Leader dots (or ・ ‥ …) and a page number at the end of a table-of-contents line.
    [GeneratedRegex(@"\s*(?:[.．・‥…]\s*){4,}([0-9０-９]{1,4})\s*$")]
    private static partial Regex LeaderPattern();

    [GeneratedRegex(@"([-,]?(Bold|Italic|Oblique|Regular|Medium|Light|Semibold|SemiBold|Black|Heavy|Book|Roman)|PSMT|MT|PS)+$", RegexOptions.IgnoreCase)]
    private static partial Regex FontStylePattern();
}
