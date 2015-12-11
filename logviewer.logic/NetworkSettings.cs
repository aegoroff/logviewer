// Created by: egr
// Created at: 04.09.2007
// © 2007-2015 Alexander Egorov

using System.Net;

namespace logviewer.logic
{
    public sealed class NetworkSettings
    {
        private const string IsUseProxyProperty = @"IsUseProxy";
        private const string IsUseIeProxyProperty = @"IsUseIeProxy";
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

        public bool IsUseProxy
        {
            get { return this.optionsProvider.ReadBooleanOption(IsUseProxyProperty); }
            set { this.optionsProvider.UpdateBooleanOption(IsUseProxyProperty, value); }
        }

        public bool IsUseIeProxy
        {
            get { return this.optionsProvider.ReadBooleanOption(IsUseIeProxyProperty, true); }
            set { this.optionsProvider.UpdateBooleanOption(IsUseIeProxyProperty, value); }
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


        public WebProxy Proxy => this.GetProxy();

        private WebProxy GetProxy()
        {
            if (!this.IsUseProxy)
            {
                return null;
            }
            var result = new WebProxy(this.Host, this.Port);
            if (!this.IsUseDefaultCredentials)
            {
                result.Credentials = new NetworkCredential(this.UserName, this.Password, this.Domain);
            }
            return result;
        }

        public static void SetProxy(IOptionsProvider optionsProvider)
        {
            var settings = new NetworkSettings(optionsProvider);
            if (!settings.IsUseIeProxy)
            {
                WebRequest.DefaultWebProxy = settings.Proxy;
            }
        }
    }
}