/*
  File: HtmlTextMath.cs
  Description: Finds math written as text inside HTML in Markdown ($x$, \(x\), $$x$$, \[x\]), as MathJax pages draw it
  Author: 5garashi.com Design Office
  Created: 2026-11-06
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text;
using System.Text.RegularExpressions;

namespace MdEditor;

// Markdown is not read inside an HTML block (<figure>, <div>, <table> written as HTML), but
// a page that draws math with MathJax finds math in all of its text, a <figcaption> too. The
// preview does the same: math in the text of an HTML block is marked as math, with the same
// rules as math in Markdown ($ followed by a space or a closing $ before a digit is not math,
// so prices stay text). Code, preformatted text, scripts, styles, form fields, SVG, and
// MathML are left as they are, as MathJax leaves them.
internal static partial class HtmlTextMath
{
    internal sealed record Piece(int Start, int End, string Tex, bool Display);

    // Marks each piece of math, keyed by the block's line and its place in the block, so
    // Word finds each one's picture even if the sanitizer later removes another.
    public const string KeyAttribute = "data-html-math";

    private static readonly HashSet<string> SkippedElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "code", "pre", "kbd", "samp", "script", "noscript", "style", "textarea", "svg", "math"
    };

    public static List<Piece> Find(string html)
    {
        var pieces = new List<Piece>();
        var skipped = new Stack<string>();
        var textStart = 0;
        foreach (Match token in Token().Matches(html))
        {
            if (skipped.Count == 0)
            {
                FindInText(html, textStart, token.Index, pieces);
            }

            textStart = token.Index + token.Length;
            if (!token.Groups["name"].Success)
            {
                continue; // a comment
            }

            var name = token.Groups["name"].Value;
            if (token.Groups["close"].Success)
            {
                if (skipped.Count > 0 && skipped.Peek().Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    skipped.Pop();
                }
            }
            else if (SkippedElements.Contains(name) && !token.Groups["empty"].Success)
            {
                skipped.Push(name);
            }
        }

        if (skipped.Count == 0)
        {
            FindInText(html, textStart, html.Length, pieces);
        }

        return pieces;
    }

    private static void FindInText(string html, int start, int end, List<Piece> pieces)
    {
        if (end <= start || html.IndexOfAny(['$', '\\'], start, end - start) < 0)
        {
            return;
        }

        foreach (Match match in Delimited().Matches(html[..end], start))
        {
            var group = match.Groups["dd"].Success ? match.Groups["dd"]
                : match.Groups["bd"].Success ? match.Groups["bd"]
                : match.Groups["bi"].Success ? match.Groups["bi"]
                : match.Groups["di"];
            // A blank line ends the text math may run over, as in Markdown.
            if (group.Value.Trim().Length == 0 || BlankLine().IsMatch(group.Value))
            {
                continue;
            }

            pieces.Add(new Piece(match.Index, match.Index + match.Length, group.Value.Trim(),
                match.Groups["dd"].Success || match.Groups["bd"].Success));
        }
    }

    // The HTML block with each piece of math in a span the page draws with KaTeX.
    public static string Mark(string html, int blockLine)
    {
        var pieces = Find(html);
        if (pieces.Count == 0)
        {
            return html;
        }

        var result = new StringBuilder(html.Length + pieces.Count * 64);
        var at = 0;
        for (var i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            result.Append(html, at, piece.Start - at)
                .Append($"<span class=\"math{(piece.Display ? " math-display" : string.Empty)}\" {KeyAttribute}=\"{Key(blockLine, i)}\">")
                .Append(piece.Display ? "\\[" : "\\(").Append(piece.Tex).Append(piece.Display ? "\\]" : "\\)")
                .Append("</span>");
            at = piece.End;
        }

        return result.Append(html, at, html.Length - at).ToString();
    }

    public static string Key(int blockLine, int index) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{blockLine}:{index}");

    // For Word: each piece of math becomes a marker (U+E000, its index, U+E001) that survives
    // removing the tags, to be replaced by the picture of that piece.
    public static string MarkForWord(string html)
    {
        var pieces = Find(html);
        var result = new StringBuilder(html.Length);
        var at = 0;
        for (var i = 0; i < pieces.Count; i++)
        {
            result.Append(html, at, pieces[i].Start - at).Append('').Append(i).Append('');
            at = pieces[i].End;
        }

        return result.Append(html, at, html.Length - at).ToString();
    }

    [GeneratedRegex(@"<!--[\s\S]*?-->|<(?<close>/)?(?<name>[A-Za-z][\w:-]*)(?:[^>""']|""[^""]*""|'[^']*')*?(?<empty>/)?>")]
    private static partial Regex Token();

    // $$…$$, \[…\], \(…\), and $…$ (not after a backslash, not starting or ending with a space,
    // not followed by a digit), in the order they come.
    [GeneratedRegex(@"(?<!\\)\$\$(?<dd>[\s\S]+?)\$\$|\\\[(?<bd>[\s\S]+?)\\\]|\\\((?<bi>[\s\S]+?)\\\)|(?<!\\)\$(?![\s$])(?<di>(?:\\[\s\S]|[^$\\])+?)(?<![\s\\])\$(?!\d)")]
    private static partial Regex Delimited();

    [GeneratedRegex(@"\n[ \t]*\r?\n")]
    private static partial Regex BlankLine();
}
