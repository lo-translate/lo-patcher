using System;

namespace LoPatcher.Unity
{
    public class ExtractedAsset
    {
        public int NameSize { get; private set; }

        public string Name { get; private set; }

        public int ContentSize { get; private set; }

        private byte[] content;

        public byte[] GetContent()
        {
            return content;
        }

        public ExtractedAsset(int nameSize, string name, int contentSize, byte[] content)
        {
            NameSize = nameSize;
            Name = name;
            ContentSize = contentSize;
            this.content = content;
        }

        public void SetContent(byte[] content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            ContentSize = content.Length;
        }
    }
}