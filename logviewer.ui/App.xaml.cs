// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using logviewer.core;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var defectSender = new DefectSender(
                ConfigurationManager.AppSettings["GitHubAccount"],
                ConfigurationManager.AppSettings["GitHubProject"]);
            var task = defectSender.Send(e.Exception);
            MiniDump.CreateMiniDump();
            task.Wait();
        }
    }
}