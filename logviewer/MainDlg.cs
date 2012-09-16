using System;
using System.ComponentModel;
using System.Drawing;
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

        private void OnOpen(object sender, EventArgs e)
        {
            var r = this.openFileDialog1.ShowDialog();

            if (r != DialogResult.OK)
            {
                return;
            }
            this.LogPath = this.openFileDialog1.FileName;

            if (!this.logReader.IsBusy)
            {
                this.syntaxRichTextBox1.Clear();
                this.logReader.RunWorkerAsync(this.LogPath);
            }
        }

        private void ReadLog(object sender, DoWorkEventArgs e)
        {
            this.controller.ReadLog(e.Argument as string);
            e.Result = this.controller.Messages;
        }

        private void ReadLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopLongRunningDisplay();
            this.SuspendLayout();
            foreach (var message in this.controller.Messages)
            {
                var i = 0;
                foreach (var s in message.Strings)
                {
                    if (i++ == 0)
                    {
                        this.syntaxRichTextBox1.AppendText(Colorize(s), s);
                    }
                    else
                    {
                        this.syntaxRichTextBox1.AppendText(s);
                        this.syntaxRichTextBox1.AppendText(Environment.NewLine);
                    }
                }
            }
            this.toolStripStatusLabel1.Text = this.controller.HumanReadableLogSize;
            this.ResumeLayout();
        }

        private static Color Colorize(string line)
        {
            var color = Color.Black;
            if (line.Contains("ERROR"))
            {
                color = Color.Red;
            }
            else if (line.Contains("WARN"))
            {
                color = Color.Orange;
            }
            else if (line.Contains("INFO"))
            {
                color = Color.Green;
            }
            else if (line.Contains("FATAL"))
            {
                color = Color.DarkViolet;
            }
            else if (line.Contains("DEBUG"))
            {
                color = Color.FromArgb(100, 100, 100);
            }
            else if (line.Contains("TRACE"))
            {
                color = Color.FromArgb(200, 200, 200);
            }
            return color;
        }
    }
}