using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Formatting
{
    /// <summary>
    /// Represents table cell borders.
    /// </summary>
    public class RtfTableCellBorders
    {
        private RtfBorder
            top = new RtfBorder(),
            left = new RtfBorder(),
            bottom = new RtfBorder(),
            right = new RtfBorder();
        
        /// <summary>
        /// Gets the top border of the cell.
        /// </summary>
        [RtfControlWord("clbrdrt"), RtfInclude(ConditionMember = "IsTopBorderSet")]
        public RtfBorder Top
        {
            get { return top; }
        }

        /// <summary>
        /// Gets the left border of the cell.
        /// </summary>
        [RtfControlWord("clbrdrl"), RtfInclude(ConditionMember = "IsLeftBorderSet")]
        public RtfBorder Left
        {
            get { return left; }
        }

        /// <summary>
        /// Gets the bottom border of the cell.
        /// </summary>
        [RtfControlWord("clbrdrb"), RtfInclude(ConditionMember = "IsBottomBorderSet")]
        public RtfBorder Bottom
        {
            get { return bottom; }
        }

        /// <summary>
        /// Gets the right border of the cell.
        /// </summary>
        [RtfControlWord("clbrdrr"), RtfInclude(ConditionMember = "IsRightBorderSet")]
        public RtfBorder Right
        {
            get { return right; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether top border of the cell is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsTopBorderSet
        {
            get { return Top.Width > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether left border of the cell is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsLeftBorderSet
        {
            get { return Left.Width > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether bottom border of the cell is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsBottomBorderSet
        {
            get { return Bottom.Width > 0; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether right border of the cell is set. This property is used by RtfWriter.
        /// </summary>
        public bool IsRightBorderSet
        {
            get { return Right.Width > 0; }
        }
    }
}