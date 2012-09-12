using System;
using System.Drawing;

namespace logviewer
{
    public class SyntaxRichTextBox : System.Windows.Forms.RichTextBox
    {
        /// <summary>
        ///     OnTextChanged
        /// </summary>
        /// <param name="e"> </param>
        protected override void OnTextChanged(EventArgs e)
        {
            this.SuspendLayout();
            this.ProcessAllLines();
            this.ResumeLayout();
        }

        /// <summary>
        ///     Process a line.
        /// </summary>
        private void ProcessLine(string line, int startPosition)
        {
            this.Select(startPosition, line.Length);
            if (line.Contains("ERROR"))
            {
                this.SelectionColor = Color.Red;
            }
            else if (line.Contains("WARN"))
            {
                this.SelectionColor = Color.Orange;
            }
            else if (line.Contains("INFO"))
            {
                this.SelectionColor = Color.Green;
            }
            else if (line.Contains("FATAL"))
            {
                this.SelectionColor = Color.DarkViolet;
            }
            else if (line.Contains("DEBUG"))
            {
                this.SelectionColor = Color.FromArgb(100, 100, 100);
            }
            else if (line.Contains("TRACE"))
            {
                this.SelectionColor = Color.FromArgb(200, 200, 200);
            }
            else
            {
                this.SelectionColor = Color.Black;
            }
        }

        public void ProcessAllLines()
        {
            var nStartPos = 0;
            foreach (var line in this.Lines)
            {
                this.ProcessLine(line, nStartPos);
                nStartPos += line.Length + 1;
            }
        }
    }
}