using FileHelpers;

namespace LoExtractText
{
#pragma warning disable CS0649 // Field is never assigned to
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

        [DelimitedRecord("\t")]
        public class LocalizationPatchTranslation
        {
            public string Code;

            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string Korean;

            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string Japanese;

            public string English;
        }
    }
}
#pragma warning restore CS0649 // Field is never assigned to