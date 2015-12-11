// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Manual proxy address setting but credentials from process default credentials
    /// </summary>
    internal class ManualProxyStateDefaultUser : ProxyState
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public ManualProxyStateDefaultUser(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void Read()
        {
            this.view.ProxyMode = ProxyMode.Custom;
            this.view.EnableProxySettings(true);
            this.view.EnableCredentialsSettings(false);

            this.view.Host = this.networkSettings.Host;
            this.view.Port = this.networkSettings.Port;
        }

        protected override void Write()
        {
            this.networkSettings.ProxyMode = ProxyMode.Custom;

            if (string.IsNullOrWhiteSpace(this.view.Host) || this.view.Port <= 0)
            {
                Log.Instance.ErrorFormatted("Bad proxy settings - host: {0} and port: {1}", this.view.Host, this.view.Port);
                return;
            }

            this.networkSettings.Host = this.view.Host;
            this.networkSettings.Port = this.view.Port;
            this.networkSettings.IsUseDefaultCredentials = this.view.IsUseDefaultCredentials;
        }
    }

    /// <summary>
    ///     All manual proxy settings including credentials
    /// </summary>
    internal class ManualProxyStateCustomUser : ManualProxyStateDefaultUser
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public ManualProxyStateCustomUser(INetworkSettingsView view, NetworkSettings networkSettings) : base(view, networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void Read()
        {
            this.view.ProxyMode = ProxyMode.Custom;
            this.view.EnableProxySettings(true);
            this.view.EnableCredentialsSettings(true);

            this.view.Host = this.networkSettings.Host;
            this.view.Port = this.networkSettings.Port;

            this.view.UserName = this.networkSettings.UserName;
            this.view.Password = this.networkSettings.Password;
            this.view.Domain = this.networkSettings.Domain;
        }

        protected override void Write()
        {
            base.Write();

            this.networkSettings.UserName = this.view.UserName;
            this.networkSettings.Password = this.view.Password;
            this.networkSettings.Domain = this.view.Domain;
        }
    }
}