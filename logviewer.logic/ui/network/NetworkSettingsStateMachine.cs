﻿// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;
using logviewer.logic.models;

namespace logviewer.logic.ui.network
{
    public class NetworkSettingsStateMachine : SolidMachine<ProxyMode>
    {
        public NetworkSettingsStateMachine(INetworkSettingsView view, NetworkSettings networkSettings, object context = null)
            : base(context, new NetworkStateResolver(view, networkSettings))
        {
            this.State<StartState>()
                .IsInitialState()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials).GoesTo<ManualProxyStateCurrentUser>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<NoProxyState>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials).GoesTo<ManualProxyStateCurrentUser>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<AutoProxyState>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials).GoesTo<ManualProxyStateCurrentUser>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<ManualProxyStateCurrentUser>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials).GoesTo<ManualProxyStateCurrentUser>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.State<ManualProxyStateCustomUser>()
                .On(ProxyMode.None).GoesTo<NoProxyState>()
                .On(ProxyMode.AutoProxyDetection).GoesTo<AutoProxyState>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials).GoesTo<ManualProxyStateCurrentUser>()
                .On(ProxyMode.Custom, () => view.IsUseDefaultCredentials == false).GoesTo<ManualProxyStateCustomUser>();

            this.Start();
        }
    }
}