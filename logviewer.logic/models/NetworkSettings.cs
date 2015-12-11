// Created by: egr
// Created at: 04.09.2007
// © 2007-2015 Alexander Egorov

using System.Net;
using logviewer.logic.support;
using logviewer.logic.ui.network;

namespace logviewer.logic.models
{
    public sealed class NetworkSettings
    {
        private const string ProxyModeProperty = @"ProxyMode";
        private const string IsUseDefaultCredentialsProperty = @"IsUseDefaultCredentials";
        private const string HostProperty = @"Host";
        private const string PortProperty = @"Port";
        private const string LoginProperty = @"Login";
        private const string PasswordProperty = @"Password";
        private const string DomainProperty = @"Domain";

        private const string PrivateKey = @"PrivateKey";
        private const string PublicKey = @"PublicKey";
        private readonly AsymCrypt crypt;
        private readonly IOptionsProvider optionsProvider;

        public NetworkSettings(IOptionsProvider optionsProvider)
        {
            this.optionsProvider = optionsProvider;

            var privateKey = this.optionsProvider.ReadStringOption(PrivateKey);
            var publicKey = this.optionsProvider.ReadStringOption(PublicKey);
            if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(publicKey))
            {
                this.crypt = new AsymCrypt();
                this.crypt.GenerateKeys();
                this.optionsProvider.UpdateStringOption(PrivateKey, this.crypt.PrivateKey);
                this.optionsProvider.UpdateStringOption(PublicKey, this.crypt.PublicKey);
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

        public ProxyMode ProxyMode
        {
            get { return (ProxyMode) this.optionsProvider.ReadIntegerOption(ProxyModeProperty, (int) ProxyMode.AutoProxyDetection); }
            set { this.optionsProvider.UpdateIntegerOption(ProxyModeProperty, (int) value); }
        }

        public bool IsUseDefaultCredentials
        {
            get { return this.optionsProvider.ReadBooleanOption(IsUseDefaultCredentialsProperty, true); }
            set { this.optionsProvider.UpdateBooleanOption(IsUseDefaultCredentialsProperty, value); }
        }

        public string Host
        {
            get { return this.optionsProvider.ReadStringOption(HostProperty); }
            set { this.optionsProvider.UpdateStringOption(HostProperty, value); }
        }

        public int Port
        {
            get { return this.optionsProvider.ReadIntegerOption(PortProperty); }
            set { this.optionsProvider.UpdateIntegerOption(PortProperty, value); }
        }

        public string UserName
        {
            get { return this.optionsProvider.ReadStringOption(LoginProperty); }
            set { this.optionsProvider.UpdateStringOption(LoginProperty, value); }
        }

        public string Password
        {
            get
            {
                return string.IsNullOrEmpty(this.optionsProvider.ReadStringOption(PasswordProperty))
                    ? null
                    : this.crypt.Decrypt(this.optionsProvider.ReadStringOption(PasswordProperty));
            }
            set
            {
                var encrypted = this.crypt.Encrypt(value);
                this.optionsProvider.UpdateStringOption(PasswordProperty, encrypted);
            }
        }

        public string Domain
        {
            get { return this.optionsProvider.ReadStringOption(DomainProperty); }
            set { this.optionsProvider.UpdateStringOption(DomainProperty, value); }
        }


        public IWebProxy Proxy => this.GetProxy();

        private IWebProxy GetProxy()
        {
            switch (this.ProxyMode)
            {
                case ProxyMode.None:
                    return null;
                case ProxyMode.AutoProxyDetection:
                    var proxy = WebRequest.GetSystemWebProxy();
                    proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    return proxy;
                case ProxyMode.Custom:
                    return new WebProxy(this.Host, this.Port)
                    {
                        Credentials =
                            this.IsUseDefaultCredentials
                                ? CredentialCache.DefaultNetworkCredentials
                                : new NetworkCredential(this.UserName, this.Password, this.Domain)
                    };
                default:
                    return null;
            }
        }

        public static void SetProxy(IOptionsProvider optionsProvider)
        {
            var settings = new NetworkSettings(optionsProvider);
            WebRequest.DefaultWebProxy = settings.Proxy;
        }
    }
}