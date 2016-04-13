using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API.General
{
    /// <summary>
    /// The current turn, and it's constants
    /// </summary>
    class Turn : IUpdateable<IPirateGame>
    {
        //////////Attributes//////////

        /// <summary>
        /// The native IPirateGame object
        /// </summary>
        private IPirateGame game;

        /// <summary>
        /// Data about this turn, availible through properties
        /// </summary>
        private int turnNumber, myScore, enemyScore;

        /// <summary>
        /// The total time per turn
        /// </summary>
        private int totalTime;

        /// <summary>
        /// The glass ceiling of time we are willing to use
        /// </summary>
        private const int GLASS_CEILING_TIME = 500;
        private int glassCeilingTime;
        private int glassCeilingTimeFirstTurn;



        //////////Methods//////////

        /// <summary>
        /// Creates a new Turn object
        /// </summary>
        public Turn(IPirateGame game)
        {
            this.Update(game);
        }

        /// <summary>
        /// Updates this turn's data
        /// </summary>
        public void Update(IPirateGame game)
        {
            this.game = game;
            this.totalTime = game.TimeRemaining();
            this.glassCeilingTime = Utils.Round(Utils.Min(GLASS_CEILING_TIME, this.totalTime * 5.0 / 6.0));
            this.glassCeilingTimeFirstTurn = Utils.Round(Utils.Min((this.totalTime - this.glassCeilingTime)*5.0/6.0, (this.glassCeilingTime * 9)*5.0/6.0));
            this.turnNumber = game.GetTurn();
            this.myScore = game.GetMyScore();
            this.enemyScore = game.GetEnemyScore();

            this.secondsPassed = 0;
            this.last = game.TimeRemaining();

            game.Debug("real start at " + this.GetTimeRemaining());
        }
        private int secondsPassed, last;
        /// <summary>
        /// Returns the time in miliseconds until timeout
        /// </summary>
        public int GetTimeRemaining()
        {
            int t = this.game.TimeRemaining() - this.totalTime + this.glassCeilingTime;
            if (this.turnNumber == 1)
                t += this.glassCeilingTimeFirstTurn;
            if (t > this.last)
                this.secondsPassed++;
            this.last = t;
            return t - secondsPassed * 1000;
        }



        //////////Properties//////////

        /// <summary>
        /// The current turn number
        /// </summary>
        public int TurnNumber
        {
            get { return this.turnNumber; }
        }

        /// <summary>
        /// My current score
        /// </summary>
        public int MyScore
        {
            get { return this.myScore; }
        }

        /// <summary>
        /// The enemy's current score
        /// </summary>
        public int EnemyScore
        {
            get { return this.enemyScore; }
        }
    }
}
