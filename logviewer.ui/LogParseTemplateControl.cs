// Created by: egr
// Created at: 14.09.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Windows.Forms;
using logviewer.core;
using Color = System.Drawing.Color;

namespace logviewer.ui
{
    public partial class LogParseTemplateControl : UserControl, IParsingTemplateView
    {
        public LogParseTemplateControl()
        {
            this.InitializeComponent();
            this.invalidTemplateTooltip.SetToolTip(this.messageStartPatternBox, string.Empty);
            this.invalidTemplateTooltip.SetToolTip(this.messageFilterBox, string.Empty);
            this.Controller = new LogParseTemplateController(this);
        }

        public LogParseTemplateController Controller { get; }

        public void ShowInvalidTemplateError(string message, object control)
        {
            var box = (Control) control;
            this.invalidTemplateTooltip.Show(message, box);
            box.ForeColor = Color.Red;
        }

        public void HideInvalidTemplateError(object control)
        {
            var box = (Control)control;
            this.invalidTemplateTooltip.Hide(box);
        }

        public void OnFixTemplate(object control)
        {
            var box = (Control)control;
            box.ForeColor = Color.Black;
        }

        public void LoadTemplate(ParsingTemplate template)
        {
            this.messageStartPatternBox.Text = template.StartMessage;
            this.messageFilterBox.Text = template.Filter;
            this.messageCompiledBox.Checked = template.Compiled;
            this.Controller.Template = template;
        }

        public object MessageStartControl => this.messageStartPatternBox;

        public object FilterControl => this.messageFilterBox;

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.Controller.UpdateStartPattern(this.messageStartPatternBox.Text);
        }

        private void OnSetMessageFilter(object sender, EventArgs e)
        {
            this.Controller.UpdateFilter(this.messageFilterBox.Text);
        }

        private void OnSetCompiled(object sender, EventArgs e)
        {
            this.Controller.UpdateCompiled(this.messageCompiledBox.Checked);
        }
    }
}