// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using System;
using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    internal class NetworkStateResolver : IStateResolver
    {
        private readonly INetworkSettingsViewModel viewModel;
        private readonly IOptionsProvider provider;

        public NetworkStateResolver(INetworkSettingsViewModel viewModel, IOptionsProvider provider)
        {
            this.viewModel = viewModel;
            this.provider = provider;
        }

        public ISolidState ResolveState(Type stateType)
        {
            if (stateType == typeof (NoProxyState))
            {
                return new NoProxyState(this.viewModel, this.provider);
            }
            if (stateType == typeof (AutoProxyState))
            {
                return new AutoProxyState(this.viewModel, this.provider);
            }
            if (stateType == typeof (ManualProxyStateDefaultUser))
            {
                return new ManualProxyStateDefaultUser(this.viewModel, this.provider);
            }
            if (stateType == typeof (ManualProxyStateCustomUser))
            {
                return new ManualProxyStateCustomUser(this.viewModel, this.provider);
            }
            return new StartState();
        }
    }
}