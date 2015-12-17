// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    internal class StateMachine : SolidMachine<ProxyMode>
    {
        public StateMachine(INetworkSettingsModel model, IOptionsProvider provider, object context = null)
            : base(context, new NetworkStateResolver(model, provider))
        {
            this.State<StartState>()
                .IsInitialState()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<AutoProxyState>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials).GoesTo<ManualProxyStateDefaultUser>()
                .On(ProxyMode.Custom, () => model.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

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