using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.Actions.Commands;
using MyBot.API;
using MyBot.States;
using MyBot.Utilities;

namespace MyBot.Events
{
    class GarbageCollectionEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to free a drunk pirate
        /// </summary>
        private Pirate myPirate;


        //////////Methods//////////

        /// <summary>
        /// Creates a new GarbageCollectionEvent
        /// </summary>
        public GarbageCollectionEvent(Pirate myPirate):base(3, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// The value of resetting a drunk pirate
        /// </summary>
        private const double GARBAGE_COLLECTION_VALUE = 120; // - (distanceLeft^1.5), range of values is about 200 to 180


        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (this.myPirate.State != PirateState.Drunk || (game.GetPirateSpawnOn(this.myPirate) != null && game.GetPirateSpawnOn(this.myPirate).Owner == Owner.Enemy)) //If my pirate isn't drunk it doesn't need help
                return;


            // try to sail to "collect" the pirate
            foreach (Pirate free in game.GetMyPirates(PirateState.Free))
            {
                var sailOptions = game.GetCompleteSailOptions(free, this.myPirate, Terrain.CurrentTreasureLocation);

                foreach (var pair in sailOptions)
                {
                    if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new RamCommand(free, this.myPirate, pair.Key), base.Id);

                        ap.AddMyPirate(myPirate);

                        double distanceFromSpawnModifier = Utils.Pow(Game.ManhattanDistance(free.InitialLocation, free), 1.5);
                        double timeModifier = Utils.Pow(game.TurnsUntilSober - this.myPirate.TurnToSober, 1.5);
                        double value = Utils.Max(GARBAGE_COLLECTION_VALUE - timeModifier - distanceFromSpawnModifier - Utils.Pow(pair.Value, 1.5), 35);

                        ap.BurnInformation("Made in GarbageCollection event, saving {0}, Value: {1:F3}", this.myPirate.Id, value);

                        chooser.AddActionsPack(ap, value);
                    }
                }
            }
        }
    }
}