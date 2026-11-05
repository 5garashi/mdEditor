/*
  File: InlineSvg.cs
  Description: Which parts of an SVG drawing written in Markdown the preview keeps, and how Word gets its pictures
  Author: 5garashi.com Design Office
  Created: 2026-11-06
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Ganss.Xss;

namespace MdEditor;

// An <svg> drawing written in Markdown shows the way VS Code, Jupyter, and browsers show
// it (GitHub removes it). Only drawing is kept: shapes, text, markers, gradients, clip
// paths, and filters. Anything that runs or loads something (script, foreignObject,
// image, animation, style sheets, event attributes, links to other files) is removed,
// and url(...) may point only inside the document (url(#arrow)).
internal static partial class InlineSvg
{
    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    // Elements that exist only in SVG; outside an <svg> they are removed.
    private static readonly string[] SvgOnlyTags =
    [
        "svg", "g", "defs", "symbol", "use", "desc", "path", "rect", "circle", "ellipse", "line", "polyline",
        "polygon", "text", "tspan", "textPath", "marker", "linearGradient", "radialGradient", "stop", "clipPath",
        "mask", "pattern", "filter", "feBlend", "feColorMatrix", "feComponentTransfer", "feComposite",
        "feDropShadow", "feFlood", "feFuncA", "feFuncB", "feFuncG", "feFuncR", "feGaussianBlur", "feMerge",
        "feMergeNode", "feMorphology", "feOffset"
    ];

    // <title> names the drawing for screen readers; in HTML it would rename the page, so it
    // is kept only inside an <svg>.
    public static IEnumerable<string> Tags => SvgOnlyTags.Append("title");

    public static readonly string[] Attributes =
    [
        "viewBox", "preserveAspectRatio", "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry", "fx", "fy", "fr",
        "d", "points", "pathLength", "transform", "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width",
        "stroke-opacity", "stroke-dasharray", "stroke-dashoffset", "stroke-linecap", "stroke-linejoin",
        "stroke-miterlimit", "opacity", "visibility", "display", "overflow", "vector-effect", "paint-order",
        "shape-rendering", "text-rendering", "font-size", "font-family", "font-weight", "font-style", "font-variant",
        "text-anchor", "dominant-baseline", "alignment-baseline", "baseline-shift", "letter-spacing", "word-spacing",
        "text-decoration", "writing-mode", "dx", "dy", "rotate", "textLength", "lengthAdjust", "startOffset", "side",
        "marker-start", "marker-mid", "marker-end", "markerWidth", "markerHeight", "markerUnits", "refX", "refY",
        "orient", "href", "xlink:href", "gradientUnits", "gradientTransform", "spreadMethod", "offset", "stop-color",
        "stop-opacity", "clip-path", "clip-rule", "clipPathUnits", "mask", "maskUnits", "maskContentUnits",
        "patternUnits", "patternContentUnits", "patternTransform", "filter", "filterUnits", "primitiveUnits", "in",
        "in2", "result", "stdDeviation", "mode", "operator", "k1", "k2", "k3", "k4", "values", "tableValues", "slope",
        "intercept", "amplitude", "exponent", "radius", "flood-color", "flood-opacity", "color-interpolation-filters",
        "xml:space", "role", "aria-label", "aria-labelledby", "aria-describedby", "aria-hidden", "focusable"
    ];

    // style="..." on a drawing keeps its size, place, and colors (as in VS Code's preview);
    // HTML elsewhere keeps only text-align.
    private static readonly HashSet<string> CssProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "display", "width", "height", "max-width", "max-height", "min-width", "min-height", "margin", "margin-top",
        "margin-right", "margin-bottom", "margin-left", "vertical-align", "color", "fill", "fill-opacity", "stroke",
        "stroke-width", "stroke-opacity", "stroke-dasharray", "stroke-linecap", "stroke-linejoin", "opacity",
        "font-size", "font-family", "font-weight", "font-style", "text-anchor", "dominant-baseline", "baseline-shift"
    };

    // Marks the outermost <svg> of each drawing, so the page can take its picture for Word.
    public const string PictureAttribute = "data-inline-svg";

    public static void Configure(HtmlSanitizer sanitizer)
    {
        sanitizer.AllowedTags.UnionWith(Tags);
        sanitizer.AllowedAttributes.UnionWith(Attributes);
        sanitizer.RemovingStyle += (_, e) =>
        {
            if (IsInSvg(e.Tag) && CssProperties.Contains(e.Style.Name) && !HasOutsideUrl(e.Style.Value))
            {
                e.Cancel = true;
            }
        };
    }

    // Called for each element after the sanitizer has kept it.
    public static void PostProcess(IElement element)
    {
        if (!IsInSvg(element))
        {
            if (Array.Exists(SvgOnlyTags, tag => tag.Equals(element.LocalName, StringComparison.OrdinalIgnoreCase))
                || element.LocalName.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                element.Remove();
            }

            return;
        }

        foreach (var attribute in element.Attributes.ToList())
        {
            var isLink = attribute.LocalName == "href";
            if ((isLink && element.LocalName != "a" && !attribute.Value.TrimStart().StartsWith('#'))
                || HasOutsideUrl(attribute.Value))
            {
                element.RemoveAttribute(attribute.NamespaceUri, attribute.LocalName);
            }
        }

        if (element.LocalName == "svg" && element.ParentElement?.Closest("svg") is null)
        {
            element.SetAttribute(PictureAttribute, string.Empty);
        }
    }

    private static bool IsInSvg(IElement? element) => element?.NamespaceUri == SvgNamespace;

    // url(...) that does not point inside the document (url(#arrow) is fine).
    private static bool HasOutsideUrl(string value) =>
        UrlReference().Matches(value).Any(match => !match.Groups[1].Value.StartsWith('#'));

    [GeneratedRegex(@"url\(\s*[""']?([^)""']*)", RegexOptions.IgnoreCase)]
    private static partial Regex UrlReference();

    // --- Word ---------------------------------------------------------------------

    // The outermost <svg> ... </svg> drawings in HTML written in Markdown, in order.
    public static IEnumerable<(int Start, int End)> Drawings(string html)
    {
        var depth = 0;
        var start = 0;
        foreach (Match tag in SvgTag().Matches(html))
        {
            if (tag.Groups["close"].Success)
            {
                if (depth > 0 && --depth == 0)
                {
                    yield return (start, tag.Index + tag.Length);
                }
            }
            else if (!tag.Groups["empty"].Success)
            {
                if (depth++ == 0)
                {
                    start = tag.Index;
                }
            }
            else if (depth == 0)
            {
                yield return (tag.Index, tag.Index + tag.Length);
            }
        }
    }

    // <svg ...>, </svg>, or <svg ... />, as a step of a drawing written over several inline tags.
    public static int Step(string tag) => SvgTag().Match(tag) is { Success: true, Index: 0 } match
        ? match.Groups["close"].Success ? -1 : match.Groups["empty"].Success ? 0 : 1
        : 0;

    public static bool StartsDrawing(string tag) => SvgTag().Match(tag) is { Success: true, Index: 0 } match && !match.Groups["close"].Success;

    [GeneratedRegex(@"<(?<close>/)?svg\b(?:[^>""']|""[^""]*""|'[^']*')*?(?<empty>/)?>", RegexOptions.IgnoreCase)]
    private static partial Regex SvgTag();
}
