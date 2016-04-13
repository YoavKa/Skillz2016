using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.Actions.Commands;
using MyBot.API;
using MyBot.States;

namespace MyBot.Events
{
    class DodgeEvent: Event
    {
        //////////Attributes//////////

        /// <summary>
        /// The pirate that is going to dodge!
        /// </summary>
        private Pirate myPirate;

        /// <summary>
        /// The values of dodging an enemy pirate.
        /// </summary>
        private const double PIRATE_WITH_TREASURE_DODGING = 180; // - (distanceMoved/(distanceMoved + 1)), range of values is about 180 to 179

        //////////Methods//////////

        /// <summary>
        /// Creates a new DodgeEvent
        /// </summary>
        public DodgeEvent(Pirate myPirate):base(1, 1)
        {
            this.myPirate = myPirate;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public override void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser)
        {
            if (!this.myPirate.CanMove || this.myPirate.State != PirateState.CarryingTreasure) // can't move = can't dodge / if I'm shielded no point in dodging
                return;

            TreasureDancingState treasureDancingState = statesManager.GetState<TreasureDancingState>();
            bool dodgedLastTurn = treasureDancingState.IsPirateDancing(this.myPirate);

            List<Pirate> enemies = game.GetEnemyPirates(PirateState.Free);

            if (enemies.Count == 0) // no enemies = no threat
                return;

            if (!dodgedLastTurn) //Pirate is carrying Treasure - can only move 1 step
            {
                foreach (var pair in GetTreasureDodgeOptions(this.myPirate, game))
                {
                    ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);

                    double distanceLeft = Game.ManhattanDistance(pair.Key, this.myPirate.InitialLocation);
                    double getCloserPrioritize = (distanceLeft == 0 ? myPirate.CarriedTreasureValue * 65536 : (distanceLeft + 1.0) / (distanceLeft)); //This is so it would prioritize moving in the direction that makes us closer.

                    double value = myPirate.CarriedTreasureValue * PIRATE_WITH_TREASURE_DODGING - pair.Value / (pair.Value + 1.0) + getCloserPrioritize; 

                    ap.BurnInformation("Made by DodgeEvent - case: dodge attack, Value: {0:F3}", value);

                    chooser.AddActionsPack(ap, value);
                }
            }
            else
            {
                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, this.myPirate), base.Id);
                double value = myPirate.CarriedTreasureValue * PIRATE_WITH_TREASURE_DODGING - 0.1;
                ap.BurnInformation("Made by DodgeEvent - case: dodge ram stage 2, Value: {0:F3}", value);
                chooser.AddActionsPack(ap, value);
            }
            foreach (var pair in GetRamDodgeOptions(this.myPirate, game))
            {
                ActionsPack ap = ActionsPack.NewCommandPack(game, new SailCommand(this.myPirate, pair.Key), base.Id);
                double value = myPirate.CarriedTreasureValue * PIRATE_WITH_TREASURE_DODGING - pair.Value / (pair.Value + 1.0);
                ap.BurnInformation("Made by DodgeEvent - case: dodge ram stage 1, Value: {0:F3}", value);
                chooser.AddActionsPack(ap, value);
            }
        }

        /// <summary>
        /// Returns all of the safe locations in distance maxMoves from start and the number of threating enemy pirates at the new location.
        /// </summary>
        private List<KeyValuePair<Location, double>> GetTreasureDodgeOptions(Pirate treasurePirate, Game game)
        {
            int moves = 1;
            List<KeyValuePair<Location, double>> safespots = new List<KeyValuePair<Location, double>>();

            List<Pirate> threats = game.GetEnemyDangerousPiratesInAttackRange(treasurePirate);
            threats.RemoveAll(p => game.PredictAttack(p) == PredictionResult.False);

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
                    List<Pirate> newThreats = game.GetEnemyDangerousPiratesInAttackRange(spot);
                    newThreats.RemoveAll(p => game.PredictAttack(p) == PredictionResult.False);
                    if (newThreats.Intersect(threats).Count() == 0)
                        safespots.Add(new KeyValuePair<Location, double>(spot, newThreats.Count));
                }
            }

            return safespots;
        }

        /// <summary>
        /// This function generates ram dodging movement options and the new distance to the closest threat.
        /// </summary>
        private List<KeyValuePair<Location, double>> GetRamDodgeOptions(Pirate treasurePirate, Game game)
        {
            int moves = 1;
            List<KeyValuePair<Location, double>> safespots = new List<KeyValuePair<Location, double>>();
            List<Pirate> threats = game.GetEnemyClosestPirates(treasurePirate, PirateState.Free);

            //Dodge enemy ramming
            threats.RemoveAll(p => (p.CanAttack && treasurePirate.DefenseDuration == 0 && Game.InEuclideanRange(p, treasurePirate, p.AttackRadius)) || !p.CanMove || !Game.InManhattanRange(p, treasurePirate, p.MaxSpeed));
            if (threats.Count == 0)
                return safespots;

            foreach (Location loc in game.GetNeighbors(treasurePirate, moves))
            {
                Pirate onNeighbor = game.GetPirateOn(loc);
                if (onNeighbor != null && ((onNeighbor.Owner == Owner.Me && onNeighbor.CanMove)|| threats.Contains(onNeighbor)))
                    safespots.Add(new KeyValuePair<Location, double>(loc, 0));
            }

            List<Location> allSpots = new List<Location>();
            allSpots.Add(new Location(treasurePirate.Location.Row + moves, treasurePirate.Location.Collumn));
            allSpots.Add(new Location(treasurePirate.Location.Row - moves, treasurePirate.Location.Collumn));
            allSpots.Add(new Location(treasurePirate.Location.Row, treasurePirate.Location.Collumn + moves));
            allSpots.Add(new Location(treasurePirate.Location.Row, treasurePirate.Location.Collumn - moves));

            List<Location> possibleSpots = new List<Location>(allSpots);
            possibleSpots.RemoveAll(loc => (game.GetPirateOn(loc) != null) || (!game.InMap(loc)) || (Game.OnStraightLine(loc, threats[0], treasurePirate))); 

            // Remove the naive options
            foreach (Location naiveOption in game.GetNaiveSailOptions(treasurePirate, treasurePirate.InitialLocation, moves))
                foreach (Location possibleMove in allSpots)
                    if (possibleMove.Equals(naiveOption))
                        possibleSpots.Remove(possibleMove);

            foreach (Location possibleMove in possibleSpots)
                safespots.Add(new KeyValuePair<Location, double>(possibleMove, Game.ManhattanDistance(threats[0], possibleMove)));

            return safespots;
        }
    }
}
