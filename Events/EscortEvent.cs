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
    class EscortEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to seek and destroy enemies
        /// </summary>
        private Pirate treasurePirate;

        /// <summary>
        /// The values of attacking each enemy pirate
        /// </summary>
        private const double ESCORT_VALUE = 250;
        private const double ESCORT_GUARD_VALUE = 500;
        private const double IMPENDING_VICTORY_MULTIPLYER = 5;

        //////////Methods//////////

        /// <summary>
        /// Creates a new SeekAndDestroyEvent
        /// </summary>
        public EscortEvent(Pirate treasurePirate):base(2, 1)
        {
            this.treasurePirate = treasurePirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (this.treasurePirate.State != PirateState.CarryingTreasure)
                return;

            //Counts how many of the pirates in the cluster carry treasure, will be used for event value.
            List<Pirate> enemyPirates = new List<Pirate>();

            foreach (Pirate enemy in game.GetEnemyPirates(PirateState.Free))
                if (Game.InAttackRange(enemy, treasurePirate, enemy.AttackRadius + enemy.MaxSpeed))
                    enemyPirates.Add(enemy);

            enemyPirates = enemyPirates.OrderBy(p => Game.EuclideanDistance(p, this.treasurePirate)).ToList();

            if (enemyPirates.Count == 0) //No threats
                return;

            TreasureDancingState treasureDancingState = statesManager.GetState<TreasureDancingState>();
            ImpendingVictoryState impendingVictory = statesManager.GetState<ImpendingVictoryState>();

            //Sends in an escort to destory/block an enemy
            foreach (Pirate myPirate in game.GetMyPirates(PirateState.Free))
            {
                //Bodyguard
                foreach (Pirate enemy in enemyPirates) // try to block an enemy's path/destroy it
                {
                    //1st priority - shoot him!
                    if (game.IsAttackPossible(myPirate, enemy) && !game.IsAttackPossible(enemy, treasurePirate))
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new AttackCommand(myPirate, enemy), base.Id);

                        double value = ESCORT_GUARD_VALUE - Game.EuclideanDistance(enemy, this.treasurePirate);
                        if (impendingVictory.IsVictoryIncoming)
                            value = value * IMPENDING_VICTORY_MULTIPLYER;
                        value *= this.treasurePirate.CarriedTreasureValue;

                        ap.BurnInformation("Made in EscortEvent - case: shoot threat, Value: {0:F3}", value);

                        chooser.AddActionsPack(ap, value);
                    }

                    //2nd priority - ram him!
                    if (Game.ManhattanDistance(enemy, myPirate) <= myPirate.MaxSpeed)
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, enemy), base.Id);

                        ap.AddEnemyPirate(enemy);

                        double value = ESCORT_GUARD_VALUE - Game.EuclideanDistance(enemy, this.treasurePirate) - Game.ManhattanDistance(enemy, myPirate);
                        if (impendingVictory.IsVictoryIncoming)
                            value = value * IMPENDING_VICTORY_MULTIPLYER;
                        value *= this.treasurePirate.CarriedTreasureValue;
                        ap.BurnInformation("Made in EscortEvent - case: ram enemy {0}, Value: {1:F3}", enemy.Id, value);

                        chooser.AddActionsPack(ap, value);
                    }

                    //3rd priority - body block him
                    if (Game.EuclideanDistance(enemy, treasurePirate) >= game.AttackRadius)
                    {
                        foreach (var pair in game.GetCompleteSailOptions(myPirate, ClosestOnCircle(myPirate.AttackRadius, this.treasurePirate, enemy), 0, 1, Terrain.CurrentTreasureLocation, Terrain.EnemyLocation))
                        {
                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                            double value = ESCORT_VALUE - Game.EuclideanDistance(enemy, this.treasurePirate) - Utils.Pow(pair.Value, 1.3);
                            if (impendingVictory.IsVictoryIncoming)
                                value = value * IMPENDING_VICTORY_MULTIPLYER;
                            value *= this.treasurePirate.CarriedTreasureValue;

                            ap.BurnInformation("Made in EscortEvent - case: block enemy {0}, Value: {1:F3}", enemy.Id, value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }
                }
                //This code does escorts even if there are no enemies close, currently not implemented
                /*else if (enemyPirates.Count <= 0)
                {
                    foreach (var pair in game.GetCompleteSailOptions(myPirate, treasurePirate, Terrain.CurrentTreasureLocation, Terrain.EnemyLocation))
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                        double value = 0.8 * (ESCORT_VALUE - Utils.Pow(Game.ManhattanDistance(myPirate, pair.Key), 1.3));
                        if (impendingVictory.IsVictoryIncoming)
                            value = value * IMPENDING_VICTORY_MULTIPLYER;
                        value *= this.treasurePirate.CarriedTreasureValue;

                        ap.BurnInformation("Made in EscortEvent - case: become escort, Value: {0:F3}", value);

                        chooser.AddActionsPack(ap, value);
                    }
                }*/
            }
        }

        /// <summary>
        /// Returns the closest point to the given point on the circle with radius r and given center
        /// </summary>
        private Location ClosestOnCircle(double r, ILocateable center, ILocateable point)
        {
            double x1 = center.GetLocation().Row;
            double y1 = center.GetLocation().Collumn;
            double x2 = point.GetLocation().Row;
            double y2 = point.GetLocation().Collumn;
            double distance = Game.EuclideanDistance(center, point);

            double x3 = (((r * x2) + (distance - r) * x1) / (distance));
            double y3 = (((r * y2) + (distance - r) * y1) / (distance));

            return new Location(Utils.Round(x3), Utils.Round(y3));
        }
    }
}