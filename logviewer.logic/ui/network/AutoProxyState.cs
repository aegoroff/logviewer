// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.logic.ui.network
{
    internal class AutoProxyState : ProxyState
    {
        private readonly INetworkSettingsView view;
        private readonly NetworkSettings networkSettings;

        public AutoProxyState(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void Read()
        {
            this.view.ProxyMode = ProxyMode.AutoProxyDetection;
            this.view.EnableProxySettings(false);
        }

        protected override void Write()
        {
            this.networkSettings.ProxyMode = ProxyMode.AutoProxyDetection;
        }
    }
}