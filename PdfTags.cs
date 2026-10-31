/*
  ファイル名 / File: PdfTags.cs
  概要 / Description: Reads the tags of a tagged PDF: reading order, headings, list items, tables, and what is not the text
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-26
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Tokens;

namespace MdEditor;

// A tagged PDF (Word, Acrobat, LibreOffice, and browsers can write one) says what its text
// is: a tree of headings, paragraphs, list items, and tables in reading order (the
// structure tree), each pointing at the marked pieces of the pages' drawing that hold its
// letters, and the page furniture (running titles, page numbers) is marked as not the text.
// Where a page's tags hold its text, they decide the reading order, the paragraphs, the
// heading levels, the list items, and tables without cell borders; the rest (figures,
// math, ruled tables, code) is found as on an untagged page. The tags are read with
// PdfPig's tokens, which keep the tree, and Page.GetMarkedContents, which joins the pieces
// to their letters.
internal static partial class PdfImport
{
    // Where a letter stands in the tree: its place in reading order, the paragraph-like
    // element it is in (Block), the heading level of a heading (HeadingTag for "H" without a
    // level, 0 outside headings), the list item it is in, and the table, row, and cell.
    private sealed record Tag(int Order, int Block, int Heading, int ListItem, int Table, int Row, int Cell);

    private const int HeadingTag = 9;

    // The tree's pieces by page and number (MCID), whether it has headings at all, and the
    // place of each list item's first piece of text (its label, a bullet drawn as a shape,
    // can come first with no letters).
    private sealed record Structure(Dictionary<(int Page, int Mcid), Tag> Pieces, HashSet<(int Page, int Mcid)> Furniture, bool HasHeadings, Dictionary<int, int> ListStarts);

    // A page's letters with their tags, and the letters marked as not the text.
    private sealed class PageTags(Dictionary<Letter, Tag> tags, HashSet<Letter> furniture, Dictionary<int, int> listStarts)
    {
        private readonly Dictionary<(int, double), Tag> byPlace = tags.GroupBy(pair => Key(pair.Key)).ToDictionary(group => group.Key, group => group.First().Value);

        // A letter set again (a superscript put back on its line) is found by its place.
        public Tag? Of(Letter letter) => tags.TryGetValue(letter, out var tag) || byPlace.TryGetValue(Key(letter), out tag) ? tag : null;

        public bool IsFurniture(Letter letter) => furniture.Contains(letter);

        // A line holding the first text of a list item starts it (the rest of an item
        // carried over to the next page does not).
        public bool StartsListItem(Tag tag) => tag.ListItem >= 0 && listStarts.TryGetValue(tag.ListItem, out var start) && tag.Order <= start;

        private static (int, double) Key(Letter letter) => (letter.TextSequence, Math.Round(letter.StartBaseLine.X, 2));
    }

    // Elements that are part of a paragraph when they are in one: a list item's label and
    // body belong to the item, a link or a span to the line it is in. Outside a paragraph
    // (a browser writes a <div> as NonStruct too) each is a block of its own. A block of
    // code is one too: a browser tags each colored word in it.
    private static readonly HashSet<string> ParagraphRoles = ["P", "H", "H1", "H2", "H3", "H4", "H5", "H6", "Title", "LI", "TD", "TH", "TOCI", "Caption", "Lbl", "LBody", "Code", "BlockCode"];

    private static readonly HashSet<string> InlineRoles = ["Span", "Quote", "Note", "Reference", "BibEntry", "Code", "Link", "Annot", "Ruby", "RB", "RT", "RP",
        "Warichu", "WT", "WP", "Lbl", "LBody", "Em", "Strong", "Sub", "Formula", "NonStruct", "Private"];

    // Null for a PDF without tags, or with tags that cannot be read.
    private static Structure? ReadStructure(PdfDocument document)
    {
        try
        {
            IToken? Resolve(IToken? token) => token is IndirectReferenceToken reference ? document.Structure.GetObject(reference.Data)?.Data : token;
            T? Get<T>(DictionaryToken? dictionary, string name) where T : class, IToken =>
                dictionary is not null && dictionary.TryGet(NameToken.Create(name), out var value) ? Resolve(value) as T : null;

            var catalog = document.Structure.Catalog.CatalogDictionary;
            if (Get<DictionaryToken>(catalog, "StructTreeRoot") is not { } root)
            {
                return null;
            }

            // The pages in order, from the page tree, to know which page a piece is on.
            var pageOf = new Dictionary<IndirectReference, int>();
            void Pages(IToken? node, int depth)
            {
                if (depth > 64 || node is not IndirectReferenceToken reference || Resolve(node) is not DictionaryToken dictionary)
                {
                    return;
                }

                if (Get<NameToken>(dictionary, "Type")?.Data == "Page")
                {
                    pageOf[reference.Data] = pageOf.Count + 1;
                }
                else if (Get<ArrayToken>(dictionary, "Kids") is { } kids)
                {
                    foreach (var kid in kids.Data) Pages(kid, depth + 1);
                }
            }

            catalog.TryGet(NameToken.Create("Pages"), out var pages);
            Pages(pages, 0);

            var roles = Get<DictionaryToken>(root, "RoleMap");
            // A role of the PDF's own maps to a standard one, perhaps through others.
            string Role(string name)
            {
                for (var i = 0; i < 8 && roles is not null && Get<NameToken>(roles, name) is { } mapped && mapped.Data != name; i++)
                {
                    name = mapped.Data;
                }

                return name;
            }

            var pieces = new Dictionary<(int Page, int Mcid), Tag>();
            var furniture = new HashSet<(int Page, int Mcid)>();
            var visited = new HashSet<IndirectReference>();
            var order = 0;
            var elements = 0;
            var hasHeadings = false;
            // Rows counted in each table, and cells in each row.
            var rows = new Dictionary<int, int>();
            var cells = new Dictionary<(int, int), int>();
            var listStarts = new Dictionary<int, int>();
            void Walk(IToken? token, int page, Tag context, bool artifact, bool label, bool paragraph, int depth)
            {
                if (depth > 256 || token is IndirectReferenceToken reference && !visited.Add(reference.Data))
                {
                    return;
                }

                switch (Resolve(token))
                {
                    case NumericToken mcid:
                        Piece(page, mcid.Int);
                        return;
                    case ArrayToken array:
                        foreach (var item in array.Data) Walk(item, page, context, artifact, label, paragraph, depth + 1);
                        return;
                    case DictionaryToken dictionary:
                        if (dictionary.TryGet(NameToken.Create("Pg"), out var pg) && pg is IndirectReferenceToken pageReference && pageOf.TryGetValue(pageReference.Data, out var number))
                        {
                            page = number;
                        }

                        var type = Get<NameToken>(dictionary, "Type")?.Data;
                        if (type == "MCR")
                        {
                            if (Get<NumericToken>(dictionary, "MCID") is { } id) Piece(page, id.Int);
                            return;
                        }

                        if (type == "OBJR")
                        {
                            return;
                        }

                        var name = Get<NameToken>(dictionary, "S")?.Data ?? string.Empty;
                        var role = Role(name);
                        var element = ++elements;
                        var inner = context;
                        if (!paragraph || !InlineRoles.Contains(role))
                        {
                            inner = inner with { Block = element };
                        }

                        if (role.Length == 2 && role[0] == 'H' && char.IsAsciiDigit(role[1]) || role is "H" or "Title")
                        {
                            inner = inner with { Heading = role is "H" ? HeadingTag : role is "Title" ? 1 : Math.Clamp(role[1] - '0', 1, 6) };
                            hasHeadings = true;
                        }
                        else if (role == "LI")
                        {
                            inner = inner with { ListItem = element };
                        }
                        else if (role == "Table")
                        {
                            inner = inner with { Table = element, Row = -1, Cell = -1 };
                            rows[element] = 0;
                        }
                        else if (role == "TR" && context.Table >= 0)
                        {
                            inner = inner with { Row = rows[context.Table]++, Cell = -1 };
                            cells[(context.Table, inner.Row)] = 0;
                        }
                        else if (role is "TD" or "TH" && context.Table >= 0 && context.Row >= 0)
                        {
                            inner = inner with { Cell = cells[(context.Table, context.Row)]++ };
                        }

                        dictionary.TryGet(NameToken.Create("K"), out var kids);
                        Walk(kids, page, inner, artifact || name == "Artifact" || role == "Artifact", label || role == "Lbl", paragraph || ParagraphRoles.Contains(role), depth + 1);
                        return;
                }

                void Piece(int at, int mcid)
                {
                    if (artifact)
                    {
                        furniture.Add((at, mcid));
                        return;
                    }

                    if (context.ListItem >= 0 && !label)
                    {
                        listStarts.TryAdd(context.ListItem, order);
                    }

                    pieces[(at, mcid)] = context with { Order = order++ };
                }
            }

            root.TryGet(NameToken.Create("K"), out var top);
            Walk(top, 0, new Tag(0, -1, 0, -1, -1, -1, -1), false, false, false, 0);
            return pieces.Count == 0 ? null : new Structure(pieces, furniture, hasHeadings, listStarts);
        }
        catch (Exception)
        {
            // Tags PdfPig cannot read: the page is read as an untagged one.
            return null;
        }
    }

    // The tags of one page, or null when they do not hold its text (most of its letters
    // are in no element: tags written for some pages only, or not kept up to date).
    private static PageTags? ReadPageTags(Structure structure, Page page)
    {
        var tags = new Dictionary<Letter, Tag>(ReferenceEqualityComparer.Instance);
        var furniture = new HashSet<Letter>(ReferenceEqualityComparer.Instance);
        // A piece inside another without a number of its own (a browser's span around one
        // letter) belongs to the one around it.
        void Add(MarkedContentElement element, int mcid, bool artifact)
        {
            if (element.MarkedContentIdentifier >= 0)
            {
                mcid = element.MarkedContentIdentifier;
            }

            artifact |= element.IsArtifact || structure.Furniture.Contains((page.Number, mcid));
            foreach (var letter in element.Letters)
            {
                if (artifact) furniture.Add(letter);
                else if (mcid >= 0 && structure.Pieces.TryGetValue((page.Number, mcid), out var tag)) tags[letter] = tag;
            }

            foreach (var child in element.Children)
            {
                Add(child, mcid, artifact);
            }
        }

        try
        {
            foreach (var element in page.GetMarkedContents())
            {
                Add(element, -1, false);
            }
        }
        catch (Exception)
        {
            return null;
        }

        var text = page.Letters.Count(letter => !IsBlank(letter) && !furniture.Contains(letter));
        var tagged = tags.Keys.Count(letter => !IsBlank(letter));
        return text > 0 && tagged >= text * Tune.TagCoverage ? new PageTags(tags, furniture, structure.ListStarts) : null;
    }

    // The tag most of the letters have.
    private static Tag? TagOf(PageTags tags, IEnumerable<Letter> letters) =>
        letters.Where(letter => !IsBlank(letter)).Select(tags.Of).OfType<Tag>().GroupBy(tag => tag.Block)
            .OrderByDescending(group => group.Count()).Select(group => group.MinBy(tag => tag.Order)).FirstOrDefault();

    // A table in the tags with no cell borders on the page, as Markdown: its first row is
    // the header. One of a single row or column is how the page is laid out, not a table.
    private static List<(PdfRectangle Box, string Markdown, List<Letter> Letters)> TaggedTables(PageTags tags, IEnumerable<Letter> letters)
    {
        var tables = new List<(PdfRectangle, string, List<Letter>)>();
        foreach (var table in letters.Select(letter => (letter, tag: tags.Of(letter))).Where(pair => pair.tag is { Table: >= 0, Row: >= 0, Cell: >= 0 })
                     .GroupBy(pair => pair.tag!.Table))
        {
            var cells = table.GroupBy(pair => (pair.tag!.Row, pair.tag.Cell)).ToDictionary(group => group.Key, group => group.Select(pair => pair.letter).ToList());
            var rows = cells.Keys.Select(key => key.Row).Distinct().Order().ToList();
            var columns = cells.Keys.Max(key => key.Cell) + 1;
            if (rows.Count < 2 || columns < 2)
            {
                continue;
            }

            string Cell(List<Letter> cell)
            {
                var words = NearestNeighbourWordExtractor.Instance.GetWords(cell).ToList();
                var lines = words.GroupBy(word => Math.Round(word.BoundingBox.Bottom / Math.Max(word.BoundingBox.Height, 1) * 2))
                    .OrderByDescending(group => group.Key).Select(group => ReadLine(group.OrderBy(word => word.BoundingBox.Left).ToList()).Text);
                return JoinLines(lines).Replace("|", "\\|").Trim();
            }

            var markdown = new System.Text.StringBuilder();
            for (var r = 0; r < rows.Count; r++)
            {
                markdown.Append('|');
                for (var c = 0; c < columns; c++)
                {
                    markdown.Append(' ').Append(cells.TryGetValue((rows[r], c), out var cell) ? Cell(cell) : string.Empty).Append(" |");
                }

                markdown.Append('\n');
                if (r == 0)
                {
                    markdown.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", columns))).Append('\n');
                }
            }

            var all = table.Select(pair => pair.letter).ToList();
            var box = new PdfRectangle(all.Min(letter => letter.BoundingBox.Left), all.Min(letter => letter.BoundingBox.Bottom),
                all.Max(letter => letter.BoundingBox.Right), all.Max(letter => letter.BoundingBox.Top));
            tables.Add((box, markdown.ToString().TrimEnd('\n'), all));
        }

        return tables;
    }
}
