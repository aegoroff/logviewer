// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represent business logic of network settings dialog
    /// </summary>
    public class NetworkSettingsController
    {
        private readonly NetworkSettings networkSettings;
        private readonly StateMachine stateMachine;
        private readonly INetworkSettingsView view;
        private bool started;

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="networkSettings">Domain model instance</param>
        /// <param name="view">User interface instance</param>
        public NetworkSettingsController(NetworkSettings networkSettings, INetworkSettingsView view)
        {
            this.networkSettings = networkSettings;
            this.view = view;
            this.stateMachine = new StateMachine(view, this.networkSettings, StateMachineMode.Read);
        }

        /// <summary>
        ///     Reads proxy mode and credentials settings from storage and sets UI to read values.
        /// </summary>
        public void Start()
        {
            this.stateMachine.Trigger(this.networkSettings.ProxyMode);
            this.started = true;
        }

        /// <summary>
        ///     Goto proxy mode specified
        /// </summary>
        /// <param name="mode"></param>
        public void Goto(ProxyMode mode)
        {
            if (this.started)
            {
                this.stateMachine.Trigger(mode);
            }
        }

        /// <summary>
        ///     Writes all proxy settings into storage.
        /// </summary>
        public void Write()
        {
            SafeRunner.Run(this.WriteUnsafe);
        }

        private void WriteUnsafe()
        {
            var sm = new StateMachine(this.view, this.networkSettings, StateMachineMode.Write);
            sm.Trigger(this.view.ProxyMode);
        }
    }
}