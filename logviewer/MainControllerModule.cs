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
        
        private static readonly Func<string>[] levelReaders =
        {
           () => Settings.TraceLevel,
           () => Settings.DebugLevel,
           () => Settings.InfoLevel,
           () => Settings.WarnLevel,
           () => Settings.ErrorLevel,
           () => Settings.FatalLevel
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

        public override void Load()
        {
            this.Bind<ILogView>().To<MainDlg>();
            this.Bind<MainController>().ToSelf()
                .WithConstructorArgument("startMessagePattern", StartMessageTemplate)
                .WithConstructorArgument("recentFilesFilePath",
                                         Path.Combine(Path.GetTempPath(), "logviewerRecentFiles.txt"))
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
                    var userDefined = levelReaders[i]();
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