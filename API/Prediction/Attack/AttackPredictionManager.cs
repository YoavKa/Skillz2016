using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// An attack prediction manager for the game
    /// </summary>
    class AttackPredictionManager : PredictionManager<PredictionResult>
    {
        public AttackPredictionManager(Game game)
            : base(game, getPredictions(game))
        { }

        private static IList<PiratePrediction<PredictionResult>> getPredictions(Game game)
        {
            List<PiratePrediction<PredictionResult>> predictions = new List<PiratePrediction<PredictionResult>>();

            // add prediction here:
            predictions.Add(new AttackInRangePrediction(game, true, PirateState.Free));
            predictions.Add(new AttackInRangePrediction(game, PirateState.Free));
            predictions.Add(new AttackInRangePrediction(game, true, PirateState.CarryingTreasure));
            predictions.Add(new AttackInRangePrediction(game, PirateState.CarryingTreasure));

            predictions.Add(new AttackCantEscapeCanDefend(game, PirateState.CarryingTreasure));

            predictions.Add(new AttackCantDefendCanEscape(game, true, PirateState.Free));
            predictions.Add(new AttackCantDefendCanEscape(game, PirateState.Free));
            predictions.Add(new AttackCantDefendCanEscape(game, true, PirateState.CarryingTreasure));
            predictions.Add(new AttackCantDefendCanEscape(game, PirateState.CarryingTreasure));

            predictions.Add(new AttackCantDefendCantEscape(game, PirateState.CarryingTreasure));

            predictions.Add(new DoesntAttack(game, true, PirateState.Free));
            predictions.Add(new DoesntAttack(game, PirateState.Free));
            predictions.Add(new DoesntAttack(game, true, PirateState.CarryingTreasure));
            predictions.Add(new DoesntAttack(game, PirateState.CarryingTreasure));

            predictions.Add(new PreferDefenseOverAttackPrediction(game, true, PirateState.Free));
            predictions.Add(new PreferDefenseOverAttackPrediction(game, PirateState.Free));

            return predictions;
        }

        /// <summary>
        /// Returns whether the pirate attacked last turn
        /// </summary>
        public static PredictionResult AttackedLastTurn(Game game, Pirate p)
        {
            if (p == null)
                return PredictionResult.NotSure;
            else if (p.HasPowerup(AttackPowerup.NAME))
                return PredictionResult.NotSure;
            else if (p.TurnsToAttackReload == game.TurnsUntilAttackReload)
                return PredictionResult.True;
            else
                return PredictionResult.False;
        }
    }
}
