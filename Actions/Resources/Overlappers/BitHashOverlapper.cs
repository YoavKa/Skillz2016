using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Actions.Resources.Overlappers
{
    /// <summary>
    /// A bit hash overlapper; checks objects, which represent the use of objects, where if the n-th bit is on the n-th object is used.
    /// CHECKS FOR THE USE OF ONE OBJECT, MEANING ONLY THE I-TH BIT.
    /// For example: myPirates, enemyPirates...
    /// </summary>
    class BitHashOverlapper : IHashOverlapper<ActionsPack>
    {
        /// <summary>
        /// The bit inside the hash to check
        /// </summary>
        private ulong bit;

        /// <summary>
        /// The method to obtain the hash from the ActionsPack
        /// </summary>
        private System.Func<ActionsPack, uint> binaryHashExtractor;

        /// <summary>
        /// Creates a new BitHashOverlapper
        /// </summary>
        /// <param name="bit"></param>
        /// <param name="binaryHashExtractor"></param>
        public BitHashOverlapper(int bit, System.Func<ActionsPack, uint> binaryHashExtractor)
        {
            this.bit = 1UL << bit;
            this.binaryHashExtractor = binaryHashExtractor;
        }

        /// <summary>
        /// Returns the maximum hash possible
        /// </summary>
        public uint GetMaxHash()
        {
            return 2;
        }

        /// <summary>
        /// Returns the hash of the given object
        /// </summary>
        public uint GetHash(ActionsPack ap)
        {
            if ((this.binaryHashExtractor(ap) & this.bit) > 0)
                return 1;
            else
                return 0;
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
