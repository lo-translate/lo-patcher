using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoPatcher
{
    public partial class MainForm : Form
    {
        private string selectedFile;
        private string defaultSelectedFileText;

        public MainForm()
        {
            InitializeComponent();

            defaultSelectedFileText = labelSelectedFile.Text;
        }

        private void Reset()
        {
            selectedFile = string.Empty;
            labelSelectedFile.Text = defaultSelectedFileText;
            buttonPatch.Enabled = false;
        }

        private void buttonChooseBundle_Click(object sender, EventArgs e)
        {
            if (dialogChooseBundle.ShowDialog() == DialogResult.OK)
            {
                selectedFile = dialogChooseBundle.FileName;
                labelSelectedFile.Text = GetShorterDirectory(selectedFile);
                buttonPatch.Enabled = true;
            }
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
