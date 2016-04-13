using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

namespace MyBot.API
{
    /// <summary>
    /// A location, than CAN be on the map (doesn't have to!)
    /// </summary>
    class Location : ILocateable
    {
        //////////Readonly's//////////

        /// <summary>
        /// The row the location
        /// </summary>
        public readonly int Row;
        /// <summary>
        /// The collumn of the location
        /// </summary>
        public readonly int Collumn;


        /// <summary>
        /// The native location object
        /// </summary>
        public readonly Pirates.Location NativeObject;



        //////////Methods//////////

        /// <summary>
        /// Creates a new location with the parameters given
        /// </summary>
        public Location(int row, int collumn)
        {
            this.Row = row;
            this.Collumn = collumn;

            this.NativeObject = new Pirates.Location(row, collumn);
        }
        /// <summary>
        /// Creates a new location based on the Location given
        /// </summary>
        public Location(Location other)
            : this(other.Row, other.Collumn)
        { }
        /// <summary>
        /// Creates a new location based on the Location given
        /// </summary>
        public Location(Pirates.Location other)
            : this(other.Row, other.Col)
        { }

        /// <summary>
        /// Returns true if the object given is the same as this one
        /// </summary>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Location);
        }
        /// <summary>
        /// Returns true if the Location given is the same as this one
        /// </summary>
        public bool Equals(Location other)
        {
            if (other == null)
                return false;

            return this.Row == other.Row && this.Collumn == other.Collumn;
        }
        /// <summary>
        /// Returns a hash value for this location
        /// </summary>
        public override int GetHashCode()
        {
            return 100*this.Collumn + this.Row;
        }

        /// <summary>
        /// Returns a string representation of this Location
        /// </summary>
        public override string ToString()
        {
            return "(" + this.Row + ", " + this.Collumn + ")";
        }

        /// <summary>
        /// Returns this location, for compatibility
        /// </summary>
        public Location GetLocation()
        {
            return this;
        }
    }



    /// <summary>
    /// An interface for objects which have a Location
    /// </summary>
    interface ILocateable
    {
        /// <summary>
        /// Returns the Location of the object
        /// </summary>
        Location GetLocation();
    }
}
