// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 14.09.2013
// © 2012-2017 Alexander Egorov

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using logviewer.engine;
using logviewer.logic;
using logviewer.logic.models;
using logviewer.logic.ui.settings;

namespace logviewer.ui
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
            this.newTemplateControl.LoadTemplate(new ParsingTemplate());
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
            this.selectedTemplateControl.LoadTemplate(template);
        }

        public void AddTemplateName(string name)
        {
            this.parsingTemplateSelector.Items.Add(name);
        }

        public void SelectParsingTemplateByName(string name)
        {
            this.parsingTemplateSelector.SelectedItem = name;
        }

        public void SelectParsingTemplate(int ix)
        {
            this.parsingTemplateSelector.SelectedIndex = ix;
        }

        public void RemoveParsingTemplateName(int ix)
        {
            this.parsingTemplateSelector.Items.RemoveAt(ix);
        }

        public void RemoveAllParsingTemplateNames()
        {
            this.parsingTemplateSelector.Items.Clear();
        }

        public void EnableRemoveTemplateControl(bool enabled)
        {
            this.removeParsingTemplateBtn.Enabled = enabled;
        }

        public ColorPickResult PickColor(Color startColor)
        {
            this.colorDialog1.Color = startColor;
            var result = new ColorPickResult
            {
                Result = this.colorDialog1.ShowDialog() == DialogResult.OK,
                SelectedColor = this.colorDialog1.Color
            };
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
            button.Image?.Dispose();
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
            this.selectedTemplateControl.Enabled = enabled;
            this.resetColorsBtn.Enabled = enabled;
        }

        public void EnableResetColors(bool enabled)
        {
            this.resetColorsBtn.Enabled = enabled;
        }

        public void ShowNewParsingTemplateForm(bool show)
        {
            var controls = new Control[]
            {
                this.newTemplateNameLabel,
                this.newTemplateNameBox,
                this.addNewTemplateBtn,
                this.cancelAddNewTemplateBtn,
                this.groupBox2,
                this.newTemplateControl
            };

            foreach (var control in controls)
            {
                if (show)
                {
                    control.Show();
                }
                else
                {
                    control.Hide();
                }
            }
        }

        public bool ShowWarning(string caption, string text)
        {
            return  MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public ParsingTemplate NewParsingTemplateData => new ParsingTemplate
        {
            Name = this.newTemplateNameBox.Text,
            StartMessage = this.newTemplateControl.Controller.Template.StartMessage,
            Filter = this.newTemplateControl.Controller.Template.Filter,
            Compiled = this.newTemplateControl.Controller.Template.Compiled,
        };

        public void EnableAddNewTemplate(bool enabled)
        {
            this.addNewTemplateBtn.Enabled = enabled;
        }

        public LogParseTemplateController SelectedTemplateController => this.selectedTemplateControl.Controller;

        public LogParseTemplateController NewTemplateController => this.newTemplateControl.Controller;

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

        private void OnChangeParsingTemplate(object sender, EventArgs e)
        {
            this.controller.LoadParsingTemplate(this.parsingTemplateSelector.SelectedIndex);
        }

        private void OnStartAddNewParsingTemplate(object sender, EventArgs e)
        {
            this.controller.StartAddNewParsingTemplate();
        }

        private void OnCancelAddNewParsingTemplate(object sender, EventArgs e)
        {
            this.controller.CancelNewParsingTemplate();
        }

        private void OnAddNewParsingTemplate(object sender, EventArgs e)
        {
            this.controller.AddNewParsingTemplate();
        }

        private void OnRemoveSelectedParsingTemplate(object sender, EventArgs e)
        {
            this.controller.RemoveSelectedParsingTemplate();
        }

        private void OnResetTemplateSettings(object sender, EventArgs e)
        {
            this.controller.RestoreDefaultTemplates();
        }
    }
}