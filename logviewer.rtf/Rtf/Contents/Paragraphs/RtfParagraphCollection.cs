using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    /// <summary>
    /// Represents a collection of ESCommon.Rtf.RtfParagraph within a paragraph.
    /// </summary>
    public class RtfParagraphCollection: ICollection<RtfParagraph>, IEnumerable<RtfParagraph>
    {
        private RtfParagraphBase Parent;
        private List<RtfParagraph> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfParagraph at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfParagraph to get or set.</param>
        public RtfParagraph this[int index]
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
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphCollection class.
        /// </summary>
        /// <param name="paragraph">Parent paragraph.</param>
        internal RtfParagraphCollection(RtfParagraphBase parent)
        {
            list = new List<RtfParagraph>();
            
            Parent = parent;
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfParagraph to the collection.
        /// </summary>
        public void Add(RtfParagraph rtfParagraph)
        {
            OnAddingParagraph(rtfParagraph);

            list.Add(rtfParagraph);
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfParagraph objects to the collection.
        /// </summary>
        public void AddRange(RtfParagraph[] rtfParagraphs)
        {
            foreach (RtfParagraph paragraph in rtfParagraphs)
            {
                Add(paragraph);
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
        /// <param name="rtfParagraph">An instance of ESCommon.Rtf.RtfParagraph to locate in the collection.</param>
        public bool Contains(RtfParagraph rtfParagraph)
        {
            return list.Contains(rtfParagraph);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfParagraph[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }



        private void OnAddingParagraph(RtfParagraph paragraph)
        {
            if (paragraph == null)
                throw (new ArgumentNullException("RtfParagraph cannot be null"));

            if (paragraph.ParentInternal != null)
                throw (new InvalidOperationException("RtfParagraph already belongs to a paragraph"));

            paragraph.ParentInternal = Parent;
            paragraph.DocumentInternal = Parent.DocumentInternal;
        }



        bool ICollection<RtfParagraph>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfParagraph>.Remove(RtfParagraph item)
        {
            return false;
        }

        public IEnumerator<RtfParagraph> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}