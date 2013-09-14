using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Contents.Table
{
    /// <summary>
    /// Represents cell definition containing information about cell style and width.
    /// </summary>
    public class RtfTableCellDefinition
    {
        private readonly RtfTableCell cell;
        private RtfTableCellStyle style;

        internal int WidthInternal = 9797;

        internal bool FirstHorizontallyMergedCellInternal
        {
            get { return FirstHorizontallyMergedCell; }
            set
            {
                FirstHorizontallyMergedCell = value;
                if (!FirstHorizontallyMergedCell)
                {
                    return;
                }
                HorizontallyMergedCell = false;
                cell.Clear();
            }
        }

        internal bool HorizontallyMergedCellInternal
        {
            get { return HorizontallyMergedCell; }
            set
            {
                HorizontallyMergedCell = value;
                if (!HorizontallyMergedCell)
                {
                    return;
                }
                FirstHorizontallyMergedCell = false;
                cell.Clear();
            }
        }

        internal bool FirstVerticallyMergedCellInternal
        {
            get { return FirstVerticallyMergedCell; }
            set
            {
                FirstVerticallyMergedCell = value;
                if (!FirstVerticallyMergedCell)
                {
                    return;
                }
                VerticallyMergedCell = false;
                cell.Clear();
            }
        }

        internal bool VerticallyMergedCellInternal
        {
            get { return VerticallyMergedCell; }
            set
            {
                VerticallyMergedCell = value;
                if (!VerticallyMergedCell)
                {
                    return;
                }
                FirstVerticallyMergedCell = false;
                cell.Clear();
            }
        }

        internal bool IsWidthSet { get; private set; }
        internal bool HasStyle { get; private set; }

        internal RtfTableCellStyle StyleInternal
        {
            get { return style; }
            set { style = value; }
        }

        /// <summary>
        /// Gets a Boolean value indicating that the cell is the first cell in a range of table cells to be merged.
        /// </summary>
        [RtfControlWord("clmgf")]
        public bool FirstHorizontallyMergedCell { get; private set; }

        /// <summary>
        /// Gets a Boolean value indicating that the contents of the table cell are merged with those of the preceding cell.
        /// </summary>
        [RtfControlWord("clmrg")]
        public bool HorizontallyMergedCell { get; private set; }

        /// <summary>
        /// Gets a Boolean value indicating that the cell is the first cell in a range of table cells to be vertically merged.
        /// </summary>
        [RtfControlWord("clvmgf")]
        public bool FirstVerticallyMergedCell { get; private set; }

        /// <summary>
        /// Gets a Boolean value indicating that the contents of the table cell are vertically merged with those of the preceding cell.
        /// </summary>
        [RtfControlWord("clvmrg")]
        public bool VerticallyMergedCell { get; private set; }

        /// <summary>
        /// Gets or sets the style of the cell.
        /// </summary>
        [RtfInclude]
        public RtfTableCellStyle Style
        {
            get { return style; }
            set
            {
                style = value;

                if (HasStyle = style != null)
                {
                    cell.Formatting = style.DefaultParagraphFormatting;
                }
            }
        }

        /// <summary>
        /// Gets the right boundary of the cell in twips.
        /// </summary>
        [RtfControlWord("cellx")]
        public int RightBoundary
        {
            get
            {
                int boundary = 0;

                for (int i = 0; i <= cell.ColumnIndexInternal; i++)
                {
                    boundary += cell.RowInternal.Cells[i].Definition.Width;
                }

                return boundary;
            }
        }

        /// <summary>
        /// Gets or sets the width of the cell in twips.
        /// </summary>
        public int Width
        {
            get { return WidthInternal; }
            set
            {
                IsWidthSet = true;
                WidthInternal = value;
            }
        }

        internal RtfTableCellDefinition(RtfTableCell cell)
        {
            this.cell = cell;
        }

        internal RtfTableCellDefinition(RtfTableCell cell, RtfTableCellStyle style)
        {
            this.cell = cell;
            this.style = style;
        }
    }
}