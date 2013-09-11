using System.Configuration;
using System.IO;
using Ninject.Modules;
using logviewer.core;

namespace logviewer
{
    public class MainControllerModule : NinjectModule
    {
        private static readonly string[] levels =
        {
            ConfigurationManager.AppSettings["TraceMarker"],
            ConfigurationManager.AppSettings["DebugMarker"],
            ConfigurationManager.AppSettings["InfoMarker"],
            ConfigurationManager.AppSettings["WarnMarker"],
            ConfigurationManager.AppSettings["ErrorMarker"],
            ConfigurationManager.AppSettings["FatalMarker"]
        };

        public override void Load()
        {
            this.Bind<ILogView>().To<MainDlg>();
            this.Bind<MainController>().ToSelf()
                .WithConstructorArgument("startMessagePattern",
                                         ConfigurationManager.AppSettings["StartMessagePattern"])
                .WithConstructorArgument("recentFilesFilePath",
                                         Path.Combine(Path.GetTempPath(), "logviewerRecentFiles.txt"))
                .WithConstructorArgument("levels", levels)
                .WithConstructorArgument("pageSize", Settings.PageSize);
        }
    }
}