using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies with treasure movement with the last movement option
    /// </summary>
    class LastOptionTreasurePrediction : NaiveTreasurePrediction
    {
        /// <summary>
        /// Constructor for this naive movement prediction, it always predics the last option
        /// </summary>
        public LastOptionTreasurePrediction(Game game, int maxSpeed)
            : base(game, maxSpeed)
        { }


        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p, List<Location> naiveOptions)
        {
            return naiveOptions.LastOrDefault();
        }

        public override string ToString()
        {
            return "Last option prediction for carrying pirates with at most " + base.MaxSpeed + " speed: " + base.Accuracy;
        }
    }
}
