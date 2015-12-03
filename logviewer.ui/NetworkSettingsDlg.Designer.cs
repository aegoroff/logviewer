namespace logviewer.ui
{
	partial class NetworkSettingsDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetworkSettingsDlg));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.autoProxyRadio = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.domainLabel = new System.Windows.Forms.Label();
            this.domainBox = new System.Windows.Forms.TextBox();
            this.pwdLabel = new System.Windows.Forms.Label();
            this.logingLabel = new System.Windows.Forms.Label();
            this.pwdBox = new System.Windows.Forms.TextBox();
            this.loginBox = new System.Windows.Forms.TextBox();
            this.defaultCredentialsBox = new System.Windows.Forms.CheckBox();
            this.portBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.hostBox = new System.Windows.Forms.TextBox();
            this.proxyLabel = new System.Windows.Forms.Label();
            this.proxyUseRadio = new System.Windows.Forms.RadioButton();
            this.directConnRadio = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.OnClose);
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OnOK);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.autoProxyRadio);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.proxyUseRadio);
            this.panel1.Controls.Add(this.directConnRadio);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // autoProxyRadio
            // 
            resources.ApplyResources(this.autoProxyRadio, "autoProxyRadio");
            this.autoProxyRadio.Checked = true;
            this.autoProxyRadio.Name = "autoProxyRadio";
            this.autoProxyRadio.TabStop = true;
            this.autoProxyRadio.UseVisualStyleBackColor = true;
            this.autoProxyRadio.CheckedChanged += new System.EventHandler(this.OnSelectProxyOption);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.domainLabel);
            this.groupBox1.Controls.Add(this.domainBox);
            this.groupBox1.Controls.Add(this.pwdLabel);
            this.groupBox1.Controls.Add(this.logingLabel);
            this.groupBox1.Controls.Add(this.pwdBox);
            this.groupBox1.Controls.Add(this.loginBox);
            this.groupBox1.Controls.Add(this.defaultCredentialsBox);
            this.groupBox1.Controls.Add(this.portBox);
            this.groupBox1.Controls.Add(this.portLabel);
            this.groupBox1.Controls.Add(this.hostBox);
            this.groupBox1.Controls.Add(this.proxyLabel);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // domainLabel
            // 
            resources.ApplyResources(this.domainLabel, "domainLabel");
            this.domainLabel.Name = "domainLabel";
            // 
            // domainBox
            // 
            resources.ApplyResources(this.domainBox, "domainBox");
            this.domainBox.Name = "domainBox";
            // 
            // pwdLabel
            // 
            resources.ApplyResources(this.pwdLabel, "pwdLabel");
            this.pwdLabel.Name = "pwdLabel";
            // 
            // logingLabel
            // 
            resources.ApplyResources(this.logingLabel, "logingLabel");
            this.logingLabel.Name = "logingLabel";
            // 
            // pwdBox
            // 
            resources.ApplyResources(this.pwdBox, "pwdBox");
            this.pwdBox.Name = "pwdBox";
            // 
            // loginBox
            // 
            resources.ApplyResources(this.loginBox, "loginBox");
            this.loginBox.Name = "loginBox";
            // 
            // defaultCredentialsBox
            // 
            resources.ApplyResources(this.defaultCredentialsBox, "defaultCredentialsBox");
            this.defaultCredentialsBox.Checked = true;
            this.defaultCredentialsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.defaultCredentialsBox.Name = "defaultCredentialsBox";
            this.defaultCredentialsBox.UseVisualStyleBackColor = true;
            this.defaultCredentialsBox.CheckedChanged += new System.EventHandler(this.OnChangeAuthSettings);
            // 
            // portBox
            // 
            resources.ApplyResources(this.portBox, "portBox");
            this.portBox.Name = "portBox";
            // 
            // portLabel
            // 
            resources.ApplyResources(this.portLabel, "portLabel");
            this.portLabel.Name = "portLabel";
            // 
            // hostBox
            // 
            resources.ApplyResources(this.hostBox, "hostBox");
            this.hostBox.Name = "hostBox";
            // 
            // proxyLabel
            // 
            resources.ApplyResources(this.proxyLabel, "proxyLabel");
            this.proxyLabel.Name = "proxyLabel";
            // 
            // proxyUseRadio
            // 
            resources.ApplyResources(this.proxyUseRadio, "proxyUseRadio");
            this.proxyUseRadio.Name = "proxyUseRadio";
            this.proxyUseRadio.UseVisualStyleBackColor = true;
            this.proxyUseRadio.CheckedChanged += new System.EventHandler(this.OnSelectProxyOption);
            // 
            // directConnRadio
            // 
            resources.ApplyResources(this.directConnRadio, "directConnRadio");
            this.directConnRadio.Name = "directConnRadio";
            this.directConnRadio.UseVisualStyleBackColor = true;
            this.directConnRadio.CheckedChanged += new System.EventHandler(this.OnSelectProxyOption);
            // 
            // NetworkSettingsDlg
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "NetworkSettingsDlg";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton directConnRadio;
		private System.Windows.Forms.RadioButton proxyUseRadio;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox portBox;
		private System.Windows.Forms.Label portLabel;
		private System.Windows.Forms.TextBox hostBox;
		private System.Windows.Forms.Label proxyLabel;
		private System.Windows.Forms.CheckBox defaultCredentialsBox;
		private System.Windows.Forms.Label pwdLabel;
		private System.Windows.Forms.Label logingLabel;
		private System.Windows.Forms.TextBox pwdBox;
		private System.Windows.Forms.TextBox loginBox;
		private System.Windows.Forms.TextBox domainBox;
		private System.Windows.Forms.Label domainLabel;
		private System.Windows.Forms.RadioButton autoProxyRadio;
	}
}