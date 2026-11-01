/*
  File: GfmExtensions.cs
  Description: The Markdown pipelines (mdEditor's own and the one closer to GitHub) and the GitHub Flavored Markdown details Markdig leaves out
  Author: 5garashi.com Design Office
  Created: 2026-10-19
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdEditor;

internal static class MarkdownPipelines
{
    // mdEditor's own pipeline adds Markdig's extras GitHub does not have (x^2^, H~2~O,
    // ==mark==, definition lists, ::: containers, {#id} attributes, and so on). "Closer
    // to GitHub" keeps only what GitHub shows, and ~text~ is struck through as there.
    // lineBreaks shows each line break in a paragraph as a line break, not a space;
    // latexMath reads \(x\) and \[x\] as math (GitHub does neither).
    public static MarkdownPipeline Build(bool nearGitHub, bool lineBreaks = false, bool latexMath = true)
    {
        // GitHub-style heading ids keep Japanese text, so links like [x](#セットアップ) work.
        var builder = new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub);
        if (nearGitHub)
        {
            builder.UseAlertBlocks()
                .UseFootnotes()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseTaskLists()
                .UseDiagrams()
                .UseAutoLinks()
                .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough);
            builder.Extensions.AddIfNotAlready<SingleTildeStrikethroughExtension>();
        }
        else
        {
            builder.UseAdvancedExtensions();
        }

        builder.Extensions.Add(new LatexMathDelimitersExtension(latexMath));

        // As on GitHub: front matter becomes a table and :smile: an emoji
        // (smileys such as :) stay text).
        builder.UseYamlFrontMatter().UseEmojiAndSmiley(enableSmileys: false);
        builder.Extensions.AddIfNotAlready<GfmDetailsExtension>();
        builder.Extensions.Add(new LineBreaksExtension(lineBreaks));
        return builder.Build();
    }
}

// Each line break in a paragraph kept as a line break, as some previews show it (VS Code's
// Markdown Preview Enhanced by default). The breaks are made hard in the document itself,
// not only in the HTML renderer, so the Word export and the rest follow it too. Either way
// the document keeps how many there were (SoftBreaks), for the notice on opening.
internal sealed class LineBreaksExtension(bool show) : IMarkdownExtension
{
    public static int SoftBreaks(MarkdownDocument document) => document.GetData(typeof(LineBreaksExtension)) as int? ?? 0;

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed += Process;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private void Process(MarkdownDocument document)
    {
        var count = 0;
        foreach (var block in document.Descendants<LeafBlock>().Where(block => block.Inline is not null))
        {
            foreach (var lineBreak in block.Inline!.Descendants<LineBreakInline>().Where(lineBreak => !lineBreak.IsHard))
            {
                count += 1;
                lineBreak.IsHard = show;
            }
        }

        document.SetData(typeof(LineBreaksExtension), count);
    }
}

// Math written with LaTeX's own delimiters, as many AI chats and papers write it: \(x\) in a
// line, \[x\] shown on a line of its own. Markdig reads only $x$ and $$x$$ (and would show
// \( as an escaped "("). The math is the same MathInline, so preview, PDF, HTML, and Word
// draw it as they draw $x$; \[x\] is marked math-display. Both pipelines read it, though
// GitHub itself shows \( as an escaped parenthesis: the math is wanted more than the
// difference, unless the other settings follow GitHub too (latex is then false). \\(x\\) and
// \\[x\\], with Markdown's escapes inside (\_), are read too: they are written for pages that
// find math after Markdown has made the HTML (MathJax in Jupyter and some sites), and the
// math is what that page would draw. The rest here ($$x$$ on a line of its own, a letter
// after the closing $) is always read.
internal sealed class LatexMathDelimitersExtension(bool latex) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (latex && !pipeline.InlineParsers.Contains<LatexMathInlineParser>())
        {
            pipeline.InlineParsers.InsertBefore<EscapeInlineParser>(new LatexMathInlineParser());
        }

        if (latex && !pipeline.BlockParsers.Contains<LatexEnvironmentBlockParser>())
        {
            pipeline.BlockParsers.InsertBefore<ParagraphBlockParser>(new LatexEnvironmentBlockParser());
        }

        // $$x$$ inside a paragraph is shown on a line of its own too, as GitHub does
        // (and \tag, which KaTeX allows only there, then works).
        pipeline.DocumentProcessed += document =>
        {
            foreach (var math in document.Descendants<MathInline>())
            {
                var attributes = math.GetAttributes();
                if (math.DelimiterCount >= 2 && attributes.Classes?.Contains("math-display") != true)
                {
                    attributes.AddClass("math-display");
                }
            }
        };

        if (pipeline.InlineParsers.Contains<MathInlineParser>() && !pipeline.InlineParsers.Contains<LetterAfterMathInlineParser>())
        {
            pipeline.InlineParsers.InsertBefore<MathInlineParser>(new LetterAfterMathInlineParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}

// $x$ (or $$x$$) followed at once by a letter, as Japanese and Chinese write it ($d$という,
// with no space): Markdig does not close math before any letter, so the whole of it stayed
// text, though the editor colors it as math. Only that case is read here, by the editor's
// rules ($x$: no space just inside either $, no digit after the closing $, one line);
// everything else, and an ASCII letter after it ($x$abc), is left to Markdig as before.
internal sealed class LetterAfterMathInlineParser : InlineParser
{
    public LetterAfterMathInlineParser()
    {
        OpeningCharacters = ['$'];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var text = slice.Text;
        var count = slice.Start + 1 <= slice.End && text[slice.Start + 1] == '$' ? 2 : 1;
        var mark = new string('$', count);
        var start = slice.Start + count;
        if (start > slice.End || text[start] is '$' || count == 1 && char.IsWhiteSpace(text[start]))
        {
            return false;
        }

        var end = start;
        while (true)
        {
            end = text.IndexOf(mark, end, slice.End - end + 1, StringComparison.Ordinal);
            if (end < 0)
            {
                return false;
            }

            if (text[end - 1] != '\\')
            {
                break;
            }

            end += 1;
        }

        var after = end + count <= slice.End ? text[end + count] : '\0';
        if (end == start || !char.IsLetter(after) || after < 0x80
            || count == 1 && (text.AsSpan(start, end - start).IndexOfAny('\n', '\r') >= 0 || char.IsWhiteSpace(text[end - 1])))
        {
            return false;
        }

        // $$x$$ in a paragraph is marked math-display when the document is done, as Markdig's own.
        var math = new MathInline
        {
            Delimiter = '$',
            DelimiterCount = count,
            Content = new StringSlice(text, start, end - 1),
            Span = new SourceSpan(processor.GetSourcePosition(slice.Start, out var line, out var column), processor.GetSourcePosition(end + count - 1)),
            Line = line,
            Column = column
        };
        math.GetAttributes().AddClass("math");
        processor.Inline = math;
        slice.Start = end + count;
        return true;
    }
}

internal sealed class LatexMathInlineParser : InlineParser
{
    // A backslash before ASCII punctuation, which Markdown reads as that character (\_ is _).
    private static readonly Regex MarkdownEscape = new(@"\\([!-/:-@\[-`{-~])", RegexOptions.CultureInvariant);

    public LatexMathInlineParser()
    {
        OpeningCharacters = ['\\'];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // \\( and \\[: the backslash escaped, as Markdown writes one to be kept.
        var escaped = slice.PeekChar() == '\\';
        var open = slice.PeekChar(escaped ? 2 : 1);
        if (open is not ('(' or '['))
        {
            return false;
        }

        var mark = escaped ? @"\\" : @"\";
        var close = mark + (open == '(' ? ")" : "]");
        var text = slice.Text;
        var start = slice.Start + mark.Length + 1;
        var end = text.IndexOf(close, start, slice.End - start + 1, StringComparison.Ordinal);
        // An empty \(\), one never closed, and one with another opening before its close (a
        // lone escaped "(") are left to the escape parser.
        if (end < 0 || end == start || string.IsNullOrWhiteSpace(text[start..end])
            || text.IndexOf(mark + open, start, end - start, StringComparison.Ordinal) >= 0)
        {
            return false;
        }

        var math = new MathInline
        {
            Delimiter = '$',
            DelimiterCount = open == '(' ? 1 : 2,
            Content = escaped ? new StringSlice(MarkdownEscape.Replace(text[start..end], "$1")) : new StringSlice(text, start, end - 1),
            Span = new SourceSpan(processor.GetSourcePosition(slice.Start, out var line, out var column), processor.GetSourcePosition(end + close.Length - 1)),
            Line = line,
            Column = column
        };
        // The class the preview draws math by (Markdig's own math parser adds it too).
        math.GetAttributes().AddClass("math");
        if (open == '[')
        {
            math.GetAttributes().AddClass("math-display");
        }

        processor.Inline = math;
        slice.Start = end + close.Length;
        return true;
    }
}

// A LaTeX environment standing on its own, with no $$ around it (\begin{align} … \end{align}),
// as Jupyter and other pages drawing math with MathJax read it: a math block like $$ … $$.
// Only the environments KaTeX draws at the top. As a paragraph it was text, and its {align}
// was even taken for an attribute.
internal sealed class LatexEnvironmentBlockParser : BlockParser
{
    private static readonly Regex Begin = new(@"^\\begin\{(equation|align|alignat|gather)(\*?)\}", RegexOptions.CultureInvariant);

    public LatexEnvironmentBlockParser()
    {
        OpeningCharacters = ['\\'];
    }

    public override BlockState TryOpen(BlockProcessor processor)
    {
        if (processor.IsCodeIndent || Begin.Match(processor.Line.ToString()) is not { Success: true } begin)
        {
            return BlockState.None;
        }

        var block = new MathBlock(this)
        {
            Column = processor.Column,
            Line = processor.LineIndex,
            Span = new SourceSpan(processor.Start, processor.Line.End)
        };
        // The class the preview draws math by, as Markdig's $$ parser adds it.
        block.GetAttributes().AddClass("math");
        var end = $@"\end{{{begin.Groups[1].Value}{begin.Groups[2].Value}}}";
        block.SetData(typeof(LatexEnvironmentBlockParser), end);
        processor.NewBlocks.Push(block);
        return Take(processor, block, end);
    }

    public override BlockState TryContinue(BlockProcessor processor, Block block) =>
        Take(processor, (MathBlock)block, (string)block.GetData(typeof(LatexEnvironmentBlockParser))!);

    // The \begin and \end lines belong to the math (KaTeX reads them), so the line is kept
    // even when it ends the block. A blank line ends one never closed, as it ends a paragraph
    // (where MathJax could not read it either), so it does not take the rest of the document.
    private static BlockState Take(BlockProcessor processor, MathBlock block, string end)
    {
        if (processor.IsBlankLine)
        {
            return BlockState.Break;
        }

        var line = processor.Line;
        if (!line.ToString().Contains(end, StringComparison.Ordinal))
        {
            return BlockState.Continue;
        }

        block.AppendLine(ref line, processor.Column, processor.LineIndex, processor.CurrentLineStartPosition, processor.TrackTrivia);
        block.UpdateSpanEnd(line.End);
        return BlockState.BreakDiscard;
    }
}

// GitHub strikes through ~text~ as well as ~~text~~ (Markdig needs two).
internal sealed class SingleTildeStrikethroughExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (pipeline.InlineParsers.Find<EmphasisInlineParser>() is { } parser)
        {
            parser.EmphasisDescriptors.RemoveAll(descriptor => descriptor.Character == '~');
            parser.EmphasisDescriptors.Add(new EmphasisDescriptor('~', 1, 2, true));
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer html && html.ObjectRenderers.FindExact<EmphasisInlineRenderer>() is { } emphasis)
        {
            var previous = emphasis.GetTag;
            emphasis.GetTag = inline => inline.DelimiterChar == '~' ? "del" : previous(inline);
        }
    }
}

// What GitHub does and Markdig does not, in both pipelines: "\|" in code inside a
// table cell is a plain "|" (GFM unescapes it before the cell is read), and an
// e-mail address in the text is a mailto: link (GFM's extended autolinks).
internal sealed class GfmDetailsExtension : IMarkdownExtension
{
    // GFM's extended e-mail autolink: the last character of the domain is not - or _.
    private static readonly Regex Email = new(
        @"(?<![A-Za-z0-9._+-])[A-Za-z0-9._+-]+@[A-Za-z0-9_-]+(?:\.[A-Za-z0-9_-]+)+",
        RegexOptions.CultureInvariant);

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed -= Process;
        pipeline.DocumentProcessed += Process;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private static void Process(MarkdownDocument document)
    {
        foreach (var code in document.Descendants<TableCell>().SelectMany(cell => cell.Descendants<CodeInline>()))
        {
            if (code.Content.Contains("\\|", StringComparison.Ordinal))
            {
                code.Content = code.Content.Replace("\\|", "|", StringComparison.Ordinal);
            }
        }

        // A block's own inline container is not among the document's descendants.
        var containers = document.Descendants<LeafBlock>()
            .Where(block => block.Inline is not null)
            .SelectMany(block => block.Inline!.Descendants<ContainerInline>().Prepend(block.Inline!))
            .ToList();
        foreach (var container in containers)
        {
            if (container is LinkInline || container.FindParentOfType<LinkInline>().Any())
            {
                continue;
            }

            LinkEmails(container);
        }
    }

    // Text in a run of plain literals (the parser may split it at characters such as _).
    private static void LinkEmails(ContainerInline container)
    {
        var run = new List<LiteralInline>();
        foreach (var child in container.ToList())
        {
            if (child is LiteralInline literal)
            {
                run.Add(literal);
                continue;
            }

            LinkRun(run);
            run.Clear();
        }

        LinkRun(run);
    }

    private static void LinkRun(List<LiteralInline> run)
    {
        if (run.Count == 0)
        {
            return;
        }

        var text = string.Concat(run.Select(literal => literal.Content.ToString()));
        var matches = Email.Matches(text).Where(match => match.Value[^1] is not ('-' or '_')).ToList();
        if (matches.Count == 0 || !text.Contains('@'))
        {
            return;
        }

        var first = run[0];
        var position = 0;
        foreach (var match in matches)
        {
            // A trailing "." ends the sentence, not the address.
            var address = match.Value.TrimEnd('.');
            if (!address[(address.IndexOf('@') + 1)..].Contains('.'))
            {
                continue;
            }

            if (match.Index > position)
            {
                first.InsertBefore(new LiteralInline(text[position..match.Index]));
            }

            first.InsertBefore(new AutolinkInline(address) { IsEmail = true });
            position = match.Index + address.Length;
        }

        if (position == 0)
        {
            return;
        }

        if (position < text.Length)
        {
            first.InsertBefore(new LiteralInline(text[position..]));
        }

        foreach (var literal in run)
        {
            literal.Remove();
        }
    }
}
