using logviewer.rtf.Rtf.Attributes;
using logviewer.rtf.Rtf.Contents.Text;
using logviewer.rtf.Rtf.Formatting;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    /// <summary>
    /// Represents a formatted paragraph.
    /// </summary>
    [RtfControlWord("pard"), RtfControlWordDenotingEnd("par")]
    public class RtfFormattedParagraph : RtfParagraphBase
    {
        private RtfLanguage _language = RtfLanguage.EnglishUnitedStates;
        private RtfParagraphFormatting _formatting = new RtfParagraphFormatting();
        private RtfTabCollection tabs = new RtfTabCollection();
        private bool isFormattingIncluded = true;
        private bool resetFormatting = true;

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
        /// Gets or sets a Boolean value indicating that font (character) formatting is reset to default value
        /// </summary>
        [RtfControlWord("plain")]
        public bool ResetFormatting
        {
            get { return resetFormatting; }
            set { resetFormatting = value; }
        }

        /// <summary>
        /// Gets or sets paragraph formatting
        /// </summary>
        [RtfInclude(ConditionMember = "IsFormattingIncluded")]
        public RtfParagraphFormatting Formatting
        {
            get { return _formatting; }
            set { _formatting = value; }
        }

        /// <summary>
        /// Default language is English (United States).
        /// </summary>
        [RtfControlWord, RtfInclude(ConditionMember = "IsNotDefaultLanguage")]
        public RtfLanguage Language
        {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>
        /// Gets an array of paragraph tabs.
        /// </summary>
        [RtfInclude]
        public RtfTabCollection Tabs
        {
            get { return tabs; }
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
        /// Condition member used by RtfWriter.
        /// </summary>
        public bool IsNotDefaultLanguage
        {
            get { return _language != DocumentInternal.DefaultLanguage; }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether RtfWriter must include formatting
        /// </summary>
        public bool IsFormattingIncluded
        {
            get { return isFormattingIncluded; }
            set { isFormattingIncluded = value; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedParagraph class
        /// </summary>
        public RtfFormattedParagraph() : base()
        {
        }

        /// <param name="text">Text to add to paragraph contents</param>
        public RtfFormattedParagraph(string text) : base(text)
        {
        }

        /// <param name="text">Text to add to paragraph contents</param>
        public RtfFormattedParagraph(RtfParagraphContentBase text) : base(text)
        {
        }

        /// <param name="formatting">Paragraph formatting</param>
        public RtfFormattedParagraph(RtfParagraphFormatting formatting) : base()
        {
            _formatting = formatting;
        }

        /// <param name="text">Text to add to paragraph contents</param>
        /// <param name="formatting">Paragraph formatting</param>
        public RtfFormattedParagraph(string text, RtfParagraphFormatting formatting) : base(text)
        {
            _formatting = formatting;
        }

        /// <param name="text">Text to add to paragraph contents</param>
        /// <param name="formatting">Paragraph formatting</param>
        public RtfFormattedParagraph(RtfParagraphContentBase text, RtfParagraphFormatting formatting) : base(text)
        {
            _formatting = formatting;
        }

        /// <summary>
        /// Clears all the contents of the paragraph.
        /// </summary>
        public void Clear()
        {
            contents.Clear();
            paragraphs.Clear();
        }

        /// <summary>
        /// Returns ESCommon.Rtf.RtfParagraphFormatting of the paragraph.
        /// </summary>
        public override RtfParagraphFormatting GetFormatting()
        {
            return _formatting;
        }
    }
}
