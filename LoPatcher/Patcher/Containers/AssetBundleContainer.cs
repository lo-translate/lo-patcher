using AssetsTools.NET;
using LoPatcher.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LoPatcher.Patcher.Containers
{
    public class AssetBundleContainer : IPatchTarget
    {
        private const string unityFsSignature = "UnityFS";

        private readonly IList<IPatchTarget> targets;

        public AssetBundleContainer(IList<IPatchTarget> targets)
        {
            this.targets = targets;
        }

        public bool CanPatch(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Position = 0;

            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            var fileSignature = Encoding.UTF8.GetString(reader.ReadBytes(unityFsSignature.Length));
            return fileSignature.Equals(unityFsSignature, StringComparison.Ordinal);
        }

        public bool Patch(Stream stream, IProgress<PatchProgress> progressReporter)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (progressReporter == null)
            {
                throw new ArgumentNullException(nameof(progressReporter));
            }

            using var bundle = new AssetsToolsBundle(Properties.Resources.classdata);

            if (!bundle.Load(stream))
            {
                throw new Exception("Failed to load bundle");
            }

            var assetReplacers = new List<AssetsReplacer>();

            foreach (var asset in bundle.GetAssets())
            {
                // We don't care about non-text assets or the Korean data files
                if (asset.Type != "TextAsset" || asset.Name.Contains("_ko.bin", StringComparison.Ordinal))
                {
                    continue;
                }

                progressReporter.Report(new PatchProgress()
                {
                    SetTargetAndReset = $"{asset.Name}"
                });

                var scriptBytes = asset.GetScript();
                if (scriptBytes == null)
                {
                    continue;
                }

                using var scriptStream = new MemoryStream();
                scriptStream.Write(scriptBytes);

                var patched = false;

                foreach (var target in targets)
                {
                    if (target.CanPatch(scriptStream))
                    {
                        if (target.Patch(scriptStream, progressReporter))
                        {
                            patched = true;
                        }
                    }
                }

                if (!patched)
                {
                    continue;
                }

                assetReplacers.Add(asset.BuildScriptReplacer(scriptStream.ToArray()));
            }

            if (assetReplacers.Count > 0)
            {
                bundle.SaveTo(assetReplacers, stream);
            }

            return assetReplacers.Count > 0;
        }
    }
}