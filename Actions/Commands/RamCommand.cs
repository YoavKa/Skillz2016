using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// A command for ramming a drunk pirate of us
    /// </summary>
    class RamCommand : SailCommand
    {
        //////////Readonly's//////////

        /// <summary>
        /// The drunk pirate being rammed
        /// </summary>
        public readonly Pirate DrunkPirate;



        //////////Methods//////////
        /// <summary>
        /// Creates a new ram command
        /// </summary>
        /// <param name="rammingPirate">The pirate that is going to ram</param>
        /// <param name="drunkPirate">The drunk pirate that is going to be rammed</param>
        /// <param name="immediateDestination">The IMMEDIATE destination of the pirate</param>
        public RamCommand(Pirate rammingPirate, Pirate drunkPirate, ILocateable immediateDestination)
            : base(rammingPirate, immediateDestination)
        {
            this.DrunkPirate = drunkPirate;
        }

        /// <summary>
        /// Returns wether this command is executable in the given Game context
        /// </summary>
        public override ActionsPackState IsExecutable(Game game)
        {
            ActionsPackState state = base.IsExecutable(game);
            Pirate pirateOnMap = game.GetPirateOn(base.ImmediateDestination);
            // if the state is unstable, and the immediate destination is the drunk pirate, then we're stable
            if (state == ActionsPackState.Unstable && pirateOnMap != null && pirateOnMap == this.DrunkPirate)
                return ActionsPackState.Stable;
            // else, return the normal state
            return state;
        }
    }
}
