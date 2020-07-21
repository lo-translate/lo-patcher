using Karambolo.PO;
using System;
using System.Collections.Generic;

namespace LoTextExtractor
{
    internal class CatalogManager
    {
        private readonly POCatalog catalog;

        public CatalogManager(POCatalog catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public int GetCatalogCount()
        {
            return catalog.Count;
        }

        public void AddToCatalog(string japaneseText, string koreanText, string englishText, string source, int sourceLine)
        {
            var key = new POKey(japaneseText);
            if (catalog.Contains(key))
            {
                return;
            }

            var newEntry = new POSingularEntry(key)
            {
                Comments = new List<POComment>
                {
                    new POReferenceComment
                    {
                        References = new POSourceReference[]
                        {
                            new POSourceReference(source, sourceLine)
                        }
                    }
                },
            };

            if (!string.IsNullOrEmpty(englishText))
            {
                newEntry.Translation = englishText.Trim(' ');
            }

            // We render a partial source in a comment as well so it is visible in poedit and gives immediate context
            // (the source reference added above can be viewed on right click but that is annoying).
            var comment = source.IndexOf("LocalizationPatch") == -1
                ? source.Replace("LastOnTable.", "")
                        .Replace("_TableManager.", "")
                        .Replace("_Table_", "")
                : "";

            // If we have Korean text add it to the comment as well. This gives us multiple strings to throw at a
            // machine translator to hopefully get better context.
            if (!string.IsNullOrEmpty(koreanText) && koreanText != japaneseText)
            {
                if (comment.Length > 0)
                {
                    comment += " - ";
                }
                
                // The comment we save the Korean text in can't contain new lines, tabs are just for visual convenience
                comment += "Korean Text: '" + koreanText.Replace("\r", "\\r")
                                                       .Replace("\n", "\\n")
                                                       .Replace("\t", "\\t") + "'";

            }

            if (!string.IsNullOrEmpty(comment))
            {
                newEntry.Comments.Add(new POTranslatorComment()
                {
                    Text = comment
                });
            }

            catalog.Add(newEntry);
        }
    }
}