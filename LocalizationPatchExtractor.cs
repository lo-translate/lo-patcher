using FileHelpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    internal class LocalizationPatchExtractor
    {
        private readonly TranslationFinder translationFinder;
        private readonly CatalogManager catalogManager;

        public LocalizationPatchExtractor(TranslationFinder translationFinder, CatalogManager catalogManager)
        {
            this.translationFinder = translationFinder ?? throw new ArgumentNullException(nameof(translationFinder));
            this.catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
        }

        public void ExtractToCatalog(string localizationPatchFile)
        {
            Stream stream = File.OpenRead(localizationPatchFile);
            if (localizationPatchFile.EndsWith(".dat"))
            {
                var asset = new RawExtractedAsset(stream);
                var content = asset.Content;

                stream.Dispose();
                stream = new MemoryStream(content);
            }

            using var reader = new StreamReader(stream);

            // We parse the TSV manually instead of using FileHelpers so the line endings match their original version.
            // This doesn't matter in the end since line endings get converted when loaded with Karambolo.PO and need
            // normalization when searching but at least this keeps the rendered line ending in the Korean text comment
            // right.
            var lines = Regex.Split(reader.ReadToEnd(), "\t\r\n");

            stream.Dispose();

            var knownStrings = catalogManager.GetCatalogCount();
            var knownTranslations = catalogManager.GetTranslationCount();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = Regex.Split(line, "\t");
                if (parts.Length < 3)
                {
                    throw new Exception(
                        $"Failed to parse LocalizationPatch TSV, found {parts.Length} columns, expected 3"
                    );
                }

                // Skip the TSV header
                if (parts[0].Equals("code", StringComparison.Ordinal))
                {
                    continue;
                }

                // The string ID is 0, Korean is 1, Japanese is 2
                var code = parts[0];
                var koreanText = parts[1];
                var japaneseText = parts[2];

                if (string.IsNullOrEmpty(japaneseText))
                {
                    continue;
                }

                // Make sure we contain non-ASCII text
                if (Regex.Match(japaneseText, "^[\x00-\x7F]+$").Success)
                {
                    //Debug.WriteLine($"Japanese text ASCII: {japaneseText}");
                    continue;
                }

                // Remove quotes if they surround the string. We don't store quotes in the translation file, this will
                // need to be taken into account when patching the file.
                ReplaceSurroundingQuotes(ref japaneseText);
                ReplaceSurroundingQuotes(ref koreanText);

                var englishText = translationFinder.FindTranslation(koreanText, japaneseText);

                if (string.IsNullOrEmpty(englishText) && koreanText == japaneseText)
                {
                    Debug.WriteLine($"Japanese text matches Korean text: {koreanText}");
                }

                catalogManager.AddToCatalog(japaneseText, koreanText, englishText, $"LocalizationPatch-{code}", int.Parse(code));
            }

            var newKnownStrings = catalogManager.GetCatalogCount();
            if (newKnownStrings > knownStrings)
            {
                var newKnownTranslations = catalogManager.GetTranslationCount();
                Console.WriteLine($"Extracted {(newKnownStrings - knownStrings).ToString("N0")} strings with " +
                    $" {(newKnownTranslations - knownTranslations).ToString("N0")} translations from " +
                    $"LocalizationPatch");
            }
        }

        private bool ReplaceSurroundingQuotes(ref string text)
        {
            var quotedMatch = Regex.Match(text, "^\"(.*)\"$", RegexOptions.Singleline);
            if (quotedMatch.Success)
            {
                text = Regex.Replace(text, "^\"(.*)\"$", "$1", RegexOptions.Singleline);
                return true;
            }

            return false;
        }
    }
}