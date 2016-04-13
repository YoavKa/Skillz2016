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
    class FetchTreasureEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to fetch a treasure
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// This is the value of getting closer to treasure
        /// </summary>
        private const double FETCH_VALUE = 320; // - (distance_left)^0.9) * (1- treasure-pirates/total-pirates)
        private const double SPEED_POWERUP_MULTIPLYER = 2;
        private const double GAME_START_MULT = 30000;



        //////////Methods//////////

        /// <summary>
        /// Creates a new FetchTreasureEvent
        /// </summary>
        public FetchTreasureEvent(Pirate myPirate):base(1, 2)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (!this.myPirate.CanMove || this.myPirate.State != PirateState.Free) //If my pirate can't move or isn't free, he can't go to fetch treasures.
                return;

            // Get threatened treasure state
            ThreatenedTreasureState treasureState = statesManager.GetState<ThreatenedTreasureState>();
            // Get all the free treasures and sort them by manhattan distance to me
            List<Treasure> freeTreasures = treasureState.GetSafeTreasures().OrderBy(t => Game.ManhattanDistance(t, this.myPirate)).ToList();


            // calculate the maximum amount of treasures to check
            int maxTreasuresToCheck = (int) Utils.Min(freeTreasures.Count, game.GetMyPiratesCount(PirateState.Free), game.ActionsPerTurn, game.MaxCommandsPerTurn);

            // remove all the excess treasures, based on their value
            Dictionary<int, int> usedTreasuresDict = new Dictionary<int, int>();   // the key is the value of the treasure, and the value is how many treasures with the value are already in the list
            for (int i = 0; i < freeTreasures.Count; )
            {
                if (usedTreasuresDict.ContainsKey(freeTreasures[i].Value))
                {
                    usedTreasuresDict[freeTreasures[i].Value]++;
                    if (usedTreasuresDict[freeTreasures[i].Value] > maxTreasuresToCheck)
                        freeTreasures.RemoveAt(i);
                    else
                        i++;
                }
                else
                {
                    usedTreasuresDict.Add(freeTreasures[i].Value, 1);
                    i++;
                }
            }


            double multiplyer = Utils.Pow(1 - ((double)game.GetMyPiratesCount(PirateState.CarryingTreasure)) / game.GetMyPiratesCount(PirateState.Free, PirateState.CarryingTreasure), 2);

            // try to sail to each treasure
            foreach (Treasure t in freeTreasures)
            {
                if (game.GetPirateOn(t) != null) // there is a pirate where the treasure is, most likely a drunk one. Leave it be.
                    continue;

                ILocateable dest = t;
                // search for a speed powerup on the way to the treasure
                foreach (Powerup up in game.GetPowerups())
                {
                    if (up is SpeedPowerup && Game.OnTrack(myPirate, t, up))
                    {
                        dest = up;
                        break;
                    }
                }

                var sailOptions = game.GetCompleteSailOptions(this.myPirate, dest, Terrain.EnemyLocation, Terrain.CurrentTreasureLocation);
                int maxActionsUsed = (int)Utils.Min(this.myPirate.MaxSpeed, game.ActionsPerTurn, Game.ManhattanDistance(this.myPirate, dest));

                foreach (var pair in sailOptions)
                {
                    if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                        ap.AddTreasure(t);

                        double returnDistance = Game.ManhattanDistance(this.myPirate.InitialLocation, t);
                        double maxDistance = game.Rows + game.Collumns;
                        double value = (FETCH_VALUE - Utils.Pow(pair.Value, 0.9)) * multiplyer * ((maxDistance - returnDistance) / (maxDistance));
                        if (game.GetTreasuresCount(TreasureState.BeingCarried, TreasureState.Taken) == 0 && Game.ManhattanDistance(pair.Key, this.myPirate) == maxActionsUsed)
                            value += GAME_START_MULT;
                        value *= t.Value;
                        if (myPirate.HasPowerup(SpeedPowerup.NAME))
                            value *= SPEED_POWERUP_MULTIPLYER;

                        ap.BurnInformation("Made in FetchTreasureEvent. Fetching Treasure: {0}, value: {1:F3}", t.Id, value);

                        chooser.AddActionsPack(ap, value);
                    }
                }
            }
        }
    }
}