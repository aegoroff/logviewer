// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using logviewer.logic;

namespace logviewer.ui
{
    public partial class NetworkSettingsDlg : Form, INetworkSettingsView
    {
        private readonly NetworkSettings networkSettings = new NetworkSettings(MainViewModel.Current.SettingsProvider.OptionsProvider);
        private readonly NetworkSettingsController nsController;
        private ProxyMode mode;

        public NetworkSettingsDlg()
        {
            this.InitializeComponent();
            this.nsController = new NetworkSettingsController(this.networkSettings, this);
            this.nsController.ReadMain();
            this.nsController.ReadAuthSettings();
            this.nsController.ReadHostAndPort();
        }

        public int Port
        {
            get
            {
                int result;
                var isParsed = int.TryParse(this.portBox.Text, out result);
                if (!isParsed)
                {
                    // TODO: LoggingService.WarnFormatted(Resources.CannotParsePort, this.portBox.Text);
                }
                return result;
            }
            set { this.portBox.Text = value.ToString(Thread.CurrentThread.CurrentCulture); }
        }

        public ProxyMode ProxyMode
        {
            [DebuggerStepThrough] get { return this.mode; }
            set
            {
                switch (value)
                {
                    case ProxyMode.None:
                        this.directConnRadio.Checked = true;
                        break;
                    case ProxyMode.Custom:
                        this.proxyUseRadio.Checked = true;
                        break;
                    case ProxyMode.AutoProxyDetection:
                        this.autoProxyRadio.Checked = true;
                        break;
                }
                this.mode = value;
            }
        }

        public bool IsUseProxy => this.proxyUseRadio.Checked;

        public bool IsUseIeProxy => this.autoProxyRadio.Checked;

        public bool IsUseDefaultCredentials
        {
            get { return this.defaultCredentialsBox.Checked; }
            set { this.defaultCredentialsBox.Checked = value; }
        }

        public string Host
        {
            get { return this.hostBox.Text; }
            set { this.hostBox.Text = value; }
        }

        public string UserName
        {
            get { return this.loginBox.Text; }
            set { this.loginBox.Text = value; }
        }

        public string Password
        {
            get { return this.pwdBox.Text; }
            set { this.pwdBox.Text = value; }
        }

        public string Domain
        {
            get { return this.domainBox.Text; }
            set { this.domainBox.Text = value; }
        }

        private void OnSelectProxyOption(object sender, EventArgs e)
        {
            this.groupBox1.Enabled = this.IsUseProxy;
            if (this.IsUseProxy)
            {
                this.nsController.ReadHostAndPort();
            }
        }

        private void OnChangeAuthSettings(object sender, EventArgs e)
        {
            this.loginBox.Enabled = !this.IsUseDefaultCredentials;
            this.pwdBox.Enabled = !this.IsUseDefaultCredentials;
            this.domainBox.Enabled = !this.IsUseDefaultCredentials;
            this.nsController.ReadAuthSettings();
        }

        private void OnOK(object sender, EventArgs e)
        {
            this.nsController.Write();
            NetworkSettings.SetProxy(MainViewModel.Current.SettingsProvider.OptionsProvider);
            this.Close();
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}