using FileHelpers;

namespace LoExtractText
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