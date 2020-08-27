using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    internal class ExtractedTextSanityChecker
    {
        public IEnumerable<string> GetWarnings(ExtractedText entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var warnings = new List<string>();

            if (string.IsNullOrEmpty(entry.English))
            {
                return warnings;
            }

            var cleanText = entry.English?.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

            var numbersInJapanese = Regex.Matches(entry.Japanese, @"(\d+)");
            //var numbersInKorean = Regex.Matches(entry.Korean, @"(\d+)");
            var numbersInEnglish = Regex.Matches(entry.English, @"(\d+)");

            if (numbersInJapanese.Count < 1 && numbersInEnglish.Count < 1)
            {
                return warnings;
            }

            var japaneseValues = numbersInJapanese.Select(m => m.Captures.First().Value);
            var englishValues = numbersInEnglish.Select(m => m.Captures.First().Value);
            var source = entry.Source;
            if (entry.SourceLine > 0)
            {
                source += $":{entry.SourceLine}";
            }

            foreach (var match in englishValues.Except(japaneseValues))
            {
                if (japaneseValues.Contains(ConvertAscii(match)))
                {
                    continue;
                }
                warnings.Add($"{source}: Number in English text but not Japanese: '{match}'");
            }

            foreach (var match in japaneseValues.Except(englishValues))
            {
                if (englishValues.Contains(ConvertUnicode(match)))
                {
                    continue;
                }

                if (ConvertUnicode(match) == "1" && entry.English.Contains("once", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                warnings.Add($"{source}: Number in Japanese text but not English: '{match}'");
            }

            return warnings;
        }

        private readonly Dictionary<string, string> unicodeCharacters = new Dictionary<string, string>()
        {
            { "１", "1" },
            { "２", "2" },
            { "３", "3" },
            { "４", "4" },
            { "５", "5" },
            { "６", "6" },
            { "７", "7" },
            { "８", "8" },
            { "９", "9" },
            { "０", "0" },
        };

        private string ConvertUnicode(string input)
        {
            foreach(var kvp in unicodeCharacters)
            {
                input = input.Replace(kvp.Key, kvp.Value, StringComparison.Ordinal);
            }

            return input;
        }

        private string ConvertAscii(string input)
        {
            foreach (var kvp in unicodeCharacters)
            {
                input = input.Replace(kvp.Value, kvp.Key, StringComparison.Ordinal);
            }

            return input;
        }
    }
}