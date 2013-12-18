// Created by: egr
// Created at: 16.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Ninject;

namespace logviewer
{
    internal static class Program
    {
        internal static IKernel Kernel { get; private set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Kernel = new StandardKernel(new LogviewerModule());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(Kernel.Get<MainDlg>());
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MiniDump.CreateMiniDump();
        }
    }
}