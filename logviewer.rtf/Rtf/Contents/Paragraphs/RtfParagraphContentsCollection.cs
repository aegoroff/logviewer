using System;
using System.Collections;
using System.Collections.Generic;
using logviewer.rtf.Rtf.Contents.Text;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    /// <summary>
    /// Represents a collection of paragraph contents.
    /// </summary>
    public class RtfParagraphContentsCollection : ICollection<RtfParagraphContentBase>, IEnumerable<RtfParagraphContentBase>
    {
        private RtfParagraphBase Paragraph;
        private List<RtfParagraphContentBase> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfParagraphContentBase at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfParagraphContentBase to get or set.</param>
        public RtfParagraphContentBase this[int index]
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
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphContentsCollection class.
        /// </summary>
        /// <param name="owner">Owning paragraph.</param>
        internal RtfParagraphContentsCollection(RtfParagraphBase owner)
        {
            list = new List<RtfParagraphContentBase>();

            Paragraph = owner;
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfParagraphContentBase to the collection.
        /// </summary>
        public void Add(RtfParagraphContentBase item)
        {
            OnAddingItem(item);

            list.Add(item);
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfParagraphContentBase objects to the collection.
        /// </summary>
        public void AddRange(RtfParagraphContentBase[] items)
        {
            foreach (RtfParagraphContentBase item in items)
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
        /// <param name="item">An instance of ESCommon.Rtf.RtfParagraphContentBase to locate in the collection.</param>
        public bool Contains(RtfParagraphContentBase item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfParagraphContentBase[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }


        private void OnAddingItem(RtfParagraphContentBase item)
        {
            if (item == null)
                throw (new ArgumentNullException("RtfParagraphContentBase cannot be null"));

            item.ParagraphInternal = this.Paragraph;
        }


        bool ICollection<RtfParagraphContentBase>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfParagraphContentBase>.Remove(RtfParagraphContentBase item)
        {
            return false;
        }

        IEnumerator<RtfParagraphContentBase> IEnumerable<RtfParagraphContentBase>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}