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

        protected ManualProxyState(INetworkSettingsViewModel viewModel, IOptionsProvider provider) : base(viewModel, provider)
        {
        }

        protected override void DoExiting(object context)
        {
            host = this.ViewModel.Host;
            port = this.ViewModel.Port;
        }

        protected override void Read()
        {
            this.ViewModel.Host = host ?? this.Provider.ReadStringOption(Constants.HostProperty);
            this.ViewModel.Port = port > 0 ? port : this.Provider.ReadIntegerOption(Constants.PortProperty);
        }

        protected bool WriteProxyCommonSettings()
        {
            if (string.IsNullOrWhiteSpace(this.ViewModel.Host) || this.ViewModel.Port <= 0)
            {
                Log.Instance.ErrorFormatted("Bad proxy settings - host: {0} and port: {1}", this.ViewModel.Host, this.ViewModel.Port);
                return false;
            }

            this.Provider.UpdateStringOption(Constants.HostProperty, this.ViewModel.Host);
            this.Provider.UpdateIntegerOption(Constants.PortProperty, this.ViewModel.Port);
            this.Provider.UpdateBooleanOption(Constants.IsUseDefaultCredentialsProperty, this.ViewModel.IsUseDefaultCredentials);

            return true;
        }
    }
}