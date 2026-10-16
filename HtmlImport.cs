/*
  File: HtmlImport.cs
  Description: Opens HTML as Markdown: the source embedded by mdEditor's Save as HTML, or a conversion
  Author: 5garashi.com Design Office
  Created: 2026-10-15
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace MdEditor;

internal static class HtmlImport
{
    // Save as HTML can keep the Markdown in an inert data block (a script of a
    // non-JavaScript type is neither shown nor run), so the file opens again
    // exactly as written; the same idea as draw.io's "include a copy of my diagram".
    private const string SourceId = "mdeditor-source";

    // The JSON encoder escapes <, >, and &, so the text can never end the script element.
    private static readonly JsonSerializerOptions SourceJson = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static string SourceBlock(string markdown) =>
        $"<script type=\"application/json\" id=\"{SourceId}\">"
        + JsonSerializer.Serialize(new { format = "markdown", version = 1, text = markdown }, SourceJson)
        + "</script>";

    // A page that only forwards elsewhere (<meta http-equiv="refresh" content="0;url=...">),
    // as "latest report" pages often do: the target, when it is a relative address.
    public static string? RefreshTarget(string html)
    {
        var document = new HtmlParser().ParseDocument(html);
        foreach (var meta in document.QuerySelectorAll("meta[http-equiv]"))
        {
            if (!string.Equals(meta.GetAttribute("http-equiv"), "refresh", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var content = meta.GetAttribute("content") ?? string.Empty;
            var index = content.IndexOf("url=", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                continue;
            }

            var target = content[(index + 4)..].Trim().Trim('\'', '"');
            // Web addresses are not followed; only files beside this one.
            if (target.Length > 0 && !Uri.TryCreate(target, UriKind.Absolute, out _))
            {
                return target;
            }
        }

        return null;
    }

    // The Markdown for an HTML file, and whether it is the embedded original
    // (exact) rather than a conversion (headings, lists, tables, links, and
    // images survive; colors, layout, and drawn diagrams do not).
    public static (string Markdown, bool Exact) ToMarkdown(string html, Uri? pageUrl = null, Func<string, bool>? existsLocally = null)
    {
        var document = new HtmlParser().ParseDocument(html);
        if (document.GetElementById(SourceId) is { } source && TryReadSource(source.TextContent, out var original))
        {
            return (original, true);
        }

        // Relative images and links that are not beside the file (a page from the
        // internet, or its copy in the browser cache) resolve from the page's own
        // address: the page that was opened, <base>, canonical, or og:url.
        if ((pageUrl ?? FindPageAddress(document)) is { } baseUrl)
        {
            foreach (var (selector, attribute) in new[] { ("img[src]", "src"), ("a[href]", "href") })
            {
                foreach (var element in document.QuerySelectorAll(selector))
                {
                    var value = element.GetAttribute(attribute)!.Trim();
                    if (value.Length == 0 || value.StartsWith('#') || Uri.TryCreate(value, UriKind.Absolute, out _)
                        || existsLocally?.Invoke(value) == true)
                    {
                        continue;
                    }

                    if (Uri.TryCreate(baseUrl, value, out var resolved))
                    {
                        element.SetAttribute(attribute, resolved.AbsoluteUri);
                    }
                }
            }
        }

        // Web components keep their content in declarative shadow roots
        // (<template shadowrootmode>), which browsers show in place; so does this.
        for (var pass = 0; pass < 20; pass++)
        {
            var roots = document.QuerySelectorAll("template[shadowrootmode], template[shadowroot]").ToList();
            if (roots.Count == 0)
            {
                break;
            }

            foreach (var root in roots)
            {
                if (root is not AngleSharp.Html.Dom.IHtmlTemplateElement template || root.Parent is not { } host)
                {
                    continue;
                }

                // The host's own children (light DOM) go into the shadow root's
                // slots, as a browser composes them: a named slot takes children
                // with slot="name", the default slot the rest, and an empty slot
                // shows its fallback content. Unassigned children are not shown.
                root.Remove();
                var light = host.ChildNodes.ToList();
                foreach (var slot in template.Content.QuerySelectorAll("slot").ToList())
                {
                    var name = slot.GetAttribute("name");
                    var assigned = light.Where(node => node is IElement element
                            ? element.GetAttribute("slot") == name || (name is null && !element.HasAttribute("slot"))
                            : name is null).ToList();
                    var replacement = assigned.Count > 0 ? assigned : slot.ChildNodes.ToList();
                    foreach (var node in replacement)
                    {
                        slot.Parent!.InsertBefore(node, slot);
                    }

                    light.RemoveAll(assigned.Contains);
                    slot.Remove();
                }

                foreach (var node in light)
                {
                    node.Parent?.RemoveChild(node);
                }

                while (template.Content.FirstChild is { } child)
                {
                    host.AppendChild(child);
                }
            }
        }

        // Nothing that runs or embeds survives (Identity 13-3): scripts, frames,
        // embedded objects, and event attributes (the converter drops attributes
        // anyway; removing them here keeps that explicit).
        foreach (var element in document.QuerySelectorAll("*"))
        {
            foreach (var attribute in element.Attributes.Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)).ToList())
            {
                element.RemoveAttribute(attribute.Name);
            }
        }

        foreach (var element in document.QuerySelectorAll("script, style, noscript, template, svg, iframe, frame, frameset, object, embed, applet, noembed").ToList())
        {
            element.Remove();
        }

        // Links that were only an icon (now removed) would become empty [](...).
        foreach (var link in document.QuerySelectorAll("a").ToList())
        {
            if (string.IsNullOrWhiteSpace(link.TextContent) && link.QuerySelector("img") is null)
            {
                link.Remove();
            }
        }

        // Math (MathML from KaTeX keeps its TeX as an annotation) comes back as $...$.
        // Placeholders keep the converter from escaping TeX characters such as _.
        var math = new List<string>();
        foreach (var element in document.QuerySelectorAll("math").ToList())
        {
            var tex = element.QuerySelector("annotation[encoding='application/x-tex']")?.TextContent.Trim();
            if (string.IsNullOrEmpty(tex))
            {
                element.Replace(document.CreateTextNode(element.TextContent));
                continue;
            }

            var block = element.GetAttribute("display") == "block";
            math.Add(block ? $"$$\n{tex}\n$$" : $"${tex}$");
            var token = $"MDEDITORMATH{math.Count - 1}X";
            if (block)
            {
                var paragraph = document.CreateElement("p");
                paragraph.TextContent = token;
                element.Replace(paragraph);
            }
            else
            {
                element.Replace(document.CreateTextNode(token));
            }
        }

        // Diagrams were drawn pictures (removed above); their boxes go too. Layout
        // wrappers keep only their content, since the converter would otherwise
        // pass a whole <main> or <div> through as raw HTML.
        foreach (var element in document.QuerySelectorAll(".mermaid").ToList())
        {
            element.Remove();
        }

        foreach (var element in document.QuerySelectorAll("main, article, section, header, footer, nav, aside, div, span").ToList())
        {
            if (element.Parent is null)
            {
                continue;
            }

            while (element.FirstChild is { } child)
            {
                element.Parent.InsertBefore(child, element);
            }

            element.Remove();
        }

        // Embedded (data:) images cannot be kept in Markdown text; their description stays.
        foreach (var image in document.QuerySelectorAll("img[src^='data:']").ToList())
        {
            image.Replace(document.CreateTextNode(image.GetAttribute("alt") ?? string.Empty));
        }

        var converter = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
        {
            // The default output already uses GitHub tables; the GitHub flavor setting
            // (6.2.1) leaves the HTML unconverted.
            Formatting = { RemoveComments = true },
            Links = { SmartHref = true },
            // Wrappers such as <div class="math"> keep only their content.
            Tags = { Unknown = ReverseMarkdown.Config.UnknownTagsOption.Bypass }
        });
        var markdown = converter.Convert(document.Body?.InnerHtml ?? html);
        for (var index = 0; index < math.Count; index++)
        {
            markdown = markdown.Replace($"MDEDITORMATH{index}X", math[index]);
        }

        return (markdown.Trim() + "\n", false);
    }

    private static Uri? FindPageAddress(IDocument document)
    {
        var candidates = new[]
        {
            document.QuerySelector("base[href]")?.GetAttribute("href"),
            document.QuerySelector("link[rel~='canonical'][href]")?.GetAttribute("href"),
            document.QuerySelector("meta[property='og:url'][content]")?.GetAttribute("content")
        };
        foreach (var candidate in candidates)
        {
            if (Uri.TryCreate(candidate?.Trim(), UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http")
            {
                return uri;
            }
        }

        return null;
    }

    private static bool TryReadSource(string json, out string text)
    {
        text = string.Empty;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("format", out var format) || format.GetString() != "markdown"
                || !root.TryGetProperty("text", out var value) || value.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            text = value.GetString() ?? string.Empty;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
