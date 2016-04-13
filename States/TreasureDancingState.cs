using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Utilities;

namespace MyBot.States
{
    class TreasureDancingState : State
    {
        //////////Attributes//////////

        /// <summary>
        /// The return distances of the pirates
        /// </summary>
        private int[] piratesDistance;

        /// <summary>
        /// Whether or not the pirates are dancing.
        /// </summary>
        private bool[] pirateIsDancing;


        //////////Methods//////////

        /// <summary>
        /// Creates a new TreasureDancingState state
        /// </summary>
        public TreasureDancingState(Game game)
        {
            this.piratesDistance = new int[game.GetAllMyPiratesCount()];
            this.pirateIsDancing = new bool[game.GetAllMyPiratesCount()];
            for (int i = 0; i < game.GetAllMyPiratesCount(); i++)
            {
                this.piratesDistance[i] = -1;
                this.pirateIsDancing[i] = false;
            }
        }


        /// <summary>
        /// Updates the state, using the given game; Will be called every turn
        /// </summary>
        public override void Update(Game game)
        {
            for (int i = 0; i < game.GetAllMyPiratesCount(); i++)
            {
                Pirate p = game.GetMyPirate(i);
                if (p.State != PirateState.CarryingTreasure)
                {
                    this.pirateIsDancing[i] = false;
                    this.piratesDistance[i] = -1;
                }
                else
                {
                    if (this.piratesDistance[i] != -1 && this.piratesDistance[i] < Game.ManhattanDistance(p, p.InitialLocation))
                        this.pirateIsDancing[i] = true;
                    else
                        this.pirateIsDancing[i] = false;

                    this.piratesDistance[i] = Game.ManhattanDistance(p, p.InitialLocation);
                }
            }
        }

        /// <summary>
        /// Returns whether or not a pirate is dancing
        /// </summary>
        public bool IsPirateDancing(Pirate p)
        {
            return this.pirateIsDancing[p.Id];
        }
    }
}
