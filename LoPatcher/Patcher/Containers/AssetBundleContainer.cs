using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // AssetsTools.NET can only operate on a FileStream so we convert back into one using a temporary file.
            var temporaryFile = Path.GetTempFileName();

            // We also need a file name in case the bundle needs to be unpacked.
            var unpackedFile = Path.GetTempFileName();

            var dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);

            File.WriteAllBytes(temporaryFile, dataBytes);

            using var fileStream = File.OpenRead(temporaryFile);

            // Load the bundled class data into a stream
            using var classData = new MemoryStream(Properties.Resources.classdata);

            try
            {
                var manager = new AssetsManager { updateAfterLoad = false };
                manager.LoadClassPackage(classData);

                var bundle = manager.LoadBundleFile(fileStream);

                // The LO bundles don't indicate they are packed when checking the header flags.  We check the reported
                // asset sizes and unpack if needed.
                if (IsPacked(bundle))
                {
                    bundle = UnpackBundle(manager, bundle, unpackedFile);
                }

                var files = BundleHelper.LoadAllAssetsDataFromBundle(bundle.file);
                using var mainStream = new MemoryStream(files[0]);
                var mainName = bundle.file.bundleInf6.dirInf[0].name;
                var assetsFile = manager.LoadAssetsFile(mainStream, mainName, true);

                var replacers = ProcessAssetsFile(manager, assetsFile, progressReporter);
                if (replacers.Count < 1)
                {
                    return false;
                }

                using var outputStream = new MemoryStream();
                using var writer = new AssetsFileWriter(outputStream);
                uint fileId = 0; // ?
                var bundleReplacer = new BundleReplacerFromAssets(
                    assetsFile.name, assetsFile.name, assetsFile.file, replacers, fileId
                );

                bundle.file.Write(writer, new List<BundleReplacer>() { bundleReplacer });
                writer.Flush();

                // Close the stream so the temporary file can be deleted.
                bundle.stream.Close();

                // We can't use stream in AssetsFileWriter or it will be closed when writer is disposed.
                stream.Position = 0;
                stream.SetLength(0);
                outputStream.CopyTo(stream);

                return true;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryFile))
                    {
                        File.Delete(temporaryFile);
                    }
                }
                catch (IOException exception)
                {
                    Debug.WriteLine($"Failed to delete temp file {temporaryFile}, {exception.Message}");
                }

                try
                {
                    if (File.Exists(unpackedFile))
                    {
                        File.Delete(unpackedFile);
                    }
                }
                catch (IOException exception)
                {
                    Debug.WriteLine($"Failed to delete temp file {unpackedFile}, {exception.Message}");
                }
            }
        }

        private List<AssetsReplacer> ProcessAssetsFile(AssetsManager manager, AssetsFileInstance assetsFile, IProgress<PatchProgress> progressReporter)
        {
            assetsFile.table.GenerateQuickLookupTree();
            manager.UpdateDependencies();
            manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

            var assetReplacers = new List<AssetsReplacer>();

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

                if (typeName == "TextAsset" && assetName == "LocalizationPatch")
                {
                    progressReporter.Report(new PatchProgress()
                    {
                        SetTargetAndReset = $"{assetName}"
                    });

                    var baseField = manager.GetATI(assetsFile.file, info).GetBaseField();

                    var name = baseField.Get("m_Name")?.GetValue().AsString();
                    var script = baseField.Get("m_Script")?.GetValue();

                    if (string.IsNullOrEmpty(name) || script == null)
                    {
                        continue;
                    }

                    using var scriptStream = new MemoryStream();

                    scriptStream.Write(Encoding.UTF8.GetBytes(script.AsString()));

                    if (targets.First(t => t is LocalizationPatchTarget).Patch(scriptStream, progressReporter))
                    {
                        script.Set(scriptStream.ToArray());

                        var replaced = baseField.WriteToByteArray();

                        assetReplacers.Add(new AssetsReplacerFromMemory(
                            info.curFileTypeOrIndex, info.index, (int)info.curFileType, 0xFFFF, replaced
                        ));
                    }
                }
            }

            return assetReplacers;
        }

        /// <summary>
        /// Unpack the specified bundle instance and return a new instance of the unpacked version.
        /// </summary>
        /// <param name="manager">The assets manager</param>
        /// <param name="bundle">The bundle to unpack</param>
        /// <param name="unpackedFile">The unpacked bundle file</param>
        /// <returns></returns>
        private static BundleFileInstance UnpackBundle(
            AssetsManager manager,
            BundleFileInstance bundle,
            string unpackedFile
        )
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
        private static bool IsPacked(BundleFileInstance bundleFile)
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