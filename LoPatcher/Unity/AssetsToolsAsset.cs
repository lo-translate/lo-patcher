using AssetsTools.NET;
using System;
using System.Diagnostics;
using System.IO;

namespace LoPatcher.Unity
{
    public class AssetsToolsAsset
    {
        private readonly AssetFileInfoEx info;
        private readonly AssetTypeValueField baseField;

        public string Type { get; private set; }

        public string Name { get; private set; }

        public AssetsToolsAsset(AssetFileInfoEx info, string type, string name, AssetTypeValueField baseField)
        {
            this.info = info ?? throw new ArgumentNullException(nameof(info));
            Type = type;
            Name = name;
            this.baseField = baseField ?? throw new ArgumentNullException(nameof(baseField));
        }

        public byte[] GetScript()
        {
            var name = baseField.Get("m_Name")?.GetValue().AsString();
            var script = baseField.Get("m_Script").GetValue();

            if (string.IsNullOrEmpty(name) || script == null)
            {
                return null;
            }

            if (name != Name)
            {
                Debug.WriteLine($"Script name does not match asset name '{name}' != '{Name}'");
            }

            return script.GetRawValue() as byte[];
        }

        public AssetsReplacer BuildReplacer(byte[] newScript)
        {
            using var memoryStream = new MemoryStream();
            using var assetWriter = new AssetsFileWriter(memoryStream);

            assetWriter.bigEndian = false;
            assetWriter.WriteCountStringInt32(Name);
            assetWriter.Align();
            assetWriter.Write(newScript.Length);
            assetWriter.Write(newScript);
            assetWriter.Align();

            var replaced = memoryStream.ToArray();

            return new AssetsReplacerFromMemory(
                info.curFileTypeOrIndex, info.index, (int)info.curFileType, 0xFFFF, replaced
            );
        }
    }
}
