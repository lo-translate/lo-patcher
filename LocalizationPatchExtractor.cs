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
            var engine = new FileHelperEngine<Translation.LocalizationPatchTranslation>();
            var result = engine.ReadString(reader.ReadToEnd());

            var knownStrings = catalogManager.GetCatalogCount();
            var knownTranslations = catalogManager.GetTranslationCount();

            foreach (var translation in result)
            {
                if (translation.Code == "code" || string.IsNullOrEmpty(translation.Japanese))
                {
                    continue;
                }

                // Make sure we contain non-ASCII text
                if (Regex.Match(translation.Japanese, "^[\x00-\x7F]+$").Success)
                {
                    //Debug.WriteLine($"Japanese text ASCII: {translation.Japanese}");
                    continue;
                }

                var englishText = translationFinder.FindTranslation(translation.Korean, translation.Japanese);

                if (string.IsNullOrEmpty(englishText) && translation.Korean == translation.Japanese)
                {
                    Debug.WriteLine($"Japanese text matches Korean text: {translation.Korean}");
                }

                catalogManager.AddToCatalog(translation.Japanese, translation.Korean, englishText, $"LocalizationPatch-{translation.Code}", int.Parse(translation.Code));
            }

            stream?.Dispose();

            var newKnownStrings = catalogManager.GetCatalogCount();
            if (newKnownStrings > knownStrings)
            {
                var newKnownTranslations = catalogManager.GetTranslationCount();
                Console.WriteLine($"Extracted {(newKnownStrings - knownStrings).ToString("N0")} strings with " +
                    $" {(newKnownTranslations - knownTranslations).ToString("N0")} translations from " +
                    $"LocalizationPatch");
            }
        }
    }
}