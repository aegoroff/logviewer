namespace logviewer.ui
{
    partial class LogParseTemplateControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogParseTemplateControl));
            this.messageCompiledBox = new System.Windows.Forms.CheckBox();
            this.messageFilterBox = new System.Windows.Forms.TextBox();
            this.messageFilterLabel = new System.Windows.Forms.Label();
            this.messageStartPatternBox = new System.Windows.Forms.TextBox();
            this.messageStartPatternLabel = new System.Windows.Forms.Label();
            this.invalidTemplateTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // messageCompiledBox
            // 
            resources.ApplyResources(this.messageCompiledBox, "messageCompiledBox");
            this.messageCompiledBox.Name = "messageCompiledBox";
            this.messageCompiledBox.UseVisualStyleBackColor = true;
            this.messageCompiledBox.CheckedChanged += new System.EventHandler(this.OnSetCompiled);
            // 
            // messageFilterBox
            // 
            resources.ApplyResources(this.messageFilterBox, "messageFilterBox");
            this.messageFilterBox.Name = "messageFilterBox";
            this.messageFilterBox.TextChanged += new System.EventHandler(this.OnSetMessageFilter);
            // 
            // messageFilterLabel
            // 
            resources.ApplyResources(this.messageFilterLabel, "messageFilterLabel");
            this.messageFilterLabel.Name = "messageFilterLabel";
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
            // LogParseTemplateControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.messageCompiledBox);
            this.Controls.Add(this.messageFilterBox);
            this.Controls.Add(this.messageFilterLabel);
            this.Controls.Add(this.messageStartPatternBox);
            this.Controls.Add(this.messageStartPatternLabel);
            this.Name = "LogParseTemplateControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox messageCompiledBox;
        private System.Windows.Forms.TextBox messageFilterBox;
        private System.Windows.Forms.Label messageFilterLabel;
        private System.Windows.Forms.TextBox messageStartPatternBox;
        private System.Windows.Forms.Label messageStartPatternLabel;
        private System.Windows.Forms.ToolTip invalidTemplateTooltip;

    }
}
