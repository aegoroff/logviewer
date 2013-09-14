using System;
using System.Collections;
using System.Collections.Generic;
using logviewer.rtf.Rtf.Contents.Text;

namespace logviewer.rtf.Rtf.Contents.Table.Collections
{
    /// <summary>
    /// Represents a collection of cells in a ESCommon.Rtf.RtfTableRow
    /// </summary>
    public class RtfTableCellCollection : ICollection<RtfTableCell>, IEnumerable<RtfTableCell>
    {
        private RtfTableRow Row;
        private List<RtfTableCell> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfTableCell at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTableCell to get or set.</param>
        public RtfTableCell this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        /// <summary>
        /// Gets the number of cells in the collection.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableCellCollection class.
        /// </summary>
        /// <param name="row">Owning row.</param>
        internal RtfTableCellCollection(RtfTableRow row)
        {
            list = new List<RtfTableCell>();
            Row = row;
        }


        /// <summary>
        /// Adds a new ESCommon.Rtf.RtfTableCell to the collection.
        /// </summary>
        public void Add()
        {
            Add(new RtfTableCell());
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableCell to the collection.
        /// </summary>
        public void Add(RtfTableCell rtfTableCell)
        {
            OnAddingCell(rtfTableCell);

            int num = list.Count;

            list.Add(rtfTableCell);
            Row.Definitions.AddInternal(rtfTableCell.Definition);

            rtfTableCell.ColumnIndexInternal = num;
            rtfTableCell.RowIndexInternal = Row.Index;

            RtfTable table = Row.Table;

            if (table != null && table.ColumnCount > num)
                rtfTableCell.ColumnInternal = table.Columns[num];

            if (!rtfTableCell.Definition.IsWidthSet)
            {
                if (table != null && table.ColumnCount > 0 && table.Columns[num].IsWidthSet)
                    rtfTableCell.Definition.Width = table.Columns[num].Width;
            }

            if (!rtfTableCell.Definition.HasStyle)
            {
                if (table != null && table.ColumnCount > 0 && table.Columns[num].DefaultCellStyle != null)
                    table.Columns[num].DefaultCellStyle.CopyTo(rtfTableCell.Definition.StyleInternal);
                else if (table != null && table.DefaultCellStyle != null)
                    table.DefaultCellStyle.CopyTo(rtfTableCell.Definition.StyleInternal);
                else
                    rtfTableCell.Definition.StyleInternal = new RtfTableCellStyle();
            }

            Row.ResizeCellsInternal();
        }

        /// <summary>
        /// Adds a new ESCommon.Rtf.RtfTableCell with specified text to the collection.
        /// </summary>
        public void Add(string text)
        {
            this.Add(new RtfTableCell(text));
        }

        /// <summary>
        /// Adds a new ESCommon.Rtf.RtfTableCell with specified text to the collection.
        /// </summary>
        public void Add(RtfParagraphContentBase text)
        {
            this.Add(new RtfTableCell(text));
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableCell objects to the collection.
        /// </summary>
        public void AddRange(RtfTableCell[] rtfTableCells)
        {
            foreach (RtfTableCell cell in rtfTableCells)
            {
                if (cell != null)
                    this.Add(cell);
                else
                    this.Add(new RtfTableCell());
            }
        }

        /// <summary>
        /// Determines if an element is in the collection.
        /// </summary>
        /// <param name="cell">An instance of ESCommon.Rtf.RtfTableCell to locate in the collection.</param>
        public bool Contains(RtfTableCell rtfTableCell)
        {
            return list.Contains(rtfTableCell);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfTableCell[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        

        private void OnAddingCell(RtfTableCell rtfTableCell)
        {
            if (rtfTableCell == null)
                throw (new ArgumentNullException("RtfTableCell cannot be null"));

            if (rtfTableCell.RowInternal != null)
                throw (new InvalidOperationException("RtfTableCell already belongs to a RtfTableRow"));

            rtfTableCell.RowInternal = this.Row;
            rtfTableCell.DocumentInternal = Row.DocumentInternal;
        }
        

        void ICollection<RtfTableCell>.Clear()
        {

        }

        bool ICollection<RtfTableCell>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfTableCell>.Remove(RtfTableCell item)
        {
            return false;
        }

        IEnumerator<RtfTableCell> IEnumerable<RtfTableCell>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}