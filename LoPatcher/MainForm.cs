using Karambolo.PO;
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
        private POCatalog catalog;

        private delegate void SetStatusTextDelegate(string text, Color color);
        private delegate void EnableFormDelegate(bool enable);
        private delegate void ResetFormDelegate(bool enable);

        public MainForm()
        {
            InitializeComponent();

            defaultSelectedFileText = labelSelectedFile.Text;

            SetStatusText("");

            if (File.Exists("LoTranslation.po"))
            {
                using var stream = File.OpenRead("LoTranslation.po");
                catalog = LoadTranslations(stream);
            }
            else
            {
                using var stream = new MemoryStream(Properties.Resources.LoTranslation);
                catalog = LoadTranslations(stream);
            }
        }

        private static POCatalog LoadTranslations(Stream stream)
        {
            var parser = new POParser(new POParserSettings());
            var result = parser.Parse(stream, Encoding.UTF8);

            if (result.Success)
            {
                return result.Catalog;
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.ErrorModalTranslationParse,
                    Properties.Resources.ErrorModalTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
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
                labelSelectedFile.Invoke(new ResetFormDelegate(ResetForm), new object[] { includeStatus });
                return;
            }

            selectedFile = string.Empty;
            labelSelectedFile.Text = defaultSelectedFileText;
            buttonPatch.Enabled = false;
            labelLanguageVersion.Text = GetTranslationVersion(catalog).ToString();

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
                labelStatus.Invoke(new SetStatusTextDelegate(SetStatusText), new object[] { text, textColor });
                return;
            }

            labelStatus.ForeColor = textColor;
            labelStatus.Text = text;
        }

        private void EnableForm(bool enable)
        {
            if (buttonChooseBundle.InvokeRequired)
            {
                buttonChooseBundle.Invoke(new EnableFormDelegate(EnableForm), new object[] { enable });
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

        private static Version GetTranslationVersion(POCatalog catalog)
        {
            var versionHeader = catalog.Headers.FirstOrDefault(
                h => h.Key.Equals("Project-Id-Version", StringComparison.Ordinal)
            );
            if (versionHeader.Value != null)
            {
                return Version.Parse(versionHeader.Value);
            }

            return null;
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
