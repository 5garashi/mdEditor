/*
  File: TextFormats.cs
  Description: Plain text without Markdown marks, CSV and TSV tables, and e-mail (.eml) as Markdown
  Author: 5garashi.com Design Office
  Created: 2026-10-16
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MimeKit;
using MdTable = Markdig.Extensions.Tables.Table;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using MdTableCell = Markdig.Extensions.Tables.TableCell;

namespace MdEditor;

// A table of contents: the headings as a nested list of links to them, between
// <!-- toc --> and <!-- /toc --> so pressing Contents again replaces it. A single
// title (one level-1 heading at the top) is left out, and three levels are kept.
internal static class TableOfContents
{
    public const string Start = "<!-- toc -->";
    public const string End = "<!-- /toc -->";

    public static string? Build(string markdown, MarkdownPipeline pipeline)
    {
        var headings = Markdown.Parse(markdown, pipeline).OfType<HeadingBlock>()
            .Select(heading => (heading.Level, Text: Text(heading.Inline), Id: Markdig.Renderers.Html.HtmlAttributesExtensions.GetAttributes(heading).Id))
            .Where(heading => heading.Text.Length > 0 && !string.IsNullOrEmpty(heading.Id))
            .ToList();
        if (headings.Count > 0 && headings[0].Level == 1 && headings.Count(heading => heading.Level == 1) == 1)
        {
            headings.RemoveAt(0);
        }

        if (headings.Count == 0)
        {
            return null;
        }

        var top = headings.Min(heading => heading.Level);
        var lines = headings.Where(heading => heading.Level < top + 3).Select(heading =>
            $"{new string(' ', (heading.Level - top) * 2)}- [{Regex.Replace(heading.Text, @"([\\\[\]])", @"\$1")}](#{heading.Id})");
        return $"{Start}\n{string.Join("\n", lines)}\n{End}";
    }

    private static string Text(ContainerInline? container) => container is null ? string.Empty : string.Concat(container.Descendants<Inline>().Select(inline => inline switch
    {
        LiteralInline literal => literal.Content.ToString(),
        CodeInline code => code.Content,
        _ => string.Empty
    })).Trim();
}

// Front matter (the YAML block between --- lines at the top), read as YAML and
// shown the way GitHub shows it: a map is a table with its keys as headers and
// its values in one row, a list is a one-row table, and nested maps and lists
// become tables inside the cells.
internal abstract record YamlValue;
internal sealed record YamlText(string Text) : YamlValue;
internal sealed record YamlMap(List<(string Key, YamlValue Value)> Entries) : YamlValue;
internal sealed record YamlList(List<YamlValue> Items) : YamlValue;

internal static class FrontMatter
{
    // Null when the block is not valid YAML (it is then shown as written).
    public static YamlValue? Read(IEnumerable<string> lines)
    {
        var text = string.Join("\n", lines.Where(line => line.Trim() is not ("---" or "...")));
        try
        {
            var stream = new YamlDotNet.RepresentationModel.YamlStream();
            stream.Load(new StringReader(text));
            return stream.Documents.Count == 0 ? null : Convert(stream.Documents[0].RootNode);
        }
        catch (YamlDotNet.Core.YamlException)
        {
            return null;
        }
    }

    private static YamlValue Convert(YamlDotNet.RepresentationModel.YamlNode node) => node switch
    {
        YamlDotNet.RepresentationModel.YamlMappingNode map => new YamlMap(map.Children
            .Select(pair => (Key: pair.Key is YamlDotNet.RepresentationModel.YamlScalarNode key ? key.Value ?? string.Empty : pair.Key.ToString(), Value: Convert(pair.Value)))
            .ToList()),
        YamlDotNet.RepresentationModel.YamlSequenceNode list => new YamlList(list.Children.Select(Convert).ToList()),
        YamlDotNet.RepresentationModel.YamlScalarNode scalar => new YamlText(scalar.Value ?? string.Empty),
        _ => new YamlText(string.Empty)
    };

    // In the preview, a card of "key  value" rows: a list of words (tags and so
    // on) as small labels, nested maps as rows inside the value, and web
    // addresses as links.
    public static string ToHtml(YamlValue value) => value switch
    {
        YamlMap map => "<dl class=\"front-matter\">"
            + string.Concat(map.Entries.Select(entry => $"<div><dt>{WebUtility.HtmlEncode(entry.Key)}</dt><dd>{ToHtml(entry.Value)}</dd></div>"))
            + "</dl>",
        YamlList list when list.Items.All(item => item is YamlText) => "<ul class=\"front-matter-labels\">"
            + string.Concat(list.Items.Select(item => $"<li>{ToHtml(item)}</li>"))
            + "</ul>",
        YamlList list => "<ul class=\"front-matter-items\">"
            + string.Concat(list.Items.Select(item => $"<li>{ToHtml(item)}</li>"))
            + "</ul>",
        YamlText { Text: var text } when IsWebAddress(text) => $"<a href=\"{WebUtility.HtmlEncode(text)}\">{WebUtility.HtmlEncode(text)}</a>",
        YamlText text => WebUtility.HtmlEncode(text.Text).Replace("\n", "<br>"),
        _ => string.Empty
    };

    private static bool IsWebAddress(string text) =>
        Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http" && !text.Trim().Contains(' ');

    // For text output: "key: value", nested values indented below their key.
    public static string ToText(YamlValue value, string indent = "") => value switch
    {
        YamlMap map => string.Join("\n", map.Entries.Select(entry => entry.Value is YamlText text
            ? $"{indent}{entry.Key}: {text.Text.Replace('\n', ' ')}"
            : $"{indent}{entry.Key}:\n{ToText(entry.Value, indent + "  ")}")),
        YamlList list => string.Join("\n", list.Items.Select(item => item is YamlText text
            ? $"{indent}- {text.Text.Replace('\n', ' ')}"
            : $"{indent}-\n{ToText(item, indent + "  ")}")),
        YamlText text => indent + text.Text,
        _ => string.Empty
    };
}

// Code fences the preview draws as pictures: Mermaid, Graphviz (DOT), 3D models
// (STL), and maps (GeoJSON, TopoJSON).
internal static class DiagramFences
{
    public static bool IsDiagram(string? info) =>
        (info ?? string.Empty).Trim().ToLowerInvariant() is "mermaid" or "dot" or "graphviz" or "gv" or "stl" or "geojson" or "topojson";

    // ```math blocks are drawn as display math, as on GitHub.
    public static bool IsMath(string? info) =>
        string.Equals((info ?? string.Empty).Trim(), "math", StringComparison.OrdinalIgnoreCase);

    // The title GitHub shows for an alert (> [!NOTE] and so on).
    public static string AlertTitle(Markdig.Extensions.Alerts.AlertBlock alert)
    {
        var kind = alert.Kind.ToString().ToLowerInvariant();
        return kind.Length == 0 ? string.Empty : char.ToUpperInvariant(kind[0]) + kind[1..];
    }

    // GitHub also writes inline math as $`x`$; the backticks are not part of it.
    public static string InlineMath(string tex) =>
        tex.Length >= 2 && tex[0] == '`' && tex[^1] == '`' ? tex[1..^1] : tex;
}

// Text as the preview shows it: the words without Markdown's marks. Lines stay
// where they were written, lists keep a bullet or number, tables become
// tab-separated rows, and links keep their address in parentheses.
internal static class PlainTextExport
{
    public static string Convert(string markdown, MarkdownPipeline pipeline, bool japanese)
    {
        var context = new Context(japanese);
        var parts = Markdown.Parse(markdown, pipeline)
            .Select(block => Block(block, context))
            .Where(text => !string.IsNullOrWhiteSpace(text));
        return string.Join("\r\n\r\n", parts).Replace("\r\n", "\n").Replace("\n", "\r\n") + "\r\n";
    }

    private sealed record Context(bool Japanese);

    private static string? Block(Block block, Context context)
    {
        switch (block)
        {
            case HeadingBlock heading:
                return Inlines(heading.Inline, context);
            case ParagraphBlock paragraph:
                return Inlines(paragraph.Inline, context);
            case MathBlock math:
                return Lines(math);
            case FencedCodeBlock fenced when DiagramFences.IsDiagram(fenced.Info):
                return context.Japanese ? "［図］" : "[Figure]";
            case Markdig.Extensions.Yaml.YamlFrontMatterBlock yaml:
                var yamlLines = yaml.Lines.Lines.Take(yaml.Lines.Count).Select(line => line.Slice.ToString()).ToList();
                return FrontMatter.Read(yamlLines) is { } matter
                    ? FrontMatter.ToText(matter)
                    : string.Join("\n", yamlLines.Where(line => line.Trim() != "---"));
            case HtmlBlock html:
                return StripHtml(Lines(html));
            case CodeBlock code:
                return Lines(code);
            case ThematicBreakBlock:
                return new string('─', 20);
            case Markdig.Extensions.Alerts.AlertBlock alert:
                return Indent(DiagramFences.AlertTitle(alert) + "\n" + Children(alert, context, "\n\n"), "    ", "    ");
            case QuoteBlock quote:
                return Indent(Children(quote, context, "\n\n"), "    ", "    ");
            case ListBlock list:
                return List(list, context);
            case MdTable table:
                return string.Join("\n", Rows(table, context).Select(row => string.Join("\t", row.Select(cell => cell.Replace('\n', ' ')))));
            case Footnote footnote:
                return $"[{footnote.Order}] " + Children(footnote, context, "\n");
            case ContainerBlock container:
                return Children(container, context, "\n\n");
            default:
                return null;
        }
    }

    private static string Children(ContainerBlock container, Context context, string separator) =>
        string.Join(separator, container.Select(child => Block(child, context)).Where(text => !string.IsNullOrWhiteSpace(text)));

    private static string List(ListBlock list, Context context)
    {
        var number = int.TryParse(list.OrderedStart, out var start) ? start : 1;
        var items = new List<string>();
        foreach (var item in list.OfType<ListItemBlock>())
        {
            var marker = list.IsOrdered ? $"{number++}{list.OrderedDelimiter} " : context.Japanese ? "・" : "• ";
            var body = Children(item, context, list.IsLoose ? "\n\n" : "\n");
            items.Add(Indent(body, marker, new string(' ', list.IsOrdered ? marker.Length : 2)));
        }

        return string.Join(list.IsLoose ? "\n\n" : "\n", items);
    }

    // Every row of a table as plain cells, the header first (for CSV and TSV too).
    public static List<List<string>> Rows(MdTable table, bool japanese) => Rows(table, new Context(japanese));

    private static List<List<string>> Rows(MdTable table, Context context) =>
        table.OfType<MdTableRow>()
            .Select(row => row.OfType<MdTableCell>().Select(cell => Children(cell, context, " ").Trim()).ToList())
            .ToList();

    private static string Inlines(ContainerInline? container, Context context)
    {
        if (container is null)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    text.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    text.Append(code.Content);
                    break;
                case LineBreakInline:
                    // Lines stay as they were written, as in a text file.
                    text.Append('\n');
                    break;
                case TaskList task:
                    text.Append(task.Checked ? "☑" : "☐");
                    break;
                case MathInline math:
                    text.Append(DiagramFences.InlineMath(math.Content.ToString()));
                    break;
                case LinkInline { IsImage: true } image:
                    var alt = Inlines(image, context);
                    text.Append(context.Japanese ? $"［{(alt.Length > 0 ? alt : "画像")}］" : $"[{(alt.Length > 0 ? alt : "Image")}]");
                    break;
                case LinkInline link:
                    text.Append(LinkText(Inlines(link, context), link.Url, context));
                    break;
                case AutolinkInline autolink:
                    text.Append(autolink.Url);
                    break;
                case HtmlEntityInline entity:
                    text.Append(entity.Transcoded.ToString());
                    break;
                case HtmlInline html when Regex.IsMatch(html.Tag, @"^<br\b", RegexOptions.IgnoreCase):
                    text.Append('\n');
                    break;
                case FootnoteLink { IsBackLink: false } footnote:
                    text.Append($"[{footnote.Index}]");
                    break;
                case ContainerInline nested:
                    text.Append(Inlines(nested, context));
                    break;
            }
        }

        return text.ToString();
    }

    private static string LinkText(string text, string? url, Context context)
    {
        if (string.IsNullOrWhiteSpace(url) || url.StartsWith('#'))
        {
            return text;
        }

        var address = url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ? url[7..] : url;
        if (text.Length == 0 || text == address || text == url)
        {
            return address;
        }

        return context.Japanese ? $"{text}（{address}）" : $"{text} ({address})";
    }

    private static string Lines(LeafBlock block) =>
        string.Join("\n", block.Lines.Lines.Take(block.Lines.Count).Select(line => line.Slice.ToString()));

    public static string? StripHtml(string html)
    {
        var text = Regex.Replace(html, "<!--.*?-->", string.Empty, RegexOptions.Singleline);
        text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", string.Empty)).Trim();
        return text.Length > 0 ? text : null;
    }

    private static string Indent(string text, string first, string rest)
    {
        var lines = text.Split('\n');
        return string.Join("\n", lines.Select((line, index) => (index == 0 ? first : line.Length > 0 ? rest : string.Empty) + line));
    }
}

// CSV and TSV: a table file opens as a Markdown table (the first row is the
// header), and the tables in a document save as CSV or TSV files.
internal static class TableFiles
{
    public static bool IsTablePath(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".csv" or ".tsv";

    public static string ToMarkdown(string text, char separator) => RowsToMarkdown(Parse(text, separator));

    // Rows of cells as a Markdown table, the first row as the header.
    public static string RowsToMarkdown(List<List<string>> rows)
    {
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var columns = rows.Max(row => row.Count);
        string Cell(string value) => value.Trim()
            .Replace("\\", "\\\\").Replace("|", "\\|")
            .Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
        string Row(List<string> row) =>
            "| " + string.Join(" | ", Enumerable.Range(0, columns).Select(i => i < row.Count ? Cell(row[i]) : string.Empty)) + " |";

        var lines = new List<string> { Row(rows[0]), "|" + string.Concat(Enumerable.Repeat(" --- |", columns)) };
        lines.AddRange(rows.Skip(1).Select(Row));
        return string.Join("\n", lines) + "\n";
    }

    // RFC 4180: quoted fields may hold separators, quotes ("") and line breaks.
    private static List<List<string>> Parse(string text, char separator)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var fieldStarted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    field.Append(c);
                }
            }
            else if (c == '"' && !fieldStarted)
            {
                quoted = true;
                fieldStarted = true;
            }
            else if (c == separator)
            {
                row.Add(field.ToString());
                field.Clear();
                fieldStarted = false;
            }
            else if (c is '\r' or '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                row.Add(field.ToString());
                field.Clear();
                fieldStarted = false;
                rows.Add(row);
                row = [];
            }
            else
            {
                field.Append(c);
                fieldStarted = true;
            }
        }

        if (fieldStarted || field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        // Blank lines are not rows.
        return rows.Where(r => r.Any(value => value.Length > 0)).ToList();
    }

    public static List<List<List<string>>> Tables(string markdown, MarkdownPipeline pipeline, bool japanese) =>
        Markdown.Parse(markdown, pipeline).Descendants<MdTable>()
            .Select(table => PlainTextExport.Rows(table, japanese))
            .ToList();

    public static string Write(List<List<string>> rows, char separator)
    {
        string Field(string value)
        {
            if (separator == '\t')
            {
                // TSV has no quoting: tabs and line breaks in a cell become spaces.
                return Regex.Replace(value, @"[\t\r\n]+", " ");
            }

            return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 || value != value.Trim()
                ? "\"" + value.Replace("\"", "\"\"") + "\""
                : value;
        }

        return string.Concat(rows.Select(row => string.Join(separator, row.Select(Field)) + "\r\n"));
    }
}

// JSON and XML. A JSON list of records (an array of objects) opens as a table;
// any other JSON, and XML, opens as a code block to read and annotate. The
// tables in a document save as JSON: one array of objects per table, keyed by
// the header row (values stay text, so codes such as 007 keep their zeros).
internal static class DataFiles
{
    public static bool IsDataPath(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".json" or ".xml";

    // Returns the Markdown and its kind: "table", "code", or "invalid" (not
    // valid JSON, so the text is kept as it is in a code block).
    public static (string Markdown, string Kind) JsonToMarkdown(string text)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        }
        catch (JsonException)
        {
            return (Fence(text.Trim(), "json"), "invalid");
        }

        if (root is JsonArray array && array.Count > 0 && array.All(item => item is JsonObject))
        {
            var keys = array.Cast<JsonObject>().SelectMany(item => item.Select(pair => pair.Key)).Distinct().ToList();
            var rows = new List<List<string>> { keys };
            rows.AddRange(array.Cast<JsonObject>().Select(item => keys.Select(key => item[key] switch
            {
                null => string.Empty,
                JsonValue value when value.TryGetValue<string>(out var textValue) => textValue,
                // A list of plain values reads as "a, b"; anything deeper stays JSON.
                JsonArray list when list.All(entry => entry is JsonValue) =>
                    string.Join(", ", list.Select(entry => entry!.GetValueKind() == JsonValueKind.String ? entry.GetValue<string>() : entry.ToJsonString())),
                var other => other.ToJsonString(Compact)
            }).ToList()));
            return (TableFiles.RowsToMarkdown(rows), "table");
        }

        return (Fence(root?.ToJsonString(Indented) ?? "null", "json"), "code");
    }

    public static (string Markdown, string Kind) XmlToMarkdown(string text)
    {
        try
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            using var reader = XmlReader.Create(new StringReader(text), settings);
            while (reader.Read())
            {
            }

            return (Fence(text.Trim(), "xml"), "code");
        }
        catch (XmlException)
        {
            return (Fence(text.Trim(), "xml"), "invalid");
        }
    }

    public static string TableToJson(List<List<string>> rows)
    {
        var header = rows.Count > 0 ? rows[0] : [];
        var keys = new List<string>();
        for (var i = 0; i < header.Count; i++)
        {
            var key = header[i].Length > 0 ? header[i] : $"column{i + 1}";
            var name = key;
            for (var n = 2; keys.Contains(name); n++)
            {
                name = $"{key}_{n}";
            }

            keys.Add(name);
        }

        var array = new JsonArray();
        foreach (var row in rows.Skip(1))
        {
            var item = new JsonObject();
            for (var i = 0; i < keys.Count; i++)
            {
                item[keys[i]] = i < row.Count ? row[i] : string.Empty;
            }

            array.Add(item);
        }

        return array.ToJsonString(Indented).Replace("\r\n", "\n") + "\n";
    }

    private static readonly JsonSerializerOptions Indented = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions Compact = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // A fence longer than any run of backticks in the text, so nothing ends it early.
    private static string Fence(string text, string language)
    {
        var longest = Regex.Matches(text, "`+").Select(match => match.Length).DefaultIfEmpty(0).Max();
        var fence = new string((char)96, Math.Max(3, longest + 1));
        return $"{fence}{language}\n{text.Replace("\r\n", "\n")}\n{fence}\n";
    }
}

// An e-mail (.eml) opens as Markdown: the subject as the heading, the sender,
// recipients, date, and attachments as a list, then the body (its HTML through
// the HTML conversion, or its text as written). Pictures in the mail go to a
// temporary folder like a Word document's.
internal static class MailImport
{
    public sealed record Result(string Markdown, int Images);

    public static Result ToMarkdown(string path, string assets, string imagesName, bool japanese)
    {
        using var stream = File.OpenRead(path);
        var message = MimeMessage.Load(stream);
        var text = new StringBuilder();
        var subject = string.IsNullOrWhiteSpace(message.Subject) ? (japanese ? "（件名なし）" : "(No subject)") : message.Subject.Trim();
        text.Append("# ").Append(Escape(subject)).Append("\n\n");

        void Field(string ja, string en, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                text.Append("- ").Append(japanese ? ja : en).Append(": ").Append(Escape(value.Trim())).Append('\n');
            }
        }

        Field("差出人", "From", Addresses(message.From));
        Field("宛先", "To", Addresses(message.To));
        Field("CC", "Cc", Addresses(message.Cc));
        Field("日付", "Date", message.Date == DateTimeOffset.MinValue ? null : message.Date.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
        var attachments = message.Attachments
            .Select(part => part is MimePart mime ? mime.FileName : part is MessagePart rfc ? rfc.Message?.Subject + ".eml" : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();
        Field("添付", "Attachments", attachments.Count > 0 ? string.Join(", ", attachments) : null);
        text.Append('\n');

        var images = 0;
        if (message.HtmlBody is { } html)
        {
            // Pictures referred to by cid: are saved beside the document's images.
            var byId = message.BodyParts.OfType<MimePart>()
                .Where(part => !string.IsNullOrEmpty(part.ContentId) && part.ContentType.MediaType.Equals("image", StringComparison.OrdinalIgnoreCase))
                .GroupBy(part => part.ContentId!.Trim('<', '>'), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            var saved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            html = Regex.Replace(html, @"cid:([^""'\s>)]+)", match =>
            {
                var id = WebUtility.UrlDecode(match.Groups[1].Value);
                if (saved.TryGetValue(id, out var known))
                {
                    return known;
                }

                if (!byId.TryGetValue(id, out var part) || part.Content is null)
                {
                    return match.Value;
                }

                var extension = part.ContentType.MimeType.ToLowerInvariant() switch
                {
                    "image/jpeg" or "image/jpg" => ".jpg",
                    "image/gif" => ".gif",
                    "image/bmp" => ".bmp",
                    "image/webp" => ".webp",
                    _ => ".png"
                };
                var file = $"image{++images}{extension}";
                Directory.CreateDirectory(Path.Combine(assets, imagesName));
                using (var target = File.Create(Path.Combine(assets, imagesName, file)))
                {
                    part.Content.DecodeTo(target);
                }

                return saved[id] = $"{Uri.EscapeDataString(imagesName)}/{file}";
            });
            text.Append(HtmlImport.ToMarkdown(html, null, _ => true).Markdown.Trim());
        }
        else if (message.TextBody is { } plain)
        {
            // A text mail keeps its lines: each line break is kept as a break.
            var lines = plain.Replace("\r\n", "\n").TrimEnd().Split('\n');
            text.Append(string.Join("\n", lines.Select((line, index) =>
                index + 1 < lines.Length && line.Trim().Length > 0 && lines[index + 1].Trim().Length > 0
                    ? line.TrimEnd() + "  "
                    : line.TrimEnd())));
        }

        return new Result(text.ToString().TrimEnd() + "\n", images);
    }

    private static string? Addresses(InternetAddressList list) =>
        list.Count == 0 ? null : string.Join(", ", list.Select(address => address switch
        {
            MailboxAddress mailbox when !string.IsNullOrWhiteSpace(mailbox.Name) => $"{mailbox.Name} <{mailbox.Address}>",
            MailboxAddress mailbox => mailbox.Address,
            _ => address.ToString()
        }));

    // Header text is shown as written, not read as Markdown or HTML.
    private static string Escape(string value) =>
        Regex.Replace(value.Replace("\r", " ").Replace("\n", " "), @"([\\`*_\[\]<>#|~])", @"\$1");
}
