using System.Text;

namespace logviewer.rtf.Rtf.Contents.Text
{
    public abstract class RtfTextBase : RtfParagraphContentBase
    {
        protected StringBuilder sb;

        protected RtfTextBase()
        {
            sb = new StringBuilder();
        }

        protected RtfTextBase(string text)
        {
            sb = new StringBuilder(text);
        }

        /// <summary>
        /// Appends text to the current object.
        /// </summary>
        /// <param name="text">Text to append.</param>
        public void AppendText(string text)
        {
            sb.Append(text);
        }
    }
}