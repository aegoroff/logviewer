using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfTabKind
    {
        Normal,
        [RtfControlWord("tqr")]
        FlushRight,
        [RtfControlWord("tqc")]
        Centered,
        [RtfControlWord("tqdec")]
        Decimal
    }

    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfTabLead
    {
        None,
        [RtfControlWord("tldot")]
        Dots,
        [RtfControlWord("tlmdot")]
        MiddleDots,
        [RtfControlWord("tlhyph")]
        Hyphens,
        [RtfControlWord("tlul")]
        Underline,
        [RtfControlWord("tlth")]
        ThickLine,
        [RtfControlWord("tleq")]
        EqualSign
    }


    /// <summary>
    /// Represents a tab.
    /// </summary>
    public class RtfTab
    {
        private RtfTabKind _kind = RtfTabKind.Normal;
        private RtfTabLead _lead = RtfTabLead.None;


        /// <summary>
        /// Gets or sets tab kind property.
        /// </summary>
        [RtfControlWord, RtfInclude(ConditionMember="IsNotBar")]
        public RtfTabKind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        /// <summary>
        /// Gets or sets tab lead property.
        /// </summary>
        [RtfControlWord]
        public RtfTabLead Lead
        {
            get { return _lead; }
            set { _lead = value; }
        }

        /// <summary>
        /// Gets tab position in twips. The value is used by RtfWriter.
        /// </summary>
        [RtfControlWord("tx"), RtfInclude(ConditionMember="IsNotBar")]
        public int TabPosition
        {
            get { return Position; }
        }

        /// <summary>
        /// Gets bar tab position in twips. The value is used by RtfWriter.
        /// </summary>
        [RtfControlWord("tb"), RtfInclude(ConditionMember = "Bar")]
        public int BarPosition
        {
            get { return Position; }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether a vertical bar is drawn at the tab position.
        /// </summary>
        public bool Bar { get; set; }

        /// <summary>
        /// Condition member used by RtfWriter.
        /// </summary>
        public bool IsNotBar
        {
            get { return !Bar; }
        }

        /// <summary>
        /// Gets or sets tab position in twips.
        /// </summary>
        public int Position { get; set; }


        /// <summary>
        /// Creates an instance of ESCommon.Rtf.RtfTab class.
        /// </summary>
        /// <param name="position">Tab position in twips</param>
        public RtfTab(int position)
        {
            Position = position;
        }

        /// <summary>
        /// Creates an instance of ESCommon.Rtf.RtfTab class.
        /// </summary>
        /// <param name="position">Tab position in twips</param>
        /// <param name="kind">Tab kind</param>
        public RtfTab(int position, RtfTabKind kind)
        {
            Position = position;
            _kind = kind;
        }

        /// <summary>
        /// Creates an instance of ESCommon.Rtf.RtfTab class.
        /// </summary>
        /// <param name="position">Tab position in twips</param>
        /// <param name="lead">Tab lead</param>
        public RtfTab(int position, RtfTabLead lead)
        {
            Position = position;
            _lead = lead;
        }

        /// <summary>
        /// Creates an instance of ESCommon.Rtf.RtfTab class.
        /// </summary>
        /// <param name="position">Tab position in twips</param>
        /// <param name="kind">Tab kind</param>
        /// <param name="lead">Tab lead</param>
        public RtfTab(int position, RtfTabKind kind, RtfTabLead lead)
        {
            Position = position;
            _kind = kind;
            _lead = lead;
        }
    }
}