using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// A command to do nothing
    /// </summary>
    class DoNothingCommand : Command
    {
        //////////Methods//////////

        /// <summary>
        /// Creates a new nothing command
        /// </summary>
        public DoNothingCommand()
            : base((Pirate) null)
        { }

        /// <summary>
        /// Returns wether this command is executable in the given Game context
        /// </summary>
        public override ActionsPackState IsExecutable(Game game)
        {
            return ActionsPackState.Stable;
        }

        /// <summary>
        /// Returns a new ResourcesPack, that contains the necessary resources for the command to run,
        ///     or null if the command cannot be executed in the current context.
        /// </summary>
        public override ResourcesPack CreateResourcesPack(Game game)
        {
            return new ResourcesPack(game);
        }

        /// <summary>
        /// Executes the command in the given game context
        /// </summary>
        public override void Execute(Pirates.IPirateGame game)
        {
            // do nothing command does nothing, ya know
        }

        public override string ToString()
        {
            return "Do nothing command ;)";
        }
    }
}
