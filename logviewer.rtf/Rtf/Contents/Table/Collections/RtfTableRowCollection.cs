using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Contents.Table.Collections
{
    /// <summary>
    /// Represents a collection of rows in a ESCommon.Rtf.RtfTable
    /// </summary>
    public class RtfTableRowCollection : ICollection<RtfTableRow>, IEnumerable<RtfTableRow>
    {
        private RtfTable Table;
        private List<RtfTableRow> list;

        /// <summary>
        /// Gets ESCommon.Rtf.RtfTableRow at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTableRow to get.</param>
        public RtfTableRow this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        /// Gets the number of rows in the collection.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableRowCollection class.
        /// </summary>
        /// <param name="table">Owning table.</param>
        internal RtfTableRowCollection(RtfTable table)
        {
            list = new List<RtfTableRow>();
            Table = table;
        }


        /// <summary>
        /// Adds a new ESCommon.Rtf.RtfTableRow to the collection.
        /// </summary>
        public void Add()
        {
            Add(new RtfTableRow(Table.ColumnCount));
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableRow to the collection.
        /// </summary>
        public void Add(RtfTableRow rtfTableRow)
        {
            OnAddingRow(rtfTableRow);

            int num = list.Count;
            
            list.Add(rtfTableRow);

            rtfTableRow.IndexInternal = num;

            foreach (RtfTableCell cell in rtfTableRow.Cells)
                cell.RowIndexInternal = num;
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableRow objects to the collection.
        /// </summary>
        public void AddRange(RtfTableRow[] rtfTableRows)
        {
            foreach (RtfTableRow row in rtfTableRows)
            {
                if (row != null)
                    this.Add(row);
                else
                    this.Add();
            }
        }

        /// <summary>
        /// Determines if an element is in the collection.
        /// </summary>
        /// <param name="cell">An instance of ESCommon.Rtf.RtfTableRow to locate in the collection.</param>
        public bool Contains(RtfTableRow rtfTableRow)
        {
            return list.Contains(rtfTableRow);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfTableRow[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }


        private void OnAddingRow(RtfTableRow rtfTableRow)
        {
            if (rtfTableRow == null)
                throw (new ArgumentNullException("RtfTableRow cannot be null"));

            if (rtfTableRow.TableInternal != null)
                throw (new InvalidOperationException("RtfTableRow already belongs to a RtfTable"));

            rtfTableRow.TableInternal = this.Table;
            rtfTableRow.DocumentInternal = Table.DocumentInternal;

            rtfTableRow.Width = this.Table.Width;
            rtfTableRow.AlignInternal = this.Table.Align;

            foreach (RtfTableCell cell in rtfTableRow.Cells)
            {
                if (Table.ColumnCount > cell.ColumnIndexInternal)
                    cell.ColumnInternal = Table.Columns[cell.ColumnIndexInternal];
                
                if (!cell.Definition.HasStyle)
                {
                    if (cell.ColumnInternal != null && cell.ColumnInternal.DefaultCellStyle != null)
                        Table.Columns[cell.ColumnIndexInternal].DefaultCellStyle.CopyTo(cell.Definition.StyleInternal);
                    else if (Table.DefaultCellStyle != null)
                        Table.DefaultCellStyle.CopyTo(cell.Definition.StyleInternal);
                }

                if (!cell.Definition.IsWidthSet)
                {
                    if (cell.ColumnInternal != null && cell.ColumnInternal.IsWidthSet)
                        cell.Definition.Width = cell.ColumnInternal.Width;
                }
            }
        }
        

        void ICollection<RtfTableRow>.Clear()
        {

        }

        bool ICollection<RtfTableRow>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfTableRow>.Remove(RtfTableRow item)
        {
            return false;
        }

        IEnumerator<RtfTableRow> IEnumerable<RtfTableRow>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}