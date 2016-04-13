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
    class GhostTreasureEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to fetch a treasure
        /// </summary>
        private Treasure treasure;

        /// <summary>
        /// This is the value of getting closer to treasure
        /// </summary>
        private const double TREASURE_VALUE = 320;
        private const double ENEMY_HAS_TREASURE_ALLY_CLOSE = 0.9;
        private const double ENEMY_HAS_TREASURE_ALLY_MEDIUM = 0.9;
        private const double ENEMY_HAS_TREASURE_ALLY_FAR = 0.5;
        private const double ALLY_ABOUT_TO_DIE = 0.9;
        private const double SPEED_POWERUP_MULTIPLYER = 2;

        //////////Methods//////////

        /// <summary>
        /// Creates a new GhostTreasureEvent
        /// </summary>
        public GhostTreasureEvent(Treasure treasure):base(3, 2)
        {
            this.treasure = treasure;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (this.treasure.CarryingPirate == null)
                return;
            Pirate carrying = this.treasure.CarryingPirate;

            double multiplyer = Utils.Pow(1 - ((double)game.GetMyPiratesCount(PirateState.CarryingTreasure)) / game.GetMyPiratesCount(PirateState.Free, PirateState.CarryingTreasure), 2);
            multiplyer *= this.treasure.Value;

            switch (carrying.Owner)
            {
                case Owner.Enemy:
                    if (game.GetMyPiratesCount(PirateState.Free) > 0)
                    {
                        Pirate myClosestPirate = game.GetMyClosestPirates(carrying, PirateState.Free)[0];

                        foreach (Pirate free in game.GetMyPirates(PirateState.Free))
                        {
                            if (free == myClosestPirate)
                                continue;

                            var sailOptions = game.GetCompleteSailOptions(free, treasure.InitialLocation, Terrain.EnemyLocation, Terrain.CurrentTreasureLocation);

                            foreach (var pair in sailOptions)
                            {
                                if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                                {
                                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(free, pair.Key), base.Id);

                                    ap.AddTreasure(treasure);

                                    double returnDistance = Game.ManhattanDistance(free.InitialLocation, treasure);
                                    double maxDistance = game.Rows + game.Collumns;
                                    double value = (TREASURE_VALUE - Utils.Pow(pair.Value, 0.9)) * multiplyer * ((maxDistance - returnDistance) / (maxDistance));
                                    if (myClosestPirate.HasPowerup(SpeedPowerup.NAME))
                                        value = value * SPEED_POWERUP_MULTIPLYER;

                                    switch (GetDistanceCategory(game, myClosestPirate, Game.EuclideanDistance(carrying, myClosestPirate)))
                                    {
                                        case DistanceCategory.Close:
                                            value = value * ENEMY_HAS_TREASURE_ALLY_CLOSE;
                                            ap.BurnInformation("Made in GhostTreasureEvent, collecting treasure {0} from {1}, value: {2:F3}, case: close ally", treasure.Id, carrying.Id, value);
                                            break;

                                        case DistanceCategory.Medium:
                                            value = value * ENEMY_HAS_TREASURE_ALLY_MEDIUM;
                                            ap.BurnInformation("Made in GhostTreasureEvent, collecting treasure {0} from {1}, value: {2:F3}, case: medium ally", treasure.Id, carrying.Id, value);
                                            break;

                                        case DistanceCategory.Far:
                                            value = value * ENEMY_HAS_TREASURE_ALLY_FAR;
                                            ap.BurnInformation("Made in GhostTreasureEvent, collecting treasure {0} from {1}, value: {2:F3}, case: far ally", treasure.Id, carrying.Id, value);
                                            break;

                                        default:
                                            continue;

                                    }

                                    chooser.AddActionsPack(ap, value);
                                }
                            }
                        }
                    }
                    break;

                case Owner.Me:
                    if (game.GetEnemyPiratesCount(PirateState.Free) > 0)
                    {
                        Pirate closestEnemy = game.GetEnemyClosestPirates(carrying, PirateState.Free)[0];

                        if (Game.InAttackRange(carrying, closestEnemy, closestEnemy.AttackRadius+closestEnemy.MaxSpeed))
                        {
                            if (game.GetMyPiratesCount(PirateState.Free) > 0)
                            {
                                Pirate closestAlly = game.GetMyClosestPirates(carrying, PirateState.Free)[0];
                                if (Game.InAttackRange(carrying, closestAlly, closestAlly.AttackRadius) || Game.InAttackRange(closestAlly, closestEnemy, closestAlly.AttackRadius)) 
                                    return; // is safe by the power of escort

                                foreach (Pirate free in game.GetMyPirates(PirateState.Free))
                                {
                                    var sailOptions = game.GetCompleteSailOptions(free, treasure.InitialLocation, Terrain.EnemyLocation, Terrain.CurrentTreasureLocation);

                                    foreach (var pair in sailOptions)
                                    {
                                        if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                                        {
                                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(free, pair.Key), base.Id);

                                            ap.AddTreasure(treasure);

                                            double returnDistance = Game.ManhattanDistance(free.InitialLocation, treasure);
                                            double maxDistance = game.Rows + game.Collumns;
                                            double value = (TREASURE_VALUE - Utils.Pow(pair.Value, 0.9)) * multiplyer * ((maxDistance - returnDistance) / (maxDistance));
                                            if (free.HasPowerup(SpeedPowerup.NAME))
                                                value = value * SPEED_POWERUP_MULTIPLYER;

                                            value = value * ALLY_ABOUT_TO_DIE;

                                            ap.BurnInformation("Made in GhostTreasureEvent, collecting treasure {0} from {1}, value: {2:F3}, case: dead ally", treasure.Id, carrying.Id, value);

                                            chooser.AddActionsPack(ap, value);
                                        }

                                    }
                                }
                            }
                            }
                        }
                    break;

                default:
                    break;

            }
        }
        private DistanceCategory GetDistanceCategory(Game game, Pirate attacker, double distance)
        {
            if (distance <= attacker.AttackRadius + ((game.ActionsPerTurn + 1) / 2)) // enemy is close
                return DistanceCategory.Close;

            else if (distance <= attacker.AttackRadius + ((game.ActionsPerTurn * 4) / 3)) // enemy is in medium-range
                return DistanceCategory.Medium;

            else // enemy is far
                return DistanceCategory.Far;
        }

        enum DistanceCategory
        {
            Far,
            Medium,
            Close
        }
    }
}
