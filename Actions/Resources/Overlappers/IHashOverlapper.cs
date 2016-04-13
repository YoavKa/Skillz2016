using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Actions.Resources.Overlappers
{
    /// <summary>
    /// A hash overlapper, to be used when choosing things from a list
    /// </summary>
    interface IHashOverlapper<T>
    {
        /// <summary>
        /// Returns the maximum hash possible
        /// </summary>
        uint GetMaxHash();

        /// <summary>
        /// Returns the hash of the given object
        /// </summary>
        uint GetHash(T t);

        /// <summary>
        /// Returns wether the two hashed overlap
        /// </summary>
        bool Overlap(uint hash1, uint hash2);
    }
}
