using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LoPatcher
{
    public class LanguageCatalog : ILanguageCatalog
    {
        private POCatalog catalog;

        public Version Version { get; private set; }

        public IEnumerable<string> Errors { get; private set; }

        public bool LoadTranslations(Stream stream)
        {
            Errors = Array.Empty<string>();

            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            if (result.Success)
            {
                Version = ParseVersion(result.Catalog);

                catalog = result.Catalog;

                return true;
            }
            else
            {
                Errors = result.Diagnostics.Where(d => d.Severity > DiagnosticSeverity.Warning)
                                           .Select(d => d.ToString());

                return false;
            }
        }

        public bool LoadTranslations(string translationFile)
        {
            using var stream = File.OpenRead(translationFile);

            return LoadTranslations(stream);
        }

        public bool LoadTranslations(byte[] translationBytes)
        {
            using var stream = new MemoryStream(translationBytes);

            return LoadTranslations(stream);
        }

        public string FindTranslation(string text)
        {
            var key = new POKey(text);
            var translation = catalog.GetTranslation(key);

            if (!string.IsNullOrEmpty(translation))
            {
                return translation;
            }

            // If we were not found try again with \r\n as the line ending (this is needed due to Karambolo.PO
            // using it in the parsed strings when parsing the PO file)
            var normalizedText = Regex.Replace(text, @"\r\n|\n\r|\n|\r", "\r\n");

            key = new POKey(normalizedText);
            return catalog.GetTranslation(key);
        }

        private static Version ParseVersion(POCatalog catalog)
        {
            var versionHeader = catalog.Headers.FirstOrDefault(
                h => h.Key.Equals("Project-Id-Version", StringComparison.Ordinal)
            );

            if (versionHeader.Value != null)
            {
                return Version.Parse(versionHeader.Value);
            }

            return null;
        }
    }
}
