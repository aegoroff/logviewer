// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    internal class StateMachine : SolidMachine<ProxyMode>
    {
        public StateMachine(INetworkSettingsViewModel viewModel, IOptionsProvider provider, object context = null)
            : base(context, new NetworkStateResolver(viewModel, provider))
        {
            this.State<StartState>()
                .IsInitialState()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<AutoProxyState>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => viewModel.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<ManualProxyStateDefaultUser>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom).GoesTo<ManualProxyStateCustomUser>();

            this.State<ManualProxyStateCustomUser>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom).GoesTo<ManualProxyStateDefaultUser>();

            this.Start();
        }
    }
}