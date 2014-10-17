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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDlg));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.resetColorsBtn = new System.Windows.Forms.Button();
            this.fatalBtn = new System.Windows.Forms.Button();
            this.errorBtn = new System.Windows.Forms.Button();
            this.warnBtn = new System.Windows.Forms.Button();
            this.infoBtn = new System.Windows.Forms.Button();
            this.debugBtn = new System.Windows.Forms.Button();
            this.traceBtn = new System.Windows.Forms.Button();
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
            this.removeParsingTemplateBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.newMessageCompiledBox = new System.Windows.Forms.CheckBox();
            this.newTemplateMessageFilterBox = new System.Windows.Forms.TextBox();
            this.newTemplateFilterLabel = new System.Windows.Forms.Label();
            this.newTemplateNameLabel = new System.Windows.Forms.Label();
            this.addNewTemplateBtn = new System.Windows.Forms.Button();
            this.newTemplateNameBox = new System.Windows.Forms.TextBox();
            this.cancelAddNewTemplateBtn = new System.Windows.Forms.Button();
            this.newPatternMessageStartLabel = new System.Windows.Forms.Label();
            this.newTemplateMessageStartBox = new System.Windows.Forms.TextBox();
            this.addNewParsingTemplateBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.parsingTemplateSelector = new System.Windows.Forms.ComboBox();
            this.commonTemplatesBox = new System.Windows.Forms.GroupBox();
            this.messageCompiledBox = new System.Windows.Forms.CheckBox();
            this.messageFilterBox = new System.Windows.Forms.TextBox();
            this.messageFilterLabel = new System.Windows.Forms.Label();
            this.messageStartPatternBox = new System.Windows.Forms.TextBox();
            this.messageStartPatternLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.invalidTemplateTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.viewSettingsBox.SuspendLayout();
            this.fileSettingsBox.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.invalidTemplateTooltip.SetToolTip(this.tabControl1, resources.GetString("tabControl1.ToolTip"));
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.viewSettingsBox);
            this.tabPage1.Controls.Add(this.fileSettingsBox);
            this.tabPage1.Name = "tabPage1";
            this.invalidTemplateTooltip.SetToolTip(this.tabPage1, resources.GetString("tabPage1.ToolTip"));
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.resetColorsBtn);
            this.groupBox1.Controls.Add(this.fatalBtn);
            this.groupBox1.Controls.Add(this.errorBtn);
            this.groupBox1.Controls.Add(this.warnBtn);
            this.groupBox1.Controls.Add(this.infoBtn);
            this.groupBox1.Controls.Add(this.debugBtn);
            this.groupBox1.Controls.Add(this.traceBtn);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.invalidTemplateTooltip.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // resetColorsBtn
            // 
            resources.ApplyResources(this.resetColorsBtn, "resetColorsBtn");
            this.resetColorsBtn.Name = "resetColorsBtn";
            this.invalidTemplateTooltip.SetToolTip(this.resetColorsBtn, resources.GetString("resetColorsBtn.ToolTip"));
            this.resetColorsBtn.UseVisualStyleBackColor = true;
            this.resetColorsBtn.Click += new System.EventHandler(this.OnResetToDefault);
            // 
            // fatalBtn
            // 
            resources.ApplyResources(this.fatalBtn, "fatalBtn");
            this.fatalBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.fatalBtn.Name = "fatalBtn";
            this.invalidTemplateTooltip.SetToolTip(this.fatalBtn, resources.GetString("fatalBtn.ToolTip"));
            this.fatalBtn.UseVisualStyleBackColor = true;
            this.fatalBtn.Click += new System.EventHandler(this.OnChangeFatal);
            // 
            // errorBtn
            // 
            resources.ApplyResources(this.errorBtn, "errorBtn");
            this.errorBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.errorBtn.Name = "errorBtn";
            this.invalidTemplateTooltip.SetToolTip(this.errorBtn, resources.GetString("errorBtn.ToolTip"));
            this.errorBtn.UseVisualStyleBackColor = true;
            this.errorBtn.Click += new System.EventHandler(this.OnChangeError);
            // 
            // warnBtn
            // 
            resources.ApplyResources(this.warnBtn, "warnBtn");
            this.warnBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.warnBtn.Name = "warnBtn";
            this.invalidTemplateTooltip.SetToolTip(this.warnBtn, resources.GetString("warnBtn.ToolTip"));
            this.warnBtn.UseVisualStyleBackColor = true;
            this.warnBtn.Click += new System.EventHandler(this.OnChangeWarn);
            // 
            // infoBtn
            // 
            resources.ApplyResources(this.infoBtn, "infoBtn");
            this.infoBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.infoBtn.Name = "infoBtn";
            this.invalidTemplateTooltip.SetToolTip(this.infoBtn, resources.GetString("infoBtn.ToolTip"));
            this.infoBtn.UseVisualStyleBackColor = true;
            this.infoBtn.Click += new System.EventHandler(this.OnChangeInfo);
            // 
            // debugBtn
            // 
            resources.ApplyResources(this.debugBtn, "debugBtn");
            this.debugBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.debugBtn.Name = "debugBtn";
            this.invalidTemplateTooltip.SetToolTip(this.debugBtn, resources.GetString("debugBtn.ToolTip"));
            this.debugBtn.UseVisualStyleBackColor = true;
            this.debugBtn.Click += new System.EventHandler(this.OnChangeDebug);
            // 
            // traceBtn
            // 
            resources.ApplyResources(this.traceBtn, "traceBtn");
            this.traceBtn.BackColor = System.Drawing.Color.Transparent;
            this.traceBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.traceBtn.Name = "traceBtn";
            this.invalidTemplateTooltip.SetToolTip(this.traceBtn, resources.GetString("traceBtn.ToolTip"));
            this.traceBtn.UseVisualStyleBackColor = false;
            this.traceBtn.Click += new System.EventHandler(this.OnChangeTrace);
            // 
            // viewSettingsBox
            // 
            resources.ApplyResources(this.viewSettingsBox, "viewSettingsBox");
            this.viewSettingsBox.Controls.Add(this.pageSizeBox);
            this.viewSettingsBox.Controls.Add(this.pageSizeLabel);
            this.viewSettingsBox.Name = "viewSettingsBox";
            this.viewSettingsBox.TabStop = false;
            this.invalidTemplateTooltip.SetToolTip(this.viewSettingsBox, resources.GetString("viewSettingsBox.ToolTip"));
            // 
            // pageSizeBox
            // 
            resources.ApplyResources(this.pageSizeBox, "pageSizeBox");
            this.pageSizeBox.Name = "pageSizeBox";
            this.invalidTemplateTooltip.SetToolTip(this.pageSizeBox, resources.GetString("pageSizeBox.ToolTip"));
            this.pageSizeBox.TextChanged += new System.EventHandler(this.OnSetPageSize);
            this.pageSizeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
            // 
            // pageSizeLabel
            // 
            resources.ApplyResources(this.pageSizeLabel, "pageSizeLabel");
            this.pageSizeLabel.Name = "pageSizeLabel";
            this.invalidTemplateTooltip.SetToolTip(this.pageSizeLabel, resources.GetString("pageSizeLabel.ToolTip"));
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
            this.invalidTemplateTooltip.SetToolTip(this.fileSettingsBox, resources.GetString("fileSettingsBox.ToolTip"));
            // 
            // autoRefreshCheckBox
            // 
            resources.ApplyResources(this.autoRefreshCheckBox, "autoRefreshCheckBox");
            this.autoRefreshCheckBox.Name = "autoRefreshCheckBox";
            this.invalidTemplateTooltip.SetToolTip(this.autoRefreshCheckBox, resources.GetString("autoRefreshCheckBox.ToolTip"));
            this.autoRefreshCheckBox.UseVisualStyleBackColor = true;
            this.autoRefreshCheckBox.CheckedChanged += new System.EventHandler(this.OnChangeAutorefresh);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.invalidTemplateTooltip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // keepLastNFilesBox
            // 
            resources.ApplyResources(this.keepLastNFilesBox, "keepLastNFilesBox");
            this.keepLastNFilesBox.Name = "keepLastNFilesBox";
            this.invalidTemplateTooltip.SetToolTip(this.keepLastNFilesBox, resources.GetString("keepLastNFilesBox.ToolTip"));
            this.keepLastNFilesBox.TextChanged += new System.EventHandler(this.OnKeepLastNFilesChange);
            this.keepLastNFilesBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPressInNumberOnlyBox);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.invalidTemplateTooltip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            // 
            // openLastFile
            // 
            resources.ApplyResources(this.openLastFile, "openLastFile");
            this.openLastFile.Name = "openLastFile";
            this.invalidTemplateTooltip.SetToolTip(this.openLastFile, resources.GetString("openLastFile.ToolTip"));
            this.openLastFile.UseVisualStyleBackColor = true;
            this.openLastFile.CheckedChanged += new System.EventHandler(this.OnCheckLastOpenedFileOption);
            // 
            // tabPage2
            // 
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Controls.Add(this.removeParsingTemplateBtn);
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.addNewParsingTemplateBtn);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.parsingTemplateSelector);
            this.tabPage2.Controls.Add(this.commonTemplatesBox);
            this.tabPage2.Name = "tabPage2";
            this.invalidTemplateTooltip.SetToolTip(this.tabPage2, resources.GetString("tabPage2.ToolTip"));
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // removeParsingTemplateBtn
            // 
            resources.ApplyResources(this.removeParsingTemplateBtn, "removeParsingTemplateBtn");
            this.removeParsingTemplateBtn.Name = "removeParsingTemplateBtn";
            this.invalidTemplateTooltip.SetToolTip(this.removeParsingTemplateBtn, resources.GetString("removeParsingTemplateBtn.ToolTip"));
            this.removeParsingTemplateBtn.UseVisualStyleBackColor = true;
            this.removeParsingTemplateBtn.Click += new System.EventHandler(this.OnRemoveSelectedParsingTemplate);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.newMessageCompiledBox);
            this.groupBox2.Controls.Add(this.newTemplateMessageFilterBox);
            this.groupBox2.Controls.Add(this.newTemplateFilterLabel);
            this.groupBox2.Controls.Add(this.newTemplateNameLabel);
            this.groupBox2.Controls.Add(this.addNewTemplateBtn);
            this.groupBox2.Controls.Add(this.newTemplateNameBox);
            this.groupBox2.Controls.Add(this.cancelAddNewTemplateBtn);
            this.groupBox2.Controls.Add(this.newPatternMessageStartLabel);
            this.groupBox2.Controls.Add(this.newTemplateMessageStartBox);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            this.invalidTemplateTooltip.SetToolTip(this.groupBox2, resources.GetString("groupBox2.ToolTip"));
            // 
            // newMessageCompiledBox
            // 
            resources.ApplyResources(this.newMessageCompiledBox, "newMessageCompiledBox");
            this.newMessageCompiledBox.Name = "newMessageCompiledBox";
            this.invalidTemplateTooltip.SetToolTip(this.newMessageCompiledBox, resources.GetString("newMessageCompiledBox.ToolTip"));
            this.newMessageCompiledBox.UseVisualStyleBackColor = true;
            // 
            // newTemplateMessageFilterBox
            // 
            resources.ApplyResources(this.newTemplateMessageFilterBox, "newTemplateMessageFilterBox");
            this.newTemplateMessageFilterBox.Name = "newTemplateMessageFilterBox";
            this.invalidTemplateTooltip.SetToolTip(this.newTemplateMessageFilterBox, resources.GetString("newTemplateMessageFilterBox.ToolTip"));
            // 
            // newTemplateFilterLabel
            // 
            resources.ApplyResources(this.newTemplateFilterLabel, "newTemplateFilterLabel");
            this.newTemplateFilterLabel.Name = "newTemplateFilterLabel";
            this.invalidTemplateTooltip.SetToolTip(this.newTemplateFilterLabel, resources.GetString("newTemplateFilterLabel.ToolTip"));
            // 
            // newTemplateNameLabel
            // 
            resources.ApplyResources(this.newTemplateNameLabel, "newTemplateNameLabel");
            this.newTemplateNameLabel.Name = "newTemplateNameLabel";
            this.invalidTemplateTooltip.SetToolTip(this.newTemplateNameLabel, resources.GetString("newTemplateNameLabel.ToolTip"));
            // 
            // addNewTemplateBtn
            // 
            resources.ApplyResources(this.addNewTemplateBtn, "addNewTemplateBtn");
            this.addNewTemplateBtn.Name = "addNewTemplateBtn";
            this.invalidTemplateTooltip.SetToolTip(this.addNewTemplateBtn, resources.GetString("addNewTemplateBtn.ToolTip"));
            this.addNewTemplateBtn.UseVisualStyleBackColor = true;
            this.addNewTemplateBtn.Click += new System.EventHandler(this.OnAddNewParsingTemplate);
            // 
            // newTemplateNameBox
            // 
            resources.ApplyResources(this.newTemplateNameBox, "newTemplateNameBox");
            this.newTemplateNameBox.Name = "newTemplateNameBox";
            this.invalidTemplateTooltip.SetToolTip(this.newTemplateNameBox, resources.GetString("newTemplateNameBox.ToolTip"));
            // 
            // cancelAddNewTemplateBtn
            // 
            resources.ApplyResources(this.cancelAddNewTemplateBtn, "cancelAddNewTemplateBtn");
            this.cancelAddNewTemplateBtn.Name = "cancelAddNewTemplateBtn";
            this.invalidTemplateTooltip.SetToolTip(this.cancelAddNewTemplateBtn, resources.GetString("cancelAddNewTemplateBtn.ToolTip"));
            this.cancelAddNewTemplateBtn.UseVisualStyleBackColor = true;
            this.cancelAddNewTemplateBtn.Click += new System.EventHandler(this.OnCancelAddNewParsingTemplate);
            // 
            // newPatternMessageStartLabel
            // 
            resources.ApplyResources(this.newPatternMessageStartLabel, "newPatternMessageStartLabel");
            this.newPatternMessageStartLabel.Name = "newPatternMessageStartLabel";
            this.invalidTemplateTooltip.SetToolTip(this.newPatternMessageStartLabel, resources.GetString("newPatternMessageStartLabel.ToolTip"));
            // 
            // newTemplateMessageStartBox
            // 
            resources.ApplyResources(this.newTemplateMessageStartBox, "newTemplateMessageStartBox");
            this.newTemplateMessageStartBox.Name = "newTemplateMessageStartBox";
            this.invalidTemplateTooltip.SetToolTip(this.newTemplateMessageStartBox, resources.GetString("newTemplateMessageStartBox.ToolTip"));
            // 
            // addNewParsingTemplateBtn
            // 
            resources.ApplyResources(this.addNewParsingTemplateBtn, "addNewParsingTemplateBtn");
            this.addNewParsingTemplateBtn.Name = "addNewParsingTemplateBtn";
            this.invalidTemplateTooltip.SetToolTip(this.addNewParsingTemplateBtn, resources.GetString("addNewParsingTemplateBtn.ToolTip"));
            this.addNewParsingTemplateBtn.UseVisualStyleBackColor = true;
            this.addNewParsingTemplateBtn.Click += new System.EventHandler(this.OnStartAddNewParsingTemplate);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.invalidTemplateTooltip.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
            // 
            // parsingTemplateSelector
            // 
            resources.ApplyResources(this.parsingTemplateSelector, "parsingTemplateSelector");
            this.parsingTemplateSelector.FormattingEnabled = true;
            this.parsingTemplateSelector.Name = "parsingTemplateSelector";
            this.invalidTemplateTooltip.SetToolTip(this.parsingTemplateSelector, resources.GetString("parsingTemplateSelector.ToolTip"));
            this.parsingTemplateSelector.SelectedIndexChanged += new System.EventHandler(this.OnChangeParsingTemplate);
            this.parsingTemplateSelector.TextUpdate += new System.EventHandler(this.OnSetParsingTemplateName);
            // 
            // commonTemplatesBox
            // 
            resources.ApplyResources(this.commonTemplatesBox, "commonTemplatesBox");
            this.commonTemplatesBox.Controls.Add(this.messageCompiledBox);
            this.commonTemplatesBox.Controls.Add(this.messageFilterBox);
            this.commonTemplatesBox.Controls.Add(this.messageFilterLabel);
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternBox);
            this.commonTemplatesBox.Controls.Add(this.messageStartPatternLabel);
            this.commonTemplatesBox.Name = "commonTemplatesBox";
            this.commonTemplatesBox.TabStop = false;
            this.invalidTemplateTooltip.SetToolTip(this.commonTemplatesBox, resources.GetString("commonTemplatesBox.ToolTip"));
            // 
            // messageCompiledBox
            // 
            resources.ApplyResources(this.messageCompiledBox, "messageCompiledBox");
            this.messageCompiledBox.Name = "messageCompiledBox";
            this.invalidTemplateTooltip.SetToolTip(this.messageCompiledBox, resources.GetString("messageCompiledBox.ToolTip"));
            this.messageCompiledBox.UseVisualStyleBackColor = true;
            this.messageCompiledBox.CheckedChanged += new System.EventHandler(this.OnSetCompiled);
            // 
            // messageFilterBox
            // 
            resources.ApplyResources(this.messageFilterBox, "messageFilterBox");
            this.messageFilterBox.Name = "messageFilterBox";
            this.invalidTemplateTooltip.SetToolTip(this.messageFilterBox, resources.GetString("messageFilterBox.ToolTip"));
            this.messageFilterBox.TextChanged += new System.EventHandler(this.OnSetMessageFilter);
            // 
            // messageFilterLabel
            // 
            resources.ApplyResources(this.messageFilterLabel, "messageFilterLabel");
            this.messageFilterLabel.Name = "messageFilterLabel";
            this.invalidTemplateTooltip.SetToolTip(this.messageFilterLabel, resources.GetString("messageFilterLabel.ToolTip"));
            // 
            // messageStartPatternBox
            // 
            resources.ApplyResources(this.messageStartPatternBox, "messageStartPatternBox");
            this.messageStartPatternBox.Name = "messageStartPatternBox";
            this.invalidTemplateTooltip.SetToolTip(this.messageStartPatternBox, resources.GetString("messageStartPatternBox.ToolTip"));
            this.messageStartPatternBox.TextChanged += new System.EventHandler(this.OnSetMessageStartPattern);
            // 
            // messageStartPatternLabel
            // 
            resources.ApplyResources(this.messageStartPatternLabel, "messageStartPatternLabel");
            this.messageStartPatternLabel.Name = "messageStartPatternLabel";
            this.invalidTemplateTooltip.SetToolTip(this.messageStartPatternLabel, resources.GetString("messageStartPatternLabel.ToolTip"));
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.invalidTemplateTooltip.SetToolTip(this.tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip"));
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Controls.Add(this.saveButton);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.invalidTemplateTooltip.SetToolTip(this.flowLayoutPanel1, resources.GetString("flowLayoutPanel1.ToolTip"));
            // 
            // closeButton
            // 
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Name = "closeButton";
            this.invalidTemplateTooltip.SetToolTip(this.closeButton, resources.GetString("closeButton.ToolTip"));
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            resources.ApplyResources(this.saveButton, "saveButton");
            this.saveButton.Name = "saveButton";
            this.invalidTemplateTooltip.SetToolTip(this.saveButton, resources.GetString("saveButton.ToolTip"));
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.OnSave);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.FullOpen = true;
            // 
            // invalidTemplateTooltip
            // 
            this.invalidTemplateTooltip.ToolTipTitle = "Неверный шаблон";
            // 
            // SettingsDlg
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDlg";
            this.invalidTemplateTooltip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnClosed);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.viewSettingsBox.ResumeLayout(false);
            this.viewSettingsBox.PerformLayout();
            this.fileSettingsBox.ResumeLayout(false);
            this.fileSettingsBox.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button traceBtn;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button debugBtn;
        private System.Windows.Forms.Button infoBtn;
        private System.Windows.Forms.Button warnBtn;
        private System.Windows.Forms.Button errorBtn;
        private System.Windows.Forms.Button fatalBtn;
        private System.Windows.Forms.Button resetColorsBtn;
        private System.Windows.Forms.Button addNewParsingTemplateBtn;
        private System.Windows.Forms.Label newTemplateNameLabel;
        private System.Windows.Forms.TextBox newTemplateNameBox;
        private System.Windows.Forms.Label newPatternMessageStartLabel;
        private System.Windows.Forms.TextBox newTemplateMessageStartBox;
        private System.Windows.Forms.Button cancelAddNewTemplateBtn;
        private System.Windows.Forms.Button addNewTemplateBtn;
        private System.Windows.Forms.Button removeParsingTemplateBtn;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label messageFilterLabel;
        private System.Windows.Forms.TextBox messageFilterBox;
        private System.Windows.Forms.Label newTemplateFilterLabel;
        private System.Windows.Forms.TextBox newTemplateMessageFilterBox;
        private System.Windows.Forms.CheckBox newMessageCompiledBox;
        private System.Windows.Forms.CheckBox messageCompiledBox;
        private System.Windows.Forms.ToolTip invalidTemplateTooltip;
    }
}