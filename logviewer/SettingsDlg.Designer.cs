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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.generalSettingsBox = new System.Windows.Forms.GroupBox();
            this.openLastFile = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.generalSettingsBox.SuspendLayout();
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
            this.tabPage1.Controls.Add(this.generalSettingsBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(630, 408);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            // generalSettingsBox
            // 
            this.generalSettingsBox.Controls.Add(this.openLastFile);
            this.generalSettingsBox.Location = new System.Drawing.Point(8, 6);
            this.generalSettingsBox.Name = "generalSettingsBox";
            this.generalSettingsBox.Size = new System.Drawing.Size(276, 394);
            this.generalSettingsBox.TabIndex = 0;
            this.generalSettingsBox.TabStop = false;
            this.generalSettingsBox.Text = "General settings";
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
            this.generalSettingsBox.ResumeLayout(false);
            this.generalSettingsBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox generalSettingsBox;
        private System.Windows.Forms.CheckBox openLastFile;
    }
}