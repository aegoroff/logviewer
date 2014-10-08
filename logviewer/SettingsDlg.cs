// Created by: egr
// Created at: 14.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using logviewer.core;

namespace logviewer
{
    public partial class SettingsDlg : Form, ISettingsView
    {
        private readonly SettingsController controller;
        private Action<bool> apply;

        public SettingsDlg(ISettingsProvider settings)
        {
            this.InitializeComponent();
            this.controller = new SettingsController(this, settings);
            this.controller.Load();
        }

        public void SetApplyAction(Action<bool> action)
        {
            this.apply = action;
        }

        public void EnableSave(bool enabled)
        {
            this.saveButton.Enabled = enabled;
        }

        public void LoadFormData(FormData formData)
        {
            this.openLastFile.Checked = formData.OpenLastFile;
            this.pageSizeBox.Text = formData.PageSize;
            this.keepLastNFilesBox.Text = formData.KeepLastNFiles;
            this.autoRefreshCheckBox.Checked = formData.AutoRefreshOnFileChange;
        }

        public void LoadParsingTemplate(ParsingTemplate template)
        {
            this.messageStartPatternBox.Text = template.StartMessage;
        }

        public void AddTemplateName(string name)
        {
            this.parsingTemplateSelector.Items.Add(name);
        }

        public void SelectParsingTemplateByName(string name)
        {
            this.parsingTemplateSelector.SelectedItem = name;
        }

        public ColorPickResult PickColor(Color startColor)
        {
            this.colorDialog1.Color = startColor;
            ColorPickResult result = new ColorPickResult();
            result.Result = this.colorDialog1.ShowDialog() == DialogResult.OK;
            result.SelectedColor = this.colorDialog1.Color;
            return result;
        }

        static void Draw(ButtonBase button, Color color)
        {
            const int squareSize = 16;

            Image img = new Bitmap(squareSize, squareSize);
            using (var g = Graphics.FromImage(img))
            {
                // draw black background
                g.Clear(Color.White);
                var rect = new Rectangle(0, 0, squareSize, squareSize);
                var pen = new Pen(color, 1);
                using (pen)
                {
                    g.FillRectangle(pen.Brush, rect);
                }
            }
            if (button.Image != null)
            {
                button.Image.Dispose();
            }
            button.Image = img;
        }

        public void UpdateTraceColor(Color color)
        {
            Draw(this.traceBtn, color);
        }

        public void UpdateDebugColor(Color color)
        {
            Draw(this.debugBtn, color);
        }

        public void UpdateInfoColor(Color color)
        {
            Draw(this.infoBtn, color);
        }

        public void UpdateWarnColor(Color color)
        {
            Draw(this.warnBtn, color);
        }

        public void UpdateErrorColor(Color color)
        {
            Draw(this.errorBtn, color);
        }

        public void UpdateFatalColor(Color color)
        {
            Draw(this.fatalBtn, color);
        }

        public void EnableChangeOrClose(bool enabled)
        {
            this.closeButton.Enabled = enabled;
            this.traceBtn.Enabled = enabled;
            this.debugBtn.Enabled = enabled;
            this.infoBtn.Enabled = enabled;
            this.warnBtn.Enabled = enabled;
            this.errorBtn.Enabled = enabled;
            this.fatalBtn.Enabled = enabled;
            this.pageSizeBox.Enabled = enabled;
            this.keepLastNFilesBox.Enabled = enabled;
            this.openLastFile.Enabled = enabled;
            this.autoRefreshCheckBox.Enabled = enabled;
            this.parsingTemplateSelector.Enabled = enabled;
            this.messageStartPatternBox.Enabled = enabled;
            this.resetColorsBtn.Enabled = enabled;
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            this.controller.UpdateOpenLastFile(this.openLastFile.Checked);
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            this.controller.UpdatePageSize(this.pageSizeBox.Text);
        }

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.controller.UpdateMessageStartPattern(this.messageStartPatternBox.Text);
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            this.controller.UpdateKeepLastNFiles(this.keepLastNFilesBox.Text);
        }

        private void OnSave(object sender, EventArgs e)
        {
            this.controller.Save();
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            Debug.Assert(this.apply != null);
            this.apply(this.controller.RefreshOnClose);
        }

        private void OnSetParsingTemplateName(object sender, EventArgs e)
        {
            this.controller.UpdateParsingTemplateName(this.parsingTemplateSelector.Text);
        }

        private void OnChangeAutorefresh(object sender, EventArgs e)
        {
            this.controller.UpdateAutoRefreshOnFileChange(this.autoRefreshCheckBox.Checked);
        }

        private void OnChangeTrace(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Trace);
        }

        private void OnChangeDebug(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Debug);
        }

        private void OnChangeInfo(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Info);
        }

        private void OnChangeWarn(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Warn);
        }

        private void OnChangeError(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Error);
        }

        private void OnChangeFatal(object sender, EventArgs e)
        {
            this.controller.OnChangeLevelColor(LogLevel.Fatal);
        }

        private void OnResetToDefault(object sender, EventArgs e)
        {
            this.controller.ResetColorsToDefault();
        }
    }
}