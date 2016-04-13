using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API
{
    /// <summary>
    /// A powerup in the game
    /// </summary>
    class Powerup : ILocateable
    {
        //////////Readonly's//////////

        /// <summary>
        /// The id of the powerup
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// The location of the powerup
        /// </summary>
        public readonly Location Location;
        /// <summary>
        /// The number of turns the powerup will be active on a pirate
        /// </summary>
        public readonly int ActiveTurns;
        /// <summary>
        /// The type of the powerup
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// The number of turns until the powerup will disappear
        /// </summary>
        public readonly int TurnsToDisappear;


        /// <summary>
        /// The native powerup object
        /// </summary>
        protected readonly Pirates.Powerup nativeObject;



        //////////Methods//////////

        /// <summary>
        /// Creates a new powerup
        /// </summary>
        public Powerup(Pirates.Powerup powerup, int currentTurn)
        {
            this.nativeObject = powerup;

            this.Id = powerup.Id;
            this.Location = new Location(powerup.Location);
            this.ActiveTurns = powerup.ActiveTurns;
            this.Type = powerup.Type;
            this.TurnsToDisappear = powerup.EndTurn - currentTurn;
        }

        public Location GetLocation()
        {
            return this.Location;
        }



        //////////Static Methods//////////

        /// <summary>
        /// Gets the powerup object, based on the parameters given
        /// </summary>
        public static Powerup GetPowerup(Pirates.Powerup powerup, int curTurn)
        {
            if (powerup == null)
                return null;
            else if (powerup is Pirates.AttackPowerup)
                return new AttackPowerup(powerup as Pirates.AttackPowerup, curTurn);
            else if (powerup is Pirates.SpeedPowerup)
                return new SpeedPowerup(powerup as Pirates.SpeedPowerup, curTurn);
            else
                return null;
        }
    }
}
