using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using logviewer.Properties;
using logviewer.core;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private readonly MainController controller;
        readonly int pageSize;

        private readonly string[] levels = new[]
            {
                ConfigurationManager.AppSettings["TraceMarker"],
                ConfigurationManager.AppSettings["DebugMarker"],
                ConfigurationManager.AppSettings["InfoMarker"],
                ConfigurationManager.AppSettings["WarnMarker"],
                ConfigurationManager.AppSettings["ErrorMarker"],
                ConfigurationManager.AppSettings["FatalMarker"]
            };

        private int logFilterMax;
        private int logFilterMin;
        private string logFilterText;
        private string originalCapion;
        private string originalLogInfo;
        private bool reverse;
        private bool textFilterChanging;

        public MainDlg()
        {
            this.InitializeComponent();
            Application.ThreadException += OnUnhandledException;
            int.TryParse(ConfigurationManager.AppSettings["PageSize"], out pageSize);
            this.pageSizeLabel.Text = string.Format(this.pageSizeLabel.Text, pageSize);
            this.controller = new MainController(this, ConfigurationManager.AppSettings["StartMessagePattern"],
                                                 Path.Combine(Path.GetTempPath(), "logviewerRecentFiles.txt"), this.levels, pageSize);
            this.KeepOriginalCaption();
            this.toolStripComboBox1.SelectedIndex = 0;
            this.toolStripComboBox2.SelectedIndex = this.toolStripComboBox2.Items.Count - 1;
            this.toolStripComboBox3.SelectedIndex = 0;
            this.EnableControls(false);
            this.controller.ReadRecentFiles();
        }

        #region ILogView Members

        public string LogPath { get; set; }

        public string LogFileName
        {
            get { return this.openFileDialog1.FileName; }
        }

        public bool IsBusy
        {
            get { return this.logReader.IsBusy; }
        }

        public bool CancellationPending
        {
            get { return this.logReader.CancellationPending; }
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
            this.Text = string.Format("{0}: {1}", this.originalCapion, this.LogPath);
            this.StartReading();
            this.controller.SaveRecentFiles();
        }

        public void CancelRead()
        {
            this.logReader.CancelAsync();
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

        public void SaveRtf()
        {
            this.syntaxRichTextBox1.SaveFile(this.exportDialog.FileName, RichTextBoxStreamType.RichText);
        }

        public void SetCurrentPage(int page)
        {
            this.currentPage.Text = string.Format("{0} / {1}", page, this.controller.TotalPages);
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

        #endregion

        private static void OnUnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Instance.Fatal(e.Exception.Message, e.Exception);
        }

        private void EnableControls(bool enabled)
        {
            this.toolStripComboBox1.Enabled = enabled;
            this.toolStripComboBox2.Enabled = enabled;
            this.toolStripComboBox3.Enabled = enabled;
            this.toolStripButton2.Enabled = enabled;
            this.refreshToolStripMenuItem.Enabled = enabled;
            this.exportToolStripMenuItem.Enabled = enabled;
            this.toolStripTextBox1.Enabled = enabled;
            this.prev.Enabled = enabled;
            this.next.Enabled = enabled;
            this.last.Enabled = enabled;
            this.first.Enabled = enabled;
        }

        private void OnOpenRecentLogFile(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            this.LogPath = item.Tag as string;
            this.controller.CurrentPage = 1;
            this.controller.ClearCache();
            this.ReadLog();
        }

        private void KeepOriginalCaption()
        {
            this.originalCapion = this.Text;
            this.originalLogInfo = this.toolStripStatusLabel2.Text;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            this.controller.OpenLogFile();
        }

        private void WatchLogFile()
        {
            this.logWatch.Path = Path.GetDirectoryName(this.LogPath);
            this.logWatch.Filter = Path.GetFileName(this.LogPath);
        }

        private void StartReading()
        {
            if (this.logReader.IsBusy)
            {
                return;
            }
            this.EnableControls(true);
            this.DisableBack(true);
            this.DisableForward(true);
            this.logFilterMin = this.toolStripComboBox1.SelectedIndex;
            this.logFilterMax = this.toolStripComboBox2.SelectedIndex;
            this.logFilterText = this.toolStripTextBox1.Text;
            this.reverse = this.toolStripComboBox3.SelectedIndex == 0;
            this.toolStripStatusLabel1.Text = Resources.ReadingLogMessage;
            this.syntaxRichTextBox1.Clear();
            this.toolStrip1.Focus();
            this.logReader.RunWorkerAsync(this.LogPath);
        }

        private void ReadLog(object sender, DoWorkEventArgs e)
        {
            this.controller.MinFilter(this.logFilterMin);
            this.controller.MaxFilter(this.logFilterMax);
            this.controller.TextFilter(this.logFilterText);
            this.controller.Ordering(this.reverse);
            e.Result = Executive.SafeRun<string>(this.controller.ReadLog);
        }

        private void ReadLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripStatusLabel1.Text = this.controller.HumanReadableLogSize;
            this.toolStripStatusLabel2.Text = string.Format(this.originalLogInfo, this.controller.DisplayedMessages,
                                                            this.controller.TotalMessages, this.controller.CountMessages(LogLevel.Trace), this.controller.CountMessages(LogLevel.Debug),
                                                            this.controller.CountMessages(LogLevel.Info), this.controller.CountMessages(LogLevel.Warn), this.controller.CountMessages(LogLevel.Error),
                                                            this.controller.CountMessages(LogLevel.Fatal), controller.MessagesCount);
            this.controller.LoadLog(e.Result as string);
            this.controller.ReadRecentFiles();
            if (this.textFilterChanging)
            {
                this.toolStripTextBox1.Focus();
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.controller.CancelReading();
            this.Text = this.originalCapion;
            this.toolStripStatusLabel1.Text = null;
            this.syntaxRichTextBox1.Clear();
            this.EnableControls(false);
            this.controller.CurrentPage = 1;
            this.logWatch.Filter = string.Empty;
        }

        private void OnExit(object sender, EventArgs e)
        {
            this.controller.CancelReading();
            this.Close();
        }

        private void OnChangeFilter(object sender, EventArgs e)
        {
            this.textFilterChanging = false;
            this.controller.RebuildMessages = true;
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
            this.controller.ClearCache();
            this.StartReading();
        }

        private void OnExport(object sender, EventArgs e)
        {
            this.controller.ExportToRtf();
        }

        private void OnChangeTextFilter(object sender, EventArgs e)
        {
            this.textFilterChanging = true;
            this.controller.RebuildMessages = true;
            this.StartReading();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this.controller.ClearCache();
            this.OnChangeFilter(sender, e);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            this.controller.InitializeLogger();
        }

        private void OnPrevPage(object sender, EventArgs e)
        {
            this.controller.CurrentPage -= 1;
            this.controller.RebuildMessages = false;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnNextPage(object sender, EventArgs e)
        {
            this.controller.CurrentPage += 1;
            this.controller.RebuildMessages = false;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnFirst(object sender, EventArgs e)
        {
            this.controller.CurrentPage = 1;
            this.controller.RebuildMessages = false;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnLast(object sender, EventArgs e)
        {
            this.controller.CurrentPage = this.controller.TotalPages;
            this.controller.RebuildMessages = false;
            this.textFilterChanging = false;
            this.StartReading();
        }

        private void OnDragAndDropFile(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            this.LogPath = files[0];
            this.controller.CurrentPage = 1;
            this.controller.ClearCache();
            this.ReadLog();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }
}