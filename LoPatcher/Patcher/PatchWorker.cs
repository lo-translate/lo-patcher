﻿using System;
using System.ComponentModel;
using System.IO;

namespace LoPatcher.Patcher
{
    public class PatchWorker : IDisposable
    {
        private readonly BackgroundWorker patchWorker;

        public event EventHandler<PatchResponse> OnComplete;

        public PatchWorker()
        {
            patchWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
            };

            patchWorker.DoWork += DoPatch;
            patchWorker.RunWorkerCompleted += PatchWorkerComplete;
        }

        public void StartPatching(PatchQueue queue, IProgress<PatchProgress> progressReporter)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            patchWorker.RunWorkerAsync(new PatchArguments(queue, progressReporter));
        }

        private void DoPatch(object sender, DoWorkEventArgs e)
        {
            var arguments = e.Argument as PatchArguments;
            var response = new PatchResponse();

            e.Result = response;

            foreach (var item in arguments.Queue.Items)
            {
                try
                {
                    using var fileStream = File.OpenRead(item.File);
                    using var memoryStream = new MemoryStream();

                    // Copy the file to a memory stream so we can modify it
                    fileStream.CopyTo(memoryStream);

                    if (item.Container.Patch(memoryStream, arguments.ProgressReporter))
                    {
                        response.FilesPatched++;

                        var outputFile = $"{item.File}.patched";

                        using var outputStream = File.OpenWrite(outputFile);
                        outputStream.Position = 0;
                        outputStream.SetLength(0); // In case the file exists and is larger than we're writing

                        memoryStream.Position = 0;
                        memoryStream.CopyTo(outputStream);
                    }
                    else
                    {
                        response.Errors.Add($"Nothing changed in {Path.GetFileName(item.File)}");
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    response.Errors.Add(exception.Message);
                }
            }

            response.Success = true;
        }

        private void PatchWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(e.Result is PatchResponse response))
            {
                response = new PatchResponse();
            }

            if (e.Error != null)
            {
                response.Errors.Add(e.Error.Message);
            }

            OnComplete?.Invoke(this, response);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                patchWorker?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private class PatchArguments
        {
            public PatchQueue Queue;
            public IProgress<PatchProgress> ProgressReporter;

            public PatchArguments(PatchQueue queue, IProgress<PatchProgress> progressReporter)
            {
                Queue = queue;
                ProgressReporter = progressReporter;
            }
        }
    }
}