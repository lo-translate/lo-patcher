using FileHelpers;

#pragma warning disable CS0649 // Field is never assigned to

namespace LoTextExtractor
{
    internal class Translation
    {
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Korean;

        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string English;

        [DelimitedRecord("\t")]
        public class TsvTranslation : Translation
        {
        }

        [DelimitedRecord(",")]
        public class CsvTranslation : Translation
        {
        }
    }
}

#pragma warning restore CS0649 // Field is never assigned to