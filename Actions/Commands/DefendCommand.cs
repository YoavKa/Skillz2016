using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;
using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// A command for a pirate to defend itself
    /// </summary>
    class DefendCommand : Command
    {
        //////////Readonly's//////////

        /// <summary>
        /// The pirate defending itself
        /// </summary>
        public readonly Pirate Pirate;



        //////////Methods//////////

        /// <summary>
        /// Creates a new defend command
        /// </summary>
        public DefendCommand(Pirate pirate)
            : base(pirate)
        {
            this.Pirate = pirate;
        }

        /// <summary>
        /// Returns wether this command is executable in the given Game context
        /// </summary>
        public override ActionsPackState IsExecutable(Game game)
        {
            // if the pirate is null or not mine
            if (this.Pirate == null ||
                this.Pirate.Owner != Owner.Me)
                return ActionsPackState.NotPossible;

            // else, use the built-in check method for an attack
            if (this.Pirate.CanDefend)
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

            bool success = rp.AddMyPirate(this.Pirate);
            success &= rp.AddTakenLocation(this.Pirate);

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
            game.Defend(this.Pirate.NativeObject);
        }

        public override string ToString()
        {
            return "Pirate " + this.Pirate.Id + " defends itself";
        }
    }
}
