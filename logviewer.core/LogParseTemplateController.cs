// Created by: egr
// Created at: 17.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core.Properties;

namespace logviewer.core
{
    public class LogParseTemplateController : BaseGuiController
    {
        private readonly IParsingTemplateView view;

        public LogParseTemplateController(IParsingTemplateView view)
        {
            this.view = view;
        }

        public ParsingTemplate Template { get; set; }

        public event EventHandler TemplateChangeSuccess;
        public event EventHandler TemplateChangeFailure;

        public void UpdateStartPattern(string value)
        {
            if (this.Template == null)
            {
                return;
            }
            var valid = this.ValidatePattern(value, this.view.ShowInvalidTemplateError, this.view.HideInvalidTemplateError, this.view.OnFixTemplate);
            if (valid)
            {
                this.Template.StartMessage = value;
            }
        }

        public void UpdateFilter(string value)
        {
            if (this.Template == null)
            {
                return;
            }
            this.Template.Filter = value;
            this.OnTemplateChangeSuccess();
        }

        public void UpdateCompiled(bool value)
        {
            if (this.Template == null)
            {
                return;
            }
            this.Template.Compiled = value;
            this.OnTemplateChangeSuccess();
        }

        private void OnTemplateChangeSuccess()
        {
            if (this.TemplateChangeSuccess != null)
            {
                this.TemplateChangeSuccess(this, new EventArgs());
            }
        }

        private bool ValidatePattern(string value, Action<string> showError, Action hideTooltip, Action fixAction)
        {
            if (new GrokMatcher(value).CompilationFailed)
            {
                showError(Resources.InvalidTemplate);
                if (this.TemplateChangeFailure != null)
                {
                    this.TemplateChangeFailure(this, new EventArgs());
                }
                Task.Factory.StartNew(delegate
                {
                    Thread.Sleep(2 * 1000);
                    this.RunOnGuiThread(hideTooltip);
                });
                return false;
            }
            fixAction();
            hideTooltip();
            this.OnTemplateChangeSuccess();
            return true;
        }
    }
}