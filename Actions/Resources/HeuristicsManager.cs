using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions.Resources.Overlappers;
using MyBot.API;

namespace MyBot.Actions.Resources
{
    /// <summary>
    /// A manager class for managing the use of multiple PacksHeuristics
    /// </summary>
    class HeuristicsManager
    {
        //////////Attributes//////////

        /// <summary>
        /// The list of the heuristics used by this manager
        /// </summary>
        private List<IPacksHeuristics> heuristics;



        //////////Methods//////////

        /// <summary>
        /// Creates a new HeuristicsManager
        /// </summary>
        public HeuristicsManager(IList<KeyValuePair<double, ActionsPack>> packs, Game game)
        {
            this.heuristics = new List<IPacksHeuristics>();

            ActionsOverlapper actionsOverlapper = new ActionsOverlapper(game.ActionsPerTurn);
            BinaryHashOverlapper myPiratesOverlapper = new BinaryHashOverlapper(game.GetAllMyPiratesCount(), ap => ap.MyPiratesHash);
            BinaryHashOverlapper enemyPiratesOverlapper = new BinaryHashOverlapper(game.GetAllEnemyPiratesCount(), ap => ap.EnemyPiratesHash);

            this.heuristics.Add(new PacksHeuristics1Param(packs, actionsOverlapper));
            for (int i = 0; i < game.GetAllMyPiratesCount(); i++)
                this.heuristics.Add(new PacksHeuristics1Param(packs, new BitHashOverlapper(i, ap => ap.MyPiratesHash)));
            for (int i = 0; i < game.GetAllTreasuresCount(); i++)
                this.heuristics.Add(new PacksHeuristics1Param(packs, new BitHashOverlapper(i, ap => ap.TreasuresHash)));
        }
    
        /// <summary>
        /// Returns the next possible index, based on the PacksHeuristics included
        /// </summary>
        public int NextPossiblePackIndex(int currentIndex, ActionsPack currentPacks)
        {
            // look after the current index
            currentIndex++;

            // assume the next possible index can be obtained by the first heuristics
            int maxIndex = this.heuristics[0].NextPossiblePackIndex(currentIndex, currentPacks);
            int lastMax, tmp;

            // make sure this is true, or make maxIndex better
            do
            {
                lastMax = maxIndex;
                foreach (IPacksHeuristics heuristics in this.heuristics)
                {
                    tmp = heuristics.NextPossiblePackIndex(lastMax, currentPacks);
                    if (tmp > maxIndex)
                    {
                        maxIndex = tmp;
                        break;
                    }
                }
            } while (lastMax != maxIndex); // until we found the right index
            return maxIndex;
        }
    }
}
