// Created by: egr
// Created at: 29.10.2014
// © 2012-2015 Alexander Egorov

using Ninject.Modules;

namespace logviewer.logic
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IVersionsReader>().To<VersionsReader>()
                .WithConstructorArgument("account", System.Configuration.ConfigurationManager.AppSettings["GitHubAccount"])
                .WithConstructorArgument("project", System.Configuration.ConfigurationManager.AppSettings["GitHubProject"]);
        }
    }
}