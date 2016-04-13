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
    class AntiTerroristEvent: Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The location we want to send anti-terrorists to
        /// </summary>
        private Cluster<Pirate> hotspot;
        
        /// <summary>
        /// The base value of the event
        /// </summary>
        private const double BASE_VAL = 210;

        /// <summary>
        /// The values of each pirate in the cluster depending whether he carries a treasure or not
        /// </summary>
        private const double TREASURE_VAL = 40;
        private const double NO_TREASURE_VAL = 25;
        private const double ENEMY_VAL = 5;
        private const double IMPENDING_VICTORY_MULT = 5;

        /// <summary>
        /// The extra value a ship gets if its spawn is in this cluster
        /// </summary>
        private const double IS_HOME_VAL = 50;



        //////////Methods//////////

        /// <summary>
        /// Creates a new AntiTerroristEvent
        /// </summary>
        public AntiTerroristEvent(Cluster<Pirate> cluster, Game game): base(2, 1)
        {
            this.hotspot = cluster;
            game.Log(LogType.Clusters, LogImportance.Important, "My Hotspot: " + hotspot.GetLocation());
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            //Counts how many of the pirates in the cluster carry treasure, will be used for event value.
            List<Pirate> treasurePiratesInCluster = this.hotspot.Intersect(game.GetMyPirates(PirateState.CarryingTreasure)).ToList();
            int valueInCluster = treasurePiratesInCluster.Sum(pirate => pirate.CarriedTreasureValue);
            if (valueInCluster == 0)
                return;
            // the pirates in the threat zone
            List<Pirate> enemiesInCluster = game.GetEnemyPiratesInAttackRange(this.hotspot, PirateState.Free).Union(
                                            game.GetEnemyPiratesInAttackRange(this.hotspot, PirateState.Drunk)).ToList();

            //Checks if there is an enemy in the cluster already, if so - sends in the cavalry!
            foreach (Pirate enemy in enemiesInCluster)
            {
                if (enemy.State == PirateState.Drunk && game.GetPirateSpawnOn(enemy.Location) != null && game.GetPirateSpawnOn(enemy.Location).Owner != Owner.Me)
                {
                    continue;
                }

                foreach (Pirate myPirate in game.GetMyPirates(PirateState.Free))
                {
                    var sailOptions = game.GetCompleteSailOptions(myPirate, enemy, myPirate.DefenseDuration == 0 && !Game.InAttackRange(myPirate, enemy, enemy.AttackRadius), Terrain.CurrentTreasureLocation);

                    foreach (var pair in sailOptions)
                    {
                        if (sailOptions[0].Value > pair.Value)  //sail only if it's actually better than staying in place
                        {
                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), this.Id);

                            ap.AddEnemyPirate(enemy);

                            double value = BASE_VAL + valueInCluster * TREASURE_VAL + (this.hotspot.Count - treasurePiratesInCluster.Count) * NO_TREASURE_VAL + enemiesInCluster.Count * ENEMY_VAL - Utils.Pow(pair.Value, 1.25);
                            if (this.hotspot.Contains(myPirate))
                                value += IS_HOME_VAL;
                            if (statesManager.GetState<ImpendingVictoryState>().IsVictoryIncoming)
                                value *= IMPENDING_VICTORY_MULT;
                            ap.BurnInformation("Made in AntiTeroristEvent against {0}, Value: {1:F3}", enemy.Id, value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }
                }
            }
        }
    }
}
