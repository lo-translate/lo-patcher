using Karambolo.PO;
using System;
using System.IO;
using System.Text;

namespace LoTextExtractor
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: LoTextExtractor <data.bin[.dat]> <data_ko.bin[.dat]> <LocalizationPatch.tsv[.dat]>");
                return 1;
            }

            if (File.Exists("LoText.po"))
            {
                File.Delete("LoText.po");
            }

            using var stream = File.OpenRead("LoText.template.po");
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            var translationFinder = new TranslationFinder();

            translationFinder.LoadKnownTextFromTsv("Resources/binfilepatcher-142_new.tsv");
            translationFinder.LoadKnownTextFromTsv("Resources/binfilepatcher-library.tsv");
            translationFinder.LoadKnownTextFromTsv("Resources/localefilepatcher-library.tsv");

            translationFinder.LoadKnownTextFromCsv("Resources/skilltool-known-effects.csv");
            translationFinder.LoadKnownTextFromCsv("Resources/skilltool-known-skills.csv");
            translationFinder.LoadKnownTextFromCsv("Resources/skilltool-skill-trans.csv");
            translationFinder.LoadKnownTextFromCsv("Resources/skilltool-strings.csv");

            translationFinder.LoadKnownRegexFromTsv("Resources/binfilepatcher-regex.tsv");

            if (File.Exists("Resources/LoText.po"))
            {
                translationFinder.LoadKnownTextFromTranslation("Resources/LoText.po");
            }

            var catalogManager = new CatalogManager(result.Catalog);

            new SerializedTextExtractor(translationFinder, catalogManager).ExtractToCatalog(args[0], args[1]);
            new LocalizationPatchExtractor(translationFinder, catalogManager).ExtractToCatalog(args[2]);

            var generator = new POGenerator(new POGeneratorSettings());
            using var outStream = File.Open("LoText.po", FileMode.OpenOrCreate);
            using var writer = new StreamWriter(outStream, Encoding.UTF8);
            generator.Generate(writer, result.Catalog);

            return 0;
        }
    }
}