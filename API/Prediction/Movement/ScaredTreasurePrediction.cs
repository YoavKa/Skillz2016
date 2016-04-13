using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies with treasure movement with the distance to the closest enemy
    /// </summary>
    class ScaredTreasurePrediction : NaiveTreasurePrediction
    {
        /// <summary>
        /// Constructor for this euclidean movement prediction
        /// </summary>
        public ScaredTreasurePrediction(Game game, int maxSpeed)
            : base(game, maxSpeed)
        { }

        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p, List<Location> naiveOptions)
        {
            return naiveOptions.OrderBy(loc => Game.ManhattanDistance(loc, base.Game.GetMyClosestPirates(loc, PirateState.Free).FirstOrDefault())).LastOrDefault();
        }

        public override string ToString()
        {
            return "Scared prediction for carrying pirates with at most " + base.MaxSpeed + " speed: " + base.Accuracy;
        }
    }
}
