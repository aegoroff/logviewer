// Created by: egr
// Created at: 14.09.2013
// © 2012-2013 Alexander Egorov

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using logviewer.core;
using Ninject.Modules;

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

        public override void Load()
        {
            this.Bind<ILogView>().To<MainDlg>();
            this.Bind<MainController>().ToSelf()
                .WithConstructorArgument("startMessagePattern", ConfigurationManager.AppSettings["StartMessagePattern"])
                .WithConstructorArgument("levels", Levels)
                .WithConstructorArgument("settingsDatabaseFileName",
                    ConfigurationManager.AppSettings["SettingsDatabase"])
                .WithConstructorArgument("keepLastNFiles", 10)
                .WithConstructorArgument("pageSize", 5000);
        }

        private static IEnumerable<string> Levels
        {
            get { return markers.Select(t => ConfigurationManager.AppSettings[t]); }
        }
    }
}