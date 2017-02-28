using System;
using logviewer.logic.support;
using Stateless;

namespace logviewer.logic.ui.network
{
    internal class NetworkWorflow
    {
        private static string host;
        private static int port;
        private static string login;
        private static string password;
        private static string domain;
        private readonly StateMachine<ProxyMode, ProxyMode> machine;
        private readonly IOptionsProvider provider;
        private readonly INetworkSettingsViewModel viewModel;
        private AsymCrypt crypt;
        private ProxyMode mode;

        public NetworkWorflow(INetworkSettingsViewModel viewModel, IOptionsProvider provider, StateMachineMode machineMode)
        {
            this.viewModel = viewModel;
            this.provider = provider;

            this.InitCrypter();

            this.machine = new StateMachine<ProxyMode, ProxyMode>(() => this.mode, s => this.mode = s);

            this.machine.Configure(ProxyMode.None)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => { }))
                .Permit(ProxyMode.AutoProxyDetection, ProxyMode.AutoProxyDetection)
                .Permit(ProxyMode.Custom, ProxyMode.Custom)
                .PermitReentry(ProxyMode.None);

            this.machine.Configure(ProxyMode.AutoProxyDetection)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => { }))
                .Permit(ProxyMode.None, ProxyMode.None)
                .Permit(ProxyMode.Custom, ProxyMode.Custom)
                .PermitReentry(ProxyMode.AutoProxyDetection);

            this.machine.Configure(ProxyMode.Custom)
                .OnEntry(t => this.OnEnter(machineMode, this.ReadOnEnterIntoCustom, this.WriteOnEnterIntoCustom))
                .OnExit(this.OnExitCustom)
                .Permit(ProxyMode.None, ProxyMode.None)
                .Permit(ProxyMode.AutoProxyDetection, ProxyMode.AutoProxyDetection)
                .PermitReentry(ProxyMode.Custom);
        }

        public void Trigger(ProxyMode proxyMode)
        {
            this.machine.Fire(proxyMode);
        }

        private void InitCrypter()
        {
            var privateKey = this.provider.ReadStringOption(Constants.PrivateKey);
            var publicKey = this.provider.ReadStringOption(Constants.PublicKey);
            if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(publicKey))
            {
                this.crypt = new AsymCrypt();
                this.crypt.GenerateKeys();
                this.provider.UpdateStringOption(Constants.PrivateKey, this.crypt.PrivateKey);
                this.provider.UpdateStringOption(Constants.PublicKey, this.crypt.PublicKey);
            }
            else
            {
                this.crypt = new AsymCrypt
                {
                    PublicKey = publicKey,
                    PrivateKey = privateKey
                };
            }
        }

        private void OnEnter(StateMachineMode machineMode, Action readAction, Action writeAction)
        {
            if (machineMode == StateMachineMode.Read)
            {
                this.viewModel.ProxyMode = this.mode;
                readAction();
            }
            else
            {
                this.provider.UpdateIntegerOption(Constants.ProxyModeProperty, (int) this.mode);
                writeAction();
            }
        }

        private void OnExitCustom()
        {
            host = this.viewModel.Host;
            port = this.viewModel.Port;

            if (!this.viewModel.IsUseDefaultCredentials)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.viewModel.UserName))
            {
                login = this.viewModel.UserName;
            }

            if (this.viewModel.Password != null)
            {
                password = this.crypt.Encrypt(this.viewModel.Password);
            }

            if (!string.IsNullOrWhiteSpace(this.viewModel.Domain))
            {
                domain = this.viewModel.Domain;
            }
        }

        private void Read()
        {
            this.viewModel.Host = null;
            this.viewModel.Port = 0;

            this.viewModel.UserName = null;
            this.viewModel.Password = null;
            this.viewModel.Domain = null;
        }

        private void ReadOnEnterIntoCustom()
        {
            this.viewModel.Host = host ?? this.provider.ReadStringOption(Constants.HostProperty);
            this.viewModel.Port = port > 0 ? port : this.provider.ReadIntegerOption(Constants.PortProperty);

            if (this.viewModel.IsUseDefaultCredentials)
            {
                this.viewModel.UserName = null;
                this.viewModel.Password = null;
                this.viewModel.Domain = null;
            }
            else
            {
                this.viewModel.UserName = login ?? this.provider.ReadStringOption(Constants.LoginProperty);
                this.viewModel.Password = this.crypt.Decrypt(password ?? this.provider.ReadStringOption(Constants.PasswordProperty));
                this.viewModel.Domain = domain ?? this.provider.ReadStringOption(Constants.DomainProperty);
            }
        }

        private void WriteOnEnterIntoCustom()
        {
            if (this.viewModel.IsUseDefaultCredentials)
            {
                this.WriteProxyCommonSettings();
            }
            else
            {
                this.WriteProxyCustomUserSettings();
            }
        }

        private bool WriteProxyCommonSettings()
        {
            if (string.IsNullOrWhiteSpace(this.viewModel.Host) || this.viewModel.Port <= 0)
            {
                Log.Instance.ErrorFormatted("Bad proxy settings - host: {0} and port: {1}", this.viewModel.Host, this.viewModel.Port);
                return false;
            }

            this.provider.UpdateStringOption(Constants.HostProperty, this.viewModel.Host);
            this.provider.UpdateIntegerOption(Constants.PortProperty, this.viewModel.Port);
            this.provider.UpdateBooleanOption(Constants.IsUseDefaultCredentialsProperty, this.viewModel.IsUseDefaultCredentials);

            return true;
        }

        private void WriteProxyCustomUserSettings()
        {
            if (!this.WriteProxyCommonSettings())
            {
                return;
            }

            this.provider.UpdateStringOption(Constants.LoginProperty, this.viewModel.UserName);

            var encrypted = this.crypt.Encrypt(this.viewModel.Password);
            this.provider.UpdateStringOption(Constants.PasswordProperty, encrypted);

            this.provider.UpdateStringOption(Constants.DomainProperty, this.viewModel.Domain);
        }
    }
}