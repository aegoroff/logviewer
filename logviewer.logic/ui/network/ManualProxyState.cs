// Created by: egr
// Created at: 16.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    internal abstract class ManualProxyState : ProxyState
    {
        private static string host;
        private static int port;

        protected ManualProxyState(INetworkSettingsModel model, IOptionsProvider provider) : base(model, provider)
        {
        }

        protected override void DoExiting(object context)
        {
            host = this.Model.Host;
            port = this.Model.Port;
        }

        protected override void Read()
        {
            this.Model.Host = host ?? this.Provider.ReadStringOption(Constants.HostProperty);
            this.Model.Port = port > 0 ? port : this.Provider.ReadIntegerOption(Constants.PortProperty);
        }

        protected bool WriteProxyCommonSettings()
        {
            if (string.IsNullOrWhiteSpace(this.Model.Host) || this.Model.Port <= 0)
            {
                Log.Instance.ErrorFormatted("Bad proxy settings - host: {0} and port: {1}", this.Model.Host, this.Model.Port);
                return false;
            }

            this.Provider.UpdateStringOption(Constants.HostProperty, this.Model.Host);
            this.Provider.UpdateIntegerOption(Constants.PortProperty, this.Model.Port);
            this.Provider.UpdateBooleanOption(Constants.IsUseDefaultCredentialsProperty, this.Model.IsUseDefaultCredentials);

            return true;
        }
    }
}