using Imgeneus.World.Game.Buffs;
using System;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Send notification to client, when new buff added.
        /// </summary>
        protected void BuffAdded(Buff buff)
        {
            //if (Client != null)
            //    SendAddBuff(buff);
        }

        /// <summary>
        /// Send notification to client, when buff was removed.
        /// </summary>
        protected void BuffRemoved(Buff buff)
        {
            //if (Client != null)
            //    SendRemoveBuff(buff);
        }
    }
}
