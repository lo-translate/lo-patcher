using LoPatcher.LanguageUpdate;
using LoPatcher.Patcher;
using LoPatcher.Patcher.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LoPatcher
{
    public partial class MainForm : Form, IProgress<PatchProgress>, IProgress<int>
    {
        private readonly PatchWorker patchWorker = new PatchWorker();
        private readonly IEnumerable<IPatchTarget> containers;
        private readonly LanguageUpdateCheckWorker languageUpdateChecker = new LanguageUpdateCheckWorker();
        private readonly LanguageUpdateWorker languageUpdater = new LanguageUpdateWorker();

        private readonly LanguageCatalog languageCatalog;
        private readonly string localLanguageFile;

        private readonly string defaultSelectedFileText;

        private PatchQueue patchQueue;
        private Uri lanugageUpdateUrl;

        public MainForm(LanguageCatalog languageCatalog, IEnumerable<IPatchTarget> containers)
        {
            InitializeComponent();

            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));
            this.containers = containers ?? throw new ArgumentNullException(nameof(containers));

            // Save the selected file text so we can set it again when resetting the form.
            defaultSelectedFileText = labelSelectedFile.Text;

            // Hide the newest version label and update button. They will be shown when the user clicks check update.
            labelNewestLangVersion.Visible = false;
            buttonLanguageUpdate.Visible = false;

            // Load the translations from a file if it exists (downloaded via update) or the resource if not
            localLanguageFile = Path.Join(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Properties.Resources.LanguageLocalFile
            );

            if (File.Exists(localLanguageFile))
            {
                languageCatalog.LoadTranslations(localLanguageFile);
            }
            else
            {
                languageCatalog.LoadTranslations(Properties.Resources.LoTranslation);
            }

            // Display any translation catalog errors to the user. We continue so the user can attempt to update the
            // broken translations.
            if (languageCatalog.Errors.Any())
            {
                ErrorMessage(
                    Properties.Resources.ErrorModalTranslationParse,
                    string.Join("\r\n", languageCatalog.Errors)
                );
            }

            patchWorker.OnComplete += PatchWorker_OnComplete;
            languageUpdateChecker.OnComplete += LanguageUpdateChecker_OnComplete;
            languageUpdater.OnComplete += LanguageUpdater_OnComplete;

            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;

            ResetForm();
        }

        /// <summary>
        /// Displays an error message to the user. Replaces {reason} in the error message string with the specified
        /// reason.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reason"></param>
        private static void ErrorMessage(string message, string reason = null)
        {
            message = message.Replace("{reason}", reason ?? "", StringComparison.Ordinal).Trim();

            MessageBox.Show(message, Properties.Resources.ErrorModalTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Builds the local translation path using the executable path and the file name from the resources.
        /// </summary>
        /// <returns></returns>
        private static string GetLocalTranslationPath()
        {
            return Path.Join(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Properties.Resources.LanguageLocalFile
            );
        }

        /// <summary>
        /// Builds the queue from the specified input files
        /// </summary>
        /// <param name="files"></param>
        private void ChooseInputFiles(string[] files)
        {
            patchQueue = new PatchQueue();

            var errors = new List<string>();
            var builder = new StringBuilder();
            foreach (var file in files)
            {
                var onlyFileName = Path.GetFileName(file);
                using var fileStream = File.OpenRead(file);
                var fileContainer = containers.FirstOrDefault(container => container.CanPatch(fileStream));

                if (fileContainer is AssetBundleContainer)
                {
                    MessageBox.Show(
                        "Asset bundle support is not complete and will not translate as much. For more translations " +
                        "extract data.bin and LocalizationPatch from __data with UABE, then patch the raw export " +
                        "files and re-import the patched version with UABE.",
                        "Warning"
                    );
                }

                if (fileContainer == null)
                {
                    errors.Add($"Unknown file format: {onlyFileName}");
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(onlyFileName);
                patchQueue.Items.Add(new PatchQueueItem(file, fileContainer));
            }

            if (errors.Count > 0)
            {
                if (builder.Length > 0)
                {
                    ErrorMessage(Properties.Resources.ErrorModalSelectionPartialFail, string.Join("\r\n", errors));
                }
                else
                {
                    ErrorMessage(Properties.Resources.ErrorModalSelectionFullFail, string.Join("\r\n", errors));
                }
            }

            if (builder.Length > 0)
            {
                labelSelectedFile.Text = builder.ToString();
                buttonPatch.Enabled = true;
            }
            else
            {
                labelSelectedFile.Text = defaultSelectedFileText;
                buttonPatch.Enabled = false;
                patchQueue = null;
            }
        }

        /// <summary>
        /// Enables or disables the controls that the user can interact with.
        /// </summary>
        /// <param name="enable"></param>
        private void EnableForm(bool enable)
        {
            if (enable)
            {
                buttonPatch.Enabled = !labelSelectedFile.Text.Equals(defaultSelectedFileText, StringComparison.Ordinal);
            }
            else
            {
                buttonPatch.Enabled = false;
            }

            labelSelectedFile.Enabled = enable;
            linkLabelCheckLanguageUpdate.Enabled = enable;
            buttonChooseBundle.Enabled = enable;
            buttonLanguageUpdate.Enabled = enable;
        }

        /// <summary>
        /// Resets the form to the initial state.
        /// </summary>
        private void ResetForm()
        {
            patchQueue = null;
            labelSelectedFile.Text = defaultSelectedFileText;
            buttonPatch.Enabled = false;

            labelCurrentLangVersion.Text = languageCatalog.Version?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Called when the user clicks the check language update button. Hides the link, shows the version label, and
        /// starts the check update task.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkLabelCheckLanguageUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            labelNewestLangVersion.Text = Properties.Resources.LabelTextCheckingUpdate;
            labelNewestLangVersion.Visible = true;

            linkLabelCheckLanguageUpdate.Visible = false;

            EnableForm(false);

            languageUpdateChecker.StartUpdateCheck(new Uri(Properties.Resources.LanguageUpdateUrl));
        }

        /// <summary>
        /// Called after the update checker is done (on error or success)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageUpdateChecker_OnComplete(object sender, UpdateCheckResponse e)
        {
            if (labelNewestLangVersion.InvokeRequired)
            {
                labelNewestLangVersion.Invoke(new MethodInvoker(delegate { LanguageUpdateChecker_OnComplete(sender, e); }));
                return;
            }

            EnableForm(true);

            if (!e.Success)
            {
                labelNewestLangVersion.Visible = false;
                linkLabelCheckLanguageUpdate.Visible = true;

                ErrorMessage(Properties.Resources.ErrorModalUpdateCheckFailed, e.Error?.Message ?? "Unknown error.");
                return;
            }

            labelNewestLangVersion.Text = e.Version.ToString();

            try
            {
                lanugageUpdateUrl = new Uri(e.UpdateLocation);
            }
            catch (UriFormatException)
            {
                ErrorMessage(Properties.Resources.ErrorModalUpdateCheckFailed, "Invalid update URL receieved.");
                return;
            }

            if (languageCatalog.Version == null || e.Version > languageCatalog.Version)
            {
                buttonLanguageUpdate.Visible = true;
            }
        }

        /// <summary>
        /// Called when the user clicks the update button. Starts the updater task.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLanguageUpdate_Click(object sender, EventArgs e)
        {
            var languageOutputPath = Path.Join(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Properties.Resources.LanguageLocalFile
            );

            EnableForm(false);

            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.Visible = true;

            languageUpdater.StartUpdate(
                lanugageUpdateUrl, Properties.Resources.LanguageRemoteFile, languageOutputPath, this
            );
        }

        public void Report(int progress)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new MethodInvoker(delegate { Report(progress); }));
                return;
            }

            progressBar.Value = progress;
        }

        /// <summary>
        /// Called when the language task is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageUpdater_OnComplete(object sender, UpdateResponse e)
        {
            if (labelCurrentLangVersion.InvokeRequired)
            {
                labelCurrentLangVersion.Invoke(new MethodInvoker(delegate { LanguageUpdater_OnComplete(sender, e); }));
                return;
            }

            progressBar.Visible = false;

            EnableForm(true);

            if (!e.Success)
            {
                ErrorMessage(Properties.Resources.ErrorModalUpdateFailed, e.Error?.Message ?? "Unknown error.");
                return;
            }

            using var stream = File.OpenRead(GetLocalTranslationPath());

            if (languageCatalog.LoadTranslations(stream))
            {
                buttonLanguageUpdate.Visible = false;
                labelCurrentLangVersion.Text = languageCatalog.Version?.ToString() ?? "Unknown";
            }
            else
            {
                ErrorMessage(
                    Properties.Resources.ErrorModalTranslationParse,
                    string.Join("\r\n", languageCatalog.Errors)
                );
            }
        }

        /// <summary>
        /// Called when the user clicks the browse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonChooseBundle_Click(object sender, EventArgs e)
        {
            if (dialogChooseInput.ShowDialog() == DialogResult.OK)
            {
                ChooseInputFiles(dialogChooseInput.FileNames);
            }
        }

        /// <summary>
        /// Listen for drop events to select any files dropped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                ChooseInputFiles(files);
            }
        }

        /// <summary>
        /// Listen for drag enter events let Windows know we want the drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Called when the user clicks on the selected file label, forwards on to the choose file button event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelSelectedFile_Click(object sender, EventArgs e)
        {
            ButtonChooseBundle_Click(sender, e);
        }

        /// <summary>
        /// Called when the user clicks the patch button, starts the patch thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPatch_Click(object sender, EventArgs e)
        {
            EnableForm(false);

            progressBar.Maximum = 1;
            progressBar.Value = 0;
            progressBar.Visible = true;

            patchWorker.StartPatching(patchQueue, this);
            patchQueue = null;
        }

        /// <summary>
        /// Called when the patch task has a progress update.
        /// </summary>
        /// <param name="value"></param>
        public void Report(PatchProgress value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new MethodInvoker(delegate { Report(value); }));
                return;
            }

            if (value.IncreaseTotal > 0)
            {
                progressBar.Maximum += value.IncreaseTotal;
            }

            if (value.IncreaseCurrent > 0)
            {
                progressBar.Value += value.IncreaseCurrent;
            }
        }

        /// <summary>
        /// Called when the patch thread is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PatchWorker_OnComplete(object sender, PatchResponse e)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new MethodInvoker(delegate { PatchWorker_OnComplete(sender, e); }));
                return;
            }

            progressBar.Visible = false;

            if (e.Errors.Count > 0)
            {
                if (e.FilesPatched > 0)
                {
                    ErrorMessage(Properties.Resources.ErrorModalPatchPartialFail, string.Join("\r\n", e.Errors));
                }
                else
                {
                    ErrorMessage(Properties.Resources.ErrorModalPatchFullFail, string.Join("\r\n", e.Errors));
                }

                ResetForm();
                EnableForm(true);
                return;
            }

            MessageBox.Show(Properties.Resources.ModalPatchComplete);
            ResetForm();
            EnableForm(true);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                patchWorker?.Dispose();
                languageUpdateChecker?.Dispose();
                languageUpdater?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the specified number of parts from the specified path.
        /// </summary>
        /// <param name="path">The path to shorten</param>
        /// <param name="partsToKeep">The number of parts to keep</param>
        /// <returns></returns>
        private static string GetShorterDirectory(string path, int partsToKeep = 2)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);

            return string.Join(Path.DirectorySeparatorChar, parts.Skip(Math.Max(0, parts.Length - partsToKeep)));
        }
    }
}