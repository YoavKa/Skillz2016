using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Utilities;

namespace MyBot.States
{
    class StatesManager : IUpdateable<Game>
    {
        //////////Attributes//////////

        /// <summary>
        /// The actuall states' list
        /// </summary>
        private List<State> statesList;



        //////////Methods//////////

        /// <summary>
        /// Creates a new StatesManager
        /// </summary>
        public StatesManager()
        {
            this.statesList = new List<State>();
        }

        /// <summary>
        /// Adds a new state to the StatesManager;
        /// NOTE: "state" won't be accessible if another state of the same type was previously added!
        /// </summary>
        public void AddState(State state)
        {
            if (state != null)
                this.statesList.Add(state);
        }

        /// <summary>
        /// Returns a state
        /// </summary>
        /// <typeparam name="T">The class which the state initiates</typeparam>
        /// <returns>The state which initiates the T class, or null if one couldn't be found</returns>
        public T GetState<T>() where T : State
        {
            foreach (State state in this.statesList)
                if (state is T)
                    return state as T;
            return null;
        }

        /// <summary>
        /// Updates all the states in the StatesManager
        /// </summary>
        /// <param name="game"></param>
        public void Update(Game game)
        {
            foreach (State state in this.statesList)
                state.Update(game);
        }
    }

    /// <summary>
    /// An abstract class for a state
    /// </summary>
    abstract class State : IUpdateable<Game>
    {
        /// <summary>
        /// Updates the state, using the given game; Will be called every turn, from turn 2 onwards
        /// </summary>
        public abstract void Update(Game game);
    }
}
