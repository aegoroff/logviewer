// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using System;
using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    internal class NetworkStateResolver : IStateResolver
    {
        private readonly INetworkSettingsModel model;
        private readonly IOptionsProvider provider;

        public NetworkStateResolver(INetworkSettingsModel model, IOptionsProvider provider)
        {
            this.model = model;
            this.provider = provider;
        }

        public ISolidState ResolveState(Type stateType)
        {
            if (stateType == typeof (NoProxyState))
            {
                return new NoProxyState(this.model, this.provider);
            }
            if (stateType == typeof (AutoProxyState))
            {
                return new AutoProxyState(this.model, this.provider);
            }
            if (stateType == typeof (ManualProxyStateDefaultUser))
            {
                return new ManualProxyStateDefaultUser(this.model, this.provider);
            }
            if (stateType == typeof (ManualProxyStateCustomUser))
            {
                return new ManualProxyStateCustomUser(this.model, this.provider);
            }
            return new StartState();
        }
    }
}