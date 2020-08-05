using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Diagnostics;

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

            return script.AsStringBytes();
        }

        public AssetsReplacer BuildScriptReplacer(byte[] newScript)
        {
            if (newScript == null)
            {
                throw new ArgumentNullException(nameof(newScript));
            }

            var script = baseField.Get("m_Script").GetValue();
            if (script == null)
            {
                throw new InvalidOperationException("Script value doesn't exist");
            }

            script.Set(newScript);

            var replaced = baseField.WriteToByteArray();
            
            return new AssetsReplacerFromMemory(
                info.curFileTypeOrIndex, info.index, (int)info.curFileType, 0xFFFF, replaced
            );
        }
    }
}