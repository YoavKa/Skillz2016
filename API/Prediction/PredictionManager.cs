using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Utilities;

namespace MyBot.API.Prediction
{
    /// <summary>
    /// A prediction manager for the game
    /// </summary>
    class PredictionManager<T> : IUpdateable<Game>
    {
        //////////Attributes//////////

        /// <summary>
        /// The list of predictions
        /// </summary>
        private List<PiratePrediction<T>> predictions;



        //////////Methods//////////

        /// <summary>
        /// Creates a new PredictionManager
        /// </summary>
        public PredictionManager(Game game, IList<PiratePrediction<T>> predictions)
        {
            this.predictions = new List<PiratePrediction<T>>(predictions);

            this.Update(game);
        }

        /// <summary>
        /// Updates the PredictionManager
        /// </summary>
        public void Update(Game game)
        {
            foreach (PiratePrediction<T> prediction in this.predictions)
            {
                // recalculate the accuracy of the prediction
                prediction.Update();

                // predict value for enemy pirates
                foreach (Pirate pirate in game.GetAllEnemyPirates())
                    prediction.Predict(pirate);
                
                // predict value for my pirates
                // not strictly necessary ;)
                foreach (Pirate pirate in game.GetAllMyPirates())
                    prediction.Predict(pirate);
            }

            // sort the predictions by decreasing accuracy
            this.predictions.Sort();

            foreach (PiratePrediction<T> prediction in this.predictions)
            {
                LogImportance importance = prediction.Accuracy >= 0.85 ? LogImportance.Important : LogImportance.SomewhatImportant;
                if (prediction is TemporaryPrediction<T>)
                    importance = LogImportance.NotImportant;
                if (this.predictions[0] == prediction)
                    importance = LogImportance.Important;
                game.Log(LogType.Prediction, importance, prediction);
            }
        }

        /// <summary>
        /// Returns the predicted value of the pirate, or default(T) if no prediction was made
        /// </summary>
        public T Predict(Pirate p)
        {
            foreach (PiratePrediction<T> prediction in this.predictions)
            {
                if (prediction.Accuracy < 0.3)
                    break;

                if (!EqualityComparer<T>.Default.Equals(prediction.Predict(p), default(T)))
                    return prediction.Predict(p);
            }
            return default(T);
        }
    }
}
