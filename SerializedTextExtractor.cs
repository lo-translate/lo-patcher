using LoTextExtractor.Lo.Generated;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
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
            Stream japaneseStream = File.OpenRead(japaneseFile);
            if (japaneseFile.EndsWith(".dat"))
            {
                var asset = new RawExtractedAsset(japaneseStream);
                var content = asset.Content;

                japaneseStream.Dispose();
                japaneseStream = new MemoryStream(content);
            }

            Stream koreanStream = File.OpenRead(koreanFile);
            if (koreanFile.EndsWith(".dat"))
            {
                var asset = new RawExtractedAsset(koreanStream);
                var content = asset.Content;

                koreanStream.Dispose();
                koreanStream = new MemoryStream(content);
            }

            var formatter = new BinaryFormatter() { Binder = new BinaryFormatterBinder() };

            var japaneseRoot = formatter.Deserialize(japaneseStream) as LastOnTable;
            var koreanRoot = formatter.Deserialize(koreanStream) as LastOnTable;

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

            foreach (var kvp in japaneseRoot._Table_BuffEffect_Client)
            {
                ProcessObject(
                    kvp.Value,
                    koreanRoot._Table_BuffEffect_Client[kvp.Key],
                    $"LastOnTable._Table_BuffEffect_Client[{kvp.Key}]"
                );
            }

            foreach (var field in japaneseRoot._TableManager.GetType().GetFields())
            {
                if (field.Name == "_Table_Forbidden")
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

                if (property.PropertyType.Name != "String")
                {
                    continue;
                }

                var japaneseText = (string)property.GetValue(japaneseObject, null);
                if (string.IsNullOrWhiteSpace(japaneseText))
                {
                    continue;
                }

                // Make sure we contain non-ASCII text
                if (System.Text.RegularExpressions.Regex.Match(japaneseText, "^[\x00-\x7F]+$").Success)
                {
                    //Debug.WriteLine($"Japanese text ASCII: {japaneseText}");
                    continue;
                }

                var koreanText = koreanObject == null
                    ? ""
                    : (string)koreanObject.GetType().GetProperty(property.Name).GetValue(koreanObject, null);

                var englishText = translationFinder.FindTranslation(koreanText, japaneseText);

                // Skip text the Japanese translators ignored under the assumption it isn't used
                if (koreanText == japaneseText)
                {
                    Debug.WriteLine($"Japanese text matches Korean text: {japaneseText}");
                }

                if (property.Name == "Char_Name")
                {
                    var officialTranslation = (string)japaneseObject.GetType().GetProperty("Char_Name_EngDisp").GetValue(japaneseObject, null);
                    if (!string.IsNullOrEmpty(englishText))
                    {
                        Debug.WriteLine($"Duplicate translation: {officialTranslation} != {englishText}");
                    }
                    englishText = officialTranslation;
                }

                catalogManager.AddToCatalog(japaneseText, koreanText, englishText, $"{reference}.{property.Name}", 0);
            }
        }
    }
}