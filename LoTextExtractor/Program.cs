using System;
using System.IO;
using LoPatcher.Unity;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: LoTextExtractor <__data>");
                return 1;
            }

            var translationFinder = new TranslationFinder();

            using var stream = File.OpenRead(args[0]);
            using var helper = new AssetsToolsBundle(File.ReadAllBytes(@"Resources\classdata.tpk"));

            if (!helper.Load(stream))
            {
                Console.WriteLine("Failed to load bundle");
                return 1;
            }

            var localizationPatchExtractor = new LocalizationPatchExtractor();
            var serializedTextExtractor = new SerializedTextExtractor();

            var entries = new List<ExtractedText>();

            var bundleAssets = helper.GetAssets();
            foreach (var asset in bundleAssets)
            {
                // We don't care about non-text assets, the Korean data files are handled with the Japanese
                if (asset.Type != "TextAsset" || asset.Name.Contains("_ko.bin", StringComparison.Ordinal))
                {
                    continue;
                }

                if (asset.Name == "LocalizationPatch")
                {
                    Console.WriteLine($"Parsing {asset.Name} as TSV");

                    entries.AddRange(localizationPatchExtractor.ExtractText(asset.GetScript()));
                }
                else if (asset.Name.EndsWith(".bin", StringComparison.Ordinal))
                {
                    if (asset.Name != "data.bin")
                    {
                        continue;
                    }

                    Console.WriteLine($"Parsing {asset.Name} as serialized data");

                    var koreanAsset = bundleAssets.FirstOrDefault(b => b.Name == asset.Name.Replace(".bin", "_ko.bin"));

                    entries.AddRange(serializedTextExtractor.ExtractText(asset.GetScript(), koreanAsset?.GetScript()));
                }
                else
                {
                    Console.WriteLine($"Unknown bundle {asset.Name}");
                }
            }

            var onlyWanted = new List<ExtractedText>();

            Console.WriteLine($"Filtering and translating list");

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Japanese) || Regex.Match(entry.Japanese, "^[\x00-\x7F]+$").Success)
                {
                    continue;
                }

                entry.English = translationFinder.FindTranslation(entry.Korean, entry.Japanese);
                onlyWanted.Add(entry);
            }

            Console.WriteLine($"Saving translation catalog");

            var catalogManager = new CatalogManager();
            catalogManager.Save(onlyWanted);

            return 0;
        }
    }
}