using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// A prediction for predicting attack when the the target and can't defend
    /// </summary>
    class AttackCantDefendCanEscape : EnemyAttackPrediction
    {
        /// <summary>
        /// Creates a new AttackCantDefendCanEscape
        /// </summary>
        public AttackCantDefendCanEscape(Game game, bool checkPowerups, params PirateState[] checkStates)
            : base(game, checkPowerups, checkStates)
        { }
        /// <summary>
        /// Creates a new AttackCantDefendCanEscape
        /// </summary>
        public AttackCantDefendCanEscape(Game game, bool checkPowerups)
            : base(game, checkPowerups)
        { }
        /// <summary>
        /// Creates a new AttackCantDefendCanEscape
        /// </summary>
        public AttackCantDefendCanEscape(Game game, params PirateState[] checkStates)
            : base(game, checkStates)
        { }

        /// <summary>
        /// Predicts an attack of the attacking pirate on the endangered pirates next turn
        /// </summary>
        protected override PredictionResult predict(Pirate attackingPirate, List<Pirate> endangeredPirates)
        {
            if (endangeredPirates.Count > 0)
            {
                foreach (Pirate myPirate in endangeredPirates)
                {
                    if (!myPirate.CanDefend)
                        return PredictionResult.True;
                }
                return PredictionResult.False;
            }
            else
                return PredictionResult.NotSure;
        }

        public override string ToString()
        {
            return "Attack prediction for attacking whenever target can't defend: " + base.Accuracy;
        }
    }
}
