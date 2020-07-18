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

        public void AddToCatalog(string japaneseText, string koreanText, string englishText, string sourceComment, int sourceLine)
        {
            var key = new POKey(japaneseText);
            if (catalog.Contains(key))
            {
                return;
            }

            var sourceReference = new POSourceReference[]
            {
                new POSourceReference(sourceComment, sourceLine)
            };

            var newEntry = new POSingularEntry(key)
            {
                Comments = new List<POComment>
                {
                    new POReferenceComment { References = sourceReference },
                },
            };

            if (!string.IsNullOrEmpty(englishText))
            {
                newEntry.Translation = englishText;
            }

            if (!string.IsNullOrEmpty(koreanText) && koreanText != japaneseText)
            {
                // The comment we save the Korean text in can't contain new lines
                koreanText = koreanText.Replace("\r", "\\r")
                                       .Replace("\n", "\\n")
                                       .Replace("\t", "\\t");

                newEntry.Comments.Add(new POTranslatorComment()
                {
                    Text = $"Korean Text: {koreanText}"
                });
            }

            catalog.Add(newEntry);
        }
    }
}