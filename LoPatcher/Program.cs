using System;
using System.Windows.Forms;

namespace LoPatcher
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var languageCatalog = new LanguageCatalog();

            using var form = new MainForm(languageCatalog);
            Application.Run(form);
        }
    }
}
