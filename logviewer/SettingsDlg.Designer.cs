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
            this.viewSettingsBox = new System.Windows.Forms.GroupBox();
            this.pageSizeBox = new System.Windows.Forms.TextBox();
            this.pageSizeLabel = new System.Windows.Forms.Label();
            this.fileSettingsBox = new System.Windows.Forms.GroupBox();
            this.openLastFile = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.commonTemplatesBox = new System.Windows.Forms.GroupBox();
            this.messageStartPatternBox = new System.Windows.Forms.TextBox();
            this.messageStartPatternLabel = new System.Windows.Forms.Label();
            this.logLevelsBox = new System.Windows.Forms.GroupBox();
            this.fatalLabel = new System.Windows.Forms.Label();
            this.fatalBox = new System.Windows.Forms.TextBox();
            this.errorLabel = new System.Windows.Forms.Label();
            this.errorBox = new System.Windows.Forms.TextBox();
            this.warnLabel = new System.Windows.Forms.Label();
            this.warnBox = new System.Windows.Forms.TextBox();
            this.infoLabel = new System.Windows.Forms.Label();
            this.infoBox = new System.Windows.Forms.TextBox();
            this.debugBox = new System.Windows.Forms.TextBox();
            this.traceBox = new System.Windows.Forms.TextBox();
            this.debugLabel = new System.Windows.Forms.Label();
            this.traceLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.viewSettingsBox.SuspendLayout();
            this.fileSettingsBox.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.commonTemplatesBox.SuspendLayout();
            this.logLevelsBox.SuspendLayout();
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
            this.tabControl1.Size = new System.Drawing.Size(555, 309);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.viewSettingsBox);
            this.tabPage1.Controls.Add(this.fileSettingsBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(547, 283);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // viewSettingsBox
            // 
            this.viewSettingsBox.Controls.Add(this.pageSizeBox);
            this.viewSettingsBox.Controls.Add(this.pageSizeLabel);
            this.viewSettingsBox.Location = new System.Drawing.Point(8, 133);
            this.viewSettingsBox.Name = "viewSettingsBox";
            this.viewSettingsBox.Size = new System.Drawing.Size(531, 142);
            this.viewSettingsBox.TabIndex = 1;
            this.viewSettingsBox.TabStop = false;
            this.viewSettingsBox.Text = "View settings";
            // 
            // pageSizeBox
            // 
            this.pageSizeBox.Location = new System.Drawing.Point(68, 31);
            this.pageSizeBox.Name = "pageSizeBox";
            this.pageSizeBox.Size = new System.Drawing.Size(57, 20);
            this.pageSizeBox.TabIndex = 3;
            this.pageSizeBox.TextChanged += new System.EventHandler(this.OnSetPageSize);
            this.pageSizeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
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
            // fileSettingsBox
            // 
            this.fileSettingsBox.Controls.Add(this.label2);
            this.fileSettingsBox.Controls.Add(this.textBox1);
            this.fileSettingsBox.Controls.Add(this.label1);
            this.fileSettingsBox.Controls.Add(this.openLastFile);
            this.fileSettingsBox.Location = new System.Drawing.Point(8, 6);
            this.fileSettingsBox.Name = "fileSettingsBox";
            this.fileSettingsBox.Size = new System.Drawing.Size(531, 121);
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
            this.tabPage2.Controls.Add(this.commonTemplatesBox);
            this.tabPage2.Controls.Add(this.logLevelsBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(547, 283);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Templates";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // commonTemplatesBox
            // 
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternBox);
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternLabel);
            this.commonTemplatesBox.Location = new System.Drawing.Point(8, 6);
            this.commonTemplatesBox.Name = "commonTemplatesBox";
            this.commonTemplatesBox.Size = new System.Drawing.Size(531, 65);
            this.commonTemplatesBox.TabIndex = 1;
            this.commonTemplatesBox.TabStop = false;
            this.commonTemplatesBox.Text = "Common";
            // 
            // messageStartPatternBox
            // 
            this.messageStartPatternBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.messageStartPatternBox.Location = new System.Drawing.Point(124, 25);
            this.messageStartPatternBox.Name = "messageStartPatternBox";
            this.messageStartPatternBox.Size = new System.Drawing.Size(393, 20);
            this.messageStartPatternBox.TabIndex = 1;
            this.messageStartPatternBox.TextChanged += new System.EventHandler(this.OnSetMessageStartPattern);
            // 
            // messageStartPatternLabel
            // 
            this.messageStartPatternLabel.AutoSize = true;
            this.messageStartPatternLabel.Location = new System.Drawing.Point(6, 28);
            this.messageStartPatternLabel.Name = "messageStartPatternLabel";
            this.messageStartPatternLabel.Size = new System.Drawing.Size(112, 13);
            this.messageStartPatternLabel.TabIndex = 0;
            this.messageStartPatternLabel.Text = "Message start pattern:";
            this.messageStartPatternLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // logLevelsBox
            // 
            this.logLevelsBox.Controls.Add(this.fatalLabel);
            this.logLevelsBox.Controls.Add(this.fatalBox);
            this.logLevelsBox.Controls.Add(this.errorLabel);
            this.logLevelsBox.Controls.Add(this.errorBox);
            this.logLevelsBox.Controls.Add(this.warnLabel);
            this.logLevelsBox.Controls.Add(this.warnBox);
            this.logLevelsBox.Controls.Add(this.infoLabel);
            this.logLevelsBox.Controls.Add(this.infoBox);
            this.logLevelsBox.Controls.Add(this.debugBox);
            this.logLevelsBox.Controls.Add(this.traceBox);
            this.logLevelsBox.Controls.Add(this.debugLabel);
            this.logLevelsBox.Controls.Add(this.traceLabel);
            this.logLevelsBox.Location = new System.Drawing.Point(8, 77);
            this.logLevelsBox.Name = "logLevelsBox";
            this.logLevelsBox.Size = new System.Drawing.Size(531, 198);
            this.logLevelsBox.TabIndex = 0;
            this.logLevelsBox.TabStop = false;
            this.logLevelsBox.Text = "Log level parsing templates";
            // 
            // fatalLabel
            // 
            this.fatalLabel.AutoSize = true;
            this.fatalLabel.Location = new System.Drawing.Point(85, 162);
            this.fatalLabel.Name = "fatalLabel";
            this.fatalLabel.Size = new System.Drawing.Size(33, 13);
            this.fatalLabel.TabIndex = 11;
            this.fatalLabel.Text = "Fatal:";
            this.fatalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fatalBox
            // 
            this.fatalBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.fatalBox.Location = new System.Drawing.Point(124, 159);
            this.fatalBox.Name = "fatalBox";
            this.fatalBox.Size = new System.Drawing.Size(393, 20);
            this.fatalBox.TabIndex = 10;
            this.fatalBox.TextChanged += new System.EventHandler(this.OnSetFatalLevel);
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.Location = new System.Drawing.Point(86, 136);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(32, 13);
            this.errorLabel.TabIndex = 9;
            this.errorLabel.Text = "Error:";
            this.errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // errorBox
            // 
            this.errorBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.errorBox.Location = new System.Drawing.Point(124, 133);
            this.errorBox.Name = "errorBox";
            this.errorBox.Size = new System.Drawing.Size(393, 20);
            this.errorBox.TabIndex = 8;
            this.errorBox.TextChanged += new System.EventHandler(this.OnSetErrorLevel);
            // 
            // warnLabel
            // 
            this.warnLabel.AutoSize = true;
            this.warnLabel.Location = new System.Drawing.Point(68, 110);
            this.warnLabel.Name = "warnLabel";
            this.warnLabel.Size = new System.Drawing.Size(50, 13);
            this.warnLabel.TabIndex = 7;
            this.warnLabel.Text = "Warning:";
            this.warnLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // warnBox
            // 
            this.warnBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.warnBox.Location = new System.Drawing.Point(124, 107);
            this.warnBox.Name = "warnBox";
            this.warnBox.Size = new System.Drawing.Size(393, 20);
            this.warnBox.TabIndex = 6;
            this.warnBox.TextChanged += new System.EventHandler(this.OnSetWarnLevel);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(56, 84);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(62, 13);
            this.infoLabel.TabIndex = 5;
            this.infoLabel.Text = "Information:";
            this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // infoBox
            // 
            this.infoBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.infoBox.Location = new System.Drawing.Point(124, 81);
            this.infoBox.Name = "infoBox";
            this.infoBox.Size = new System.Drawing.Size(393, 20);
            this.infoBox.TabIndex = 4;
            this.infoBox.TextChanged += new System.EventHandler(this.OnSetInfoLevel);
            // 
            // debugBox
            // 
            this.debugBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.debugBox.Location = new System.Drawing.Point(124, 55);
            this.debugBox.Name = "debugBox";
            this.debugBox.Size = new System.Drawing.Size(393, 20);
            this.debugBox.TabIndex = 3;
            this.debugBox.TextChanged += new System.EventHandler(this.OnSetDebugLevel);
            // 
            // traceBox
            // 
            this.traceBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.traceBox.Location = new System.Drawing.Point(124, 28);
            this.traceBox.Name = "traceBox";
            this.traceBox.Size = new System.Drawing.Size(393, 20);
            this.traceBox.TabIndex = 2;
            this.traceBox.TextChanged += new System.EventHandler(this.OnSetTraceLevel);
            // 
            // debugLabel
            // 
            this.debugLabel.AutoSize = true;
            this.debugLabel.Location = new System.Drawing.Point(76, 58);
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.Size = new System.Drawing.Size(42, 13);
            this.debugLabel.TabIndex = 1;
            this.debugLabel.Text = "Debug:";
            this.debugLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // traceLabel
            // 
            this.traceLabel.AutoSize = true;
            this.traceLabel.Location = new System.Drawing.Point(80, 31);
            this.traceLabel.Name = "traceLabel";
            this.traceLabel.Size = new System.Drawing.Size(38, 13);
            this.traceLabel.TabIndex = 0;
            this.traceLabel.Text = "Trace:";
            this.traceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Keep last";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(63, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(30, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "opened files";
            // 
            // SettingsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 309);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsDlg";
            this.Text = "Settings";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.viewSettingsBox.ResumeLayout(false);
            this.viewSettingsBox.PerformLayout();
            this.fileSettingsBox.ResumeLayout(false);
            this.fileSettingsBox.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.commonTemplatesBox.ResumeLayout(false);
            this.commonTemplatesBox.PerformLayout();
            this.logLevelsBox.ResumeLayout(false);
            this.logLevelsBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox logLevelsBox;
        private System.Windows.Forms.GroupBox commonTemplatesBox;
        private System.Windows.Forms.Label messageStartPatternLabel;
        private System.Windows.Forms.TextBox messageStartPatternBox;
        private System.Windows.Forms.Label debugLabel;
        private System.Windows.Forms.Label traceLabel;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.TextBox infoBox;
        private System.Windows.Forms.TextBox debugBox;
        private System.Windows.Forms.TextBox traceBox;
        private System.Windows.Forms.Label warnLabel;
        private System.Windows.Forms.TextBox warnBox;
        private System.Windows.Forms.TextBox errorBox;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.Label fatalLabel;
        private System.Windows.Forms.TextBox fatalBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
    }
}