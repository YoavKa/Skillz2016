using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Prediction.Attack
{
    /// <summary>
    /// A prediction for predicting attack when the the target has treasure and can't escape or defend
    /// </summary>
    class AttackCantDefendCantEscape : EnemyAttackPrediction
    {
        /// <summary>
        /// Creates a new AttackCantDefendCantEscape
        /// </summary>
        public AttackCantDefendCantEscape(Game game, bool checkSpeedPowerUp, params PirateState[] checkStates)
            : base(game, checkSpeedPowerUp, checkStates)
        { }
        /// <summary>
        /// Creates a new AttackCantDefendCantEscape
        /// </summary>
        public AttackCantDefendCantEscape(Game game, bool checkSpeedPowerUp)
            : base(game, checkSpeedPowerUp)
        { }
        /// <summary>
        /// Creates a new AttackCantDefendCantEscape
        /// </summary>
        public AttackCantDefendCantEscape(Game game, params PirateState[] checkStates)
            : base(game, checkStates)
        { }

        /// <summary>
        /// Predicts an attack of the attacking pirate on the endangered pirates next turn
        /// </summary>
        protected override PredictionResult predict(Pirate attackingPirate, List<Pirate> endangeredPirates)
        {
            if (endangeredPirates.Count > 0)
            {
                foreach (Pirate myPirate in endangeredPirates)
                {
                    if (GetTreasureDodgeOptions(myPirate, attackingPirate, base.Game).Count == 0 && !myPirate.CanDefend)
                        return PredictionResult.True;
                }
                return PredictionResult.False;
            }
            else
                return PredictionResult.NotSure;
        }

        /// <summary>
        /// Returns all of the safe locations in distance maxMoves from start and the number of threating enemy pirates at the new location.
        /// </summary>
        private List<KeyValuePair<Location, double>> GetTreasureDodgeOptions(Pirate treasurePirate, Pirate attackingPirate, Game game)
        {
            int moves = 1;
            List<KeyValuePair<Location, double>> safespots = new List<KeyValuePair<Location, double>>();

            List<Pirate> threats = new List<Pirate>();
            threats.Add(attackingPirate);

            if (treasurePirate.DefenseDuration > 0 || threats.Count == 0)
                return safespots;


            List<Location> allSpots = new List<Location>();
            allSpots.Add(new Location(treasurePirate.Location.Row + moves, treasurePirate.Location.Collumn));
            allSpots.Add(new Location(treasurePirate.Location.Row - moves, treasurePirate.Location.Collumn));
            allSpots.Add(new Location(treasurePirate.Location.Row, treasurePirate.Location.Collumn + moves));
            allSpots.Add(new Location(treasurePirate.Location.Row, treasurePirate.Location.Collumn - moves));

            foreach (Location spot in allSpots)
            {
                if (game.InMap(spot))
                {
                    if (!Game.InAttackRange(spot, attackingPirate, attackingPirate.AttackRadius))
                        safespots.Add(new KeyValuePair<Location, double>(spot, 0));
                }
            }

            return safespots;
        }

        public override string ToString()
        {
            return "Attack prediction for attacking whenever hit is garunteed (on treasured only): " + base.Accuracy;
        }
    }

}
