using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Utilities
{
    /// <summary>
    /// A static class, containing general utility methods
    /// </summary>
    static class Utils
    {
        //////////Constants//////////

        /// <summary>
        /// The seed for the RNG
        /// </summary>
        public const int RNGSeed = 65259;



        //////////Static Attributes//////////

        /// <summary>
        /// A random number generator
        /// </summary>
        public static System.Random RNG = new System.Random(RNGSeed);



        //////////Static Methods//////////

        /// <summary>
        /// Shuffles the IList, using the Fisher–Yates shuffle
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i >= 1; i--)
            {
                int j = Utils.RNG.Next(i + 1);
                // exchange list[i] and list[j]
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        /// <summary>
        /// Returns if the index is in the boundaries of the given array
        /// </summary>
        public static bool InBounds(System.Array array, int index)
        {
            if (array == null)
                return false;
            return index >= 0 && index < array.Length;
        }

        /// <summary>
        /// Returns the euclidean distance between (x1, y1) and (x2, y2)
        /// </summary>
        public static double EuclideanDistance(int x1, int y1, int x2, int y2)
        {
            return Utils.Sqrt(Utils.Pow(x1 - x2, 2) + Utils.Pow(y1 - y2, 2));
        }

        /// <summary>
        /// Returns the manhattan distance between (x1, y1) and (x2, y2)
        /// </summary>
        public static int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Utils.Abs(x1 - x2) + Utils.Abs(y1 - y2);
        }

        /// <summary>
        /// Returns wether a is completely in b
        /// </summary>
        public static bool IsIn<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            return !a.Except(b).Any();
        }

        /// <summary>
        /// Returns x to the power of y
        /// </summary>
        public static double Pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        /// <summary>
        /// Returns the square root of x
        /// </summary>
        public static double Sqrt(double x)
        {
            return Utils.Pow(x, 0.5);
        }

        /// <summary>
        /// Returns the abstract value of x
        /// </summary>
        public static int Abs(int x)
        {
            if (x < 0)
                x *= -1;
            return x;
        }

        /// <summary>
        /// Rounds x to the nearest integer
        /// </summary>
        public static int Round(double x)
        {
            return (int)System.Math.Round(x);
        }

        /// <summary>
        /// Returns the minimum double from the doubles given
        /// </summary>
        public static double Min(params double[] doubles)
        {
            double min = doubles[0];
            for (int i = 1; i < doubles.Length; i++)
                if (doubles[i] < min)
                    min = doubles[i];
            return min;
        }

        /// <summary>
        /// Returns the maximum double from the doubles given
        /// </summary>
        public static double Max(params double[] doubles)
        {
            double max = doubles[0];
            for (int i = 1; i < doubles.Length; i++)
                if (doubles[i] > max)
                    max = doubles[i];
            return max;
        }

        /// <summary>
        /// Returns whether (x3, y3) is inside the square whose corners are (x1, y1) and (x2, y2)
        /// </summary>
        public static bool InSquare(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            bool isOnXAxis = x3 >= Utils.Min(x1, x2) && x3 <= Utils.Max(x1, x2);
            bool isOnYAxis = y3 >= Utils.Min(y1, y2) && y3 <= Utils.Max(y1, y2);
            return isOnXAxis && isOnYAxis;
        }

        /// <summary>
        /// Returns whether (x3, y3) is on the straight line from (x1, y1) to (x2, y2)
        /// </summary>
        public static bool OnStraightLine(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            if (x1 == x2 && y1 == y2 && x1 == x3 && y1 == y3)
                return true;
            else if ((x1 == x2 && y1 == y2) || (x1 == x3 && y1 == y3) || (x2 == x3 && y2 == y3))
                return false;
            else
                return (y2 - y1) * (x3 - x2) == (x2 - x1) * (y3 - y2);
        }
    }
}
