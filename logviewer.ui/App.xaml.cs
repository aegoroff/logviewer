// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.08.2015
// © 2012-2016 Alexander Egorov

#if !DEBUG
using System.Configuration;
using System.Windows.Threading;
using logviewer.logic;
#endif

using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows;
using logviewer.logic.ui.network;
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
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            WebRequest.DefaultWebProxy = new NetworkSettingsViewModel().Proxy;

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