using AssetsTools.NET;
using AssetsTools.NET.Extra;
using LoPatcher.BundlePatch.AssetPatch;
using System;
using System.Collections.Generic;
using System.IO;

namespace LoPatcher.BundlePatch
{
    public class BundlePatcher
    {
        private readonly List<IAssetPatcher> assetPatchers = new List<IAssetPatcher>();
        private readonly Stream classDataStream;

        public BundlePatcher(Stream classDataStream)
        {
            this.classDataStream = classDataStream ?? throw new ArgumentNullException(nameof(classDataStream));
        }

        /// <summary>
        /// Attempts to patch inputFile, saving the patched version to outputFile
        /// </summary>
        /// <param name="inputFile">The file to patch</param>
        /// <param name="outputFile">Where to save the patched file</param>
        /// <returns></returns>
        public PatchResult Patch(string inputFile, string outputFile)
        {
            var temporaryFile = Path.GetTempFileName();

            try
            {
                using var fileStream = File.OpenRead(inputFile);

                var manager = new AssetsManager { updateAfterLoad = false };
                manager.LoadClassPackage(classDataStream);

                var bundle = manager.LoadBundleFile(fileStream);

                // The LO bundles don't indicate they are packed when checking the header flags.  We check the reported
                // asset sizes and unpack if needed.
                if (IsPacked(bundle))
                {
                    bundle = UnpackBundle(manager, bundle, temporaryFile);
                }

                var files = BundleHelper.LoadAllAssetsDataFromBundle(bundle.file);
                using var mainStream = new MemoryStream(files[0]);
                var mainName = bundle.file.bundleInf6.dirInf[0].name;
                var assetsFile = manager.LoadAssetsFile(mainStream, mainName, true);

                assetsFile.table.GenerateQuickLookupTree();
                manager.UpdateDependencies();
                manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

                var replacers = new List<AssetsReplacer>();

                // Check each asset in the bundle to see if we know how to handle
                foreach (var info in assetsFile.table.assetFileInfo)
                {
                    var type = AssetHelper.FindAssetClassByID(manager.classFile, info.curFileType);
                    if (type == null)
                    {
                        continue;
                    }

                    var typeName = type.name.GetString(manager.classFile);
                    var assetName = AssetHelper.GetAssetNameFast(assetsFile.file, manager.classFile, info);

                    foreach (var patcher in assetPatchers)
                    {
                        if (!patcher.CanPatch(typeName, assetName))
                        {
                            continue;
                        }

                        var replacer = patcher.Patch(manager, assetsFile, info);
                        if (replacer == null)
                        {
                            // TODO Throw in patcher.Patch instead?
                            throw new PatchFailedException($"Failed to patch \"{assetName}\" in \"{inputFile}\"");
                        }

                        replacers.Add(replacer);
                    }
                }

                // If we didn't find any replacers there was nothing in this file for us to patch
                if (replacers.Count < 1)
                {
                    // Close the stream so the temporary file can be deleted
                    bundle.stream.Close();

                    return new PatchResult(false, "No patchable assets found");
                }

                using var outputStream = File.OpenWrite(outputFile);
                using var writer = new AssetsFileWriter(outputStream);
                uint fileId = 0; // ?
                var bundleReplacer = new BundleReplacerFromAssets(assetsFile.name, assetsFile.name, assetsFile.file, replacers, fileId);
                bundle.file.Write(writer, new List<BundleReplacer>() { bundleReplacer });
                writer.Flush();

                // Close the stream so the temporary file can be deleted
                bundle.stream.Close();

                return new PatchResult(true);
            }
            catch (Exception e) when (e is PatchFailedException || e is IOException)
            {
                return new PatchResult(false, $"Patching failed: {e.Message}");
            }
            finally
            {
                if (File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }
            }
        }

        /// <summary>
        /// Unpack the specified bundle instance and return a new instance of the unpacked version.
        /// </summary>
        /// <param name="manager">The assets manager</param>
        /// <param name="bundle">The bundle to unpack</param>
        /// <param name="unpackedFile">The unpacked bundle file</param>
        /// <returns></returns>
        private static BundleFileInstance UnpackBundle(AssetsManager manager, BundleFileInstance bundle, string unpackedFile)
        {
            using var unpackedStream = File.OpenWrite(unpackedFile);
            using var reader = new AssetsFileReader(bundle.stream);
            using var writer = new AssetsFileWriter(unpackedStream);

            bundle.file.Unpack(reader, writer);
            writer.Flush();

            // Close the stream so assetsManager can open the file
            unpackedStream.Close();

            return manager.LoadBundleFile(unpackedFile);
        }

        /// <summary>
        /// Manually check if the asset is bundled by comparing the decompressed and compressed block sizes.
        /// </summary>
        /// <param name="bundleFile">The file to check</param>
        /// <returns>true if the bundle is compressed</returns>
        static bool IsPacked(BundleFileInstance bundleFile)
        {
            foreach (var block in bundleFile.file.bundleInf6.blockInf)
            {
                if (block.decompressedSize > block.compressedSize)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
