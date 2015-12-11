// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov


namespace logviewer.logic
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

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="networkSettings">Domain model instance</param>
        /// <param name="ui">User interface instance</param>
        public NetworkSettingsController(NetworkSettings networkSettings, INetworkSettingsView ui)
        {
            this.networkSettings = networkSettings;
            this.ui = ui;
        }

        /// <summary>
        ///     Reads proxy mode and credentials settings from storage and sets UI to read values.
        /// </summary>
        public void ReadMain()
        {
            if (this.networkSettings.IsUseProxy)
            {
                this.ui.ProxyMode = ProxyMode.Custom;
                this.ui.IsUseDefaultCredentials = this.networkSettings.IsUseDefaultCredentials;
            }
            else if (this.networkSettings.IsUseIeProxy)
            {
                this.ui.ProxyMode = ProxyMode.AutoProxyDetection;
            }
            else
            {
                this.ui.ProxyMode = ProxyMode.None;
            }
        }

        /// <summary>
        ///     Reads authentification settings and sets UI to read values
        /// </summary>
        public void ReadAuthSettings()
        {
            if (this.networkSettings.IsUseDefaultCredentials)
            {
                return;
            }
            this.ui.UserName = this.networkSettings.UserName;
            this.ui.Password = this.networkSettings.Password;
            this.ui.Domain = this.networkSettings.Domain;
        }

        /// <summary>
        ///     Reads host and port from storage and sets UI to the values.
        /// </summary>
        public void ReadHostAndPort()
        {
            if (!this.networkSettings.IsUseProxy)
            {
                return;
            }
            this.ui.Host = this.networkSettings.Host;
            this.ui.Port = this.networkSettings.Port;
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
            this.networkSettings.IsUseProxy = this.ui.IsUseProxy;
            this.networkSettings.IsUseIeProxy = this.ui.IsUseIeProxy;
            if (!this.ui.IsUseProxy || this.ui.IsUseIeProxy)
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