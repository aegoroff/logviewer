// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.02.2017
// © 2007-2017 Alexander Egorov

using System;
using logviewer.logic.support;
using Stateless;

namespace logviewer.logic.ui.network
{
    internal class NetworkWorflow
    {
        private string host;
        private int port;
        private string login;
        private string password;
        private string domain;
        private readonly StateMachine<ProxyMode, ProxyModeTransition> machine;
        private readonly IOptionsProvider provider;
        private readonly INetworkSettingsViewModel viewModel;
        private readonly IAsymCrypt crypt;
        private ProxyMode mode;

        public NetworkWorflow(INetworkSettingsViewModel viewModel, IOptionsProvider provider, IAsymCrypt crypt, StateMachineMode machineMode)
        {
            this.viewModel = viewModel;
            this.provider = provider;
            this.crypt = crypt;

            this.machine = new StateMachine<ProxyMode, ProxyModeTransition>(() => this.mode, s => this.mode = s);

            this.machine.Configure(ProxyMode.None)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => { }))
                .Permit(ProxyModeTransition.ToAutoProxyDetection, ProxyMode.AutoProxyDetection)
                .Permit(ProxyModeTransition.ToCustom, ProxyMode.Custom)
                .PermitReentry(ProxyModeTransition.ToNone);

            this.machine.Configure(ProxyMode.AutoProxyDetection)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => { }))
                .Permit(ProxyModeTransition.ToNone, ProxyMode.None)
                .Permit(ProxyModeTransition.ToCustom, ProxyMode.Custom)
                .PermitReentry(ProxyModeTransition.ToAutoProxyDetection);

            this.machine.Configure(ProxyMode.Custom)
                .OnEntry(t => this.OnEnter(machineMode, this.ReadOnEnterIntoCustom, this.WriteOnEnterIntoCustom))
                .OnExit(this.OnExitCustom)
                .Permit(ProxyModeTransition.ToNone, ProxyMode.None)
                .Permit(ProxyModeTransition.ToAutoProxyDetection, ProxyMode.AutoProxyDetection)
                .PermitReentry(ProxyModeTransition.ToCustom)
                .InternalTransition(ProxyModeTransition.ToCustomDefaultUser, this.ReadOnEnterToCustomDefaultUser)
                .InternalTransition(ProxyModeTransition.ToCustomWithManualUser,this.ReadOnEnterToCustomManualUser);
        }

        public void Trigger(ProxyModeTransition transition)
        {
            this.machine.Fire(transition);
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
            this.host = this.viewModel.Host;
            this.port = this.viewModel.Port;

            this.login = this.viewModel.UserName;
            this.domain = this.viewModel.Domain;

            if (this.viewModel.Password != null)
            {
                this.password = this.crypt.Encrypt(this.viewModel.Password);
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
            this.viewModel.Host = this.host ?? this.provider.ReadStringOption(Constants.HostProperty);
            this.viewModel.Port = this.port > 0 ? this.port : this.provider.ReadIntegerOption(Constants.PortProperty);
        }

        private void ReadOnEnterToCustomManualUser(StateMachine<ProxyMode, ProxyModeTransition>.Transition transition)
        {
            this.viewModel.UserName = this.login ?? this.provider.ReadStringOption(Constants.LoginProperty);
            this.viewModel.Password = this.crypt.Decrypt(this.password ?? this.provider.ReadStringOption(Constants.PasswordProperty));
            this.viewModel.Domain = this.domain ?? this.provider.ReadStringOption(Constants.DomainProperty);
        }

        private void ReadOnEnterToCustomDefaultUser()
        {
            this.viewModel.UserName = null;
            this.viewModel.Password = null;
            this.viewModel.Domain = null;
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