﻿// Created by: egr
// Created at: 16.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using logviewer.core;
using Ninject;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private int logFilterMax = (int)LogLevel.Fatal;
        private int logFilterMin;
        private string logFilterText;
        private string originalCapion;
        private string originalPageSizeFormat;
        private bool reverse;
        private bool textFilterChanging;
        private MainController controller;
        private bool applicationInitialized;

        public MainDlg()
        {
            this.InitializeComponent();
        }

        #region ILogView Members

        public string LogPath { get; set; }

        public string LogFileName
        {
            get { return this.openFileDialog1.FileName; }
        }

        public string HumanReadableLogSize
        {
            get { return this.toolStripStatusLabel1.Text; }
            set { this.toolStripStatusLabel1.Text = value; }
        }

        public string LogInfo
        {
            get { return this.toolStripStatusLabel2.Text; }
            set { this.toolStripStatusLabel2.Text = value; }
        }

        [Inject]
        public MainController Controller
        {
            get { return this.controller; }
            set
            {
                this.controller = value;
                this.controller.SetView(this);
            }
        }

        public void Initialize()
        {
            Application.ThreadException += OnUnhandledException;
            this.KeepFormatStrings();
            this.minLevelBox.SelectedIndex = Controller.Settings.MinLevel;
            this.maxLevelBox.SelectedIndex = Controller.Settings.MaxLevel;
            this.sortingBox.SelectedIndex = Controller.Settings.Sorting ? 0 : 1;
            this.useRegexp.Checked = Controller.Settings.UseRegexp;
            this.filterBox.Text = Controller.Settings.MessageFilter;
            this.EnableControls(false);
            this.Controller.ReadRecentFiles();
            this.Controller.ReadCompleted += this.OnReadCompleted;
            this.Controller.SetPageSize();
            this.Controller.ResetLogStatistic();
            this.applicationInitialized = true;
            this.Controller.LoadLastOpenedFile();
        }

        public void FocusOnTextFilterControl()
        {
            if (this.textFilterChanging)
            {
                this.filterBox.Focus();
            }
        }

        public void SetProgress(LoadProgress progress)
        {
            this.logLoadProgress.Value = progress.Percent;
            this.logLoadPercent.Text = progress.ToString();
        }

        public void CreateRecentFileItem(string file)
        {
            var result = new ToolStripMenuItem(file) { Tag = file };
            result.Click += this.OnOpenRecentLogFile;
            this.recentFilesToolStripMenuItem.DropDownItems.Add(result);
        }

        public bool OpenLogFile()
        {
            return this.openFileDialog1.ShowDialog() == DialogResult.OK;
        }

        public string LogInfoFormatString { get; private set; }

        public void ClearRecentFilesList()
        {
            foreach (ToolStripMenuItem item in this.recentFilesToolStripMenuItem.DropDownItems)
            {
                item.Click -= this.OnOpenRecentLogFile;
            }
            this.recentFilesToolStripMenuItem.DropDownItems.Clear();
        }

        public void ReadLog()
        {
            Executive.SafeRun(this.WatchLogFile);
            this.StartReading();
            this.Controller.AddCurrentFileToRecentFilesList();
        }

        private void OnSuccessRead(string rtf)
        {
            this.EnableControls(true);
            this.syntaxRichTextBox1.SuspendLayout();
            this.syntaxRichTextBox1.Rtf = rtf;
            this.syntaxRichTextBox1.ResumeLayout();
        }
        
        public void OnFailureRead(string errorMessage)
        {
            this.EnableControls(true);
            Log.Instance.Error(errorMessage);
        }

        public bool OpenExport(string path)
        {
            this.exportDialog.FileName = path;
            return this.exportDialog.ShowDialog() == DialogResult.OK;
        }

        private void SetLoadedFileCapltion(string path)
        {
            this.Text = string.Format("{0}: {1}", this.originalCapion, path);
        }

        public void SaveRtf()
        {
            this.syntaxRichTextBox1.SaveFile(this.exportDialog.FileName, RichTextBoxStreamType.RichText);
        }

        public void SetCurrentPage(int page)
        {
            this.currentPage.Text = string.Format("{0} / {1}", page, this.Controller.TotalPages);
        }

        private void DisableForward(bool disabled)
        {
            this.next.Enabled = !disabled;
            this.last.Enabled = !disabled;
        }

        private void DisableBack(bool disabled)
        {
            this.prev.Enabled = !disabled;
            this.first.Enabled = !disabled;
        }

        public void SetPageSize(int size)
        {
            this.pageSizeLabel.Text = string.Format(this.originalPageSizeFormat, size);
        }

        #endregion

        private static void OnUnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Instance.Fatal(e.Exception.Message, e.Exception);
        }

        private void EnableControls(bool enabled)
        {
            this.minLevelBox.Enabled = enabled;
            this.maxLevelBox.Enabled = enabled;
            this.sortingBox.Enabled = enabled;
            this.toolStripButton2.Enabled = enabled;
            this.refreshToolStripMenuItem.Enabled = enabled;
            this.exportToolStripMenuItem.Enabled = enabled;
            this.filterBox.Enabled = enabled;
            this.prev.Enabled = enabled;
            this.next.Enabled = enabled;
            this.last.Enabled = enabled;
            this.first.Enabled = enabled;
            this.useRegexp.Enabled = enabled;
        }

        private void OnOpenRecentLogFile(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            this.StartLoadingLog(item.Tag as string);
        }

        public void StartLoadingLog(string path)
        {
            this.LogPath = path;
            this.Controller.CurrentPage = 1;
            this.Controller.ClearCache();
            this.ReadLog();
        }

        private void KeepFormatStrings()
        {
            this.originalCapion = this.Text;
            this.LogInfoFormatString = this.toolStripStatusLabel2.Text;
            this.originalPageSizeFormat = this.pageSizeLabel.Text;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            this.Controller.OpenLogFile();
        }

        private void WatchLogFile()
        {
            this.logWatch.Path = Path.GetDirectoryName(this.LogPath);
            this.logWatch.Filter = Path.GetFileName(this.LogPath);
        }

        private void StartReading()
        {
            if (!this.applicationInitialized)
            {
                return;
            }
            this.EnableControls(false);
            this.DisableBack(true);
            this.DisableForward(true);
            this.logFilterMin = this.minLevelBox.SelectedIndex;
            this.logFilterMax = this.maxLevelBox.SelectedIndex;
            this.logFilterText = this.filterBox.Text;
            this.reverse = this.sortingBox.SelectedIndex == 0;
            Controller.Settings.MinLevel = this.logFilterMin;
            Controller.Settings.MaxLevel = this.logFilterMax;
            Controller.Settings.Sorting = this.reverse;
            this.syntaxRichTextBox1.Clear();
            this.toolStrip1.Focus();
            this.Controller.BeginLogReading(this.logFilterMin, this.logFilterMax, this.logFilterText, this.reverse,
                this.useRegexp.Checked);
        }

        void OnReadCompleted(object sender, LogReadCompletedEventArgs e)
        {
            this.LogInfo = string.Format(this.LogInfoFormatString, this.Controller.DisplayedMessages,
                this.Controller.TotalMessages, this.Controller.CountMessages(LogLevel.Trace), this.Controller.CountMessages(LogLevel.Debug),
                this.Controller.CountMessages(LogLevel.Info), this.Controller.CountMessages(LogLevel.Warn), this.Controller.CountMessages(LogLevel.Error),
                this.Controller.CountMessages(LogLevel.Fatal), this.Controller.TotalFiltered);

            this.OnSuccessRead(e.Rtf);
            this.SetCurrentPage(this.Controller.CurrentPage);
            this.DisableBack(this.Controller.CurrentPage <= 1);
            this.DisableForward(this.Controller.CurrentPage >= this.Controller.TotalPages);

            this.SetLoadedFileCapltion(this.LogPath);
            this.Controller.ReadRecentFiles();
            this.FocusOnTextFilterControl();
            this.SetProgress(LoadProgress.FromPercent(100));
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.Controller.CancelReading();
            this.Text = this.originalCapion;
            this.toolStripStatusLabel1.Text = null;
            this.syntaxRichTextBox1.Clear();
            this.EnableControls(false);
            this.Controller.CurrentPage = 1;
            this.logWatch.Filter = string.Empty;
        }

        private void OnExit(object sender, EventArgs e)
        {
            this.Controller.CancelReading();
            this.Close();
        }

        private void OnChangeFilter(object sender, EventArgs e)
        {
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            var dlg = new AboutDlg();
            using (dlg)
            {
                dlg.ShowDialog();
            }
        }

        private void OnChangeLog(object sender, FileSystemEventArgs e)
        {
            this.Controller.UpdateLog(e.FullPath);
        }

        private void OnExport(object sender, EventArgs e)
        {
            this.Controller.ExportToRtf();
        }

        private void OnChangeTextFilter(object sender, EventArgs e)
        {
            this.textFilterChanging = true;
            Controller.Settings.MessageFilter = this.filterBox.Text;
            if (MainController.IsValidFilter(this.filterBox.Text, this.useRegexp.Checked))
            {
                this.StartReading();
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this.Controller.ClearCache();
            this.OnChangeFilter(sender, e);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.Controller.InitializeLogger();
        }

        private void OnPrevPage(object sender, EventArgs e)
        {
            this.Controller.CurrentPage -= 1;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnNextPage(object sender, EventArgs e)
        {
            this.Controller.CurrentPage += 1;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnFirst(object sender, EventArgs e)
        {
            this.Controller.CurrentPage = 1;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnLast(object sender, EventArgs e)
        {
            this.Controller.CurrentPage = this.Controller.TotalPages;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnDragAndDropFile(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null || files.Length == 0)
            {
                return;
            }
            this.StartLoadingLog(files[0]);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void OnSettings(object sender, EventArgs e)
        {
            var dlg = new SettingsDlg(this.Controller.Settings);
            using (dlg)
            {
                dlg.ShowDialog();
                var template = this.Controller.Settings.ReadParsingTemplate();
                var levels = new[]
                {
                    template.Trace, template.Debug, template.Info, template.Warn, template.Error, template.Fatal
                };
                this.controller.CreateMarkers(levels);
                this.controller.CreateMessageHead(template.StartMessage);
                this.controller.PageSize();
                this.controller.UpdateKeepLastNFiles();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            this.controller.Dispose();
            base.Dispose(disposing);
        }

        private void OnChangeRegexpUsage(object sender, EventArgs e)
        {
            Controller.Settings.UseRegexp = this.useRegexp.Checked;
        }
    }
}