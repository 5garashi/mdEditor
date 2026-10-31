/*
  ファイル名 / File: DocxHeadings.cs
  概要 / Description: Turns size-only headings in a Word document into heading styles before it is converted
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-24
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MdEditor;

// Mammoth turns paragraphs with the "heading 1" to "heading 6" styles into headings and
// drops text size. A document whose author made headings only by setting a larger size
// would open as body text, so before converting, a temporary copy gives those
// paragraphs heading styles (see HeadingGuess). The Word file itself is never written.
// Documents that already use heading styles are left as they are.
internal static class DocxHeadings
{
    private const int DefaultHalfPoints = 20;

    // The file to convert: a temporary copy with guessed headings, or the original
    // when there is nothing to guess. Count is the number of paragraphs made headings.
    public static (string Path, int Count) Prepare(string path, string folder)
    {
        var copy = System.IO.Path.Combine(folder, "headings.docx");
        Directory.CreateDirectory(folder);
        File.Copy(path, copy, true);
        int count;
        try
        {
            using var document = WordprocessingDocument.Open(copy, true);
            count = Apply(document);
            if (count > 0)
            {
                document.Save();
            }
        }
        catch (Exception ex) when (ex is OpenXmlPackageException or InvalidDataException or IOException
                                       or InvalidOperationException or System.Xml.XmlException or FormatException)
        {
            // Guessing is optional: the document converts as before, and a damaged file
            // is reported by the conversion itself.
            count = 0;
        }

        if (count > 0)
        {
            return (copy, count);
        }

        File.Delete(copy);
        return (path, 0);
    }

    private static int Apply(WordprocessingDocument document)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return 0;
        }

        var stylesPart = document.MainDocumentPart!.StyleDefinitionsPart;
        var styles = stylesPart?.Styles?.Elements<Style>()
            .Where(style => style.StyleId?.Value is not null)
            .GroupBy(style => style.StyleId!.Value!)
            .ToDictionary(group => group.Key, group => group.First()) ?? [];
        var paragraphs = body.Descendants<Paragraph>().ToList();

        // A document that has headings by style already says what its headings are.
        if (paragraphs.Any(paragraph => IsHeadingStyle(styles, paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value)))
        {
            return 0;
        }

        var defaultSize = int.TryParse(stylesPart?.Styles?.DocDefaults?.RunPropertiesDefault?.RunPropertiesBaseStyle?
            .GetFirstChild<FontSize>()?.Val?.Value, out var half) ? half : DefaultHalfPoints;
        var sized = new List<SizedParagraph>();
        var sources = new List<Paragraph>();
        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            var runs = paragraph.Descendants<Run>().Where(run => run.InnerText.Trim().Length > 0).ToList();
            if (runs.Count == 0)
            {
                continue;
            }

            var sizes = runs.Select(run => Size(styles, run, styleId, defaultSize)).Distinct().ToList();
            var excluded = sizes.Count > 1
                || paragraph.Ancestors<TableCell>().Any()
                || paragraph.ParagraphProperties?.NumberingProperties is not null;
            sized.Add(new SizedParagraph(
                string.Concat(runs.Select(run => run.InnerText)),
                sizes[0] / 2.0,
                runs.All(run => Bold(styles, run, styleId)),
                excluded));
            sources.Add(paragraph);
        }

        var levels = HeadingGuess.Levels(sized);
        var count = 0;
        for (var i = 0; i < levels.Length; i++)
        {
            if (levels[i] == 0)
            {
                continue;
            }

            var properties = sources[i].ParagraphProperties;
            if (properties is null)
            {
                properties = new ParagraphProperties();
                sources[i].PrependChild(properties);
            }

            properties.ParagraphStyleId = new ParagraphStyleId { Val = EnsureHeadingStyle(document, levels[i]) };
            // The heading itself carries the emphasis; Markdown should not wrap it in ** as well.
            foreach (var bold in sources[i].Descendants<Bold>().ToList())
            {
                bold.Remove();
            }

            // A line break (Shift+Enter) or space before or after the heading text would
            // show as "#  heading".
            var content = sources[i].Descendants().Where(element => element is Text { Text.Length: > 0 } or Break).ToList();
            foreach (var edge in content.TakeWhile(element => element is Break)
                         .Concat(content.AsEnumerable().Reverse().TakeWhile(element => element is Break)).ToList())
            {
                edge.Remove();
            }

            var texts = sources[i].Descendants<Text>().Where(text => text.Text.Length > 0).ToList();
            if (texts.Count > 0)
            {
                texts[0].Text = texts[0].Text.TrimStart();
                texts[^1].Text = texts[^1].Text.TrimEnd();
            }

            count++;
        }

        return count;
    }

    private static bool IsHeadingStyle(Dictionary<string, Style> styles, string? styleId)
    {
        var seen = new HashSet<string>();
        while (styleId is not null && styles.TryGetValue(styleId, out var style) && seen.Add(styleId))
        {
            var name = style.StyleName?.Val?.Value ?? string.Empty;
            if (name.StartsWith("heading", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Title", StringComparison.OrdinalIgnoreCase)
                || style.StyleParagraphProperties?.OutlineLevel is not null)
            {
                return true;
            }

            styleId = style.BasedOn?.Val?.Value;
        }

        return false;
    }

    // Text size in half points: the run's own, then its character style, then the
    // paragraph style (and the styles it is based on), then the document default.
    private static int Size(Dictionary<string, Style> styles, Run run, string? paragraphStyle, int defaultSize) =>
        ParseHalfPoints(run.RunProperties?.FontSize?.Val?.Value)
        ?? StyleValue(styles, run.RunProperties?.RunStyle?.Val?.Value, style => ParseHalfPoints(style.StyleRunProperties?.FontSize?.Val?.Value))
        ?? StyleValue(styles, paragraphStyle, style => ParseHalfPoints(style.StyleRunProperties?.FontSize?.Val?.Value))
        ?? defaultSize;

    private static bool Bold(Dictionary<string, Style> styles, Run run, string? paragraphStyle) =>
        IsOn(run.RunProperties?.Bold)
        ?? StyleValue(styles, run.RunProperties?.RunStyle?.Val?.Value, style => IsOn(style.StyleRunProperties?.Bold))
        ?? StyleValue(styles, paragraphStyle, style => IsOn(style.StyleRunProperties?.Bold))
        ?? false;

    private static bool? IsOn(Bold? bold) => bold is null ? null : bold.Val is null || bold.Val.Value;

    private static int? ParseHalfPoints(string? value) => int.TryParse(value, out var half) ? half : null;

    private static T? StyleValue<T>(Dictionary<string, Style> styles, string? styleId, Func<Style, T?> read) where T : struct
    {
        var seen = new HashSet<string>();
        while (styleId is not null && styles.TryGetValue(styleId, out var style) && seen.Add(styleId))
        {
            if (read(style) is { } value)
            {
                return value;
            }

            styleId = style.BasedOn?.Val?.Value;
        }

        return null;
    }

    // A paragraph style named "heading N", which Mammoth maps to <hN>.
    private static string EnsureHeadingStyle(WordprocessingDocument document, int level)
    {
        var id = $"MdEditorHeading{level}";
        var part = document.MainDocumentPart!.StyleDefinitionsPart ?? document.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
        part.Styles ??= new Styles();
        if (!part.Styles.Elements<Style>().Any(style => style.StyleId?.Value == id))
        {
            part.Styles.Append(new Style(new StyleName { Val = $"heading {level}" }) { Type = StyleValues.Paragraph, StyleId = id });
        }

        return id;
    }
}
