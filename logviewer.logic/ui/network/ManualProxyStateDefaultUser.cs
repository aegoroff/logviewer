// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Manual proxy address setting but credentials from process default credentials
    /// </summary>
    internal class ManualProxyStateDefaultUser : ManualProxyState
    {
        public ManualProxyStateDefaultUser(INetworkSettingsModel model, IOptionsProvider provider) : base(model, provider)
        {
        }

        protected override ProxyMode Mode => ProxyMode.Custom;

        protected override void Read()
        {
            base.Read();

            this.Model.UserName = string.Empty;
            this.Model.Password = string.Empty;
            this.Model.Domain = string.Empty;
        }

        protected override void Write()
        {
            this.WriteProxyCommonSettings();
        }
    }
}