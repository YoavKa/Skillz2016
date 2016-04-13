using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Utilities;

namespace MyBot.States
{
    class ThreatenedTreasureState : State
    {
        //////////Attributes//////////

        /// <summary>
        /// The actuall treasures list
        /// </summary>
        private List<Treasure> treasureList;

        /// <summary>
        /// The threatened treasures list
        /// </summary>
        private List<Treasure> threatenedTreasures;

        /// <summary>
        /// The safe treasures list
        /// </summary>
        private List<Treasure> safeTreasures;


        //////////Methods//////////

        /// <summary>
        /// Creates a new ThreatenedTreasure state
        /// </summary>
        public ThreatenedTreasureState(Game game)
        {
            this.treasureList = game.GetTreasures(TreasureState.FreeToTake);
            this.threatenedTreasures = new List<Treasure>();
            this.safeTreasures = new List<Treasure>();

            this.Update(game);
        }


        /// <summary>
        /// Updates the state, using the given game; Will be called every turn, from turn 2 onwards
        /// </summary>
        public override void Update(Game game)
        {
            this.treasureList.Clear();
            this.threatenedTreasures.Clear();
            this.safeTreasures.Clear();

            this.treasureList = game.GetTreasures(TreasureState.FreeToTake);

            foreach (Treasure t in this.treasureList)
            {
                bool threatened = false;
                foreach (Pirate enemy in game.GetEnemyPirates(PirateState.Free))
                {
                    if (enemy.CanAttack && Game.InAttackRange(enemy, t, enemy.AttackRadius))
                    {
                        threatened = true;
                        break;
                    }
                    if (Game.ManhattanDistance(enemy, t) <= Utils.Min(enemy.MaxSpeed, game.ActionsPerTurn))
                    {
                        threatened = true;
                        break;
                    }
                }

                Pirate p = game.GetPirateOn(t);
                if (p != null && p.State == PirateState.Drunk)
                    threatened = true;

                if (threatened)
                    this.threatenedTreasures.Add(t);
                else
                    this.safeTreasures.Add(t);
            }
        }

        /// <summary>
        /// Returns a new list containing the current threatened treasures
        /// </summary>
        public List<Treasure> GetThreatenedTreasures()
        {
            return new List<Treasure>(this.threatenedTreasures);
        }
        /// <summary>
        /// Returns a new list containing the current safe treasures
        /// </summary>
        public List<Treasure> GetSafeTreasures()
        {
            return new List<Treasure>(this.safeTreasures);
        }
    }
}
