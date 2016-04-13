using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions.Resources.Overlappers;

namespace MyBot.Actions.Resources
{
    /// <summary>
    /// A heuristics for calculating the next possible ActionsPack in the list
    /// </summary>
    interface IPacksHeuristics
    {
        int NextPossiblePackIndex(int fromIndex, ActionsPack currentPack);
    }

    /// <summary>
    /// A heuristics for calculating the next possible ActionsPack in the list, using 2 Overlappers
    /// </summary>
    class PacksHeuristics2Param : IPacksHeuristics
    {
        //////////Attributes//////////

        /// <summary>
        /// nextPackMatrix[i, a_1, ..., a_n] is the most left location in the list from i forwards,
        ///  whose ALL of the hashes (according to overlappers[a_i]) do NOT overlap with a_i
        /// </summary>
        private int[,,] nextPackMatrix;

        /// <summary>
        /// The overlappers to be used when calculating the heuristics
        /// </summary>
        private IHashOverlapper<ActionsPack> overlapper1, overlapper2;



        //////////Methods//////////

        /// <summary>
        /// Creates a new PacksHeuristics for the given list
        /// </summary>
        /// <param name="packs">The packs to calculate the heuristics for</param>
        /// <param name="overlapper1">The first overlapper to use for the heuristics</param>
        /// <param name="overlapper2">The second overlapper to use for the heuristics</param>
        public PacksHeuristics2Param(IList<KeyValuePair<double, ActionsPack>> packs, IHashOverlapper<ActionsPack> overlapper1, IHashOverlapper<ActionsPack> overlapper2)
        {
            // save the overlappers
            this.overlapper1 = overlapper1;
            this.overlapper2 = overlapper2;

            // create nextPackMatrix
            this.nextPackMatrix = new int[packs.Count, this.overlapper1.GetMaxHash() + 1, this.overlapper2.GetMaxHash() + 1];

            // fill nextPackMatrix up, from the last index backwards
            for (int i = this.nextPackMatrix.GetLength(0) - 1; i >= 0; i--)
            {
                for (int a1 = 0; a1 < this.nextPackMatrix.GetLength(1); a1++)
                {
                    for (int a2 = 0; a2 < this.nextPackMatrix.GetLength(2); a2++)
                    {
                        // if we don't overlap, then this point is cool
                        if (!this.overlapper1.Overlap((uint)a1, this.overlapper1.GetHash(packs[i].Value)) &&
                            !this.overlapper2.Overlap((uint)a2, this.overlapper2.GetHash(packs[i].Value)))
                        {
                            this.nextPackMatrix[i, a1, a2] = i;
                        }
                        // else, if we are at the last collumn, put a value outside the list
                        else if (i == packs.Count - 1)
                        {
                            this.nextPackMatrix[i, a1, a2] = packs.Count;
                        }
                        // else, fetch the value from the next collumn
                        else
                        {
                            this.nextPackMatrix[i, a1, a2] = this.nextPackMatrix[i + 1, a1, a2];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the next possible index, based on the current ActionsPacks
        /// </summary>
        public int NextPossiblePackIndex(int fromIndex, ActionsPack currentPacks)
        {
            // if we are already outside the matrix ;)
            if (fromIndex >= this.nextPackMatrix.GetLength(0))
                return this.nextPackMatrix.GetLength(0);

            // return the next index
            return this.nextPackMatrix[fromIndex, this.overlapper1.GetHash(currentPacks), this.overlapper2.GetHash(currentPacks)];
        }
    }

    /// <summary>
    /// A heuristics for calculating the next possible ActionsPack in the list, using 1 Overlappers
    /// </summary>
    class PacksHeuristics1Param : IPacksHeuristics
    {
        //////////Attributes//////////

        /// <summary>
        /// nextPackMatrix[i, a_1, ..., a_n] is the most left location in the list from i forwards,
        ///  whose ALL of the hashes (according to overlappers[a_i]) do NOT overlap with a_i
        /// </summary>
        private int[,] nextPackMatrix;

        /// <summary>
        /// The overlappers to be used when calculating the heuristics
        /// </summary>
        private IHashOverlapper<ActionsPack> overlapper1;



        //////////Methods//////////

        /// <summary>
        /// Creates a new PacksHeuristics for the given list
        /// </summary>
        /// <param name="packs">The packs to calculate the heuristics for</param>
        /// <param name="overlapper1">The first overlapper to use for the heuristics</param>
        public PacksHeuristics1Param(IList<KeyValuePair<double, ActionsPack>> packs, IHashOverlapper<ActionsPack> overlapper1)
        {
            // save the overlappers
            this.overlapper1 = overlapper1;

            // create nextPackMatrix
            this.nextPackMatrix = new int[packs.Count, this.overlapper1.GetMaxHash() + 1];

            // fill nextPackMatrix up, from the last index backwards
            for (int i = this.nextPackMatrix.GetLength(0) - 1; i >= 0; i--)
            {
                for (int a1 = 0; a1 < this.nextPackMatrix.GetLength(1); a1++)
                {
                    // if we don't overlap, then this point is cool
                    if (!this.overlapper1.Overlap((uint)a1, this.overlapper1.GetHash(packs[i].Value)))
                    {
                        this.nextPackMatrix[i, a1] = i;
                    }
                    // else, if we are at the last collumn, put a value outside the list
                    else if (i == packs.Count - 1)
                    {
                        this.nextPackMatrix[i, a1] = packs.Count;
                    }
                    // else, fetch the value from the next collumn
                    else
                    {
                        this.nextPackMatrix[i, a1] = this.nextPackMatrix[i + 1, a1];
                    }
                }
            }
        }

        /// <summary>
        /// Returns the next possible index, based on the current ActionsPacks
        /// </summary>
        public int NextPossiblePackIndex(int fromIndex, ActionsPack currentPacks)
        {
            // if we are already outside the matrix ;)
            if (fromIndex >= this.nextPackMatrix.GetLength(0))
                return this.nextPackMatrix.GetLength(0);

            // return the next index
            return this.nextPackMatrix[fromIndex, this.overlapper1.GetHash(currentPacks)];
        }
    }
}
