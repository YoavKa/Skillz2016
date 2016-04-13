using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

namespace MyBot.Actions.Resources
{
    /// <summary>
    /// The resources an ActionsPack requires
    /// </summary>
    class ResourcesPack
    {
        //////////Attributes//////////

        /// <summary>
        /// My pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        private uint myPirates;
        /// <summary>
        /// The enemy pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        private uint enemyPirates;

        /// <summary>
        /// The treasures that are being used by the ActionsPack; if the i-th bit is on, than the treasure with id i is being used
        /// </summary>
        private uint treasures;

        /// <summary>
        /// The powerups that are being used by the ActionsPack; if the i-th bit is on, than the treasure with id i is being used
        /// </summary>
        private ulong powerups;

        /// <summary>
        /// The locations that were freed-up by the ActionsPack
        /// </summary>
        private HashSet<Location> freedUpLocations;
        /// <summary>
        /// The locations that were taken by the ActionsPack
        /// </summary>
        private HashSet<Location> takenLocations;

        /// <summary>
        /// The amount of actions being used
        /// </summary>
        private int actions;

        /// <summary>
        /// The imaginary resources that are being used by the ActionsPack
        /// </summary>
        private HashSet<int> imaginaryResources;


        /// <summary>
        /// The generic object resources used by the ActionsPack
        /// </summary>
        private HashSet<object> objectResources;


        /// <summary>
        /// The game which will execute the ActionsPack
        /// </summary>
        private Game game;



        //////////Methods//////////

        /// <summary>
        /// Creates a new ResourcesPack
        /// </summary>
        public ResourcesPack(Game game)
        {
            this.myPirates = 0;
            this.enemyPirates = 0;

            this.treasures = 0;
            this.powerups = 0;

            this.freedUpLocations = new HashSet<Location>();
            this.takenLocations = new HashSet<Location>();

            this.imaginaryResources = new HashSet<int>();
            this.objectResources = new HashSet<object>();

            this.actions = 0;

            this.game = game;
        }
        /// <summary>
        /// Creates a new ResourcesPack, based on the pack given
        /// </summary>
        public ResourcesPack(ResourcesPack other)
        {
            this.myPirates = other.myPirates;
            this.enemyPirates = other.enemyPirates;

            this.treasures = other.treasures;
            this.powerups = other.powerups;

            this.freedUpLocations = new HashSet<Location>(other.freedUpLocations);
            this.takenLocations = new HashSet<Location>(other.takenLocations);

            this.imaginaryResources = new HashSet<int>(other.imaginaryResources);
            this.objectResources = new HashSet<object>(other.objectResources);

            this.actions = other.actions;

            this.game = other.game;
        }

        /// <summary>
        /// Clears the data in this ResourcesPack
        /// </summary>
        public void Clear(Game game)
        {
            this.myPirates = 0;
            this.enemyPirates = 0;

            this.treasures = 0;
            this.powerups = 0;

            this.freedUpLocations.Clear();
            this.takenLocations.Clear();

            this.imaginaryResources.Clear();
            this.objectResources.Clear();

            this.actions = 0;

            this.game = game;
        }

        /// <summary>
        /// Checks if this ResourcesPack and the other ResourcesPack use overlapping resources
        /// </summary>
        public bool Overlaps(ResourcesPack other)
        {
            return (this.myPirates & other.myPirates) > 0 ||        // there are pirates overlapping
                   (this.enemyPirates & other.enemyPirates) > 0 ||  // there are pirates overlapping
                   (this.treasures & other.treasures) > 0 ||        // there are treasures overlapping
                   (this.powerups & other.powerups) > 0 ||          // there are powerups overlapping
                   this.takenLocations.Overlaps(other.takenLocations) ||
                   this.imaginaryResources.Overlaps(other.imaginaryResources) ||
                   this.objectResources.Overlaps(other.objectResources) ||
                   this.actions + other.actions > this.game.ActionsPerTurn;
        }

        /// <summary>
        /// Removes the resources in the other ResourcesPack from this one
        /// </summary>
        public void RemoveResources(ResourcesPack other)
        {
            if (other == null)
                return;

            this.myPirates -= this.myPirates & other.myPirates;
            this.enemyPirates -= this.enemyPirates & other.enemyPirates;

            this.treasures -= this.treasures & other.treasures;
            this.powerups -= this.powerups & other.powerups;

            this.takenLocations.ExceptWith(other.takenLocations);
            this.freedUpLocations.ExceptWith(other.freedUpLocations);

            this.imaginaryResources.ExceptWith(other.imaginaryResources);
            this.objectResources.ExceptWith(other.objectResources);

            this.actions -= other.actions;
        }

        /// <summary>
        /// Tries to add the resources of another ResourcesPack into this one, if they do not overlap
        /// </summary>
        /// <returns>Was the addition successful (and they did not overlap)</returns>
        public bool AddResources(ResourcesPack other)
        {
            // if the ResourcesPacks overlap
            if (this.Overlaps(other))
                return false;

            // else, the addition is possible!
            this.myPirates |= other.myPirates;
            this.enemyPirates |= other.enemyPirates;

            this.treasures |= other.treasures;
            this.powerups |= other.powerups;

            this.freedUpLocations.UnionWith(other.freedUpLocations);
            this.takenLocations.UnionWith(other.takenLocations);

            this.imaginaryResources.UnionWith(other.imaginaryResources);
            this.objectResources.UnionWith(other.objectResources);

            this.actions += other.actions;

            return true;
        }

        /// <summary>
        /// Tries to add my pirate to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful (and the pirate is my pirate, and is not already included)</returns>
        public bool AddMyPirate(Pirate p)
        {
            if (p == null || p.Owner != Owner.Me)
                return false;
            uint bit = 1U << p.Id;
            if ((bit & this.myPirates) > 0) // if the pirate is already included
                return false;
            this.myPirates |= bit;
            return true;
        }
        /// <summary>
        /// Tries to add the enemy pirate to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful (and the pirate is an enemy pirate, and is not already included)</returns>
        public bool AddEnemyPirate(Pirate p)
        {
            if (p == null || p.Owner != Owner.Enemy)
                return false;
            uint bit = 1U << p.Id;
            if ((bit & this.enemyPirates) > 0) // if the pirate is already included
                return false;
            this.enemyPirates |= bit;
            return true;
        }

        /// <summary>
        /// Tries to add the treasure to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddTreasure(Treasure t)
        {
            if (t == null)
                return false;
            uint bit = 1U << t.Id;
            if ((bit & this.treasures) > 0) // if the treasure is already included
                return false;
            this.treasures |= bit;
            return true;
        }
        /// <summary>
        /// Tries to add the powerup to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddPowerup(Powerup pu)
        {
            if (pu == null)
                return false;
            ulong bit = 1UL << pu.Id;
            if ((bit & this.powerups) > 0) // if the powerup is already included
                return false;
            this.powerups |= bit;
            return true;
        }

        /// <summary>
        /// Tries to add a freed-up location to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddFreedLocation(ILocateable l)
        {
            if (l == null || l.GetLocation() == null)
                return false;
            return this.freedUpLocations.Add(l.GetLocation());
        }

        /// <summary>
        /// Tries to add a taken location to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddTakenLocation(ILocateable l)
        {
            if (l == null || l.GetLocation() == null)
                return false;
            return this.takenLocations.Add(l.GetLocation());
        }

        /// <summary>
        /// Tries to add used actions to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddActions(int actions)
        {
            if (actions < 0 || this.actions + actions > this.game.ActionsPerTurn)
                return false;

            this.actions += actions;
            return true;
        }

        /// <summary>
        /// Tries to add the imaginary resource to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful (and the resource is not already included)</returns>
        public bool AddImaginaryResource(int resourceId)
        {
            return this.imaginaryResources.Add(resourceId);
        }
        /// <summary>
        /// Tries to add the object to the resources used by the ResourcesPack;
        /// WARNING: USE OF THIS METHOD IS NOT RECOMMENDED
        /// </summary>
        /// <returns>Was the addition successful (and the resource is not already included)</returns>
        public bool AddObjectResource(object obj)
        {
            return this.objectResources.Add(obj);
        }


        /// <summary>
        /// Returns wether the given location is freed in this ResourcesPack or not
        /// </summary>
        public bool IsFreed(Location l)
        {
            if (l == null)
                return false;
            return this.freedUpLocations.Contains(l) && !this.takenLocations.Contains(l);
        }



        //////////Properties//////////

        /// <summary>
        /// The number of actions used by the ActionsPack
        /// </summary>
        public int ActionsUsed
        {
            get { return this.actions; }
        }

        /// <summary>
        /// My pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        public uint MyPiratesHash
        {
            get { return this.myPirates; }
        }

        /// <summary>
        /// The enemy pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        public uint EnemyPiratesHash
        {
            get { return this.enemyPirates; }
        }

        /// <summary>
        /// The treasures that are being used by the ActionsPack; if the i-th bit is on, than the treasure with id i is being used
        /// </summary>
        public uint TreasuresHash
        {
            get { return this.treasures; }
        }
    }
}