using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.API.Mapping;

namespace MyBot.API
{
    /// <summary>
    /// A treasure that is (or was) in the game
    /// </summary>
    class Treasure : ILocateable
    {
        //////////Attributes//////////

        /// <summary>
        /// The current location of the treasure, or null if it is already taken
        /// </summary>
        private Location location;

        /// <summary>
        /// The current state of the treasure
        /// </summary>
        private TreasureState state;

        /// <summary>
        /// The pirate currently carries the treasure, or null if is not being carried
        /// </summary>
        private Pirate carryingPirate;


        /// <summary>
        /// The native treasure object
        /// </summary>
        private Pirates.Treasure nativeObject;



        //////////Readonly's//////////

        /// <summary>
        /// The id of the treasure
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The initial location of the treasure
        /// </summary>
        public readonly Location InitialLocation;

        /// <summary>
        /// The value of the treasure
        /// </summary>
        public readonly int Value;



        //////////Methods//////////

        /// <summary>
        /// Creates a new treasure with the parameters given.
        /// carryingPirate can be null.
        /// </summary>
        public Treasure(int id, ILocateable location, TreasureState state, Pirate carryingPirate, int value)
        {
            this.Id = id;
            this.InitialLocation = location.GetLocation();
            this.location = this.InitialLocation;
            this.state = state;
            this.carryingPirate = carryingPirate;
            this.Value = value;

            this.nativeObject = null;
        }
        /// <summary>
        /// Creates a new treasure based on the Treasure given
        /// </summary>
        public Treasure(Treasure other)
            : this(other.Id, other.Location, other.State, other.CarryingPirate, other.Value)
        { }
        /// <summary>
        /// Creates a new treasure based on the Treasure given
        /// </summary>
        public Treasure(Pirates.Treasure other)
            : this(other.Id, new Location(other.Location), TreasureState.FreeToTake, null, other.Value)
        { }

        /// <summary>
        /// Returns true if the object given is the same as this one
        /// </summary>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Treasure);
        }
        /// <summary>
        /// Returns true if the Treasure given is the same as this one
        /// </summary>
        public bool Equals(Treasure other)
        {
            if (other == null)
                return false;

            return this.Id == other.Id;
        }
        /// <summary>
        /// Returns a hash value for this treasure
        /// </summary>
        public override int GetHashCode()
        {
            return this.Id;
        }

        /// <summary>
        /// Returns a string representation of this Location
        /// </summary>
        public override string ToString()
        {
            // append a string describing the location of the treasure, if it is not already taken
            string locationString = (this.Location == null) ? ("") : (", At " + this.Location);
            // append a string describing the pirate carrying it, if it is being carried
            string carryingPirateString = (this.CarryingPirate == null) ? ("") : (", Carried by Pirate " + this.CarryingPirate.Id);
            return "<Treasure " + this.Id + " with value " + this.Value + ": " + this.State + locationString + carryingPirateString + ">";
        }

        /// <summary>
        /// Returns the current location of the treasure, or null if it is already taken
        /// </summary>
        public Location GetLocation()
        {
            return this.Location;
        }

        /// <summary>
        /// Updates the treasure. All of the pirates' objects + some of the map object need to be updated BEFORE calling this method
        /// </summary>
        /// <param name="map">Both map.GetFreeToTakeTreasures() and map.GetPirateAt() need to be updated BEFORE calling this method</param>
        public void Update(Map map)
        {
            // if the pirate doesn't know in advance what he's carrying
            if (map.GetTreasures(TreasureState.FreeToTake).Contains(this))
            {
                // the FreeToTake list is updated, thus this treasure is free to take
                this.state = TreasureState.FreeToTake;
                this.carryingPirate = null;
                this.location = this.InitialLocation;
            }
            else
            {
                // the treasure is NOT FreeToTake
                // lets divide the cases, based on the last state of the treasure
                switch (this.State)
                {
                    case TreasureState.FreeToTake:
                        // if the treasure was free to take last turn and now is not, then it must be carried right now by the pirate
                        // who is at its initial location
                        this.state = TreasureState.BeingCarried;
                        this.carryingPirate = map.GetPirateOn(this.InitialLocation); // the map's pirate matrix is updated
                        this.location = this.InitialLocation;
                        break;
                    case TreasureState.BeingCarried:
                        // if the treasure was being carried last turn, the treasure will be taken if and only if the carrying pirate is now
                        //      at its initial location and is free. Otherwise, it will be carried by the same pirate (if it is still carrying something)
                        //      or by the pirate at the treasure's initial location (not sure if it is even possible)
                        if (this.CarryingPirate.Location.Equals(this.CarryingPirate.InitialLocation) && this.CarryingPirate.State == PirateState.Free)
                        {
                            this.state = TreasureState.Taken;
                            this.carryingPirate = null;
                            this.location = null;
                        }
                        else
                        {
                            if (this.CarryingPirate.State == PirateState.CarryingTreasure)
                            {
                                // only need to update the location of the treasure
                                this.location = this.CarryingPirate.Location;
                            }
                            else
                            {
                                // only need to update the location and the carrying pirate
                                this.carryingPirate = map.GetPirateOn(this.InitialLocation);
                                this.location = this.InitialLocation;
                            }
                        }
                        break;
                    case TreasureState.Taken:
                        // if the treasure was taken last turn, then it must also be taken now; there is nothing to update
                        break;
                }
            }

            // if the pirate DOES know in advance what he's carrying
            //// if the treasure is free
            //if (map.GetTreasures(TreasureState.FreeToTake).Contains(this))
            //{
            //    // the FreeToTake list is updated, thus this treasure is free to take
            //    this.state = TreasureState.FreeToTake;
            //    this.carryingPirate = null;
            //    this.location = this.InitialLocation;
            //}
            //else
            //{
            //    // else, assume the treasure is taken, and change it if the treasure is being carried
            //    this.state = TreasureState.Taken;
            //    this.carryingPirate = null;
            //    this.location = null;

            //    foreach (Pirate p in map.MyPirateManager.GetAllPirates().Union(map.EnemyPirateManager.GetAllPirates()))
            //    {
            //        if ("p carries this.Id") // <-- change the check here, to see if p actually carries this treasure
            //        {
            //            this.state = TreasureState.BeingCarried;
            //            this.carryingPirate = p;
            //            this.location = p.Location;
            //        }
            //    }
            //}
        }



        //////////Properties//////////

        /// <summary>
        /// The location of the treasure
        /// </summary>
        public Location Location
        {
            get { return this.location; }
        }

        /// <summary>
        /// The state of the treasure
        /// </summary>
        public TreasureState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// The pirate currently carries the treasure, or null if is not being carried
        /// </summary>
        public Pirate CarryingPirate
        {
            get { return this.carryingPirate; }
        }


        /// <summary>
        /// The native treasure object, or null if one does not exist
        /// </summary>
        public Pirates.Treasure NativeObject
        {
            get { return this.nativeObject; }
            set
            {
                // if the treausre is not free, than there is not native object;
                // else, set the native object only if the object given is actually ours.
                if (this.State != TreasureState.FreeToTake)
                    this.nativeObject = null;
                else if (value.Id == this.Id)
                    this.nativeObject = value;
            }
        }
    }



    /// <summary>
    /// The states in which a treasure can be in
    /// </summary>
    enum TreasureState
    {
        /// <summary>
        /// The treasure is not taken yet
        /// </summary>
        FreeToTake,
        /// <summary>
        /// The treasure is currently being carried by a pirate
        /// </summary>
        BeingCarried,
        /// <summary>
        /// The treasure has already been taken and left the game
        /// </summary>
        Taken,
        /// <summary>
        /// The number of items in the enum ;)
        /// WARNING: DON'T USE UNLESS YOU KNOW WHAT ARE YOU DOING!
        /// </summary>
        COUNT
    }
}
