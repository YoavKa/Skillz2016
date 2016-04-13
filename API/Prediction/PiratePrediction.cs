using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction
{
    /// <summary>
    /// A pirate prediction for the game
    /// </summary>
    /// <typeparam name="T">The value being predicted</typeparam>
    abstract class PiratePrediction<T> : System.IComparable<PiratePrediction<T>>
    {
        //////////Attributes//////////

        /// <summary>
        /// The function to extract the current value being predicted from the pirate
        /// </summary>
        private System.Func<Pirate, T> valueExtractor;

        /// <summary>
        /// The predicted values for each pirate
        /// </summary>
        private Dictionary<Pirate, T> predictedValues;

        /// <summary>
        /// The total amount of predictions made
        /// </summary>
        protected int totalPredictions;
        /// <summary>
        /// The total amount of correct predictions made
        /// </summary>
        protected int correctPredictions;


        /// <summary>
        /// The current game object
        /// </summary>
        private Game game;



        //////////Methods//////////

        /// <summary>
        /// Creates a new PiratePrediction
        /// </summary>
        public PiratePrediction(Game game, System.Func<Pirate, T> valueExtractor)
        {
            this.game = game;

            this.valueExtractor = valueExtractor;

            this.predictedValues = new Dictionary<Pirate, T>();

            this.totalPredictions = 0;
            this.correctPredictions = 0;
        }

        /// <summary>
        /// Updates the prediction
        /// </summary>
        public virtual void Update()
        {
            foreach (var pair in this.predictedValues)
            {
                if (pair.Key.State != PirateState.Lost && !EqualityComparer<T>.Default.Equals(pair.Value, default(T)))
                {
                    this.totalPredictions++;
                    if (this.valueExtractor(pair.Key).Equals(pair.Value))
                        this.correctPredictions++;
                }
            }
            this.predictedValues.Clear();
        }

        /// <summary>
        /// Returns the predicted value for pirate p, or default(T) if no prediction was made
        /// </summary>
        public T Predict(Pirate p)
        {
            if (this.predictedValues.ContainsKey(p))
                return this.predictedValues[p];
            T predictedValue = this.predict(p);
            if (EqualityComparer<T>.Default.Equals(predictedValue, default(T)))
                return default(T);
            this.predictedValues.Add(p, predictedValue);
            return predictedValue;
        }

        /// <summary>
        /// Returns the predicted value for the next turn of the pirate, or default(T) if no prediction was made
        /// </summary>
        protected abstract T predict(Pirate p);

        /// <summary>
        /// An inverse compare for two movement prediction
        /// </summary>
        public int CompareTo(PiratePrediction<T> other)
        {
            return other.Accuracy.CompareTo(this.Accuracy);
        }



        //////////Properties//////////

        /// <summary>
        /// Returns the accuracy of the prediction
        /// </summary>
        public double Accuracy
        {
            get
            {
                if (totalPredictions == 0)
                    return 0;
                else
                    return ((double)this.correctPredictions) / totalPredictions;
            }
        }

        /// <summary>
        /// The current game object
        /// </summary>
        protected Game Game
        {
            get { return this.game; }
        }
    }
}