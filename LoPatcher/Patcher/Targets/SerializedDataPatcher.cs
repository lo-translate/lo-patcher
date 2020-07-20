using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LoPatcher.Patcher.Targets
{
    internal class SerializedDataPatcher : IPatchTarget
    {
        private readonly ILanguageCatalog languageCatalog;

        public SerializedDataPatcher(ILanguageCatalog languageCatalog)
        {
            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));
        }

        public bool CanPatch(Stream stream)
        {
            stream.Position = 0;

            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            // Make sure the first record is SerializationHeaderRecord
            var recordId = reader.ReadByte();
            if (recordId != 0)
            {
                return false;
            }
            reader.ReadInt32(); // Root ID
            reader.ReadInt32(); // Header ID
            reader.ReadInt32(); // Major Version
            reader.ReadInt32(); // Minor Version

            // Followed by BinaryLibrary
            recordId = reader.ReadByte();
            if (recordId != 12)
            {
                return false;
            }

            reader.ReadInt32(); // LibraryId

            var libraryName = reader.ReadString();
            if (!Regex.Match(libraryName, "^[\x00-\x7F]+$").Success)
            {
                return false;
            }

            return true;
        }

        public bool Patch(Stream stream)
        {
            stream.Position = 0;

            var dictionary = languageCatalog.AsDictionary();

            var dataBytes = new byte[stream.Length];
            stream.Read(dataBytes, 0, (int)stream.Length);

            var replacedInsances = 0;
            var replacedStrings = 0;
            var neededEolChange = 0;

            foreach (var kvp in dictionary)
            {
                var japaneseText = kvp.Key;
                var englishText = kvp.Value;

                if (string.IsNullOrEmpty(japaneseText) || string.IsNullOrEmpty(englishText))
                {
                    continue;
                }

                var searchBytes = GetSearchBytes(japaneseText);
                var indexes = IndexesOf(dataBytes, searchBytes);

                var windowsEol = japaneseText.IndexOf("\r\n", StringComparison.Ordinal) != -1;

                // Karambolo.PO parses the PO file with \r\n as the newline character, we try again without the \r in
                // case that is how it was stored.
                if (indexes.Count < 1 && japaneseText.Contains("\r", StringComparison.Ordinal))
                {
                    japaneseText = japaneseText.Replace("\r", "", StringComparison.Ordinal);
                    searchBytes = GetSearchBytes(japaneseText);
                    indexes = IndexesOf(dataBytes, searchBytes);
                    windowsEol = false;

                    if (indexes.Count > 0)
                    {
                        neededEolChange++;
                    }
                }

                if (indexes.Count < 1)
                {
                    continue;
                }

                // We replace in reverse order so we don't need to worry about recalculating the indexes when there are
                // multiple instances found.
                foreach (var index in indexes.Reverse())
                {
                    replacedInsances++;

                    using var bufferStream = new MemoryStream();
                    using var bufferWriter = new BinaryWriter(bufferStream);

                    // Write the area before the match.
                    bufferStream.Write(dataBytes, 0, index);

                    if (!windowsEol)
                    {
                        englishText = englishText.Replace("\r", "", StringComparison.Ordinal);
                    }

                    // Write the English string where the Japanese string would have been. It is imporant this is done
                    // with BinaryWriter so it is written as a length prefixed string.
                    bufferWriter.Write(englishText);

                    // Write the area after the match.
                    bufferStream.Write(
                        dataBytes, 
                        index + searchBytes.Length, // Start after the search result
                        dataBytes.Length - searchBytes.Length - index // To the end
                    );

                    // Update the dataBytes array with the replaced version.
                    dataBytes = bufferStream.ToArray();
                }

                replacedStrings++;
            }

            Debug.WriteLine(
                $"Replaced {replacedStrings.ToString("N0", NumberFormatInfo.CurrentInfo)} strings " +
                $"({replacedInsances.ToString("N0", NumberFormatInfo.CurrentInfo)} instances) in SerializedData " +
                $"({neededEolChange} EOL changes needed)"
            );

            if (replacedStrings > 0)
            {
                stream.Position = 0;
                stream.SetLength(0);
                stream.Write(dataBytes);
            }

            return replacedStrings > 0;
        }

        /// <summary>
        /// Converts a string into a byte array that matches what would exist in a .NET binary serialized file.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static byte[] GetSearchBytes(string text)
        {
            using var binaryWriterStream = new MemoryStream();

            // Write the text to the stream using BinaryWriter so it writes as a length prefixed string
            using var binaryWriter = new BinaryWriter(binaryWriterStream);
            binaryWriter.Write(text);

            return binaryWriterStream.ToArray();
        }

        // https://stackoverflow.com/a/58347430
        private static ICollection<int> IndexesOf(byte[] haystack, byte[] needle, int startIndex = 0, bool includeOverlapping = false)
        {
            var collection = new List<int>();
            int matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
            while (matchIndex >= 0)
            {
                collection.Add(startIndex + matchIndex);
                startIndex += matchIndex + (includeOverlapping ? 1 : needle.Length);
                matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
            }
            return collection;
        }
    }
}