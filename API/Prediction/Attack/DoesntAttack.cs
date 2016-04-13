using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// A prediction for predicting attack when the enemy doesn't attack
    /// </summary>
    class DoesntAttack : EnemyAttackPrediction
    {
        /// <summary>
        /// Creates a new DoesntAttack
        /// </summary>
        public DoesntAttack(Game game, bool checkSpeedPowerUp, params PirateState[] checkStates)
            : base(game, checkSpeedPowerUp, checkStates)
        { }
        /// <summary>
        /// Creates a new DoesntAttack
        /// </summary>
        public DoesntAttack(Game game, bool checkSpeedPowerUp)
            : base(game, checkSpeedPowerUp)
        { }
        /// <summary>
        /// Creates a new DoesntAttack
        /// </summary>
        public DoesntAttack(Game game, params PirateState[] checkStates)
            : base(game, checkStates)
        { }

        /// <summary>
        /// Predicts an attack of the attacking pirate on the endangered pirates next turn
        /// </summary>
        protected override PredictionResult predict(Pirate attackingPirate, List<Pirate> endangeredPirates)
        {
            if (endangeredPirates.Count > 0)
                return PredictionResult.False;
            else
                return PredictionResult.NotSure;
        }

        public override string ToString()
        {
            return "Attack prediction for attacking never: " + base.Accuracy;
        }
    }
}
