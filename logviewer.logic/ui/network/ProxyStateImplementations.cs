// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Base proxy state class
    /// </summary>
    internal abstract class ProxyState : SolidState
    {
        protected ProxyState(INetworkSettingsViewModel viewModel, IOptionsProvider provider)
        {
            this.ViewModel = viewModel;
            this.Provider = provider;
        }

        protected INetworkSettingsViewModel ViewModel { get; }

        protected IOptionsProvider Provider { get; }

        protected abstract ProxyMode Mode { get; }

        protected override void DoEntering(object context)
        {
            var mode = (StateMachineMode) context;
            if (mode == StateMachineMode.Read)
            {
                this.ViewModel.ProxyMode = this.Mode;
                this.Read();
            }
            else
            {
                this.Provider.UpdateIntegerOption(Constants.ProxyModeProperty, (int)this.Mode);
                this.Write();
            }
        }

        protected virtual void Read()
        {
            this.ViewModel.Host = string.Empty;
            this.ViewModel.Port = 0;

            this.ViewModel.UserName = string.Empty;
            this.ViewModel.Password = string.Empty;
            this.ViewModel.Domain = string.Empty;
        }

        protected virtual void Write()
        {
        }
    }

    /// <summary>
    ///     Auto detect proxy state
    /// </summary>
    internal class AutoProxyState : ProxyState
    {
        public AutoProxyState(INetworkSettingsViewModel viewModel, IOptionsProvider provider) : base(viewModel, provider)
        {
        }

        protected override ProxyMode Mode => ProxyMode.AutoProxyDetection;
    }

    /// <summary>
    ///     Dont use proxy state
    /// </summary>
    internal class NoProxyState : ProxyState
    {
        public NoProxyState(INetworkSettingsViewModel viewModel, IOptionsProvider provider) : base(viewModel, provider)
        {
        }

        protected override ProxyMode Mode => ProxyMode.None;
    }

    /// <summary>
    ///     Fake state
    /// </summary>
    internal class StartState : SolidState
    {
    }
}