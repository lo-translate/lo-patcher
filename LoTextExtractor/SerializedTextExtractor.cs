using LoTextExtractor.Lo.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

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
            var koreanRoot = koreanStream.Length > 0 ? formatter.Deserialize(koreanStream) as LastOnTable : null;

            foreach (var storyKvp in japaneseRoot._Table_PCStory_Client)
            {
                var storyId = storyKvp.Key;
                var stages = storyKvp.Value;
                var index = 0;

                foreach (var japaneseStage in stages)
                {
                    var sourceName = $"PCStory_Client[{storyId}][{index}]";
                    var koreanStage = koreanRoot?._Table_PCStory_Client[storyId][index];

                    extractedText.AddRange(FindTextInObject(japaneseStage, koreanStage, sourceName));

                    index++;
                }
            }

            foreach (var kvp in japaneseRoot._Table_BuffEffect_Client)
            {
                var sourceName = $"BuffEffect_Client[{kvp.Key}]";
                var koreanObject = koreanRoot._Table_BuffEffect_Client.ContainsKey(kvp.Key)
                    ? koreanRoot._Table_BuffEffect_Client[kvp.Key]
                    : null;

                extractedText.AddRange(FindTextInObject(kvp.Value, koreanObject, sourceName));
            }

            var excludedFields = new[]
            {
                "_Table_Announce",  // Announcement popup is now a website
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
                var koreanFieldInstance = koreanRoot != null
                    ? field.GetValue(koreanRoot._TableManager) as IDictionary
                    : null;

                foreach (DictionaryEntry kvp in japaneseFieldInstance)
                {
                    var sourceName = $"{field.Name.Replace("_Table_", "")}[{kvp.Key}]";
                    var koreanValue = koreanFieldInstance != null ? koreanFieldInstance[kvp.Key] : null;

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
                            Source = UpdateSource($"{sourceName}.{property.Name}[{stringIndex}]"),
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
                    Source = UpdateSource($"{sourceName}.{property.Name}"),
                });
            }

            return foundText;
        }

        private static string UpdateSource(string source)
        {
            if (Regex.IsMatch(source, @"\.LobbyScript1\[[0-9]\]$"))
            {
                source += "(Affection<100)";
            }
            else if (Regex.IsMatch(source, @"\.LobbyScript2\[[0-9]\]$"))
            {
                source += "(Affection>40)";
            }
            else if (Regex.IsMatch(source, @"\.LobbyScript3\[[0-9]\]$"))
            {
                source += "(Affection>70)";
            }
            else if (Regex.IsMatch(source, @"\.LobbyScript4\[[0-9]\]$"))
            {
                source += "(Affection=100)";
            }
            else if (Regex.IsMatch(source, @"\.LobbyScriptSp1\[[0-9]\]$"))
            {
                source += "(Affection<100,SpecialTouch)";
            }
            else if (Regex.IsMatch(source, @"\.LobbyScriptSp2\[[0-9]\]$"))
            {
                source += "(Affection=100,SpecialTouch)";
            }

            return source;
        }
    }
}