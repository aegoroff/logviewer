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
            this.autoRefreshCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.keepLastNFilesBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.openLastFile = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.parsingTemplateSelector = new System.Windows.Forms.ComboBox();
            this.commonTemplatesBox = new System.Windows.Forms.GroupBox();
            this.messageStartPatternBox = new System.Windows.Forms.TextBox();
            this.messageStartPatternLabel = new System.Windows.Forms.Label();
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
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Controls.Add(this.viewSettingsBox);
            this.tabPage1.Controls.Add(this.fileSettingsBox);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // viewSettingsBox
            // 
            resources.ApplyResources(this.viewSettingsBox, "viewSettingsBox");
            this.viewSettingsBox.Controls.Add(this.pageSizeBox);
            this.viewSettingsBox.Controls.Add(this.pageSizeLabel);
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
            resources.ApplyResources(this.fileSettingsBox, "fileSettingsBox");
            this.fileSettingsBox.Controls.Add(this.autoRefreshCheckBox);
            this.fileSettingsBox.Controls.Add(this.label2);
            this.fileSettingsBox.Controls.Add(this.keepLastNFilesBox);
            this.fileSettingsBox.Controls.Add(this.label1);
            this.fileSettingsBox.Controls.Add(this.openLastFile);
            this.fileSettingsBox.Name = "fileSettingsBox";
            this.fileSettingsBox.TabStop = false;
            // 
            // autoRefreshCheckBox
            // 
            resources.ApplyResources(this.autoRefreshCheckBox, "autoRefreshCheckBox");
            this.autoRefreshCheckBox.Name = "autoRefreshCheckBox";
            this.autoRefreshCheckBox.UseVisualStyleBackColor = true;
            this.autoRefreshCheckBox.CheckedChanged += new System.EventHandler(this.OnChangeAutorefresh);
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
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.parsingTemplateSelector);
            this.tabPage2.Controls.Add(this.commonTemplatesBox);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // parsingTemplateSelector
            // 
            resources.ApplyResources(this.parsingTemplateSelector, "parsingTemplateSelector");
            this.parsingTemplateSelector.FormattingEnabled = true;
            this.parsingTemplateSelector.Name = "parsingTemplateSelector";
            this.parsingTemplateSelector.TextUpdate += new System.EventHandler(this.OnSetParsingTemplateName);
            // 
            // commonTemplatesBox
            // 
            resources.ApplyResources(this.commonTemplatesBox, "commonTemplatesBox");
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternBox);
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternLabel);
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
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Controls.Add(this.saveButton);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // closeButton
            // 
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
            this.tabPage2.PerformLayout();
            this.commonTemplatesBox.ResumeLayout(false);
            this.commonTemplatesBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox commonTemplatesBox;
        private System.Windows.Forms.Label messageStartPatternLabel;
        private System.Windows.Forms.TextBox messageStartPatternBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox keepLastNFilesBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ComboBox parsingTemplateSelector;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox autoRefreshCheckBox;
    }
}