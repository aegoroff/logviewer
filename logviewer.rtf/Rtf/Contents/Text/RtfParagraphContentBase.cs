using logviewer.rtf.Rtf.Contents.Paragraphs;

namespace logviewer.rtf.Rtf.Contents.Text
{
    /// <summary>
    /// Can be used within a paragraph
    /// </summary>
    public abstract class RtfParagraphContentBase
    {
        internal RtfParagraphBase ParagraphInternal;

        public RtfParagraphBase Paragraph
        {
            get { return ParagraphInternal; }
        }
    }
}
