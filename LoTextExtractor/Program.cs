using LoPatcher.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
                if (!asset.Type.Equals("TextAsset", StringComparison.OrdinalIgnoreCase) ||
                    asset.Name.EndsWith("_ko.bin", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"Skipped bundle '{asset.Name}' ({asset.Type})");
                    continue;
                }

                var count = entries.Count();

                if (asset.Name.Equals("LocalizationPatch", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write($"Processing {asset.Name} ");

                    entries.AddRange(localizationPatchExtractor.ExtractText(asset.GetScript()));
                }
                else if (asset.Name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write($"Processing {asset.Name} ");

                    var koreanAsset = bundleAssets.FirstOrDefault(b => b.Name == asset.Name.Replace(".bin", "_ko.bin"));

                    entries.AddRange(serializedTextExtractor.ExtractText(asset.GetScript(), koreanAsset?.GetScript()));
                }
                else
                {
                    Debug.WriteLine($"Skipped bundle '{asset.Name}' ({asset.Type})");
                    continue;
                }

                var newCount = entries.Count();
                Console.WriteLine($"{newCount - count:N0} entries extracted");
            }

            Console.WriteLine($"Filtering and translating extracted text");

            var groups = new Dictionary<string, List<ExtractedText>>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Japanese) || Regex.Match(entry.Japanese, "^[\x00-\x7F]+$").Success)
                {
                    continue;
                }

                entry.English = translationFinder.FindTranslation(entry.Korean, entry.Japanese);
                entry.Comment = translationFinder.FindComment(entry.Korean, entry.Japanese);

                var parts = Regex.Split(entry.Source, "[^a-zA-Z_]");
                if (parts.Length < 1)
                {
                    Debug.WriteLine($"Unknown source format {entry.Source}");
                    continue;
                }

                var group = parts[0];

                // At time of creating this app the text extracted text from _Client tables are the same as the table
                // with _Client. Putting them in the same group will ensure only one copy of them is stored (whichever
                // ends up first after sorting)
                if (group.EndsWith("_Client"))
                {
                    group = Regex.Replace(group, "_Client$", "");
                }

                // Split up DialogScript into 2 groups, one for events and one for normal stages.
                if (group == "DialogScript" && !Regex.IsMatch(entry.Source, "Ch[0-9]+Stage"))
                {
                    group = "DialogScriptEvent";
                }

                if (!groups.ContainsKey(group))
                {
                    groups[group] = new List<ExtractedText>();
                }

                groups[group].Add(entry);
            }

            Console.WriteLine($"Saving translation catalogs");

            var outputPath = new DirectoryInfo(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName),
                "Extracted"
            ));

            if (outputPath.Exists)
            {
                foreach (var file in Directory.GetFiles(outputPath.FullName, "*.po"))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(outputPath.FullName);
            }

            var warnings = new List<string>();
            var versionDate = DateTime.Now;

            File.WriteAllText(Path.Join(outputPath.FullName, "VERSION"), string.Format("{0:yyyy.MM.dd.00}", versionDate));

            foreach (var group in groups)
            {
                var file = Path.Join(outputPath.FullName, $"{group.Key}.po");

                Console.WriteLine($" - Saving {file}");

                var catalogManager = new CatalogManager();
                var catalogWarnings = catalogManager.SaveTo(file, versionDate, group.Value);

                foreach (var warning in catalogWarnings)
                {
                    warnings.Add($"{group.Key}: {warning}");
                }
            }

            var warningFile = Path.Join(outputPath.FullName, "WARNINGS");
            if (warnings.Any())
            {
                File.WriteAllLines(Path.Join(outputPath.FullName, "WARNINGS"), warnings);
            }
            else
            {
                if (File.Exists(warningFile))
                {
                    File.Delete(warningFile);
                }
            }

            return 0;
        }
    }
}