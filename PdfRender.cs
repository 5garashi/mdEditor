/*
  ファイル名 / File: PdfRender.cs
  概要 / Description: Finds PDF pages whose letters cannot be read and draws them as pictures with Windows' own PDF renderer
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-26
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MdEditor;

// A PDF keeps, for each font, a table from its glyphs to characters (ToUnicode). Without one,
// a font that numbers its glyphs in its own order (Identity-H, as many older programs embed
// Japanese and Chinese fonts) gives letters that look like text but are other characters
// ("宓宇安季" for "PDF を"), and nothing in the letters tells. A page set mostly in such fonts
// is drawn instead as a picture of itself by Windows' PDF renderer (Windows.Data.Pdf), which
// draws the glyphs, not the characters.
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

    // A page drawn as a PNG picture by Windows, twice as wide as it is shown (sharp on a
    // high-resolution screen); null when Windows cannot draw it.
    private static byte[]? RenderPage(string path, int number, double width)
    {
        try
        {
            // Away from the window's thread: the renderer's work completes on its own threads.
            return Task.Run(async () =>
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path));
                var document = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
                using var page = document.GetPage((uint)(number - 1));
                using var stream = new InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(stream, new Windows.Data.Pdf.PdfPageRenderOptions { DestinationWidth = (uint)Math.Round(width * 4 / 3 * 2) });
                var bytes = new byte[stream.Size];
                using var reader = new DataReader(stream.GetInputStreamAt(0));
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes);
                return bytes;
            }).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
