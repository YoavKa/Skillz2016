using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction
{
    /// <summary>
    /// A prediction for the game, that takes into account only the last turns
    /// </summary>
    abstract class TemporaryPrediction<T> : PiratePrediction<T>
    {
        //////////Attributes//////////

        /// <summary>
        /// The number of turns for which the prediction saves its success precentage
        /// </summary>
        private int predictionMemory;

        /// <summary>
        /// The total predictions queue
        /// </summary>
        private Queue<int> totalPredictionsQueue;
        /// <summary>
        /// The total correct predictions queue
        /// </summary>
        private Queue<int> correctPredictionsQueue;



        //////////Methods//////////

        /// <summary>
        /// Creates a new TemporaryPrediction
        /// </summary>
        public TemporaryPrediction(Game game, System.Func<Pirate, T> valueExtractor, int predictionMemory)
            : base(game, valueExtractor)
        {
            this.predictionMemory = predictionMemory;

            this.totalPredictionsQueue = new Queue<int>(this.predictionMemory);
            this.correctPredictionsQueue = new Queue<int>(this.predictionMemory);
        }

        /// <summary>
        /// Updates the prediction
        /// </summary>
        public override void Update()
        {
            // clears the queues if needed
            while (this.totalPredictionsQueue.Count >= this.predictionMemory)
                base.totalPredictions -= this.totalPredictionsQueue.Dequeue();
            while (this.correctPredictionsQueue.Count >= this.predictionMemory)
                base.correctPredictions -= this.correctPredictionsQueue.Dequeue();

            // calculate the total and correct predictions count
            int totalPredictions = base.totalPredictions;
            int correctPredictions = base.correctPredictions;
            base.Update();
            totalPredictions = base.totalPredictions - totalPredictions;
            correctPredictions = base.correctPredictions - correctPredictions;

            // push the current counts to the queues
            this.totalPredictionsQueue.Enqueue(totalPredictions);
            this.correctPredictionsQueue.Enqueue(correctPredictions);
        }
    }
}
