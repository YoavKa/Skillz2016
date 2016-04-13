using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// A basic attack prediction for enemies
    /// </summary>
    abstract class EnemyAttackPrediction : PiratePrediction<PredictionResult>
    {
        private bool checkSpeedPowerUp;
        private PirateState[] checkStates;

        /// <summary>
        /// Creates a new EnemyAttackPrediction
        /// </summary>
        public EnemyAttackPrediction(Game game,bool checkSpeedPowerUp, params PirateState[] checkStates)
            : base(game, p => AttackPredictionManager.AttackedLastTurn(game, p))
        {
            this.checkSpeedPowerUp = checkSpeedPowerUp;
            this.checkStates = checkStates;
        }
        /// <summary>
        /// Creates a new EnemyAttackPrediction
        /// </summary>
        public EnemyAttackPrediction(Game game,bool checkSpeedPowerUp)
            : this(game,checkSpeedPowerUp, new PirateState[0])
        { }
        /// <summary>
        /// Creates a new EnemyAttackPrediction
        /// </summary>
        public EnemyAttackPrediction(Game game, params PirateState[] checkStates)
            : this(game, false, checkStates)
        { }

        /// <summary>
        /// Predicts an attack of the attacking pirate next turn
        /// </summary>
        protected override PredictionResult predict(Pirate p)
        {
            if (p == null || !p.CanAttack || p.Owner != Owner.Enemy)
                return PredictionResult.NotSure;

            List<Pirate> endangeredPirates = new List<Pirate>();
            foreach (Pirate myPirate in base.Game.GetAllMyPirates())
            {
                if (myPirate.DefenseDuration == 0)
                {
                    if ((myPirate.HasPowerup(SpeedPowerup.NAME) == this.checkSpeedPowerUp) &&
                        (this.checkStates.Contains(myPirate.State) || this.checkStates.Count() == 0))
                    {
                        if (base.Game.IsAttackPossible(p, myPirate))
                            endangeredPirates.Add(myPirate);
                    }
                }
            }

            return this.predict(p, endangeredPirates);
        }

        /// <summary>
        /// Predicts an attack of the attacking pirate on the endangered pirates next turn
        /// </summary>
        abstract protected PredictionResult predict(Pirate attackingPirate, List<Pirate> endangeredPirates);
    }
}
