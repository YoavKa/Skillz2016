using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// This class tries to predict enemies with treasure movement using the naive enemy movement
    /// </summary>
    abstract class NaiveTreasurePrediction : PiratePrediction<Location>
    {
        /// <summary>
        /// The maximum speed for which to check the pirates
        /// </summary>
        private int maxSpeed;

        /// <summary>
        /// Constructor for this naive prediction
        /// </summary>
        public NaiveTreasurePrediction(Game game, int maxSpeed)
            : base(game, p => p.Location)
        {
            this.maxSpeed = maxSpeed;
        }

        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        protected override Location predict(Pirate p)
        {
            // only handle treasure pirates that have small enough maximum speed
            if (p.State != PirateState.CarryingTreasure || p.Owner != Owner.Enemy || p.MaxSpeed > this.maxSpeed)
                return null;
            
            List<Location> sailOptions = base.Game.GetNaiveSailOptions(p, p.InitialLocation, p.MaxSpeed);

            // remove location that have a pirate on them
            sailOptions.RemoveAll(loc => base.Game.GetPirateOn(loc) != null /*&& ((!base.Game.GetPirateOn(loc).CanMove && base.Game.GetPirateOn(loc).Owner == Owner.Enemy) 
            || base.Game.GetPirateOn(loc).Owner == Owner.Me)*/ && base.Game.GetPirateOn(loc) != p);
            if (sailOptions.Count == 0)
                return null;

            return predict(p, sailOptions);
        }

        /// <summary>
        /// Returns the predicted location for the next turn of the pirate, or null if no prediction was made
        /// </summary>
        abstract protected Location predict(Pirate p, List<Location> naiveOptions);

        /// <summary>
        /// The maximum speed for which to check the pirates
        /// </summary>
        protected int MaxSpeed
        {
            get { return this.maxSpeed; }
        }
    }
}
