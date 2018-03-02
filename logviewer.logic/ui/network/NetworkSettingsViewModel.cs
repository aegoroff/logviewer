// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.09.2007
// © 2007-2018 Alexander Egorov

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using logviewer.logic.Annotations;

namespace logviewer.logic.ui.network
{
    [PublicAPI]
    public sealed class NetworkSettingsViewModel : INetworkSettingsViewModel, INotifyPropertyChanged
    {
        private string domain;

        private string host;

        private bool isNoUseProxy;

        private bool isSettingsChanged;

        private bool isUseAutoProxy;

        private bool isUseDefaultCredentials;

        private bool isUseManualProxy;

        private string password;

        private int port;

        private ProxyMode proxyMode;

        private string userName;

        private bool enableCustomCredentials;

        public IWebProxy Proxy
        {
            get
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
                                               : this.Credentials
                               };
                    default:
                        return null;
                }
            }
        }

        private ICredentials Credentials => new NetworkCredential(this.UserName, this.Password, this.Domain);

        public bool EnableCustomCredentials
        {
            get => this.enableCustomCredentials;
            set
            {
                this.enableCustomCredentials = value;
                this.OnPropertyChanged(nameof(this.EnableCustomCredentials));
            }
        }

        public void Initialize(ProxyMode mode, bool useDefaultCredentials)
        {
            this.ProxyMode = mode;
            this.isUseDefaultCredentials = useDefaultCredentials;
            this.EnableCustomCredentials = !useDefaultCredentials;
        }

        public ProxyMode ProxyMode
        {
            get => this.proxyMode;
            set
            {
                this.proxyMode = value;
                this.isNoUseProxy = value == ProxyMode.None;
                this.isUseAutoProxy = value == ProxyMode.AutoProxyDetection;
                this.isUseManualProxy = value == ProxyMode.Custom;
                this.OnPropertyChanged(nameof(this.IsNoUseProxy));
                this.OnPropertyChanged(nameof(this.IsUseAutoProxy));
                this.OnPropertyChanged(nameof(this.IsUseManualProxy));
            }
        }

        public bool IsNoUseProxy
        {
            get => this.isNoUseProxy;
            set => this.OnModeChange(value, ref this.isNoUseProxy, ProxyModeTransition.ToNone);
        }

        public bool IsUseAutoProxy
        {
            get => this.isUseAutoProxy;
            set => this.OnModeChange(value, ref this.isUseAutoProxy, ProxyModeTransition.ToAutoProxyDetection);
        }

        public bool IsUseManualProxy
        {
            get => this.isUseManualProxy;
            set
            {
                this.OnModeChange(value, ref this.isUseManualProxy, ProxyModeTransition.ToCustom);
                if (value)
                {
                    this.OnModeChanged(this.GetCustomModeTransition());
                }
            }
        }

        public bool IsUseDefaultCredentials
        {
            get => this.isUseDefaultCredentials;
            set
            {
                this.isUseDefaultCredentials = value;
                this.EnableCustomCredentials = !value;
                this.OnModeChanged(this.GetCustomModeTransition());
            }
        }

        public bool IsSettingsChanged
        {
            get => this.isSettingsChanged;
            set
            {
                this.isSettingsChanged = value;
                this.OnPropertyChanged(nameof(this.IsSettingsChanged));
            }
        }

        public string Host
        {
            get => this.host;
            set
            {
                this.host = value;
                this.OnPropertyChanged(nameof(this.Host));
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                this.port = value;
                this.OnPropertyChanged(nameof(this.Port));
            }
        }

        public string UserName
        {
            get => this.userName;
            set
            {
                this.userName = value;
                this.OnPropertyChanged(nameof(this.UserName));
            }
        }

        public string Password
        {
            get => this.password;
            set
            {
                this.password = value;
                this.PasswordUpdated?.Invoke(this, value);
            }
        }

        public string Domain
        {
            get => this.domain;
            set
            {
                this.domain = value;
                this.OnPropertyChanged(nameof(this.Domain));
            }
        }

        public event EventHandler<ProxyModeTransition> ModeChanged;

        public event EventHandler<string> PasswordUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnModeChange(bool newValue, ref bool field, ProxyModeTransition transition)
        {
            var notifyModeChange = newValue && !field;
            field = newValue;
            if (notifyModeChange)
            {
                this.OnModeChanged(transition);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnModeChanged(ProxyModeTransition transition) => this.ModeChanged?.Invoke(this, transition);

        private ProxyModeTransition GetCustomModeTransition() => this.isUseDefaultCredentials
                                                                     ? ProxyModeTransition.ToCustomDefaultUser
                                                                     : ProxyModeTransition.ToCustomWithManualUser;
    }
}
