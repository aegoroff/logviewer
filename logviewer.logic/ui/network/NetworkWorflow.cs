// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.02.2017
// © 2007-2017 Alexander Egorov

using System;
using logviewer.logic.storage;
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

        private readonly ISimpleOptionsStore store;

        private readonly INetworkSettingsViewModel viewModel;

        private readonly IAsymCrypt crypt;

        private ProxyMode mode;

        public NetworkWorflow(INetworkSettingsViewModel viewModel, ISimpleOptionsStore store, IAsymCrypt crypt,
                              StateMachineMode machineMode)
        {
            this.viewModel = viewModel;
            this.store = store;
            this.crypt = crypt;

            this.machine = new StateMachine<ProxyMode, ProxyModeTransition>(() => this.mode, s => this.mode = s);

            this.machine.Configure(ProxyMode.None)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => {}))
                .Permit(ProxyModeTransition.ToAutoProxyDetection, ProxyMode.AutoProxyDetection)
                .Permit(ProxyModeTransition.ToCustom, ProxyMode.Custom)
                .PermitReentry(ProxyModeTransition.ToNone);

            this.machine.Configure(ProxyMode.AutoProxyDetection)
                .OnEntry(t => this.OnEnter(machineMode, this.Read, () => {}))
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
                .InternalTransition(ProxyModeTransition.ToCustomWithManualUser, this.ReadOnEnterToCustomManualUser);
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
                this.store.UpdateIntegerOption(Constants.ProxyModeProperty, (int)this.mode);
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
            this.viewModel.Host = this.host ?? this.store.ReadStringOption(Constants.HostProperty);
            this.viewModel.Port = this.port > 0 ? this.port : this.store.ReadIntegerOption(Constants.PortProperty);
        }

        private void ReadOnEnterToCustomManualUser(StateMachine<ProxyMode, ProxyModeTransition>.Transition transition)
        {
            this.viewModel.UserName = this.login ?? this.store.ReadStringOption(Constants.LoginProperty);
            this.viewModel.Password = this.crypt.Decrypt(this.password ?? this.store.ReadStringOption(Constants.PasswordProperty));
            this.viewModel.Domain = this.domain ?? this.store.ReadStringOption(Constants.DomainProperty);
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

            this.store.UpdateStringOption(Constants.HostProperty, this.viewModel.Host);
            this.store.UpdateIntegerOption(Constants.PortProperty, this.viewModel.Port);
            this.store.UpdateBooleanOption(Constants.IsUseDefaultCredentialsProperty, this.viewModel.IsUseDefaultCredentials);

            return true;
        }

        private void WriteProxyCustomUserSettings()
        {
            if (!this.WriteProxyCommonSettings())
            {
                return;
            }

            this.store.UpdateStringOption(Constants.LoginProperty, this.viewModel.UserName);

            var encrypted = this.crypt.Encrypt(this.viewModel.Password);
            this.store.UpdateStringOption(Constants.PasswordProperty, encrypted);

            this.store.UpdateStringOption(Constants.DomainProperty, this.viewModel.Domain);
        }
    }
}
