using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API
{
    /// <summary>
    /// A speed powerup
    /// </summary>
    class SpeedPowerup : Powerup
    {
        //////////Readonly's//////////

        /// <summary>
        /// The new speed for carrying pirates, given by the powerup
        /// </summary>
        public readonly int CarryTreasureSpeed;



        //////////Methods//////////

        /// <summary>
        /// Creates a new SpeedPowerup
        /// </summary>
        public SpeedPowerup(Pirates.SpeedPowerup powerup, int currentTurn)
            : base(powerup, currentTurn)
        {
            this.CarryTreasureSpeed = powerup.CarryTreasureSpeed;
        }



        //////////Properties//////////

        /// <summary>
        /// The native powerup object
        /// </summary>
        public Pirates.AttackPowerup NativeObject
        {
            get { return base.nativeObject as Pirates.AttackPowerup; }
        }
        
        
        
        //////////Static Consts//////////

        /// <summary>
        /// The name of the powerup
        /// </summary>
        public const string NAME = "speed";
    }
}