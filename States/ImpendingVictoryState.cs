using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.API;
using MyBot.States;

namespace MyBot.States
{
    class ImpendingVictoryState : State
    {
        private bool isVictoryIncoming;

        public ImpendingVictoryState(Game game)
        {
            this.isVictoryIncoming = false;
            Update(game);
        }

        public override void Update(Game game)
        {
            this.isVictoryIncoming = (game.MyScore + game.MyCarriedValue) >= game.MaxPoints;
        }

        public bool IsVictoryIncoming
        {
            get { return this.isVictoryIncoming; }
        }
    }
}
