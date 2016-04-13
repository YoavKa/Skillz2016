using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyBot.Actions;
using MyBot.API;
using MyBot.States;

namespace MyBot.States
{
    class ImpendingDoomState:State
    {
        private bool isDoomIncoming;

        public ImpendingDoomState(Game game)
        {
            this.isDoomIncoming = false;
            Update(game);
        }

        public override void Update(Game game)
        {
            this.isDoomIncoming = (game.EnemyScore + game.EnemyCarriedValue) >= game.MaxPoints;
        }

        public bool IsDoomIncoming
        {
            get { return this.isDoomIncoming; }
        }
    }
}
