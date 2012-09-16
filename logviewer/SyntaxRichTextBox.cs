using System;
using System.Drawing;

namespace logviewer
{
    public class SyntaxRichTextBox : System.Windows.Forms.RichTextBox
    {
        public void AppendText(Color color, string text)
        {
            var start = this.TextLength;
            this.AppendText(text);
            var end = this.TextLength;

            // Textbox may transform chars, so (end-start) != text.Length
            this.Select(start, end - start);
            {
                this.SelectionColor = color;
                // could set box.SelectionBackColor, box.SelectionFont too.
            }
            this.SelectionLength = 0; // clear
            this.AppendText(Environment.NewLine);
        }
    }
}