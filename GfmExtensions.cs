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
    public static MarkdownPipeline Build(bool nearGitHub)
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

        builder.Extensions.AddIfNotAlready<LatexMathDelimitersExtension>();

        // As on GitHub: front matter becomes a table and :smile: an emoji
        // (smileys such as :) stay text).
        builder.UseYamlFrontMatter().UseEmojiAndSmiley(enableSmileys: false);
        builder.Extensions.AddIfNotAlready<GfmDetailsExtension>();
        return builder.Build();
    }
}

// Math written with LaTeX's own delimiters, as many AI chats and papers write it: \(x\) in a
// line, \[x\] shown on a line of its own. Markdig reads only $x$ and $$x$$ (and would show
// \( as an escaped "("). The math is the same MathInline, so preview, PDF, HTML, and Word
// draw it as they draw $x$; \[x\] is marked math-display. Both pipelines read it, though
// GitHub itself shows \( as an escaped parenthesis: the math is wanted more than the
// difference.
internal sealed class LatexMathDelimitersExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<LatexMathInlineParser>())
        {
            pipeline.InlineParsers.InsertBefore<EscapeInlineParser>(new LatexMathInlineParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}

internal sealed class LatexMathInlineParser : InlineParser
{
    public LatexMathInlineParser()
    {
        OpeningCharacters = ['\\'];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var open = slice.PeekChar();
        if (open is not ('(' or '['))
        {
            return false;
        }

        var close = open == '(' ? @"\)" : @"\]";
        var text = slice.Text;
        var start = slice.Start + 2;
        var end = text.IndexOf(close, start, slice.End - start + 1, StringComparison.Ordinal);
        // An empty \(\), one never closed, and one with another opening before its close (a
        // lone escaped "(") are left to the escape parser.
        if (end < 0 || end == start || string.IsNullOrWhiteSpace(text[start..end])
            || text.IndexOf(close == @"\)" ? @"\(" : @"\[", start, end - start, StringComparison.Ordinal) >= 0)
        {
            return false;
        }

        var math = new MathInline
        {
            Delimiter = '$',
            DelimiterCount = open == '(' ? 1 : 2,
            Content = new StringSlice(text, start, end - 1),
            Span = new SourceSpan(processor.GetSourcePosition(slice.Start, out var line, out var column), processor.GetSourcePosition(end + 1)),
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
        slice.Start = end + 2;
        return true;
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
