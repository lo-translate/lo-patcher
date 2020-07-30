using System;
using System.Collections.Generic;

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
            var cleanText = entry.English?.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

            for (var index = 0; index < 99; index++)
            {
                var placeholder = "{" + index.ToString() + "}";
                if (!IsEntrySane(entry, placeholder))
                {
                    warnings.Add($"Possible placeholder issue: '{placeholder}' in '{cleanText}' at {entry.Source}");
                }

                var level = "Lv." + index.ToString();
                if (!IsEntrySane(entry, level))
                {
                    warnings.Add($"Possible level issue: '{level}' in '{cleanText}' at {entry.Source}");
                }
            }

            return warnings;
        }

        private bool IsEntrySane(
            ExtractedText entry, string textToCheck, StringComparison comparison = StringComparison.OrdinalIgnoreCase
        )
        {
            if (string.IsNullOrEmpty(entry.English))
            {
                // We only care about issues with the English string
                return true;
            }

            var inKorean = entry.Korean != null && entry.Korean.Contains(textToCheck, comparison);
            var inJapanese = entry.Japanese != null && entry.Japanese.Contains(textToCheck, comparison);
            var inEnglish = entry.English != null && entry.English.Contains(textToCheck, comparison);

            if (
                ((inKorean || inJapanese) && !inEnglish) ||
                ((!inKorean || !inJapanese) && inEnglish)
            )
            {
                return false;
            }

            return true;
        }
    }
}