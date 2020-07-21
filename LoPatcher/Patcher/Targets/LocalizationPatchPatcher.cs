using LoPatcher.Patcher.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LoPatcher.Patcher.Targets
{
    internal class LocalizationPatchPatcher : IPatchTarget
    {
        private readonly ILanguageCatalog languageCatalog;

        public LocalizationPatchPatcher(ILanguageCatalog languageCatalog)
        {
            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));
        }

        public bool CanPatch(Stream stream)
        {
            stream.Position = 0;

            // The LocalizationPatch should start with code\t[language-code]\t...
            var expectedText = "code\t";
            var buffer = new byte[expectedText.Length];
            var readBytes = stream.Read(buffer, 0, buffer.Length);
            if (readBytes < expectedText.Length)
            {
                return false;
            }

            if (!expectedText.Equals(Encoding.UTF8.GetString(buffer), StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public bool Patch(Stream stream, IProgress<PatchProgress> progressReporter)
        {
            stream.Position = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var content = reader.ReadToEnd();
            var replaced = 0;

            // We manually parse the TSV instead of using something like FileHelpers so the line endings don't get
            // changed. I have not tested whether it would cause an issue, I just checked whether changing nothing with
            // FileHelpers caused a data change, it did so I stuck with manual parsing.
            // The tab we use in the match is technically there because there is a third, unused column.  If they ever
            // add data to it we'll have to switch to a real parser that can handle multi-line entries.
            var lines = Regex.Split(content, "\t\r\n");

            progressReporter.Report(new PatchProgress() { IncreaseTotal = lines.Length });

            for (var i = 0; i < lines.Length; i++)
            {
                progressReporter.Report(new PatchProgress() { IncreaseCurrent = 1 });

                var parts = Regex.Split(lines[i], "\t");
                if (parts.Length < 3)
                {
                    throw new PatchFailedException(
                        $"Failed to parse LocalizationPatch TSV, found {parts.Length} columns, expected 3"
                    );
                }

                // Skip the TSV header
                if (parts[0].Equals("code", StringComparison.Ordinal))
                {
                    continue;
                }

                // The string ID is 0, Korean is 1, Japanese is 2
                var japaneseText = parts[2];

                var wasQuoted = false;
                var quotedMatch = Regex.Match(japaneseText, "^\"(.*)\"$", RegexOptions.Singleline);

                if (quotedMatch.Success)
                {
                    wasQuoted = true;
                    japaneseText = Regex.Replace(japaneseText, "^\"(.*)\"$", "$1", RegexOptions.Singleline);
                }

                var translation = languageCatalog.FindTranslation(japaneseText);

                if (string.IsNullOrEmpty(translation))
                {
                    continue;
                }

                if (wasQuoted)
                {
                    translation = $"\"{translation}\"";
                }
                
                replaced++;

                parts[2] = translation.Replace("\r", "", StringComparison.Ordinal);
                lines[i] = string.Join("\t", parts);
            }

            Debug.WriteLine(
                $"Replaced {replaced.ToString("N0", NumberFormatInfo.CurrentInfo)} strings out of " +
                $"{(lines.Length - 1).ToString("N0", NumberFormatInfo.CurrentInfo)} in LocalizationPatch"
            );

            if (replaced < 1)
            {
                return false;
            }

            var rejoined = string.Join("\t\r\n", lines);

            stream.Position = 0;
            stream.SetLength(0);
            stream.Write(Encoding.UTF8.GetBytes(rejoined));

            return true;
        }
    }
}