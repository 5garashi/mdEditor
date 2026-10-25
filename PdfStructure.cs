/*
  ファイル名 / File: PdfStructure.cs
  概要 / Description: Finds what a PDF keeps only as drawings: ruled tables, figures, and bullets or check boxes drawn as shapes
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-25
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace MdEditor;

// A table, a diagram, or a list drawn by a browser or a word processor leaves only lines and
// shapes in the PDF. They are found from the page's paths before its text is read:
// - a grid of thin lines (cell borders) that holds text is a table, read cell by cell;
// - a cluster of shapes that are not boxes (curves, slanted lines, arrowheads) is a figure:
//   it is drawn again as an SVG picture with its labels (PdfFigure.cs), so they are left
//   out of the text;
// - a small shape or a small square just before the start of a line is a bullet or a check
//   box, so the line becomes a list item.
// Boxes behind text (code, notes) and lines under headings are none of these.
internal static partial class PdfImport
{
    private sealed record Grid(PdfRectangle Box, List<double> Columns, List<double> Rows);

    private sealed record Mark(PdfRectangle Box, bool CheckBox, bool Checked);

    // Frames are the wide boxes behind text (a code block's background, a note).
    private sealed record PageShapes(List<Grid> Tables, List<PdfRectangle> Figures, List<Mark> Marks, List<PdfRectangle> Frames)
    {
        public static readonly PageShapes None = new([], [], [], []);
    }

    private sealed record Segment(bool Horizontal, double At, double From, double To);

    private sealed record Shape(PdfRectangle Box, bool BoxLike, bool Filled);

    private static PageShapes FindShapes(Page page)
    {
        var segments = new List<Segment>();
        var shapes = new List<Shape>();
        var clips = new List<PdfRectangle>();
        foreach (var path in page.Paths)
        {
            if (path.IsClipping)
            {
                // A browser clips each diagram (an SVG) to its own area.
                if (path.GetBoundingRectangle() is { } clip)
                {
                    clips.Add(clip);
                }

                continue;
            }

            if (!(path.IsFilled || path.IsStroked))
            {
                continue;
            }

            foreach (var subpath in path)
            {
                if (subpath.GetBoundingRectangle() is not { } box || (box.Width < 0.1 && box.Height < 0.1))
                {
                    continue;
                }

                // The page's own background.
                if (box.Width > page.Width * 0.9 && box.Height > page.Height * 0.9)
                {
                    continue;
                }

                if (box.Height <= 2 && box.Width >= 6)
                {
                    segments.Add(new Segment(true, (box.Top + box.Bottom) / 2, box.Left, box.Right));
                }
                else if (box.Width <= 2 && box.Height >= 6)
                {
                    segments.Add(new Segment(false, (box.Left + box.Right) / 2, box.Bottom, box.Top));
                }
                else
                {
                    shapes.Add(new Shape(box, IsBoxLike(subpath), path.IsFilled));
                }
            }
        }

        var letters = page.Letters.Where(letter => !string.IsNullOrWhiteSpace(letter.Value)).ToList();
        var (tables, used) = FindTables(segments, letters);
        var marks = FindMarks(shapes, letters);
        // A wide box (behind code, a note, or the front matter) frames text, not a figure.
        // Boxes and table lines are part of a figure but do not make one. Marks count too
        // (a chart's points and legend look like bullets); a list's column of bullets is
        // too narrow to be a figure.
        var parts = shapes.Where(shape => !(shape.BoxLike && shape.Box.Width > page.Width * 0.45))
            .Select(shape => (shape.Box, Counts: !shape.BoxLike))
            .Concat(segments.Select(segment => (Box: SegmentBox(segment), Counts: !used.Contains(segment))))
            .ToList();
        var figures = new List<PdfRectangle>();
        foreach (var cluster in Clusters(parts.Select(part => part.Box).ToList(), 6))
        {
            var box = Union(cluster.Select(index => parts[index].Box));
            if (cluster.Count(index => parts[index].Counts) < 3 || box.Width < 30 || box.Height < 30)
            {
                continue;
            }

            // A formula (KaTeX draws root signs and large brackets) stays text.
            var inside = letters.Where(letter => Contains(box, letter)).ToList();
            if (inside.Count > 0 && inside.Count(letter => (letter.FontName ?? string.Empty).Contains("KaTeX", StringComparison.OrdinalIgnoreCase)) * 2 >= inside.Count)
            {
                continue;
            }

            figures.Add(box);
        }

        // A figure takes in its labels and titles: it is the smallest clipped area around it
        // that is not the whole page.
        for (var i = 0; i < figures.Count; i++)
        {
            var figure = figures[i];
            var area = clips.Where(clip => Contains(clip, figure) && !(clip.Width > page.Width * 0.8 && clip.Height > page.Height * 0.6))
                .OrderBy(clip => clip.Width * clip.Height).FirstOrDefault();
            if (area.Width > 0)
            {
                figures[i] = area;
            }
        }

        tables.RemoveAll(table => figures.Any(figure => Overlaps(figure, table.Box)));
        marks.RemoveAll(mark => figures.Any(figure => Contains(figure, mark.Box)));
        var frames = shapes.Where(shape => shape.BoxLike && shape.Box.Width > page.Width * 0.45 && !figures.Any(figure => Overlaps(figure, shape.Box)))
            .Select(shape => shape.Box).ToList();
        return new PageShapes(tables, figures, marks, frames);
    }

    // A rectangle, or one with rounded corners: straight lines only across or down, and
    // curves only as small corners. A small circle (curves only) is not a box.
    private static bool IsBoxLike(PdfSubpath subpath) => subpath.Commands.Count(command => command is PdfSubpath.Line) >= 2 && subpath.Commands.All(command => command switch
    {
        PdfSubpath.Line line => Math.Abs(line.From.X - line.To.X) < 0.5 || Math.Abs(line.From.Y - line.To.Y) < 0.5,
        PdfSubpath.BezierCurve curve => curve.GetBoundingRectangle() is not { } box || (box.Width <= 8 && box.Height <= 8),
        _ => true
    });

    // Thin lines that cross each other make a grid; one with at least two columns and two
    // rows that holds text is a table.
    private static (List<Grid> Tables, HashSet<Segment> Used) FindTables(List<Segment> segments, List<Letter> letters)
    {
        var tables = new List<Grid>();
        var used = new HashSet<Segment>();
        var boxes = segments.Select(SegmentBox).ToList();
        foreach (var cluster in Clusters(boxes, 2))
        {
            var group = cluster.Select(index => segments[index]).ToList();
            var columns = Distinct(group.Where(segment => !segment.Horizontal).Select(segment => segment.At));
            var rows = Distinct(group.Where(segment => segment.Horizontal).Select(segment => segment.At));
            var box = Union(cluster.Select(index => boxes[index]));
            // One row is a table too when its column lines run its full height (the rest of a
            // table that goes on from the page before).
            var oneRow = rows.Count == 2 && group.Where(segment => !segment.Horizontal)
                .All(segment => segment.From <= box.Bottom + 3 && segment.To >= box.Top - 3);
            if (columns.Count < 3 || (rows.Count < 3 && !oneRow))
            {
                continue;
            }

            rows.Reverse();
            if (!letters.Any(letter => Contains(box, letter)))
            {
                continue;
            }

            tables.Add(new Grid(box, columns, rows));
            used.UnionWith(group);
        }

        return (tables, used);
    }

    // A small shape, or a small square box, right before the first letter of a line (with no
    // letter just before it) marks a list item. A square with something inside is ticked.
    private static List<Mark> FindMarks(List<Shape> shapes, List<Letter> letters)
    {
        var marks = new List<Mark>();
        foreach (var shape in shapes)
        {
            var box = shape.Box;
            var dot = box.Width <= 7 && box.Height <= 7;
            var square = shape.BoxLike && box.Width is >= 8 and <= 16 && Math.Abs(box.Width - box.Height) <= 2;
            if (!dot && !square)
            {
                continue;
            }

            var middle = (box.Top + box.Bottom) / 2;
            bool SameLine(Letter letter) => middle > letter.GlyphRectangle.Bottom - 2 && middle < letter.GlyphRectangle.Top + 2;
            var size = letters.Where(SameLine).Select(letter => letter.PointSize).DefaultIfEmpty(0).Max();
            if (size <= 0)
            {
                continue;
            }

            var after = letters.Any(letter => SameLine(letter) && letter.GlyphRectangle.Left >= box.Right - 1
                && letter.GlyphRectangle.Left - box.Right < size * 2.5);
            var before = letters.Any(letter => SameLine(letter) && letter.GlyphRectangle.Right <= box.Left + 1
                && box.Left - letter.GlyphRectangle.Right < size * 1.5);
            if (!after || before)
            {
                continue;
            }

            var ticked = square && shapes.Any(other => !ReferenceEquals(other, shape) && Contains(box, other.Box) && other.Box.Width < box.Width - 1);
            marks.Add(new Mark(box, square, ticked));
        }

        return marks;
    }

    // The mark before a line, if any: it ends just left of the line's start, level with it.
    private static Mark? MarkBefore(PageShapes shapes, double left, double top, double size) =>
        shapes.Marks.FirstOrDefault(mark => mark.Box.Right <= left + 1 && left - mark.Box.Right < Math.Max(size, 1) * 2.5
            && (mark.Box.Top + mark.Box.Bottom) / 2 is var middle && middle <= top + 2 && middle >= top - Math.Max(size, 1) * 1.3);

    // A table as Markdown: the first row is the header; a cell's lines are joined, and "|"
    // is escaped.
    private static string TableMarkdown(Grid grid, IReadOnlyList<Letter> letters)
    {
        var inside = letters.Where(letter => !string.IsNullOrWhiteSpace(letter.Value) && Contains(grid.Box, letter)).ToList();
        var words = UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor.NearestNeighbourWordExtractor.Instance.GetWords(inside).ToList();
        var cells = new string[grid.Rows.Count - 1, grid.Columns.Count - 1];
        for (var row = 0; row < grid.Rows.Count - 1; row++)
        {
            for (var column = 0; column < grid.Columns.Count - 1; column++)
            {
                var (top, bottom, left, right) = (grid.Rows[row], grid.Rows[row + 1], grid.Columns[column], grid.Columns[column + 1]);
                var cellWords = words.Where(word =>
                {
                    var x = (word.BoundingBox.Left + word.BoundingBox.Right) / 2;
                    var y = (word.BoundingBox.Top + word.BoundingBox.Bottom) / 2;
                    return x > left && x < right && y < top && y > bottom;
                }).ToList();
                var lines = CellLines(cellWords).Select(line => ReadLine(line).Text).Where(text => text.Length > 0);
                cells[row, column] = JoinLines(lines).Replace("|", "\\|");
            }
        }

        var builder = new StringBuilder();
        for (var row = 0; row < cells.GetLength(0); row++)
        {
            if (Enumerable.Range(0, cells.GetLength(1)).All(column => cells[row, column].Length == 0))
            {
                continue;
            }

            builder.Append('|');
            for (var column = 0; column < cells.GetLength(1); column++)
            {
                builder.Append(' ').Append(cells[row, column]).Append(" |");
            }

            builder.Append('\n');
            if (builder.ToString().Count(c => c == '\n') == 1)
            {
                builder.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", cells.GetLength(1)))).Append('\n');
            }
        }

        return builder.ToString().TrimEnd('\n');
    }

    // The words of a cell in lines, top to bottom, each read left to right.
    private static List<List<Word>> CellLines(List<Word> words)
    {
        var lines = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(word => word.BoundingBox.Bottom))
        {
            var line = lines.LastOrDefault();
            if (line is not null && Math.Abs(line[0].BoundingBox.Bottom - word.BoundingBox.Bottom) < Math.Max(word.BoundingBox.Height, 1) * 0.5)
            {
                line.Add(word);
            }
            else
            {
                lines.Add([word]);
            }
        }

        foreach (var line in lines)
        {
            line.Sort((a, b) => a.BoundingBox.Left.CompareTo(b.BoundingBox.Left));
        }

        return lines;
    }

    // Groups of boxes that touch or lie within `gap` of each other.
    private static List<List<int>> Clusters(List<PdfRectangle> boxes, double gap)
    {
        var parent = Enumerable.Range(0, boxes.Count).ToArray();
        int Find(int i) => parent[i] == i ? i : parent[i] = Find(parent[i]);
        for (var i = 0; i < boxes.Count; i++)
        {
            for (var j = i + 1; j < boxes.Count; j++)
            {
                if (boxes[i].Left - gap <= boxes[j].Right && boxes[j].Left - gap <= boxes[i].Right
                    && boxes[i].Bottom - gap <= boxes[j].Top && boxes[j].Bottom - gap <= boxes[i].Top)
                {
                    parent[Find(i)] = Find(j);
                }
            }
        }

        return Enumerable.Range(0, boxes.Count).GroupBy(Find).Select(group => group.ToList()).ToList();
    }

    // Positions within 2.5 points count as one.
    private static List<double> Distinct(IEnumerable<double> values)
    {
        var result = new List<double>();
        foreach (var value in values.OrderBy(value => value))
        {
            if (result.Count == 0 || value - result[^1] > 2.5)
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static PdfRectangle SegmentBox(Segment segment) => segment.Horizontal
        ? new PdfRectangle(segment.From, segment.At - 1, segment.To, segment.At + 1)
        : new PdfRectangle(segment.At - 1, segment.From, segment.At + 1, segment.To);

    private static PdfRectangle Union(IEnumerable<PdfRectangle> boxes)
    {
        var list = boxes.ToList();
        return new PdfRectangle(list.Min(box => box.Left), list.Min(box => box.Bottom), list.Max(box => box.Right), list.Max(box => box.Top));
    }

    private static bool Contains(PdfRectangle outer, PdfRectangle inner) =>
        inner.Left >= outer.Left - 1 && inner.Right <= outer.Right + 1 && inner.Bottom >= outer.Bottom - 1 && inner.Top <= outer.Top + 1;

    // The smallest frame a point is in, or -1.
    private static int FrameAt(PageShapes shapes, double x, double y)
    {
        var found = -1;
        for (var i = 0; i < shapes.Frames.Count; i++)
        {
            var frame = shapes.Frames[i];
            if (x >= frame.Left && x <= frame.Right && y >= frame.Bottom && y <= frame.Top
                && (found < 0 || frame.Width * frame.Height < shapes.Frames[found].Width * shapes.Frames[found].Height))
            {
                found = i;
            }
        }

        return found;
    }

    private static bool Overlaps(PdfRectangle a, PdfRectangle b) =>
        a.Left < b.Right && b.Left < a.Right && a.Bottom < b.Top && b.Bottom < a.Top;

    private static bool Contains(PdfRectangle box, Letter letter)
    {
        var x = (letter.GlyphRectangle.Left + letter.GlyphRectangle.Right) / 2;
        var y = (letter.GlyphRectangle.Top + letter.GlyphRectangle.Bottom) / 2;
        return x >= box.Left && x <= box.Right && y >= box.Bottom && y <= box.Top;
    }
}
