/*
  File: HtmlImport.cs
  Description: Opens HTML as Markdown: the source embedded by mdEditor's Save as HTML, or a conversion
  Author: 5garashi.com Design Office
  Created: 2026-10-15
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AngleSharp;
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
    // images survive; colors and layout do not). Diagrams mdEditor drew come back as
    // their code when the HTML keeps it (data-mdeditor-code); other drawn pictures
    // (SVG, and images written into the page as data:) are handed to keepPicture,
    // which stores them and gives the address to use, or are left out without it.
    public static (string Markdown, bool Exact) ToMarkdown(string html, Uri? pageUrl = null, Func<string, bool>? existsLocally = null,
        Func<byte[], string, string?>? keepPicture = null)
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

        // Diagrams saved by mdEditor with their code return as code blocks (placeholders,
        // as for math below, keep the converter from escaping the code).
        var fences = new List<string>();
        foreach (var element in document.QuerySelectorAll("[data-mdeditor-fence][data-mdeditor-code]").ToList())
        {
            var language = System.Text.RegularExpressions.Regex.Replace(element.GetAttribute("data-mdeditor-fence")!, "[^A-Za-z0-9_+-]", "");
            var code = element.GetAttribute("data-mdeditor-code")!.Replace("\r\n", "\n").TrimEnd('\n');
            var longest = System.Text.RegularExpressions.Regex.Matches(code, "`+").Select(match => match.Length).DefaultIfEmpty(0).Max();
            var fence = new string('`', Math.Max(3, longest + 1));
            fences.Add($"{fence}{language}\n{code}\n{fence}");
            var paragraph = document.CreateElement("p");
            paragraph.TextContent = $"MDEDITORFENCE{fences.Count - 1}X";
            element.Replace(paragraph);
        }

        // What only works on the page (buttons, choices, closed dialogs, what is hidden,
        // canvases, and a Leaflet map's tiles and controls) is not part of the text.
        foreach (var element in document.QuerySelectorAll(
                     "button, select, input, textarea, label, dialog:not([open]), [hidden], [aria-hidden='true'], canvas, .leaflet-pane, .leaflet-control-zoom").ToList())
        {
            element.Remove();
        }

        foreach (var element in document.QuerySelectorAll("[style]").ToList())
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(element.GetAttribute("style")!, @"(^|;)\s*(display\s*:\s*none|visibility\s*:\s*hidden)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                element.Remove();
            }
        }

        if (keepPicture is not null)
        {
            KeepPictures(document, keepPicture);
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

        // A box holding only words (no headings, paragraphs, or lists inside) is a
        // paragraph of its own, so the words of boxes side by side do not run together.
        foreach (var element in document.QuerySelectorAll("main, article, section, header, footer, nav, aside, div, span").ToList())
        {
            if (element.Parent is null)
            {
                continue;
            }

            if (element.LocalName != "span" && !string.IsNullOrWhiteSpace(element.TextContent)
                && element.QuerySelector(BlockElements) is null
                && element.Closest("p, li, dt, dd, td, th, h1, h2, h3, h4, h5, h6, pre, blockquote, a, figcaption") is null)
            {
                var paragraph = document.CreateElement("p");
                while (element.FirstChild is { } child)
                {
                    paragraph.AppendChild(child);
                }

                element.Replace(paragraph);
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

        for (var index = 0; index < fences.Count; index++)
        {
            markdown = markdown.Replace($"MDEDITORFENCE{index}X", fences[index]);
        }

        return (markdown.Trim() + "\n", false);
    }

    private const string BlockElements =
        "p, div, main, article, section, header, footer, nav, aside, h1, h2, h3, h4, h5, h6, ul, ol, li, dl, table, pre, blockquote, figure, hr, form, fieldset, details";

    // Drawn pictures become picture files: each SVG drawing (a diagram mdEditor drew, or
    // any drawing at least 48 pixels on a side, so icons are left out) as an .svg of its
    // own, and each image written into the page (data:) as its file. The drawing's box
    // (.mermaid, .graphviz, .geo-map) gives way to the picture.
    private static void KeepPictures(IDocument document, Func<byte[], string, string?> keepPicture)
    {
        foreach (var svg in document.QuerySelectorAll("svg").ToList())
        {
            if (svg.Parent is null || svg.ParentElement?.Closest("svg, a, button") is not null)
            {
                continue;
            }

            var box = svg.ParentElement?.Closest(".mermaid, .graphviz, .geo-map");
            if (box is null && !LargeEnough(svg))
            {
                continue;
            }

            var alt = svg.GetAttribute("aria-label")
                      ?? svg.Children.FirstOrDefault(child => child.LocalName == "title")?.TextContent.Trim()
                      ?? (box?.ClassList.FirstOrDefault() is { } kind ? kind switch { "mermaid" => "Mermaid", "graphviz" => "Graphviz", _ => "" } : "");
            if (keepPicture(System.Text.Encoding.UTF8.GetBytes(StandaloneSvg(svg)), ".svg") is not { } address)
            {
                continue;
            }

            var image = document.CreateElement("img");
            image.SetAttribute("src", address);
            image.SetAttribute("alt", alt);
            if (box is not null)
            {
                var paragraph = document.CreateElement("p");
                paragraph.AppendChild(image);
                box.Replace(paragraph);
            }
            else
            {
                svg.Replace(image);
            }
        }

        foreach (var image in document.QuerySelectorAll("img[src^='data:']").ToList())
        {
            var match = System.Text.RegularExpressions.Regex.Match(image.GetAttribute("src")!, @"^data:image/(png|jpeg|gif|webp|svg\+xml);base64,([A-Za-z0-9+/=\s]+)$");
            if (!match.Success)
            {
                continue;
            }

            try
            {
                var extension = match.Groups[1].Value switch { "jpeg" => ".jpg", "svg+xml" => ".svg", var other => "." + other };
                if (keepPicture(Convert.FromBase64String(match.Groups[2].Value), extension) is { } address)
                {
                    image.SetAttribute("src", address);
                }
            }
            catch (FormatException)
            {
                // Not a picture after all; the description stays (below).
            }
        }
    }

    // Whether an SVG is a picture rather than an icon: its size from width and height,
    // or from its viewBox when they are missing or relative (width="100%").
    private static bool LargeEnough(IElement svg)
    {
        static double? Length(string? value) =>
            value is not null && System.Text.RegularExpressions.Regex.Match(value, @"^\s*([\d.]+)\s*(px|pt)?\s*$") is { Success: true } match
                && double.TryParse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture, out var number)
                ? number * (match.Groups[2].Value == "pt" ? 4.0 / 3 : 1)
                : null;
        var box = (svg.GetAttribute("viewBox") ?? string.Empty).Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        double? Box(int index) => box.Length == 4 && double.TryParse(box[index], System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : null;
        var width = Length(svg.GetAttribute("width")) ?? Box(2);
        var height = Length(svg.GetAttribute("height")) ?? Box(3);
        return width >= 48 && height >= 48;
    }

    // The SVG as a file of its own: written as XML (so <br> and &nbsp; inside labels stay
    // valid), with its namespaces, and with a size when the page gave it only a relative
    // one (Mermaid's width="100%" with max-width), so it shows at the size it had.
    private static string StandaloneSvg(IElement svg)
    {
        var copy = (IElement)svg.Clone(true);
        copy.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
        if (copy.OuterHtml.Contains("xlink:", StringComparison.Ordinal))
        {
            copy.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
        }

        var box = (copy.GetAttribute("viewBox") ?? string.Empty).Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => double.TryParse(part, System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : 0).ToArray();
        if (box.Length == 4 && box[2] > 0 && box[3] > 0 && (copy.GetAttribute("width")?.Contains('%') ?? true))
        {
            var widest = System.Text.RegularExpressions.Regex.Match(copy.GetAttribute("style") ?? string.Empty, @"max-width\s*:\s*([\d.]+)px");
            var width = widest.Success && double.TryParse(widest.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture, out var limit) ? limit : box[2];
            copy.SetAttribute("width", width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            copy.SetAttribute("height", (width * box[3] / box[2]).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        }

        return copy.ToHtml(AngleSharp.Xhtml.XhtmlMarkupFormatter.Instance);
    }

    // For the original of an HTML file: its stylesheets beside it (<link rel="stylesheet">)
    // put into the page as <style>, since the frame that shows it loads no files but
    // pictures; addresses inside the styles (url(...)) are made whole, so pictures they
    // name come from the document's folder (documentAddress). Only files inside the
    // document's folder are read, up to 4 MB each. Pictures written into the page (data:),
    // which the frame does not load either, become cid-part:{n} with their data in parts,
    // as an e-mail's pictures do (SendOriginalHtml serves them).
    public static (string Html, List<(string Type, byte[] Data)> Parts) ForOriginal(string html, string folder, string documentAddress)
    {
        var document = new HtmlParser().ParseDocument(html);
        var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string? Inside(string baseFolder, string relative)
        {
            try
            {
                var path = Path.GetFullPath(Path.Combine(baseFolder, Uri.UnescapeDataString(relative.Split('#', 2)[0].Split('?', 2)[0]).Replace('/', Path.DirectorySeparatorChar)));
                return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? path : null;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
            {
                return null;
            }
        }

        string Address(string path) =>
            documentAddress + string.Join('/', Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar).Select(Uri.EscapeDataString));
        // url(x) relative to the stylesheet's folder, made whole; data: and full addresses stay.
        string Rewrite(string css, string cssFolder) => System.Text.RegularExpressions.Regex.Replace(css,
            @"url\(\s*(['""]?)([^'"")]+)\1\s*\)",
            match => Uri.TryCreate(match.Groups[2].Value, UriKind.Absolute, out _) || match.Groups[2].Value.StartsWith('#')
                ? match.Value
                : Inside(cssFolder, match.Groups[2].Value) is { } path ? $"url(\"{Address(path)}\")" : "url(\"\")");

        foreach (var link in document.QuerySelectorAll("link[rel][href]").ToList())
        {
            if (!(link.GetAttribute("rel") ?? string.Empty).Split(' ').Contains("stylesheet", StringComparer.OrdinalIgnoreCase)
                || Uri.TryCreate(link.GetAttribute("href"), UriKind.Absolute, out _)
                || Inside(root, link.GetAttribute("href")!) is not { } path
                || !File.Exists(path) || new FileInfo(path).Length > 4 * 1024 * 1024)
            {
                continue;
            }

            var style = document.CreateElement("style");
            if (link.GetAttribute("media") is { } media)
            {
                style.SetAttribute("media", media);
            }

            style.TextContent = Rewrite(File.ReadAllText(path), Path.GetDirectoryName(path)!);
            link.Replace(style);
        }

        foreach (var style in document.QuerySelectorAll("style").ToList())
        {
            if (style.TextContent.Contains("url(", StringComparison.OrdinalIgnoreCase))
            {
                style.TextContent = Rewrite(style.TextContent, root);
            }
        }

        var parts = new List<(string Type, byte[] Data)>();
        foreach (var image in document.QuerySelectorAll("img[src^='data:']").ToList())
        {
            var match = System.Text.RegularExpressions.Regex.Match(image.GetAttribute("src")!, @"^data:(image/(?:png|jpeg|gif|webp|svg\+xml));base64,([A-Za-z0-9+/=\s]+)$");
            try
            {
                if (match.Success)
                {
                    parts.Add((match.Groups[1].Value, Convert.FromBase64String(match.Groups[2].Value)));
                    image.SetAttribute("src", $"cid-part:{parts.Count - 1}");
                }
            }
            catch (FormatException)
            {
                // Left as it was; the frame shows no picture for it.
            }
        }

        return ("<!doctype html>" + document.DocumentElement.OuterHtml, parts);
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
