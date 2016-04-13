using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.API;

using MyBot.Actions.Commands;
using MyBot.Actions.Resources;

using MyBot.Utilities;

namespace MyBot.Actions
{
    /// <summary>
    /// A class that can choose the best combination of ActionsPacks, based on their value
    /// </summary>
    class ActionsChooser
    {
        //////////Consts//////////

        /// <summary>
        /// The time to save for each command
        /// </summary>
        public const double TIME_PER_COMMAND = 1;

        /// <summary>
        /// The bonus value each command gets for executing by the same event in consecutive turns
        /// </summary>
        public const double EVENT_CONSISTENCY_BONUS = 0;

        /// <summary>
        /// The bonus value each command gets for executing for the same pirate in consecutive turns
        /// </summary>
        public const double PIRATE_CONSISTENCY_BONUS = 300;



        //////////Attributes//////////

        /// <summary>
        /// The list of the ActionsPacks
        /// </summary>
        private List<KeyValuePair<double, ActionsPack>> list;

        /// <summary>
        /// The heuristics manager for the ActionsPacks
        /// </summary>
        private HeuristicsManager heuristics;

        /// <summary>
        /// The game which will execute the ActionsPack
        /// </summary>
        private Game game;

        /// <summary>
        /// The minimum time, after which the chooser will choose a working combination and return
        /// </summary>
        private int exitTime;

        /// <summary>
        /// The event that used the pirate last turn
        /// </summary>
        private int[] executingEvents;

        /// <summary>
        /// The number of turns since the last turn that the pirate was active in
        /// </summary>
        private int[] turnsSinceActive;



        //////////Methods//////////

        /// <summary>
        /// Creates a new ActionsChooser
        /// </summary>
        public ActionsChooser(Game game)
        {
            this.list = new List<KeyValuePair<double, ActionsPack>>();

            this.game = game;

            this.exitTime = 20 + (int)(game.MaxCommandsPerTurn * TIME_PER_COMMAND);

            this.executingEvents = new int[game.GetAllMyPiratesCount()];
            for (int i = 0; i < this.executingEvents.Length; i++)
                this.executingEvents[i] = -1;
            this.turnsSinceActive = new int[game.GetAllMyPiratesCount()];
            for (int i = 0; i < this.turnsSinceActive.Length; i++)
                this.turnsSinceActive[i] = -1;
        }

        /// <summary>
        /// Adds the actions pack to the chooser
        /// </summary>
        /// <returns>wether the addition was successful</returns>
        public bool AddActionsPack(ActionsPack pack, double value)
        {
            if (pack == null || pack.IsEmpty || value <= 0)
                return false;

            // give the pack the consistency bonuses
            foreach (Command cmd in pack.GetCommands())
            {
                foreach (Pirate pirate in cmd.GetMyPiratesUsed())
                {
                    if (pirate != null && this.executingEvents[pirate.Id] == cmd.SourceEvent)
                        value += EVENT_CONSISTENCY_BONUS;
                    if (pirate != null && this.turnsSinceActive[pirate.Id] != -1 && this.game.GetAllMyPiratesCount() > this.game.ActionsPerTurn)
                        value += PIRATE_CONSISTENCY_BONUS / this.turnsSinceActive[pirate.Id];
                }
            }

            this.list.Add(new KeyValuePair<double, ActionsPack>(value, pack));
            return true;
        }


        /// <summary>
        /// Clears the ActionsChooser
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
        }


        /// <summary>
        /// Returns the best ActionsPack the can be fabricated from the contained ActionsPacks
        /// </summary>
        public ActionsPack ChooseBestStableCombination()
        {
            this.game.Log(LogType.ActionsChoosing, LogImportance.Important, "Packs' count: {0}", this.list.Count);
            // sort the list, by descending order
            this.list.Sort(new ReverseKeyValueComparer<double, ActionsPack>());

            // initialize the heuristics
            this.game.Log(LogType.Timing, LogImportance.SomewhatImportant, "start heuristics calculation: {0}", this.game.GetTimeRemaining());
            this.heuristics = new HeuristicsManager(this.list, this.game);
            this.game.Log(LogType.Timing, LogImportance.SomewhatImportant, "end heuristics calculation: {0}", this.game.GetTimeRemaining());

            // will be used to hold the best option
            ActionsPack bestOption = new ActionsPack(this.game);

            // start up the recursion
            this.chooseBestStableCombination(new ActionsPack(this.game),    // no packs until now; no need to track this pack, as it will be cleared out anyways
                                             0,                             // 0 value until now
                                             bestOption,                    // the best option until now
                                             0,                             //  and its value 
                                             0,                             // choose from the start
                                             this.game.MaxCommandsPerTurn); // the maximum count of ActionsPacks that can be merged

            // update the consistency bonus
            // reset executing events
            for (int i = 0; i < this.executingEvents.Length; i++)
                this.executingEvents[i] = -1;
            // reset turns since active
            for (int i = 0; i < this.turnsSinceActive.Length; i++)
            {
                Pirate curPirate = game.GetMyPirate(i);
                if (curPirate.State == PirateState.Drunk || curPirate.State == PirateState.Lost || curPirate.Location.Equals(curPirate.InitialLocation))
                    this.turnsSinceActive[i] = -1;
                else if (this.turnsSinceActive[i] != -1)
                    this.turnsSinceActive[i] += 1;
            }
            // update the arrays
            foreach (Command cmd in bestOption.GetCommands())
            {
                foreach (Pirate pirate in cmd.GetMyPiratesUsed())
                {
                    if (pirate != null)
                    {
                        this.executingEvents[pirate.Id] = cmd.SourceEvent;
                        this.turnsSinceActive[pirate.Id] = 1;
                    }
                }
            }

            // return the best option
            return bestOption;
        }

        /// <summary>
        /// Returns the best value of a stable combination of ActionsPacks, and changes currentBestPacks accordingly
        /// </summary>
        /// <param name="chosenPacksUntilNow">The packs that were chosen before the ones we choose now</param>
        /// <param name="valueUntilNow">The value of the packs that were chosen before</param>
        /// <param name="currentBestPacks">The packs combination which is currently the best</param>
        /// <param name="currentBestValue">The value of currentBestPacks</param>
        /// <param name="startAt">The first index in the list to start choose from</param>
        /// <param name="maxDepth">The count of ActionsPacks that can be merged</param>
        /// <returns>The best value of a stable combination of ActionsPacks, until now</returns>
        private double chooseBestStableCombination(ActionsPack chosenPacksUntilNow, double valueUntilNow, ActionsPack currentBestPacks, double currentBestValue, int startAt, int maxDepth)
        {
            // if we can choose no more OR we're out of time
            if (startAt >= this.list.Count || maxDepth <= 0 || this.game.GetTimeRemaining() <= this.exitTime)
            {
                // if this pack is stable and better than the best one
                if (valueUntilNow > currentBestValue && chosenPacksUntilNow.GetState() == ActionsPackState.Stable)
                {
                    // change the maximum
                    currentBestPacks.Clear(this.game);
                    currentBestPacks.AddActions(chosenPacksUntilNow);
                    //this.game.Log("New best value {0}, at {1}", valueUntilNow, this.game.GetTimeRemaining());
                    return valueUntilNow;
                }
                else
                {
                    // change nothing
                    return currentBestValue;
                }
            }
            // if we cannot possibly choose better combination
            else if (valueUntilNow + this.list[startAt].Key * maxDepth <= currentBestValue)
            {
                // change nothing
                return currentBestValue;
            }

            int nextIndex;
            double newValue;

            // try to add the current pack to the previous ones
            if (chosenPacksUntilNow.AddActions(this.list[startAt].Value))
            {
                // if the addition was successful, than check for a better combination including this pack
                nextIndex = this.heuristics.NextPossiblePackIndex(startAt, chosenPacksUntilNow);
                newValue = chooseBestStableCombination(chosenPacksUntilNow, valueUntilNow + this.list[startAt].Key, currentBestPacks, currentBestValue, nextIndex, maxDepth - 1);
                if (newValue > currentBestValue)
                    currentBestValue = newValue;
                chosenPacksUntilNow.RemoveActions(this.list[startAt].Value);
            }

            // check for a better combination excluding this pack
            nextIndex = this.heuristics.NextPossiblePackIndex(startAt, chosenPacksUntilNow);
            newValue = chooseBestStableCombination(chosenPacksUntilNow, valueUntilNow, currentBestPacks, currentBestValue, nextIndex, maxDepth);
            if (newValue > currentBestValue)
                currentBestValue = newValue;

            // return the best value we could achieve
            return currentBestValue;
        }



        //////////Properties//////////

        /// <summary>
        /// Returns the number of ActionsPacks in the chooser
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }
    }
}
