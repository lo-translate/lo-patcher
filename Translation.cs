using FileHelpers;

namespace LoExtractText
{
    internal class Translation
    {
#pragma warning disable CS0649 // Field is never assigned to
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Korean;

        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string English;
#pragma warning restore CS0649 // Field is never assigned to

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