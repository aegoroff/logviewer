// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov


using logviewer.logic.models;
using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represents all possible proxy modes.
    /// </summary>
    public enum ProxyMode
    {
        /// <summary>
        ///     Do not use proxy
        /// </summary>
        None,

        /// <summary>
        ///     Automatic proxy detection
        /// </summary>
        AutoProxyDetection,

        /// <summary>
        ///     Manual proxy settings
        /// </summary>
        Custom
    }

    /// <summary>
    ///     Represent business logic of network settings dialog
    /// </summary>
    public class NetworkSettingsController
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView ui;
        private readonly NetworkSettingsStateMachine stateMachine;
        private bool started;

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="networkSettings">Domain model instance</param>
        /// <param name="ui">User interface instance</param>
        public NetworkSettingsController(NetworkSettings networkSettings, INetworkSettingsView ui)
        {
            this.networkSettings = networkSettings;
            this.ui = ui;
            this.stateMachine = new NetworkSettingsStateMachine(ui, this.networkSettings);
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
        /// Goto proxy mode specified
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
            this.networkSettings.ProxyMode = this.ui.ProxyMode;
            if (!this.ui.IsUseProxy || this.ui.IsUseAutoProxy)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(this.ui.Host) || this.ui.Port == 0)
            {
                Log.Instance.ErrorFormatted("Bad proxy settings - host: {0} and port: {1}", this.ui.Host, this.ui.Port);
                return;
            }
            this.networkSettings.Host = this.ui.Host;
            this.networkSettings.Port = this.ui.Port;
            this.networkSettings.IsUseDefaultCredentials = this.ui.IsUseDefaultCredentials;
            if (this.ui.IsUseDefaultCredentials)
            {
                return;
            }
            this.networkSettings.UserName = this.ui.UserName;
            this.networkSettings.Password = this.ui.Password;
            this.networkSettings.Domain = this.ui.Domain;
        }
    }
}