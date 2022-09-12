
using Imgeneus.Database.Preload;
using Imgeneus.Game.Monster;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Zone
{
    public class GuildHouseMap : GuildMap
    {
        public GuildHouseMap(uint guildId, IGuildRankingManager guildRankingManager, ushort id, MapDefinition definition, Svmap config, ILogger<Map> logger, IGamePacketFactory packetFactory, IGameDefinitionsPreloder definitionsPreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
            : base(guildId, guildRankingManager, id, definition, config, new List<BossConfiguration>(), logger, packetFactory, definitionsPreloader, mobFactory, npcFactory, obeliskFactory, timeService)
        {

        }
    }
}
