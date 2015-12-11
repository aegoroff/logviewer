// Created by: egr
// Created at: 17.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Threading;
using System.Threading.Tasks;
using logviewer.engine;
using logviewer.logic.Properties;

namespace logviewer.logic
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
            this.UpdatePattern(value, this.view.MessageStartControl, s => this.Template.StartMessage = s);
        }

        public void UpdateFilter(string value)
        {
            this.UpdatePattern(value, this.view.FilterControl, s => this.Template.Filter = s);
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
            this.TemplateChangeSuccess.Do(handler => handler(this, new EventArgs()));
        }

        private void UpdatePattern(string value, object control, Action<string> assignAction)
        {
            if (this.Template == null)
            {
                return;
            }
            const int millisecondsToShowTooltip = 1500;

            if (new GrokMatcher(value).CompilationFailed)
            {
                this.view.ShowInvalidTemplateError(Resources.InvalidTemplate, control);
                this.TemplateChangeFailure.Do(handler => handler(this, new EventArgs()));
                var task = Task.Factory.StartNew(delegate
                {
                    Thread.Sleep(millisecondsToShowTooltip);
                });

                this.CompleteTask(
                    task, 
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    delegate { this.view.HideInvalidTemplateError(control); });
                return;
            }
            assignAction(value);
            this.view.OnFixTemplate(control);
            this.view.HideInvalidTemplateError(control);
            this.OnTemplateChangeSuccess();

        }
    }
}