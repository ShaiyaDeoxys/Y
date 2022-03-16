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

        /// <summary>
        /// Send guild npcs as soon as character is selected or joined guild.
        /// </summary>
        private void SendGuildNpcLvlList()
        {
            //if (!HasGuild)
            //    return;

            //_packetsHelper.SendGuildNpcs(Client, GuildManager.GetGuildNpcs((int)GuildId));
        }
    }
}
