using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Movement
{
    /// <summary>
    /// A movement prediction manager for the game
    /// </summary>
    class MovementPredictionManager : PredictionManager<Location>
    {
        public MovementPredictionManager(Game game)
            : base(game, getPredictions(game))
        { }

        private static IList<PiratePrediction<Location>> getPredictions(Game game)
        {
            List<PiratePrediction<Location>> predictions = new List<PiratePrediction<Location>>();

            // add predictions here:
            predictions.Add(new EuclideanTreasurePrediction(game, 1));
            predictions.Add(new EuclideanTreasurePrediction(game, game.ActionsPerTurn));

            predictions.Add(new FirstOptionTreasurePrediction(game, 1));
            predictions.Add(new FirstOptionTreasurePrediction(game, game.ActionsPerTurn));

            predictions.Add(new LastOptionTreasurePrediction(game, 1));
            predictions.Add(new LastOptionTreasurePrediction(game, game.ActionsPerTurn));

            predictions.Add(new ScaredTreasurePrediction(game, 1));
            predictions.Add(new ScaredTreasurePrediction(game, game.ActionsPerTurn));

            predictions.Add(new ShieldTreasurePrediction(game));

            foreach (Pirate p in game.GetAllEnemyPirates())
                predictions.Add(new StrokePrediction(game, p));

            return predictions;
        }
    }
}
