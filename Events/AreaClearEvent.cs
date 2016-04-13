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
    class AreaClearEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to step away from the treasure
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// This is the value of staying away
        /// </summary>
        private const double CLEAR_VALUE = 280; // *(1- treasure-pirates/total-pirates)


        public AreaClearEvent(Pirate myPirate) : base(2, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (!this.myPirate.CanMove || this.myPirate.State != PirateState.Free)
                return;

            int myMaxSteps = myPirate.MaxSpeed;
            int enemyMaxSteps;

            double multiplyer = Utils.Pow(1 - ((double)game.GetMyPiratesCount(PirateState.CarryingTreasure)) / game.GetMyPiratesCount(PirateState.Free, PirateState.CarryingTreasure), 2);

            ThreatenedTreasureState treasureState = statesManager.GetState<ThreatenedTreasureState>();
            List<Treasure> treasures = treasureState.GetThreatenedTreasures().OrderBy(t => Game.ManhattanDistance(t, this.myPirate)).ToList();

            while (treasures.Count > 0 && game.GetPirateOn(treasures[0]) != null)
                treasures.Remove(treasures[0]);

            if (treasures.Count == 0)
                return;

            if (Game.ManhattanDistance(this.myPirate, treasures[0]) > myMaxSteps) //only do area clear if close enough to capture treasure
                return;

            Treasure closestTreasure = treasures[0];
            multiplyer *= closestTreasure.Value;

            foreach (Pirate enemy in game.GetEnemyPirates(PirateState.Free))
            {
                enemyMaxSteps = enemy.MaxSpeed;

                if (Game.ManhattanDistance(enemy, closestTreasure) > enemyMaxSteps)//only do area clear if enemy is close enough to capture treasure
                    continue;

                if (!Game.InAttackRange(this.myPirate, closestTreasure, this.myPirate.AttackRadius + 1)) //Get closer.
                {
                    var sailOptions = game.GetCompleteSailOptions(this.myPirate, closestTreasure, (int)this.myPirate.AttackRadius + 1, (int)this.myPirate.AttackRadius + 1, Terrain.InEnemyRange, Terrain.CurrentTreasureLocation);
                    foreach (var pair in sailOptions)
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), this.Id);

                        ap.AddTreasure(treasures[0]);

                        double value = CLEAR_VALUE * multiplyer;

                        ap.BurnInformation("Made in AreaClearEvent on Treasure {0} - case: not close enough, Value {1:F3}", closestTreasure.Id, value);

                        chooser.AddActionsPack(ap, value);
                    }
                }
                else if (Game.InAttackRange(this.myPirate, closestTreasure, this.myPirate.AttackRadius) && this.myPirate.CanAttack && enemy.DefenseDuration == 0 && Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius)) //Attack enemy.
                {
                    ActionsPack ap = ActionsPack.NewCommandPack(game, new AttackCommand(this.myPirate, enemy), this.Id);

                    ap.AddTreasure(treasures[0]);
                    ap.AddEnemyPirate(enemy);

                    double value = CLEAR_VALUE * multiplyer;

                    ap.BurnInformation("Made in AreaClearEvent on Treasure {0} - case: too close, attack pirate {1}, Value: {2:F3}", closestTreasure.Id, enemy.Id, value);

                    chooser.AddActionsPack(ap, value);
                }
                else if (Game.InAttackRange(this.myPirate, closestTreasure, this.myPirate.AttackRadius + 1)) //Stay Still. TODO: ask Adi/Itai why isn't this an else
                {
                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, this.myPirate), this.Id);
                    ap.AddTreasure(closestTreasure);

                    double value = CLEAR_VALUE * multiplyer;

                    ap.BurnInformation("Made in AreaClearEvent on Treasure {0} - case: close enough, Value {1:F3}", closestTreasure.Id, value);

                    chooser.AddActionsPack(ap, value);
                }

            }
            
        }
    }
}