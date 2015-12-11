// Created by: egr
// Created at: 05.09.2007
// � 2007-2008 Alexander Egorov

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using logviewer.logic.models;
using logviewer.logic.ui;

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

            this.directConnRadio.Tag = ProxyMode.None;
            this.autoProxyRadio.Tag = ProxyMode.AutoProxyDetection;
            this.proxyUseRadio.Tag = ProxyMode.Custom;

            this.IsUseDefaultCredentials = this.networkSettings.IsUseDefaultCredentials;

            this.nsController.Start();
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
                this.directConnRadio.Checked = value == ProxyMode.None;
                this.proxyUseRadio.Checked = value == ProxyMode.Custom;
                this.autoProxyRadio.Checked = value == ProxyMode.AutoProxyDetection;
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

        public void EnableProxySettings(bool enabled)
        {
            this.groupBox1.Enabled = enabled;
        }

        public void EnableCredentialsSettings(bool enabled)
        {
            this.loginBox.Enabled = enabled;
            this.pwdBox.Enabled = enabled;
            this.domainBox.Enabled = enabled;
        }

        private void OnSelectProxyOption(object sender, EventArgs e)
        {
            var radio = (RadioButton) sender;
            if (!radio.Checked)
            {
                return;
            }
            var toMode = (ProxyMode)radio.Tag;
            this.nsController.Goto(toMode);
        }

        private void OnChangeAuthSettings(object sender, EventArgs e)
        {
            this.nsController.Goto(ProxyMode.Custom);
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