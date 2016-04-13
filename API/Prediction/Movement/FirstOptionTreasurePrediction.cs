using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies with treasure movement with the first movement option
    /// </summary>
    class FirstOptionTreasurePrediction : NaiveTreasurePrediction
    {
        /// <summary>
        /// Constructor for this naive movement prediction
        /// </summary>
        public FirstOptionTreasurePrediction(Game game, int maxSpeed)
            : base(game, maxSpeed)
        { }


        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p, List<Location> naiveOptions)
        {
            return naiveOptions.FirstOrDefault();
        }

        public override string ToString()
        {
            return "First option prediction for carrying pirates with at most " + base.MaxSpeed + " speed: " + base.Accuracy;
        }
    }
}
