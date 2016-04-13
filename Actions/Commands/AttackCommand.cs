using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// A command for a pirate to attack another one
    /// </summary>
    class AttackCommand : Command
    {
        //////////Readonly's//////////

        /// <summary>
        /// The pirate attacking
        /// </summary>
        public readonly Pirate AttackingPirate;
        /// <summary>
        /// The pirate being attacked
        /// </summary>
        public readonly Pirate AttackedPirate;



        //////////Methods//////////

        /// <summary>
        /// Creates a new attack command
        /// </summary>
        public AttackCommand(Pirate attackingPirate, Pirate attackedPirate)
            : base(attackingPirate)
        {
            this.AttackingPirate = attackingPirate;
            this.AttackedPirate = attackedPirate;
        }

        /// <summary>
        /// Returns wether this command is executable in the given Game context
        /// </summary>
        public override ActionsPackState IsExecutable(Game game)
        {
            // if the pirates are null, or the attacking pirate is not mine
            if (this.AttackingPirate == null ||
                this.AttackedPirate == null ||
                this.AttackingPirate.Owner != Owner.Me)
                return ActionsPackState.NotPossible;

            // else, use the built-in check method for an attack
            if (game.IsAttackPossible(this.AttackingPirate, this.AttackedPirate))
                return ActionsPackState.Stable;
            else
                return ActionsPackState.NotPossible;
        }

        /// <summary>
        /// Returns a new ResourcesPack, that contains the necessary resources for the command to run,
        ///     or null if the command cannot be executed in the current context.
        /// </summary>
        public override ResourcesPack CreateResourcesPack(Game game)
        {
            ResourcesPack rp = new ResourcesPack(game);

            bool success = rp.AddMyPirate(this.AttackingPirate);
            success &= rp.AddTakenLocation(this.AttackingPirate);
            success &= rp.AddEnemyPirate(this.AttackedPirate);

            // if the addition of the resources was not successful
            if (!success)
                return null;
            // else, rp is ready!
            return rp;
        }

        /// <summary>
        /// Executes the command in the given game context
        /// </summary>
        public override void Execute(Pirates.IPirateGame game)
        {
            game.Attack(this.AttackingPirate.NativeObject, this.AttackedPirate.NativeObject);
        }

        public override string ToString()
        {
            return "Pirate " + this.AttackingPirate.Id + " attacks pirate " + this.AttackedPirate.Id;
        }
    }
}
