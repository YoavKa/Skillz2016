using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API.Mapping
{
    /// <summary>
    /// The sail manager of the game; helps with the movement of pirates on board
    /// </summary>
    class SailManager : IUpdateable<IPirateGame>
    {
        //////////Readonly's//////////

        /// <summary>
        /// Should the A* algorithm be enabled
        /// </summary>
        private bool aStar_enabled;



        //////////Attributes//////////

        /// <summary>
        /// The native IPirateGame object
        /// </summary>
        private IPirateGame game;

        /// <summary>
        /// The map of the game
        /// </summary>
        private Map map;


        /// <summary>
        /// The current count of aStar algorithms that were conducted
        /// </summary>
        private ulong aStar_counter;
        /// <summary>
        /// The gScore map of A*
        /// </summary>
        private int[,] aStar_gScore;
        /// <summary>
        /// Contains in each cell the last time the corresponding cell in gScore was touched;
        /// </summary>
        private ulong[,] aStar_gScoreLastTouched;
        /// <summary>
        /// The closed set of A*
        /// </summary>
        private HashSet<Location> aStar_closedSet;
        /// <summary>
        /// The open set of A*
        /// </summary>
        private HashSet<Location> aStar_openSet;


        /// <summary>
        /// The priority from which to enable the A*; the higher the better
        /// </summary>
        private int aStar_priority;



        //////////Methods//////////

        /// <summary>
        /// Creates a new sail manager, for the given map and game
        /// </summary>
        public SailManager(IPirateGame game, Map map)
        {
            this.game = game;
            this.map = map;

            this.aStar_counter = 0;
            this.aStar_gScore = new int[map.Rows, map.Collumns];
            this.aStar_gScoreLastTouched = new ulong[map.Rows, map.Collumns];
            this.aStar_closedSet = new HashSet<Location>();
            this.aStar_openSet = new HashSet<Location>();

            // disable the A* algorithm, if the map is too big
            this.aStar_enabled = !map.MyPirateManager.Armada && !map.IsBig;

            this.aStar_priority = 0;
        }

        /// <summary>
        /// Updates the sail manager
        /// </summary>
        public void Update(IPirateGame game)
        {
            this.game = game;
        }


        /// <summary>
        /// Returns the naive sail options that the native game object returns
        /// </summary>
        public List<Location> GetNaiveSailOptions(Pirate pirate, ILocateable destination, int moves)
        {
            if (pirate == null || destination == null || destination.GetLocation() == null)
                return new List<Location>();

            Pirates.Pirate nativePirate = pirate.Owner == Owner.Me ? this.game.GetMyPirate(pirate.Id) : this.game.GetEnemyPirate(pirate.Id);
            Pirates.Location nativeDestination = new Pirates.Location(destination.GetLocation().Row, destination.GetLocation().Collumn);
            List<Location> res = new List<Location>();
            foreach (Pirates.Location loc in this.game.GetSailOptions(nativePirate, nativeDestination, moves))
                res.Add(new Location(loc.Row, loc.Col));
            return res;
        }


        /// <summary>
        /// Returns wether the update of A* worked, when pathfinding from one of the points a_i to all points b_i,
        ///     where you can only walk on the allowed terrain specified
        /// </summary>
        /// <param name="from">all the possible starting location</param>
        /// <param name="to">all the end locations to check for</param>
        /// <param name="maxDistancePerTurn">the maximum amount of distance a pirate can travel in a turn</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>wether the constructions of ALL the paths were successful</returns>
        public bool Update_AStar(IList<Location> from, IList<Location> to, int maxDistancePerTurn, params Terrain[] disallowedTerrain)
        {
            // sanity check and a bit of boundaries
            if (from == null || to.Count <= 0 ||
                to == null || to.Count <= 0)
                return false;

            // we're starting, update the a* counter!
            this.aStar_counter++;

            // get goal locations
            List<Location> goals = new List<Location>();
            bool notFoundablePaths = false;
            foreach (Location possibleGoal in to)
            {
                if (possibleGoal == null)
                    continue;

                // boundaries and terrain check
                if (this.map.InMap(possibleGoal) && isAllowedByTerrain(possibleGoal, disallowedTerrain))
                    goals.Add(possibleGoal);
                // remember unreachable goals, for the final return code
                else if (!isAllowedByTerrain(possibleGoal, disallowedTerrain))
                    notFoundablePaths = true;
            }

            // clear the closed set
            this.aStar_closedSet.Clear();
            
            // set the open set to include the start point only
            this.aStar_openSet.Clear();

            // add all the starting points to open set, and set their gScore to 0
            List<Location> starts = new List<Location>();
            foreach (Location loc in from)
            {
                if (this.map.InMap(loc))
                {
                    this.aStar_openSet.Add(loc);
                    this.setGScore(loc, 0);
                    starts.Add(loc);
                }
                else
                    notFoundablePaths = true;
            }
            Location start = Map.ArithmeticAverage(starts);

            // if we shouldn't run the actuall A* algorithm, calculate the distances using simple Manhattan distance
            if (!this.aStar_enabled || Game.CurrentAStarPriority <= this.aStar_priority)
            {
                if (starts.Count > 0)
                    foreach (Location curGoal in goals)
                        foreach (Location curStart in starts)
                            this.setGScore(curGoal, (int) Utils.Min(this.getGScore(curGoal), Map.ManhattanDistance(curGoal, curStart)));
                return !notFoundablePaths;
            }

            // head for goals[0], and remove goals reached
            while (this.aStar_openSet.Count > 0 && goals.Count > 0) // while there are more elements to check, and more goals to be reached
            {
                // choose the element with the lowest fScore
                double fScore = this.aStar_gScore.Length; // a big enough value to be considered infinity
                Location currentNode = null;
                foreach (Location loc in this.aStar_openSet)
                {
                    // prefers points along the straight line from start to goal
                    int dx1 = loc.Row - goals[0].Row;
                    int dy1 = loc.Collumn - goals[0].Collumn;
                    int dx2 = start.Row - goals[0].Row;
                    int dy2 = start.Collumn - goals[0].Collumn;
                    int cross = Utils.Abs(dx1 * dy2 - dx2 * dy1);
                    double curFScore = this.getGScore(loc) +
                                       Map.ManhattanDistance(loc, goals[0]) +
                                       cross * 0.001;
                    if (curFScore < fScore)
                    {
                        fScore = curFScore;
                        currentNode = loc;
                    }
                }

                // if we reached the destination
                if (goals.Contains(currentNode))
                    goals.Remove(currentNode);

                // move currentNode from the open set to the closed set
                this.aStar_openSet.Remove(currentNode);
                this.aStar_closedSet.Add(currentNode);

                // iterate over each of the neighbors
                for (int distance = 1; distance <= maxDistancePerTurn; distance++)
                {
                    foreach (Location neighbor in this.map.GetNeighbors(currentNode, distance))
                    {
                        // if this neighbor has already been examined
                        if (this.aStar_closedSet.Contains(neighbor))
                            continue;

                        // check if the neighbor terrain is allowed
                        
                        // if not, continue; we cannot use this path!
                        if (!isAllowedByTerrain(neighbor, disallowedTerrain))
                        {
                            this.aStar_closedSet.Add(neighbor);
                            continue;
                        }

                        int newGScore = this.getGScore(currentNode) + distance;
                        // if we discovered a new node
                        if (!this.aStar_openSet.Contains(neighbor))
                            this.aStar_openSet.Add(neighbor);
                        // else, if this is not a better path
                        else if (newGScore >= this.getGScore(neighbor))
                            continue;

                        // we've got a new best path!
                        this.setGScore(neighbor, newGScore);
                    }
                }
            }

            // success = no goals remaining ;)
            return !notFoundablePaths && goals.Count == 0;
        }
        /// <summary>
        /// Returns wether the given location is allowed by the terrain parameters
        /// </summary>
        private bool isAllowedByTerrain(ILocateable l, params Terrain[] disallowedTerrain)
        {
            List<Terrain> allDisallowedTerrain = new List<Terrain>(disallowedTerrain);

            // add default terrain here
            // for example:
            // allDisallowedTerrain.Add(Terrain.EnemyLocation);

            foreach (Terrain t in allDisallowedTerrain)
                if (this.map.IsTerrain(l, t))
                    return false;
            return true;
        }
        /// <summary>
        /// Gets the gScore of the given location, while considering aStar_counter 
        /// </summary>
        private int getGScore(Location l)
        {
            if (this.aStar_gScoreLastTouched[l.Row, l.Collumn] != this.aStar_counter)
                return this.aStar_gScore.Length; // big enough to be considered infinity
            else
                return this.aStar_gScore[l.Row, l.Collumn];
        }
        /// <summary>
        /// Sets the gScore of the given location, and modifies aStar_gScoreLastTouched accordingly 
        /// </summary>
        private void setGScore(Location l, int score)
        {
            this.aStar_gScore[l.Row, l.Collumn] = score;
            this.aStar_gScoreLastTouched[l.Row, l.Collumn] = this.aStar_counter;
        }


        /// <summary>
        /// Returns a list of possible immediate destinations for the sail and the calculated distance to the destination,
        ///     that uses every number of moves satisfing minMoves &lt;= moves &lt;= maxMoves
        /// </summary>
        /// <param name="from">The start location</param>
        /// <param name="to">The destination</param>
        /// <param name="minToDistance">The minimum distance from the destination, which is OK to get to</param>
        /// <param name="maxToDistance">The maximum distance from the destination, which is OK to get to</param>
        /// <param name="maxDistancePerTurn">the maximum amount of distance a pirate can travel in a turn</param>
        /// <param name="dontStayInRange">should you avoid staying in the attack range of an enemy pirate</param>
        /// <param name="minMoves">The minimum number of moves to use</param>
        /// <param name="maxMoves">The maximum number of moves to use (or less, if the destination is in reach from the start location)</param>
        /// <param name="maxResultsForMovesNum">The maximum number of results to include, for each number of moves being checked</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>A list of possible immediate destinations for the sail and their calculated distance to the destination</returns>
        public List<KeyValuePair<Location, int>> GetCompleteSailOptions(ILocateable from, ILocateable to, int minToDistance, int maxToDistance, int maxDistancePerTurn, bool dontStayInRange, int minMoves, int maxMoves, int maxResultsForMovesNum, params Terrain[] disallowedTerrain)
        {
            // sanity check
            if (from == null || from.GetLocation() == null || !this.map.InMap(from) ||
                to == null || to.GetLocation() == null || !this.map.InMap(to))
                return new List<KeyValuePair<Location, int>>();

            // the attack map of the enemy
            AttackMap enemyAttackMap = this.map.EnemyAttackMap;
            // do the location stays in attack range
            System.Predicate<Location> staysInAttackRange =
                loc => enemyAttackMap.GetDangerousPiratesInAttackRange(loc).Intersect(
                       enemyAttackMap.GetDangerousPiratesInAttackRange(from)).Count() > 0;

            // use at most "distance" moves to reach destination
            int distance = Map.ManhattanDistance(from, to);
            if (distance < maxMoves)
                maxMoves = distance;

            // get all the possible results
            List<Location> possibleResults = new List<Location>();
            for (int moves = minMoves; moves <= maxMoves; moves++)
                possibleResults.AddRange(this.map.GetNeighbors(from, moves));
            // remove the results that stay in attack range if needed
            if (dontStayInRange)
                possibleResults.RemoveAll(staysInAttackRange);

            // get all the possible final destinations
            List<Location> finalDestinations = new List<Location>();
            for (int radius = minToDistance; radius <= maxToDistance; radius++)
                finalDestinations.AddRange(this.map.GetNeighbors(to, radius));

            // update A* algorithm
            this.Update_AStar(finalDestinations, possibleResults, maxDistancePerTurn, disallowedTerrain);

            // calculate the results
            List<KeyValuePair<Location, int>> results = new List<KeyValuePair<Location, int>>();
            // for each possible "moves" value
            for (int moves = minMoves; moves <= maxMoves; moves++)
            {
                // choose only best results
                possibleResults.Clear();
                possibleResults.AddRange(this.map.GetNeighbors(from, moves));
                // remove the results that stay in attack range if needed
                if (dontStayInRange)
                    possibleResults.RemoveAll(staysInAttackRange);
                possibleResults.Shuffle(); // shuffle the neighbors, so that two neighbors with the same gScore will be randomly ordered
                possibleResults = possibleResults.OrderBy(loc => this.getGScore(loc)).ToList(); // sort by gScore
                for (int i = 0; i < possibleResults.Count && i < maxResultsForMovesNum; i++)
                {
                    if (this.getGScore(possibleResults[i]) >= this.aStar_gScore.Length) // if the value is "infinity"
                        break;
                    // add the result, and its distance to the destination
                    results.Add(new KeyValuePair<Location, int>(possibleResults[i], this.getGScore(possibleResults[i])));
                }
            }
            return results;
        }
        /// <summary>
        /// Returns a list of possible immediate destinations for the sail and the calculated distance to the destination
        /// </summary>
        /// <param name="pirate">The start location</param>
        /// <param name="to">The destination</param>
        /// <param name="minToDistance">The minimum distance from the destination, which is OK to get to</param>
        /// <param name="maxToDistance">The maximum distance from the destination, which is OK to get to</param>
        /// <param name="dontStayInRange">should you avoid staying in the attack range of an enemy pirate</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>A list of possible immediate destinations for the sail and their calculated distance to the destination</returns>
        public List<KeyValuePair<Location, int>> GetCompleteSailOptions(Pirate pirate, ILocateable to, int minToDistance, int maxToDistance, bool dontStayInRange, params Terrain[] disallowedTerrain)
        {
            return this.GetCompleteSailOptions(pirate, to, minToDistance, maxToDistance, pirate.MaxSpeed, dontStayInRange, 0, pirate.MaxSpeed, 2, disallowedTerrain);
        }
        /// <summary>
        /// Returns a list of possible immediate destinations for the sail and the calculated distance to the destination
        /// </summary>
        /// <param name="pirate">The start location</param>
        /// <param name="to">The destination</param>
        /// <param name="dontStayInRange">should you avoid staying in the attack range of an enemy pirate</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>A list of possible immediate destinations for the sail and their calculated distance to the destination</returns>
        public List<KeyValuePair<Location, int>> GetCompleteSailOptions(Pirate pirate, ILocateable to, bool dontStayInRange, params Terrain[] disallowedTerrain)
        {
            return this.GetCompleteSailOptions(pirate, to, 0, 0, dontStayInRange, disallowedTerrain);
        }
        /// <summary>
        /// Returns a list of possible immediate destinations for the sail and the calculated distance to the destination
        /// </summary>
        /// <param name="pirate">The start location</param>
        /// <param name="to">The destination</param>
        /// <param name="minToDistance">The minimum distance from the destination, which is OK to get to</param>
        /// <param name="maxToDistance">The maximum distance from the destination, which is OK to get to</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>A list of possible immediate destinations for the sail and their calculated distance to the destination</returns>
        public List<KeyValuePair<Location, int>> GetCompleteSailOptions(Pirate pirate, ILocateable to, int minToDistance, int maxToDistance, params Terrain[] disallowedTerrain)
        {
            return this.GetCompleteSailOptions(pirate, to, minToDistance, maxToDistance, pirate.DefenseDuration == 0, disallowedTerrain);
        }
        /// <summary>
        /// Returns a list of possible immediate destinations for the sail and the calculated distance to the destination
        /// </summary>
        /// <param name="pirate">The start location</param>
        /// <param name="to">The destination</param>
        /// <param name="disallowedTerrain">the list of disallowed terrain to NOT walk on</param>
        /// <returns>A list of possible immediate destinations for the sail and their calculated distance to the destination</returns>
        public List<KeyValuePair<Location, int>> GetCompleteSailOptions(Pirate pirate, ILocateable to, params Terrain[] disallowedTerrain)
        {
            return this.GetCompleteSailOptions(pirate, to, 0, 0, disallowedTerrain);
        }


        /// <summary>
        /// Increases the priority from which the A* works
        /// </summary>
        public void IncreasePriority()
        {
            this.aStar_priority++;
        }
    }
}
