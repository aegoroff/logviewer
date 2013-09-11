using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Ninject;
using logviewer.core;
using Settings = logviewer.core.Settings;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private int logFilterMax = (int)LogLevel.Fatal;
        private int logFilterMin;
        private string logFilterText;
        private string originalCapion;
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
            get { return controller; }
            set
            {
                controller = value;
                this.controller.SetView(this);
            }
        }

        public void Initialize()
        {
            Application.ThreadException += OnUnhandledException;
            this.KeepOriginalCaption();
            this.minLevelBox.SelectedIndex = Settings.MinLevel;
            this.maxLevelBox.SelectedIndex = Settings.MaxLevel;
            this.sortingBox.SelectedIndex = Settings.Sorting ? 0 : 1;
            this.filterBox.Text = Settings.MessageFilter;
            this.EnableControls(false);
            this.Controller.ReadRecentFiles();
            this.Controller.SetPageSize();
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

        public void SetProgress(double value)
        {
            var percent = (int)value;
            logLoadProgress.Value = percent;
            logLoadPercent.Text = string.Format("{0} %", percent);
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
            this.Controller.SaveRecentFiles();
        }

        public void LoadLog(string path)
        {
            this.syntaxRichTextBox1.SuspendLayout();
            this.syntaxRichTextBox1.LoadFile(path, RichTextBoxStreamType.RichText);
            this.syntaxRichTextBox1.ResumeLayout();
        }

        public bool OpenExport(string path)
        {
            this.exportDialog.FileName = path;
            return this.exportDialog.ShowDialog() == DialogResult.OK;
        }

        public void SetLoadedFileCapltion(string path)
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

        public void DisableForward(bool disabled)
        {
            this.next.Enabled = !disabled;
            this.last.Enabled = !disabled;
        }

        public void DisableBack(bool disabled)
        {
            this.prev.Enabled = !disabled;
            this.first.Enabled = !disabled;
        }

        public void SetPageSize(int size)
        {
            this.pageSizeLabel.Text = string.Format(this.pageSizeLabel.Text, size);
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
        }

        private void OnOpenRecentLogFile(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            this.StartLoadingLog(item.Tag as string);
        }

        public void StartLoadingLog(string path)
        {
            this.LogPath = path;
            this.Controller.CurrentPage = 1;
            this.Controller.ClearCache();
            this.ReadLog();
        }

        private void KeepOriginalCaption()
        {
            this.originalCapion = this.Text;
            this.LogInfoFormatString = this.toolStripStatusLabel2.Text;
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
            this.EnableControls(true);
            this.DisableBack(true);
            this.DisableForward(true);
            this.logFilterMin = this.minLevelBox.SelectedIndex;
            this.logFilterMax = this.maxLevelBox.SelectedIndex;
            this.logFilterText = this.filterBox.Text;
            this.reverse = this.sortingBox.SelectedIndex == 0;
            Settings.MinLevel = this.logFilterMin;
            Settings.MaxLevel = this.logFilterMax;
            Settings.Sorting = this.reverse;
            this.syntaxRichTextBox1.Clear();
            this.toolStrip1.Focus();
            this.Controller.BeginLogReading(this.logFilterMin, this.logFilterMax, this.logFilterText, this.reverse);
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
            this.textFilterChanging = true;
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
            this.Controller.ClearCache();
            this.StartReading();
        }

        private void OnExport(object sender, EventArgs e)
        {
            this.Controller.ExportToRtf();
        }

        private void OnChangeTextFilter(object sender, EventArgs e)
        {
            this.textFilterChanging = true;
            Settings.MessageFilter = this.filterBox.Text;
            this.StartReading();
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
            var dlg = new SettingsDlg();
            using (dlg)
            {
                dlg.ShowDialog();
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
    }
}