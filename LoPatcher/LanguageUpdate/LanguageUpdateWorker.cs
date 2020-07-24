using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace LoPatcher.LanguageUpdate
{
    public class LanguageUpdateWorker : IDisposable
    {
        private readonly BackgroundWorker downloadWorker;

        public event EventHandler<UpdateResponse> OnComplete;

        public LanguageUpdateWorker()
        {
            downloadWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
            };

            downloadWorker.DoWork += DoDownload;
            downloadWorker.RunWorkerCompleted += DownloadWorkerComplete;
        }

        public void StartUpdate(Uri updateUrl, string outputPath, IProgress<int> progressReporter)
        {
            if (updateUrl == null)
            {
                throw new ArgumentNullException(nameof(updateUrl));
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException(nameof(outputPath));
            }

            downloadWorker.RunWorkerAsync(new LanguageUpdaterTaskArguments()
            {
                DownloadFrom = updateUrl,
                DownloadTo = outputPath,
                ProgressReporter = progressReporter,
            });
        }

        private void DownloadWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OnComplete?.Invoke(this, new UpdateResponse() { Success = false, Error = e.Error });
            }
            else if (e.Cancelled)
            {
                OnComplete?.Invoke(this, new UpdateResponse() { Success = false });
            }
            else
            {
                OnComplete?.Invoke(this, new UpdateResponse() { Success = true });
            }
        }

        private void DoDownload(object sender, DoWorkEventArgs e)
        {
            var arguments = e.Argument as LanguageUpdaterTaskArguments;

            var tempFile = Path.GetTempFileName();

            // Download the JSON to a temporary file
            using var client = new HttpClient();

            client.DownloadFile(arguments.DownloadFrom, tempFile);
            client.DownloadProgressChanged += (object sender, System.Net.DownloadProgressChangedEventArgs e) =>
            {
                arguments.ProgressReporter?.Report(e.ProgressPercentage);
            };

            var foundCatalog = false;

            using (var fileStream = new FileStream(tempFile, FileMode.Open))
            using (var zip = new ZipArchive(fileStream))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name.EndsWith(".po", StringComparison.OrdinalIgnoreCase))
                    {
                        foundCatalog = true;
                        break;
                    }
                }
            }

            if (foundCatalog)
            {
                File.Copy(tempFile, arguments.DownloadTo);
                return;
            }

            try
            {
                File.Delete(tempFile);
            }
            catch (IOException exception)
            {
                Debug.WriteLine($"Failed to delete temp file {tempFile}, {exception.Message}");
            }

            throw new Exception("Failed to find translation in release archive");
        }

        private class LanguageUpdaterTaskArguments
        {
            public Uri DownloadFrom;
            public string DownloadTo;
            public IProgress<int> ProgressReporter;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                downloadWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}