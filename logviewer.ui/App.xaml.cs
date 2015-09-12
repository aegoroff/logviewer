// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

using System.Globalization;
using System.Threading;
using System.Windows;

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
        }
    }
}