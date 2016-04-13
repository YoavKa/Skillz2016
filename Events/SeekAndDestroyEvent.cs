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
    class SeekAndDestroyEvent : Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to seek and destroy enemies
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of attacking each enemy pirate
        /// </summary>
        private const double CLOSE_ENEMY_HAS_TREASURE = 510; // - (distanceLeft^1.5), range of values is about 200 to 180
        private const double MEDIUM_RANGE_ENEMY_HAS_TREASURE = 435; // - ((distanceLeft - CLOSE_RANGE)^1.3), range of values is about 175 to 155
        private const double FAR_RANGE_ENEMY_HAS_TREASURE = 160; // - ((distanceLeft - MEDIUM_RANGE)^1.2), range of values is about 150 to 100ish
        private const double CLOSE_ENEMY_HAS_POWERUP = 460; // - (distanceLeft^1.5), range of values is about 200 to 180
        private const double MEDIUM_RANGE_ENEMY_HAS_POWERUP = 390; // - ((distanceLeft - CLOSE_RANGE)^1.3), range of values is about 175 to 155
        private const double FAR_RANGE_ENEMY_HAS_POWERUP = 145; // - ((distanceLeft - MEDIUM_RANGE)^1.2), range of values is about 150 to 100ish
        private const double ENEMY_HAS_NO_TREASURE = 40; // - (distanceLeft^0.9), range of values is about 36 to 20
        private const double ATTACK_POWERUP_MULTIPLYER = 1.2; //value multiplier if the ship has an attack boost
        private const double IMPENDING_DOOM_MULTIPLYER = 10;//value multiplier if doom is coming
        private const double ENEMY_WITH_SPEED_UP_AND_TREASURE = 100;



        //////////Methods//////////

        /// <summary>
        /// Creates a new SeekAndDestroyEvent
        /// </summary>
        public SeekAndDestroyEvent(Pirate myPirate):base(1, 2)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (!this.myPirate.CanMove || this.myPirate.State != PirateState.Free) //If my pirate can't move, or isn't free he can't Seek&Destroy
                return;

            // get all the enemy pirates with treasure, sorted by their manhattan distance to me
            List<Pirate> enemyPiratesWithTreasure = game.GetEnemyClosestPirates(this.myPirate, PirateState.CarryingTreasure);

            ImpendingDoomState impendingDoom = statesManager.GetState<ImpendingDoomState>();

            double pirateCountMultiplier = (double)game.ActionsPerTurn / (double)game.GetAllEnemyPiratesCount();
            pirateCountMultiplier = pirateCountMultiplier > 1 ? 1.0 : pirateCountMultiplier;

            // If the enemy has treasure
            foreach (Pirate enemy in enemyPiratesWithTreasure)
            {
                if (Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius - 1))
                    continue; //We are in attack range to AttackEvent. This should be handled by it.

                int distanceLeftForEnemy = Game.ManhattanDistance(enemy, enemy.InitialLocation);

                if (distanceLeftForEnemy <= 1 || distanceLeftForEnemy < Game.ManhattanDistance(this.myPirate, enemy) / game.ActionsPerTurn)
                    continue; //It's too late now, we can't reach him. Douchebag should handle this.

                //get the sailing directions
                ILocateable dest = enemy;
                int distanceFromDest = (int)this.myPirate.AttackRadius - 2;
                foreach (Powerup up in game.GetPowerups())
                {
                    if (up is AttackPowerup && Game.OnTrack(myPirate, enemy, up))
                    {
                        dest = up;
                        distanceFromDest = 0;
                        break;
                    }
                }

                var sailOptions = game.GetCompleteSailOptions(this.myPirate, enemy, 0, distanceFromDest, Terrain.CurrentTreasureLocation, Terrain.EnemyLocation);

                //Get sailing packs - with Value and Burnt information according to the Distance Category from the enemy
                switch (GetDistanceCategory(game, enemy, Game.EuclideanDistance(myPirate, enemy)))
                {
                    case DistanceCategory.Close: //enemy is close to me
                        foreach (var pair in sailOptions)
                        {
                            if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = CLOSE_ENEMY_HAS_TREASURE * enemy.CarriedTreasureValue - Utils.Pow((pair.Value), 1.5) - Game.ManhattanDistance(pair.Key, enemy.InitialLocation);
                                if (impendingDoom.IsDoomIncoming)
                                    value = value * IMPENDING_DOOM_MULTIPLYER;
                                if (myPirate.HasPowerup(AttackPowerup.NAME))
                                    value = value * ATTACK_POWERUP_MULTIPLYER;

                                //value *= pirateCountMultiplier;

                                ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: close treasure", enemy.Id, value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                        break;

                    case DistanceCategory.Medium: //enemy is in medium range
                        foreach (var pair in sailOptions)
                        {
                            if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = MEDIUM_RANGE_ENEMY_HAS_TREASURE * enemy.CarriedTreasureValue - Utils.Pow((pair.Value) + (game.ActionsPerTurn + 1) / 2 - 2, 1.3) - Game.ManhattanDistance(pair.Key, enemy.InitialLocation);
                                if (enemy.HasPowerup(SpeedPowerup.NAME))
                                    value += ENEMY_WITH_SPEED_UP_AND_TREASURE;
                                if (impendingDoom.IsDoomIncoming)
                                    value = value * IMPENDING_DOOM_MULTIPLYER;
                                if (myPirate.HasPowerup(AttackPowerup.NAME))
                                    value = value * ATTACK_POWERUP_MULTIPLYER;

                                value *= pirateCountMultiplier;

                                ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: medium treasure", enemy.Id, value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                        break;

                    case DistanceCategory.Far: //enemy is far away
                        foreach (var pair in sailOptions)
                        {
                            if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = FAR_RANGE_ENEMY_HAS_TREASURE * enemy.CarriedTreasureValue - Utils.Pow((pair.Value) - game.ActionsPerTurn / 3 - 2, 1.2) - Game.ManhattanDistance(pair.Key, enemy.InitialLocation);
                                if (enemy.HasPowerup(SpeedPowerup.NAME))
                                    value += ENEMY_WITH_SPEED_UP_AND_TREASURE;
                                if (impendingDoom.IsDoomIncoming)
                                    value = value * IMPENDING_DOOM_MULTIPLYER;
                                if (myPirate.HasPowerup(AttackPowerup.NAME))
                                    value = value * ATTACK_POWERUP_MULTIPLYER;

                                value *= pirateCountMultiplier;

                                ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: far treasure", enemy.Id, value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                        break;
                }
            }
            Pirate closest = null;
            if (game.GetEnemyPiratesCount(PirateState.Free) > 0)
                closest = game.GetEnemyClosestPirates(myPirate, PirateState.Free)[0];
            //if the enemy doesn't have a treasure
            foreach (Pirate enemy in game.GetEnemyPirates(PirateState.Free))
            {
                if (Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius))
                    continue; //We are in attack range to AttackEvent. This should be handled by it.
                
                //get the sailing directions
                var sailOptions = game.GetCompleteSailOptions(this.myPirate, enemy, 0, (int) this.myPirate.AttackRadius, Terrain.CurrentTreasureLocation, Terrain.EnemyLocation);

                //Get sailing packs - with Value and Burnt information according to the Distance Category from the enemy
                if (enemy.PowerupCount > 0)
                {
                    switch (GetDistanceCategory(game, enemy, Game.EuclideanDistance(myPirate, enemy)))
                    {
                        case DistanceCategory.Close: //enemy is close to me
                            foreach (var pair in sailOptions)
                            {
                                if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                                {
                                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                    ap.AddEnemyPirate(enemy);

                                    double value = CLOSE_ENEMY_HAS_POWERUP - Utils.Pow((pair.Value), 1.3);
                                    if (myPirate.HasPowerup(AttackPowerup.NAME))
                                        value = value * ATTACK_POWERUP_MULTIPLYER;

                                    value *= pirateCountMultiplier;

                                    ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: close powerup", enemy.Id, value);

                                    chooser.AddActionsPack(ap, value);
                                }
                            }
                            break;

                        case DistanceCategory.Medium: //enemy is in medium range
                            foreach (var pair in sailOptions)
                            {
                                if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                                {
                                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                    ap.AddEnemyPirate(enemy);

                                    double value = MEDIUM_RANGE_ENEMY_HAS_POWERUP - Utils.Pow((pair.Value) + (game.ActionsPerTurn + 1) / 2 - 2, 1.2);
                                    if (myPirate.HasPowerup(AttackPowerup.NAME))
                                        value = value * ATTACK_POWERUP_MULTIPLYER;

                                    value *= pirateCountMultiplier;

                                    ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: medium powerup", enemy.Id, value);

                                    chooser.AddActionsPack(ap, value);
                                }
                            }
                            break;

                        case DistanceCategory.Far: //enemy is far away
                            foreach (var pair in sailOptions)
                            {
                                if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                                {
                                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                    ap.AddEnemyPirate(enemy);

                                    double value = FAR_RANGE_ENEMY_HAS_POWERUP - Utils.Pow((pair.Value) - game.ActionsPerTurn / 3 - 2, 1);
                                    if (myPirate.HasPowerup(AttackPowerup.NAME))
                                        value = value * ATTACK_POWERUP_MULTIPLYER;

                                    value *= pirateCountMultiplier;

                                    ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: far powerup", enemy.Id, value);

                                    chooser.AddActionsPack(ap, value);
                                }
                            }
                            break;
                    }
                }
                else if (enemy == closest || !game.EnemyArmada)
                {
                    if (impendingDoom.IsDoomIncoming)
                        return;

                    if (myPirate.CanAttack) //Seek and Destroy a non-treasure enemy only if you can shoot him
                    {

                        if (Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius))
                            continue; //We are in attack range to AttackEvent. This should be handled by it.;

                        //Get sailing packs
                        foreach (var pair in sailOptions)
                        {
                            if (sailOptions[0].Value > pair.Value) //sail only if it's actually better than staying in place
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = Utils.Max(ENEMY_HAS_NO_TREASURE - Utils.Pow(pair.Value, 0.7), 1.0 / (pair.Value + 1.0));
                                if (myPirate.HasPowerup(AttackPowerup.NAME))
                                    value = value * ATTACK_POWERUP_MULTIPLYER;
                                value *= pirateCountMultiplier;

                                ap.BurnInformation("Made in SeekAndDestroyEvent, against {0}, value: {1:F3}, case: no Treasure", enemy.Id, value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                    }
                }
            }


        }

        private DistanceCategory GetDistanceCategory(Game game, Pirate enemy, double distance)
        {
            if (distance <= enemy.AttackRadius + ((game.ActionsPerTurn + 1) / 2)) // enemy is close
                return DistanceCategory.Close;

            else if (distance <= enemy.AttackRadius + ((game.ActionsPerTurn * 4) / 3)) // enemy is in medium-range
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