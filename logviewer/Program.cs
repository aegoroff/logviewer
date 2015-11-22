// Created by: egr
// Created at: 16.09.2012
// © 2012-2015 Alexander Egorov
// Created by: egr
// Created at: 11.09.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BurnSystems.CommandLine;
using logviewer.core;
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
        private static void Main(string[] args)
        {
            var arguments = Parser.ParseIntoOrShowUsage<ProgramArguments>(args);
            if (arguments?.ProjectKey != null && arguments.ResultFile != null)
            {
                var crypt = new AsymCrypt();
                crypt.GenerateKeys();
                var crypted = crypt.Encrypt(arguments.ProjectKey);
                var key = $"Private key:\r\n{crypt.PrivateKey}\r\n\r\n";
                var token = $"Encrypted token:\r\n{crypted}";
                File.WriteAllText(arguments.ResultFile, key + token);
                return;
            }
            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
            Kernel = new StandardKernel(new LogviewerModule());
            using (Kernel)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(Kernel.Get<MainDlg>());
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var defectSender = new DefectSender(
                ConfigurationManager.AppSettings["GitHubAccount"],
                ConfigurationManager.AppSettings["GitHubProject"]);
            var task = defectSender.Send(e.ExceptionObject as Exception);
            MiniDump.CreateMiniDump();
            task.Wait();
        }
    }
}