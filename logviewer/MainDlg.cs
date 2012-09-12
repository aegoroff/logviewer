using System.ComponentModel;
using System.Windows.Forms;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private readonly LogController controller;
        private LongRunningOperationDisplay longOperationDisplay;

        public MainDlg()
        {
            this.InitializeComponent();
            this.controller = new LogController(this);
        }

        #region ILogView Members

        public string LogPath { get; private set; }

        public void StartLongRunningDisplay()
        {
            this.longOperationDisplay = new LongRunningOperationDisplay(this, "Reading log ...");
        }

        public void StopLongRunningDisplay()
        {
            LongRunningOperationDisplay.Complete(this.longOperationDisplay);
        }

        #endregion

        private void OnOpen(object sender, System.EventArgs e)
        {
            DialogResult r = this.openFileDialog1.ShowDialog();

            if (r != DialogResult.OK)
            {
                return;
            }
            this.LogPath = this.openFileDialog1.FileName;
            
            if (!this.logReader.IsBusy)
            {
                this.logReader.RunWorkerAsync(this.LogPath);
            }
        }

        private void ReadLog(object sender, DoWorkEventArgs e)
        {
            e.Result = this.controller.ReadLog(e.Argument as string);
        }

        private void ReadLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopLongRunningDisplay();
            this.syntaxRichTextBox1.Text = e.Result as string;
            toolStripStatusLabel1.Text = this.controller.HumanReadableLogSize;
        }
    }
}