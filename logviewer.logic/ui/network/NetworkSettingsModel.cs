// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 05.09.2007
// © 2007-2017 Alexander Egorov

using logviewer.logic.support;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represent business logic of network settings dialog
    /// </summary>
    public class NetworkSettingsModel
    {
        private readonly INetworkSettingsViewModel viewModel;

        private readonly ISimpleOptionsStore store;

        private readonly NetworkWorflow stateMachine;

        private AsymCrypt crypt;

        /// <summary>
        ///     Creates new controller class
        /// </summary>
        /// <param name="viewModel">Domain model instance</param>
        /// <param name="store">Options store instance</param>
        public NetworkSettingsModel(INetworkSettingsViewModel viewModel, ISimpleOptionsStore store)
        {
            this.viewModel = viewModel;
            this.store = store;
            this.InitCrypter();
            this.stateMachine = new NetworkWorflow(viewModel, store, this.crypt, StateMachineMode.Read);
        }

        /// <summary>
        ///     Reads proxy mode and credentials settings from storage and sets UI to read values.
        /// </summary>
        public void Initialize()
        {
            // TODO: think over about async read
            var mode = (ProxyMode)this.store.ReadIntegerOption(Constants.ProxyModeProperty, (int)ProxyMode.AutoProxyDetection);
            var useDefalutCredentials = this.store.ReadBooleanOption(Constants.IsUseDefaultCredentialsProperty, true);
            this.viewModel.Initialize(mode, useDefalutCredentials);
            this.viewModel.ModeChanged += this.OnModeChanged;
            var transition = this.GetTransition();
            this.stateMachine.Trigger(transition);
            if (transition == ProxyModeTransition.ToCustom)
            {
                this.stateMachine.Trigger(useDefalutCredentials
                                              ? ProxyModeTransition.ToCustomDefaultUser
                                              : ProxyModeTransition.ToCustomWithManualUser);
            }
        }

        private void OnModeChanged(object sender, ProxyModeTransition e)
        {
            this.stateMachine.Trigger(e);
            this.InvokeSettingsChange();
        }

        public void InvokeSettingsChange()
        {
            if (!this.viewModel.IsSettingsChanged)
            {
                this.viewModel.IsSettingsChanged = true;
            }
        }

        /// <summary>
        ///     Writes all proxy settings into storage.
        /// </summary>
        public void Write(string password)
        {
            this.viewModel.Password = password;
            SafeRunner.Run(this.WriteUnsafe);
        }

        private void WriteUnsafe()
        {
            var sm = new NetworkWorflow(this.viewModel, this.store, this.crypt, StateMachineMode.Write);
            sm.Trigger(this.GetTransition());
        }

        private ProxyModeTransition GetTransition() => ToTransition(this.viewModel.ProxyMode);

        private static ProxyModeTransition ToTransition(ProxyMode mode)
        {
            switch (mode)
            {
                case ProxyMode.None:
                    return ProxyModeTransition.ToNone;
                case ProxyMode.AutoProxyDetection:
                    return ProxyModeTransition.ToAutoProxyDetection;
                case ProxyMode.Custom:
                    return ProxyModeTransition.ToCustom;
                default:
                    return ProxyModeTransition.ToNone;
            }
        }

        private void InitCrypter()
        {
            var privateKey = this.store.ReadStringOption(Constants.PrivateKey);
            var publicKey = this.store.ReadStringOption(Constants.PublicKey);
            if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(publicKey))
            {
                this.crypt = new AsymCrypt();
                this.crypt.GenerateKeys();
                this.store.UpdateStringOption(Constants.PrivateKey, this.crypt.PrivateKey);
                this.store.UpdateStringOption(Constants.PublicKey, this.crypt.PublicKey);
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
    }
}
