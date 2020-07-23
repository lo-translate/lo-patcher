using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LoPatcher
{
    public class LanguageCatalog : ILanguageCatalog
    {
        public Version Version { get; private set; }

        public Dictionary<string, string> Catalog { get; private set; } = new Dictionary<string, string>();

        public IEnumerable<string> Errors { get; private set; } = Array.Empty<string>();

        public bool LoadTranslations(byte[] translationBytes)
        {
            if (translationBytes == null)
            {
                throw new ArgumentNullException(nameof(translationBytes));
            }

            using var stream = new MemoryStream(translationBytes);

            return LoadTranslations(stream);
        }

        public bool LoadTranslations(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using var stream = File.OpenRead(file.FullName);

            return LoadTranslations(stream);
        }

        public bool LoadTranslations(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            Catalog.Clear();
            Errors = Array.Empty<string>();

            var languageFiles = Directory.GetFiles(directory.FullName, "*.po", SearchOption.AllDirectories);
            foreach (var languageFile in languageFiles)
            {
                using var fileStream = File.OpenRead(languageFile);
                LoadFromPoCatalog(fileStream);
            }

            var versionFiles = Directory.GetFiles(directory.FullName, "VERSION", SearchOption.AllDirectories);
            foreach (var versionFile in versionFiles)
            {
                using var fileStream = File.OpenRead(versionFile);
                var version = ReadVersion(fileStream);
                if (version != null)
                {
                    Version = version;
                    break;
                }
            }

            return Catalog.Any();
        }

        public bool LoadTranslations(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Catalog.Clear();
            Errors = Array.Empty<string>();

            try
            {
                using var zip = new ZipArchive(stream);

                foreach (var entry in zip.Entries)
                {
                    if (entry.Name.Equals("VERSION", StringComparison.OrdinalIgnoreCase))
                    {
                        using var zipStream = entry.Open();

                        var version = ReadVersion(zipStream);
                        if (version != null)
                        {
                            Version = version;
                        }
                    }
                    else if (entry.Name.EndsWith(".po", StringComparison.OrdinalIgnoreCase))
                    {
                        using var zipStream = entry.Open();
                        LoadFromPoCatalog(zipStream);
                    }
                }
            }
            catch (InvalidDataException e)
            {
                Debug.WriteLine($"Failed to parse archive, {e.Message}");
                return false;
            }

            return Catalog.Any();
        }

        private static Version ReadVersion(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var version = reader.ReadLine();

            return version == null ? null : Version.Parse(version);
        }

        public string FindTranslation(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (!Catalog.ContainsKey(text))
            {
                if (text.IndexOf("\r\n", StringComparison.Ordinal) != -1)
                {
                    // If we checked with Windows EOLs try without
                    text = text.Replace("\r", "", StringComparison.Ordinal);
                }
                else
                {
                    // Otherwise try with them
                    text = Regex.Replace(text, @"\r\n|\n\r|\n|\r", "\r\n");
                }
            }

            return Catalog.ContainsKey(text) ? Catalog[text] : null;
        }

        private bool LoadFromPoCatalog(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            if (result.Success)
            {
                foreach (var key in result.Catalog)
                {
                    var original = key.Key.Id;
                    var translation = result.Catalog.GetTranslation(key.Key);

                    if (string.IsNullOrEmpty(translation))
                    {
                        continue;
                    }

                    if (Catalog.ContainsKey(original))
                    {
                        if (translation != Catalog[original])
                        {
                            Debug.WriteLine($"Duplicate with different translation: '{original}' != '{translation}'");
                        }

                        continue;
                    }

                    Catalog[key.Key.Id] = translation;
                }

                return true;
            }
            else
            {
                foreach (var error in result.Diagnostics.Where(d => d.Severity > DiagnosticSeverity.Warning)
                                           .Select(d => d.ToString()))
                {
                    Errors.Append(error);
                }

                return false;
            }
        }
    }
}