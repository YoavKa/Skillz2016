using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.API
{
    /// <summary>
    /// An attack powerup
    /// </summary>
    class AttackPowerup : Powerup
    {
        //////////Readonly's//////////

        /// <summary>
        /// The new attack radius given by the powerup
        /// </summary>
        public readonly int AttackRadius;



        //////////Methods//////////

        /// <summary>
        /// Creates a new AttackPowerup
        /// </summary>
        public AttackPowerup(Pirates.AttackPowerup powerup, int currentTurn)
            : base(powerup, currentTurn)
        {
            this.AttackRadius = powerup.AttackRadius;
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
        public const string NAME = "attack";
    }
}
