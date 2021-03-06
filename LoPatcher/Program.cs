using LoPatcher.Patcher;
using LoPatcher.Patcher.Containers;
using LoPatcher.Patcher.Targets;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LoPatcher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var languageCatalog = new LanguageCatalog();

            var targets = new List<IPatchTarget>()
            {
                new SerializedDataTarget(languageCatalog),
                new LocalizationPatchTarget(languageCatalog),
            };

            var containers = new List<IPatchTarget>()
            {
                new AssetBundleContainer(targets),
                new ExtractedAssetContainer(targets),
            };

            using var form = new MainForm(languageCatalog, containers);
            Application.Run(form);
        }
    }
}