using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// A prediction for predicting shieldup when an enemy is close to a carrying pirate
    /// </summary>
    class ShieldTreasurePrediction : PiratePrediction<Location>
    {
        /// <summary>
        /// Creates a new ShieldTreasurePrediction
        /// </summary>
        public ShieldTreasurePrediction(Game game)
            : base(game, p => p.Location)
        { }

        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p)
        {
            if (p == null || p.State != PirateState.CarryingTreasure || !p.CanDefend)
                return null;

            bool inDanger;
            if (p.Owner == Owner.Me)
                inDanger = base.Game.GetEnemyDangerousPiratesInAttackRange(p).Count > 0;
            else
                inDanger = base.Game.GetMyDangerousPiratesInAttackRange(p).Count > 0;

            if (inDanger)
                return p.Location;
            else
                return null;
        }

        public override string ToString()
        {
            return "Shield prediction for carrying pirates: " + base.Accuracy;
        }
    }
}
