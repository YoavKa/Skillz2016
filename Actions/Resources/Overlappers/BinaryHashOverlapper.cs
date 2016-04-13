using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Actions.Resources.Overlappers
{
    /// <summary>
    /// A binary hash overlapper; checks objects, which represent the use of objects, where if the n-th bit is on the n-th object is used.
    /// CHECKS FOR THE USE OF ALL THE OBJECTS SIMULTANEOUSLY.
    /// For example: myPirates, enemyPirates...
    /// </summary>
    class BinaryHashOverlapper : IHashOverlapper<ActionsPack>
    {
        /// <summary>
        /// The maximum count of bits used inside the hash
        /// </summary>
        private int maxBitsCount;

        /// <summary>
        /// The method to obtain the hash from the ActionsPack
        /// </summary>
        private System.Func<ActionsPack, uint> binaryHashExtractor;

        /// <summary>
        /// Creates a new BinaryHashOverlapper
        /// </summary>
        public BinaryHashOverlapper(int maxBitsCount, System.Func<ActionsPack, uint> binaryHashExtractor)
        {
            this.maxBitsCount = maxBitsCount;
            this.binaryHashExtractor = binaryHashExtractor;
        }

        /// <summary>
        /// Returns the maximum hash possible
        /// </summary>
        public uint GetMaxHash()
        {
            return 1U << maxBitsCount;
        }

        /// <summary>
        /// Returns the hash of the given object
        /// </summary>
        public uint GetHash(ActionsPack ap)
        {
            return this.binaryHashExtractor(ap);
        }

        /// <summary>
        /// Returns whether the two hashes overlap
        /// </summary>
        public bool Overlap(uint hash1, uint hash2)
        {
            return (hash1 & hash2) > 0;
        }
    }
}
