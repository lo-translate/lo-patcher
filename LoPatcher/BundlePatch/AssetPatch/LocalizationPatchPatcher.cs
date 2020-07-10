using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace LoPatcher.BundlePatch.AssetPatch
{
    public class LocalizationPatchPatcher : IAssetPatcher
    {
        private readonly POCatalog languageCatalog;
        private readonly Dictionary<string, string> koreanToEnglishDictionary = new Dictionary<string, string>();

        public LocalizationPatchPatcher(POCatalog languageCatalog)
        {
            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));

            if (File.Exists("library.txt"))
            {
                LoadKoreanToJapaneseFromTsv("library.txt");
            }

            if (File.Exists("library.csv"))
            {
                LoadKoreanToJapaneseFromTsv("library.csv");
            }
        }

        private void LoadKoreanToJapaneseFromTsv(string filename)
        {
            foreach (var line in File.ReadLines(filename))
            {
                var parts = Regex.Split(line, "\t");
                if (parts.Length != 2)
                {
                    throw new PatchFailedException(
                        $"Failed to parse KOR->JPN TSV, found {parts.Length} columns, expected 2"
                    );
                }

                var koreanText = parts[0].Replace("`n", "\n", StringComparison.Ordinal);
                var englishText = parts[1].Replace("`n", "\n", StringComparison.Ordinal);

                if (koreanToEnglishDictionary.ContainsKey(koreanText))
                {
                    if (!koreanToEnglishDictionary[koreanText].Equals(englishText, StringComparison.Ordinal))
                    {
                        Trace.WriteLine("Duplicate Korean text with different translation");
                        Trace.WriteLine($" - {koreanText}");
                        Trace.WriteLine($" - {englishText}");
                        Trace.WriteLine($" - {koreanToEnglishDictionary[koreanText]}");
                    }
                    continue;
                }

                koreanToEnglishDictionary[koreanText] = englishText;
            }
        }

        public bool CanPatch(string type, string assetName)
        {
            return type != null && type.Equals("TextAsset", StringComparison.Ordinal)
                && assetName != null && assetName.Equals("LocalizationPatch", StringComparison.Ordinal);
        }

        public AssetsReplacer Patch(AssetsManager assetsManager, AssetsFileInstance assetsFile, AssetFileInfoEx info)
        {
            if (assetsManager == null)
            {
                throw new ArgumentNullException(nameof(assetsManager));
            }

            if (assetsFile == null)
            {
                throw new ArgumentNullException(nameof(assetsFile));
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var baseField = assetsManager.GetATI(assetsFile.file, info).GetBaseField();

            var name = baseField.Get("m_Name")?.GetValue().AsString();
            var script = baseField.Get("m_Script")?.GetValue();

            if (string.IsNullOrEmpty(name) || script == null)
            {
                return null;
            }

            var replacedScript = ReplaceTextInTsv(script.AsString(), assetsFile.name);
            if (replacedScript.Equals(script.AsString(), StringComparison.Ordinal))
            {
                return null;
            }

            script.Set(replacedScript);

            var replaced = baseField.WriteToByteArray();

            return new AssetsReplacerFromMemory(
                info.curFileTypeOrIndex, info.index, (int)info.curFileType, 0xFFFF, replaced
            );
        }

        private string ReplaceTextInTsv(string script, string name)
        {
            var lines = Regex.Split(script, "\t\r\n");

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
                var code = parts[0];
                var koreanText = parts[1];
                var japaneseText = parts[2];

                var key = new POKey(japaneseText);
                var translation = languageCatalog.GetTranslation(key);

                if (translation == null)
                {
                    // If we were not found try again with \r\n as the line ending (this is needed due to Karambolo.PO
                    // using it in the parsed strings)
                    var normalizedJapaneseText = Regex.Replace(japaneseText, @"\r\n|\n\r|\n|\r", "\r\n");
                    key = new POKey(normalizedJapaneseText);
                    translation = languageCatalog.GetTranslation(key);
                }

                if (translation == null)
                {
                    var newKey = new POKey(japaneseText);
                    var codeAsInt = int.Parse(code, NumberStyles.Integer, CultureInfo.CreateSpecificCulture("en-US"));

                    var sourceReference = new POSourceReference[]
                    {
                        new POSourceReference($"{name}-LocalizationPatch", codeAsInt)
                    };

                    var newEntry = new POSingularEntry(newKey)
                    {
                        Comments = new List<POComment>
                        {
                            new POReferenceComment { References = sourceReference },
                        },
                    };

                    languageCatalog.Add(newEntry);

                    if (!koreanToEnglishDictionary.ContainsKey(koreanText))
                    {
                        var cleanKoreanText = koreanText.Replace("\r", "\\r", StringComparison.Ordinal)
                            .Replace("\n", "\\n", StringComparison.Ordinal)
                            .Replace("\t", "\\t", StringComparison.Ordinal);

                        newEntry.Comments.Add(new POTranslatorComment()
                        {
                            Text = $"Korean Text: {cleanKoreanText}"
                        });
                        newEntry.Translation = "";
                        continue;
                    }

                    translation = koreanToEnglishDictionary[koreanText];
                    newEntry.Translation = translation;
                }

                if (string.IsNullOrEmpty(translation))
                {
                    Trace.WriteLine($"Missing translation for '{japaneseText}'");
                    continue;
                }

                parts[2] = translation.Replace("\r", "", StringComparison.Ordinal);
                lines[i] = string.Join("\t", parts);
            }

            return string.Join("\t\r\n", lines);
        }
    }
}
