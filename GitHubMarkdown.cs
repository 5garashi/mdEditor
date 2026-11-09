/*
  File: GitHubMarkdown.cs
  Description: GitHub conversion (rewriting what GitHub does not show as written) and the GitHub view (the text sent to GitHub's Markdown API and the HTML it returns, made ready for the preview)
  Author: 5garashi.com Design Office
  Created: 2026-11-08
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdEditor;

// One place to rewrite: what kind it is, the range it covers, and its edits (on the original
// text, from/to exclusive). A block rewrite (alert, definition list, lettered list) builds its
// text from the original with the other rewrites inside it applied.
internal sealed record GitHubEdit(int From, int To, string Insert);

internal sealed class GitHubChange(string kind, int from, int to)
{
    public string Kind { get; } = kind;
    public int From { get; } = from;
    public int To { get; } = to;
    public List<GitHubEdit> Edits { get; } = [];
    public Func<Func<int, int, string>, string>? Build { get; init; }
}

internal sealed record GitHubKindCount(string Kind, int Count, string Example);

internal sealed record GitHubPlan(List<GitHubKindCount> Kinds, List<GitHubKindCount> Skipped, List<GitHubEdit> Edits);

// GitHub conversion. The text is read as mdEditor reads it (its own syntax, \(x\) math), and
// each way of writing GitHub shows differently is rewritten to the way GitHub shows it, as
// checked with GitHub's Markdown API on 2026-11-08. Nothing else in the text changes. A way of
// writing GitHub has nothing for (abbreviations, grid tables, other ::: boxes) stays as it is
// and is only counted.
internal static class GitHubConversion
{
    // Every kind, in the order the dialog lists them.
    public static readonly string[] AllKinds =
    [
        "latexInline", "latexDisplay", "spaceMath", "riskyMath", "mathAfterText", "multilineMath", "bareEnvironment", "riskyMathBlock",
        "markdownLineMath", "sup", "sub", "ins", "mark", "lineBreak", "alert", "definitionList", "letterList", "headingId"
    ];

    private static readonly MarkdownPipeline Pipeline =
        MarkdownPipelines.Build(nearGitHub: false, lineBreaks: false, latexMath: true, preciseSource: true);

    private static readonly Dictionary<string, string> AlertTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["note"] = "NOTE", ["tip"] = "TIP", ["important"] = "IMPORTANT", ["warning"] = "WARNING", ["caution"] = "CAUTION"
    };

    // A backslash before ASCII punctuation: GitHub reads it as Markdown's escape even inside
    // $…$ and $$…$$ (\{ shows as {, and \\ loses a backslash).
    private static readonly Regex Escape = new(@"\\[!-/:-@\[-`{-~]", RegexOptions.CultureInvariant);
    // _ that can open emphasis (not inside a word), as in $(a)_n$ … $f_{ij}$ on GitHub.
    private static readonly Regex OpeningUnderscore = new(@"(?<![\p{L}\p{N}])_", RegexOptions.CultureInvariant);
    private static readonly Regex Entity = new(@"&(?:#[0-9]+|#x[0-9A-Fa-f]+|[A-Za-z][A-Za-z0-9]*);", RegexOptions.CultureInvariant);
    private static readonly Regex HeadingAttributes = new(@"[ \t]*\{(?<attrs>[^{}\n]*)\}[ \t]*$", RegexOptions.CultureInvariant);
    private static readonly Regex ListMarker = new(@"(?:[-*+]|[0-9]{1,9}[.)]|[A-Za-z][.)])(?=[ \t])", RegexOptions.CultureInvariant);
    private static readonly Regex MathInHtml = new(@"(?<![\\$\w])\$(?![\s$])[^$\n<>]+?(?<!\s)\$(?![\w$])|\\\([^\n]+?\\\)|\$\$[^$]+?\$\$", RegexOptions.CultureInvariant);
    private static readonly Regex MarkdownCharacters =new(@"[*_`\[\]<>&\\$~^=+!|]", RegexOptions.CultureInvariant);

    // What GitHub would read as Markdown in inline math written $…$.
    public static bool RiskyInline(string math) =>
        Escape.IsMatch(math) || math.Contains('*') || math.Contains('~') || OpeningUnderscore.IsMatch(math)
        || math.Contains("](", StringComparison.Ordinal) || math.Contains("][", StringComparison.Ordinal) || Entity.IsMatch(math);

    // What GitHub would read as Markdown in display math written $$…$$ (a ```math block keeps
    // all of it).
    public static bool RiskyBlock(string math) => RiskyInline(math);

    // A line of $$ … $$ on lines of its own that GitHub reads as Markdown before it looks for
    // math, so the block stops being math there (checked with GitHub's Markdown API on
    // 2026-11-09): = or - alone makes the lines above a heading, and so does # x; - x, + x,
    // * x, and 1. x start a list; ***, ---, and ___ a rule; > a quote; ``` and ~~~ code; <div>
    // and <!-- HTML; :- and |-| a table; and a blank line ends the paragraph. A line indented
    // four spaces or more is not read so, nor are + or * alone, -x, 2. x, #x, <span>, or < b.
    private static readonly Regex MarkdownLine = new(
        @"^ {0,3}(?:=+|(?:[-*_][ \t]*){3,}|[-+*][ \t]+\S|0*1[.)][ \t]+\S|#{1,6}(?:[ \t]|$)|>|`{3,}|~{3,}"
        + @"|\|?[ \t]*:?-+:?[ \t]*(?:\|[ \t]*:?-+:?[ \t]*)*\|?[ \t]*$"
        + @"|<(?:script|pre|style|textarea)(?:[\s>]|$)|<!--|<\?|<![A-Za-z]|<!\[CDATA\["
        + @"|</?(?:address|article|aside|base|basefont|blockquote|body|caption|center|col|colgroup|dd|details|dialog|dir|div|dl|dt"
        + @"|fieldset|figcaption|figure|footer|form|frame|frameset|h[1-6]|head|header|hr|html|iframe|legend|li|link|main|menu"
        + @"|menuitem|nav|noframes|ol|optgroup|option|p|param|search|section|summary|table|tbody|td|tfoot|th|thead|title|tr"
        + @"|track|ul)(?:[\s/>]|$))",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool ReadAsMarkdown(IEnumerable<string> lines) =>
        lines.Select(line => line.TrimEnd('\r')).Any(line => string.IsNullOrWhiteSpace(line) || MarkdownLine.IsMatch(line));

    public static GitHubPlan Plan(string text, int selectionFrom, int selectionTo, bool lineBreaks, IReadOnlySet<string>? kinds)
    {
        var document = Markdown.Parse(text, Pipeline);
        var changes = new List<GitHubChange>();
        var skipped = new Dictionary<string, (int Count, string Example)>();
        void Skip(string reason, int from, int to)
        {
            skipped[reason] = skipped.TryGetValue(reason, out var seen) ? (seen.Count + 1, seen.Example) : (1, Example(text, from, to));
        }

        var context = new Context(text, changes, Skip);
        context.CollectMath(document);
        context.CollectEmphasis(document);
        if (lineBreaks)
        {
            context.CollectLineBreaks(document);
        }

        context.CollectBlocks(document);

        var whole = selectionFrom >= selectionTo;
        var from = whole ? 0 : selectionFrom;
        var to = whole ? text.Length : selectionTo;
        var active = new List<GitHubChange>();
        foreach (var change in changes.Where(change => kinds is null || kinds.Contains(change.Kind)))
        {
            if (change.From >= from && change.To <= to)
            {
                active.Add(change);
            }
            else if (change.From < to && change.To > from)
            {
                Skip("boundary", change.From, change.To);
            }
        }

        // Block rewrites take the rewrites inside them; the rest are edits of their own.
        var consumed = new HashSet<GitHubChange>();
        var edits = new List<GitHubEdit>();
        foreach (var block in active.Where(change => change.Build is not null).OrderBy(change => change.From))
        {
            if (consumed.Contains(block))
            {
                continue;
            }

            var inner = active.Where(change => change != block && change.From >= block.From && change.To <= block.To).ToList();
            consumed.UnionWith(inner);
            string Slice(int sliceFrom, int sliceTo) => Apply(text, sliceFrom, sliceTo, inner.Where(change => change.From >= sliceFrom && change.To <= sliceTo));
            edits.Add(new GitHubEdit(block.From, block.To, block.Build!(Slice)));
        }

        foreach (var change in active.Where(change => change.Build is null && !consumed.Contains(change)))
        {
            edits.AddRange(change.Edits);
        }

        edits.Sort((a, b) => a.From != b.From ? a.From.CompareTo(b.From) : a.To.CompareTo(b.To));
        // A safety net: rewrites never overlap; if two ever did, the later one is left out.
        var kept = new List<GitHubEdit>();
        var end = -1;
        foreach (var edit in edits)
        {
            if (edit.From < end)
            {
                continue;
            }

            kept.Add(edit);
            end = Math.Max(end, edit.To);
        }

        var counts = active.GroupBy(change => change.Kind)
            .OrderBy(group => Array.IndexOf(AllKinds, group.Key))
            .Select(group => new GitHubKindCount(group.Key, group.Count(), Example(text, group.First().From, group.First().To)))
            .ToList();
        var skips = skipped.Select(pair => new GitHubKindCount(pair.Key, pair.Value.Count, pair.Value.Example)).ToList();
        return new GitHubPlan(counts, skips, kept);
    }

    // The text from..to with the given rewrites applied.
    private static string Apply(string text, int from, int to, IEnumerable<GitHubChange> changes)
    {
        var builder = new StringBuilder();
        var position = from;
        foreach (var edit in changes.SelectMany(change => change.Edits).OrderBy(edit => edit.From).ThenBy(edit => edit.To))
        {
            if (edit.From < position)
            {
                continue;
            }

            builder.Append(text, position, edit.From - position).Append(edit.Insert);
            position = edit.To;
        }

        builder.Append(text, position, to - position);
        return builder.ToString();
    }

    private static string Example(string text, int from, int to)
    {
        var length = Math.Min(Math.Max(0, to - from), 60);
        var example = text.Substring(from, length).Replace("\r", string.Empty).Replace("\n", " ↵ ");
        return to - from > length ? example + "…" : example;
    }

    private sealed class Context(string text, List<GitHubChange> changes, Action<string, int, int> skip)
    {
        private int LineStart(int position)
        {
            var newline = position > 0 ? text.LastIndexOf('\n', position - 1) : -1;
            return newline + 1;
        }

        // What stands before the block on its first line ("> ", "- ", spaces), and the same for
        // the lines after it (a list marker becomes spaces, a quote mark stays).
        private string ContinuationPrefix(int blockStart)
        {
            var prefix = text[LineStart(blockStart)..blockStart];
            return ListMarker.Replace(prefix, match => new string(' ', match.Length));
        }

        // Where the block's content ends: a list's own range runs on over the blank line after
        // it, which has to stay. A ::: box ends with its closing line.
        private static int EndOf(MarkdownObject node) =>
            node is ContainerBlock { Count: > 0 } container and not CustomContainer ? container.Max(EndOf) : node.Span.End;

        // The end of a block to rewrite (exclusive), without the line break after it.
        private int BlockEnd(MarkdownObject node)
        {
            var to = Math.Min(text.Length, EndOf(node) + 1);
            while (to > node.Span.Start + 1 && text[to - 1] is '\n' or '\r')
            {
                to--;
            }

            return to;
        }

        private void Add(string kind, int from, int to, params GitHubEdit[] edits)
        {
            var change = new GitHubChange(kind, from, to);
            change.Edits.AddRange(edits);
            changes.Add(change);
        }

        // A ```math block in place of from..to (exclusive), with the lines of the math.
        private string MathFence(int blockStart, IEnumerable<string> lines)
        {
            var prefix = ContinuationPrefix(blockStart);
            var body = lines.Select(line => line.TrimEnd('\r')).ToList();
            while (body.Count > 0 && string.IsNullOrWhiteSpace(body[0])) body.RemoveAt(0);
            while (body.Count > 0 && string.IsNullOrWhiteSpace(body[^1])) body.RemoveAt(body.Count - 1);
            var builder = new StringBuilder("```math");
            foreach (var line in body)
            {
                builder.Append('\n').Append(prefix).Append(line);
            }

            return builder.Append('\n').Append(prefix).Append("```").ToString();
        }

        public void CollectMath(MarkdownDocument document)
        {
            foreach (var math in document.Descendants<MathInline>().ToList())
            {
                var start = math.Span.Start;
                var end = math.Span.End + 1;
                if (start < 0 || end > text.Length || end <= start)
                {
                    continue;
                }

                var source = text[start..end];
                var latex = source.StartsWith('\\');
                var content = math.Content.ToString();
                if (source.StartsWith("$`", StringComparison.Ordinal))
                {
                    continue;
                }

                if (math.DelimiterCount >= 2)
                {
                    var multiline = !latex && content.Contains('\n');
                    var paragraph = math.Parent?.ParentBlock as ParagraphBlock;
                    if (paragraph is not null && StandsAlone(paragraph, math))
                    {
                        var kind = latex ? "latexDisplay" : multiline ? "multilineMath" : RiskyBlock(content) ? "riskyMath" : null;
                        if (kind is not null)
                        {
                            var from = paragraph.Span.Start;
                            var to = paragraph.Span.End + 1;
                            Add(kind, from, to, new GitHubEdit(from, to, MathFence(from, content.Split('\n'))));
                        }

                        continue;
                    }

                    var oneLine = Regex.Replace(content.Trim(), @"\s*\n\s*", " ");
                    if (latex || multiline)
                    {
                        if (RiskyBlock(oneLine))
                        {
                            skip("inlineDisplay", start, end);
                        }
                        else
                        {
                            Add(latex ? "latexDisplay" : "multilineMath", start, end, new GitHubEdit(start, end, "$$" + oneLine + "$$"));
                        }
                    }
                    else if (RiskyBlock(oneLine))
                    {
                        skip("inlineDisplay", start, end);
                    }

                    continue;
                }

                var trimmed = Regex.Replace(content, @"\s*\n\s*", " ").Trim();
                // GitHub reads $…$ as math only after a space or ASCII punctuation: right after
                // 、 。 （ or a kana it stays text (checked 2026-11-08). $`…`$ is read anywhere.
                var before = start > 0 ? text[start - 1] : '\n';
                var afterText = before > 0x7F && !char.IsWhiteSpace(before);
                var risky = RiskyInline(trimmed) || afterText;
                var inlineKind = latex ? "latexInline" : content != content.Trim() ? "spaceMath" : RiskyInline(trimmed) ? "riskyMath" : afterText ? "mathAfterText" : null;
                if (inlineKind is null)
                {
                    continue;
                }

                if (risky && trimmed.Contains('`'))
                {
                    skip("mathBacktick", start, end);
                    continue;
                }

                Add(inlineKind, start, end, new GitHubEdit(start, end, risky ? "$`" + trimmed + "`$" : "$" + trimmed + "$"));
            }

            foreach (var block in document.Descendants<MathBlock>().ToList())
            {
                var bare = block.GetData(typeof(LatexEnvironmentBlockParser)) is not null;
                var lines = block.Lines.Lines.Take(block.Lines.Count).Select(line => line.Slice.ToString()).ToList();
                var kind = bare ? "bareEnvironment" : RiskyBlock(string.Join("\n", lines)) ? "riskyMathBlock"
                    : ReadAsMarkdown(lines) ? "markdownLineMath" : null;
                if (kind is null)
                {
                    continue;
                }

                var from = block.Span.Start;
                var to = Math.Min(text.Length, block.Span.End + 1);
                Add(kind, from, to, new GitHubEdit(from, to, MathFence(from, lines)));
            }
        }

        // The paragraph holds nothing but this math (and line breaks or spaces around it).
        private static bool StandsAlone(ParagraphBlock paragraph, MathInline math)
        {
            if (paragraph.Inline is null)
            {
                return false;
            }

            foreach (var inline in paragraph.Inline)
            {
                if (inline == math || inline is LineBreakInline || inline is LiteralInline literal && literal.Content.IsEmptyOrWhitespace())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public void CollectEmphasis(MarkdownDocument document)
        {
            foreach (var emphasis in document.Descendants<EmphasisInline>().ToList())
            {
                var (kind, tag) = (emphasis.DelimiterChar, emphasis.DelimiterCount) switch
                {
                    ('^', 1) => ("sup", "sup"),
                    ('~', 1) => ("sub", "sub"),
                    ('+', 2) => ("ins", "ins"),
                    ('=', 2) => ("mark", "mark"),
                    ('"', 2) => ("citation", null),
                    _ => (null, null)
                };
                if (kind is null)
                {
                    continue;
                }

                var count = emphasis.DelimiterCount;
                var start = emphasis.Span.Start;
                var end = emphasis.Span.End + 1;
                var mark = new string(emphasis.DelimiterChar, count);
                if (start < 0 || end > text.Length || end - start < 2 * count
                    || text.Substring(start, count) != mark || text.Substring(end - count, count) != mark)
                {
                    continue;
                }

                if (tag is null)
                {
                    skip(kind, start, end);
                    continue;
                }

                Add(kind, start, end, new GitHubEdit(start, start + count, $"<{tag}>"), new GitHubEdit(end - count, end, $"</{tag}>"));
            }
        }

        public void CollectLineBreaks(MarkdownDocument document)
        {
            foreach (var paragraph in document.Descendants<ParagraphBlock>().Where(paragraph => paragraph.Inline is not null))
            {
                foreach (var lineBreak in paragraph.Inline!.Descendants<LineBreakInline>().Where(lineBreak => !lineBreak.IsHard))
                {
                    var at = lineBreak.Span.Start;
                    if (at <= 0 || at >= text.Length || text[at] != '\n')
                    {
                        continue;
                    }

                    Add("lineBreak", at, at, new GitHubEdit(at, at, "\\"));
                }
            }
        }

        public void CollectBlocks(MarkdownDocument document)
        {
            foreach (var container in document.Descendants<CustomContainer>().ToList())
            {
                CollectContainer(container);
            }

            foreach (var list in document.Descendants<DefinitionList>().ToList())
            {
                CollectDefinitionList(list);
            }

            foreach (var list in document.Descendants<ListBlock>().Where(list => list.IsOrdered && list.BulletType is 'a' or 'A' or 'i' or 'I').ToList())
            {
                CollectLetterList(list);
            }

            foreach (var heading in document.Descendants<HeadingBlock>().ToList())
            {
                CollectHeadingId(heading);
            }

            if (document.GetAbbreviations() is { Count: > 0 } abbreviations)
            {
                foreach (var abbreviation in abbreviations.Values)
                {
                    skip("abbreviation", abbreviation.Span.Start, abbreviation.Span.End + 1);
                }
            }

            foreach (var table in document.Descendants<Table>().Where(table => table.Span.Start >= 0 && table.Span.Start < text.Length && text[table.Span.Start] == '+'))
            {
                skip("gridTable", table.Span.Start, EndOf(table) + 1);
            }

            // Math written as text inside HTML (a <figcaption>): mdEditor draws it, GitHub does
            // not, and no Markdown around it changes that.
            foreach (var html in document.Descendants<HtmlBlock>())
            {
                var from = html.Span.Start;
                var to = Math.Min(text.Length, html.Span.End + 1);
                if (from < 0 || to <= from)
                {
                    continue;
                }

                foreach (Match math in MathInHtml.Matches(text[from..to]))
                {
                    skip("mathInHtml", from + math.Index, from + math.Index + math.Length);
                }
            }

            foreach (var figure in document.Descendants<Figure>())
            {
                skip("figure", figure.Span.Start, EndOf(figure) + 1);
            }

            foreach (var footer in document.Descendants<FooterBlock>())
            {
                skip("footer", footer.Span.Start, EndOf(footer) + 1);
            }
        }

        private void CollectContainer(CustomContainer container)
        {
            var from = container.Span.Start;
            var to = BlockEnd(container);
            if (container.Info is not { } info || !AlertTypes.TryGetValue(info, out var type))
            {
                skip("container", from, to);
                return;
            }

            if (container.Parent is not MarkdownDocument)
            {
                skip("nested", from, to);
                return;
            }

            var firstLineEnd = text.IndexOf('\n', from);
            if (firstLineEnd < 0 || firstLineEnd >= to)
            {
                return;
            }

            var contentStart = firstLineEnd + 1;
            var lastLineStart = LineStart(to - 1);
            var contentEnd = lastLineStart > contentStart && text[lastLineStart..to].TrimStart().StartsWith(":::", StringComparison.Ordinal)
                ? lastLineStart
                : to;
            var title = container.Arguments?.Trim();
            changes.Add(new GitHubChange("alert", from, to)
            {
                Build = slice =>
                {
                    var lines = new List<string> { $"> [!{type}]" };
                    if (!string.IsNullOrEmpty(title))
                    {
                        lines.Add($"> **{title}**");
                        lines.Add(">");
                    }

                    var content = slice(contentStart, contentEnd).TrimEnd('\n');
                    if (content.Length > 0)
                    {
                        lines.AddRange(content.Split('\n').Select(line => line.Length == 0 ? ">" : "> " + line));
                    }

                    return string.Join("\n", lines);
                }
            });
        }

        // Removes the indentation that put the lines after the first inside a list item or a
        // definition (up to that many columns), so they are not read as code inside HTML.
        private static string Dedent(string content, int columns)
        {
            var lines = content.Split('\n');
            for (var index = 1; index < lines.Length; index++)
            {
                var remove = 0;
                while (remove < lines[index].Length && remove < columns && lines[index][remove] == ' ')
                {
                    remove++;
                }

                lines[index] = lines[index][remove..];
            }

            return string.Join("\n", lines);
        }

        private static bool Plain(string content) => !content.Contains('\n') && !MarkdownCharacters.IsMatch(content);

        private static IEnumerable<string> Element(string tag, string content, string attributes = "")
        {
            if (Plain(content))
            {
                return [$"<{tag}{attributes}>{content}</{tag}>"];
            }

            // Blank lines around it let GitHub read the Markdown inside the HTML.
            return [$"<{tag}{attributes}>", string.Empty, content, string.Empty, $"</{tag}>"];
        }

        private void CollectDefinitionList(DefinitionList list)
        {
            var from = list.Span.Start;
            var to = BlockEnd(list);
            if (list.Parent is not MarkdownDocument)
            {
                skip("nested", from, to);
                return;
            }

            changes.Add(new GitHubChange("definitionList", from, to)
            {
                Build = slice =>
                {
                    var lines = new List<string> { "<dl>" };
                    foreach (var item in list.OfType<DefinitionItem>())
                    {
                        var blocks = new List<Block>();
                        foreach (var child in item)
                        {
                            if (child is DefinitionTerm term)
                            {
                                lines.AddRange(Element("dt", slice(term.Span.Start, term.Span.End + 1).Trim()));
                            }
                            else
                            {
                                blocks.Add(child);
                            }
                        }

                        if (blocks.Count > 0)
                        {
                            var content = Dedent(slice(blocks[0].Span.Start, EndOf(blocks[^1]) + 1), blocks[0].Column).Trim('\n');
                            lines.AddRange(["<dd>", string.Empty, content, string.Empty, "</dd>"]);
                        }
                    }

                    lines.Add("</dl>");
                    return string.Join("\n", lines);
                }
            });
        }

        private void CollectLetterList(ListBlock list)
        {
            var from = list.Span.Start;
            var to = BlockEnd(list);
            if (list.Parent is not MarkdownDocument)
            {
                skip("nested", from, to);
                return;
            }

            var type = list.BulletType.ToString();
            var start = StartNumber(list.OrderedStart, list.BulletType);
            var attributes = $" type=\"{type}\"" + (start > 1 ? $" start=\"{start}\"" : string.Empty);
            changes.Add(new GitHubChange("letterList", from, to)
            {
                Build = slice =>
                {
                    var lines = new List<string> { $"<ol{attributes}>" };
                    foreach (var item in list.OfType<ListItemBlock>())
                    {
                        if (item.Count == 0)
                        {
                            lines.Add("<li></li>");
                            continue;
                        }

                        var content = Dedent(slice(item[0].Span.Start, EndOf(item[^1]) + 1), item[0].Column).Trim('\n');
                        lines.AddRange(Element("li", content));
                    }

                    lines.Add("</ol>");
                    return string.Join("\n", lines);
                }
            });
        }

        private static int StartNumber(string? start, char type)
        {
            if (string.IsNullOrEmpty(start))
            {
                return 1;
            }

            if (type is 'a' or 'A')
            {
                return start.Length == 1 && char.IsAsciiLetter(start[0]) ? char.ToLowerInvariant(start[0]) - 'a' + 1 : 1;
            }

            var values = new Dictionary<char, int> { ['i'] = 1, ['v'] = 5, ['x'] = 10, ['l'] = 50, ['c'] = 100, ['d'] = 500, ['m'] = 1000 };
            var total = 0;
            var lower = start.ToLowerInvariant();
            for (var index = 0; index < lower.Length; index++)
            {
                if (!values.TryGetValue(lower[index], out var value))
                {
                    return 1;
                }

                total += index + 1 < lower.Length && values.TryGetValue(lower[index + 1], out var next) && next > value ? -value : value;
            }

            return Math.Max(1, total);
        }

        private void CollectHeadingId(HeadingBlock heading)
        {
            var from = heading.Span.Start;
            if (from < 0 || from >= text.Length)
            {
                return;
            }

            var lineEnd = text.IndexOf('\n', from);
            if (lineEnd < 0) lineEnd = text.Length;
            var line = text[from..lineEnd];
            if (HeadingAttributes.Match(line) is not { Success: true } match)
            {
                return;
            }

            // Markdig took it as attributes only if it is not left in the heading's text.
            var shown = string.Concat(heading.Inline?.Descendants<LiteralInline>().Select(literal => literal.Content.ToString()) ?? []);
            if (shown.Contains('{') && shown.Contains(match.Groups["attrs"].Value, StringComparison.Ordinal))
            {
                return;
            }

            var tokens = match.Groups["attrs"].Value.Split((char[])[' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            var to = from + match.Index + match.Length;
            if (tokens.Length != 1 || !Regex.IsMatch(tokens[0], @"^#[\p{L}\p{N}_:.\-]+$"))
            {
                skip("headingAttributes", from, lineEnd);
                return;
            }

            if (heading.Parent is not MarkdownDocument)
            {
                skip("nested", from, lineEnd);
                return;
            }

            var id = WebUtility.HtmlEncode(tokens[0][1..]);
            var lineStart = LineStart(from);
            Add("headingId", lineStart, to,
                new GitHubEdit(lineStart, lineStart, $"<a id=\"{id}\"></a>\n\n"),
                new GitHubEdit(from + match.Index, to, string.Empty));
        }
    }
}

// The GitHub view: the document as GitHub's Markdown API makes it, shown in the preview.
internal static class GitHubView
{
    private static readonly MarkdownPipeline Pipeline =
        MarkdownPipelines.Build(nearGitHub: true, lineBreaks: false, latexMath: false, preciseSource: true);

    private static readonly Regex JoinableBreak = new(@"^\n[ \t>]*$", RegexOptions.CultureInvariant);

    // The API reads Markdown as GitHub comments do, with each line break in a paragraph shown
    // as a line break; files such as README join them with a space. They are joined here, so
    // the result is what a file on GitHub shows. The code blocks are returned with their
    // language, in order, to tell which diagrams GitHub draws (the API leaves only code), and
    // the top-level blocks with their lines, to keep the editor and the view scrolled together.
    public static (string Text, List<string> Fences, List<(string Tag, int Line)> Blocks) Prepare(string text)
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var document = Markdown.Parse(text, Pipeline);
        var edits = new List<(int From, int To)>();
        foreach (var paragraph in document.Descendants<ParagraphBlock>().Where(paragraph => paragraph.Inline is not null))
        {
            foreach (var lineBreak in paragraph.Inline!.Descendants<LineBreakInline>().Where(lineBreak => !lineBreak.IsHard))
            {
                if (lineBreak.NextSibling is not { } next)
                {
                    continue;
                }

                var from = lineBreak.Span.Start;
                var to = next.Span.Start;
                if (from >= 0 && to > from && to <= text.Length && JoinableBreak.IsMatch(text[from..to]))
                {
                    edits.Add((from, to));
                }
            }
        }

        var builder = new StringBuilder(text);
        foreach (var (from, to) in edits.OrderByDescending(edit => edit.From))
        {
            builder.Remove(from, to - from).Insert(from, ' ');
        }

        var fences = document.Descendants<CodeBlock>()
            .Where(block => block is not MathBlock && !(block is FencedCodeBlock fenced && string.Equals(fenced.Info, "math", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(block => block.Span.Start)
            .Select(block => block is FencedCodeBlock fenced ? (fenced.Info ?? string.Empty).Trim().ToLowerInvariant() : string.Empty)
            .ToList();
        var blocks = new List<(string Tag, int Line)>();
        foreach (var block in document)
        {
            if (BlockTag(block) is { } tag)
            {
                blocks.Add((tag, block.Line));
            }
        }

        return (builder.ToString(), fences, blocks);
    }

    // The element GitHub makes of a top-level block, after ToPreviewHtml (display math alone
    // becomes a div). Blocks that make no element of their own, or not a known one (link
    // definitions, footnotes, HTML), have none and are skipped when lining the two up.
    private static string? BlockTag(Block block) => block switch
    {
        // Front matter: GitHub's API makes a line and a heading of it, mdEditor a card.
        Markdig.Extensions.Yaml.YamlFrontMatterBlock => null,
        HeadingBlock heading => "h" + heading.Level,
        MathBlock => "div",
        FencedCodeBlock fenced when string.Equals(fenced.Info, "math", StringComparison.OrdinalIgnoreCase) => "div",
        CodeBlock => "pre",
        ParagraphBlock => "p",
        ListBlock list => list.IsOrdered ? "ol" : "ul",
        Markdig.Extensions.Alerts.AlertBlock => "div",
        QuoteBlock => "blockquote",
        Table => "table",
        ThematicBreakBlock => "hr",
        _ => null
    };

    // GitHub's HTML in the form the preview draws: math as mdEditor's math, code blocks as
    // plain code with their language (so the preview colors them, and draws the diagrams
    // GitHub draws: Mermaid, maps, and 3D models, not Graphviz), ids without GitHub's
    // "user-content-" (which github.com's own script handles), and images at their own
    // address instead of GitHub's image proxy. The sanitizer runs on the result afterwards.
    public static string ToPreviewHtml(string html, IReadOnlyList<string> fences, IReadOnlyList<(string Tag, int Line)> blocks)
    {
        var document = new HtmlParser().ParseDocument("<!doctype html><html><body>" + html + "</body></html>");
        var body = document.Body!;

        var pres = body.QuerySelectorAll("pre").ToList();
        var byOrder = pres.Count == fences.Count;
        for (var index = 0; index < pres.Count; index++)
        {
            var pre = pres[index];
            var holder = pre.ParentElement is { LocalName: "div" } parent && parent.ClassList.Contains("highlight") ? parent : pre;
            var language = byOrder
                ? fences[index].Split(' ', 2)[0]
                : holder.ClassList.FirstOrDefault(name => name.StartsWith("highlight-source-", StringComparison.Ordinal))?["highlight-source-".Length..] ?? string.Empty;
            var code = pre.TextContent;
            IElement replacement;
            if (language == "mermaid")
            {
                replacement = document.CreateElement("pre");
                replacement.ClassList.Add("mermaid");
                replacement.TextContent = code;
            }
            else
            {
                replacement = document.CreateElement("pre");
                var inner = document.CreateElement("code");
                // GitHub shows Graphviz and PlantUML as code.
                if (language.Length > 0 && language is not ("dot" or "graphviz" or "gv" or "plantuml" or "nomnoml")
                    && Regex.IsMatch(language, "^[a-z0-9+#_-]+$"))
                {
                    inner.ClassList.Add("language-" + language);
                }

                inner.TextContent = code.EndsWith('\n') ? code : code + "\n";
                replacement.AppendChild(inner);
            }

            holder.Replace(replacement);
        }

        foreach (var math in body.QuerySelectorAll("math-renderer").ToList())
        {
            var display = math.ClassList.Contains("js-display-math");
            var source = math.TextContent.Trim();
            if (source.StartsWith("$$", StringComparison.Ordinal) && source.EndsWith("$$", StringComparison.Ordinal) && source.Length >= 4)
            {
                source = source[2..^2];
            }
            else if (source.StartsWith('$') && source.EndsWith('$') && source.Length >= 2)
            {
                source = source[1..^1];
            }

            // GitHub escapes < and & in math twice; its script reads them once more.
            source = WebUtility.HtmlDecode(source).Trim();
            var element = document.CreateElement(display ? "div" : "span");
            element.ClassList.Add("math");
            element.TextContent = display ? $"\\[{source}\\]" : $"\\({source}\\)";
            // Display math alone in a paragraph takes the paragraph's place (a div inside a p
            // would end the p when the preview reads the HTML).
            if (display && math.ParentElement is { LocalName: "p" } paragraph
                && paragraph.ChildNodes.All(node => node == math || node.NodeType == NodeType.Text && string.IsNullOrWhiteSpace(node.TextContent)))
            {
                paragraph.Replace(element);
            }
            else
            {
                math.Replace(element);
            }
        }

        foreach (var wrapper in body.QuerySelectorAll("markdown-accessiblity-table, markdown-accessibility-table, g-emoji").ToList())
        {
            wrapper.Replace([.. wrapper.ChildNodes]);
        }

        foreach (var section in body.QuerySelectorAll("section").ToList())
        {
            var div = document.CreateElement("div");
            if (section.GetAttribute("class") is { } classes)
            {
                div.SetAttribute("class", classes);
            }

            div.Append([.. section.ChildNodes]);
            section.Replace(div);
        }

        foreach (var hidden in body.QuerySelectorAll(".sr-only").ToList())
        {
            hidden.ClassList.Remove("sr-only");
            hidden.ClassList.Add("visually-hidden");
        }

        const string prefix = "user-content-";
        foreach (var element in body.QuerySelectorAll("[id]").ToList())
        {
            var id = element.GetAttribute("id") ?? string.Empty;
            if (id.Length == 0)
            {
                element.RemoveAttribute("id");
            }
            else if (id.StartsWith(prefix, StringComparison.Ordinal))
            {
                element.SetAttribute("id", id[prefix.Length..]);
            }
        }

        foreach (var link in body.QuerySelectorAll("a[href]").ToList())
        {
            var href = link.GetAttribute("href")!;
            if (href.StartsWith("#" + prefix, StringComparison.Ordinal))
            {
                link.SetAttribute("href", "#" + href[(prefix.Length + 1)..]);
            }
        }

        // github.com gives headings ids made from their text (the API does not).
        var used = new HashSet<string>(body.QuerySelectorAll("[id]").Select(element => element.GetAttribute("id")!), StringComparer.Ordinal);
        foreach (var heading in body.QuerySelectorAll("h1, h2, h3, h4, h5, h6").Where(heading => !heading.HasAttribute("id")))
        {
            var slug = Slug(heading.TextContent);
            var id = slug;
            for (var number = 1; used.Contains(id); number++)
            {
                id = $"{slug}-{number}";
            }

            used.Add(id);
            heading.SetAttribute("id", id);
        }

        foreach (var image in body.QuerySelectorAll("img[data-canonical-src]").ToList())
        {
            var original = image.GetAttribute("data-canonical-src")!;
            var proxied = image.GetAttribute("src");
            image.SetAttribute("src", original);
            image.RemoveAttribute("data-canonical-src");
            if (image.ParentElement is { LocalName: "a" } link && link.GetAttribute("href") == proxied)
            {
                link.SetAttribute("href", original);
            }
        }

        // Each top-level element gets the source line of its block (data-line, as in the normal
        // preview), so the editor and the view scroll together. Blocks and elements are lined up
        // by kind over the whole document (the longest common subsequence), so a block GitHub
        // makes differently (math it does not read is a paragraph) does not shift the rest.
        var elements = body.Children.Where(element => !element.ClassList.Contains("footnotes")).ToList();
        foreach (var (element, line) in LineUp(blocks, elements))
        {
            element.SetAttribute("data-line", line.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return body.InnerHtml;
    }

    // A block of display math becomes a paragraph when GitHub does not read it as math.
    private static bool SameKind(string block, string element) =>
        block == element || block == "div" && element == "p";

    private static IEnumerable<(IElement Element, int Line)> LineUp(IReadOnlyList<(string Tag, int Line)> blocks, IReadOnlyList<IElement> elements)
    {
        var n = blocks.Count;
        var m = elements.Count;
        if (n == 0 || m == 0 || (long)n * m > 25_000_000)
        {
            yield break;
        }

        var lengths = new int[n + 1, m + 1];
        for (var i = n - 1; i >= 0; i--)
        {
            for (var j = m - 1; j >= 0; j--)
            {
                lengths[i, j] = SameKind(blocks[i].Tag, elements[j].LocalName)
                    ? lengths[i + 1, j + 1] + 1
                    : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        for (int i = 0, j = 0; i < n && j < m;)
        {
            if (SameKind(blocks[i].Tag, elements[j].LocalName) && lengths[i, j] == lengths[i + 1, j + 1] + 1)
            {
                yield return (elements[j], blocks[i].Line);
                i++;
                j++;
            }
            else if (lengths[i + 1, j] >= lengths[i, j + 1])
            {
                i++;
            }
            else
            {
                j++;
            }
        }
    }

    // A heading id as GitHub makes it: lower case, letters, digits, - and _ kept, spaces as -.
    public static string Slug(string text)
    {
        var builder = new StringBuilder();
        foreach (var character in text.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) || character is '-' or '_'
                || char.GetUnicodeCategory(character) is System.Globalization.UnicodeCategory.NonSpacingMark or System.Globalization.UnicodeCategory.SpacingCombiningMark)
            {
                builder.Append(character);
            }
            else if (character == ' ')
            {
                builder.Append('-');
            }
        }

        return builder.ToString();
    }
}
