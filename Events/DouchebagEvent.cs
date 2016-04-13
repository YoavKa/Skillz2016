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
    class DouchebagEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The location we want to send douches to
        /// </summary>
        private Cluster<Pirate> hotspot;

        /// <summary>
        /// The values of each pirate in the cluster depending whether he carries a treasure or not
        /// </summary>
        private const double TREASURE_VAL = 80;
        private const double NO_TREASURE_VAL = 10;
        private const double PROTECTOR_VAL = 100;

        /// <summary>
        /// The base value of acting and becoming a douch
        /// </summary>
        private const double DOUCH_SPAWN_CAMP = 625;
        private const double DOUCH_CLUSTER_CAMP = 220;
        private const double DOUCH_BECOME = 80;
        private const double IMPENDING_POWERUP_MULTIPLYER = 50;



        //////////Methods//////////

        /// <summary>
        /// Creates a new DouchebagEvent
        /// </summary>
        public DouchebagEvent(Cluster<Pirate> cluster, Game game):base(3, 1)
        {
            this.hotspot = cluster;
            game.Log(LogType.Clusters, LogImportance.Important, "Enemy Hotspot: " + hotspot.GetLocation());
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            //Counts how many of the pirates in the cluster carry treasure, will be used for event value.
            List<Pirate> treasurePiratesInCluster = this.hotspot.Intersect(game.GetEnemyPirates(PirateState.CarryingTreasure)).ToList();
            int valueInCluster = treasurePiratesInCluster.Sum(pirate => pirate.CarriedTreasureValue);
            List<Pirate> protectors = game.GetEnemyPiratesInAttackRange(this.hotspot, PirateState.Free);

            bool myPirateInCluster = false;

            int drunkDouches = game.GetMyPiratesInAttackRange(this.hotspot, PirateState.Drunk).Count;

            //Checks if there is a pirate in the cluster already, if so - tells it to be a smart douchebag!
            foreach (Pirate myPirate in game.GetMyPirates(PirateState.Free))
            {
                if (Game.InAttackRange(myPirate, this.hotspot, myPirate.AttackRadius)) // pirate is in cluster already!
                {
                    myPirateInCluster = true;
                    //Release the douche!;
                    if (treasurePiratesInCluster.Count > 0)
                    {
                        foreach (Pirate enemy in treasurePiratesInCluster) // go to an enemy with treasure spawn to lock it (act as a douch)
                        {
                            if (game.GetPirateOn(enemy.InitialLocation) != null && game.GetPirateOn(enemy.InitialLocation) != myPirate)
                                continue;
                            List<Pirate> threats = game.GetEnemyDangerousPiratesInAttackRange(myPirate);

                            var sailOptions = game.GetCompleteSailOptions(myPirate, enemy.InitialLocation, false, Terrain.CurrentTreasureLocation);
                            foreach (var pair in sailOptions)
                            {
                                if (pair.Key.Equals(enemy.InitialLocation) || game.GetEnemyDangerousPiratesInAttackRange(pair.Key).Union(threats).Count() == 0 || myPirate.DefenseDuration > 0)
                                {
                                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                                    ap.AddEnemyPirate(enemy);

                                    double value = DOUCH_SPAWN_CAMP + valueInCluster * TREASURE_VAL + (this.hotspot.Count - treasurePiratesInCluster.Count) * NO_TREASURE_VAL - Game.ManhattanDistance(enemy.InitialLocation, enemy) - Utils.Pow(pair.Value, 1.2);
                                    if (enemy.HasPowerup(SpeedPowerup.NAME) && statesManager.GetState<ImpendingDoomState>().IsDoomIncoming)
                                        value *= IMPENDING_POWERUP_MULTIPLYER;

                                    ap.BurnInformation("Made in DouchebagEvent - case: spawn-camp pirate {0}, Value: {1:F3}", enemy.Id, value);

                                    chooser.AddActionsPack(ap, value);
                                }
                            }
                        }
                    }
                    /*else
                    {
                        foreach (var pair in game.GetCompleteSailOptions(myPirate, this.hotspot, 0, 0, myPirate.MaxSpeed, true, 0, myPirate.MaxSpeed, 2, Terrain.CurrentTreasureLocation, Terrain.EnemyLocation)) // or return to the center of the cluster
                        {
                            //ActionsPack ap = ActionsPack.NewSailPack(game, myPirate, pair.Key, this.Id);
                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                            double value = DOUCH_CLUSTER_CAMP + (this.enemiesInCluster.Count - treasurePiratesInCluster.Count) * NO_TREASURE_VAL - Utils.Pow(pair.Value, 1.2) - PROTECTOR_VAL * protectors.Count + treasurePiratesInCluster.Count * TREASURE_VAL;

                            ap.BurnInformation("Made in DouchebagEvent case: cluster-camp, Value: {0:F3}", value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }*/
                }
            }

            if (!myPirateInCluster && drunkDouches < treasurePiratesInCluster.Count) // no pirate in cluster - send in the douches! (become a doouche)
            {
                foreach (Pirate myPirate in game.GetMyPirates(PirateState.Free))
                {
                    foreach (Pirate enemy in treasurePiratesInCluster) // go to an enemy with treasure spawn to lock it (act as a douch)
                    {
                        var sailOptions = game.GetCompleteSailOptions(myPirate, enemy.InitialLocation, Terrain.CurrentTreasureLocation);
                        foreach (var pair in sailOptions)
                        {
                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                            ap.AddEnemyPirate(enemy);

                            double value = DOUCH_BECOME + valueInCluster * TREASURE_VAL + (this.hotspot.Count - treasurePiratesInCluster.Count) * NO_TREASURE_VAL - Game.ManhattanDistance(enemy.InitialLocation, enemy) - Utils.Pow(pair.Value, 1.5) - PROTECTOR_VAL * protectors.Count;
                            if (enemy.HasPowerup(SpeedPowerup.NAME) && statesManager.GetState<ImpendingDoomState>().IsDoomIncoming)
                                value *= IMPENDING_POWERUP_MULTIPLYER;
                            ap.BurnInformation("Made in DouchebagEvent case: become spawn-camper on pirate {0}, Value: {1:F3}", enemy.Id, value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }
                    /*var sailOptions = game.GetCompleteSailOptions(myPirate, this.hotspot, 0, 0, myPirate.MaxSpeed, true, 0, myPirate.MaxSpeed, 2, Terrain.CurrentTreasureLocation);

                    foreach (var pair in sailOptions)
                    {
                        if (sailOptions[0].Value > pair.Value)  //sail only if it's actually better than staying in place
                        {
                            //ActionsPack ap = ActionsPack.NewSailPack(game, myPirate, pair.Key, this.Id);
                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(myPirate, pair.Key), base.Id);

                            double value = DOUCH_BECOME + treasurePiratesInCluster.Count * TREASURE_VAL + (this.enemiesInCluster.Count - treasurePiratesInCluster.Count) * NO_TREASURE_VAL - Utils.Pow(pair.Value, 1.25) - PROTECTOR_VAL * protectors.Count;
                            value = 0.2 * value;

                            ap.BurnInformation("Made in DouchebagEvent case: become cluster-camper, Value: {0:F3}", value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }*/
                }
            }
        }
    }
}