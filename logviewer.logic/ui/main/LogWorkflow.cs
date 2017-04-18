// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.03.2017
// © 2012-2017 Alexander Egorov

using System;
using System.ComponentModel;
using logviewer.logic.Annotations;
using Stateless;

namespace logviewer.logic.ui.main
{
    public sealed class LogWorkflow : IDisposable
    {
        private readonly LogWatcher logWatch;

        private readonly StateMachine<State, Trigger> machine;

        private readonly MainModel model;

        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string> openTrigger;

        private readonly IMainViewModel viewModel;

        private State state = State.Closed;

        public LogWorkflow(MainModel model, IMainViewModel viewModel)
        {
            this.model = model;
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
            this.logWatch = new LogWatcher(path => this.model.UpdateLog(path));

            this.machine = new StateMachine<State, Trigger>(() => this.state, s => this.state = s);
            this.openTrigger = this.machine.SetTriggerParameters<string>(Trigger.Open);

            this.machine.Configure(State.Closed)
                .OnEntryFrom(Trigger.Close, this.Dispose)
                .Permit(Trigger.Open, State.Opened)
                .Permit(Trigger.Start, State.Opened);

            this.machine.Configure(State.Opened)
                .OnEntryFrom(this.openTrigger, this.OnOpen)
                .OnEntryFrom(Trigger.Start, this.OnStart)
                .OnEntryFrom(Trigger.Reload, this.OnReload)
                .OnEntryFrom(Trigger.Filter, this.OnFilter)
                .PermitReentry(Trigger.Open)
                .PermitReentry(Trigger.Reload)
                .PermitReentry(Trigger.Filter)
                .Permit(Trigger.Close, State.Closed);
        }

        public void Dispose()
        {
            this.model?.Dispose();

            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= this.ViewModelOnPropertyChanged;
                this.viewModel.Dispose();
            }

            this.logWatch?.Dispose();
        }

        [PublicAPI]
        public void Close() => this.machine.Fire(Trigger.Close);

        [PublicAPI]
        public void Open(string path) => this.machine.Fire(this.openTrigger, path);

        [PublicAPI]
        public void Start() => this.machine.Fire(Trigger.Start);

        [PublicAPI]
        public void Reload() => this.machine.Fire(Trigger.Reload);

        [PublicAPI]
        public void Filter() => this.machine.Fire(Trigger.Filter);

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(this.viewModel.From):
                case nameof(this.viewModel.To):
                case nameof(this.viewModel.MinLevel):
                case nameof(this.viewModel.MaxLevel):
                case nameof(this.viewModel.SortingOrder):
                case nameof(this.viewModel.UseRegularExpressions) when !string.IsNullOrWhiteSpace(this.viewModel.MessageFilter):
                    this.Filter();
                    break;
                case nameof(this.viewModel.MessageFilter):
                    this.viewModel.IsTextFilterFocused = false;
                    this.Filter();
                    break;
                case nameof(this.viewModel.Visible):
                    this.viewModel.UpdateCount();
                    break;
            }
        }

        private void OnOpen(string path)
        {
            this.viewModel.LogPath = path;
            this.model.ReadNewLog();
            this.logWatch.WatchLogFile(path);
        }

        private void OnStart()
        {
            this.model.LoadLastOpenedFile();
            this.logWatch.WatchLogFile(this.viewModel.LogPath);
        }

        private void OnReload() => this.model.ReadNewLog();

        private void OnFilter() => this.model.StartReadingLogOnFilterChange();

        private enum State
        {
            Opened,

            Closed
        }

        private enum Trigger
        {
            Open,

            Start,

            Reload,

            Close,

            Filter
        }
    }
}
