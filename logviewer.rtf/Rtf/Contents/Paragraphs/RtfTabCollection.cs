using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Contents.Paragraphs
{
    /// <summary>
    /// Represents a collection of ESCommon.Rtf.RtfTab within a paragraph.
    /// </summary>
    public class RtfTabCollection: ICollection<RtfTab>, IEnumerable<RtfTab>
    {
        private List<RtfTab> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfTab at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfTab to get or set.</param>
        public RtfTab this[int index]
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
        /// Initializes a new instance of ESCommon.Rtf.RtfTabCollection class.
        /// </summary>
        /// <param name="paragraph">Parent paragraph.</param>
        internal RtfTabCollection()
        {
            list = new List<RtfTab>();
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTab to the collection.
        /// </summary>
        public void Add(RtfTab rtfTab)
        {
            OnAddingTab(rtfTab);

            list.Add(rtfTab);
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfTab objects to the collection.
        /// </summary>
        public void AddRange(RtfTab[] rtfTabs)
        {
            foreach (RtfTab tab in rtfTabs)
            {
                Add(tab);
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
        /// <param name="rtfParagraph">An instance of ESCommon.Rtf.RtfTab to locate in the collection.</param>
        public bool Contains(RtfTab rtfTab)
        {
            return list.Contains(rtfTab);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfTab[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }



        private void OnAddingTab(RtfTab rtfTab)
        {
            if (rtfTab == null)
                throw (new ArgumentNullException("RtfTab cannot be null"));

            foreach (RtfTab t in list)
            {
                if (t.Position == rtfTab.Position)
                {
                    throw (new InvalidOperationException("Cannot insert two tabs at one position"));
                }
            }
        }



        bool ICollection<RtfTab>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<RtfTab>.Remove(RtfTab item)
        {
            return false;
        }

        public IEnumerator<RtfTab> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}