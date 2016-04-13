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
    class ReturnTreasureEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is returning the treasure
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of attacking each enemy pirate
        /// </summary>
        private const double MOVE_TOWARDS_HOME = 150; // + (distant_left + 1.0) / (distant_left), range of values is about 152 to 150
        private const double RETURN_TREASURE_NOW = 65536;
        private const double SPEED_POWERUP_MULTIPLYER = 1.5;
        private const double ARMADA_BONUS = 300;


        //////////Methods//////////

        /// <summary>
        /// Creates a new event for when the given pirate is returning a treasure home
        /// </summary>
        public ReturnTreasureEvent(Pirate myPirate):base(1, 3)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            // if the pirate is not carrying treasure OR if he can't move, we cannot do anything
            if (this.myPirate.State != PirateState.CarryingTreasure ||
                !this.myPirate.CanMove)
                return;

            /*
            bool isEscorted = false;
            foreach (Pirate ally in game.GetMyFreePirates())
            {
                if (Game.InAttackRange(ally, myPirate, ally.AttackRadius))
                {
                    isEscorted = true;
                    break;
                }
            }*/

            var sailOptions = game.GetCompleteSailOptions(this.myPirate, this.myPirate.InitialLocation, myPirate.DefenseDuration == 0 && Game.ManhattanDistance(myPirate, myPirate.InitialLocation) > myPirate.MaxSpeed, Terrain.EnemyLocation);

            List<Pirate> closestEnemies = game.GetEnemyClosestPirates(myPirate, PirateState.Free);

            // check all possible destinations for returning home
            foreach (var pair in sailOptions)
            {
                if (sailOptions[0].Value > pair.Value && (game.GetPirateOn(pair.Key) == null || game.GetPirateOn(pair.Key).Owner == Owner.Me))
                {
                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                    double prioritize = (pair.Value == 0 ? myPirate.CarriedTreasureValue * 65536 : 5.0 * (pair.Value + 1.0) / (pair.Value));

                    double stayAwayPrioritize = 10;

                    if (closestEnemies.Count > 0)
                    {
                        Pirate enemy = closestEnemies[0];
                        if (Game.ManhattanDistance(enemy, myPirate) > enemy.MaxSpeed)
                        {
                            double distanceToEnemy = Game.EuclideanDistance(pair.Key, enemy);
                            stayAwayPrioritize = 5.0 * (distanceToEnemy) / (distanceToEnemy + 1.0);
                        }
                    }

                    double value = (MOVE_TOWARDS_HOME + prioritize + stayAwayPrioritize - Utils.Pow(pair.Value, 1.3)) * myPirate.CarriedTreasureValue;
                    if (myPirate.HasPowerup(SpeedPowerup.NAME))
                        value = value * SPEED_POWERUP_MULTIPLYER;
                    if (game.GetAllMyPiratesCount() > game.ActionsPerTurn)
                        value += ARMADA_BONUS;

                    ap.BurnInformation("Made in ReturnTreasureEvent, Value: {0:F3}", value);
                    
                    chooser.AddActionsPack(ap, value);
                }
            }
        }
    }
}