using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    internal class LocalizationPatchExtractor
    {
        public IEnumerable<ExtractedText> ExtractText(byte[] asset)
        {
            var foundText = new List<ExtractedText>();

            var text = Encoding.UTF8.GetString(asset);

            // We parse the TSV manually instead of using FileHelpers so the line endings match their original version.
            // This doesn't matter in the end since line endings get converted when loaded with Karambolo.PO and need
            // normalization when searching but at least this keeps the rendered line ending in the Korean text comment
            // right.
            var lines = Regex.Split(text, "\t\r\n");

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

                foundText.Add(new ExtractedText()
                {
                    Korean = RemoveSurroundingQuotes(parts[1]),
                    Japanese = RemoveSurroundingQuotes(parts[2]),
                    Source = "LocalizationPatch",
                    SourceLine = int.Parse(parts[0], System.Globalization.NumberStyles.Integer, NumberFormatInfo.CurrentInfo),
                });
            }

            return foundText;
        }

        private string RemoveSurroundingQuotes(string text)
        {
            var quotedMatch = Regex.Match(text, "^\"(.*)\"$", RegexOptions.Singleline);
            if (quotedMatch.Success)
            {
                return Regex.Replace(text, "^\"(.*)\"$", "$1", RegexOptions.Singleline);
            }

            return text;
        }
    }
}