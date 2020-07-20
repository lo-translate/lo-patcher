using LoPatcher.BundlePatch.AssetPatch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoPatcher
{
    public partial class MainForm : Form
    {
        private string selectedFile;
        private readonly string defaultSelectedFileText;
        private Thread patchThread;
        private readonly LanguageCatalog languageCatalog;

        public MainForm()
        {
            InitializeComponent();

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

        private void ChooseInputFile(string file)
        {
            selectedFile = file;
            labelSelectedFile.Text = GetShorterDirectory(selectedFile);
            buttonPatch.Enabled = true;

            SetStatusText("");
        }

        private void ResetForm(bool includeStatus)
        {
            if (labelSelectedFile.InvokeRequired)
            {
				labelSelectedFile.Invoke(new MethodInvoker(delegate { ResetForm(includeStatus); }));
                return;
            }

            selectedFile = string.Empty;
            labelSelectedFile.Text = defaultSelectedFileText;
            buttonPatch.Enabled = false;

            labelCurrentLangVersion.Text = languageCatalog.Version?.ToString() ?? "Unknown";

            if (includeStatus)
            {
                SetStatusText("");
            }
        }

        private void SetStatusText(string text)
        {
            SetStatusText(text, SystemColors.WindowText);
        }

        private void SetStatusText(string text, Color textColor)
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.Invoke(new MethodInvoker(delegate { SetStatusText(text, color); }));
                return;
            }

            labelStatus.ForeColor = textColor;
            labelStatus.Text = text;
        }

        private void EnableForm(bool enable)
        {
            if (buttonChooseBundle.InvokeRequired)
            {
                buttonChooseBundle.Invoke(new MethodInvoker(delegate { EnableForm(enable); }));
                return;
            }

            Enabled = enable;
        }

        private void DoPatch()
        {
            EnableForm(false);

            using var classData = new MemoryStream(Properties.Resources.classdata);
            var assetPatchers = new List<IAssetPatcher>() { new LocalizationPatchPatcher(languageCatalog) };
            var patcher = new BundlePatch.BundlePatcher(classData, assetPatchers);
            var outputFile = dialogChoosePatchOutput.FileName;
            var result = patcher.Patch(selectedFile, outputFile);
            if (result.Success)
            {
                SetStatusText($"File patched, saved to {GetShorterDirectory(outputFile)}", Color.Green);
                ResetForm(false);
            }
            else
            {
                SetStatusText($"Failed to patch file: {result.Status}", Color.Red);
            }

            EnableForm(true);
        }

        private void ButtonChooseBundle_Click(object sender, EventArgs e)
        {
            if (dialogChooseInput.ShowDialog() == DialogResult.OK)
            {
                ChooseInputFile(dialogChooseInput.FileName);
            }
        }

        private void LabelSelectedFile_Click(object sender, EventArgs e)
        {
            ButtonChooseBundle_Click(sender, e);
        }

        private void ButtonPatch_Click(object sender, EventArgs e)
        {
            if (dialogChoosePatchOutput.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (patchThread != null && patchThread.IsAlive)
            {
                MessageBox.Show(
                    Properties.Resources.ErrorModalAlreadyRunning,
                    Properties.Resources.ErrorModalTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            patchThread = new Thread(new ThreadStart(DoPatch)) { IsBackground = true };
            patchThread.Start();
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
