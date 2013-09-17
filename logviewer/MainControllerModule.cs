using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Ninject.Modules;
using logviewer.core;

namespace logviewer
{
    public class MainControllerModule : NinjectModule
    {
        private static readonly string[] markers =
        {
           "TraceMarker",
           "DebugMarker",
           "InfoMarker",
           "WarnMarker",
           "ErrorMarker",
           "FatalMarker"
        };
        
        private static readonly Action<string>[] levelSetters =
        {
           l => Settings.TraceLevel = l,
           l => Settings.DebugLevel = l,
           l => Settings.InfoLevel = l,
           l => Settings.WarnLevel = l,
           l => Settings.ErrorLevel = l,
           l => Settings.FatalLevel = l
        };

        const string ApplicationOptionsFolder = "logviewer";
        const string RecentFilesFileName = "logviewerRecentFiles.txt";

        public override void Load()
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var applicationFolder = Path.Combine(baseFolder, ApplicationOptionsFolder);
            var recentFilesPath = Path.Combine(applicationFolder, RecentFilesFileName);
            var oldRecentFilesPath = Path.Combine(Path.GetTempPath(), RecentFilesFileName);
            if (!Directory.Exists(applicationFolder))
            {
                Directory.CreateDirectory(applicationFolder);
            }
            
            if (File.Exists(oldRecentFilesPath))
            {
                File.Move(oldRecentFilesPath, recentFilesPath);
            }
            
            this.Bind<ILogView>().To<MainDlg>();
            this.Bind<MainController>().ToSelf()
                .WithConstructorArgument("startMessagePattern", StartMessageTemplate)
                .WithConstructorArgument("recentFilesFilePath", recentFilesPath)
                .WithConstructorArgument("levels", Levels)
                .WithConstructorArgument("pageSize", Settings.PageSize);
        }

        private static string StartMessageTemplate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Settings.StartMessageTemplate))
                {
                    Settings.StartMessageTemplate = ConfigurationManager.AppSettings["StartMessagePattern"];
                }
                return Settings.StartMessageTemplate;
            }
        }

        static IEnumerable<string> Levels
        {
            get
            {
                for (var i = 0; i < markers.Length; i++)
                {
                    var userDefined = Settings.LevelReaders[i]();
                    if(string.IsNullOrWhiteSpace(userDefined))
                    {
                        var vendorDefined = ConfigurationManager.AppSettings[markers[i]];
                        levelSetters[i](vendorDefined);
                        yield return vendorDefined;
                    }
                    else
                    {
                        yield return userDefined;
                    }
                }
            }
        }
    }
}