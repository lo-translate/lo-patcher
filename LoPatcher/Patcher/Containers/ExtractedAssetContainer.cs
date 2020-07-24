using LoPatcher.Unity;
using System;
using System.Collections.Generic;
using System.IO;

namespace LoPatcher.Patcher.Containers
{
    public class ExtractedAssetContainer : IPatchTarget
    {
        private IList<IPatchTarget> targets;

        public ExtractedAssetContainer(IList<IPatchTarget> targets)
        {
            this.targets = targets;
        }

        public bool CanPatch(Stream stream)
        {
            return ExtractedAssetReader.MatchesStructure(stream);
        }

        public bool Patch(Stream stream, IProgress<PatchProgress> progressReporter)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var asset = ExtractedAssetReader.ReadFromStream(stream);

            progressReporter.Report(new PatchProgress()
            {
                SetTargetAndReset = $"{asset.Name}"
            });

            // Load the asset content into a memory stream. The stream will be passed to any targets that can handle
            // it and written to a new file once patching is compolete.
            using var memoryStream = new MemoryStream();

            // We write to the memory stream instead of intializing with it so the stream can be expanded if needed.
            memoryStream.Write(asset.GetContent());

            bool patchApplied = false;
            foreach (var target in targets)
            {
                // Reset the position to 0 in case a target forgets to reset the position before checking
                memoryStream.Position = 0;

                if (target.CanPatch(memoryStream))
                {
                    // Reset the position to 0 in case a target forgets to reset the position before patching
                    memoryStream.Position = 0;

                    if (target.Patch(memoryStream, progressReporter))
                    {
                        patchApplied = true;
                    }
                }
            };

            if (patchApplied)
            {
                // Set the content on the asset (this also updates the content size)
                asset.SetContent(memoryStream.ToArray());

                // Reset the stream position and set the length to 0 in case the new length is lower
                stream.Position = 0;
                stream.SetLength(0);

                asset.WriteTo(stream);
            }

            return patchApplied;
        }
    }
}