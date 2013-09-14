using System.Collections;
using System.Collections.Generic;

namespace logviewer.rtf.Rtf.Header
{
    /// <summary>
    /// Represents a collection of ESCommon.Rtf.RtfColor in the header of a RTF document.
    /// </summary>
    public class RtfColorCollection: ICollection<RtfColor>, IEnumerable<RtfColor>
    {
        private List<RtfColor> list;

        /// <summary>
        /// Gets or sets ESCommon.Rtf.RtfColor at specified location.
        /// </summary>
        /// <param name="index">A zero-based index of ESCommon.Rtf.RtfColor to get or set.</param>
        public RtfColor this[int index]
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
        /// Initializes a new instance of ESCommon.Rtf.RtfColorCollection class.
        /// </summary>
        internal RtfColorCollection()
        {
            list = new List<RtfColor>();

            list.Add(RtfColor.Auto);
        }


        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfColor to the collection.
        /// </summary>
        public void Add(RtfColor rtfColor)
        {
            list.Add(rtfColor);
        }

        /// <summary>
        /// Adds the specified ESCommon.Rtf.RtfColor objects to the collection.
        /// </summary>
        public void AddRange(RtfColor[] rtfColors)
        {
            foreach (RtfColor color in rtfColors)
            {
                Add(color);
            }
        }
        
        /// <summary>
        /// Clears all the contents of the collection.
        /// </summary>
        public void Clear()
        {
            list.Clear();

            list.Add(RtfColor.Auto);
        }
        
        /// <summary>
        /// Determines if an element is in the collection.
        /// </summary>
        /// <param name="rtfColor">An instance of ESCommon.Rtf.RtfColor to locate in the collection.</param>
        public bool Contains(RtfColor rtfColor)
        {
            return list.Contains(rtfColor);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RtfColor[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes specified color from the collection.
        /// </summary>
        /// <param name="rtfColor">Color to remove.</param>
        public bool Remove(RtfColor rtfColor)
        {
            bool result = list.Remove(rtfColor);

            if (list.Count == 0)
            {
                list.Add(RtfColor.Auto);
            }

            return result;
        }


        bool ICollection<RtfColor>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<RtfColor> IEnumerable<RtfColor>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}