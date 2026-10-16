/*
  File: NotebookFormat.cs
  Description: Jupyter Notebook (.ipynb) as Markdown, and Markdown as a notebook
  Author: 5garashi.com Design Office
  Created: 2026-10-16
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;

namespace MdEditor;

// A notebook opens as Markdown: Markdown cells as they are (with their
// attached pictures), code cells as code blocks in the notebook's language,
// and outputs below them as "output" blocks, pictures, or tables. Saving as a
// notebook turns code blocks back into code cells and the rest into Markdown
// cells; outputs are left out, since running the notebook makes them again.
internal static class NotebookFormat
{
    public const string OutputInfo = "output";

    // Put around an output shown as a picture, table, or Markdown (comments are
    // not shown in the preview), so saving as a notebook can leave it out.
    private const string OutputStart = "<!-- output -->";
    private const string OutputEnd = "<!-- /output -->";

    private static string Marked(string shown) => $"{OutputStart}\n\n{shown}\n\n{OutputEnd}";

    public sealed record Result(string Markdown, int Images);

    public static Result ToMarkdown(string text, string assets, string imagesName)
    {
        var root = JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true }) as JsonObject
            ?? throw new FormatException("The notebook is not a JSON object.");
        if (root["cells"] is not JsonArray cells)
        {
            throw new FormatException("Only notebooks in format 4 (with a \"cells\" list) can be opened.");
        }

        var language = (root["metadata"]?["language_info"]?["name"] ?? root["metadata"]?["kernelspec"]?["language"])?.GetValue<string>() ?? "python";
        var images = 0;
        string SaveImage(string base64, string extension)
        {
            Directory.CreateDirectory(Path.Combine(assets, imagesName));
            var file = $"image{++images}{extension}";
            File.WriteAllBytes(Path.Combine(assets, imagesName, file), Convert.FromBase64String(Regex.Replace(base64, @"\s", string.Empty)));
            return $"{Uri.EscapeDataString(imagesName)}/{file}";
        }

        var parts = new List<string>();
        foreach (var cell in cells.OfType<JsonObject>())
        {
            var source = Join(cell["source"]).TrimEnd();
            switch (cell["cell_type"]?.GetValue<string>())
            {
                case "markdown":
                    // Pictures pasted into a Markdown cell are kept inside the notebook.
                    if (cell["attachments"] is JsonObject attachments)
                    {
                        foreach (var (name, value) in attachments)
                        {
                            var data = value as JsonObject;
                            var (mime, content) = data?.FirstOrDefault(pair => pair.Key.StartsWith("image/", StringComparison.Ordinal)) ?? default;
                            if (content is null)
                            {
                                continue;
                            }

                            var path = SaveImage(Join(content), Extension(mime));
                            source = source.Replace($"attachment:{name}", path).Replace($"attachment:{Uri.EscapeDataString(name)}", path);
                        }
                    }

                    if (source.Length > 0)
                    {
                        parts.Add(source);
                    }

                    break;
                case "code":
                    parts.Add(Fence(source, language));
                    foreach (var output in (cell["outputs"] as JsonArray ?? []).OfType<JsonObject>())
                    {
                        if (Output(output, SaveImage) is { Length: > 0 } shown)
                        {
                            parts.Add(shown);
                        }
                    }

                    break;
                case "raw":
                    if (source.Length > 0)
                    {
                        parts.Add(Fence(source, string.Empty));
                    }

                    break;
            }
        }

        return new Result(string.Join("\n\n", parts) + "\n", images);
    }

    private static string? Output(JsonObject output, Func<string, string, string> saveImage)
    {
        switch (output["output_type"]?.GetValue<string>())
        {
            case "stream":
                return Fence(Join(output["text"]).TrimEnd(), OutputInfo);
            case "error":
                var trace = output["traceback"] is JsonArray lines
                    ? string.Join("\n", lines.Select(line => line?.GetValue<string>() ?? string.Empty))
                    : $"{output["ename"]}: {output["evalue"]}";
                // Tracebacks carry terminal colors; only the text is kept.
                return Fence(Regex.Replace(trace, @"\x1B\[[0-9;]*[A-Za-z]", string.Empty).TrimEnd(), OutputInfo);
            case "execute_result" or "display_data" when output["data"] is JsonObject data:
                foreach (var mime in new[] { "image/png", "image/jpeg", "image/gif" })
                {
                    if (data[mime] is { } image)
                    {
                        return Marked($"![]({saveImage(Join(image), Extension(mime))})");
                    }
                }

                if (data["text/markdown"] is { } markdown)
                {
                    return Marked(Join(markdown).Trim());
                }

                // An HTML result (such as a pandas table) comes back as a Markdown table.
                if (data["text/html"] is { } html)
                {
                    var converted = HtmlImport.ToMarkdown(Join(html), null, _ => false).Markdown.Trim();
                    if (converted.Length > 0)
                    {
                        return Marked(converted);
                    }
                }

                return data["text/plain"] is { } plain ? Fence(Join(plain).TrimEnd(), OutputInfo) : null;
            default:
                return null;
        }
    }

    // Markdown back to a notebook: each top-level code block with a language
    // (other than diagrams and outputs) is a code cell; everything between is
    // a Markdown cell, kept exactly as written.
    public static string FromMarkdown(string markdown, MarkdownPipeline pipeline)
    {
        markdown = markdown.Replace("\r\n", "\n");
        var cells = new JsonArray();
        string? language = null;
        var pending = new StringBuilder();
        var position = 0;

        void AddCell(string type, string source, string? id = null)
        {
            source = source.Trim('\n');
            if (source.Trim().Length == 0)
            {
                return;
            }

            var cell = new JsonObject
            {
                ["cell_type"] = type,
                ["id"] = id ?? Guid.NewGuid().ToString("N")[..8],
                ["metadata"] = new JsonObject(),
                ["source"] = new JsonArray(SplitLines(source).Select(line => (JsonNode?)JsonValue.Create(line)).ToArray())
            };
            if (type == "code")
            {
                cell["execution_count"] = null;
                cell["outputs"] = new JsonArray();
            }

            cells.Add(cell);
        }

        // The Markdown written so far becomes a cell, and the text resumes after this block.
        void Cut(Block block)
        {
            pending.Append(markdown, position, Math.Max(0, block.Span.Start - position));
            position = Math.Min(markdown.Length, block.Span.End + 1);
            AddCell("markdown", pending.ToString());
            pending.Clear();
        }

        var inOutput = false;
        foreach (var block in Markdown.Parse(markdown, pipeline))
        {
            // Outputs ("output" blocks, and whatever is between the output marks)
            // are left out: running the notebook makes them again.
            var mark = block is HtmlBlock html ? html.Lines.ToString().Trim() : null;
            var info = block is FencedCodeBlock code ? (code.Info ?? string.Empty).Trim().ToLowerInvariant() : string.Empty;
            if (inOutput || mark == OutputStart || info == OutputInfo)
            {
                Cut(block);
                inOutput = mark == OutputStart || (inOutput && mark != OutputEnd);
                continue;
            }

            if (block is not FencedCodeBlock fenced || info.Length == 0 || info is "mermaid" or "math" or "text" or "markdown"
                || (language is not null && info != language))
            {
                continue;
            }

            Cut(block);
            language ??= info;
            AddCell("code", string.Join("\n", fenced.Lines.Lines.Take(fenced.Lines.Count).Select(line => line.Slice.ToString())));
        }

        pending.Append(markdown, position, markdown.Length - position);
        AddCell("markdown", pending.ToString());

        language ??= "python";
        var notebook = new JsonObject
        {
            ["cells"] = cells,
            ["metadata"] = new JsonObject
            {
                ["kernelspec"] = language == "python"
                    ? new JsonObject { ["display_name"] = "Python 3", ["language"] = "python", ["name"] = "python3" }
                    : new JsonObject { ["display_name"] = language, ["language"] = language, ["name"] = language },
                ["language_info"] = new JsonObject { ["name"] = language }
            },
            ["nbformat"] = 4,
            ["nbformat_minor"] = 5
        };
        return notebook.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }).Replace("\r\n", "\n") + "\n";
    }

    // Notebook text is a string or a list of lines.
    private static string Join(JsonNode? node) => node switch
    {
        JsonArray lines => string.Concat(lines.Select(line => line?.GetValue<string>())),
        JsonValue value when value.TryGetValue<string>(out var text) => text,
        _ => string.Empty
    };

    private static IEnumerable<string> SplitLines(string text)
    {
        var lines = text.Split('\n');
        return lines.Select((line, index) => index + 1 < lines.Length ? line + "\n" : line);
    }

    private static string Extension(string mime) => mime switch
    {
        "image/jpeg" => ".jpg",
        "image/gif" => ".gif",
        _ => ".png"
    };

    private static string Fence(string text, string info)
    {
        var longest = Regex.Matches(text, "`+").Select(match => match.Length).DefaultIfEmpty(0).Max();
        var fence = new string('`', Math.Max(3, longest + 1));
        return $"{fence}{info}\n{text.Replace("\r\n", "\n")}\n{fence}";
    }
}
