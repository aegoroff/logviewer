// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Manual proxy address setting but credentials from process default credentials
    /// </summary>
    internal class ManualProxyStateDefaultUser : ManualProxyState
    {
        public ManualProxyStateDefaultUser(INetworkSettingsViewModel viewModel, IOptionsProvider provider) : base(viewModel, provider)
        {
        }

        protected override ProxyMode Mode => ProxyMode.Custom;

        protected override void Read()
        {
            base.Read();

            this.ViewModel.UserName = string.Empty;
            this.ViewModel.Password = string.Empty;
            this.ViewModel.Domain = string.Empty;
        }

        protected override void Write()
        {
            this.WriteProxyCommonSettings();
        }
    }
}