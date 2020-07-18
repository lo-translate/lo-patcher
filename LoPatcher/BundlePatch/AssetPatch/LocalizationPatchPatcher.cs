using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Karambolo.PO;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LoPatcher.BundlePatch.AssetPatch
{
    public class LocalizationPatchPatcher : IAssetPatcher
    {
        private readonly POCatalog languageCatalog;

        public LocalizationPatchPatcher(POCatalog languageCatalog)
        {
            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));
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

            var replacedScript = ReplaceTextInTsv(script.AsString());
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

        private string ReplaceTextInTsv(string script)
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
