using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LoTextExtractor
{
    internal class CatalogManager
    {
        public void Save(List<ExtractedText> entries)
        {
            var translationCatalog = CreateCatalog();

            entries.Sort((x, y) =>
            {
                if (x.Source == y.Source)
                {
                    return x.SourceLine.CompareTo(y.SourceLine);
                }

                if (x.Source == "LocalizationPatch")
                {
                    return 1;
                }

                if (y.Source == "LocalizationPatch")
                {
                    return -1;
                }

                return new NaturalStringComparer().Compare(x.Source, y.Source);
            });

            foreach (var entry in entries)
            {
                AddToCatalog(translationCatalog, entry);
            }

            WriteCatalog(translationCatalog, "LoTranslation.Extracted.po");
        }

        private void AddToCatalog(POCatalog catalog, ExtractedText entry)
        {
            var key = new POKey(entry.Japanese);
            if (catalog.Contains(key))
            {
                return;
            }

            var newEntry = new POSingularEntry(key)
            {
                Comments = new List<POComment>
                {
                    new POReferenceComment
                    {
                        References = new POSourceReference[]
                        {
                            new POSourceReference($"{entry.Source}", entry.SourceLine)
                        }
                    }
                },
            };

            if (!string.IsNullOrEmpty(entry.English))
            {
                newEntry.Translation = entry.English.Trim(' ');
            }

            var comment = "";

            // TODO Read comments from loaded PO files?

            // If we have Korean text add it to the comment as well. This gives us multiple strings to throw at a
            // machine translator to hopefully get better context.
            if (!string.IsNullOrEmpty(entry.Korean) && entry.Korean != entry.Japanese)
            {
                if (comment.Length > 0)
                {
                    comment += "; ";
                }

                // The comment we save the Korean text in can't contain new lines, tabs are just for visual convenience
                comment += "Korean Text: '" + entry.Korean.Replace("\r", "\\r", StringComparison.Ordinal)
                                                       .Replace("\n", "\\n", StringComparison.Ordinal)
                                                       .Replace("\t", "\\t", StringComparison.Ordinal) + "'";
            }

            if (!string.IsNullOrEmpty(comment))
            {
                newEntry.Comments.Add(new POTranslatorComment()
                {
                    Text = comment
                });
            }

            catalog.Add(newEntry);
        }

        private static POCatalog CreateCatalog()
        {
            using var templateStream = File.OpenRead("Resources/LoTranslation.extracted.template.po");
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(templateStream, Encoding.UTF8);
            return result.Catalog;
        }

        private static void WriteCatalog(POCatalog catalog, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var generator = new POGenerator(new POGeneratorSettings());
            using var outStream = File.Open(outputFile, FileMode.OpenOrCreate);
            using var writer = new StreamWriter(outStream, Encoding.UTF8);

            // We only store the date when generating
            catalog.Headers["PO-Revision-Date"] = string.Format("{0:yyyy-MM-dd 12:00-0400}", DateTime.Now);

            generator.Generate(writer, catalog);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        private sealed class NaturalStringComparer : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                return NativeMethods.StrCmpLogicalW(a, b);
            }
        }

    }
}