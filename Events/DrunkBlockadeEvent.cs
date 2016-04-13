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
    class DrunkBlockadeEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is drunk
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of staying away
        /// </summary>
        private const double BLOCKADE_VALUE = 600;


        //////////Methods//////////

        /// <summary>
        /// Creates a new event for when the given pirate is drunk and blocking somebody's spawn
        /// </summary>
        public DrunkBlockadeEvent(Pirate myPirate)
            : base(1, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (this.myPirate.State != PirateState.Drunk)
                return;

            Pirate enemy = game.GetPirateSpawnOn(this.myPirate);

            if (enemy != null && enemy.Owner == Owner.Enemy)
            {
                ActionsPack ap = ActionsPack.NewCommandPack(game, new DoNothingCommand(), base.Id);

                double value = BLOCKADE_VALUE;

                ap.AddEnemyPirate(enemy);
                ap.AddMyPirate(this.myPirate);
                ap.BurnInformation("Made in DrunkBlockadeEvent for pirate {0} on pirate {1}, Value: {2:F3}", this.myPirate, enemy.Id, value);

                chooser.AddActionsPack(ap, value);
            }
        }
    }
}