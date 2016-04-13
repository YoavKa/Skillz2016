using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// A command for a pirate to sail to a location
    /// </summary>
    class SailCommand : Command
    {
        //////////Readonly's//////////

        /// <summary>
        /// The pirate moving
        /// </summary>
        public readonly Pirate Pirate;
        /// <summary>
        /// The immediate destination of the pirate
        /// </summary>
        public readonly Location ImmediateDestination;

        /// <summary>
        /// The Manhattan distance between the pirate and the immediate destination
        /// </summary>
        private readonly int distance;



        //////////Methods//////////

        /// <summary>
        /// Creates a new sail command
        /// </summary>
        /// <param name="pirate">The pirate that will sail</param>
        /// <param name="immediateDestionation">The IMMEDIATE destination of the pirate</param>
        public SailCommand(Pirate pirate, ILocateable immediateDestionation)
            : base (pirate)
        {
            this.Pirate = pirate;
            this.ImmediateDestination = immediateDestionation.GetLocation();

            this.distance = Game.ManhattanDistance(this.Pirate, this.ImmediateDestination);
        }

        /// <summary>
        /// Returns wether this command is executable in the given Game context
        /// </summary>
        public override ActionsPackState IsExecutable(Game game)
        {
            if (this.Pirate == null ||                  // if the pirate is null
                this.Pirate.Owner != Owner.Me ||        // if the pirate is not mine
                !this.Pirate.CanMove ||                 // if the pirate cannot move
                !game.InMap(this.ImmediateDestination)) // if the destination is outside of the map
                return ActionsPackState.NotPossible;

            // if the immediate destination is to far apart (for sure)
            if (this.distance > this.Pirate.MaxSpeed)
                return ActionsPackState.NotPossible;

            Pirate pirateAtDestination = game.GetPirateOn(this.ImmediateDestination);
            // if the pirate at the destination is ours, than the command is unstable
            if (pirateAtDestination != null &&
                pirateAtDestination.Owner == Owner.Me &&
                pirateAtDestination != this.Pirate)
                return ActionsPackState.Unstable;
            // else, the command is stable
            else
                return ActionsPackState.Stable;
        }

        /// <summary>
        /// Returns a new ResourcesPack, that contains the necessary resources for the command to run,
        ///     or null if the command cannot be executed in the current context.
        /// </summary>
        public override ResourcesPack CreateResourcesPack(Game game)
        {
            ResourcesPack rp = new ResourcesPack(game);

            bool success = rp.AddMyPirate(this.Pirate);
            success &= rp.AddActions(this.distance);
            success &= rp.AddFreedLocation(this.Pirate);
            success &= rp.AddTakenLocation(this.ImmediateDestination);

            // if the addition of the resources was not successful
            if (!success)
                return null;
            // else, rp is ready!
            return rp;
        }

        /// <summary>
        /// Returns wether the presence of the given ResourcesPack stabilizes this command.
        /// </summary>
        public override bool IsStabilized(ResourcesPack resourcesPack)
        {
            // will stabilize if the destination will be freed
            return resourcesPack.IsFreed(this.ImmediateDestination);
        }

        /// <summary>
        /// Executes the command in the given game context
        /// </summary>
        public override void Execute(Pirates.IPirateGame game)
        {
            // stop pirates from sailing to their own location
            if (this.Pirate.Location.Equals(this.ImmediateDestination))
                return;

            game.SetSail(this.Pirate.NativeObject, this.ImmediateDestination.NativeObject);
        }

        public override string ToString()
        {
            return "Pirate " + this.Pirate.Id + " sails to " + this.ImmediateDestination;
        }
    }
}
