// Created by: egr
// Created at: 04.09.2007
// � 2007-2015 Alexander Egorov

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using logviewer.logic.Annotations;

namespace logviewer.logic.ui.network
{
    public sealed class NetworkSettingsModel : INetworkSettingsModel, INotifyPropertyChanged
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
            get { return this.enableCustomCredentials; }
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
            get { return this.proxyMode; }
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
            get { return this.isNoUseProxy; }
            set { this.OnModeChange(value, ref this.isNoUseProxy, ProxyMode.None); }
        }

        public bool IsUseAutoProxy
        {
            get { return this.isUseAutoProxy; }
            set { this.OnModeChange(value, ref this.isUseAutoProxy, ProxyMode.AutoProxyDetection); }
        }

        public bool IsUseManualProxy
        {
            get { return this.isUseManualProxy; }
            set { this.OnModeChange(value, ref this.isUseManualProxy, ProxyMode.Custom); }
        }

        public bool IsUseDefaultCredentials
        {
            get { return this.isUseDefaultCredentials; }
            set
            {
                this.isUseDefaultCredentials = value;
                this.EnableCustomCredentials = !value;
                this.OnModeChanged(ProxyMode.Custom);
            }
        }

        public bool IsSettingsChanged
        {
            get { return this.isSettingsChanged; }
            set
            {
                this.isSettingsChanged = value;
                this.OnPropertyChanged(nameof(this.IsSettingsChanged));
            }
        }

        public string Host
        {
            get { return this.host; }
            set
            {
                this.host = value;
                this.OnPropertyChanged(nameof(this.Host));
            }
        }

        public int Port
        {
            get { return this.port; }
            set
            {
                this.port = value;
                this.OnPropertyChanged(nameof(this.Port));
            }
        }

        public string UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
                this.OnPropertyChanged(nameof(this.UserName));
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.PasswordUpdated?.Invoke(this, value);
            }
        }

        public string Domain
        {
            get { return this.domain; }
            set
            {
                this.domain = value;
                this.OnPropertyChanged(nameof(this.Domain));
            }
        }

        public event EventHandler<ProxyMode> ModeChanged;
        public event EventHandler<string> PasswordUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnModeChange(bool newValue, ref bool field, ProxyMode mode)
        {
            var notifyModeChange = newValue && !field;
            field = newValue;
            if (notifyModeChange)
            {
                this.OnModeChanged(mode);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnModeChanged(ProxyMode mode)
        {
            this.ModeChanged?.Invoke(this, mode);
        }
    }
}