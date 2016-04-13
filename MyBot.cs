using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Actions;
using MyBot.API;
using MyBot.Events;
using MyBot.States;

/*
 * TIPS AND TRICKS:
 * 
 * 
 * ADDITIONS:
 *      - New Command:
 *          API changes needed:
 *              1. the new command needs to be added in /Actions/Commands, preferably by duplicating an existing command in the directory
 *          Usage:
 *              use the new command the same as the other ones, for example: ActionsPack.NewCommandPack(game, new XCommand(...), base.Id);
 *              
 *      - New Powerup:
 *          API changes needed:
 *              1. the new powerup needs to be added in /API, preferably by duplicating an existing powerup in the directory
 *              2. the game needs to "know" about the powerup, by replicating code lines in Powerup.GetPowerup() to be able to create the new powerup type
 *          Usage:
 *              use the new powerup the same as the other ones, for example: mypirate.HasPowerup(XPowerup.NAME); if (powerup is XPowerup) {...};
 *              
 *      - New Terrain:
 *          API changes needed:
 *              1. a new terrain type needs to be added to the Terrain enum at the botttom of API/Game.
 *              2. the support for the new terrain needs to be added in API/Mapping/Map.IsTerrain
 *              3. OPTIONAL: in order to add a default event (so ALL the calls to GetCompleteSailOptions will use it), add the terrain in API/Mapping/SailManager.isAllowedByTerrain
 *          Usage:
 *              use the same as the other terrains (in calls to GetCompleteSailOptions and its overloads)
 *              
 *      - New Pirate State:
 *          API changes needed:
 *              1. add the new state at /API/Pirate.cs, at PirateState
 *              2. make sure the pirate knows its right state, at pirate.Update(Pirates.Pirate p)
 *          Usage:
 *              use the new state the same way you use the other pirate states
 *           
 *      - New Treasure State:
 *          API changes needed:
 *              1. add the new state at /API/Treasure.cs, at TreasureState
 *              2. make sure the treasure knows its right state, at treasure.Update(Map map)
 *          Usage:
 *              use the new state the same way you use the other pirate states
 *
 * 
 * NATIVE OBJECTS:
 *      Currently, the following API objects have the property X.NativeObject, which retrieves the original game object used by the actual game.
 *      Use the native objects only to hack in faster into the API, a full integration to the API is much more recommended:
 *          - Game
 *          
 *          - Pirate
 *          - Treasure (NOTE: treasures which are not free to take, have no native object, thus their native object is null)
 *          - Location
 *          
 *          - AttackPowerup
 *          - SpeedPowerup
 * 
 * 
 * EVENT VALUE NOTES:
 *      - If the event deals with treasures in any way, the value should be multiplied by the treasure's value
 *      - The more important the need for moves, the bigger the exponent of the remaining distance needs to be.
 *        IMPORTANT NOTE: The above is true, only if you subtract the exponent expression from the main value.
 *      - Usually, powerup is an additional multiplier for the main value of the events relating to it.
 * 
 * 
 * GENERAL NOTES:
 *      - If a new object is added to the game (for example, the kraken) and you want to create only one pack including the object, you can add the object to every
 *          ActionsPack using ap.AddObjectResource(obj). Two ActionsPacks that have overlapping object resources (there is an object that is in both of them) won't
 *          be executed together.
 *      - If there is a way to easily detect which pirate carries which treasure, there are two things that need to be done:
 *          1. In Pirate.Update(Pirates.Pirate p), you should save an indicator of the carried treasure (ID for example) as a property.
 *          2. In Treasure.Update(Map map), you should comment all the uncommented stuff and uncomment the commented stuff (comment the first half and uncomment the second half).
 *              Then, you will need to change the if ("p carries this.Id"), to actually check if the pirate carries the current treasure, and then everything should work fine.
 */

namespace MyBot
{
    /// <summary>
    /// The main class of the bot
    /// </summary>
    public class MyBot : Pirates.IPirateBot
    {
        //////////Attributes//////////

        /// <summary>
        /// The game object
        /// </summary>
        private Game game;

        /// <summary>
        /// The list of all possible events, handled by the bot
        /// </summary>
        private List<Event> events;

        /// <summary>
        /// The states manager
        /// </summary>
        private StatesManager statesManager;

        /// <summary>
        /// The ActionsChooser for the game
        /// </summary>
        private ActionsChooser actionsChooser;


        /// <summary>
        /// The timeout gate of the events
        /// </summary>
        public const int EVENT_TIMEOUT_GATE = 100;




        //////////Methods//////////

        /// <summary>
        /// The method that the engine calls whenever our turn arrives
        /// </summary>
        public void DoTurn(IPirateGame game)
        {
            game.Debug("start turn at {0}", game.TimeRemaining());
            // if this is the first turn
            if (game.GetTurn() == 1)
            {
                // create a new game enviroment
                this.game = new Game(game);

                // initialize the event list, and fill it up!
                this.events = new List<Event>();
                this.AddEvents();

                // initialize the states manager, and fill it up!
                this.statesManager = new StatesManager();
                this.AddStates();

                // initialize the actions chooser
                this.actionsChooser = new ActionsChooser(this.game);
            }
            // if this is NOT the first turn
            else
            {
                // update the game enviroment
                this.game.Update(game);

                // update the states manager
                this.statesManager.Update(this.game);
            }

            this.game.Log(LogType.Timing, LogImportance.Important, "finish update at {0}", this.game.GetTimeRemaining());

            // run the best events for us
            this.UpdateChooser();
            this.game.Execute(this.actionsChooser);

            this.game.Log(LogType.Timing, LogImportance.ExtremelyImportant, "finish turn at {0}", this.game.GetTimeRemaining());
            game.Debug("real finish at {0}", game.TimeRemaining());
        }

        /// <summary>
        /// Adds the "mind", the events of the bot to the events list!
        /// </summary>
        private void AddEvents()
        {
            // Add the code to add events here
            foreach (API.Pirate p in this.game.GetAllMyPirates())
            {
                this.events.Add(new FetchTreasureEvent(p));
                this.events.Add(new ReturnTreasureEvent(p));
                this.events.Add(new AttackEvent(p));
                this.events.Add(new SeekAndDestroyEvent(p));
                this.events.Add(new DodgeEvent(p));
                this.events.Add(new AreaClearEvent(p));
                this.events.Add(new EscortEvent(p));
                this.events.Add(new DefendEvent(p));
                this.events.Add(new GarbageCollectionEvent(p));
                this.events.Add(new PowerUpEvent(p));
                this.events.Add(new DrunkBlockadeEvent(p));
            }
            foreach (Cluster<API.Pirate> cluster in this.game.GetEnemySpawnClusters())
            {
                this.events.Add(new DouchebagEvent(cluster, this.game));
            }
            foreach (Cluster<API.Pirate> myCluster in this.game.GetMySpawnClusters())
            {
                this.events.Add(new AntiTerroristEvent(myCluster, this.game));
            }
            foreach (API.Treasure treasure in this.game.GetAllTreasures())
            {
                this.events.Add(new GhostTreasureEvent(treasure));
            }
            this.events = this.events.OrderBy(e => e.Priority).ToList();
        }

        /// <summary>
        /// Adds the states to the states manager
        /// </summary>
        private void AddStates()
        {
            // Add the code to add states here
            this.statesManager.AddState(new ThreatenedTreasureState(this.game));
            this.statesManager.AddState(new ImpendingDoomState(this.game));
            this.statesManager.AddState(new ImpendingVictoryState(this.game));
            this.statesManager.AddState(new TreasureDancingState(this.game));
        }

        /// <summary>
        /// Updates the ActionsChooser containing all the actions we can do this turn
        /// </summary>
        private void UpdateChooser()
        {
            // clears the actions-chooser
            this.actionsChooser.Clear();

            // fill it up
            this.game.Log(LogType.Timing, LogImportance.SomewhatImportant, "start events loading at {0}", this.game.GetTimeRemaining());
            foreach (Event e in this.events)
            {
                ActionsPack.CurrentEventId = e.Id;
                Game.CurrentAStarPriority = e.AStarPriority;
                int oldChooserSize = this.actionsChooser.Count;
                e.AddResponseOptions(this.game, this.statesManager, this.actionsChooser);
                this.game.Log(LogType.Events, LogImportance.NotImportant, "Event {0} created {1} packs", e.Id, this.actionsChooser.Count - oldChooserSize);
                this.game.Log(LogType.Timing, LogImportance.NotImportant, "Finished loading event {0} at {1}", e.Id, this.game.GetTimeRemaining());

                if (this.game.GetTimeRemaining() <= EVENT_TIMEOUT_GATE)
                {
                    this.game.IncreaseAStarPriority();
                    game.Log(LogType.Events, LogImportance.ExtremelyImportant, "Events were cut due to time constraints, disabling a* for following rounds");
                    break;
                }
            }
            this.game.Log(LogType.Timing, LogImportance.SomewhatImportant, "finish events loading at {0}", this.game.GetTimeRemaining());
        }
    }
}