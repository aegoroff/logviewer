// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    public class AutoProxyState : SolidState
    {
        private readonly INetworkSettingsView view;

        public AutoProxyState(INetworkSettingsView view)
        {
            this.view = view;
        }

        protected override void DoEntering(object context)
        {
            this.view.ProxyMode = ProxyMode.AutoProxyDetection;
            this.view.EnableProxySettings(false);
        }
    }
}