using System;
using System.Windows.Forms;
using Ninject;

namespace logviewer
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var kernel = new StandardKernel(new MainControllerModule());
            using (kernel)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(kernel.Get<MainDlg>());
            }
        }
    }
}