using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LoTextExtractor
{
    internal class CatalogManager
    {
        public IEnumerable<string> Save(List<ExtractedText> entries, string filename)
        {
            var translationCatalog = CreateCatalog();
            var sanityChecker = new ExtractedTextSanityChecker();

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

            var warnings = new List<string>();

            foreach (var entry in entries)
            {
                // There are a lot of this string in various unrelated parts of the structure, seems to be a generic
                // error string. We don't want it anywhere but the error string table.
                if (entry.Japanese == "2-エラーが発生しました。" && !entry.Source.StartsWith("ServerErrString"))
                {
                    continue;
                }

                var key = new POKey(entry.Japanese);
                if (translationCatalog.Contains(key))
                {
                    continue;
                }

                // Since PO catalogs can't contain duplicate entries any after this will be skipped. We pull the Korean
                // text from the duplicate entries if they have it and we don't.
                //
                // This is needed for the PCStory PO file to contain both. The PCStory table only contains Japanese
                // while the PCStory_Client contains both (but doesn't have contextual key names so we prefer PCStory).
                // For example, for a string in PCStory_Client the path is PCStory_Client[1][0].DgStartTriggerString1,
                // in PCStory the path is PCStory[Story_3P_ConstantiaS2_1_3P_ConstantiaS2_1].DgStartTriggerString1.
                if (string.IsNullOrEmpty(entry.Korean))
                {
                    var otherEntryWithKorean = entries.Where(e =>
                    {
                        return e != entry
                            && e.Japanese.Equals(entry.Japanese, StringComparison.Ordinal)
                            && !string.IsNullOrEmpty(e.Korean);
                    }).FirstOrDefault();

                    if (otherEntryWithKorean != null)
                    {
                        entry.Korean = otherEntryWithKorean.Korean;
                    }
                }

                warnings.AddRange(sanityChecker.GetWarnings(entry));

                AddToCatalog(translationCatalog, entry);
            }

            WriteCatalog(translationCatalog, filename);

            return warnings;
        }

        private void AddToCatalog(POCatalog catalog, ExtractedText entry)
        {
            var key = new POKey(entry.Japanese);

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