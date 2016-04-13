using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies that do not move
    /// </summary>
    class StrokePrediction : TemporaryPrediction<Location>
    {
        //////////Consts//////////

        /// <summary>
        /// The number of turns for which the prediction saves its success precentage
        /// </summary>
        public const int STROKE_MEMORY = 3;



        //////////Attributes//////////

        /// <summary>
        /// The pirates whose stroke is detected
        /// </summary>
        private Pirate pirate;



        //////////Methods//////////

        /// <summary>
        /// Creates a new StrokePrediction
        /// </summary>
        public StrokePrediction(Game game, Pirate pirate)
            : base(game, p => p.Location, STROKE_MEMORY)
        {
            this.pirate = pirate;
        }

        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p)
        {
            if (p != this.pirate || p.State == PirateState.Lost || p.State == PirateState.Drunk)
                return null;

            return p.Location;
        }

        public override string ToString()
        {
            return "Stroke prediction for pirate " + this.pirate.Id + ": " + this.Accuracy;
        }
    }
}
