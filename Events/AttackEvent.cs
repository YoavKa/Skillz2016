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
    class AttackEvent : Event
    {
        //////////Attribtues//////////

        /// <summary>
        /// My attacking pirate
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of attacking each enemy pirate
        /// </summary>
        private const double ENEMY_HAS_TREASURE = 65536;
        private const double ENEMY_HAS_NO_TREASURE = 60;
        private const double ATTACK_POWERUP_MULTIPLYER = 5;
        private const double ENEMY_HAS_POWERUP = 280;
        private const double GHOST_TREASURE_VAL = 610;

        //////////Methods//////////

        /// <summary>
        /// Creates a new AttackEvent
        /// </summary>
        public AttackEvent(Pirate myPirate):base(1, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (this.myPirate.State != PirateState.Free)
                return;

            bool inRisk = false; // Is my pirate in risk of shooting right now?
            foreach (Pirate threat in game.GetEnemyPirates(PirateState.Free))
            {
                if (Game.InAttackRange(threat, myPirate, threat.AttackRadius) || Game.ManhattanDistance(threat, myPirate) <= threat.MaxSpeed)
                {
                    inRisk = true;
                    break;
                }
            }

            bool isDouche = game.GetPirateSpawnOn(myPirate) != null && game.GetPirateSpawnOn(myPirate).Owner == Owner.Enemy; // If my pirate is a douche, he shouldn't ram the enemy since spawn-blocking is better.


            // try to attack every enemy pirate which is carrying a treasure / which is free
            foreach (Pirate enemy in game.GetEnemyPirates(PirateState.CarryingTreasure, PirateState.Free, PirateState.Drunk))
            {
                //safe attack a treasure carrier
                if (enemy.State == PirateState.CarryingTreasure)
                {
                    if (Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius - 1) //in close range - enemy can't escape
                        && game.IsAttackPossible(this.myPirate, enemy) //can fire
                        && (!inRisk || isDouche)) // it's safe to shoot (a douche shouldn't care if it's safe or not to shoot - he has nothing to lose)
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new AttackCommand(this.myPirate, enemy), this.Id);

                        double value = ENEMY_HAS_TREASURE * enemy.CarriedTreasureValue;
                        if (myPirate.HasPowerup(AttackPowerup.NAME))
                            value *= ATTACK_POWERUP_MULTIPLYER;

                        ap.BurnInformation("Made in AttackEvent, case: shoot treasure. Value: {0:F3}", value);

                        chooser.AddActionsPack(ap, value);
                    }
                    else if ((Game.ManhattanDistance(enemy, enemy.InitialLocation) <= enemy.MaxSpeed || inRisk || !myPirate.CanAttack || myPirate.TurnsToAttackReload > enemy.DefenseDuration) && enemy.MaxSpeed < myPirate.MaxSpeed && !enemy.HasPowerup(SpeedPowerup.NAME))// ram
                    {
                        if (!isDouche || (!myPirate.CanDefend && inRisk && Game.ManhattanDistance(enemy, enemy.InitialLocation) > enemy.MaxSpeed))
                        {
                            // ram his predicted location
                            Location predictedLocation = game.PredictMovement(enemy);

                            // if there is no predicted location, try to ram his current location
                            if (predictedLocation == null)
                                predictedLocation = enemy.Location;

                            if (Game.ManhattanDistance(predictedLocation, myPirate) <= myPirate.MaxSpeed) // too far to ram
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, predictedLocation), this.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = ENEMY_HAS_TREASURE * enemy.CarriedTreasureValue - Game.ManhattanDistance(myPirate, enemy);
                                ap.BurnInformation("Made in AttackEvent, case: ram treasure. Value: {0:F3}", value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                    }
                    else if (enemy.DefenseDuration > 0 && myPirate.TurnsToAttackReload < enemy.DefenseDuration
                        && Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius - 1))
                    {
                        var sailOptions = game.GetCompleteSailOptions(this.myPirate, enemy.InitialLocation, 0, 0, this.myPirate.MaxSpeed, this.myPirate.DefenseDuration == 0, 0, enemy.MaxSpeed, 2, Terrain.EnemyLocation, Terrain.CurrentTreasureLocation);

                        foreach (var pair in sailOptions)
                        {
                            if (sailOptions[0].Value > pair.Value) // sail only if it's actually better than staying in place
                            {
                                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                                ap.AddEnemyPirate(enemy);

                                double value = GHOST_TREASURE_VAL * enemy.CarriedTreasureValue - Utils.Pow(pair.Value, 1.5) - Game.ManhattanDistance(pair.Key, enemy);
                                if (myPirate.HasPowerup(AttackPowerup.NAME))
                                    value *= ATTACK_POWERUP_MULTIPLYER;

                                ap.BurnInformation("Made in AttackEvent, against {0}, value: {1:F3}, case: ghost shielded treasure", enemy.Id, value);

                                chooser.AddActionsPack(ap, value);
                            }
                        }
                    }
                }
                else if (enemy.PowerupCount > 0)
                {
                    /*double multiplier = 2 * game.GetMyPiratesInAttackRange(enemy, PirateState.Free).Count;
                    multiplier += 4 * game.GetMyPiratesInAttackRange(enemy, PirateState.CarryingTreasure).Count;*/
                    double multiplier = 1;
                    if (game.GetMyPiratesInAttackRange(enemy, PirateState.Free).Count > 1 && game.GetMyPiratesInAttackRange(enemy, PirateState.CarryingTreasure).Count == 0)
                        multiplier = 8;

                    if (game.IsAttackPossible(myPirate, enemy) && enemy.TurnToSober == 0) //fire!
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new AttackCommand(this.myPirate, enemy), base.Id);

                        double value = ENEMY_HAS_POWERUP;
                        if (myPirate.HasPowerup(AttackPowerup.NAME))
                            value = value * ATTACK_POWERUP_MULTIPLYER;
                        value = value * multiplier;

                        ap.BurnInformation("Made in AttackEvent, case: shoot powerup. Value: {0:F3}", value);

                        chooser.AddActionsPack(ap, value);
                    }
                    else if (Game.ManhattanDistance(myPirate, enemy) <= myPirate.MaxSpeed && myPirate.PowerupCount == 0)// ram
                    {
                        if (!isDouche)
                        {
                            if (!enemy.CanAttack && !enemy.CanDefend)
                                continue;
                            // ram his location
                            Location enemyLocation = enemy.Location; // assume enemy will shoot or defend, otherwise ramming is practically impossible

                            ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, enemyLocation), base.Id);
                            double value = ENEMY_HAS_POWERUP - Game.ManhattanDistance(myPirate, enemy);
                            value = value * multiplier;

                            ap.AddEnemyPirate(enemy);
                            ap.BurnInformation("Made in AttackEvent, case: ram powerup. Value: {0:F3}", value);

                            chooser.AddActionsPack(ap, value);
                        }
                    }
                }
                //just attack a free pirate
                else if (enemy.State == PirateState.Free && Game.InAttackRange(this.myPirate, enemy, this.myPirate.AttackRadius))
                {
                    if (!myPirate.CanAttack)
                        continue;

                    if (enemy.DefenseDuration > 0)
                        continue;

                    if (game.IsAttackPossible(this.myPirate, enemy))
                    {
                        ActionsPack ap = ActionsPack.NewCommandPack(game, new AttackCommand(this.myPirate, enemy), base.Id);

                        /*double multiplier = 2 * game.GetMyPiratesInAttackRange(enemy, PirateState.Free).Count;
                        multiplier += 4 * game.GetMyPiratesInAttackRange(enemy, PirateState.CarryingTreasure).Count;*/
                        double multiplier = 1;
                        if (game.GetMyPiratesInAttackRange(enemy, PirateState.Free).Count > 1 && game.GetMyPiratesInAttackRange(enemy, PirateState.CarryingTreasure).Count == 0)
                            multiplier = 8;

                        double value = ENEMY_HAS_NO_TREASURE * multiplier;
                        if (myPirate.HasPowerup(AttackPowerup.NAME))
                            value = value * ATTACK_POWERUP_MULTIPLYER;

                        ap.BurnInformation("Made in AttackEvent, case: shoot regular. Value: {0:F3}", value);

                        chooser.AddActionsPack(ap, value);
                    }
                }
            }
        }
    }
}