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
            foreach (var foreignText in new[] { japaneseText, koreanText })
            {
                if (foreignText == null)
                {
                    continue;
                }

                var translation = knownText.ContainsKey(foreignText) ? knownText[foreignText] : null;
                if (string.IsNullOrEmpty(translation) && foreignText.Contains('\n'))
                {
                    // If we can't find a known translation try again using Windows style new lines. This is needed
                    // due to Karambolo.PO using it while parsing the PO file.
                    var normalizedForeignText = Regex.Replace(foreignText, @"\r\n|\n\r|\n|\r", "\r\n");
                    if (!normalizedForeignText.Equals(foreignText, System.StringComparison.Ordinal))
                    {
                        translation = knownText.ContainsKey(normalizedForeignText) ? knownText[normalizedForeignText] : null;
                    }
                }

                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }

                foreach (var regexKvp in knownRegex)
                {
                    var match = regexKvp.Key.Match(foreignText);
                    if (match.Success)
                    {
                        var replaced = regexKvp.Value.Replace("`", match.Groups[1].Value);

                        // Make sure we replaced something
                        if (replaced != match.Groups[1].Value)
                        {
                            return replaced;
                        }
                    }
                }

            }

            return null;
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

                    japaneseText = japaneseText.Replace("`n", "\n");
                    translation = translation.Replace("`n", "\n");

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
            
            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n")));
        }

        public void LoadKnownTextFromCsv(string input)
        {
            var engine = new FileHelperEngine<Translation.CsvTranslation>();

            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n")));
        }

        private void AddTranslations(Translation[] translations)
        {
            foreach (var translation in translations)
            {
                var englishText = translation.English;
                var koreanText = translation.Korean;

                englishText = englishText.Replace("…", "...");

                if (knownText.ContainsKey(koreanText))
                {
                    if (!englishText.Equals(knownText[koreanText], System.StringComparison.Ordinal))
                    {
                        Debug.WriteLine($"Duplicate translation: '{knownText[koreanText]}' != '{englishText}'");
                    }
                    continue;
                }

                knownText.Add(koreanText, englishText);
            }
        }

        public void LoadKnownRegexFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();
            var lines = engine.ReadFile(input);

            foreach (var translation in lines)
            {
                knownRegex.Add(new Regex($"^\\s+?{translation.Korean}\\s+?$"), translation.English);
            }
        }
    }
}