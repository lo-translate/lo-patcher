using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace LoPatcher.LanguageUpdate
{
    public class LanguageUpdateCheckWorker : IDisposable
    {
        private readonly BackgroundWorker downloadWorker;

        public event EventHandler<UpdateCheckResponse> OnComplete;

        public LanguageUpdateCheckWorker()
        {
            downloadWorker = new BackgroundWorker();

            downloadWorker.DoWork += DoDownload;
            downloadWorker.RunWorkerCompleted += DownloadWorkerComplete;
        }

        public void StartUpdateCheck(Uri updateUrl)
        {
            if (updateUrl == null)
            {
                throw new ArgumentNullException(nameof(updateUrl));
            }

            downloadWorker.RunWorkerAsync(updateUrl);
        }

        private void DownloadWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OnComplete?.Invoke(this, new UpdateCheckResponse() { Success = false, Error = e.Error });
            }
            else if (e.Cancelled)
            {
                OnComplete?.Invoke(this, new UpdateCheckResponse() { Success = false });
            }
            else
            {
                var response = e.Result as UpdateCheckResponse;

                OnComplete?.Invoke(this, response);
            }
        }

        private void DoDownload(object sender, DoWorkEventArgs e)
        {
            var updateUrl = e.Argument as Uri;
            var temporaryFile = Path.GetTempFileName();

            // Download the JSON to a temporary file
            using var client = new HttpClient();

            client.DownloadFile(updateUrl, temporaryFile);

            // Read the downloaded JSON into memory so we can delete the temp file
            using var tempFileStream = new MemoryStream(File.ReadAllBytes(temporaryFile));

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

            var serializer = new DataContractJsonSerializer(typeof(GithubReleasesContract));
            var responseData = serializer.ReadObject(tempFileStream) as GithubReleasesContract;
            var version = responseData.tag_name;
            if (Regex.Match(version, "^v[0-9]").Success)
            {
                version = Regex.Replace(version, "^v", "");
            }

            if (!Version.TryParse(version, out var parsedVersion))
            {
                throw new Exception("Unknown version format");
            }

            var uploadData = new UpdateCheckResponse
            {
                Success = true,
                Version = parsedVersion,
                UpdateLocation = responseData.zipball_url
            };

            e.Result = uploadData;
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