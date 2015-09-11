﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fluent;
using logviewer.core;
using logviewer.engine;

namespace logviewer.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ILogView
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        public void ShowDialogAboutNewVersionAvaliable(Version current, Version latest, string targetAddress)
        {
            throw new NotImplementedException();
        }

        public void ShowNoUpdateAvaliable()
        {
            throw new NotImplementedException();
        }

        public string LogPath
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string LogFileName
        {
            get { throw new NotImplementedException(); }
        }

        public string HumanReadableLogSize
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string LogInfo
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void ClearRecentFilesList()
        {
            throw new NotImplementedException();
        }

        public void CreateRecentFileItem(string file)
        {
            throw new NotImplementedException();
        }

        public bool OpenLogFile()
        {
            throw new NotImplementedException();
        }

        public void ReadLog()
        {
            throw new NotImplementedException();
        }

        public void OnFailureRead(string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool OpenExport(string path)
        {
            throw new NotImplementedException();
        }

        public void SaveRtf()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentPage(int page)
        {
            throw new NotImplementedException();
        }

        public void SetPageSize(int size)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void FocusOnTextFilterControl()
        {
            throw new NotImplementedException();
        }

        public void SetProgress(LoadProgress progress)
        {
            throw new NotImplementedException();
        }

        public void SetLogProgressCustomText(string text)
        {
            throw new NotImplementedException();
        }

        public void StartLoadingLog(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLoadedFileCapltion(string path)
        {
            throw new NotImplementedException();
        }

        public void SetFileEncoding(string encoding)
        {
            throw new NotImplementedException();
        }

        public void StartReading()
        {
            throw new NotImplementedException();
        }

        public void AddFilterItems(string[] items)
        {
            throw new NotImplementedException();
        }

        public void ClearTemplatesList()
        {
            throw new NotImplementedException();
        }

        public void CreateTemplateSelectionItem(ParsingTemplate template, int current)
        {
            throw new NotImplementedException();
        }

        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
