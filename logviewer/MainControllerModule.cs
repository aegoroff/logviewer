using System.Configuration;
using System.IO;
using Ninject.Modules;
using logviewer.core;

namespace logviewer
{
    public class MainControllerModule : NinjectModule
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

        private static int PageSize()
        {
            int pageSize;
            int.TryParse(ConfigurationManager.AppSettings["PageSize"], out pageSize);
            return pageSize;
        }

        public override void Load()
        {
            this.Bind<IMainController>().To<MainController>()
                .WithConstructorArgument("startMessagePattern",
                                         ConfigurationManager.AppSettings["StartMessagePattern"])
                .WithConstructorArgument("recentFilesFilePath",
                                         Path.Combine(Path.GetTempPath(), "logviewerRecentFiles.txt"))
                .WithConstructorArgument("levels", levels)
                .WithConstructorArgument("pageSize", PageSize());
        }
    }
}