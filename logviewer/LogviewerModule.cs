// Created by: egr
// Created at: 14.09.2013
// © 2012-2015 Alexander Egorov

using System.Configuration;
using logviewer.logic;
using Ninject.Modules;

namespace logviewer
{
    public class LogviewerModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogView>().To<MainDlg>();
            this.Bind<MainController>().ToSelf();
            
            this.Bind<IParsingTemplateView>().To<LogParseTemplateControl>();
            this.Bind<LogParseTemplateController>().ToSelf();
            
            this.Bind<ISettingsProvider>().To<SqliteSettingsProvider>().InSingletonScope()
                .WithConstructorArgument("settingsDatabaseFileName", ConfigurationManager.AppSettings["SettingsDatabase"])
                .WithConstructorArgument("defaultKeepLastNFiles", 10)
                .WithConstructorArgument("defaultPageSize", 2000);
        }
    }
}