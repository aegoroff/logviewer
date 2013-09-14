using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Contents.Table.Collections
{
    /// <summary>
    /// Represents a collection of columns in a ESCommon.Rtf.RtfTable
    /// </summary>
    public class RtfTableColumnCollection : ICollection<RtfTableColumn>, IEnumerable<RtfTableColumn>
    {
        private RtfTable Table;
        private List<RtfTableColumn> list;

        /// <summary>
        /// Gets ESCommon.Rtf.RtfTableColumn at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTableColumn to get.</param>
        public RtfTableColumn this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        /// Gets the number of columns in the collection.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableColumnCollection class.
        /// </summary>
        /// <param name="table">Owning table.</param>
        internal RtfTableColumnCollection(RtfTable table)
        {
            list = new List<RtfTableColumn>();
            Table = table;
        }
        
        /// <summary>
        /// Adds a new ESCommon.Rtf.RtfTableColumn to the collection.
        /// </summary>
        public void Add()
        {
            Add(new RtfTableColumn());
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableColumn to the collection.
        /// </summary>
        public void Add(RtfTableColumn rtfTableColumn)
        {
            OnAddingColumn(rtfTableColumn);

            int num = list.Count;
                
            list.Add(rtfTableColumn);

            rtfTableColumn.IndexInternal = num;

            foreach (RtfTableRow row in Table.Rows)
            {
                if (row.Cells.Count > num)
                    row.Cells[num].ColumnInternal = rtfTableColumn;
            }
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableColumn objects to the collection.
        /// </summary>
        public void AddRange(RtfTableColumn[] rtfTableColumns)
        {
            foreach (RtfTableColumn column in rtfTableColumns)
            {
                if (column != null)
                    this.Add(column);
                else
                    this.Add(new RtfTableColumn());
            }
        }

        /// <summary>
        /// Determines if an element is in the collection.
        /// </summary>
        /// <param name="cell">An instance of ESCommon.Rtf.RtfTableColumn to locate in the collection.</param>
        public bool Contains(RtfTableColumn rtfTableColumn)
        {
            return list.Contains(rtfTableColumn);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfTableColumn[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        

        private void OnAddingColumn(RtfTableColumn rtfTableColumn)
        {
            if (rtfTableColumn == null)
                throw (new ArgumentNullException("RtfTableColumn cannot be null"));

            if (rtfTableColumn.TableInternal != null)
                throw (new InvalidOperationException("RtfTableColumn already belongs to a RtfTable"));

            rtfTableColumn.TableInternal = this.Table;
        }


        void ICollection<RtfTableColumn>.Clear()
        {
            
        }

        bool ICollection<RtfTableColumn>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfTableColumn>.Remove(RtfTableColumn item)
        {
            return false;
        }

        IEnumerator<RtfTableColumn> IEnumerable<RtfTableColumn>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}