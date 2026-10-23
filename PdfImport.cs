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
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace MdEditor;

// A PDF records where each letter goes, not what is a heading, a paragraph, or a table.
// PdfPig (Apache 2.0, managed code only) reads the letters; text blocks are put in reading
// order, lines are joined into paragraphs, headings are guessed from text size
// (HeadingGuess), lines starting with a bullet or number become list items, and pictures
// are written to "name_files". Tables, math, and drawings come back only as their text.
// Nothing is sent anywhere, and the PDF itself is never written.
internal static partial class PdfImport
{
    public const int MaxPages = 500;

    public sealed record Result(string Markdown, int Images, int Pages, int ReadPages, bool HasText, int Headings);

    private sealed record Line(string Text, double Size, bool Bold, double Top, double Right, bool MathFont, string Family);

    private sealed record Item(string Text, double Size, bool Bold, ItemKind Kind, double Top, int Page, bool Math = false, string Family = "", bool Toc = false);

    private enum ItemKind { Paragraph, Bullet, Numbered, Image }

    public static Result ToMarkdown(string path, string assets, string imagesName)
    {
        using var document = PdfDocument.Open(path);
        var items = new List<Item>();
        var images = 0;
        var readPages = Math.Min(document.NumberOfPages, MaxPages);
        for (var number = 1; number <= readPages; number++)
        {
            var page = document.GetPage(number);
            var pageItems = new List<Item>();
            var blocks = Blocks(page).ToList();
            var textWidth = blocks.Count == 0 ? 0 : blocks.Max(block => block.BoundingBox.Width);
            foreach (var block in blocks)
            {
                pageItems.AddRange(Paragraphs(block, number, textWidth));
            }

            // A page number sits alone near the top or bottom edge; numbers elsewhere (a
            // table's figures) stay.
            var margin = page.Height * 0.08;
            pageItems.RemoveAll(item => IsPageNumber(item.Text) && (item.Top > page.Height - margin || item.Top < margin));

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

            foreach (var image in page.GetImages())
            {
                if (SaveImage(image, assets, imagesName, images + 1) is { } file)
                {
                    images++;
                    // Placed before the first text that starts below the picture's top.
                    var top = image.BoundingBox.Top;
                    var at = pageItems.FindIndex(item => item.Kind != ItemKind.Image && item.Top < top - 1);
                    var picture = new Item(file, 0, false, ItemKind.Image, top, number);
                    if (at < 0) pageItems.Add(picture); else pageItems.Insert(at, picture);
                }
            }

            items.AddRange(pageItems);
        }

        var text = items.Where(item => item.Kind != ItemKind.Image).ToList();
        // The body typeface is the one most of the text is set in; a heading set in another
        // (Gothic over Mincho, for example) stands out even at the body size.
        var bodyFamily = text.GroupBy(item => item.Family)
            .OrderByDescending(group => group.Sum(item => item.Text.Length))
            .Select(group => group.Key).FirstOrDefault() ?? string.Empty;
        var levels = HeadingGuess.Levels(text.Select(item => new SizedParagraph(
            item.Text, item.Size, item.Bold,
            item.Kind == ItemKind.Bullet || item.Math || item.Toc,
            item.Family.Length > 0 && item.Family != bodyFamily)).ToList());
        var headingOf = new Dictionary<Item, int>(ReferenceEqualityComparer.Instance);
        for (var i = 0; i < text.Count; i++)
        {
            headingOf[text[i]] = levels[i];
        }

        var markdown = new StringBuilder();
        ItemKind? previous = null;
        foreach (var item in items)
        {
            var level = headingOf.GetValueOrDefault(item);
            var kind = level > 0 ? ItemKind.Paragraph : item.Kind;
            var listContinues = (previous is ItemKind.Bullet or ItemKind.Numbered) && kind == previous;
            if (markdown.Length > 0)
            {
                markdown.Append(listContinues ? "\n" : "\n\n");
            }

            switch (kind)
            {
                case ItemKind.Image:
                    markdown.Append($"![]({Uri.EscapeDataString(imagesName)}/{item.Text})");
                    break;
                case ItemKind.Bullet:
                    markdown.Append("- ").Append(Escape(item.Text));
                    break;
                case ItemKind.Numbered:
                    markdown.Append(item.Text);
                    break;
                default:
                    markdown.Append(level > 0 ? new string('#', level) + " " + item.Text : Escape(item.Text));
                    break;
            }

            previous = kind;
        }

        var headings = levels.Count(level => level > 0);
        return new Result(markdown.Length > 0 ? markdown.Append('\n').ToString() : string.Empty,
            images, document.NumberOfPages, readPages, text.Count > 0, headings);
    }

    // Text blocks of a page in reading order (columns are read one after another).
    private static IEnumerable<TextBlock> Blocks(Page page)
    {
        if (page.Letters.Count == 0)
        {
            return [];
        }

        var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToList();
        var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        return UnsupervisedReadingOrderDetector.Instance.Get(blocks);
    }

    // The paragraphs of a block: a new one starts where the size changes, a line starts a
    // list item, or the line before ended well short of the block's right edge (so it was
    // not wrapped: the last line of a paragraph, or a short item such as a checklist line).
    // In a block much narrower than the page's text (a list of short items, table cells),
    // every line stands alone. Wrapped lines are joined.
    private static IEnumerable<Item> Paragraphs(TextBlock block, int page, double textWidth)
    {
        var lines = SplitLines(block).Select(ReadLine).Where(line => line.Text.Length > 0).ToList();
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

        IEnumerable<Item> Flush()
        {
            if (current.Count == 0)
            {
                yield break;
            }

            var text = JoinLines(current.Select(line => line.Text));
            var mathInItem = current.Any(line => line.MathFont);
            current.Clear();
            yield return new Item(text, firstSize, firstBold, kind, firstTop, page, mathInItem, firstFamily);
        }

        foreach (var line in lines)
        {
            // A table-of-contents line ("3.1 Definitions ........ 6") reaches the right edge
            // because of its leader dots, so it would look wrapped; it stands alone, with the
            // dots shortened, and is never a heading.
            var leader = LeaderPattern().Match(line.Text);
            if (leader.Success)
            {
                foreach (var item in Flush()) yield return item;
                var entry = line.Text[..leader.Index].TrimEnd() + " …… " + leader.Groups[1].Value;
                yield return new Item(entry, line.Size, line.Bold, ItemKind.Paragraph, line.Top, page, Family: line.Family, Toc: true);
                previousLine = null;
                continue;
            }

            var (lineKind, rest) = ListMarker(line.Text);
            var sizeChanged = current.Count > 0 && Math.Abs(line.Size - firstSize) > firstSize * 0.1;
            var fontChanged = current.Count > 0 && line.Family != firstFamily;
            var endedEarly = previousLine is not null && blockRight - previousLine.Right > shortLine;
            if (current.Count > 0 && (sizeChanged || fontChanged || endedEarly || lineKind != ItemKind.Paragraph))
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

        var letters = words.SelectMany(word => word.Letters).Where(letter => !string.IsNullOrWhiteSpace(letter.Value)).ToList();
        var size = letters.Count == 0 ? 0 : letters.Select(letter => letter.PointSize).OrderBy(value => value).ElementAt(letters.Count / 2);
        var bold = letters.Count > 0 && letters.Count(IsBold) * 2 > letters.Count;
        var mathFont = letters.Any(letter => (letter.FontName ?? string.Empty).Contains("KaTeX", StringComparison.OrdinalIgnoreCase)
            || (letter.FontName ?? string.Empty).Contains("Math", StringComparison.OrdinalIgnoreCase));
        var family = letters.Count == 0 ? string.Empty : letters.GroupBy(letter => Family(letter.FontName))
            .OrderByDescending(group => group.Count()).First().Key;
        return new Line(Normalize(builder.ToString()), size, bold, words.Max(word => word.BoundingBox.Top), words.Max(word => word.BoundingBox.Right), mathFont, family);
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

    [GeneratedRegex(@"^\s*(\d{1,3})[.)．）]\s+")]
    private static partial Regex NumberedPattern();

    [GeneratedRegex(@"^[\s\-–—]*\d{1,4}(\s*/\s*\d{1,4})?[\s\-–—]*$")]
    private static partial Regex PageNumberPattern();

    [GeneratedRegex(@"^\s*[•●○◦▪■□・·‣⁃]\s*$")]
    private static partial Regex LoneBulletPattern();

    // Leader dots (or ・ ‥ …) and a page number at the end of a table-of-contents line.
    [GeneratedRegex(@"\s*(?:[.．・‥…]\s*){4,}([0-9０-９]{1,4})\s*$")]
    private static partial Regex LeaderPattern();

    [GeneratedRegex(@"([-,]?(Bold|Italic|Oblique|Regular|Medium|Light|Semibold|SemiBold|Black|Heavy|Book|Roman)|PSMT|MT|PS)+$", RegexOptions.IgnoreCase)]
    private static partial Regex FontStylePattern();
}
