using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.API;
using MyBot.States;

namespace MyBot.Events
{
    /// <summary>
    /// An event that can occur in the game
    /// </summary>
    abstract class Event
    {
        //////////Readonly's//////////

        /// <summary>
        /// The unique id of the event; can be used to group ActionsPack, for example
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The priority of the event; the LOWER the better
        /// </summary>
        public readonly int Priority;
        /// <summary>
        /// The aStar priority of the event; the HIGHER the better
        /// </summary>
        public readonly int AStarPriority;


        //////////Methods//////////

        /// <summary>
        /// Creates a new event
        /// </summary>
        public Event(int priority, int aStarPriority)
        {
            this.Id = Event.getNextId();
            this.Priority = priority;
            this.AStarPriority = aStarPriority;
        }

        /// <summary>
        /// Add possible responses to "chooser", based on the situation in "game" and in "statesManager"
        /// </summary>
        public abstract void AddResponseOptions(Game game, StatesManager statesManager, ActionsChooser chooser);



        //////////Static Attributes//////////

        /// <summary>
        /// The next id, to be used by an event
        /// </summary>
        private static int nextId = 0;



        //////////Static Methods//////////

        /// <summary>
        /// Gets the next id, and increments it
        /// </summary>
        private static int getNextId()
        {
            nextId++;
            return nextId-1;
        }
    }
}
