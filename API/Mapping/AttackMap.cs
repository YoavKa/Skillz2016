using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API.Mapping
{
    class AttackMap : List<Pirate>
    {
        //////////Attributes//////////

        /// <summary>
        /// The map of the game
        /// </summary>
        private Map map;

        /// <summary>
        /// attackRangeMap[r, c, s] contains all the pirates whose state is s, and they are in attack range of (r, c)
        /// </summary>
        private List<Pirate>[,,] attackRangeMap;

        /// <summary>
        /// A list of the spawn clusters of the pirates
        /// </summary>
        private List<Cluster<Pirate>> spawnClusters;



        //////////Methods//////////

        /// <summary>
        /// Creates a new AttackMap, based on the map and pirates given
        /// </summary>
        public AttackMap(Map map, List<Pirate> pirates)
        {
            this.map = map;

            // initialize attackRangeMap
            this.attackRangeMap = new List<Pirate>[this.map.Rows, this.map.Collumns, (int) PirateState.COUNT];
            for (int row = 0; row < this.attackRangeMap.GetLength(0); row++)
                for (int collumn = 0; collumn < this.attackRangeMap.GetLength(1); collumn++)
                    for (int state = 0; state < this.attackRangeMap.GetLength(2); state++)
                        this.attackRangeMap[row, collumn, state] = new List<Pirate>();
            this.updateAttackRangeMap(pirates);


            // initialize spawnClusters
            this.spawnClusters = Cluster.Clusterify<Pirate>(pirates, map.AttackRadius);
        }

        /// <summary>
        /// Updates the AttackMap, based on the pirates given
        /// </summary>
        public void Update(List<Pirate> pirates)
        {
            this.updateAttackRangeMap(pirates);
        }

        /// <summary>
        /// Updates enemyAttackRangeMap, based on the pirates given
        /// </summary>
        private void updateAttackRangeMap(List<Pirate> pirates)
        {
            // for each cell in the map
            for (int row = 0; row < this.attackRangeMap.GetLength(0); row++)
            {
                for (int collumn = 0; collumn < this.attackRangeMap.GetLength(1); collumn++)
                {
                    // clear the lists
                    for (int state = 0; state < this.attackRangeMap.GetLength(2); state++)
                        this.attackRangeMap[row, collumn, state].Clear();

                    // repopulate it
                    foreach (Pirate p in pirates)
                        if (Map.InAttackRange(p, new Location(row, collumn), p.AttackRadius))
                            // add the pirate, based on its state
                            this.attackRangeMap[row, collumn, (int)p.State].Add(p);
                }
            }
        }

        /// <summary>
        /// Returns a list of all the spawn clusters
        /// </summary>
        public List<Cluster<Pirate>> GetSpawnClusters()
        {
            return this.spawnClusters.Select(c => new Cluster<Pirate>(c)).ToList();
        }

        /// <summary>
        /// Returns a list of pirates that are within attack range of "loc", and their state is "state"
        /// </summary>
        public List<Pirate> GetPiratesInAttackRange(ILocateable loc, PirateState state)
        {
            if (!this.map.InMap(loc))
                return null;
            return new List<Pirate>(this.attackRangeMap[loc.GetLocation().Row, loc.GetLocation().Collumn, (int) state]);
        }

        /// <summary>
        /// Returns a list of pirates that actually CAN attack, that are in attack range of loc
        /// </summary>
        public List<Pirate> GetDangerousPiratesInAttackRange(ILocateable loc)
        {
            if (!this.map.InMap(loc))
                return null;

            return this.attackRangeMap[loc.GetLocation().Row, loc.GetLocation().Collumn, (int)PirateState.Free].FindAll(p => p.CanAttack);
        }

        /// <summary>
        /// Returns wether there are pirates whose state is "state", that are in attack range of "loc"
        /// </summary>
        public bool HasPiratesInAttackRange(ILocateable loc, PirateState state)
        {
            if (!this.map.InMap(loc))
                return false;

            return this.attackRangeMap[loc.GetLocation().Row, loc.GetLocation().Collumn, (int)state].Count > 0;
        }

        /// <summary>
        /// Returns wether there are pirates that actually CAN attack, that are in attack range of loc
        /// </summary>
        public bool HasDangerousPiratesInAttackRange(ILocateable loc)
        {
            if (!this.map.InMap(loc))
                return false;

            foreach (Pirate p in this.attackRangeMap[loc.GetLocation().Row, loc.GetLocation().Collumn, (int)PirateState.Free])
                if (p.CanAttack)
                    return true;
            return false;
        }
    }
}
