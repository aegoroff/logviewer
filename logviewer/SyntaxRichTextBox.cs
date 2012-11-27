using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace logviewer
{
    public class SyntaxRichTextBox : RichTextBox
    {
        private const int WmVscroll = 0x115;
        private const int WmMousewheel = 0x20A;
        private const int WmUser = 0x400;
        private const int SbVert = 1;
        private const int EmGetscrollpos = WmUser + 221;

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

        public event EventHandler ScrolledToBottom;

        [DllImport("user32.dll")]
        private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        public bool IsAtMaxScroll()
        {
            int minScroll;
            int maxScroll;
            GetScrollRange(this.Handle, SbVert, out minScroll, out maxScroll);
            Point rtfPoint = Point.Empty;
            SendMessage(this.Handle, EmGetscrollpos, 0, ref rtfPoint);

            return (rtfPoint.Y + this.ClientSize.Height >= maxScroll);
        }

        protected virtual void OnScrolledToBottom(EventArgs e)
        {
            if (this.ScrolledToBottom != null)
            {
                this.ScrolledToBottom(this, e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.IsAtMaxScroll())
            {
                this.OnScrolledToBottom(EventArgs.Empty);
            }

            base.OnKeyUp(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmVscroll || m.Msg == WmMousewheel)
            {
                if (this.IsAtMaxScroll())
                {
                    this.OnScrolledToBottom(EventArgs.Empty);
                }
            }

            base.WndProc(ref m);
        }
    }
}