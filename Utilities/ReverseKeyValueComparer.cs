using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Utilities
{
    /// <summary>
    /// A reverse comparer
    /// </summary>
    class ReverseKeyValueComparer<T, P> : IComparer<KeyValuePair<T, P>> where T : System.IComparable
    {
        /// <summary>
        /// An inverse compare
        /// </summary>
        public int Compare(KeyValuePair<T, P> a, KeyValuePair<T, P> b)
        {
            return b.Key.CompareTo(a.Key);
        }
    }
}
