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
            this.label2 = new System.Windows.Forms.Label();
            this.keepLastNFilesBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.viewSettingsBox.SuspendLayout();
            this.fileSettingsBox.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.commonTemplatesBox.SuspendLayout();
            this.logLevelsBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.viewSettingsBox);
            this.tabPage1.Controls.Add(this.fileSettingsBox);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // viewSettingsBox
            // 
            this.viewSettingsBox.Controls.Add(this.pageSizeBox);
            this.viewSettingsBox.Controls.Add(this.pageSizeLabel);
            resources.ApplyResources(this.viewSettingsBox, "viewSettingsBox");
            this.viewSettingsBox.Name = "viewSettingsBox";
            this.viewSettingsBox.TabStop = false;
            // 
            // pageSizeBox
            // 
            resources.ApplyResources(this.pageSizeBox, "pageSizeBox");
            this.pageSizeBox.Name = "pageSizeBox";
            this.pageSizeBox.TextChanged += new System.EventHandler(this.OnSetPageSize);
            this.pageSizeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
            // 
            // pageSizeLabel
            // 
            resources.ApplyResources(this.pageSizeLabel, "pageSizeLabel");
            this.pageSizeLabel.Name = "pageSizeLabel";
            // 
            // fileSettingsBox
            // 
            this.fileSettingsBox.Controls.Add(this.label2);
            this.fileSettingsBox.Controls.Add(this.keepLastNFilesBox);
            this.fileSettingsBox.Controls.Add(this.label1);
            this.fileSettingsBox.Controls.Add(this.openLastFile);
            resources.ApplyResources(this.fileSettingsBox, "fileSettingsBox");
            this.fileSettingsBox.Name = "fileSettingsBox";
            this.fileSettingsBox.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // keepLastNFilesBox
            // 
            resources.ApplyResources(this.keepLastNFilesBox, "keepLastNFilesBox");
            this.keepLastNFilesBox.Name = "keepLastNFilesBox";
            this.keepLastNFilesBox.TextChanged += new System.EventHandler(this.OnKeepLastNFilesChange);
            this.keepLastNFilesBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // openLastFile
            // 
            resources.ApplyResources(this.openLastFile, "openLastFile");
            this.openLastFile.Name = "openLastFile";
            this.openLastFile.UseVisualStyleBackColor = true;
            this.openLastFile.CheckedChanged += new System.EventHandler(this.OnCheckLastOpenedFileOption);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.commonTemplatesBox);
            this.tabPage2.Controls.Add(this.logLevelsBox);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // commonTemplatesBox
            // 
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternBox);
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternLabel);
            resources.ApplyResources(this.commonTemplatesBox, "commonTemplatesBox");
            this.commonTemplatesBox.Name = "commonTemplatesBox";
            this.commonTemplatesBox.TabStop = false;
            // 
            // messageStartPatternBox
            // 
            resources.ApplyResources(this.messageStartPatternBox, "messageStartPatternBox");
            this.messageStartPatternBox.Name = "messageStartPatternBox";
            this.messageStartPatternBox.TextChanged += new System.EventHandler(this.OnSetMessageStartPattern);
            // 
            // messageStartPatternLabel
            // 
            resources.ApplyResources(this.messageStartPatternLabel, "messageStartPatternLabel");
            this.messageStartPatternLabel.Name = "messageStartPatternLabel";
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
            resources.ApplyResources(this.logLevelsBox, "logLevelsBox");
            this.logLevelsBox.Name = "logLevelsBox";
            this.logLevelsBox.TabStop = false;
            // 
            // fatalLabel
            // 
            resources.ApplyResources(this.fatalLabel, "fatalLabel");
            this.fatalLabel.Name = "fatalLabel";
            // 
            // fatalBox
            // 
            resources.ApplyResources(this.fatalBox, "fatalBox");
            this.fatalBox.Name = "fatalBox";
            this.fatalBox.TextChanged += new System.EventHandler(this.OnSetFatalLevel);
            // 
            // errorLabel
            // 
            resources.ApplyResources(this.errorLabel, "errorLabel");
            this.errorLabel.Name = "errorLabel";
            // 
            // errorBox
            // 
            resources.ApplyResources(this.errorBox, "errorBox");
            this.errorBox.Name = "errorBox";
            this.errorBox.TextChanged += new System.EventHandler(this.OnSetErrorLevel);
            // 
            // warnLabel
            // 
            resources.ApplyResources(this.warnLabel, "warnLabel");
            this.warnLabel.Name = "warnLabel";
            // 
            // warnBox
            // 
            resources.ApplyResources(this.warnBox, "warnBox");
            this.warnBox.Name = "warnBox";
            this.warnBox.TextChanged += new System.EventHandler(this.OnSetWarnLevel);
            // 
            // infoLabel
            // 
            resources.ApplyResources(this.infoLabel, "infoLabel");
            this.infoLabel.Name = "infoLabel";
            // 
            // infoBox
            // 
            resources.ApplyResources(this.infoBox, "infoBox");
            this.infoBox.Name = "infoBox";
            this.infoBox.TextChanged += new System.EventHandler(this.OnSetInfoLevel);
            // 
            // debugBox
            // 
            resources.ApplyResources(this.debugBox, "debugBox");
            this.debugBox.Name = "debugBox";
            this.debugBox.TextChanged += new System.EventHandler(this.OnSetDebugLevel);
            // 
            // traceBox
            // 
            resources.ApplyResources(this.traceBox, "traceBox");
            this.traceBox.Name = "traceBox";
            this.traceBox.TextChanged += new System.EventHandler(this.OnSetTraceLevel);
            // 
            // debugLabel
            // 
            resources.ApplyResources(this.debugLabel, "debugLabel");
            this.debugLabel.Name = "debugLabel";
            // 
            // traceLabel
            // 
            resources.ApplyResources(this.traceLabel, "traceLabel");
            this.traceLabel.Name = "traceLabel";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Controls.Add(this.saveButton);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.Name = "closeButton";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            resources.ApplyResources(this.saveButton, "saveButton");
            this.saveButton.Name = "saveButton";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.OnSave);
            // 
            // SettingsDlg
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDlg";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnClosed);
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
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox keepLastNFilesBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button closeButton;
    }
}