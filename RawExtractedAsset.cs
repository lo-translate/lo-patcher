using System;
using System.IO;
using System.Text;

namespace LoExtractText
{
    internal class RawExtractedAsset
    {
        public int NameSize { get; }
        public string Name { get; }
        public int ContentSize { get; private set; }
        public byte[] Content { get; private set; }

        public RawExtractedAsset(Stream stream)
        {
            using var reader = new BinaryReader(stream);

            reader.BaseStream.Position = 0;

            NameSize = reader.ReadInt32();
            if (NameSize < 0 || NameSize > 1024)
            {
                throw new InvalidOperationException("Name size not sane");
            }

            Name = Encoding.UTF8.GetString(reader.ReadBytes(NameSize));
            while (reader.BaseStream.Position % 4 != 0)
            {
                reader.BaseStream.Position++;
            }

            ContentSize = reader.ReadInt32();
            if (ContentSize < 0 || ContentSize > (reader.BaseStream.Length - reader.BaseStream.Position))
            {
                throw new InvalidOperationException("Content size not sane");
            }

            Content = reader.ReadBytes(ContentSize);

            var remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            var expectedPadding = reader.BaseStream.Position % 4;

            if (remainingBytes != expectedPadding)
            {
                System.Diagnostics.Debug.WriteLine("File does not match expected signature");
            }
        }

        public void SetContent(byte[] content)
        {
            Content = content;
            ContentSize = content.Length;
        }

        public void WriteTo(string outputFile)
        {
            using var stream = File.OpenWrite(outputFile);
            using var writer = new BinaryWriter(stream);

            writer.BaseStream.Position = 0;

            writer.Write(NameSize);
            writer.Write(Encoding.UTF8.GetBytes(Name));
            writer.Write(ContentSize);
            writer.Write(Content);
            while (writer.BaseStream.Position % 4 != 0)
            {
                writer.Write((byte)0x00);
            }

            writer.Close();
            stream.Close();
        }
    }
}