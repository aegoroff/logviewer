// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represent business logic of network settings dialog
    /// </summary>
    public class NetworkSettingsModel
    {
        private readonly INetworkSettingsViewModel viewModel;
        private readonly IOptionsProvider provider;
        private readonly NetworkWorflow stateMachine;

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="viewModel">Domain model instance</param>
        /// <param name="provider">Options provider instance</param>
        public NetworkSettingsModel(INetworkSettingsViewModel viewModel, IOptionsProvider provider)
        {
            this.viewModel = viewModel;
            this.provider = provider;
            this.stateMachine = new NetworkWorflow(viewModel, provider, StateMachineMode.Read);
        }

        /// <summary>
        ///     Reads proxy mode and credentials settings from storage and sets UI to read values.
        /// </summary>
        public void Initialize()
        {
            // TODO: think over about async read
            var mode = (ProxyMode)this.provider.ReadIntegerOption(Constants.ProxyModeProperty, (int)ProxyMode.AutoProxyDetection);
            var useDefalutCredentials = this.provider.ReadBooleanOption(Constants.IsUseDefaultCredentialsProperty, true);
            this.viewModel.Initialize(mode, useDefalutCredentials);
            this.viewModel.ModeChanged += this.OnModeChanged;
            this.stateMachine.Trigger(this.viewModel.ProxyMode);
        }

        private void OnModeChanged(object sender, ProxyMode e)
        {
            this.stateMachine.Trigger(e);
            this.InvokeSettingsChange();
        }

        public void InvokeSettingsChange()
        {
            if (!this.viewModel.IsSettingsChanged)
            {
                this.viewModel.IsSettingsChanged = true;
            }
        }

        /// <summary>
        ///     Writes all proxy settings into storage.
        /// </summary>
        public void Write(string password)
        {
            this.viewModel.Password = password;
            SafeRunner.Run(this.WriteUnsafe);
        }

        private void WriteUnsafe()
        {
            var sm = new NetworkWorflow(this.viewModel, this.provider, StateMachineMode.Write);
            sm.Trigger(this.viewModel.ProxyMode);
        }
    }
}