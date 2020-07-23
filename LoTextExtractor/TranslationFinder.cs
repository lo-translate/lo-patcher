using FileHelpers;
using LoPatcher;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    public class TranslationFinder
    {
        private readonly LanguageCatalog languageCatalog = new LanguageCatalog() { LoadComments = true };
        private readonly Dictionary<string, string> knownText = new Dictionary<string, string>();
        private readonly Dictionary<Regex, string> knownRegex = new Dictionary<Regex, string>();

        public TranslationFinder()
        {
            LoadKnownTextFromTsv("Resources/binfilepatcher-142_new.tsv");
            LoadKnownTextFromTsv("Resources/binfilepatcher-library.tsv");
            LoadKnownTextFromTsv("Resources/localefilepatcher-library.tsv");

            LoadKnownTextFromCsv("Resources/skilltool-known-effects.csv");
            LoadKnownTextFromCsv("Resources/skilltool-known-skills.csv");
            LoadKnownTextFromCsv("Resources/skilltool-skill-trans.csv");
            LoadKnownTextFromCsv("Resources/skilltool-strings.csv");

            LoadKnownRegexFromTsv("Resources/binfilepatcher-regex.tsv");

            var localFile = "LoTranslation.zip";
            var localFolder = @"..\..\..\..\..\LoTranslation";

            if (File.Exists(localFile))
            {
                languageCatalog.LoadTranslations(new FileInfo(localFile));
            }
            else if (Directory.Exists(localFolder))
            {
                languageCatalog.LoadTranslations(new DirectoryInfo(localFolder));
            }

            if (languageCatalog.Catalog.Any())
            {
                LoadKnownTextFromDictionary(languageCatalog.Catalog);
            }
        }

        public string FindComment(string koreanText, string japaneseText)
        {
            foreach (var foreignText in new[] { japaneseText, koreanText })
            {
                if (foreignText == null)
                {
                    continue;
                }

                if (languageCatalog.Comments.ContainsKey(foreignText))
                {
                    return languageCatalog.Comments[foreignText];
                }
            }

            return null;
        }

        public string FindTranslation(string koreanText, string japaneseText)
        {
            foreach (var foreignText in new[] { japaneseText, koreanText })
            {
                if (foreignText == null)
                {
                    continue;
                }

                var translation = knownText.ContainsKey(foreignText) ? knownText[foreignText] : null;
                if (string.IsNullOrEmpty(translation) && foreignText.Contains('\n', System.StringComparison.Ordinal))
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
                        var replaced = regexKvp.Value.Replace("`", match.Groups[1].Value, System.StringComparison.Ordinal);

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

        public void LoadKnownTextFromDictionary(Dictionary<string, string> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var japaneseText = kvp.Key;
                var translation = kvp.Value;
                if (string.IsNullOrEmpty(translation))
                {
                    continue;
                }

                japaneseText = japaneseText.Replace("`n", "\n", System.StringComparison.Ordinal);
                translation = translation.Replace("`n", "\n", System.StringComparison.Ordinal);

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

        public void LoadKnownTextFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();

            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n", System.StringComparison.Ordinal)));
        }

        public void LoadKnownTextFromCsv(string input)
        {
            var engine = new FileHelperEngine<Translation.CsvTranslation>();

            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n", System.StringComparison.Ordinal)));
        }

        private void AddTranslations(Translation[] translations)
        {
            foreach (var translation in translations)
            {
                var englishText = translation.English;
                var koreanText = translation.Korean;

                englishText = englishText.Replace("…", "...", System.StringComparison.Ordinal);

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