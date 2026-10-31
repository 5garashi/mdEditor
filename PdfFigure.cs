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
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Filters;
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.Graphics.Operations;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;
using UglyToad.PdfPig.Logging;
using UglyToad.PdfPig.Parser;
using UglyToad.PdfPig.Tokens;

namespace MdEditor;

// A diagram or chart printed to PDF keeps what it is drawn with: lines and shapes with their
// colors, and its labels as letters at their places. Those inside the figure's area are
// written out again as SVG (in points, shown at the size the page had), so the figure comes
// back as a picture that stays sharp. Labels are set in a typeface of the same kind (sans
// serif, serif, or monospaced), each letter where the PDF put it. Shadings, clipping inside
// the figure, and the drawing order of text and shapes are not kept: shapes that hold a
// picture (its background or frame) come first, then pictures, then the other shapes (lines
// and names drawn over an illustration), then text.
internal static partial class PdfImport
{
    // How each shape the page paints was drawn, in the order of page.Paths without the
    // clipping paths: its see-through (fill, stroke), the scaling its line width is set in
    // (Scale), and the space of the drawing it is in (Base, where a gradient is placed).
    private sealed record PaintState(double Fill, double Stroke, double Scale, TransformationMatrix Base);

    // PdfPig reads these but does not keep them with the shape, so the page's drawing is
    // gone through again: the graphics states (gs) and scalings (cm) it sets, saved and
    // restored with q and Q, inside the forms it draws too (a browser puts a whole page in
    // one). A form drawn as a group (a browser's opacity) is see-through as a whole: the
    // see-through set when it is drawn applies on top of what it sets inside. Null when the
    // shapes found do not match PdfPig's (all are then drawn solid, and line widths guessed).
    private static IReadOnlyList<PaintState>? PathStates(PdfDocument document, Page page)
    {
        try
        {
            IToken? Resolve(IToken? token) => token is IndirectReferenceToken reference ? document.Structure.GetObject(reference.Data)?.Data : token;
            T? Get<T>(DictionaryToken? dictionary, string name) where T : class, IToken =>
                dictionary is not null && dictionary.TryGet(NameToken.Create(name), out var value) ? Resolve(value) as T : null;

            var parser = new PageContentParser(ReflectionGraphicsStateOperationFactory.Instance, new StackDepthGuard(256), false);
            var paints = new List<PaintState>();
            var current = (Fill: 1.0, Stroke: 1.0);
            var matrix = TransformationMatrix.Identity;
            var saved = new Stack<((double Fill, double Stroke) Alpha, TransformationMatrix Matrix)>();
            void Run(IEnumerable<IGraphicsStateOperation> operations, DictionaryToken? resources, (double Fill, double Stroke) group, TransformationMatrix space, int depth)
            {
                var states = Get<DictionaryToken>(resources, "ExtGState");
                foreach (var operation in operations)
                {
                    switch (operation)
                    {
                        case Push:
                            saved.Push((current, matrix));
                            break;
                        case Pop when saved.Count > 0:
                            (current, matrix) = saved.Pop();
                            break;
                        case ModifyCurrentTransformationMatrix cm:
                            matrix = TransformationMatrix.FromArray(cm.Value).Multiply(matrix);
                            break;
                        case SetGraphicsStateParametersFromDictionary gs when Get<DictionaryToken>(states, gs.Name.Data) is { } state:
                            current = (Get<NumericToken>(state, "ca")?.Double ?? current.Fill, Get<NumericToken>(state, "CA")?.Double ?? current.Stroke);
                            break;
                        case InvokeNamedXObject named when depth < 8 && Get<StreamToken>(Get<DictionaryToken>(resources, "XObject"), named.Name.Data) is { } form
                            && Get<NameToken>(form.StreamDictionary, "Subtype")?.Data == "Form":
                            var inner = parser.Parse(page.Number, new MemoryInputBytes(form.Decode(DefaultFilterProvider.Instance).ToArray()), new NoLog());
                            saved.Push((current, matrix));
                            if (Get<ArrayToken>(form.StreamDictionary, "Matrix") is { Data.Count: 6 } placed)
                            {
                                matrix = TransformationMatrix.FromArray(placed.Data.Select(value => (Resolve(value) as NumericToken)?.Double ?? 0).ToArray()).Multiply(matrix);
                            }

                            var grouped = Get<DictionaryToken>(form.StreamDictionary, "Group") is not null;
                            var outer = grouped ? (current.Fill * group.Fill, current.Stroke * group.Stroke) : group;
                            if (grouped)
                            {
                                current = (1, 1);
                            }

                            Run(inner, Get<DictionaryToken>(form.StreamDictionary, "Resources") ?? resources, outer, matrix, depth + 1);
                            (current, matrix) = saved.Pop();
                            break;
                        case StrokePath or CloseAndStrokePath or FillPathEvenOddRule or FillPathNonZeroWinding or FillPathNonZeroWindingCompatibility
                            or FillPathEvenOddRuleAndStroke or FillPathNonZeroWindingAndStroke or CloseFillPathEvenOddRuleAndStroke or CloseFillPathNonZeroWindingAndStroke:
                            paints.Add(new PaintState(current.Fill * group.Fill, current.Stroke * group.Stroke,
                                Math.Sqrt(Math.Abs(matrix.A * matrix.D - matrix.B * matrix.C)), space));
                            break;
                    }
                }
            }

            Run(page.Operations, Get<DictionaryToken>(page.Dictionary, "Resources"), (1, 1), TransformationMatrix.Identity, 0);
            return paints.Count == page.Paths.Count(path => !path.IsClipping) ? paints : null;
        }
        catch (Exception)
        {
            // A form or a stream PdfPig cannot read: shapes are drawn solid.
            return null;
        }
    }

    private sealed class NoLog : ILog
    {
        public void Debug(string message) { }
        public void Debug(string message, Exception ex) { }
        public void Warn(string message) { }
        public void Error(string message) { }
        public void Error(string message, Exception ex) { }
    }

    // A formula's picture also takes lines that reach out of it (KaTeX draws a root sign as a
    // very long line that the page cuts off), cut off at its edge. Letters that stay in the
    // running text (text) are not drawn.
    private static string FigureSvg(Page page, PdfRectangle area, IReadOnlyList<PaintState>? states, bool formula = false, IReadOnlySet<Letter>? text = null)
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
        // PdfPig gives line widths before the page's scaling. Without the shapes' own scaling
        // (states), the labels show it (their size on the page over the size they were set
        // in), and a width wider than a quarter of the figure is scaled by something else (a
        // nested group) and is kept to that.
        var scales = page.Letters.Where(letter => letter.FontSize > 0 && Contains(area, letter))
            .Select(letter => letter.PointSize / letter.FontSize).Order().ToList();
        var scale = scales.Count > 0 ? scales[scales.Count / 2] : 1;
        var widest = Math.Max(Math.Min(area.Width, area.Height) / 4, 0.25);

        var svg = new StringBuilder();
        svg.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{N(area.Width * 4 / 3)}\" height=\"{N(area.Height * 4 / 3)}\" viewBox=\"0 0 {N(area.Width)} {N(area.Height)}\">\n");
        // The page is white; without it, dark lines would vanish on a dark preview.
        svg.Append("<rect width=\"100%\" height=\"100%\" fill=\"#fff\"/>\n");
        var gradients = 0;
        // A gradient (a shading pattern from one color to another along a line) becomes an
        // SVG gradient where the page has it; any other pattern is half-tone gray.
        string Paint(IColor? color, PaintState? state, out double opacity)
        {
            opacity = 1;
            if (color is ShadingPatternColor { Shading: AxialShading axial } pattern && state is not null && axial.Coords.Length == 4)
            {
                try
                {
                    var place = pattern.Matrix.Multiply(state.Base);
                    var (from, to) = (place.Transform(new PdfPoint(axial.Coords[0], axial.Coords[1])), place.Transform(new PdfPoint(axial.Coords[2], axial.Coords[3])));
                    var stops = string.Concat(Enumerable.Range(0, 11).Select(i =>
                    {
                        var t = axial.Domain[0] + (axial.Domain[1] - axial.Domain[0]) * i / 10;
                        return $"<stop offset=\"{N(i / 10.0)}\" stop-color=\"{Color(axial.ColorSpace.GetColor(axial.Eval([t])))}\"/>";
                    }));
                    var id = $"g{++gradients}";
                    svg.Append($"<linearGradient id=\"{id}\" gradientUnits=\"userSpaceOnUse\" x1=\"{X(from.X)}\" y1=\"{Y(from.Y)}\" x2=\"{X(to.X)}\" y2=\"{Y(to.Y)}\">{stops}</linearGradient>\n");
                    return $"url(#{id})";
                }
                catch (Exception)
                {
                    // A shading PdfPig cannot work out: gray as below.
                }
            }

            if (color is PatternColor)
            {
                opacity = 0.5;
            }

            return Color(color);
        }

        var pictures = page.GetImages().Where(image => Contains(area, image.BoundingBox)).ToList();
        var front = new StringBuilder();
        var painted = -1;
        foreach (var path in page.Paths)
        {
            painted += path.IsClipping ? 0 : 1;
            var state = states is not null && !path.IsClipping ? states[painted] : null;
            var (fillAlpha, strokeAlpha) = state is not null ? (state.Fill, state.Stroke) : (1, 1);
            var filled = path.IsFilled && fillAlpha > 0;
            var stroked = path.IsStroked && strokeAlpha > 0;
            if (path.IsClipping || !(filled || stroked) || path.GetBoundingRectangle() is not { } box
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

            // A gradient is defined before the shape that uses it.
            var fillPattern = 1.0;
            var strokePattern = 1.0;
            var fill = filled ? Paint(path.FillColor, state, out fillPattern) : "none";
            var stroke = stroked ? Paint(path.StrokeColor, state, out strokePattern) : "none";
            var start = svg.Length;
            svg.Append("<path d=\"").Append(d).Append('"');
            svg.Append(" fill=\"").Append(fill).Append('"');
            var fillOpacity = fillAlpha * fillPattern;
            if (filled && fillOpacity < 1)
            {
                svg.Append($" fill-opacity=\"{N(fillOpacity)}\"");
            }

            if (filled && path.FillingRule == FillingRule.EvenOdd)
            {
                svg.Append(" fill-rule=\"evenodd\"");
            }

            if (stroked)
            {
                var width = state is not null ? Math.Max(path.LineWidth * state.Scale, 0.25) : Math.Clamp(path.LineWidth * scale, 0.25, widest);
                svg.Append($" stroke=\"{stroke}\" stroke-width=\"{N(width)}\" stroke-linejoin=\"round\"");
                var strokeOpacity = strokeAlpha * strokePattern;
                if (strokeOpacity < 1)
                {
                    svg.Append($" stroke-opacity=\"{N(strokeOpacity)}\"");
                }

                if (path.LineDashPattern is { } dash && dash.Array.Count > 0 && dash.Array.Any(length => length > 0))
                {
                    svg.Append($" stroke-dasharray=\"{string.Join(' ', dash.Array.Select(length => N(length * (state?.Scale ?? scale))))}\"");
                }
            }

            svg.Append("/>\n");
            // Only a shape that holds a picture (its background or frame) goes under it.
            if (!pictures.Any(image => Contains(box, image.BoundingBox)))
            {
                front.Append(svg, start, svg.Length - start);
                svg.Length = start;
            }
        }

        foreach (var image in pictures)
        {
            var bounds = image.BoundingBox;
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

        svg.Append(front);

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
            if (IsBlank(letter) || letter.RenderingMode == TextRenderingMode.Neither || !Contains(area, letter) || text?.Contains(letter) == true)
            {
                continue;
            }

            // TeX's radical sign hangs from its baseline down past the radicand, and no font
            // here sets it so (it would stand on the baseline): it is drawn as lines in its
            // box, from the top right down to the bottom and up a little to the left.
            if (letter.Value == "√" && letter.BoundingBox.Top < letter.StartBaseLine.Y + letter.PointSize * 0.3)
            {
                Flush();
                var sign = letter.BoundingBox;
                var (width, rise, bottom) = (sign.Right - sign.Left, sign.Top - sign.Bottom, sign.Bottom);
                svg.Append($"<path d=\"M{X(sign.Left)} {Y(bottom + rise * 0.45)}L{X(sign.Left + width * 0.2)} {Y(bottom + rise * 0.55)}L{X(sign.Left + width * 0.5)} {Y(bottom)}L{X(sign.Right)} {Y(sign.Top)}\"")
                    .Append($" fill=\"none\" stroke=\"{Color(letter.Color)}\" stroke-width=\"{N(letter.PointSize * 0.06)}\" stroke-linejoin=\"round\"/>\n");
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
