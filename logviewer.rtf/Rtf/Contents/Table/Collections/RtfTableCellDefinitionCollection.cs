using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Contents.Table.Collections
{
    /// <summary>
    /// Represents a collection of cell definitions in a ESCommon.Rtf.RtfTableRow
    /// </summary>
    public class RtfTableCellDefinitionCollection : IEnumerable<RtfTableCellDefinition>
    {
        private RtfTableRow Row;
        private List<RtfTableCellDefinition> list;

        /// <summary>
        /// Gets ESCommon.Rtf.RtfTableCellDefinition at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTableCellDefinition to get.</param>
        public RtfTableCellDefinition this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        /// Gets the number of cell definitions in the collection.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTableCellDefinitionCollection class.
        /// </summary>
        /// <param name="row">Owning row.</param>
        internal RtfTableCellDefinitionCollection(RtfTableRow row)
        {
            list = new List<RtfTableCellDefinition>();
            Row = row;
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTableCellDefinition to the collection.
        /// </summary>
        internal void AddInternal(RtfTableCellDefinition rtfTableCellDefinition)
        {
            list.Add(rtfTableCellDefinition);
        }


        IEnumerator<RtfTableCellDefinition> IEnumerable<RtfTableCellDefinition>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

    }
}