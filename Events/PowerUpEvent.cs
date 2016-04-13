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
    class PowerUpEvent : Event
    {
        //////////Attribtues//////////

        /// <summary>
        /// My pirate to collect the power up
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of collecting a power up
        /// </summary>
        private const double ATTACK_POWERUP_VALUE = 350;
        private const double SPEED_POWERUP_VALUE = 350;
        private const double DEFAULT_POWERUP_VALUE = 350;
        private const double PICK_UP_VALUE = 30000;

        //////////Methods//////////

        /// <summary>
        /// Creates a new PowerUp event
        /// </summary>
        public PowerUpEvent(Pirate myPirate):base(1, 2)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {

            if (!this.myPirate.CanMove || this.myPirate.State != PirateState.Free) //If my pirate can't move or isn't free, he can't go to take power ups. If my pirate already has a power up, he shouldn't take another.
                return;

            // Get all the free powerups and sort them by manhattan distance to me
            List<Powerup> freePowerUps = game.GetPowerups().OrderBy(t => Game.ManhattanDistance(t, this.myPirate)).ToList();

            // Get the maximum treasure value
            int maxTreasureValue = 1;
            foreach (Treasure t in game.GetTreasures(TreasureState.FreeToTake, TreasureState.BeingCarried))
            {
                if (t.Value > maxTreasureValue)
                    maxTreasureValue = t.Value;
            }

            // try to sail to each treasure
            foreach (Powerup powerup in freePowerUps)
            {
                if (game.GetPirateOn(powerup) != null) // there is a pirate where the treasure is, most likely a drunk one. Leave it be.
                    continue;

                var sailOptions = game.GetCompleteSailOptions(this.myPirate, powerup, myPirate.DefenseDuration == 0 || Game.ManhattanDistance(myPirate, powerup) <= myPirate.MaxSpeed, Terrain.EnemyLocation, Terrain.CurrentTreasureLocation);

                foreach (var pair in sailOptions)
                {
                    if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                        double value =  - Utils.Pow(pair.Value, 1.5);
                        if (powerup is AttackPowerup)
                            value += ATTACK_POWERUP_VALUE;
                        else if (powerup is SpeedPowerup)
                            value += SPEED_POWERUP_VALUE;
                        else
                            value += DEFAULT_POWERUP_VALUE;
                        if (pair.Value == 0)
                            value += PICK_UP_VALUE;
                        value *= Utils.Pow(0.75, myPirate.PowerupCount);

                        // Add a multiplier for maximum treasure value
                        if (powerup is SpeedPowerup)
                            value *= maxTreasureValue;

                        ap.AddPowerup(powerup);
                        ap.BurnInformation("Made in PowerUpEvent. Fetching PowerUp: {0}. Value: {1:F3}", powerup.Id, value);

                        chooser.AddActionsPack(ap, value);
                    }
                }
            }
        }
    }
}