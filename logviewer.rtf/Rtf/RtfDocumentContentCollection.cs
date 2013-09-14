using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf
{
    /// <summary>
    /// Represents a collection of RTF document contents.
    /// </summary>
    public class RtfDocumentContentCollection : ICollection<RtfDocumentContentBase>
    {
        private readonly RtfDocument document;
        private readonly List<RtfDocumentContentBase> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfDocumentContentBase at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfDocumentContentBase to get or set.</param>
        public RtfDocumentContentBase this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfDocumentContentCollection class.
        /// </summary>
        /// <param name="document">Owning document.</param>
        internal RtfDocumentContentCollection(RtfDocument document)
        {
            list = new List<RtfDocumentContentBase>();
            this.document = document;
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfDocumentContentBase to the collection.
        /// </summary>
        public void Add(RtfDocumentContentBase item)
        {
            list.Add(item);
            OnAddingItem(item);
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfDocumentContentBase objects to the collection.
        /// </summary>
        public void AddRange(RtfDocumentContentBase[] items)
        {
            foreach (RtfDocumentContentBase item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Clears all the contents of the collection.
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Determines if an element is in the collection.
        /// </summary>
        /// <param name="item">An instance of ESCommon.Rtf.RtfDocumentContentBase to locate in the collection.</param>
        public bool Contains(RtfDocumentContentBase item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfDocumentContentBase[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes specified font from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public bool Remove(RtfDocumentContentBase item)
        {
            return list.Remove(item);
        }


        private void OnAddingItem(RtfDocumentContentBase item)
        {
            item.DocumentInternal = document;
        }


        bool ICollection<RtfDocumentContentBase>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<RtfDocumentContentBase> IEnumerable<RtfDocumentContentBase>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}