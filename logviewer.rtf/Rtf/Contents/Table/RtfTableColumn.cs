namespace logviewer.rtf.Rtf.Contents.Table
{
    /// <summary>
    /// Represents a table column.
    /// </summary>
    public class RtfTableColumn
    {
        private bool isWidthSet = false;
        private int _width = 9797;
        private RtfTableCellStyle _defaultCellStyle;

        internal int IndexInternal = -1;
        internal RtfTable TableInternal;

        internal bool IsWidthSet
        {
            get { return isWidthSet; }
        }

        /// <summary>
        /// Gets or sets the default cell style to be applied to the cells in the ESCommon.Rtf.RtfTable if no other cell style properties are set.
        /// </summary>
        public RtfTableCellStyle DefaultCellStyle
        {
            get { return _defaultCellStyle; }
            set
            {
                _defaultCellStyle = value;

                if (TableInternal == null)
                    return;

                foreach (RtfTableRow row in TableInternal.Rows)
                {
                    if (!row[IndexInternal].Definition.HasStyle)
                    {
                        row[IndexInternal].Definition.StyleInternal = _defaultCellStyle;
                        if (row[IndexInternal].IsFormattingIncluded = _defaultCellStyle != null)
                            row[IndexInternal].Formatting = _defaultCellStyle.DefaultParagraphFormatting;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the column in twips.
        /// </summary>
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                isWidthSet = true;

                if (TableInternal == null)
                    return;

                foreach (RtfTableRow row in TableInternal.Rows)
                {
                    row[IndexInternal].Definition.Width = _width;

                    row.ResizeCellsInternal();
                }
            }
        }

        /// <summary>
        /// Gets the owning table.
        /// </summary>
        public RtfTable Table
        {
            get { return TableInternal; }
        }

        /// <summary>
        /// Gets the index of the current column in the table.
        /// </summary>
        public int Index
        {
            get { return IndexInternal; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableColumn class.
        /// </summary>
        public RtfTableColumn()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableColumn class.
        /// </summary>
        /// <param name="width">Width in centimeters.</param>
        public RtfTableColumn(float width)
        {
            Width = TwipConverter.ToTwip(width, MetricUnit.Centimeter);
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableColumn class.
        /// </summary>
        /// <param name="style">Default cell style.</param>
        public RtfTableColumn(RtfTableCellStyle style)
        {
            DefaultCellStyle = style;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableColumn class.
        /// </summary>
        /// <param name="width">Width in centimeters.</param>
        /// <param name="style">Default cell style.</param>
        public RtfTableColumn(float width, RtfTableCellStyle style)
        {
            Width = TwipConverter.ToTwip(width, MetricUnit.Centimeter);
            DefaultCellStyle = style;
        }
    }
}