using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Contents.Text
{
    /// <summary>
    /// Represents plain text.
    /// </summary>
    public class RtfText : RtfTextBase
    {
        /// <summary>
        /// Gets string value of the text.
        /// </summary>
        [RtfTextData]
        public string Text
        {
            get { return sb.ToString(); }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfText class.
        /// </summary>
        public RtfText()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfText class.
        /// </summary>
        public RtfText(string text) : base(text)
        {
        }
    }
}