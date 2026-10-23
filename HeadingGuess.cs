/*
  ファイル名 / File: HeadingGuess.cs
  概要 / Description: Guesses headings from text size, typeface, and numbering, for Word documents without heading styles and for PDFs
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-24
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text.RegularExpressions;

namespace MdEditor;

// One paragraph as read from a Word document or a PDF: its text, its text size in
// points, whether all of it is bold, whether it can never be a heading (a list item, a
// table cell, a line of a table of contents), and whether it is set in another typeface
// than the body text (a Gothic heading over Mincho text, for example).
internal sealed record SizedParagraph(string Text, double Size, bool Bold, bool Excluded = false, bool DistinctFont = false);

// A paragraph clearly larger than the body text (or bold and a step larger), short, and
// not ending like a sentence is taken as a heading. Larger sizes come first, and at the
// same size bold comes before regular, so "14 pt bold" is heading 1 and "14 pt" heading 2.
//
// Documents that number their headings (1. / 3.1 / (1) / (ア) / ①, common in Japanese
// official documents) often set them in another typeface at the body size. There, a short
// numbered line that stands out (another typeface, bold, or a step larger) is a heading,
// and its level follows its size and then how deep its numbering is. Larger unnumbered
// text above them (a title) is heading 1, and the numbered headings start at heading 2.
internal static partial class HeadingGuess
{
    // Regular text must be clearly larger than the body; bold text one step larger is
    // enough (13 pt bold over 11 pt, as many third-level headings are).
    private const double MinimumRatio = 1.2;
    private const double MinimumBoldRatio = 1.08;
    private const int MaximumLength = 60;
    private const int MaximumLevels = 3;
    private const int MaximumNumberedLevels = 6;

    // The heading level (1 to 6) of each paragraph, or 0 for body text.
    public static int[] Levels(IReadOnlyList<SizedParagraph> paragraphs)
    {
        var levels = new int[paragraphs.Count];
        if (paragraphs.Count == 0)
        {
            return levels;
        }

        // The body size is the size most of the text is set in.
        var bodySize = paragraphs
            .GroupBy(paragraph => Round(paragraph.Size))
            .OrderByDescending(group => group.Sum(paragraph => paragraph.Text.Length))
            .First().Key;

        bool Short(SizedParagraph paragraph)
        {
            var text = paragraph.Text.Trim();
            return !paragraph.Excluded
                && text.Length > 0
                && text.Length <= MaximumLength
                && !text.EndsWith('。') && !text.EndsWith('．') && !text.EndsWith('.')
                && !text.EndsWith('：') && !text.EndsWith(':') && !text.EndsWith('、') && !text.EndsWith(',');
        }

        bool BySize(SizedParagraph paragraph) =>
            Short(paragraph) && Round(paragraph.Size) >= bodySize * (paragraph.Bold ? MinimumBoldRatio : MinimumRatio);

        bool ByNumbering(SizedParagraph paragraph) =>
            Short(paragraph) && NumberingDepth(paragraph.Text) > 0
            && (paragraph.DistinctFont || paragraph.Bold || Round(paragraph.Size) >= bodySize * MinimumBoldRatio);

        var numbered = paragraphs.Where(ByNumbering).ToList();
        if (numbered.Count == 0)
        {
            var ranks = paragraphs.Where(BySize)
                .Select(paragraph => (Size: Round(paragraph.Size), paragraph.Bold))
                .Distinct()
                .OrderByDescending(key => key.Size)
                .ThenByDescending(key => key.Bold)
                .Take(MaximumLevels)
                .Select((key, index) => (key, Level: index + 1))
                .ToDictionary(item => item.key, item => item.Level);
            for (var i = 0; i < paragraphs.Count; i++)
            {
                if (BySize(paragraphs[i]) && ranks.TryGetValue((Round(paragraphs[i].Size), paragraphs[i].Bold), out var level))
                {
                    levels[i] = level;
                }
            }

            return levels;
        }

        var largestNumbered = numbered.Max(paragraph => Round(paragraph.Size));
        var numberedRanks = numbered
            .Select(paragraph => (Size: Round(paragraph.Size), paragraph.Bold, Depth: NumberingDepth(paragraph.Text)))
            .Distinct()
            .OrderByDescending(key => key.Size)
            .ThenByDescending(key => key.Bold)
            .ThenBy(key => key.Depth)
            .Take(MaximumNumberedLevels - 1)
            .Select((key, index) => (key, Level: index + 2))
            .ToDictionary(item => item.key, item => item.Level);
        for (var i = 0; i < paragraphs.Count; i++)
        {
            var paragraph = paragraphs[i];
            if (ByNumbering(paragraph))
            {
                levels[i] = numberedRanks.GetValueOrDefault((Round(paragraph.Size), paragraph.Bold, NumberingDepth(paragraph.Text)));
            }
            else if (BySize(paragraph) && NumberingDepth(paragraph.Text) == 0 && Round(paragraph.Size) > largestNumbered)
            {
                levels[i] = 1;
            }
        }

        return levels;
    }

    // How deep a heading's numbering is: 第1章 (1), 1. (2), 1.1 (3), 1.1.1 (4), (1) (5),
    // (ア) (6), ① (7); 0 when the text does not start with heading numbering.
    public static int NumberingDepth(string text)
    {
        var t = text.TrimStart();
        if (ChapterPattern().IsMatch(t)) return 1;
        var dotted = DottedPattern().Match(t);
        if (dotted.Success) return 2 + dotted.Groups[1].Value.Count(c => c is '.' or '．');
        if (SingleNumberPattern().IsMatch(t)) return 2;
        if (ParenNumberPattern().IsMatch(t)) return 5;
        if (ParenKanaPattern().IsMatch(t)) return 6;
        if (t.Length > 1 && t[0] is >= '①' and <= '⑳') return 7;
        return 0;
    }

    [GeneratedRegex(@"^第\s*[0-9０-９一二三四五六七八九十]+\s*[章節部]")]
    private static partial Regex ChapterPattern();

    // "1.1 " "1.1.1 " "1.1." — numbers joined by dots, then a space or a period.
    [GeneratedRegex(@"^([0-9０-９]{1,2}(?:[.．][0-9０-９]{1,2}){1,2})(?:[.．]\s*|\s+)\S")]
    private static partial Regex DottedPattern();

    // "1. " "２．" — a single number needs its period ("1 April" is not a heading number).
    [GeneratedRegex(@"^[0-9０-９]{1,2}[.．]\s*\S")]
    private static partial Regex SingleNumberPattern();

    [GeneratedRegex(@"^[（(][0-9０-９]{1,2}[)）]\s*\S")]
    private static partial Regex ParenNumberPattern();

    [GeneratedRegex(@"^[（(][ア-ンｱ-ﾝa-zａ-ｚ][)）]\s*\S")]
    private static partial Regex ParenKanaPattern();

    // Sizes are compared in half points, as Word stores them, so 14 and 14.04 are equal.
    private static double Round(double size) => Math.Round(size * 2, MidpointRounding.AwayFromZero) / 2;
}
