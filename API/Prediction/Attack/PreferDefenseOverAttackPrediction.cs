using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// A prediction for predicting that attack won't happen if the enemy thinks he need to defend
    /// </summary>
    class PreferDefenseOverAttackPrediction : EnemyAttackPrediction
    {
        /// <summary>
        /// Creates a new PreferDefenseOverAttackPrediction
        /// </summary>
        public PreferDefenseOverAttackPrediction(Game game, bool checkSpeedPowerUp, params PirateState[] checkStates)
            : base(game, checkSpeedPowerUp, checkStates)
        { }
        /// <summary>
        /// Creates a new PreferDefenseOverAttackPrediction
        /// </summary>
        public PreferDefenseOverAttackPrediction(Game game, bool checkSpeedPowerUp)
            : base(game, checkSpeedPowerUp)
        { }
        /// <summary>
        /// Creates a new PreferDefenseOverAttackPrediction
        /// </summary>
        public PreferDefenseOverAttackPrediction(Game game, params PirateState[] checkStates)
            : base(game, checkStates)
        { }

        /// <summary>
        /// Predicts an attack of the attacking pirate on the endangered pirates next turn
        /// </summary>
        protected override PredictionResult predict(Pirate attackingPirate, List<Pirate> endangeredPirates)
        {
            if (endangeredPirates.Count > 0 && attackingPirate.CanDefend)
            {
                foreach (Pirate myPirate in endangeredPirates)
                {
                    if (base.Game.IsAttackPossible(myPirate, attackingPirate))
                    {
                        return PredictionResult.False; //it will defend, so it will not attack
                    }
                }
            }
            
            return PredictionResult.NotSure;
        }

        public override string ToString()
        {
            return "Attack prediction for not attacking whenever need to be defended: " + base.Accuracy;
        }
    }
}
