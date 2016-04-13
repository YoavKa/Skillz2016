using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies with treasure movement with the eculidian distance left
    /// </summary>
    class EuclideanTreasurePrediction : NaiveTreasurePrediction
    {
        /// <summary>
        /// Constructor for this euclidean movement prediction
        /// </summary>
        public EuclideanTreasurePrediction(Game game, int maxSpeed)
            : base(game, maxSpeed)
        { }


        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p, List<Location> naiveOptions)
        {
            return naiveOptions.OrderBy(loc => Game.ManhattanDistance(p, loc)).FirstOrDefault();
        }

        public override string ToString()
        {
            return "Euclidean prediction for carrying pirates with at most " + base.MaxSpeed + " speed: " + base.Accuracy;
        }
    }
}
