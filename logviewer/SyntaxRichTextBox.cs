// Created by: egr
// Created at: 16.09.2012
// © 2012-2016 Alexander Egorov

using System;
using System.Drawing;

namespace logviewer
{
    public class SyntaxRichTextBox : System.Windows.Forms.RichTextBox
    {
        public void AppendText(Color color, string text)
        {
            this.SuspendLayout();
            var start = this.TextLength;
            this.AppendText(text);
            var end = this.TextLength;

            // Textbox may transform chars, so (end-start) != text.Length
            this.Select(start, end - start);
            {
                this.SelectionColor = color;
                this.SelectionFont = new Font("Verdana", 10, FontStyle.Regular);
            }
            this.SelectionLength = 0; // clear
            this.AppendText(Environment.NewLine);
            this.ResumeLayout();
        }
    }
}