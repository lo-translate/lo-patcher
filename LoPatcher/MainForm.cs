using LoPatcher.BundlePatch.AssetPatch;
using LoPatcher.Patcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoPatcher
{
    public partial class MainForm : Form
    {
        private readonly PatchWorker patchWorker = new PatchWorker();
        private readonly IEnumerable<IPatchTarget> containers;

        private readonly LanguageCatalog languageCatalog;

        private readonly string defaultSelectedFileText;

        private PatchQueue patchQueue;

        public MainForm(LanguageCatalog languageCatalog, IEnumerable<IPatchTarget> containers)
        {
            InitializeComponent();

            this.languageCatalog = languageCatalog ?? throw new ArgumentNullException(nameof(languageCatalog));
            this.containers = containers ?? throw new ArgumentNullException(nameof(containers));

            // Save the selected file text so we can set it again when resetting the form.
            defaultSelectedFileText = labelSelectedFile.Text;

            SetStatusText("");

            if (File.Exists("LoTranslation.po"))
            {
                languageCatalog.LoadTranslations("LoTranslation.po");
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

            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;
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

            linkLabelCheckLanguageUpdate.Enabled = enable;
            buttonChooseBundle.Enabled = enable;
            buttonLanguageUpdate.Enabled = enable;
        }

        private void ResetForm()
        {
            patchQueue = null;
            labelSelectedFile.Text = defaultSelectedFileText;
            buttonPatch.Enabled = false;

            labelCurrentLangVersion.Text = languageCatalog.Version?.ToString() ?? "Unknown";
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

        private void ButtonPatch_Click(object sender, EventArgs e)
        {
            EnableForm(false);

            patchWorker.StartPatching(patchQueue);
            patchQueue = null;
        }

        private void PatchWorker_OnComplete(object sender, PatchResponse e)
        {
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

                EnableForm(true);
                return;
            }

            MessageBox.Show(Properties.Resources.ModalPatchComplete);
            ResetForm();
            EnableForm(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                patchWorker?.Dispose();
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
