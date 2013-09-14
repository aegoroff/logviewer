using logviewer.rtf.Rtf.Contents.Text;
using logviewer.rtf.Rtf.Formatting;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    public abstract class RtfParagraphBase : RtfDocumentContentBase
    {
        protected bool isPartOfATable = false;
        protected RtfParagraphContentsCollection contents;
        protected RtfParagraphCollection paragraphs;


        internal override RtfDocument DocumentInternal
        {
            get
            {
                return base.DocumentInternal;
            }
            set
            {
                base.DocumentInternal = value;

                foreach (RtfParagraph paragraph in paragraphs)
                {
                    paragraph.DocumentInternal = value;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraph class.
        /// </summary>
        public RtfParagraphBase()
        {
            contents = new RtfParagraphContentsCollection(this);
            paragraphs = new RtfParagraphCollection(this);
        }

        /// <param name="text">Text to add to paragraph contents.</param>
        public RtfParagraphBase(string text) : this()
        {
            AppendText(text);
        }

        /// <param name="text">Text to add to paragraph contents.</param>
        public RtfParagraphBase(RtfParagraphContentBase text) : this()
        {
            AppendText(text);
        }


        /// <summary>
        /// Add text to paragraph contents
        /// </summary>
        public void AppendText(string text)
        {
            AppendText(new RtfText(text));
        }

        /// <summary>
        /// Add text to paragraph contents
        /// </summary>
        public void AppendText(RtfParagraphContentBase text)
        {
            if (paragraphs.Count == 0)
            {
                contents.Add(text);
            }
            else
            {
                paragraphs[paragraphs.Count - 1].AppendText(text);
            }
        }

        /// <summary>
        /// Add an empty paragraph with inherited formatting
        /// </summary>
        public void AppendParagraph()
        {
            paragraphs.Add(new RtfParagraph());
        }

        /// <summary>
        /// Add a new paragraph with inherited formatting
        /// </summary>
        public void AppendParagraph(string text)
        {
            this.AppendParagraph(new RtfParagraph(text));
        }

        /// <summary>
        /// Add a new paragraph with inherited formatting
        /// </summary>
        public void AppendParagraph(RtfParagraphContentBase text)
        {
            this.AppendParagraph(new RtfParagraph(text));
        }

        /// <summary>
        /// Add a new paragraph with inherited formatting
        /// </summary>
        public void AppendParagraph(RtfParagraph paragraph)
        {
            paragraphs.Add(paragraph);
        }


        public abstract RtfParagraphFormatting GetFormatting();
    }
}