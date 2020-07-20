using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LoPatcher.Unity
{
    internal class ExtractedAssetReader
    {
        public static bool MatchesStructure(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            var structure = ReadStructureFromStream(reader);

            return structure != null;
        }

        public static ExtractedAsset ReadFromStream(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            var structure = ReadStructureFromStream(reader);
            var content = reader.ReadBytes(structure.ContentSize);

            return new ExtractedAsset(structure.NameSize, structure.Name, structure.ContentSize, content);
        }

        private static PartialAsset ReadStructureFromStream(BinaryReader reader)
        {
            reader.BaseStream.Position = 0;

            var nameSize = reader.ReadInt32();
            if (nameSize < 0 || nameSize > 1024)
            {
                return null;
            }

            var name = Encoding.UTF8.GetString(reader.ReadBytes(nameSize));
            if (!Regex.Match(name, "^[\x00-\x7F]+$").Success)
            {
                return null;
            }

            AlignStream(reader.BaseStream);

            var contentSize = reader.ReadInt32();
            if (contentSize < 0 || contentSize > (reader.BaseStream.Length - reader.BaseStream.Position))
            {
                return null;
            }

            return new PartialAsset()
            {
                NameSize = nameSize,
                Name = name,
                ContentSize = contentSize,
            };
        }

        private static void AlignStream(Stream stream)
        {
            while (stream.Position % 4 != 0)
            {
                stream.Position++;
            }
        }

        private class PartialAsset
        {
            public int NameSize { get; set; }
            public string Name { get; set; }
            public int ContentSize { get; set; }
        }
    }
}