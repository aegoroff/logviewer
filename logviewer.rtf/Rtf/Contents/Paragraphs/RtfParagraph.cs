using logviewer.rtf.Rtf.Attributes;
using logviewer.rtf.Rtf.Contents.Text;
using logviewer.rtf.Rtf.Formatting;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    [RtfControlWord("par")]
    public class RtfParagraph : RtfParagraphBase
    {
        internal RtfParagraphBase ParentInternal;

        public RtfParagraphBase Parent
        {
            get { return ParentInternal; }
        }

        [RtfIgnore]
        public RtfParagraphFormatting InheritedFormatting
        {
            get
            {
                if (Parent == null)
                {
                    return null;
                }
                
                return Parent.GetFormatting();
            }
        }
        
        /// <summary>
        /// Gets or sets a Boolean value indicating that the paragraph is a part of a table
        /// </summary>
        [RtfControlWord("intbl")]
        public bool IsPartOfATable
        {
            get { return isPartOfATable; }
            set { isPartOfATable = value; }
        }

        /// <summary>
        /// Gets an array of paragraph contents
        /// </summary>
        [RtfInclude]
        public RtfParagraphContentsCollection Contents
        {
            get { return contents; }
        }

        /// <summary>
        /// Gets an array of paragraphs with inherited formatting
        /// </summary>
        [RtfInclude]
        public RtfParagraphCollection Paragraphs
        {
            get { return paragraphs; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraph class.
        /// </summary>
        public RtfParagraph() : base()
        {
        }

        /// <param name="text">Text to add to paragraph contents.</param>
        public RtfParagraph(string text) : base(text)
        {
        }

        /// <param name="text">Text to add to paragraph contents.</param>
        public RtfParagraph(RtfParagraphContentBase text) : base(text)
        {
        }

        /// <summary>
        /// Returns ESCommon.Rtf.RtfParagraphFormatting of the paragraph.
        /// </summary>
        public override RtfParagraphFormatting GetFormatting()
        {
            return InheritedFormatting;
        }
    }
}