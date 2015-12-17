// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     All manual proxy settings including credentials
    /// </summary>
    internal class ManualProxyStateCustomUser : ManualProxyState
    {
        private readonly AsymCrypt crypt;
        private static string login;
        private static string password;
        private static string domain;

        public ManualProxyStateCustomUser(INetworkSettingsModel model, IOptionsProvider provider) : base(model, provider)
        {
            var privateKey = this.Provider.ReadStringOption(Constants.PrivateKey);
            var publicKey = this.Provider.ReadStringOption(Constants.PublicKey);
            if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(publicKey))
            {
                this.crypt = new AsymCrypt();
                this.crypt.GenerateKeys();
                this.Provider.UpdateStringOption(Constants.PrivateKey, this.crypt.PrivateKey);
                this.Provider.UpdateStringOption(Constants.PublicKey, this.crypt.PublicKey);
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

        protected override void DoExiting(object context)
        {
            base.DoExiting(context);

            login = this.Model.UserName;
            password = this.crypt.Encrypt(this.Model.Password);
            domain = this.Model.Domain;
        }

        protected override ProxyMode Mode => ProxyMode.Custom;

        protected override void Read()
        {
            base.Read();

            this.Model.UserName = login ?? this.Provider.ReadStringOption(Constants.LoginProperty);
            this.Model.Password = this.crypt.Decrypt(password ?? this.Provider.ReadStringOption(Constants.PasswordProperty));
            this.Model.Domain = domain ?? this.Provider.ReadStringOption(Constants.DomainProperty);
        }

        protected override void Write()
        {
            if (!this.WriteProxyCommonSettings())
            {
                return;
            }

            this.Provider.UpdateStringOption(Constants.LoginProperty, this.Model.UserName);

            var encrypted = this.crypt.Encrypt(this.Model.Password);
            this.Provider.UpdateStringOption(Constants.PasswordProperty, encrypted);

            this.Provider.UpdateStringOption(Constants.DomainProperty, this.Model.Domain);
        }
    }
}