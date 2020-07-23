using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LoPatcher.Unity
{
    public class AssetsToolsBundle : IDisposable
    {
        private readonly AssetsManager manager;
        private BundleFileInstance bundle;
        private AssetsFileInstance assetsFile;

        private readonly string unpackFile;
        private readonly string temporaryFile;

        private MemoryStream assetsFileStream;
        private FileStream bundleFileStream;

        public AssetsToolsBundle(byte[] classData)
        {
            using var classDataStream = new MemoryStream(classData);

            manager = new AssetsManager { updateAfterLoad = false };
            manager.LoadClassPackage(classDataStream);

            unpackFile = Path.GetTempFileName();
            temporaryFile = Path.GetTempFileName();
        }

        public bool Load(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            bundleFileStream?.Dispose();

            // AssetsTools.NET can only operate on a FileStream so we convert back into one using a temporary file.
            var temporaryFile = Path.GetTempFileName();

            var dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);

            File.WriteAllBytes(temporaryFile, dataBytes);

            bundleFileStream = File.OpenRead(temporaryFile);

            return Load(bundleFileStream);
        }

        public bool Load(FileStream bundleStream)
        {
            bundle = manager.LoadBundleFile(bundleStream);

            // The LO bundles don't indicate they are packed when checking the header flags.  We check the reported
            // asset sizes and unpack if needed.
            if (IsPacked(bundle))
            {
                bundle = Unpack(manager, bundle, unpackFile);
            }

            var files = BundleHelper.LoadAllAssetsDataFromBundle(bundle.file);
            assetsFileStream = new MemoryStream(files[0]);

            assetsFile = manager.LoadAssetsFile(assetsFileStream, bundle.file.bundleInf6.dirInf[0].name, true);

            assetsFile.table.GenerateQuickLookupTree();
            manager.UpdateDependencies();
            manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

            return true;
        }

        public IList<AssetsToolsAsset> GetAssets()
        {
            var assets = new List<AssetsToolsAsset>();

            foreach (var info in assetsFile.table.assetFileInfo)
            {
                var type = AssetHelper.FindAssetClassByID(manager.classFile, info.curFileType);
                if (type == null)
                {
                    continue;
                }

                var typeName = type.name.GetString(manager.classFile);
                var assetName = AssetHelper.GetAssetNameFast(assetsFile.file, manager.classFile, info);
                var baseField = manager.GetATI(assetsFile.file, info).GetBaseField();

                assets.Add(new AssetsToolsAsset(info, typeName, assetName, baseField));
            }

            return assets;
        }

        public void SaveTo(List<AssetsReplacer> replacers, Stream outputStream)
        {
            if (replacers == null)
            {
                throw new ArgumentNullException(nameof(replacers));
            }

            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            using var bufferStream = new MemoryStream();
            using var assetFileWriter = new AssetsFileWriter(bufferStream);

            uint fileId = 0; // ?

            var bundleReplacer = new BundleReplacerFromAssets(
                assetsFile.name, assetsFile.name, assetsFile.file, replacers, fileId
            );

            bundle.file.Write(assetFileWriter, new List<BundleReplacer>() { bundleReplacer });
            assetFileWriter.Flush();

            // Close the stream so the temporary file can be deleted.
            bundle.stream.Close();

            // We can't use stream in AssetsFileWriter or it will be closed when writer is disposed.
            outputStream.Position = 0;
            outputStream.SetLength(0);

            bufferStream.Position = 0;
            bufferStream.CopyTo(outputStream);
        }

        /// <summary>
        /// Unpack the specified bundle instance and return a new instance of the unpacked version.
        /// </summary>
        /// <param name="manager">The assets manager</param>
        /// <param name="bundle">The bundle to unpack</param>
        /// <param name="unpackedFile">The unpacked bundle file</param>
        /// <returns></returns>
        private static BundleFileInstance Unpack(
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                bundle?.stream.Close();
                assetsFile?.stream.Close();

                assetsFileStream?.Dispose();
                bundleFileStream?.Dispose();

                try
                {
                    if (File.Exists(unpackFile))
                    {
                        File.Delete(unpackFile);
                    }
                }
                catch (IOException exception)
                {
                    Debug.WriteLine($"Failed to delete temp file {unpackFile}, {exception.Message}");
                }

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
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}