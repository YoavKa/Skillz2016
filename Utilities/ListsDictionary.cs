using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Utilities
{
    /// <summary>
    /// A lists of dictionaries
    /// </summary>
    class ListsDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        /// <summary>
        /// Adds a new value to the dictionary, at the given key
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (!base.ContainsKey(key))
                base[key] = new List<TValue>();
            if (!base[key].Contains(value))
                base[key].Add(value);
        }
    }
}
