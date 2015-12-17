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
        private readonly INetworkSettingsModel model;
        private readonly IOptionsProvider provider;
        private readonly StateMachine stateMachine;

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="model">Domain model instance</param>
        /// <param name="provider">Options provider instance</param>
        public NetworkSettingsController(INetworkSettingsModel model, IOptionsProvider provider)
        {
            this.model = model;
            this.provider = provider;
            this.stateMachine = new StateMachine(model, provider, StateMachineMode.Read);
        }

        /// <summary>
        ///     Reads proxy mode and credentials settings from storage and sets UI to read values.
        /// </summary>
        public void Initialize()
        {
            // TODO: think over about async read
            var mode = (ProxyMode)this.provider.ReadIntegerOption(Constants.ProxyModeProperty, (int)ProxyMode.AutoProxyDetection);
            var useDefalutCredentials = this.provider.ReadBooleanOption(Constants.IsUseDefaultCredentialsProperty, true);
            this.model.Initialize(mode, useDefalutCredentials);
            this.model.ModeChanged += this.OnModeChanged;
            this.stateMachine.Trigger(this.model.ProxyMode);
        }

        private void OnModeChanged(object sender, ProxyMode e)
        {
            this.stateMachine.Trigger(e);
            this.InvokeSettingsChange();
        }

        public void InvokeSettingsChange()
        {
            if (!this.model.IsSettingsChanged)
            {
                this.model.IsSettingsChanged = true;
            }
        }

        /// <summary>
        ///     Writes all proxy settings into storage.
        /// </summary>
        public void Write(string password)
        {
            this.model.Password = password;
            SafeRunner.Run(this.WriteUnsafe);
        }

        private void WriteUnsafe()
        {
            var sm = new StateMachine(this.model, this.provider, StateMachineMode.Write);
            sm.Trigger(this.model.ProxyMode);
        }
    }
}