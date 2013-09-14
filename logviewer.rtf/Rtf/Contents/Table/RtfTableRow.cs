using System;
using logviewer.rtf.Rtf.Attributes;
using logviewer.rtf.Rtf.Contents.Table.Collections;
using logviewer.rtf.Rtf.Formatting;

namespace logviewer.rtf.Rtf.Contents.Table
{
    /// <summary>
    /// Represents a table row.
    /// </summary>
    [RtfControlWord("trowd"), RtfControlWordDenotingEnd("row")]
    public class RtfTableRow : RtfDocumentContentBase
    {
        private int 
            _width = 9797,
            height = 0;

        private RtfTableRowPadding padding = new RtfTableRowPadding();
        private RtfTableCellDefinitionCollection definitions;
        private RtfTableCellCollection cells;

        internal int IndexInternal = -1;
        internal RtfTable TableInternal;
        internal RtfTableAlign AlignInternal = RtfTableAlign.Center;

        internal override RtfDocument DocumentInternal
        {
            get
            {
                return base.DocumentInternal;
            }
            set
            {
                base.DocumentInternal = value;

                foreach (RtfTableCell cell in cells)
                {
                    cell.DocumentInternal = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the row in twips.
        /// </summary>
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                ResizeCellsInternal();
            }
        }

        /// <summary>
        /// Gets or sets the height of the row in twips. Default is 0 (auto).
        /// </summary>
        [RtfControlWord("trrh")]
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Gets or sets the align of the table row.
        /// </summary>
        [RtfControlWord]
        public RtfTableAlign Align
        {
            get { return AlignInternal; }
            set
            {
                if (TableInternal != null)
                    return;
                
                AlignInternal = value;
            }
        }

        /// <summary>
        /// Gets table row padding.
        /// </summary>
        [RtfInclude]
        public RtfTableRowPadding Padding
        {
            get { return padding; }
        }
        
        /// <summary>
        /// Gets a collection that contains the definitions of the cell in the row.
        /// </summary>
        [RtfInclude]
        public RtfTableCellDefinitionCollection Definitions
        {
            get { return definitions; }
        }

        /// <summary>
        /// Gets a collection of all the cells in the row.
        /// </summary>
        [RtfInclude]
        public RtfTableCellCollection Cells
        {
            get { return cells; }
        }

        /// <summary>
        /// Gets the owning table.
        /// </summary>
        [RtfIgnore]
        public RtfTable Table
        {
            get { return TableInternal; }
        }

        /// <summary>
        /// Gets the index of the current row in the table.
        /// </summary>
        public int Index
        {
            get { return IndexInternal; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableRow class.
        /// </summary>
        public RtfTableRow()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableRow class.
        /// </summary>
        /// <param name="columnCount">Number of columns.</param>
        public RtfTableRow(int columnCount)
        {
            Initialize(columnCount);
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableRow class.
        /// </summary>
        /// <param name="align">Align of the text row.</param>
        public RtfTableRow(RtfTableAlign align)
        {
            AlignInternal = align;
            Initialize();
        }

        private void Initialize()
        {
            cells = new RtfTableCellCollection(this);
            definitions = new RtfTableCellDefinitionCollection(this);
        }

        private void Initialize(int columnCount)
        {
            Initialize();
            
            cells.AddRange(new RtfTableCell[columnCount]);
        }

        /// <summary>
        /// Gets or sets ESCommmon.Rtf.RtfTableCell at a specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTableCell to get or set.</param>
        [RtfIgnore]
        public RtfTableCell this[int index]
        {
            get { return Cells[index]; }
            set { Cells[index] = value; }
        }

        internal void ResizeCellsInternal()
        {
            int
                count = cells.Count,
                availableWidth = _width,
                fixedWidth = 0;

            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Definition.IsWidthSet)
                {
                    count--;
                    availableWidth -= cells[i].Definition.WidthInternal;
                    fixedWidth += cells[i].Definition.WidthInternal;
                }
            }

            if (count > 0)
            {
                int cellWidth = availableWidth / count;

                if (cellWidth < 225)
                    throw (new InvalidOperationException("Too many cells in a row"));

                for (int i = 0; i < cells.Count; i++)
                {
                    if (!cells[i].Definition.IsWidthSet)
                        cells[i].Definition.WidthInternal = availableWidth / count;
                }
            }

            if (fixedWidth > 9797)
                throw (new InvalidOperationException("Width is too large"));
        }
    }
}