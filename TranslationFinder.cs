using FileHelpers;
using Karambolo.PO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    public class TranslationFinder
    {
        private readonly Dictionary<string, string> knownText = new Dictionary<string, string>();
        private readonly Dictionary<Regex, string> knownRegex = new Dictionary<Regex, string>();

        public string FindTranslation(string koreanText, string japaneseText)
        {
            string enText = null;

            if (koreanText != null && knownText.ContainsKey(koreanText))
            {
                enText = knownText[koreanText];
            }
            else if (japaneseText != null && knownText.ContainsKey(japaneseText))
            {
                enText = knownText[japaneseText];
            }
            else if (koreanText != null)
            {
                foreach (var regexKvp in knownRegex)
                {
                    var match = regexKvp.Key.Match(koreanText);
                    if (match.Success)
                    {
                        var replaced = regexKvp.Value.Replace("`", match.Groups[1].Value);

                        // Make sure we replaced something
                        if (replaced != match.Groups[1].Value)
                        {
                            enText = replaced;
                        }
                    }
                }
            }

            return enText;
        }

        public void LoadKnownTextFromTranslation(string input)
        {
            using var stream = File.OpenRead(input);
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            if (result.Success)
            {
                foreach (var key in result.Catalog.Keys)
                {
                    var japaneseText = key.Id;
                    var translation = result.Catalog.GetTranslation(key);
                    if (string.IsNullOrEmpty(translation))
                    {
                        continue;
                    }

                    if (knownText.ContainsKey(japaneseText))
                    {
                        if (!translation.Equals(knownText[japaneseText], System.StringComparison.Ordinal))
                        {
                            Debug.WriteLine($"Duplicate translation: '{translation}' != '{knownText[japaneseText]}'");
                        }

                        // We intentionally don't prevent the translation from being overwritten under the assumption
                        // the translation file contains the most up to date translations.
                    }

                    knownText[japaneseText] = translation;
                }
            }
        }

        public void LoadKnownTextFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();

            AddTranslations(engine.ReadFile(input));
        }

        public void LoadKnownTextFromCsv(string input)
        {
            var engine = new FileHelperEngine<Translation.CsvTranslation>()
            {
            };

            AddTranslations(engine.ReadFile(input));
        }

        private void AddTranslations(Translation[] translations)
        {
            foreach (var translation in translations)
            {
                if (knownText.ContainsKey(translation.Korean))
                {
                    if (!translation.English.Equals(knownText[translation.Korean], System.StringComparison.Ordinal))
                    {
                        Debug.WriteLine($"Duplicate translation: '{knownText[translation.Korean]}' != '{translation.English}'");
                    }
                    continue;
                }

                knownText.Add(translation.Korean, translation.English);
            }
        }

        public void LoadKnownRegexFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();
            var lines = engine.ReadFile(input);

            foreach (var translation in lines)
            {
                knownRegex.Add(new Regex(translation.Korean), translation.English);
            }
        }
    }
}