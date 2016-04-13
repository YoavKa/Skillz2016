using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API.Mapping
{
    /// <summary>
    /// A pirate manager for the game; helps with the retieval of pirates of a bot
    /// </summary>
    class PirateManager : IUpdateable<IPirateGame>
    {
        //////////Attributes//////////

        /// <summary>
        /// All the bot's pirates
        /// </summary>
        private Pirate[] allPirates;

        /// <summary>
        /// An array of all the pirates lists, by states
        /// </summary>
        private List<Pirate>[] piratesByStates;

        /// <summary>
        /// The proxy, containing all the pirates
        /// </summary>
        private UpdateProxy<IPirateGame> allPiratesProxy;



        //////////Readonly's//////////

        /// <summary>
        /// Are there a lot of pirates
        /// </summary>
        public readonly bool Armada;



        //////////Methods//////////

        /// <summary>
        /// Creates a new Pirate Manager, to manage the pirates of "owner"
        /// </summary>
        public PirateManager(IPirateGame game, Owner owner)
        {
            allPiratesProxy = new UpdateProxy<IPirateGame>();

            // choose the list of all pirates to use
            List<Pirates.Pirate> allPirates;
            if (owner == Owner.Me)
                allPirates = game.AllMyPirates();
            else
                allPirates = game.AllEnemyPirates();

            // initialize the lists containing the pirates by states
            this.piratesByStates = new List<Pirate>[(int)PirateState.COUNT];
            for (int i = 0; i < this.piratesByStates.Length; i++)
                this.piratesByStates[i] = new List<Pirate>();

            // initialize the allPirates array
            this.allPirates = new Pirate[allPirates.Count];

            // initialize all the pirates in the allPirates array, and add them to allPiratesProxy and freePirates
            foreach (Pirates.Pirate p in allPirates)
            {
                Pirate newPirate = new Pirate(p, game.GetActionsPerTurn());
                this.allPirates[p.Id] = newPirate;
                this.piratesByStates[(int) PirateState.Free].Add(newPirate);
                this.allPiratesProxy.Add(newPirate);
            }

            this.Armada = this.GetAllPiratesCount() > 6;
        }

        /// <summary>
        /// Update the pirate manager
        /// </summary>
        public void Update(IPirateGame game)
        {
            this.allPiratesProxy.Update(game);

            // clear the lists of pirates by states
            for (int i = 0; i < this.piratesByStates.Length; i++)
                this.piratesByStates[i].Clear();

            // refill the lists of pirates by states
            foreach (Pirate p in this.allPirates)
                this.piratesByStates[(int)p.State].Add(p);
        }


        /// <summary>
        /// Returns a new list, containing all the pirates
        /// </summary>
        public List<Pirate> GetAllPirates()
        {
            return new List<Pirate>(this.allPirates);
        }
        /// <summary>
        /// Returns the count of all the pirates
        /// </summary>
        public int GetAllPiratesCount()
        {
            return this.allPirates.Length;
        }


        /// <summary>
        /// Returns a new list, containing all the pirates of the given states.
        /// </summary>
        public List<Pirate> GetPirates(params PirateState[] states)
        {
            List<Pirate> results = new List<Pirate>();
            foreach (PirateState state in states.Distinct())
                foreach (Pirate p in this.piratesByStates[(int)state])
                    results.Add(p);

            return results;
        }
        /// <summary>
        /// Returns the count of all the pirates of the given states
        /// </summary>
        public int GetPiratesCount(params PirateState[] states)
        {
            int count = 0;
            foreach (PirateState state in states.Distinct())
                count += this.piratesByStates[(int)state].Count;
            return count;
        }


        /// <summary>
        /// Returns the pirate with the given id, or null if one does not exist
        /// </summary>
        public Pirate GetPirate(int id)
        {
            if (!Utils.InBounds(this.allPirates, id))
                return null; // there is no pirate with this id
            return this.allPirates[id];
        }


        /// <summary>
        /// Returns a list of all the pirates which have one of the "states" given, ordered by their manhattan distance to "location"
        /// </summary>
        public List<Pirate> GetClosestPirates(ILocateable location, params PirateState[] states)
        {
            if (location == null || location.GetLocation() == null)
                return new List<Pirate>();

            return this.GetPirates(states).OrderBy(pirate => Map.ManhattanDistance(pirate, location)).ToList();
        }
    }
}
