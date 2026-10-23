/*
  ファイル名 / File: HeadingGuess.cs
  概要 / Description: Guesses headings from text size, for Word documents without heading styles and for PDFs
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-24
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
namespace MdEditor;

// One paragraph as read from a Word document or a PDF: its text, its text size in
// points, whether all of it is bold, and whether it can never be a heading (a list
// item, a table cell).
internal sealed record SizedParagraph(string Text, double Size, bool Bold, bool Excluded = false);

// A paragraph clearly larger than the body text (or bold and a step larger), short, and
// not ending like a sentence is taken as a heading. Larger sizes come first, and at the same size bold comes
// before regular, so "14 pt bold" is heading 1 and "14 pt" heading 2. Up to three
// levels; a paragraph the size of the body text is never a heading.
internal static class HeadingGuess
{
    // Regular text must be clearly larger than the body; bold text one step larger is
    // enough (13 pt bold over 11 pt, as many third-level headings are).
    private const double MinimumRatio = 1.2;
    private const double MinimumBoldRatio = 1.08;
    private const int MaximumLength = 60;
    private const int MaximumLevels = 3;

    // The heading level (1 to 3) of each paragraph, or 0 for body text.
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

        bool IsCandidate(SizedParagraph paragraph)
        {
            var text = paragraph.Text.Trim();
            return !paragraph.Excluded
                && text.Length > 0
                && text.Length <= MaximumLength
                && Round(paragraph.Size) >= bodySize * (paragraph.Bold ? MinimumBoldRatio : MinimumRatio)
                && !text.EndsWith('。') && !text.EndsWith('．') && !text.EndsWith('.')
                && !text.EndsWith('：') && !text.EndsWith(':') && !text.EndsWith('、') && !text.EndsWith(',');
        }

        var ranks = paragraphs.Where(IsCandidate)
            .Select(paragraph => (Size: Round(paragraph.Size), paragraph.Bold))
            .Distinct()
            .OrderByDescending(key => key.Size)
            .ThenByDescending(key => key.Bold)
            .Take(MaximumLevels)
            .Select((key, index) => (key, Level: index + 1))
            .ToDictionary(item => item.key, item => item.Level);

        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (IsCandidate(paragraphs[i]) && ranks.TryGetValue((Round(paragraphs[i].Size), paragraphs[i].Bold), out var level))
            {
                levels[i] = level;
            }
        }

        return levels;
    }

    // Sizes are compared in half points, as Word stores them, so 14 and 14.04 are equal.
    private static double Round(double size) => Math.Round(size * 2, MidpointRounding.AwayFromZero) / 2;
}
