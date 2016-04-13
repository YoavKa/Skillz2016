using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.Actions.Commands;
using MyBot.API;
using MyBot.States;

namespace MyBot.Events
{
    class DefendEvent : Event
    {
        //////////Attribtues//////////

        /// <summary>
        /// My defending pirate
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of defending our pirate
        /// </summary>
        private const double PIRATE_HAS_TREASURE = 400;
        private const double PIRATE_DOESNT_HAVE_TREASURE = 300;
        private const double PIRATE_IS_DOUCHE = 1000;

        //////////Methods//////////

        /// <summary>
        /// Creates a new AttackEvent
        /// </summary>
        public DefendEvent(Pirate myPirate):base(1, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (!this.myPirate.CanDefend || this.myPirate.DefenseDuration > 0)
                return;

            List<Pirate> threats = new List<Pirate>();

            bool isDouche = game.GetPirateSpawnOn(myPirate) != null && game.GetPirateSpawnOn(myPirate).Owner == Owner.Enemy; // If my pirate is a douche, he shouldn't ram the enemy since spawn-blocking is better.

            if (this.myPirate.State != PirateState.CarryingTreasure)
            {
                foreach (Pirate threat in game.GetEnemyDangerousPiratesInAttackRange(this.myPirate))
                {
                    if (game.PredictAttack(threat) == PredictionResult.False)
                        continue;

                    threats.Add(threat);
                    foreach (Pirate treasurePirate in game.GetMyPirates(PirateState.CarryingTreasure)) // Assume a pirate prefers to shoot a treasure pirate over a non-treasure pirate if he can
                    {
                        if (Game.InAttackRange(treasurePirate, threat, threat.AttackRadius))
                        {
                            threats.Remove(threat);
                            break;
                        }
                    }
                }
            }
            else
            {
                threats = game.GetEnemyDangerousPiratesInAttackRange(this.myPirate);
                threats.RemoveAll(p => game.PredictAttack(p) == PredictionResult.False);
                foreach (Pirate ally in game.GetMyPirates(PirateState.Free))
                    if (Game.InAttackRange(ally, myPirate, ally.AttackRadius) && ally.CanAttack)
                        return;
             }

            if (threats.Count == 0) // I'm safe, yay me!
                return;

            ActionsPack ap = ActionsPack.NewCommandPack(game, new DefendCommand(this.myPirate), base.Id);

            double value = PIRATE_DOESNT_HAVE_TREASURE;
            if (this.myPirate.State == PirateState.CarryingTreasure)
                value = PIRATE_HAS_TREASURE * this.myPirate.CarriedTreasureValue;
            else if (isDouche && this.myPirate.State != PirateState.CarryingTreasure)
                value = PIRATE_IS_DOUCHE;

            ap.BurnInformation("Made in DefendEvent, Value: {0:F3}",value);

            chooser.AddActionsPack(ap, value);
        }
    }
}