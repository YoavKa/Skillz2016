using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API
{
    /// <summary>
    /// A cluster of locatable things on the board
    /// </summary>
    class Cluster<T> : List<T>, ILocateable where T : ILocateable
    {
        //////////Attributes//////////

        /// <summary>
        /// The center of the cluster
        /// </summary>
        private Location center;



        //////////Methods//////////

        /// <summary>
        /// Creates a new empty cluster
        /// </summary>
        public Cluster(Location center)
        {
            this.center = center;
        }
        /// <summary>
        /// Creates a new cluster
        /// </summary>
        public Cluster(Location center, List<T> objectsInCluster)
            : base(objectsInCluster)
        {
            this.center = center;
        }
        /// <summary>
        /// Creates a new cluster based on the location given
        /// </summary>
        public Cluster(Cluster<T> other)
            : this(other.center, other)
        { }

        /// <summary>
        /// Returns the location of the cluster
        /// </summary>
        public Location GetLocation()
        {
            return this.center;
        }
    }

    /// <summary>
    /// A service class for clusterifying things
    /// </summary>
    static class Cluster
    {
        /// <summary>
        /// Returns a list of clusters for the objects given
        /// </summary>
        /// <typeparam name="T">The type of objects to clusterify</typeparam>
        /// <param name="objects">The objects to divide into clusters</param>
        /// <param name="attackRadius">The attack radius of the cluster</param>
        public static List<Cluster<T>> Clusterify<T>(IList<T> objects, double attackRadius) where T : ILocateable
        {
            if (objects == null || objects.Count == 0 || attackRadius <= 0)
                return new List<Cluster<T>>();

            List<Cluster<T>> clusters = new List<Cluster<T>>();

            // find minimum and maximum coordinates
            Location loc = objects[0].GetLocation();
            int minRow = loc.Row;
            int maxRow = minRow;
            int minCol = loc.Collumn;
            int maxCol = minCol;
            foreach (T obj in objects)
            {
                loc = obj.GetLocation();
                if (loc.Row < minRow)
                    minRow = loc.Row;
                if (loc.Row > maxRow)
                    maxRow = loc.Row;
                if (loc.Collumn < minCol)
                    minCol = loc.Collumn;
                if (loc.Collumn > maxCol)
                    maxCol = loc.Collumn;
            }

            // objects to be used inside the loop:
            List<T> objectsInCurrentCluster = new List<T>();
            Cluster<T> bestCluster, curCluster;
            
            // while there are objects outside clusters
            List<T> objectsOutsideClusters = new List<T>(objects);
            while (objectsOutsideClusters.Count > 0)
            {
                // find the point with the maximum amount of objects near it, that are outside spawn clusters
                bestCluster = new Cluster<T>(new Location(minRow, minCol));
                for (int row = minRow; row <= maxRow; row++)
                {
                    for (int collumn = minCol; collumn <= maxCol; collumn++)
                    {
                        // count the objects that are near (row, collumn), that are in piratesOutsideSpawnClusters
                        curCluster = new Cluster<T>(new Location(row, collumn));
                        foreach (T obj in objectsOutsideClusters)
                            if (Game.InAttackRange(obj, curCluster.GetLocation(), attackRadius))
                                curCluster.Add(obj);

                        // save this cluster if it is better
                        if (curCluster.Count > bestCluster.Count)
                            bestCluster = curCluster;
                    }
                }

                // if the center of the objects is a valid cluster to, choose it instead
                Location center = Game.ArithmeticAverage(objects.Select(obj => obj.GetLocation()).ToList());
                curCluster = new Cluster<T>(center);
                foreach (T obj in objectsOutsideClusters)
                    if (Game.InAttackRange(obj, curCluster.GetLocation(), attackRadius))
                        curCluster.Add(obj);

                if (curCluster.Count > bestCluster.Count)
                    bestCluster = curCluster;

                // add the cluster to the clusters' list, and make sure ALL the objects that are in the cluster
                //  (even the ones that are in multiple clusters) are in it
                clusters.Add(bestCluster);
                foreach (T obj in bestCluster)
                    objectsOutsideClusters.Remove(obj);
                bestCluster.Clear();
                foreach (T obj in objects)
                    if (Game.InAttackRange(obj, bestCluster, attackRadius))
                        bestCluster.Add(obj);
            }

            // return the clusters' list
            return clusters;
        }
    }
}
