using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoPatcher.BundlePatch.AssetPatch
{
    public interface IAssetPatcher
    {
        public bool CanPatch(string type, string assetName);

        AssetsReplacer Patch(AssetsManager assetsManager, AssetsFileInstance assetsFile, AssetFileInfoEx info);
    }
}
