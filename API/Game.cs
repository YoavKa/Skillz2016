using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.API.Debug;
using MyBot.API.General;
using MyBot.API.Mapping;

using MyBot.API.Prediction.Attack;
using MyBot.API.Prediction.Movement;

using MyBot.Actions;
using MyBot.Actions.Commands;

using MyBot.Utilities;

namespace MyBot.API
{
    /// <summary>
    /// A facade-like class, which contains all the API's methods to be used by the bot
    /// </summary>
    class Game : IUpdateable<IPirateGame>
    {
        //////////Attributes//////////

        /// <summary>
        /// The current turn's object
        /// </summary>
        private Turn currentTurn;

        /// <summary>
        /// The map of the game
        /// </summary>
        private Map map;

        /// <summary>
        /// This game's logger
        /// </summary>
        private Logger logger;

        /// <summary>
        /// The attack prediction manager of the game
        /// </summary>
        private AttackPredictionManager attackPrediction;
        /// <summary>
        /// The movement prediction manager of the game
        /// </summary>
        private MovementPredictionManager movementPrediction;

        /// <summary>
        /// The proxy which contains all the objects which should be updated at update
        /// </summary>
        private UpdateProxy<IPirateGame> updateProxy;
        /// <summary>
        /// Another update proxy
        /// </summary>
        private UpdateProxy<Game> myUpdateProxy;


        /// <summary>
        /// The native game object
        /// </summary>
        private IPirateGame nativeObject;



        //////////Readonly's//////////

        /// <summary>
        /// The maximum possible points one can reach in the game
        /// </summary>
        public readonly int MaxPoints;
        /// <summary>
        /// The time in turns until a dead pirate spawns again
        /// </summary>
        public readonly int TurnsUntilSpawn;
        /// <summary>
        /// The time in turns until a drunk pirate gets sober again
        /// </summary>
        public readonly int TurnsUntilSober;
        /// <summary>
        /// The minimum time in turns between a pirate's attacks
        /// </summary>
        public readonly int TurnsUntilAttackReload;
        /// <summary>
        /// The minimum time in turns between a pirate's defenses
        /// </summary>
        public readonly int TurnsUntilDefenseReload;
        /// <summary>
        /// The duration of a defense, in turns
        /// </summary>
        public readonly int DefenseDuration;
        /// <summary>
        /// The maximum amount of actions each bot can execute each turn
        /// </summary>
        public readonly int ActionsPerTurn;
        /// <summary>
        /// The maximum amount of turns the game can run before a draw is reached
        /// </summary>
        public readonly int MaxTurns;
        /// <summary>
        /// The current opponent's name
        /// </summary>
        public readonly string OpponentName;
        /// <summary>
        /// The maximum amount of commands that can be executed in the same turn
        /// </summary>
        public readonly int MaxCommandsPerTurn;



        //////////Methods//////////

        /// <summary>
        /// Create a new API's game, using the IPirateGame given
        /// </summary>
        public Game(IPirateGame game)
        {
            this.nativeObject = game;


            // each command "takes" at least one pirate
            this.MaxCommandsPerTurn = game.AllMyPirates().Count;
            
            this.currentTurn = new Turn(game);

            this.map = new Map(game);

            this.logger = new Logger(game);

            // creates the predictions
            this.attackPrediction = new AttackPredictionManager(this);
            this.movementPrediction = new MovementPredictionManager(this);


            // adds the appropriate objects to the update proxys
            this.updateProxy = new UpdateProxy<IPirateGame>();
            this.updateProxy.Add(this.currentTurn);
            this.updateProxy.Add(this.map);
            this.updateProxy.Add(this.logger);

            this.myUpdateProxy = new UpdateProxy<Game>();
            this.myUpdateProxy.Add(this.attackPrediction);
            this.myUpdateProxy.Add(this.movementPrediction);


            // initiates this game's constants
            this.MaxPoints = game.GetMaxPoints();
            this.TurnsUntilSpawn = game.GetSpawnTurns();
            this.TurnsUntilSober = game.GetSoberTurns();
            this.TurnsUntilAttackReload = game.GetReloadTurns();
            this.TurnsUntilDefenseReload = game.GetDefenseReloadTurns();
            this.DefenseDuration = game.GetDefenseExpirationTurns();
            this.ActionsPerTurn = game.GetActionsPerTurn();
            this.MaxTurns = game.GetMaxTurns();
            this.OpponentName = game.GetOpponentName();
        }

        /// <summary>
        /// Updates the API's game, using the IPirateGame given
        /// </summary>
        public void Update(IPirateGame game)
        {
            this.nativeObject = game;

            this.updateProxy.Update(game);
            this.myUpdateProxy.Update(this);
        }



        //////////Facade Methods//////////

        /// <summary>
        /// Returns a new list, containing all my pirates
        /// </summary>
        public List<Pirate> GetAllMyPirates()
        {
            return this.map.MyPirateManager.GetAllPirates();
        }
        /// <summary>
        /// Returns the count of all my pirates
        /// </summary>
        public int GetAllMyPiratesCount()
        {
            return this.map.MyPirateManager.GetAllPiratesCount();
        }
        /// <summary>
        /// Returns a new list, containing all my pirates of the given states.
        /// </summary>
        public List<Pirate> GetMyPirates(params PirateState[] states)
        {
            return this.map.MyPirateManager.GetPirates(states);
        }
        /// <summary>
        /// Returns the count of all my pirates of the given states
        /// </summary>
        public int GetMyPiratesCount(params PirateState[] states)
        {
            return this.map.MyPirateManager.GetPiratesCount(states);
        }
        /// <summary>
        /// Returns my pirate with the given id, or null if one does not exist
        /// </summary>
        public Pirate GetMyPirate(int id)
        {
            return this.map.MyPirateManager.GetPirate(id);
        }
        /// <summary>
        /// Returns a list of all of the spawn clusters of us
        /// </summary>
        public List<Cluster<Pirate>> GetMySpawnClusters()
        {
            return this.map.MyAttackMap.GetSpawnClusters();
        }
        /// <summary>
        /// Returns a list of all my pirates which have one of the "states" given, ordered by their manhattan distance to "location"
        /// </summary>
        public List<Pirate> GetMyClosestPirates(ILocateable location, params PirateState[] states)
        {
            return this.map.MyPirateManager.GetClosestPirates(location, states);
        }
        /// <summary>
        /// Returns a list of my pirates that are within attack range of "loc", and their state is "state"
        /// </summary>
        public List<Pirate> GetMyPiratesInAttackRange(ILocateable loc, PirateState state)
        {
            return this.map.MyAttackMap.GetPiratesInAttackRange(loc, state);
        }
        /// <summary>
        /// Returns a list of my pirates that actually CAN attack, that are in attack range of loc
        /// </summary>
        public List<Pirate> GetMyDangerousPiratesInAttackRange(ILocateable loc)
        {
            return this.map.MyAttackMap.GetDangerousPiratesInAttackRange(loc);
        }


        /// <summary>
        /// Returns a new list, containing all the enemy pirates
        /// </summary>
        public List<Pirate> GetAllEnemyPirates()
        {
            return this.map.EnemyPirateManager.GetAllPirates();
        }
        /// <summary>
        /// Returns the count of all the enemy pirates
        /// </summary>
        public int GetAllEnemyPiratesCount()
        {
            return this.map.EnemyPirateManager.GetAllPiratesCount();
        }
        /// <summary>
        /// Returns a new list, containing all the enemy pirates of the given states.
        /// </summary>
        public List<Pirate> GetEnemyPirates(params PirateState[] states)
        {
            return this.map.EnemyPirateManager.GetPirates(states);
        }
        /// <summary>
        /// Returns the count of all the enemy pirates of the given states
        /// </summary>
        public int GetEnemyPiratesCount(params PirateState[] states)
        {
            return this.map.EnemyPirateManager.GetPiratesCount(states);
        }
        /// <summary>
        /// Returns the enemy pirate with the given id, or null if one does not exist
        /// </summary>
        public Pirate GetEnemyPirate(int id)
        {
            return this.map.EnemyPirateManager.GetPirate(id);
        }
        /// <summary>
        /// Returns a list of all of the spawn clusters of the enemy
        /// </summary>
        public List<Cluster<Pirate>> GetEnemySpawnClusters()
        {
            return this.map.EnemyAttackMap.GetSpawnClusters();
        }
        /// <summary>
        /// Returns a list of all the enemy pirates which have one of the "states" given, ordered by their manhattan distance to "location"
        /// </summary>
        public List<Pirate> GetEnemyClosestPirates(ILocateable location, params PirateState[] states)
        {
            return this.map.EnemyPirateManager.GetClosestPirates(location, states);
        }
        /// <summary>
        /// Returns a list of the enemy pirates that are within attack range of "loc", and their state is "state"
        /// </summary>
        public List<Pirate> GetEnemyPiratesInAttackRange(ILocateable loc, PirateState state)
        {
            return this.map.EnemyAttackMap.GetPiratesInAttackRange(loc, state);
        }
        /// <summary>
        /// Returns a list of enemy pirates that actually CAN attack, that are in attack range of loc
        /// </summary>
        public List<Pirate> GetEnemyDangerousPiratesInAttackRange(ILocateable loc)
        {
            return this.map.EnemyAttackMap.GetDangerousPiratesInAttackRange(loc);
        }




        /// <summary>
        /// Returns the pirate which is on the location given, or null if one does not exist
        /// </summary>
        public Pirate GetPirateOn(ILocateable l)
        {
            return this.map.GetPirateOn(l);
        }
        /// <summary>
        /// Returns the pirate whose initial location is on the location given, or null if one does not exist
        /// </summary>
        public Pirate GetPirateSpawnOn(ILocateable l)
        {
            return this.map.GetPirateSpawnOn(l);
        }


        /// <summary>
        /// Returns a new list, containing all the treasures
        /// </summary>
        public List<Treasure> GetAllTreasures()
        {
            return this.map.GetAllTreasures();
        }
        /// <summary>
        /// Returns the count of all the treasures
        /// </summary>
        /// <returns></returns>
        public int GetAllTreasuresCount()
        {
            return this.map.GetAllTreasuresCount();
        }
        /// <summary>
        /// Returns a new list, containing all the treasures of the given states.
        /// </summary>
        public List<Treasure> GetTreasures(params TreasureState[] states)
        {
            return this.map.GetTreasures(states);
        }
        /// <summary>
        /// Returns the count of all the treasures of the given states
        /// </summary>
        public int GetTreasuresCount(params TreasureState[] states)
        {
            return this.map.GetTreasuresCount(states);
        }
        /// <summary>
        /// Returns the treasure with the given id, or null if one does not exist
        /// </summary>
        public Treasure GetTreasure(int id)
        {
            return this.map.GetTreasure(id);
        }
        /// <summary>
        /// Returns the treasure which are on the location given
        /// </summary>
        public List<Treasure> GetTreasuresOn(ILocateable l)
        {
            return this.map.GetTreasuresOn(l);
        }
        /// <summary>
        /// Returns the treasure whose initial location is on the location given, or null if one does not exist
        /// </summary>
        public Treasure GetTreasureSpawnOn(ILocateable l)
        {
            return this.map.GetTreasureSpawnOn(l);
        }
        /// <summary>
        /// Returns a list of all of the spawn clusters of the treasures
        /// </summary>
        public List<Cluster<Treasure>> GetTreasuresSpawnClusters()
        {
            return this.map.GetTreasuresSpawnClusters();
        }


        /// <summary>
        /// Returns a new list, containing the powerups on the map
        /// </summary>
        public List<Powerup> GetPowerups()
        {
            return this.map.GetPowerups();
        }


        /// <summary>
        /// Returns the time in miliseconds until timeout
        /// </summary>
        public int GetTimeRemaining()
        {
            return this.currentTurn.GetTimeRemaining();
        }


        /// <summary>
        /// Executes the best commands out of the given ActionsChooser
        /// </summary>
        public void Execute(ActionsChooser chooser)
        {
            // if chooser is null
            if (chooser == null)
                return;

            ActionsPack bestPack = chooser.ChooseBestStableCombination();

            //increase readability of game-log
            this.Log(LogType.Events, LogImportance.ExtremelyImportant, " ");
            this.Log(LogType.Events, LogImportance.ExtremelyImportant, "------START-EVENT-INFO------");

            // execute all the commands from the best ActionsPack chosen
            foreach (Command cmd in bestPack.GetCommands())
            {
                if (cmd == null)
                    continue;

                cmd.Execute(this.NativeObject);

                this.Log(LogType.Events, LogImportance.ExtremelyImportant, cmd); // log the command in-game
                foreach (string info in cmd.GetAdditionalInformation()) // log the additional info in-game
                    this.Log(LogType.Events, LogImportance.ExtremelyImportant, "Additional information: {0}", info);
                this.Log(LogType.Events, LogImportance.ExtremelyImportant, " ");
            }
            //increase readability of game-log
            this.Log(LogType.Events, LogImportance.ExtremelyImportant, "-------END-EVENT-INFO-------");
            this.Log(LogType.Events, LogImportance.ExtremelyImportant, " ");
        }


        /// <summary>
        /// Prints the message in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, string message)
        {
            this.logger.Log(type, importance, message);
        }
        /// <summary>
        /// Prints the messages in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, string format, params object[] messages)
        {
            this.logger.Log(type, importance, format, messages);
        }
        /// <summary>
        /// Prints the object given in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, object obj)
        {
            this.logger.Log(type, importance, obj);
        }


        /// <summary>
        /// Returns wether the location given is inside the map
        /// </summary>
        public bool InMap(ILocateable l)
        {
            return this.map.InMap(l);
        }

        /// <summary>
        /// Returns a list of neighbors in-map of the location given, that are away from the location as specified
        /// </summary>
        public List<Location> GetNeighbors(ILocateable l, int manhattanDistance)
        {
            return this.map.GetNeighbors(l, manhattanDistance);
        }

        /// <summary>
        /// Returns wether the given location has the given terrain characteristics
        /// </summary>
        public bool IsTerrain(ILocateable loc, Terrain terrain)
        {
            return this.map.IsTerrain(loc, terrain);
        }


        /// <summary>
        /// Returns wether the attackingPirate can attack this turn the attackedPirate
        /// </summary>
        public bool IsAttackPossible(Pirate attackingPirate, Pirate attackedPirate)
        {
            // if one of the objects is null
            if (attackingPirate == null || attackedPirate == null)
                return false;
            // if the pirates are of the same owner
            else if (attackingPirate.Owner == attackedPirate.Owner)
                return false;
            // if attackingPirate cannot attack
            else if (attackingPirate.State != PirateState.Free || !attackingPirate.CanAttack)
                return false;
            // if attackedPirate cannot be attacked or is defended
            else if (attackedPirate.State == PirateState.Lost ||
                     attackedPirate.State == PirateState.Drunk ||
                     attackedPirate.DefenseDuration > 0)
                return false;
            // if attackingPirate and attackedPirate are not in attack range
            else if (!Game.InAttackRange(attackingPirate, attackedPirate, attackingPirate.AttackRadius))
                return false;

            // else, attackingPirate can attack attackedPirate!
            return true;
        }


        /// <summary>
        /// Returns the naive sail options that the native game object returns
        /// </summary>
        public List<Location> GetNaiveSailOptions(Pirate pirate, ILocateable destination, int moves)
        {
            return this.map.SailManager.GetNaiveSailOptions(pirate, destination, moves);
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
            return this.map.SailManager.GetCompleteSailOptions(from, to, minToDistance, maxToDistance, maxDistancePerTurn, dontStayInRange, minMoves, maxMoves, maxResultsForMovesNum, disallowedTerrain);
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
            return this.map.SailManager.GetCompleteSailOptions(pirate, to, minToDistance, maxToDistance, dontStayInRange, disallowedTerrain);
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
            return this.map.SailManager.GetCompleteSailOptions(pirate, to, dontStayInRange, disallowedTerrain);
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
            return this.map.SailManager.GetCompleteSailOptions(pirate, to, minToDistance, maxToDistance, disallowedTerrain);
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
            return this.map.SailManager.GetCompleteSailOptions(pirate, to, disallowedTerrain);
        }

        /// <summary>
        /// Increases the priority from which the A* works
        /// </summary>
        public void IncreaseAStarPriority()
        {
            this.map.SailManager.IncreasePriority();
        }

        /// <summary>
        /// Returns whether the pirate will attack next turn, or PredictionResult.NotSure if no prediction was made
        /// </summary>
        public PredictionResult PredictAttack(Pirate p)
        {
            if (p != null && !p.CanAttack)
                return PredictionResult.False;
            return this.attackPrediction.Predict(p);
        }
        /// <summary>
        /// Returns the predicted location of the pirate, or null if no prediction was made
        /// </summary>
        public Location PredictMovement(Pirate p)
        {
            return this.movementPrediction.Predict(p);
        }

        //////////Facade Properties//////////

        /// <summary>
        /// The current turn number
        /// </summary>
        public int CurrentTurn
        {
            get { return this.currentTurn.TurnNumber; }
        }

        /// <summary>
        /// My current score
        /// </summary>
        public int MyScore
        {
            get { return this.currentTurn.MyScore; }
        }
        /// <summary>
        /// The enemy's current score
        /// </summary>
        public int EnemyScore
        {
            get { return this.currentTurn.EnemyScore; }
        }

        /// <summary>
        /// The value currently being carried by us
        /// </summary>
        public int MyCarriedValue
        {
            get { return this.map.MyCarriedValue; }
        }
        /// <summary>
        /// The value currently being carried by the enemy
        /// </summary>
        public int EnemyCarriedValue
        {
            get { return this.map.EnemyCarriedValue; }
        }

        /// <summary>
        /// The number of rows of the map
        /// </summary>
        public int Rows
        {
            get { return this.map.Rows; }
        }
        /// <summary>
        /// The number of collumns of the map
        /// </summary>
        public int Collumns
        {
            get { return this.map.Collumns; }
        }

        /// <summary>
        /// Is the map a big one
        /// </summary>
        public bool IsBig
        {
            get { return this.map.IsBig; }
        }
        /// <summary>
        /// Are there a lot of my pirates
        /// </summary>
        public bool MyArmada
        {
            get { return this.map.MyPirateManager.Armada; }
        }
        /// <summary>
        /// Are there a lot of enemy pirates
        /// </summary>
        public bool EnemyArmada
        {
            get { return this.map.EnemyPirateManager.Armada; }
        }

        /// <summary>
        /// The maximum euclidian distance between two pirates, where they can still attack each other, when they don't use powerups!
        /// </summary>
        public double AttackRadius
        {
            get { return this.map.AttackRadius; }
        }

        /// <summary>
        /// The maximum distance game.GetNeighbors() can get as an input
        /// </summary>
        public int MaxNeighborsDistance
        {
            get { return this.map.MaxNeighborsDistance; }
        }


        /// <summary>
        /// The native game object
        /// </summary>
        public IPirateGame NativeObject
        {
            get { return this.nativeObject; }
        }



        //////////Static Attributes//////////

        /// <summary>
        /// The current priority of the A* algorithm; The higher the better
        /// </summary>
        public static int CurrentAStarPriority = -1;



        //////////Static Methods//////////

        /// <summary>
        /// Returns the manhattan distance between two locations ("turn distance")
        /// </summary>
        public static int ManhattanDistance(ILocateable a, ILocateable b)
        {
            return Map.ManhattanDistance(a, b);
        }
        /// <summary>
        /// Returns if the given locations are in (manhattan) range of each other
        /// </summary>
        public static bool InManhattanRange(ILocateable a, ILocateable b, double range)
        {
            return Map.InManhattanRange(a, b, range);
        }
        /// <summary>
        /// Returns the euclidean distance between two locations
        /// </summary>
        public static double EuclideanDistance(ILocateable a, ILocateable b)
        {
            return Map.EuclideanDistance(a, b);
        }
        /// <summary>
        /// Returns if the given locations are in (euclidean) range of each other
        /// </summary>
        public static bool InEuclideanRange(ILocateable a, ILocateable b, double range)
        {
            return Map.InEuclideanRange(a, b, range);
        }
        /// <summary>
        /// Returns if the given locations are in attack range of each other (using the "range" given).
        /// The same as Game.InEuclideanRange(a, b, range).
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
            return Map.ArithmeticAverage(locations);
        }

        /// <summary>
        /// Returns whether "c" is on a path between "a" and "b"
        /// </summary>
        public static bool OnTrack(ILocateable a, ILocateable b, ILocateable c)
        {
            return Map.OnTrack(a, b, c);
        }

        /// <summary>
        /// Returns whether a, b and c are on the same straight line
        /// </summary>
        public static bool OnStraightLine(ILocateable a, ILocateable b, ILocateable c)
        {
            return Map.OnStraightLine(a, b, c);
        }
    }


    /// <summary>
    /// The terrain characteristics which a location in the game can have
    /// </summary>
    enum Terrain
    {
        /// <summary>
        /// The location is in attack range of an enemy pirate
        /// </summary>
        InEnemyRange,
        /// <summary>
        /// The location is currently an enemy location
        /// </summary>
        EnemyLocation,
        /// <summary>
        /// The location is currently a treasure location
        /// </summary>
        CurrentTreasureLocation
    }


    /// <summary>
    /// The different types of log messages
    /// </summary>
    enum LogType
    {
        /// <summary>
        /// Timing messages
        /// </summary>
        Timing,
        /// <summary>
        /// Messages relating to events
        /// </summary>
        Events,
        /// <summary>
        /// Choosing of events messages
        /// </summary>
        ActionsChoosing,
        /// <summary>
        /// Messages related to clusters
        /// </summary>
        Clusters,
        /// <summary>
        /// Messages related to prediction
        /// </summary>
        Prediction,
        /// <summary>
        /// Messages related to debugging
        /// </summary>
        Debug
    }
    /// <summary>
    /// The possible importance value a log message can have
    /// </summary>
    enum LogImportance
    {
        NotImportant,
        SomewhatImportant,
        Important,
        ExtremelyImportant
    }
    /// <summary>
    /// The possible results a prediction can return
    /// </summary>
    enum PredictionResult
    {
        /// <summary>
        /// The prediction doesn't know what will happen
        /// </summary>
        NotSure,
        /// <summary>
        /// The prediction thinks the action won't happen
        /// </summary>
        False,
        /// <summary>
        /// The prediction thinks the action WILL happen
        /// </summary>
        True
    }
}
