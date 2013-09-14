using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Formatting
{
    /// <summary>
    /// Defines units used to specify padding.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseValue)]
    public enum RtfPaddingUnits
    {
        /// <summary>
        /// The reader should ignore padding.
        /// </summary>
        Null = 0,
        /// <summary>
        /// The padding is set in twips.
        /// </summary>
        Twips = 3
    }

    /// <summary>
    /// Represents table row padding.
    /// </summary>
    public class RtfTableRowPadding
    {
        private RtfPaddingUnits leftUnits = RtfPaddingUnits.Twips;
        private RtfPaddingUnits topUnits = RtfPaddingUnits.Twips;
        private RtfPaddingUnits bottomUnits = RtfPaddingUnits.Twips;
        private RtfPaddingUnits rightUnits = RtfPaddingUnits.Twips;


        [RtfControlWord("trspdl"), RtfInclude(ConditionMember = "IsLeftPaddingSet")]
        public int Left { get; set; }

        [RtfControlWord("trspdt"), RtfInclude(ConditionMember = "IsTopPaddingSet")]
        public int Top { get; set; }

        [RtfControlWord("trspdb"), RtfInclude(ConditionMember = "IsBottomPaddingSet")]
        public int Bottom { get; set; }

        [RtfControlWord("trspdr"), RtfInclude(ConditionMember = "IsRightPaddingSet")]
        public int Right { get; set; }


        [RtfControlWord("trspdrfl"), RtfInclude(ConditionMember = "IsLeftPaddingSet")]
        public RtfPaddingUnits LeftUnits
        {
            get { return leftUnits; }
            set { leftUnits = value; }
        }

        [RtfControlWord("trspdrft"), RtfInclude(ConditionMember = "IsTopPaddingSet")]
        public RtfPaddingUnits TopUnits
        {
            get { return topUnits; }
            set { topUnits = value; }
        }

        [RtfControlWord("trspdrfb"), RtfInclude(ConditionMember = "IsBottomPaddingSet")]
        public RtfPaddingUnits BottomUnits
        {
            get { return bottomUnits; }
            set { bottomUnits = value; }
        }

        [RtfControlWord("trspdrfr"), RtfInclude(ConditionMember = "IsRightPaddingSet")]
        public RtfPaddingUnits RightUnits
        {
            get { return rightUnits; }
            set { rightUnits = value; }
        }


        /// <summary>
        /// Gets a Boolean value indicating whether left padding of the row is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsLeftPaddingSet
        {
            get { return Left > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether top padding of the row is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsTopPaddingSet
        {
            get { return Top > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether bottom padding of the row is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsBottomPaddingSet
        {
            get { return Bottom > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether right padding of the row is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsRightPaddingSet
        {
            get { return Right > 0; }
        }
    }
}