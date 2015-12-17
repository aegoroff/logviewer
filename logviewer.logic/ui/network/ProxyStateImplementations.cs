// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Base proxy state class
    /// </summary>
    internal abstract class ProxyState : SolidState
    {
        protected ProxyState(INetworkSettingsModel model, IOptionsProvider provider)
        {
            this.Model = model;
            this.Provider = provider;
        }

        protected INetworkSettingsModel Model { get; }

        protected IOptionsProvider Provider { get; }

        protected abstract ProxyMode Mode { get; }

        protected override void DoEntering(object context)
        {
            var mode = (StateMachineMode) context;
            if (mode == StateMachineMode.Read)
            {
                this.Model.ProxyMode = this.Mode;
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
            this.Model.Host = string.Empty;
            this.Model.Port = 0;

            this.Model.UserName = string.Empty;
            this.Model.Password = string.Empty;
            this.Model.Domain = string.Empty;
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
        public AutoProxyState(INetworkSettingsModel model, IOptionsProvider provider) : base(model, provider)
        {
        }

        protected override ProxyMode Mode => ProxyMode.AutoProxyDetection;
    }

    /// <summary>
    ///     Dont use proxy state
    /// </summary>
    internal class NoProxyState : ProxyState
    {
        public NoProxyState(INetworkSettingsModel model, IOptionsProvider provider) : base(model, provider)
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