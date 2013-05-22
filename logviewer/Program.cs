using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using Ninject;
using logviewer.core;

namespace logviewer
{
    static class Program
    {
        private static readonly string[] levels = new[]
            {
                ConfigurationManager.AppSettings["TraceMarker"],
                ConfigurationManager.AppSettings["DebugMarker"],
                ConfigurationManager.AppSettings["InfoMarker"],
                ConfigurationManager.AppSettings["WarnMarker"],
                ConfigurationManager.AppSettings["ErrorMarker"],
                ConfigurationManager.AppSettings["FatalMarker"]
            };
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var kernel = new StandardKernel();
            using (kernel)
            {
                kernel.Bind<IMainController>().To<MainController>()
                      .WithConstructorArgument("startMessagePattern",
                                               ConfigurationManager.AppSettings["StartMessagePattern"])
                      .WithConstructorArgument("recentFilesFilePath",
                                               Path.Combine(Path.GetTempPath(), "logviewerRecentFiles.txt"))
                      .WithConstructorArgument("levels", levels)
                      .WithConstructorArgument("pageSize", PageSize());
                kernel.Bind<ILogView>().To<MainDlg>();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(kernel.Get<MainDlg>());
            }
        }

        private static int PageSize()
        {
            int pageSize;
            int.TryParse(ConfigurationManager.AppSettings["PageSize"], out pageSize);
            return pageSize;
        }
    }
}
