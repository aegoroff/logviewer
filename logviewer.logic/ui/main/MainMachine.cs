using System;
using Stateless;

namespace logviewer.logic.ui.main
{
    public class MainMachine : IDisposable
    {
        private readonly MainModel model;
        private readonly IMainViewModel viewModel;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string> openTrigger;
        private readonly StateMachine<State, Trigger> machine;
        private State state = State.Closed;
        private readonly LogWatcher logWatch;

        public MainMachine(MainModel model, IMainViewModel viewModel)
        {
            this.model = model;
            this.viewModel = viewModel;
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
                .PermitReentry(Trigger.Open)
                .PermitReentry(Trigger.Reload)
                .Permit(Trigger.Close, State.Closed);
        }


        public void Close()
        {
            this.machine.Fire(Trigger.Close);
        }

        public void Open(string path)
        {
            this.machine.Fire(this.openTrigger, path);
        }

        public void Start()
        {
            this.machine.Fire(Trigger.Start);
        }

        public void Reload()
        {
            this.machine.Fire(Trigger.Reload);
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

        private void OnReload()
        {
            this.model.ReadNewLog();
        }

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
            Close
        }

        public void Dispose()
        {
            this.model?.Dispose();
            this.viewModel?.Dispose();
            this.logWatch?.Dispose();
        }
    }
}