// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.10.2014
// © 2012-2018 Alexander Egorov

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using logviewer.engine;
using logviewer.logic.models;
using logviewer.logic.Properties;

namespace logviewer.logic.ui.settings
{
    public class LogParseTemplateController : UiSynchronizeModel
    {
        private readonly IParsingTemplateView view;

        public LogParseTemplateController(IParsingTemplateView view)
        {
            this.view = view;
        }

        public ParsingTemplate Template { get; set; }

        public event EventHandler TemplateChangeSuccess;

        public event EventHandler TemplateChangeFailure;

        public void UpdateStartPattern(string value) => this.UpdatePattern(value, this.view.MessageStartControl, s => this.Template.StartMessage = s);

        public void UpdateFilter(string value) => this.UpdatePattern(value, this.view.FilterControl, s => this.Template.Filter = s);

        public void UpdateCompiled(bool value)
        {
            if (this.Template == null)
            {
                return;
            }

            this.Template.Compiled = value;
            this.OnTemplateChangeSuccess();
        }

        private void OnTemplateChangeSuccess() => this.TemplateChangeSuccess?.Invoke(this, new EventArgs());

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
                this.TemplateChangeFailure?.Invoke(this, new EventArgs());

                var o = Observable.Start(() => Thread.Sleep(millisecondsToShowTooltip), Scheduler.Default);

                o.ObserveOn(this.UiContextScheduler).Subscribe(unit => this.view.HideInvalidTemplateError(control));
                return;
            }

            assignAction(value);
            this.view.OnFixTemplate(control);
            this.view.HideInvalidTemplateError(control);
            this.OnTemplateChangeSuccess();
        }
    }
}
