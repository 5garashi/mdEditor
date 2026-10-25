/*
  ファイル名 / File: PdfFigure.cs
  概要 / Description: Draws a figure found in a PDF again as an SVG picture from its lines, shapes, labels, and pictures
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-25
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Globalization;
using System.Net;
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics.Colors;

namespace MdEditor;

// A diagram or chart printed to PDF keeps what it is drawn with: lines and shapes with their
// colors, and its labels as letters at their places. Those inside the figure's area are
// written out again as SVG (in points, shown at the size the page had), so the figure comes
// back as a picture that stays sharp. Labels are set in a typeface of the same kind (sans
// serif, serif, or monospaced), each letter where the PDF put it. Shadings, clipping inside
// the figure, and the drawing order of text and shapes are not kept: shapes come first,
// then pictures, then text.
internal static partial class PdfImport
{
    // A formula's picture also takes lines that reach out of it (KaTeX draws a root sign as a
    // very long line that the page cuts off), cut off at its edge.
    private static string FigureSvg(Page page, PdfRectangle area, bool formula = false)
    {
        // Italic letters reach a little past their boxes.
        if (formula)
        {
            area = new PdfRectangle(area.Left - 2, area.Bottom - 2, area.Right + 2, area.Top + 2);
        }

        static string N(double value) => Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);
        string X(double x) => N(x - area.Left);
        string Y(double y) => N(area.Top - y);
        string Point(PdfPoint point) => X(point.X) + " " + Y(point.Y);
        // PdfPig gives line widths before the page's scaling; the labels show it (their size
        // on the page over the size they were set in). A width wider than a quarter of the
        // figure is scaled by something else (a nested group) and is kept to that.
        var scales = page.Letters.Where(letter => letter.FontSize > 0 && Contains(area, letter))
            .Select(letter => letter.PointSize / letter.FontSize).Order().ToList();
        var scale = scales.Count > 0 ? scales[scales.Count / 2] : 1;
        var widest = Math.Max(Math.Min(area.Width, area.Height) / 4, 0.25);

        var svg = new StringBuilder();
        svg.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{N(area.Width * 4 / 3)}\" height=\"{N(area.Height * 4 / 3)}\" viewBox=\"0 0 {N(area.Width)} {N(area.Height)}\">\n");
        // The page is white; without it, dark lines would vanish on a dark preview.
        svg.Append("<rect width=\"100%\" height=\"100%\" fill=\"#fff\"/>\n");
        foreach (var path in page.Paths)
        {
            if (path.IsClipping || !(path.IsFilled || path.IsStroked) || path.GetBoundingRectangle() is not { } box
                || !(Contains(area, box) || (formula && Overlaps(area, box) && box.Height <= area.Height * 1.5)))
            {
                continue;
            }

            var d = new StringBuilder();
            PdfPoint? at = null;
            void MoveTo(PdfPoint point)
            {
                if (at is not { } current || Math.Abs(current.X - point.X) > 0.01 || Math.Abs(current.Y - point.Y) > 0.01)
                {
                    d.Append('M').Append(Point(point));
                }
            }

            foreach (var subpath in path)
            {
                at = null;
                foreach (var command in subpath.Commands)
                {
                    switch (command)
                    {
                        case PdfSubpath.Move move:
                            d.Append('M').Append(Point(move.Location));
                            at = move.Location;
                            break;
                        case PdfSubpath.Line line:
                            MoveTo(line.From);
                            d.Append('L').Append(Point(line.To));
                            at = line.To;
                            break;
                        case PdfSubpath.CubicBezierCurve curve:
                            MoveTo(curve.StartPoint);
                            d.Append('C').Append(Point(curve.FirstControlPoint)).Append(' ').Append(Point(curve.SecondControlPoint))
                                .Append(' ').Append(Point(curve.EndPoint));
                            at = curve.EndPoint;
                            break;
                        case PdfSubpath.QuadraticBezierCurve curve:
                            MoveTo(curve.StartPoint);
                            d.Append('Q').Append(Point(curve.ControlPoint)).Append(' ').Append(Point(curve.EndPoint));
                            at = curve.EndPoint;
                            break;
                        case PdfSubpath.Close:
                            d.Append('Z');
                            break;
                    }
                }
            }

            if (d.Length == 0)
            {
                continue;
            }

            svg.Append("<path d=\"").Append(d).Append('"');
            svg.Append(" fill=\"").Append(path.IsFilled ? Color(path.FillColor) : "none").Append('"');
            if (path.IsFilled && path.FillColor is PatternColor)
            {
                svg.Append(" fill-opacity=\"0.5\"");
            }

            if (path.IsFilled && path.FillingRule == FillingRule.EvenOdd)
            {
                svg.Append(" fill-rule=\"evenodd\"");
            }

            if (path.IsStroked)
            {
                svg.Append($" stroke=\"{Color(path.StrokeColor)}\" stroke-width=\"{N(Math.Clamp(path.LineWidth * scale, 0.25, widest))}\" stroke-linejoin=\"round\"");
                if (path.StrokeColor is PatternColor)
                {
                    svg.Append(" stroke-opacity=\"0.5\"");
                }

                if (path.LineDashPattern is { } dash && dash.Array.Count > 0 && dash.Array.Any(length => length > 0))
                {
                    svg.Append($" stroke-dasharray=\"{string.Join(' ', dash.Array.Select(length => N(length * scale)))}\"");
                }
            }

            svg.Append("/>\n");
        }

        foreach (var image in page.GetImages())
        {
            var bounds = image.BoundingBox;
            if (!Contains(area, bounds))
            {
                continue;
            }

            string data;
            if (image.TryGetPng(out var png))
            {
                data = "image/png;base64," + Convert.ToBase64String(png);
            }
            else if (image.RawMemory.Length > 3 && image.RawMemory.Span[0] == 0xFF && image.RawMemory.Span[1] == 0xD8)
            {
                data = "image/jpeg;base64," + Convert.ToBase64String(image.RawMemory.Span);
            }
            else
            {
                continue;
            }

            svg.Append($"<image x=\"{X(bounds.Left)}\" y=\"{Y(bounds.Top)}\" width=\"{N(bounds.Width)}\" height=\"{N(bounds.Height)}\" preserveAspectRatio=\"none\" href=\"data:{data}\"/>\n");
        }

        // Letters in runs: those on one baseline, in one size, color, and typeface are one
        // text element with each letter's own position.
        var run = new List<Letter>();
        void Flush()
        {
            if (run.Count == 0)
            {
                return;
            }

            var first = run[0];
            // KaTeX's math letters are italic; Cambria Math has the large operators.
            var katex = IsMathLetter(first);
            // KaTeX sets large operators and brackets in fonts of their own sizes: such a
            // letter is drawn as tall as it stands, centered on its place.
            if (katex && (first.FontName ?? string.Empty).Contains("KaTeX_Size", StringComparison.OrdinalIgnoreCase))
            {
                var glyph = first.BoundingBox;
                svg.Append($"<text x=\"{X((glyph.Left + glyph.Right) / 2)}\" y=\"{Y((glyph.Top + glyph.Bottom) / 2)}\" font-size=\"{N(glyph.Height)}\" text-anchor=\"middle\" dominant-baseline=\"central\" font-family=\"'Cambria Math', 'Times New Roman', serif\" fill=\"{Color(first.Color)}\">")
                    .Append(WebUtility.HtmlEncode(first.Value)).Append("</text>\n");
                run.Clear();
                return;
            }

            var family = katex ? "'Cambria Math', 'Times New Roman', serif"
                : IsMonospaced(first.FontName) ? "monospace"
                : Family(first.FontName) is var name && (name.Contains("mincho") || name.Contains("times") || name.Contains("serif") && !name.Contains("sans") || name.Contains("georgia")) ? "serif"
                : "sans-serif";
            var angle = Math.Atan2(first.EndBaseLine.Y - first.StartBaseLine.Y, first.EndBaseLine.X - first.StartBaseLine.X) * 180 / Math.PI;
            var (x, y) = (X(first.StartBaseLine.X), Y(first.StartBaseLine.Y));
            // A letter of more than one character (a ligature) spreads over its width.
            var xs = string.Join(' ', run.SelectMany(letter =>
            {
                var count = Math.Max(new StringInfo(letter.Value).LengthInTextElements, 1);
                return Enumerable.Range(0, count).Select(i => X(letter.StartBaseLine.X + (letter.EndBaseLine.X - letter.StartBaseLine.X) * i / count));
            }));
            svg.Append($"<text x=\"{(Math.Abs(angle) > 1 ? x : xs)}\" y=\"{y}\" font-size=\"{N(first.PointSize)}\" font-family=\"{family}\" fill=\"{Color(first.Color)}\"");
            if (IsBold(first))
            {
                svg.Append(" font-weight=\"bold\"");
            }

            if (katex && (first.FontName ?? string.Empty).Contains("Italic", StringComparison.OrdinalIgnoreCase))
            {
                svg.Append(" font-style=\"italic\"");
            }

            if (Math.Abs(angle) > 1)
            {
                svg.Append($" transform=\"rotate({N(-angle)} {x} {y})\"");
            }

            svg.Append('>').Append(WebUtility.HtmlEncode(string.Concat(run.Select(letter => letter.Value)))).Append("</text>\n");
            run.Clear();
        }

        foreach (var letter in page.Letters)
        {
            if (string.IsNullOrWhiteSpace(letter.Value) || letter.RenderingMode == TextRenderingMode.Neither || !Contains(area, letter))
            {
                continue;
            }

            if (run.Count > 0)
            {
                var last = run[^1];
                var level = Math.Abs(last.StartBaseLine.Y - letter.StartBaseLine.Y) < 0.5 && Math.Abs(last.EndBaseLine.Y - last.StartBaseLine.Y) < 0.01
                    && Math.Abs(letter.EndBaseLine.Y - letter.StartBaseLine.Y) < 0.01;
                if (!level || letter.StartBaseLine.X < last.StartBaseLine.X || Math.Abs(last.PointSize - letter.PointSize) > 0.1
                    || last.FontName != letter.FontName || Color(last.Color) != Color(letter.Color))
                {
                    Flush();
                }
            }

            run.Add(letter);
            if (Math.Abs(letter.EndBaseLine.Y - letter.StartBaseLine.Y) >= 0.01
                || (letter.FontName ?? string.Empty).Contains("KaTeX_Size", StringComparison.OrdinalIgnoreCase))
            {
                Flush();
            }
        }

        Flush();
        return svg.Append("</svg>\n").ToString();
    }

    private static string Color(IColor? color)
    {
        try
        {
            if (color is null)
            {
                return "#000";
            }

            // A gradient or a pattern is shown as half-tone gray.
            if (color is PatternColor)
            {
                return "#888";
            }

            var (r, g, b) = color.ToRGBValues();
            return $"#{Byte(r):x2}{Byte(g):x2}{Byte(b):x2}";
        }
        catch (Exception)
        {
            // A pattern or a color space PdfPig cannot turn into RGB.
            return "#000";
        }

        static int Byte(double value) => (int)Math.Round(Math.Clamp(value, 0, 1) * 255);
    }
}
