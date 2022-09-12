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
    public abstract class GuildMap : Map, IGuildMap
    {
        protected readonly uint _guildId;
        protected readonly IGuildRankingManager _guildRankingManager;

        public uint GuildId
        {
            get
            {
                return _guildId;
            }
        }

        public GuildMap(uint guildId, IGuildRankingManager guildRankingManager, ushort id, MapDefinition definition, Svmap config, IEnumerable<BossConfiguration> bosses, ILogger<Map> logger, IGamePacketFactory packetFactory, IGameDefinitionsPreloder definitionsPreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
            : base(id, definition, config, new List<ObeliskConfiguration>(), bosses, logger, packetFactory, definitionsPreloader, mobFactory, npcFactory, obeliskFactory, timeService)
        {
            _guildId = guildId;
            _guildRankingManager = guildRankingManager;
        }

    }
}
