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

        public bool Patch(Stream stream)
        {
            stream.Position = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var content = reader.ReadToEnd();
            var replaced = 0;

            // We manually parse the TSV instead of using something like FileHelpers so the line endings don't get
            // changed.
            var lines = Regex.Split(content, "\t\r\n");

            for (var i = 0; i < lines.Length; i++)
            {
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

                var translation = languageCatalog.FindTranslation(japaneseText);

                if (string.IsNullOrEmpty(translation))
                {
                    continue;
                }

                replaced++;

                parts[2] = translation.Replace("\r", "", StringComparison.Ordinal);
                lines[i] = string.Join("\t", parts);
            }

            Debug.WriteLine(
                $"Replaced {replaced.ToString("N0", NumberFormatInfo.CurrentInfo)} strings in LocalizationPatch"
            );

            var rejoined = string.Join("\t\r\n", lines);

            if (rejoined.Equals(content, StringComparison.Ordinal))
            {
                return false;
            }

            stream.Position = 0;
            stream.SetLength(0);
            stream.Write(Encoding.UTF8.GetBytes(rejoined));

            return true;
        }
    }
}