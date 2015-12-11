// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using System;
using logviewer.logic.fsm;
using logviewer.logic.models;

namespace logviewer.logic.ui.network
{
    internal class NetworkStateResolver : IStateResolver
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public NetworkStateResolver(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.networkSettings = networkSettings;
            this.view = view;
        }

        public ISolidState ResolveState(Type stateType)
        {
            if (stateType == typeof (NoProxyState))
            {
                return new NoProxyState(this.view);
            }
            if (stateType == typeof (AutoProxyState))
            {
                return new AutoProxyState(this.view);
            }
            if (stateType == typeof (ManualProxyStateCurrentUser))
            {
                return new ManualProxyStateCurrentUser(this.view, this.networkSettings);
            }
            if (stateType == typeof (ManualProxyStateCustomUser))
            {
                return new ManualProxyStateCustomUser(this.view, this.networkSettings);
            }
            return new StartState();
        }
    }
}