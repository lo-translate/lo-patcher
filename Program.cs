using FileHelpers;
using Karambolo.PO;
using LoExtractText.Lo.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace LoExtractText
{
    internal class Program
    {
        private static Dictionary<string, string> knownText = new Dictionary<string, string>();
        private static Dictionary<Regex, string> knownRegex = new Dictionary<Regex, string>();
        private static POCatalog catalog;

        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: LoExtractText <data.bin> <data_ko.bin>");
                return 1;
            }

            if (File.Exists("data.bin.po"))
            {
                File.Delete("data.bin.po");
            }

            using var stream = File.OpenRead("data.bin-template.po");
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            using var fileJp = File.OpenRead(args[0]);
            using var fileKo = File.OpenRead(args[1]);

            LoadKnownTextFromTsv("Resources/binfilepatcher-142_new.tsv", knownText);
            LoadKnownTextFromTsv("Resources/binfilepatcher-library.tsv", knownText);
            LoadKnownTextFromTsv("Resources/localefilepatcher-library.tsv", knownText);

            LoadKnownTextFromCsv("Resources/skilltool-known-effects.csv", knownText);
            LoadKnownTextFromCsv("Resources/skilltool-known-skills.csv", knownText);
            LoadKnownTextFromCsv("Resources/skilltool-skill-trans.csv", knownText);
            LoadKnownTextFromCsv("Resources/skilltool-strings.csv", knownText);

            LoadKnownRegexFromTsv("Resources/binfilepatcher-regex.tsv", knownRegex);

            if (File.Exists("Resources/data.bin.po"))
            {
                LoadKnownTextFromTranslation("Resources/DataBin.po", knownText);
            }

            if (File.Exists("Resources/LocalizationPatch.po"))
            {
                LoadKnownTextFromTranslation("Resources/LocalizationPatch.po", knownText);
            }

            catalog = result.Catalog;

            AddStringsToCatalog(fileJp, fileKo);

            var generator = new POGenerator(new POGeneratorSettings());
            using var outStream = File.Open("data.bin.po", FileMode.OpenOrCreate);
            using var writer = new StreamWriter(outStream, Encoding.UTF8);
            generator.Generate(writer, result.Catalog);

            return 0;
        }

        private static void LoadKnownTextFromTranslation(string input, Dictionary<string, string> knownText)
        {
            using var stream = File.OpenRead(input);
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            if (result.Success)
            {
                foreach (var key in result.Catalog.Keys)
                {
                    var jpText = key.Id;
                    var translation = result.Catalog.GetTranslation(key);
                    if (string.IsNullOrEmpty(translation))
                    {
                        continue;
                    }

                    if (knownText.ContainsKey(jpText))
                    {
                        if (translation != knownText[jpText])
                        {
                            Debug.WriteLine($"Duplicate translation: {knownText[jpText]} != {translation}");
                        }
                        continue;
                    }

                    knownText.Add(jpText, translation);
                }
            }
        }

        private static void LoadKnownTextFromTsv(string input, Dictionary<string, string> knownText)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();

            AddTranslations(engine.ReadFile(input), knownText);
        }

        private static void LoadKnownTextFromCsv(string input, Dictionary<string, string> knownText)
        {
            var engine = new FileHelperEngine<Translation.CsvTranslation>()
            {
            };

            AddTranslations(engine.ReadFile(input), knownText);
        }

        private static void AddTranslations(Translation[] translations, Dictionary<string, string> knownText)
        {
            foreach (var translation in translations)
            {
                if (knownText.ContainsKey(translation.Korean))
                {
                    if (knownText[translation.Korean] != translation.English)
                    {
                        Debug.WriteLine($"Duplicate translation: {knownText[translation.Korean]} != {translation.English}");
                    }
                    continue;
                }

                knownText.Add(translation.Korean, translation.English);
            }
        }

        private static void LoadKnownRegexFromTsv(string input, Dictionary<Regex, string> knownRegex)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();
            var lines = engine.ReadFile(input);

            foreach (var translation in lines)
            {
                knownRegex.Add(new Regex(translation.Korean), translation.English);
            }
        }

        private static void AddStringsToCatalog(Stream fileJp, Stream fileKo)
        {
            var formatter = new BinaryFormatter() { Binder = new BinaryFormatterBinder() };

            var deserializedJp = formatter.Deserialize(fileJp) as LastOnTable;
            var deserializedKo = formatter.Deserialize(fileKo) as LastOnTable;

            foreach (var storyKvp in deserializedJp._Table_PCStory_Client)
            {
                var storyId = storyKvp.Key;
                var stages = storyKvp.Value;
                var index = 0;

                foreach (var stage in stages)
                {
                    var koStage = deserializedKo._Table_PCStory_Client[storyId][index];

                    ProcessObject(stage, koStage, $"LastOnTable._Table_PCStory_Client[{storyId}][{index}]");

                    index++;
                }
            }

            foreach (var kvp in deserializedJp._Table_BuffEffect_Client)
            {
                ProcessObject(
                    kvp.Value,
                    deserializedKo._Table_BuffEffect_Client[kvp.Key],
                    $"LastOnTable._Table_BuffEffect_Client[{kvp.Key}]"
                );
            }

            foreach (var field in deserializedJp._TableManager.GetType().GetFields())
            {
                if (field.Name == "_Table_Forbidden")
                {
                    continue;
                }

                if (field.FieldType.Name != "Dictionary`2")
                {
                    continue;
                }

                var fieldInstance = field.GetValue(deserializedJp._TableManager) as IDictionary;
                var fieldInstanceKr = field.GetValue(deserializedKo._TableManager) as IDictionary;

                foreach (DictionaryEntry kvp in fieldInstance)
                {
                    ProcessObject(
                        kvp.Value,
                        fieldInstanceKr[kvp.Key],
                        $"LastOnTable._TableManager.{field.Name}[{kvp.Key}]"
                    );
                }
            }
        }

        private static void ProcessObject(object jpObject, object koObject, string reference)
        {
            foreach (var property in jpObject.GetType().GetProperties())
            {
                if (property.MemberType != MemberTypes.Property && property.MemberType != MemberTypes.Field)
                {
                    continue;
                }

                var type = property.PropertyType;
                if (type.Name != "String")
                {
                    continue;
                }

                var jpText = (string)property.GetValue(jpObject, null);
                if (string.IsNullOrWhiteSpace(jpText))
                {
                    continue;
                }

                // Make sure we contain non-english text
                if (System.Text.RegularExpressions.Regex.Match(jpText, "^[A-Za-z0-9.,-_=$\\(\\) \t\r\n/'\"]+$").Success)
                {
                    continue;
                }

                var koText = koObject == null
                    ? ""
                    : (string)koObject.GetType().GetProperty(property.Name).GetValue(koObject, null);

                if (koText == jpText)
                {
                    Debug.WriteLine($"JP == KO {jpText}");
                    continue;
                }

                var enText = "";

                if (knownText.ContainsKey(koText))
                {
                    enText = knownText[koText];
                }
                else if (knownText.ContainsKey(jpText))
                {
                    enText = knownText[jpText];
                }
                else
                {
                    foreach (var regexKvp in knownRegex)
                    {
                        var match = regexKvp.Key.Match(koText);
                        if (match.Success)
                        {
                            var replaced = regexKvp.Value.Replace("`", match.Groups[1].Value);
                            
                            // Make sure we replaced something
                            if (replaced != match.Groups[1].Value)
                            {
                                enText = replaced;
                            }
                        }
                    }
                }

                // The comment we save the Korean text in can't contain new lines
                koText = koText.Replace("\r", "\\r").Replace("\n", "\\n");

                if (property.Name == "Char_Name")
                {
                    var officialTranslation = (string)jpObject.GetType().GetProperty("Char_Name_EngDisp").GetValue(jpObject, null);
                    if (!string.IsNullOrEmpty(enText))
                    {
                        Debug.WriteLine($"Duplicate translation: {officialTranslation} != {enText}");
                    }
                    enText = officialTranslation;
                }

                AddToCatalog(jpText, koText, enText, $"{reference}.{property.Name}", catalog);
            }
        }

        private static void AddToCatalog(string jpText, string koText, string enText, string sourceComment, POCatalog catalog)
        {
            var key = new POKey(jpText);
            if (catalog.Contains(key))
            {
                return;
            }

            var sourceReference = new POSourceReference[]
            {
                new POSourceReference(sourceComment, 0)
            };

            var newEntry = new POSingularEntry(key)
            {
                Comments = new List<POComment>
                {
                    new POReferenceComment { References = sourceReference },
                },
            };

            if (!string.IsNullOrEmpty(enText))
            {
                newEntry.Translation = enText;
            }

            if (!string.IsNullOrEmpty(koText))
            {
                newEntry.Comments.Add(new POTranslatorComment()
                {
                    Text = $"Korean Text: {koText}"
                });
            }

            catalog.Add(newEntry);
        }
    }
}