using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private readonly LogController controller;
        private string originalCapion;

        public MainDlg()
        {
            this.InitializeComponent();
            this.controller = new LogController(this);
            this.KeepOriginalCaption();
        }

        #region ILogView Members

        public string LogPath { get; private set; }

        #endregion

        private void KeepOriginalCaption()
        {
            this.originalCapion = this.Text;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            var r = this.openFileDialog1.ShowDialog();

            if (r != DialogResult.OK)
            {
                return;
            }
            this.LogPath = this.openFileDialog1.FileName;

            if (this.logReader.IsBusy)
            {
                return;
            }
            this.Text = string.Format("{0}: {1}", this.originalCapion, this.LogPath);
            this.toolStripStatusLabel1.Text = "Reading log ...";
            this.syntaxRichTextBox1.Clear();
            this.toolStrip1.Focus();
            this.logReader.RunWorkerAsync(this.LogPath);
        }

        private void ReadLog(object sender, DoWorkEventArgs e)
        {
            e.Result = this.controller.ReadLog(e.Argument as string);
        }

        private void ReadLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripStatusLabel1.Text = this.controller.HumanReadableLogSize;
            this.syntaxRichTextBox1.Rtf = e.Result as string;
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.CancelReading();
            this.Text = this.originalCapion;
            this.toolStripStatusLabel1.Text = null;
            this.syntaxRichTextBox1.Clear();
        }

        private void OnExit(object sender, EventArgs e)
        {
            this.CancelReading();
            this.Close();
        }

        private void CancelReading()
        {
            if (!this.logReader.IsBusy || this.logReader.CancellationPending)
            {
                return;
            }
            this.controller.CancelReading();
            this.logReader.CancelAsync();
        }
    }
}