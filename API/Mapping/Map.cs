using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API.Mapping
{
    /// <summary>
    /// The map of the game
    /// </summary>
    class Map : IUpdateable<IPirateGame>
    {
        //////////Attributes//////////

        /// <summary>
        /// All the treasures, including carried and taken treasures
        /// </summary>
        private Treasure[] allTreasures;

        /// <summary>
        /// An array of all the treasures lists, by states
        /// </summary>
        private List<Treasure>[] treasuresByStates;
        /// <summary>
        /// A list of the spawn clusters of the treasures
        /// </summary>
        private List<Cluster<Treasure>> treasuresSpawnClusers;


        /// <summary>
        /// A dictionary of the pirates on the map; the key of each pirate is its location on the map
        /// </summary>
        private Dictionary<Location, Pirate> piratesOnMap;
        /// <summary>
        /// A dictionary of the spawn points of the pirates on map; the key of each pirate is its location on the map
        /// </summary>
        private Dictionary<Location, Pirate> piratesSpawnOnMap;
        /// <summary>
        /// A dictionary of the treasures on the map; the key of each treasure is its location on the map
        /// </summary>
        private ListsDictionary<Location, Treasure> treasuresOnMap;
        /// <summary>
        /// A dictionary of the spawn points of the treasures on the map; the key of each treasure is its initial location on the map
        /// </summary>
        private Dictionary<Location, Treasure> treasuresSpawnOnMap;

        /// <summary>
        /// The powerups on the map
        /// </summary>
        private List<Powerup> powerups;

        /// <summary>
        /// The value currently being carried by us
        /// </summary>
        private int myCarriedValue;
        /// <summary>
        /// The value currently being carried by the enemy
        /// </summary>
        private int enemyCarriedValue;


        /// <summary>
        /// A neighbors matrix of the map; neighbors[r, c, i] will be a list of locations in the map,
        ///  whose manhattan distance from (r, c) is exactly i.
        /// NOTE: the maximum i is Game.MaxActionsPerTurn
        /// </summary>
        private List<Location>[, ,] neighbors;


        /// <summary>
        /// The proxy which contains all the objects which should be updated at update
        /// </summary>
        private UpdateProxy<IPirateGame> updateProxy;



        //////////Readonly's//////////

        /// <summary>
        /// The number of rows of the map
        /// </summary>
        public readonly int Rows;
        /// <summary>
        /// The number of collumns of the map
        /// </summary>
        public readonly int Collumns;

        /// <summary>
        /// The maximum euclidian distance between two pirates, where they can still attack each other, when they don't use powerups!
        /// </summary>
        public readonly double AttackRadius;

        /// <summary>
        /// The pirate manager for my pirates
        /// </summary>
        public readonly PirateManager MyPirateManager;
        /// <summary>
        /// The attack map for my pirates
        /// </summary>
        public readonly AttackMap MyAttackMap;

        /// <summary>
        /// The pirate manager for the enemy's pirates
        /// </summary>
        public readonly PirateManager EnemyPirateManager;
        /// <summary>
        /// The attack map for the enemy
        /// </summary>
        public readonly AttackMap EnemyAttackMap;

        /// <summary>
        /// The sail manager of the map
        /// </summary>
        public readonly SailManager SailManager;

        /// <summary>
        /// Is the map a big one
        /// </summary>
        public readonly bool IsBig;
        


        //////////Methods//////////

        /// <summary>
        /// Creates a new map of the game
        /// </summary>
        public Map(IPirateGame game)
        {
            // updates map constants
            this.Rows = game.GetRows();
            this.Collumns = game.GetCols();
            this.AttackRadius = Utils.Sqrt(game.GetAttackRadius());

            // set the computing limits
            this.IsBig = this.Rows > 40 || this.Collumns > 40;

            // create the pirate managers
            this.MyPirateManager = new PirateManager(game, Owner.Me);
            this.MyAttackMap = new AttackMap(this, this.MyPirateManager.GetAllPirates());
            this.EnemyPirateManager = new PirateManager(game, Owner.Enemy);
            this.EnemyAttackMap = new AttackMap(this, this.EnemyPirateManager.GetAllPirates());

            // create the sail manager
            this.SailManager = new SailManager(game, this);

            // create the update proxy
            this.updateProxy = new UpdateProxy<IPirateGame>();
            this.updateProxy.Add(this.MyPirateManager);
            this.updateProxy.Add(this.EnemyPirateManager);
            this.updateProxy.Add(this.SailManager);

            
            // initialize the neighbors matrix
            this.neighbors = new List<Location>[this.Rows, this.Collumns, (2 * game.GetActionsPerTurn()) + 1];
            // calculate the neighbors for each location in map
            for (int row = 0; row < this.Rows; row++)
            {
                for (int collumn = 0; collumn < this.Collumns; collumn++)
                {
                    // assign neighbors of distance 0 ;)
                    this.neighbors[row, collumn, 0] = new List<Location>();
                    this.neighbors[row, collumn, 0].Add(new Location(row, collumn));

                    // for each distance from 1 to max
                    for (int distance = 1; distance < this.neighbors.GetLength(2); distance++)
                    {
                        this.neighbors[row, collumn, distance] = new List<Location>();
                        // add all the neighbors in a windmill-like shape
                        for (int i = 0; i < distance; i++)
                        {
                            // all four quarters, were the row delta is i
                            this.neighbors[row, collumn, distance].Add(new Location(row + i, collumn + (distance - i)));
                            this.neighbors[row, collumn, distance].Add(new Location(row - i, collumn - (distance - i)));
                            this.neighbors[row, collumn, distance].Add(new Location(row - (distance - i), collumn + i));
                            this.neighbors[row, collumn, distance].Add(new Location(row + (distance - i), collumn - i));
                        }
                        // remove neighbors which are not in the map
                        this.neighbors[row, collumn, distance].RemoveAll(loc => !this.InMap(loc));
                    }
                }
            }
            

            // the list of availible treasures
            List<Pirates.Treasure> allTreasures = game.Treasures();

            // initialize the lists containing the treasures by states
            this.treasuresByStates = new List<Treasure>[(int) TreasureState.COUNT];
            for (int i = 0; i < this.treasuresByStates.Length; i++)
                this.treasuresByStates[i] = new List<Treasure>();

            // initialize the allTreasures array and the treasuresSpawnOnMap dictionary
            this.allTreasures = new Treasure[allTreasures.Count];
            this.treasuresSpawnOnMap = new Dictionary<Location, Treasure>();

            // initialize all the treasures in the allTreasures array, and add them to freeToTakeTreasures and to treasuresSpawnOnMap
            foreach (Pirates.Treasure t in allTreasures)
            {
                Treasure newTreasure = new Treasure(t);
                newTreasure.NativeObject = t;
                this.allTreasures[t.Id] = newTreasure;
                this.treasuresByStates[(int) TreasureState.FreeToTake].Add(newTreasure);
                this.treasuresSpawnOnMap.Add(newTreasure.InitialLocation, newTreasure);
            }

            // initialize the piratesSpawnOnMap dictionary
            this.piratesSpawnOnMap = new Dictionary<Location, Pirate>();
            foreach (Pirate p in this.MyPirateManager.GetAllPirates())
                this.piratesSpawnOnMap.Add(p.InitialLocation, p);
            foreach (Pirate p in this.EnemyPirateManager.GetAllPirates())
                this.piratesSpawnOnMap.Add(p.InitialLocation, p);

            // initialize the map dictionaries, and update them
            this.piratesOnMap = new Dictionary<Location, Pirate>();
            this.updatePiratesMapDictionary();
            this.treasuresOnMap = new ListsDictionary<Location, Treasure>();
            this.updateTreasuresMapDictionary();

            // initialize the powerups list
            this.powerups = new List<Powerup>();
            this.repopulatePowerups(game.Powerups(), game.GetTurn());

            // initialize treasuresSpawnClusters
            this.treasuresSpawnClusers = Cluster.Clusterify<Treasure>(this.allTreasures, this.AttackRadius);

            // initialize the value being carried
            this.myCarriedValue = 0;
            this.enemyCarriedValue = 0;
        }
        
        /// <summary>
        /// Updates the map
        /// </summary>
        public void Update(IPirateGame game)
        {
            // update pirate managers
            this.updateProxy.Update(game);

            // update attack maps
            this.MyAttackMap.Update(this.MyPirateManager.GetAllPirates());
            this.EnemyAttackMap.Update(this.EnemyPirateManager.GetAllPirates());

            // update pirate map dictionary
            this.updatePiratesMapDictionary();

            // clear the lists of treasures by states
            for (int i = 0; i < this.treasuresByStates.Length; i++)
                this.treasuresByStates[i].Clear();

            // fill the freeToTakeTreasures list, based on game.Treasures()
            foreach (Pirates.Treasure t in game.Treasures())
            {
                this.treasuresByStates[(int)TreasureState.FreeToTake].Add(this.allTreasures[t.Id]);
                this.allTreasures[t.Id].NativeObject = t;
            }

            // update all the treasures, and add them simultaneously to carriedTreasures and takenTreasures
            this.myCarriedValue = 0;
            this.enemyCarriedValue = 0;
            foreach (Treasure t in this.allTreasures)
            {
                t.Update(this);
                if (t.State == TreasureState.FreeToTake)
                    continue;
                // the treasure is not free to take, thus has no native object
                t.NativeObject = null;

                this.treasuresByStates[(int)t.State].Add(t);

                if (t.State == TreasureState.BeingCarried)
                {
                    // make sure the carrying pirate knows about this
                    t.CarryingPirate.CarriedTreasure = t;
                    // add the value being carried to the corresponding variable
                    if (t.CarryingPirate.Owner == Owner.Me)
                        this.myCarriedValue += t.Value;
                    else if (t.CarryingPirate.Owner == Owner.Enemy)
                        this.enemyCarriedValue += t.Value;
                }
            }

            // update treasure map dictionary
            this.updateTreasuresMapDictionary();

            // update powerup map
            this.repopulatePowerups(game.Powerups(), game.GetTurn());
        }

        /// <summary>
        /// Updates the piratesOnMap dictionary, based on the pirate managers
        /// </summary>
        private void updatePiratesMapDictionary()
        {
            this.piratesOnMap.Clear();

            // adds all my pirates that are on the map
            foreach (Pirate p in this.MyPirateManager.GetAllPirates())
                if (p.Location != null && p.State != PirateState.Lost)
                    this.piratesOnMap.Add(p.Location, p);

            // adds all enemy pirates that are on the map
            foreach (Pirate p in this.EnemyPirateManager.GetAllPirates())
                if (p.Location != null && p.State != PirateState.Lost)
                    this.piratesOnMap.Add(p.Location, p);
        }
        /// <summary>
        /// Updates the treasuresOnMap dictionary, based on allTreasures array
        /// </summary>
        private void updateTreasuresMapDictionary()
        {
            this.treasuresOnMap.Clear();

            // adds all the treasures that are on the map
            foreach (Treasure t in this.GetTreasures(TreasureState.FreeToTake, TreasureState.BeingCarried))
                this.treasuresOnMap.Add(t.Location, t);
        }
        /// <summary>
        /// Repopulates the powerups list, based on the parameters given
        /// </summary>
        private void repopulatePowerups(List<Pirates.Powerup> powerups, int curTurn)
        {
            this.powerups.Clear();
            if (powerups != null)
                foreach (Pirates.Powerup powerup in powerups)
                    this.powerups.Add(Powerup.GetPowerup(powerup, curTurn));
            this.powerups.RemoveAll(pu => pu == null);
        }

        /// <summary>
        /// Returns a new list, containing all the treasures
        /// </summary>
        public List<Treasure> GetAllTreasures()
        {
            return new List<Treasure>(this.allTreasures);
        }
        /// <summary>
        /// Returns the count of all the treasures
        /// </summary>
        /// <returns></returns>
        public int GetAllTreasuresCount()
        {
            return this.allTreasures.Length;
        }
        /// <summary>
        /// Returns a new list, containing all the treasures of the given states.
        /// </summary>
        public List<Treasure> GetTreasures(params TreasureState[] states)
        {
            List<Treasure> results = new List<Treasure>();
            foreach (TreasureState state in states.Distinct())
                foreach (Treasure t in this.treasuresByStates[(int)state])
                    results.Add(t);

            return results;
        }
        /// <summary>
        /// Returns the count of all the treasures of the given states
        /// </summary>
        public int GetTreasuresCount(params TreasureState[] states)
        {
            int count = 0;
            foreach (TreasureState state in states.Distinct())
                count += this.treasuresByStates[(int)state].Count;
            return count;
        }
        /// <summary>
        /// Returns the treasure with the given id, or null if one does not exist
        /// </summary>
        public Treasure GetTreasure(int id)
        {
            if (!Utils.InBounds(this.allTreasures, id))
                return null; // there is no treasure with this id
            return this.allTreasures[id];
        }
        /// <summary>
        /// Returns a list of all of the spawn clusters of the treasures
        /// </summary>
        public List<Cluster<Treasure>> GetTreasuresSpawnClusters()
        {
            return this.treasuresSpawnClusers.Select(c => new Cluster<Treasure>(c)).ToList();
        }
        /// <summary>
        /// Returns the pirate which is on the location given, or null if one does not exist
        /// </summary>
        public Pirate GetPirateOn(ILocateable loc)
        {
            if (loc == null)
                return null;
            if (this.piratesOnMap.ContainsKey(loc.GetLocation()))
                return this.piratesOnMap[loc.GetLocation()];
            return null;
        }
        /// <summary>
        /// Returns the pirate whose initial location is on the location given, or null if one does not exist
        /// </summary>
        public Pirate GetPirateSpawnOn(ILocateable loc)
        {
            if (loc == null)
                return null;
            if (this.piratesSpawnOnMap.ContainsKey(loc.GetLocation()))
                return this.piratesSpawnOnMap[loc.GetLocation()];
            return null;
        }
        /// <summary>
        /// Returns the treasures which are on the location given
        /// </summary>
        public List<Treasure> GetTreasuresOn(ILocateable loc)
        {
            if (loc == null)
                return null;
            if (this.treasuresOnMap.ContainsKey(loc.GetLocation()))
                return this.treasuresOnMap[loc.GetLocation()];
            return new List<Treasure>();
        }
        /// <summary>
        /// Returns the treasure whose initial location is on the location given, or null if one does not exist
        /// </summary>
        public Treasure GetTreasureSpawnOn(ILocateable loc)
        {
            if (loc == null)
                return null;
            if (this.treasuresSpawnOnMap.ContainsKey(loc.GetLocation()))
                return this.treasuresSpawnOnMap[loc.GetLocation()];
            return null;
        }


        /// <summary>
        /// Returns a new list, containing the powerups on the map
        /// </summary>
        public List<Powerup> GetPowerups()
        {
            return new List<Powerup>(this.powerups);
        }


        /// <summary>
        /// Returns wether the location given is inside the map
        /// </summary>
        public bool InMap(ILocateable l)
        {
            Location loc = l.GetLocation();
            return loc.Row >= 0 &&
                   loc.Row < this.Rows &&
                   loc.Collumn >= 0 &&
                   loc.Collumn < this.Collumns;
        }

        /// <summary>
        /// Returns a list of neighbors in-map of the location given, that are away from the location as specified
        /// </summary>
        public List<Location> GetNeighbors(ILocateable l, int manhattanDistance)
        {
            if (l == null || l.GetLocation() == null ||
                manhattanDistance < 0 || manhattanDistance >= this.neighbors.GetLength(2) ||
                !this.InMap(l))
                return null;

            return new List<Location> (this.neighbors[l.GetLocation().Row, l.GetLocation().Collumn, manhattanDistance]);
        }


        /// <summary>
        /// Returns wether the given location has the given terrain characteristics
        /// </summary>
        public bool IsTerrain(ILocateable loc, Terrain terrain)
        {
            switch (terrain)
            {
                case Terrain.InEnemyRange:
                    return this.EnemyAttackMap.HasDangerousPiratesInAttackRange(loc);
                case Terrain.EnemyLocation:
                    // check for a current enemy
                    Pirate p = this.GetPirateOn(loc);
                    if (p != null && p.Owner != Owner.Me)
                        return true;
                    // check for an enemy spawning
                    p = this.GetPirateSpawnOn(loc);
                    if (p != null && p.TurnsToRevive == 1 && p.Owner != Owner.Me)
                        return true;
                    // else, there is no enemy there
                    return false;
                case Terrain.CurrentTreasureLocation:
                    return this.GetTreasuresOn(loc).Count > 0;
                default:
                    return false;
            }
        }



        //////////Properties//////////

        /// <summary>
        /// The maximum distance map.GetNeighbors() can get as an input
        /// </summary>
        public int MaxNeighborsDistance
        {
            get { return this.neighbors.GetLength(2); }
        }

        /// <summary>
        /// The value currently being carried by us
        /// </summary>
        public int MyCarriedValue
        {
            get { return this.myCarriedValue; }
        }

        /// <summary>
        /// The value currently being carried by the enemy
        /// </summary>
        public int EnemyCarriedValue
        {
            get { return this.enemyCarriedValue; }
        }



        //////////Static Methods//////////

        /// <summary>
        /// Returns the manhattan distance between two locations ("turn distance")
        /// </summary>
        public static int ManhattanDistance(ILocateable a, ILocateable b)
        {
            if (a == null || b == null)
                return -1;

            Location loc1 = a.GetLocation();
            Location loc2 = b.GetLocation();
            if (loc1 == null || loc2 == null)
                return -1;

            return Utils.ManhattanDistance(loc1.Row, loc1.Collumn, loc2.Row, loc2.Collumn);
        }
        /// <summary>
        /// Returns if the given locations are in (manhattan) range of each other
        /// </summary>
        public static bool InManhattanRange(ILocateable a, ILocateable b, double range)
        {
            if (a == null || a.GetLocation() == null ||
                b == null || b.GetLocation() == null)
                return false;

            return Map.ManhattanDistance(a, b) <= range;
        }
        /// <summary>
        /// Returns the euclidean distance between two locations
        /// </summary>
        public static double EuclideanDistance(ILocateable a, ILocateable b)
        {
            if (a == null || b == null)
                return -1;

            Location loc1 = a.GetLocation();
            Location loc2 = b.GetLocation();
            if (loc1 == null || loc2 == null)
                return -1;

            return Utils.EuclideanDistance(loc1.Row, loc1.Collumn, loc2.Row, loc2.Collumn);
        }
        /// <summary>
        /// Returns if the given locations are in (euclidean) range of each other
        /// </summary>
        public static bool InEuclideanRange(ILocateable a, ILocateable b, double range)
        {
            if (a == null || a.GetLocation() == null ||
                b == null || b.GetLocation() == null)
                return false;

            return Map.EuclideanDistance(a, b) <= range;
        }
        /// <summary>
        /// Returns if the given locations are in attack range of each other (using the "range" given).
        /// The same as Map.InEuclideanRange(a, b, range).
        /// </summary>
        public static bool InAttackRange(ILocateable a, ILocateable b, double range)
        {
            return Map.InEuclideanRange(a, b, range);
        }

        /// <summary>
        /// Returns the arithmetic average of the locations given
        /// </summary>
        public static Location ArithmeticAverage(IList<Location> locations)
        {
            if (locations == null)
                return null;

            // count the coordinates' sums
            int rowSum = 0, collumnSum = 0, activeCount = 0;
            foreach (Location loc in locations)
            {
                if (loc == null)
                    continue;

                rowSum += loc.Row;
                collumnSum += loc.Collumn;
                activeCount++;
            }

            // return the arithmetic average of the locations given
            if (activeCount == 0)
                return null;
            else
                return new Location(rowSum / activeCount, collumnSum / activeCount);
        }

        /// <summary>
        /// Returns whether "c" is on a path between "a" and "b"
        /// </summary>
        public static bool OnTrack(ILocateable a, ILocateable b, ILocateable c)
        {
            if (a == null || a.GetLocation() == null ||
                b == null || b.GetLocation() == null ||
                c == null || c.GetLocation() == null)
                return false;

            return Utils.InSquare(a.GetLocation().Collumn, a.GetLocation().Row,
                                  b.GetLocation().Collumn, b.GetLocation().Row,
                                  c.GetLocation().Collumn, c.GetLocation().Row);
        }

        /// <summary>
        /// Returns whether a, b and c are on the same straight line
        /// </summary>
        public static bool OnStraightLine(ILocateable a, ILocateable b, ILocateable c)
        {
            if (a == null || a.GetLocation() == null ||
                b == null || b.GetLocation() == null ||
                c == null || c.GetLocation() == null)
                return false;

            return Utils.OnStraightLine(a.GetLocation().Row, a.GetLocation().Collumn,
                                        b.GetLocation().Row, b.GetLocation().Collumn,
                                        c.GetLocation().Row, c.GetLocation().Collumn);
        }
    }
}
