using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API
{
    /// <summary>
    /// A pirate of one on the two competing teams
    /// </summary>
    class Pirate : IUpdateable<IPirateGame>, ILocateable
    {
        //////////Attributes//////////

        /// <summary>
        /// The current state of the pirate
        /// </summary>
        private PirateState state;

        /// <summary>
        /// The current location of the pirate
        /// </summary>
        private Location location;

        /// <summary>
        /// The time in turns until the pirate is sober again; 0 if it is already sober
        /// </summary>
        private int turnsToSober;
        /// <summary>
        /// The time in turns until the pirate is alive again; 0 if it is already alive
        /// </summary>
        private int turnsToRevive;
        /// <summary>
        /// The time in turns until the pirate can attack again; 0 if it can attack already
        /// </summary>
        private int turnsToAttackReload;
        /// <summary>
        /// The time in turns until the priate can defend again; 0 if it can defend already
        /// </summary>
        private int turnsToDefenseReload;
        /// <summary>
        /// The time in turns until the pirate will no longer be defended; 0 if it is not defended
        /// </summary>
        private int defenseDuration;

        /// <summary>
        /// The treasure currently being carried by the pirate, or null if is not carrying anything
        /// </summary>
        private Treasure carriedTreasure;

        /// <summary>
        /// The current powerups the pirate has
        /// </summary>
        private List<string> powerups;

        /// <summary>
        /// The current maximum speed of the pirate
        /// </summary>
        private int maxSpeed;
        /// <summary>
        /// The current attack radius of the pirate
        /// </summary>
        private double attackRadius;


        /// <summary>
        /// The native pirate object
        /// </summary>
        private Pirates.Pirate nativeObject;



        //////////Readonly's//////////

        /// <summary>
        /// The id of the pirate
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The initial location of the pirate
        /// </summary>
        public readonly Location InitialLocation;

        /// <summary>
        /// The owner of the pirate
        /// </summary>
        public readonly Owner Owner;

        /// <summary>
        /// The maximum speed for a free pirate
        /// </summary>
        private readonly int freePirateMaxSpeed;



        //////////Methods//////////

        /// <summary>
        /// Creates a new pirate based on the parameter given
        /// </summary>
        public Pirate(Pirates.Pirate p, int freePirateMaxSpeed)
        {
            this.freePirateMaxSpeed = freePirateMaxSpeed;

            this.Id = p.Id;
            this.InitialLocation = new Location(p.InitialLocation);

            if (p.Owner == Consts.ME)
                this.Owner = Owner.Me;
            else
                this.Owner = Owner.Enemy;

            this.Update(p);
        }

        /// <summary>
        /// Updates this pirate
        /// </summary>
        public void Update(IPirateGame game)
        {
            // gets the native pirate object from the game object
            Pirates.Pirate p;
            if (this.Owner == Owner.Me)
                p = game.GetMyPirate(this.Id);
            else
                p = game.GetEnemyPirate(this.Id);

            this.Update(p);
        }
        /// <summary>
        /// Updates this pirate
        /// </summary>
        public void Update(Pirates.Pirate p)
        {
            this.nativeObject = p;

            // updates the pirate's state
            if (p.IsLost)
                this.state = PirateState.Lost;
            else if (p.TurnsToSober > 0)
                this.state = PirateState.Drunk;
            else if (p.HasTreasure)
                this.state = PirateState.CarryingTreasure;
            else
                this.state = PirateState.Free;

            // updates the pirate's location, but ONLY if the new location can possibly be inside the map
            if (p.Location != null && p.Location.Row >= 0 && p.Location.Col >= 0)
                this.location = new Location(p.Location);

            // updates counters
            this.turnsToSober = p.TurnsToSober;
            this.turnsToRevive = p.TurnsToRevive;
            this.turnsToAttackReload = p.ReloadTurns;
            this.turnsToDefenseReload = p.DefenseReloadTurns;
            this.defenseDuration = p.DefenseExpirationTurns;

            // updates carried treasure; should be updated to not-null manually every turn
            this.carriedTreasure = null;

            // updates powerups
            if (p.Powerups != null)
                this.powerups = new List<string>(p.Powerups);
            else
                this.powerups = new List<string>();

            // calculate current speed limit
            if (this.State == PirateState.CarryingTreasure)
                this.maxSpeed = p.CarryTreasureSpeed;
            else
                this.maxSpeed = this.freePirateMaxSpeed;

            // calculate attack radius
            this.attackRadius = Utils.Pow(p.AttackRadius, 0.5);
        }

        /// <summary>
        /// Returns the location of the pirate
        /// </summary>
        public Location GetLocation()
        {
            return this.Location;
        }

        /// <summary>
        /// Returns true if the object given is the same as this one
        /// </summary>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Pirate);
        }
        /// <summary>
        /// Returns true if the Pirate given is the same as this one
        /// </summary>
        public bool Equals(Pirate other)
        {
            if (other == null)
                return false;

            return this.Owner == other.Owner && this.Id == other.Id;
        }
        /// <summary>
        /// Returns a hash value for this pirate
        /// </summary>
        public override int GetHashCode()
        {
            return this.Id*10 + (int)this.Owner;
        }

        /// <summary>
        /// Returns a string representation of this pirate
        /// </summary>
        public override string ToString()
        {
            // append a string describing the location of the pirate, if it is not lost
            string locationString = (this.Location == null) ? ("") : (", At " + this.Location);
            return "<Pirate " + this.Id + " owned by " + this.Owner + ": " + this.State + locationString + ">";
        }


        /// <summary>
        /// Returns whether the pirate has a powerup of the given name
        /// </summary>
        public bool HasPowerup(string powerupName)
        {
            foreach (string name in this.powerups)
                if (name.ToLower().Equals(powerupName.ToLower()))
                    return true;
            return false;
        }



        //////////Properties//////////

        /// <summary>
        /// The current state of the pirate
        /// </summary>
        public PirateState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// The current location of the pirate
        /// </summary>
        public Location Location
        {
            get { return this.location; }
        }

        /// <summary>
        /// The time in turns until the pirate is sober, or 0 if it is already sober
        /// </summary>
        public int TurnToSober
        {
            get { return this.turnsToSober; }
        }

        /// <summary>
        /// The time in turns until the pirate is alive, or 0 if it is already alive
        /// </summary>
        public int TurnsToRevive
        {
            get { return this.turnsToRevive; }
        }

        /// <summary>
        /// The time in turns until the pirate can attack again, or 0 if it can already attack
        /// </summary>
        public int TurnsToAttackReload
        {
            get { return this.turnsToAttackReload; }
        }
        /// <summary>
        /// Can the pirate attack this turn
        /// </summary>
        public bool CanAttack
        {
            get { return this.TurnsToAttackReload == 0 && this.State == PirateState.Free; }
        }

        /// <summary>
        /// The time in turns until the pirate can defend again, or 0 if it can already defend
        /// </summary>
        public int TurnsToDefenseReload
        {
            get { return this.turnsToDefenseReload; }
        }
        /// <summary>
        /// The time in turns until the pirate will no longer be defended; 0 if it is not defended
        /// </summary>
        public int DefenseDuration
        {
            get { return this.defenseDuration; }
        }
        /// <summary>
        /// Can the pirate defend this turn
        /// </summary>
        public bool CanDefend
        {
            get
            {
                return (this.State == PirateState.Free || this.State == PirateState.CarryingTreasure) &&
                       this.TurnsToDefenseReload == 0 &&
                       this.DefenseDuration == 0;
            }
        }

        /// <summary>
        /// Can the pirate move this turn
        /// </summary>
        public bool CanMove
        {
            get
            {
                return this.State == PirateState.Free ||
                       this.State == PirateState.CarryingTreasure;
            }
        }

        /// <summary>
        /// The treasure currently being carried by the pirate, or null if is not carrying anything
        /// </summary>
        public Treasure CarriedTreasure
        {
            get { return this.carriedTreasure; }
            set
            {
                // set carriedTreasure only if it is actually being carried by this pirate
                if (value != null && value.CarryingPirate == this)
                    this.carriedTreasure = value;
            }
        }

        /// <summary>
        /// The count of powerups on the pirate
        /// </summary>
        public int PowerupCount
        {
            get { return this.powerups.Count; }
        }

        /// <summary>
        /// The current maximum speed of the pirate
        /// </summary>
        public int MaxSpeed
        {
            get { return this.maxSpeed; }
        }

        /// <summary>
        /// The current attack radius of the pirate
        /// </summary>
        public double AttackRadius
        {
            get { return this.attackRadius; }
        }

        /// <summary>
        /// Returns the value of the carried treasure, or 0 if no treasure is being carried
        /// </summary>
        public int CarriedTreasureValue
        {
            get { return this.State == PirateState.CarryingTreasure ? this.carriedTreasure.Value : 0; }
        }

        /// <summary>
        /// The native pirate object
        /// </summary>
        public Pirates.Pirate NativeObject
        {
            get { return this.nativeObject; }
        }
    }



    /// <summary>
    /// The possible owners of a pirate
    /// </summary>
    enum Owner
    {
        /// <summary>
        /// We own the pirate
        /// </summary>
        Me,
        /// <summary>
        /// The pirate is an enemy pirate
        /// </summary>
        Enemy
    }



    /// <summary>
    /// The possible states in which a pirate can be in
    /// </summary>
    enum PirateState
    {
        /// <summary>
        /// The pirate is sober and alive, and not carrying any treasures
        /// </summary>
        Free,
        /// <summary>
        /// The pirate is carrying a treasure
        /// </summary>
        CarryingTreasure,
        /// <summary>
        /// The pirate is drunk
        /// </summary>
        Drunk,
        /// <summary>
        /// The pirate is lost
        /// </summary>
        Lost,
        /// <summary>
        /// The number of items in the enum ;)
        /// WARNING: DON'T USE UNLESS YOU KNOW WHAT ARE YOU DOING!
        /// </summary>
        COUNT
    }
}
