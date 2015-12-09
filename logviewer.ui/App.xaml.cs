// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

#if !DEBUG
using System.Configuration;
using System.Windows.Threading;
#endif

using System.Globalization;
using System.Windows;
using logviewer.core;
using Application = System.Windows.Forms.Application;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
#endif
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;
            NetworkSettings.SetProxy(MainViewModel.Current.SettingsProvider.OptionsProvider);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

#if !DEBUG
        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var defectSender = new DefectSender(
                ConfigurationManager.AppSettings["GitHubAccount"],
                ConfigurationManager.AppSettings["GitHubProject"]);
            var task = defectSender.Send(e.Exception);
            MiniDump.CreateMiniDump();
            task.Wait();
        }
#endif
    }
}