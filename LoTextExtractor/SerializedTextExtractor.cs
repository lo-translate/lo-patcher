using LoTextExtractor.Lo.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace LoTextExtractor
{
    internal class SerializedTextExtractor
    {
        public IEnumerable<ExtractedText> ExtractText(byte[] japaneseData, byte[] koreanData)
        {
            var extractedText = new List<ExtractedText>();

            var formatter = new BinaryFormatter() { Binder = new BinaryFormatterBinder() };

            using var japaneseStream = new MemoryStream(japaneseData);
            var japaneseRoot = formatter.Deserialize(japaneseStream) as LastOnTable;

            using var koreanStream = new MemoryStream(koreanData);
            var koreanRoot = formatter.Deserialize(koreanStream) as LastOnTable;

            foreach (var storyKvp in japaneseRoot._Table_PCStory_Client)
            {
                var storyId = storyKvp.Key;
                var stages = storyKvp.Value;
                var index = 0;

                foreach (var japaneseStage in stages)
                {
                    var sourceName = $"PCStory_Client[{storyId}][{index}]";
                    var koreanStage = koreanRoot._Table_PCStory_Client[storyId][index];

                    extractedText.AddRange(FindTextInObject(japaneseStage, koreanStage, sourceName));

                    index++;
                }
            }

            foreach (var kvp in japaneseRoot._Table_BuffEffect_Client)
            {
                var sourceName = $"BuffEffect_Client[{kvp.Key}]";
                var koreanObject = koreanRoot._Table_BuffEffect_Client[kvp.Key];

                extractedText.AddRange(FindTextInObject(kvp.Value, koreanObject, sourceName));
            }

            var excludedFields = new[]
            {
                "_Table_Forbidden", // Forbidden words, nothing useful for translation
            };

            // Loop through the dictionaries in TableManager and process each of them
            foreach (var field in japaneseRoot._TableManager.GetType().GetFields())
            {
                if (excludedFields.Contains(field.Name) || field.FieldType.Name != "Dictionary`2")
                {
                    continue;
                }

                var japaneseFieldInstance = field.GetValue(japaneseRoot._TableManager) as IDictionary;
                var koreanFieldInstance = field.GetValue(koreanRoot._TableManager) as IDictionary;

                foreach (DictionaryEntry kvp in japaneseFieldInstance)
                {
                    var sourceName = $"{field.Name.Replace("_Table_", "")}[{kvp.Key}]";
                    var koreanValue = koreanFieldInstance[kvp.Key];

                    extractedText.AddRange(FindTextInObject(kvp.Value, koreanValue, sourceName));
                }
            }

            return extractedText;
        }

        private static IEnumerable<ExtractedText> FindTextInObject(object japaneseObject, object koreanObject, string sourceName)
        {
            var foundText = new List<ExtractedText>();

            foreach (var property in japaneseObject.GetType().GetProperties())
            {
                if (property.MemberType != MemberTypes.Property && property.MemberType != MemberTypes.Field)
                {
                    continue;
                }

                if (property.ToString().StartsWith("System.Collections.Generic.List`1[System.String]", StringComparison.Ordinal))
                {
                    var japanesePropInstance = property.GetValue(japaneseObject) as IList;
                    var koreanPropInstance = property.GetValue(japaneseObject) as IList;

                    var stringIndex = 0;

                    foreach (var japaneseArrayText in japanesePropInstance)
                    {
                        var koreanArrayText = koreanPropInstance[stringIndex++];

                        foundText.Add(new ExtractedText()
                        {
                            Japanese = (string)japaneseArrayText,
                            Korean = (string)koreanArrayText,
                            Source = $"{sourceName}.{property.Name}[{stringIndex}]",
                        });
                    }

                    continue;
                }

                if (property.PropertyType.Name != "String")
                {
                    continue;
                }

                var japaneseText = (string)property.GetValue(japaneseObject, null);
                var koreanText = koreanObject != null
                    ? (string)koreanObject.GetType().GetProperty(property.Name).GetValue(koreanObject, null)
                    : "";

                if (japaneseText == null && koreanText == null)
                {
                    if (property.Name != "SkinUnlockItem")
                    {
                        Debug.WriteLine($"Failed to obtain text from property {sourceName}.{property.Name}");
                    }
                    continue;
                }

                foundText.Add(new ExtractedText()
                {
                    Japanese = japaneseText,
                    Korean = koreanText,
                    Source = $"{sourceName}.{property.Name}",
                });
            }

            return foundText;
        }
    }
}