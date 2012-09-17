﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            this.controller.LogMessageRead += this.OnLogMessage;
            this.KeepOriginalCaption();
        }

        #region ILogView Members

        public string LogPath { get; private set; }

        #endregion

        private void KeepOriginalCaption()
        {
            this.originalCapion = this.Text;
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            this.logReader.ReportProgress(e.Percent, e.Messages);
            if (this.toolStripStatusLabel1.Text == null)
            {
                this.toolStripStatusLabel1.Text = this.controller.HumanReadableLogSize;
            }
            this.toolStripProgressBar1.Value = e.Percent;
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
            this.toolStripStatusLabel1.Text = null;
            this.toolStripProgressBar1.Value = 0;
            this.syntaxRichTextBox1.Clear();
            this.toolStrip1.Focus();
            this.logReader.RunWorkerAsync(this.LogPath);
        }

        private void ReadLog(object sender, DoWorkEventArgs e)
        {
            this.controller.ReadLog(e.Argument as string);
        }

        private void ReadLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.Value = 100;
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

        private void OnReadLine(object sender, ProgressChangedEventArgs e)
        {
            if (this.logReader.CancellationPending)
            {
                return;
            }

            var messages = e.UserState as IList<LogMessage>;

            if (messages == null)
            {
                return;
            }
            foreach (var message in messages)
            {
                this.syntaxRichTextBox1.AppendText(Colorize(message.Strings[0]), message.ToString());
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.CancelReading();
            this.Text = this.originalCapion;
            this.toolStripStatusLabel1.Text = null;
            this.toolStripProgressBar1.Value = 0;
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