﻿// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    public class ManualProxyStateDefaultUser : SolidState
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public ManualProxyStateDefaultUser(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void DoEntering(object context)
        {
            this.view.ProxyMode = ProxyMode.Custom;
            this.view.EnableProxySettings(true);
            this.view.EnableCredentialsSettings(false);

            this.view.Host = this.networkSettings.Host;
            this.view.Port = this.networkSettings.Port;
        }
    }
}