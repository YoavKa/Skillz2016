using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Utilities;

namespace MyBot.Actions.Resources.Overlappers
{
    /// <summary>
    /// A hash overlapper, to check whenever two ActionsPacks use to many actions together
    /// </summary>
    class ActionsOverlapper : IHashOverlapper<ActionsPack>
    {
        /// <summary>
        /// The maximum count of actions which can be used each turn
        /// </summary>
        private int maxActionsPerTurn;

        /// <summary>
        /// Creates a new ActionsOverlapper
        /// </summary>
        /// <param name="maxActionsPerTurn"></param>
        public ActionsOverlapper(int maxActionsPerTurn)
        {
            this.maxActionsPerTurn = maxActionsPerTurn;
        }

        /// <summary>
        /// Returns the maximum hash possible
        /// </summary>
        public uint GetMaxHash()
        {
            return (uint)Utils.Abs(this.maxActionsPerTurn);
        }

        /// <summary>
        /// Returns the hash of the given object
        /// </summary>
        public uint GetHash(ActionsPack ap)
        {
            return (uint)Utils.Abs(ap.ActionsUsed);
        }

        /// <summary>
        /// Returns wether the two hashed overlap
        /// </summary>
        public bool Overlap(uint hash1, uint hash2)
        {
            return hash1 + hash2 > this.maxActionsPerTurn;
        }
    }
}
