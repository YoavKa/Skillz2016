using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Resources;

namespace MyBot.Actions.Commands
{
    /// <summary>
    /// An abstract class of a command that can be executed by the API
    /// </summary>
    abstract class Command
    {
        //////////Consts//////////

        /// <summary>
        /// The id of the event that created this command
        /// </summary>
        public readonly int SourceEvent;



        //////////Attributes//////////

        /// <summary>
        /// The additional information about the command
        /// </summary>
        private List<string> additionalInfo;

        /// <summary>
        /// My pirates that are used by the command
        /// </summary>
        private List<Pirate> myPiratesUsed;



        //////////Methods//////////

        /// <summary>
        /// Creates a new command
        /// </summary>
        public Command(List<Pirate> myPiratesUsed)
        {
            this.SourceEvent = ActionsPack.CurrentEventId;
            this.additionalInfo = new List<string>();
            this.myPiratesUsed = new List<Pirate>(myPiratesUsed);
        }
        /// <summary>
        /// Creates a new command
        /// </summary>
        public Command(Pirate myPirateUsed)
        {
            this.SourceEvent = ActionsPack.CurrentEventId;
            this.additionalInfo = new List<string>();
            this.myPiratesUsed = new List<Pirate>();
            this.myPiratesUsed.Add(myPirateUsed);
        }

        /// <summary>
        /// Is the command executable in the given Game context:
        ///     Stable = completely executable,
        ///     Unstable = conditionally executable,
        ///     NotPossible = not executable.
        /// </summary>
        public abstract ActionsPackState IsExecutable(Game game);

        /// <summary>
        /// Returns a new ResourcesPack, that contains the necessary resources for the command to run,
        ///     or null if the command cannot be executed in the current context.
        /// </summary>
        public abstract ResourcesPack CreateResourcesPack(Game game);

        /// <summary>
        /// Executes the command in the given game context
        /// </summary>
        public abstract void Execute(Pirates.IPirateGame game);

        /// <summary>
        /// Returns wether the presence of the given ResourcesPack stabilizes this command.
        /// Should be overrided in possibly-unstable commands.
        /// </summary>
        public virtual bool IsStabilized(ResourcesPack resourcesPack)
        {
            return false; // won't stabilize ever
        }

        /// <summary>
        /// Adds additional information about the command
        /// </summary>
        public void AddInformation(string info)
        {
            if (string.IsNullOrWhiteSpace(info))
                return;

            this.additionalInfo.Add(info);
        }

        /// <summary>
        /// Returns the additional information about the command
        /// </summary>
        public List<string> GetAdditionalInformation()
        {
            return new List<string>(this.additionalInfo);
        }

        /// <summary>
        /// Returns a new list, containing my pirates that were used by the command
        /// </summary>
        public List<Pirate> GetMyPiratesUsed()
        {
            return new List<Pirate>(this.myPiratesUsed);
        }
    }
}
