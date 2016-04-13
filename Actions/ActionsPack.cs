using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Utilities;

using MyBot.API;

using MyBot.Actions.Commands;
using MyBot.Actions.Resources;

namespace MyBot.Actions
{
    /// <summary>
    /// A pack of actions, to be executed by the API
    /// </summary>
    class ActionsPack
    {
        //////////Attributes//////////

        /// <summary>
        /// The resources used by this ActionsPack
        /// </summary>
        private ResourcesPack resourcesPack;

        /// <summary>
        /// The stable commands packed in this ActionsPack
        /// </summary>
        private List<Command> stableCommands;
        /// <summary>
        /// The unstable commands packed in this ActionsPack
        /// </summary>
        private List<Command> unstableCommands;

        /// <summary>
        /// The game which will execute this ActionsPack
        /// </summary>
        private Game game;



        //////////Methods//////////

        /// <summary>
        /// Creates a new ActionsPack, which will be executed by the given Game
        /// </summary>
        public ActionsPack(Game game)
        {
            this.resourcesPack = new ResourcesPack(game);
            this.stableCommands = new List<Command>();
            this.unstableCommands = new List<Command>();

            this.game = game;
        }
        /// <summary>
        /// Creates a new ActionsPack, which will be based on the ActionsPack given
        /// </summary>
        public ActionsPack(ActionsPack other)
        {
            this.resourcesPack = new ResourcesPack(other.resourcesPack);
            this.stableCommands = new List<Command>(other.stableCommands);
            this.unstableCommands = new List<Command>(other.unstableCommands);

            this.game = other.game;
        }

        /// <summary>
        /// Clears the data in this ActionsPack
        /// </summary>
        public void Clear(Game game)
        {
            this.resourcesPack.Clear(game);
            this.stableCommands.Clear();
            this.unstableCommands.Clear();

            this.game = game;
        }


        /// <summary>
        /// Tries to add my pirate to this ActionsPack
        /// </summary>
        /// <returns>Was the addition successful (and the pirate is my pirate, and is not already included)</returns>
        public bool AddMyPirate(Pirate p)
        {
            return this.resourcesPack.AddMyPirate(p);
        }
        /// <summary>
        /// Tries to add the enemy pirate to this ActionsPack
        /// </summary>
        /// <returns>Was the addition successful (and the pirate is an enemy pirate, and is not already included)</returns>
        public bool AddEnemyPirate(Pirate p)
        {
            return this.resourcesPack.AddEnemyPirate(p);
        }
        /// <summary>
        /// Tries to add the treasure to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddTreasure(Treasure t)
        {
            return this.resourcesPack.AddTreasure(t);
        }
        /// <summary>
        /// Tries to add the powerup to the ResourcesPack
        /// </summary>
        /// <returns>Was the addition successful</returns>
        public bool AddPowerup(Powerup pu)
        {
            return this.resourcesPack.AddPowerup(pu);
        }
        /// <summary>
        /// Tries to add this ActionsPack to the given group, and returns wether the addition succeeded.
        /// NOTE: if two ActionsPack are in the same group, than they cannot be executed together.
        /// </summary>
        public bool AddToGroup(int groupId)
        {
            return this.resourcesPack.AddImaginaryResource(groupId);
        }
         /// <summary>
        /// Tries to add the object to the resources used by the ActionsPack, and returns whether the addition succeeded.
        /// NOTE: if two ActionsPack have overlapping object resources, than they cannot be executed together.
        /// WARNING: USE OF THIS METHOD IS NOT RECOMMENDED
        /// </summary>
        public bool AddObjectResource(object obj)
        {
            return this.resourcesPack.AddObjectResource(obj);
        }

        /// <summary>
        /// Removes the actions and resources of the other ActionsPack from this one
        /// </summary>
        public void RemoveActions(ActionsPack other)
        {
            if (other == null)
                return;

            this.resourcesPack.RemoveResources(other.resourcesPack);

            this.stableCommands = this.stableCommands.Except(other.stableCommands).ToList();
            this.unstableCommands = this.unstableCommands.Except(other.unstableCommands).ToList();
        }

        /// <summary>
        /// Tries to add the actions of another ActionsPack into this one, and returns wether the addition succeeded.
        /// NOTE: The other ActionsPack must have the same "game-context" as this one!
        /// </summary>
        public bool AddActions(ActionsPack other)
        {
            // cannot execute a null object
            if (other == null)
                return false;

            // if the pack is not given in the same context
            if (other.game != this.game)
                return false;

            // tries to add other.resourcesPack to the resources, but continue only if the addition succeeded (the two ResourcesPack do not overlap)
            if (!this.resourcesPack.AddResources(other.resourcesPack))
                return false;

            // add the commands
            this.stableCommands.AddRange(other.stableCommands);
            this.unstableCommands.AddRange(other.unstableCommands);

            return true;
        }

        /// <summary>
        /// Tries to add the given command to this ActionsPack, and returns wether the addition succeeded
        /// </summary>
        public bool AddCommand(Command cmd)
        {
            // cannot execute a null object
            if (cmd == null)
                return false;

            ActionsPackState cmdState = cmd.IsExecutable(this.game);
            // if the command is not executable in the current context
            if (cmdState == ActionsPackState.NotPossible)
                return false;

            ResourcesPack rp = cmd.CreateResourcesPack(this.game);
            // fatal error
            if (rp == null)
                return false;
            
            // tries to add rp to the resources, but continue only if the addition succeeded (the two ResourcesPack do not overlap)
            if (!this.resourcesPack.AddResources(rp))
                return false;

            // add the command
            // if the command is stable
            if (cmdState == ActionsPackState.Stable)
                this.stableCommands.Add(cmd);
            // else, the command is unstable
            else
                this.unstableCommands.Add(cmd);

            return true;
        }


        /// <summary>
        /// Returns a new list, containing all the commands contained in this ActionsPack
        /// </summary>
        public List<Command> GetCommands()
        {
            return this.stableCommands.Union(this.unstableCommands).ToList();
        }


        /// <summary>
        /// Returns the state of this ActionsPack
        /// </summary>
        public ActionsPackState GetState()
        {
            // unstable iff there are unstable commands
            foreach (Command cmd in this.unstableCommands)
                if (!cmd.IsStabilized(this.resourcesPack))
                    return ActionsPackState.Unstable;
            return ActionsPackState.Stable;
        }


        /// <summary>
        /// Burns the given string onto the commands contained in this ActionsPack, for use when executing them
        /// </summary>
        public void BurnInformation(string info)
        {
            foreach (Command cmd in this.stableCommands.Union(this.unstableCommands))
                cmd.AddInformation(info);
        }
        /// <summary>
        /// Burns the given object's ToString() onto the commands contained in this ActionsPack, for use when executing them
        /// </summary>
        public void BurnInformation(object obj)
        {
            this.BurnInformation(obj.ToString());
        }
        /// <summary>
        /// Burns the given formatted string onto the commands contained in this ActionsPack, for use when executing them
        /// </summary>
        public void BurnInformation(string format, params object[] info)
        {
            this.BurnInformation(string.Format(format, info));
        }




        //////////Properties//////////

        /// <summary>
        /// Returns the number of actions used by this ActionsPack
        /// </summary>
        public int ActionsUsed
        {
            get { return this.resourcesPack.ActionsUsed; }
        }

        /// <summary>
        /// My pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        public uint MyPiratesHash
        {
            get { return this.resourcesPack.MyPiratesHash; }
        }

        /// <summary>
        /// The enemy pirates that are being used by the ActionsPack; if the i-th bit is on, than the pirate with id i is being used
        /// </summary>
        public uint EnemyPiratesHash
        {
            get { return this.resourcesPack.EnemyPiratesHash; }
        }

        /// <summary>
        /// The treasures that are being used by the ActionsPack; if the i-th bit is on, than the treasure with id i is being used
        /// </summary>
        public uint TreasuresHash
        {
            get { return this.resourcesPack.TreasuresHash; }
        }

        /// <summary>
        /// Wether this ActionsPack is empty or not
        /// </summary>
        public bool IsEmpty
        {
            get { return this.stableCommands.Count == 0 && this.unstableCommands.Count == 0; }
        }





        //////////Static Attributes//////////

        /// <summary>
        /// The current executing event id
        /// </summary>
        public static int CurrentEventId = -1;



        //////////Static Methods//////////

        /// <summary>
        /// Returns a new ActionsPack, containing the command given, or null if one couldn't be fabricated correctly
        /// </summary>
        public static ActionsPack NewCommandPack(Game game, Command cmd, int groupId)
        {
            ActionsPack ap = ActionsPack.NewCommandPack(game, cmd);
            if (ap == null)
                return null;
            if (!ap.AddToGroup(groupId))
                return null;
            return ap;
        }
        /// <summary>
        /// Returns a new ActionsPack, containing the command given, or null if one couldn't be fabricated correctly
        /// </summary>
        public static ActionsPack NewCommandPack(Game game, Command cmd)
        {
            ActionsPack ap = new ActionsPack(game);
            if (!ap.AddCommand(cmd))
                return null;
            return ap;
        }
    }



    /// <summary>
    /// The states in which an ActionsPack can be
    /// </summary>
    enum ActionsPackState
    {
        /// <summary>
        /// The ActionsPack is ready to be executed
        /// </summary>
        Stable,
        /// <summary>
        /// The ActionsPack CANNOT be executed as is, but can be potentially executed,
        ///     provided other ActionsPacks
        /// </summary>
        Unstable,
        /// <summary>
        /// The ActionsPack cannot be executed AT ALL
        /// </summary>
        NotPossible
    }
}