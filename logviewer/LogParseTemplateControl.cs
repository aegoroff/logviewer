using System;
using System.Drawing;
using System.Windows.Forms;
using logviewer.core;
using Ninject;

namespace logviewer
{
    public partial class LogParseTemplateControl : UserControl, IParsingTemplateView
    {
        private readonly LogParseTemplateController controller;

        public LogParseTemplateControl()
        {
            this.InitializeComponent();
            this.invalidTemplateTooltip.SetToolTip(this.messageStartPatternBox, string.Empty);
            this.controller = new LogParseTemplateController(this);
        }

        public LogParseTemplateController Controller
        {
            get { return this.controller; }
        }

        public void ShowInvalidTemplateError(string message)
        {
            this.invalidTemplateTooltip.Show(message, this.messageStartPatternBox);
            this.messageStartPatternBox.ForeColor = Color.Red;
        }

        public void HideInvalidTemplateError()
        {
            this.invalidTemplateTooltip.Hide(this.messageStartPatternBox);
        }

        public void OnFixTemplate()
        {
            this.messageStartPatternBox.ForeColor = Color.Black;
        }

        public void LoadTemplate(ParsingTemplate template)
        {
            this.messageStartPatternBox.Text = template.StartMessage;
            this.messageFilterBox.Text = template.Filter;
            this.messageCompiledBox.Checked = template.Compiled;
            this.Controller.Template = template;
        }

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