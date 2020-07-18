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

            if (knownText.ContainsKey(koreanText))
            {
                enText = knownText[koreanText];
            }
            else if (knownText.ContainsKey(japaneseText))
            {
                enText = knownText[japaneseText];
            }
            else
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
                        if (translation != knownText[japaneseText])
                        {
                            Debug.WriteLine($"Duplicate translation: {knownText[japaneseText]} != {translation}");
                        }
                        continue;
                    }

                    knownText.Add(japaneseText, translation);
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
                    if (knownText[translation.Korean] != translation.English)
                    {
                        Debug.WriteLine($"Duplicate translation: {knownText[translation.Korean]} != {translation.English}");
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