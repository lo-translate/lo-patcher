﻿using LoTextExtractor.Lo.Generated;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace LoTextExtractor
{
    internal class SerializedTextExtractor
    {
        private readonly TranslationFinder translationFinder;
        private readonly CatalogManager catalogManager;

        public SerializedTextExtractor(TranslationFinder translationFinder, CatalogManager catalogManager)
        {
            this.translationFinder = translationFinder ?? throw new ArgumentNullException(nameof(translationFinder));
            this.catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
        }

        public void ExtractToCatalog(string japaneseFile, string koreanFile)
        {
            var formatter = new BinaryFormatter() { Binder = new BinaryFormatterBinder() };

            Console.WriteLine("Loading Japanese data");

            Stream japaneseStream = File.OpenRead(japaneseFile);
            if (japaneseFile.EndsWith(".dat"))
            {
                var asset = new RawExtractedAsset(japaneseStream);
                var content = asset.Content;

                japaneseStream.Dispose();
                japaneseStream = new MemoryStream(content);
            }

            var japaneseRoot = formatter.Deserialize(japaneseStream) as LastOnTable;

            Console.WriteLine("Loading Korean data");

            Stream koreanStream = File.OpenRead(koreanFile);
            if (koreanFile.EndsWith(".dat"))
            {
                var asset = new RawExtractedAsset(koreanStream);
                var content = asset.Content;

                koreanStream.Dispose();
                koreanStream = new MemoryStream(content);
            }

            var koreanRoot = formatter.Deserialize(koreanStream) as LastOnTable;
            var knownStrings = catalogManager.GetCatalogCount();

            foreach (var storyKvp in japaneseRoot._Table_PCStory_Client)
            {
                var storyId = storyKvp.Key;
                var stages = storyKvp.Value;
                var index = 0;

                foreach (var japaneseStage in stages)
                {
                    var koreanStage = koreanRoot._Table_PCStory_Client[storyId][index];

                    ProcessObject(
                        japaneseStage,
                        koreanStage,
                        $"LastOnTable._Table_PCStory_Client[{storyId}][{index}]"
                    );

                    index++;
                }
            }

            var newKnownStrings = catalogManager.GetCatalogCount();
            if (newKnownStrings > knownStrings)
            {
                Console.WriteLine($"Extracted {(newKnownStrings - knownStrings).ToString("N0")} strings from LastOnTable._Table_PCStory_Client");
            }

            knownStrings = catalogManager.GetCatalogCount();

            foreach (var kvp in japaneseRoot._Table_BuffEffect_Client)
            {
                ProcessObject(
                    kvp.Value,
                    koreanRoot._Table_BuffEffect_Client[kvp.Key],
                    $"LastOnTable._Table_BuffEffect_Client[{kvp.Key}]"
                );
            }

            newKnownStrings = catalogManager.GetCatalogCount();
            if (newKnownStrings > knownStrings)
            {
                Console.WriteLine($"Extracted {(newKnownStrings - knownStrings).ToString("N0")} strings from LastOnTable._Table_BuffEffect_Client");
            }

            var excludedFields = new[]
            {
                "_Table_Forbidden", // Forbidden words, nothing useful for translation
            };

            // Loop through the dictionaries in TableManager and process each of them
            foreach (var field in japaneseRoot._TableManager.GetType().GetFields())
            {
                knownStrings = catalogManager.GetCatalogCount();

                if (excludedFields.Contains(field.Name))
                {
                    continue;
                }

                if (field.FieldType.Name != "Dictionary`2")
                {
                    continue;
                }

                var japaneseFieldInstance = field.GetValue(japaneseRoot._TableManager) as IDictionary;
                var koreanFieldInstance = field.GetValue(koreanRoot._TableManager) as IDictionary;

                foreach (DictionaryEntry kvp in japaneseFieldInstance)
                {
                    ProcessObject(
                        kvp.Value,
                        koreanFieldInstance[kvp.Key],
                        $"LastOnTable._TableManager.{field.Name}[{kvp.Key}]"
                    );
                }

                newKnownStrings = catalogManager.GetCatalogCount();
                if (newKnownStrings > knownStrings)
                {
                    Console.WriteLine($"Extracted {(newKnownStrings - knownStrings).ToString("N0")} strings from LastOnTable._TableManager.{field.Name}");
                }
            }

            japaneseStream?.Dispose();
            koreanStream?.Dispose();
        }

        private void ProcessObject(object japaneseObject, object koreanObject, string reference)
        {
            foreach (var property in japaneseObject.GetType().GetProperties())
            {
                if (property.MemberType != MemberTypes.Property && property.MemberType != MemberTypes.Field)
                {
                    continue;
                }

                if (property.ToString().StartsWith("System.Collections.Generic.List`1[System.String]"))
                {
                    var japanesePropInstance = property.GetValue(japaneseObject) as IList;
                    var koreanPropInstance = property.GetValue(japaneseObject) as IList;

                    var stringIndex = 0;

                    foreach (var japaneseArrayText in japanesePropInstance)
                    {
                        var koreanArrayText = koreanPropInstance[stringIndex++];

                        ProcessText((string)japaneseArrayText, (string)koreanArrayText, "", $"{reference}.{property.Name}[{stringIndex}]");
                    }

                    continue;
                }

                if (property.PropertyType.Name != "String")
                {
                    continue;
                }

                var japaneseText = (string)property.GetValue(japaneseObject, null);

                var koreanText = koreanObject == null
                    ? ""
                    : (string)koreanObject.GetType().GetProperty(property.Name).GetValue(koreanObject, null);

                if (japaneseText == null && koreanText == null)
                {
                    if (property.Name != "SkinUnlockItem")
                    {
                        Console.WriteLine($"Failed to obtain text from property {property.Name}");
                    }
                    continue;
                }

                var englishText = translationFinder.FindTranslation(koreanText, japaneseText);

                if (property.Name == "Char_Name" && string.IsNullOrEmpty(englishText))
                {
                    var officialTranslation = (string)japaneseObject.GetType().GetProperty("Char_Name_EngDisp").GetValue(japaneseObject, null);
                    englishText = officialTranslation;
                }

                ProcessText(japaneseText, koreanText, englishText, $"{reference}.{property.Name}");
            }
        }

        private void ProcessText(string japaneseText, string koreanText, string englishText, string reference)
        {
            if (string.IsNullOrWhiteSpace(japaneseText))
            {
                return;
            }

            // Make sure we contain non-ASCII text
            if (System.Text.RegularExpressions.Regex.Match(japaneseText, "^[\x00-\x7F]+$").Success)
            {
                //Debug.WriteLine($"Japanese text ASCII: {japaneseText}");
                return;
            }

            if (koreanText == japaneseText)
            {
                Debug.WriteLine($"Japanese text matches Korean text: {japaneseText}");
            }

            if (string.IsNullOrEmpty(englishText))
            {
                englishText = translationFinder.FindTranslation(koreanText, japaneseText);
            }

            catalogManager.AddToCatalog(japaneseText, koreanText, englishText, $"{reference}", 0);
        }
    }
}