// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.logic.ui.network
{
    internal class NoProxyState : ProxyState
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public NoProxyState(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void Read()
        {
            this.view.ProxyMode = ProxyMode.None;
            this.view.EnableProxySettings(false);
        }

        protected override void Write()
        {
            this.networkSettings.ProxyMode = ProxyMode.None;
        }
    }
}