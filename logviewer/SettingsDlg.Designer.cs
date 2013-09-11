namespace logviewer
{
    partial class SettingsDlg
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDlg));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.fileSettingsBox = new System.Windows.Forms.GroupBox();
            this.openLastFile = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.viewSettingsBox = new System.Windows.Forms.GroupBox();
            this.pageSizeLabel = new System.Windows.Forms.Label();
            this.pageSizeBox = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.fileSettingsBox.SuspendLayout();
            this.viewSettingsBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(638, 434);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.viewSettingsBox);
            this.tabPage1.Controls.Add(this.fileSettingsBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(630, 408);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // fileSettingsBox
            // 
            this.fileSettingsBox.Controls.Add(this.openLastFile);
            this.fileSettingsBox.Location = new System.Drawing.Point(8, 6);
            this.fileSettingsBox.Name = "fileSettingsBox";
            this.fileSettingsBox.Size = new System.Drawing.Size(614, 121);
            this.fileSettingsBox.TabIndex = 0;
            this.fileSettingsBox.TabStop = false;
            this.fileSettingsBox.Text = "File settings";
            // 
            // openLastFile
            // 
            this.openLastFile.AutoSize = true;
            this.openLastFile.Location = new System.Drawing.Point(6, 19);
            this.openLastFile.Name = "openLastFile";
            this.openLastFile.Size = new System.Drawing.Size(87, 17);
            this.openLastFile.TabIndex = 0;
            this.openLastFile.Text = "Open last file";
            this.openLastFile.UseVisualStyleBackColor = true;
            this.openLastFile.CheckedChanged += new System.EventHandler(this.OnCheckLastOpenedFileOption);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(630, 408);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Templates";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // viewSettingsBox
            // 
            this.viewSettingsBox.Controls.Add(this.pageSizeBox);
            this.viewSettingsBox.Controls.Add(this.pageSizeLabel);
            this.viewSettingsBox.Location = new System.Drawing.Point(8, 133);
            this.viewSettingsBox.Name = "viewSettingsBox";
            this.viewSettingsBox.Size = new System.Drawing.Size(614, 267);
            this.viewSettingsBox.TabIndex = 1;
            this.viewSettingsBox.TabStop = false;
            this.viewSettingsBox.Text = "View settings";
            // 
            // pageSizeLabel
            // 
            this.pageSizeLabel.AutoSize = true;
            this.pageSizeLabel.Location = new System.Drawing.Point(6, 34);
            this.pageSizeLabel.Name = "pageSizeLabel";
            this.pageSizeLabel.Size = new System.Drawing.Size(56, 13);
            this.pageSizeLabel.TabIndex = 1;
            this.pageSizeLabel.Text = "Page size:";
            // 
            // pageSizeBox
            // 
            this.pageSizeBox.Location = new System.Drawing.Point(68, 31);
            this.pageSizeBox.Name = "pageSizeBox";
            this.pageSizeBox.Size = new System.Drawing.Size(57, 20);
            this.pageSizeBox.TabIndex = 3;
            this.pageSizeBox.TextChanged += new System.EventHandler(this.OnSetPageSize);
            this.pageSizeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInPageSizeBox);
            // 
            // SettingsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 434);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsDlg";
            this.Text = "Settings";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.fileSettingsBox.ResumeLayout(false);
            this.fileSettingsBox.PerformLayout();
            this.viewSettingsBox.ResumeLayout(false);
            this.viewSettingsBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox fileSettingsBox;
        private System.Windows.Forms.CheckBox openLastFile;
        private System.Windows.Forms.GroupBox viewSettingsBox;
        private System.Windows.Forms.Label pageSizeLabel;
        private System.Windows.Forms.TextBox pageSizeBox;
    }
}