using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Country;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Reloads guild ranks for <see cref="_guildManager"/>.
        /// </summary>
        public void ReloadGuildRanks(IEnumerable<(int guildId, int points, byte rank)> results)
        {
            GuildManager.ReloadGuildRanks(results);
        }
    }
}
