namespace logviewer
{
    partial class MainDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainDlg));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.encodingLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.refreshButton = new System.Windows.Forms.ToolStripButton();
            this.settingsButton = new System.Windows.Forms.ToolStripButton();
            this.statButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.minLevelBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.maxLevelBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.sortingBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.filterBox = new System.Windows.Forms.ToolStripTextBox();
            this.useRegexp = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.logLoadPercent = new System.Windows.Forms.Label();
            this.logLoadProgress = new System.Windows.Forms.ProgressBar();
            this.pageSizeLabel = new System.Windows.Forms.Label();
            this.last = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.currentPage = new System.Windows.Forms.TextBox();
            this.prev = new System.Windows.Forms.Button();
            this.first = new System.Windows.Forms.Button();
            this.syntaxRichTextBox1 = new logviewer.SyntaxRichTextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.logWatch = new System.IO.FileSystemWatcher();
            this.exportDialog = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatch)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel1,
            this.encodingLabel});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // encodingLabel
            // 
            this.encodingLabel.Name = "encodingLabel";
            resources.ApplyResources(this.encodingLabel, "encodingLabel");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.recentFilesToolStripMenuItem,
            this.toolStripSeparator1,
            this.settingsToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OnOpen);
            // 
            // exportToolStripMenuItem
            // 
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.OnExport);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            resources.ApplyResources(this.recentFilesToolStripMenuItem, "recentFilesToolStripMenuItem");
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // settingsToolStripMenuItem
            // 
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.OnSettings);
            // 
            // refreshToolStripMenuItem
            // 
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.OnRefresh);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            resources.ApplyResources(this.closeToolStripMenuItem, "closeToolStripMenuItem");
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.OnClose);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExit);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutToolStripMenuItem
            // 
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnAbout);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.refreshButton,
            this.settingsButton,
            this.statButton,
            this.toolStripSeparator3,
            this.toolStripLabel1,
            this.minLevelBox,
            this.toolStripLabel2,
            this.maxLevelBox,
            this.toolStripLabel3,
            this.sortingBox,
            this.toolStripSeparator4,
            this.toolStripLabel4,
            this.filterBox,
            this.useRegexp});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.OnOpen);
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Click += new System.EventHandler(this.OnRefresh);
            // 
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.settingsButton, "settingsButton");
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Click += new System.EventHandler(this.OnSettings);
            // 
            // statButton
            // 
            this.statButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.statButton, "statButton");
            this.statButton.Name = "statButton";
            this.statButton.Click += new System.EventHandler(this.OnOpenStatistic);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            // 
            // minLevelBox
            // 
            this.minLevelBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.minLevelBox, "minLevelBox");
            this.minLevelBox.Items.AddRange(new object[] {
            resources.GetString("minLevelBox.Items"),
            resources.GetString("minLevelBox.Items1"),
            resources.GetString("minLevelBox.Items2"),
            resources.GetString("minLevelBox.Items3"),
            resources.GetString("minLevelBox.Items4"),
            resources.GetString("minLevelBox.Items5")});
            this.minLevelBox.Name = "minLevelBox";
            this.minLevelBox.SelectedIndexChanged += new System.EventHandler(this.OnChangeFilter);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            // 
            // maxLevelBox
            // 
            this.maxLevelBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.maxLevelBox, "maxLevelBox");
            this.maxLevelBox.Items.AddRange(new object[] {
            resources.GetString("maxLevelBox.Items"),
            resources.GetString("maxLevelBox.Items1"),
            resources.GetString("maxLevelBox.Items2"),
            resources.GetString("maxLevelBox.Items3"),
            resources.GetString("maxLevelBox.Items4"),
            resources.GetString("maxLevelBox.Items5")});
            this.maxLevelBox.Name = "maxLevelBox";
            this.maxLevelBox.SelectedIndexChanged += new System.EventHandler(this.OnChangeFilter);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            resources.ApplyResources(this.toolStripLabel3, "toolStripLabel3");
            // 
            // sortingBox
            // 
            this.sortingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortingBox.Items.AddRange(new object[] {
            resources.GetString("sortingBox.Items"),
            resources.GetString("sortingBox.Items1")});
            this.sortingBox.Name = "sortingBox";
            resources.ApplyResources(this.sortingBox, "sortingBox");
            this.sortingBox.SelectedIndexChanged += new System.EventHandler(this.OnChangeFilter);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            resources.ApplyResources(this.toolStripLabel4, "toolStripLabel4");
            // 
            // filterBox
            // 
            this.filterBox.Name = "filterBox";
            resources.ApplyResources(this.filterBox, "filterBox");
            this.filterBox.TextChanged += new System.EventHandler(this.OnChangeTextFilter);
            // 
            // useRegexp
            // 
            this.useRegexp.Checked = true;
            this.useRegexp.CheckOnClick = true;
            this.useRegexp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useRegexp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.useRegexp, "useRegexp");
            this.useRegexp.Name = "useRegexp";
            this.useRegexp.CheckedChanged += new System.EventHandler(this.OnChangeRegexpUsage);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.logLoadPercent);
            this.panel1.Controls.Add(this.logLoadProgress);
            this.panel1.Controls.Add(this.pageSizeLabel);
            this.panel1.Controls.Add(this.last);
            this.panel1.Controls.Add(this.next);
            this.panel1.Controls.Add(this.currentPage);
            this.panel1.Controls.Add(this.prev);
            this.panel1.Controls.Add(this.first);
            this.panel1.Controls.Add(this.syntaxRichTextBox1);
            this.panel1.Name = "panel1";
            // 
            // logLoadPercent
            // 
            resources.ApplyResources(this.logLoadPercent, "logLoadPercent");
            this.logLoadPercent.Name = "logLoadPercent";
            // 
            // logLoadProgress
            // 
            resources.ApplyResources(this.logLoadProgress, "logLoadProgress");
            this.logLoadProgress.Name = "logLoadProgress";
            // 
            // pageSizeLabel
            // 
            resources.ApplyResources(this.pageSizeLabel, "pageSizeLabel");
            this.pageSizeLabel.Name = "pageSizeLabel";
            // 
            // last
            // 
            resources.ApplyResources(this.last, "last");
            this.last.Name = "last";
            this.last.UseVisualStyleBackColor = true;
            this.last.Click += new System.EventHandler(this.OnLast);
            // 
            // next
            // 
            resources.ApplyResources(this.next, "next");
            this.next.Name = "next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.OnNextPage);
            // 
            // currentPage
            // 
            resources.ApplyResources(this.currentPage, "currentPage");
            this.currentPage.BackColor = System.Drawing.SystemColors.Control;
            this.currentPage.Name = "currentPage";
            this.currentPage.ReadOnly = true;
            // 
            // prev
            // 
            resources.ApplyResources(this.prev, "prev");
            this.prev.Name = "prev";
            this.prev.UseVisualStyleBackColor = true;
            this.prev.Click += new System.EventHandler(this.OnPrevPage);
            // 
            // first
            // 
            resources.ApplyResources(this.first, "first");
            this.first.Name = "first";
            this.first.UseVisualStyleBackColor = true;
            this.first.Click += new System.EventHandler(this.OnFirst);
            // 
            // syntaxRichTextBox1
            // 
            resources.ApplyResources(this.syntaxRichTextBox1, "syntaxRichTextBox1");
            this.syntaxRichTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.syntaxRichTextBox1.DetectUrls = false;
            this.syntaxRichTextBox1.Name = "syntaxRichTextBox1";
            this.syntaxRichTextBox1.ReadOnly = true;
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // logWatch
            // 
            this.logWatch.EnableRaisingEvents = true;
            this.logWatch.SynchronizingObject = this;
            this.logWatch.Changed += new System.IO.FileSystemEventHandler(this.OnChangeLog);
            // 
            // exportDialog
            // 
            this.exportDialog.DefaultExt = "rtf";
            resources.ApplyResources(this.exportDialog, "exportDialog");
            // 
            // MainDlg
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainDlg";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragAndDropFile);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private SyntaxRichTextBox syntaxRichTextBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripComboBox minLevelBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox maxLevelBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox sortingBox;
        private System.Windows.Forms.ToolStripButton refreshButton;
        private System.IO.FileSystemWatcher logWatch;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripTextBox filterBox;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog exportDialog;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button first;
        private System.Windows.Forms.Button prev;
        private System.Windows.Forms.TextBox currentPage;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button last;
        private System.Windows.Forms.Label pageSizeLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ProgressBar logLoadProgress;
        private System.Windows.Forms.Label logLoadPercent;
        private System.Windows.Forms.ToolStripButton useRegexp;
        private System.Windows.Forms.ToolStripButton settingsButton;
        private System.Windows.Forms.ToolStripButton statButton;
        private System.Windows.Forms.ToolStripStatusLabel encodingLabel;
    }
}

