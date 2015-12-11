using logviewer.logic.fsm;
using logviewer.logic.models;

namespace logviewer.logic.ui.network
{
    public class ManualProxyStateCustomUser : SolidState
    {
        private readonly NetworkSettings networkSettings;
        private readonly INetworkSettingsView view;

        public ManualProxyStateCustomUser(INetworkSettingsView view, NetworkSettings networkSettings)
        {
            this.view = view;
            this.networkSettings = networkSettings;
        }

        protected override void DoEntering(object context)
        {
            this.view.ProxyMode = ProxyMode.Custom;
            this.view.EnableProxySettings(true);
            this.view.EnableCredentialsSettings(true);

            this.view.Host = this.networkSettings.Host;
            this.view.Port = this.networkSettings.Port;

            this.view.UserName = this.networkSettings.UserName;
            this.view.Password = this.networkSettings.Password;
            this.view.Domain = this.networkSettings.Domain;
        }
    }
}