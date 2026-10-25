/*
  ファイル名 / File: PdfRender.cs
  概要 / Description: Finds PDF pages whose letters cannot be read and draws them as pictures with PDFium
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-26
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace MdEditor;

// A PDF keeps, for each font, a table from its glyphs to characters (ToUnicode). Without one,
// a font that numbers its glyphs in its own order (Identity-H, as many older programs embed
// Japanese and Chinese fonts) gives letters that look like text but are other characters
// ("宓宇安季" for "PDF を"), and nothing in the letters tells. A page set mostly in such fonts
// is drawn instead as a picture of itself by PDFium (the PDF renderer of Chromium and Edge,
// BSD-3-Clause; pdfium.dll from bblanchon.PDFium.Win32), which draws the glyphs, not the
// characters.
internal static partial class PdfImport
{
    // The fonts of a page (and of the forms it draws) that have no table to characters and
    // number their glyphs in their own order, by name as letters give it.
    private static HashSet<string> UnreadableFonts(PdfDocument document, Page page)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            IToken? Resolve(IToken? token) => token is IndirectReferenceToken reference ? document.Structure.GetObject(reference.Data)?.Data : token;
            T? Get<T>(DictionaryToken? dictionary, string name) where T : class, IToken =>
                dictionary is not null && dictionary.TryGet(NameToken.Create(name), out var value) ? Resolve(value) as T : null;

            void Scan(DictionaryToken? resources, int depth)
            {
                if (resources is null || depth > 8)
                {
                    return;
                }

                if (Get<DictionaryToken>(resources, "Font") is { } fonts)
                {
                    foreach (var key in fonts.Data.Keys)
                    {
                        if (Get<DictionaryToken>(fonts, key) is not { } font || Get<NameToken>(font, "Subtype")?.Data != "Type0"
                            || font.ContainsKey(NameToken.Create("ToUnicode")) || Get<NameToken>(font, "Encoding")?.Data is not ("Identity-H" or "Identity-V"))
                        {
                            continue;
                        }

                        // A font of a known character collection (Adobe-Japan1 and so on) is
                        // read through it; only "Identity" has no way back to characters.
                        var descendant = Get<ArrayToken>(font, "DescendantFonts")?.Data.Select(Resolve).OfType<DictionaryToken>().FirstOrDefault();
                        var ordering = Get<StringToken>(Get<DictionaryToken>(descendant, "CIDSystemInfo"), "Ordering")?.Data;
                        if (ordering is null or "Identity" && Get<NameToken>(font, "BaseFont")?.Data is { } name)
                        {
                            names.Add(name);
                        }
                    }
                }

                if (Get<DictionaryToken>(resources, "XObject") is { } objects)
                {
                    foreach (var key in objects.Data.Keys)
                    {
                        if (Get<StreamToken>(objects, key) is { } form && Get<NameToken>(form.StreamDictionary, "Subtype")?.Data == "Form")
                        {
                            Scan(Get<DictionaryToken>(form.StreamDictionary, "Resources"), depth + 1);
                        }
                    }
                }
            }

            Scan(Get<DictionaryToken>(page.Dictionary, "Resources"), 0);
        }
        catch (Exception)
        {
            // Fonts PdfPig cannot read: the page is read as text.
        }

        return names;
    }

    // Whether most of a page's text (three in ten letters or more, at least 20) is in such fonts.
    private static bool UnreadablePage(PdfDocument document, Page page)
    {
        var text = page.Letters.Where(letter => !IsBlank(letter)).ToList();
        if (text.Count < 20)
        {
            return false;
        }

        var fonts = UnreadableFonts(document, page);
        return fonts.Count > 0 && text.Count(letter => letter.FontName is { } name && fonts.Contains(name)) * 10 >= text.Count * 3;
    }

    // A page drawn as a PNG picture by PDFium, twice as wide as it is shown (sharp on a
    // high-resolution screen), on white; null when PDFium cannot draw it.
    private static byte[]? RenderPage(string path, int number, double width)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            lock (Pdfium.Gate)
            {
                Pdfium.Initialize();
                var data = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var document = IntPtr.Zero;
                var page = IntPtr.Zero;
                var bitmap = IntPtr.Zero;
                try
                {
                    document = Pdfium.FPDF_LoadMemDocument64(data.AddrOfPinnedObject(), (nuint)bytes.Length, null);
                    page = document == IntPtr.Zero ? IntPtr.Zero : Pdfium.FPDF_LoadPage(document, number - 1);
                    if (page == IntPtr.Zero)
                    {
                        return null;
                    }

                    var scale = width * 4 / 3 * 2 / Math.Max(Pdfium.FPDF_GetPageWidthF(page), 1);
                    var (pixelWidth, pixelHeight) = ((int)Math.Round(Pdfium.FPDF_GetPageWidthF(page) * scale), (int)Math.Round(Pdfium.FPDF_GetPageHeightF(page) * scale));
                    if (pixelWidth <= 0 || pixelHeight <= 0 || (long)pixelWidth * pixelHeight > 40_000_000)
                    {
                        return null;
                    }

                    bitmap = Pdfium.FPDFBitmap_Create(pixelWidth, pixelHeight, 0);
                    if (bitmap == IntPtr.Zero)
                    {
                        return null;
                    }

                    Pdfium.FPDFBitmap_FillRect(bitmap, 0, 0, pixelWidth, pixelHeight, 0xFFFFFFFF);
                    Pdfium.FPDF_RenderPageBitmap(bitmap, page, 0, 0, pixelWidth, pixelHeight, 0, Pdfium.Annotations);
                    var stride = Pdfium.FPDFBitmap_GetStride(bitmap);
                    var picture = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgr32, null,
                        Pdfium.FPDFBitmap_GetBuffer(bitmap), stride * pixelHeight, stride);
                    picture.Freeze();
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(picture));
                    using var png = new MemoryStream();
                    encoder.Save(png);
                    return png.ToArray();
                }
                finally
                {
                    if (bitmap != IntPtr.Zero) Pdfium.FPDFBitmap_Destroy(bitmap);
                    if (page != IntPtr.Zero) Pdfium.FPDF_ClosePage(page);
                    if (document != IntPtr.Zero) Pdfium.FPDF_CloseDocument(document);
                    data.Free();
                }
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or IOException
                                       or UnauthorizedAccessException or InvalidOperationException or ArgumentException or OutOfMemoryException)
        {
            return null;
        }
    }

    // The few PDFium calls used (fpdfview.h). PDFium is not safe to call from two threads at
    // once, so every use holds Gate.
    private static class Pdfium
    {
        public const int Annotations = 0x01;

        public static readonly object Gate = new();

        private static bool initialized;

        public static void Initialize()
        {
            if (!initialized)
            {
                FPDF_InitLibrary();
                initialized = true;
            }
        }

        [DllImport("pdfium.dll")]
        private static extern void FPDF_InitLibrary();

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDF_LoadMemDocument64(IntPtr data, nuint size, [MarshalAs(UnmanagedType.LPStr)] string? password);

        [DllImport("pdfium.dll")]
        public static extern void FPDF_CloseDocument(IntPtr document);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDF_LoadPage(IntPtr document, int index);

        [DllImport("pdfium.dll")]
        public static extern void FPDF_ClosePage(IntPtr page);

        [DllImport("pdfium.dll")]
        public static extern float FPDF_GetPageWidthF(IntPtr page);

        [DllImport("pdfium.dll")]
        public static extern float FPDF_GetPageHeightF(IntPtr page);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDFBitmap_Create(int width, int height, int alpha);

        [DllImport("pdfium.dll")]
        public static extern int FPDFBitmap_FillRect(IntPtr bitmap, int left, int top, int width, int height, uint color);

        [DllImport("pdfium.dll")]
        public static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int x, int y, int width, int height, int rotate, int flags);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDFBitmap_GetBuffer(IntPtr bitmap);

        [DllImport("pdfium.dll")]
        public static extern int FPDFBitmap_GetStride(IntPtr bitmap);

        [DllImport("pdfium.dll")]
        public static extern void FPDFBitmap_Destroy(IntPtr bitmap);
    }
}
