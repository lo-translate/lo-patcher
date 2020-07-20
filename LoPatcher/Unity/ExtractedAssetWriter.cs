using System.IO;
using System.Text;

namespace LoPatcher.Unity
{
    public static class ExtractedAssetWriter
    {
        public static void WriteTo(this ExtractedAsset asset, Stream stream)
        {
            if (asset == null)
            {
                throw new System.ArgumentNullException(nameof(asset));
            }

            if (stream == null)
            {
                throw new System.ArgumentNullException(nameof(stream));
            }

            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            writer.BaseStream.Position = 0;

            writer.Write(asset.NameSize);
            writer.Write(Encoding.UTF8.GetBytes(asset.Name));
            AlignStream(writer);

            writer.Write(asset.ContentSize);
            writer.Write(asset.GetContent());
            AlignStream(writer);
        }

        private static void AlignStream(BinaryWriter writer)
        {
            while (writer.BaseStream.Position % 4 != 0)
            {
                writer.Write((byte)0x00);
            }
        }
    }
}