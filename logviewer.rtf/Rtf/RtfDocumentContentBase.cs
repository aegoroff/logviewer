namespace logviewer.rtf.Rtf
{
    /// <summary>
    /// Can be used within an RtfDocument
    /// </summary>
    public abstract class RtfDocumentContentBase
    {
        protected RtfDocument Document;

        internal virtual RtfDocument DocumentInternal
        {
            get { return Document; }
            set { Document = value; }
        }
    }
}
