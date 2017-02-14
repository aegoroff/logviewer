// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

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

        public ManualProxyStateCustomUser(INetworkSettingsViewModel viewModel, IOptionsProvider provider) : base(viewModel, provider)
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

            login = this.ViewModel.UserName;
            password = this.crypt.Encrypt(this.ViewModel.Password);
            domain = this.ViewModel.Domain;
        }

        protected override ProxyMode Mode => ProxyMode.Custom;

        protected override void Read()
        {
            base.Read();

            this.ViewModel.UserName = login ?? this.Provider.ReadStringOption(Constants.LoginProperty);
            this.ViewModel.Password = this.crypt.Decrypt(password ?? this.Provider.ReadStringOption(Constants.PasswordProperty));
            this.ViewModel.Domain = domain ?? this.Provider.ReadStringOption(Constants.DomainProperty);
        }

        protected override void Write()
        {
            if (!this.WriteProxyCommonSettings())
            {
                return;
            }

            this.Provider.UpdateStringOption(Constants.LoginProperty, this.ViewModel.UserName);

            var encrypted = this.crypt.Encrypt(this.ViewModel.Password);
            this.Provider.UpdateStringOption(Constants.PasswordProperty, encrypted);

            this.Provider.UpdateStringOption(Constants.DomainProperty, this.ViewModel.Domain);
        }
    }
}