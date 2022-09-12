using Imgeneus.Database.Preload;
using Imgeneus.Game.Monster;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Zone
{
    public class MapFactory : IMapFactory
    {
        private readonly ILogger<Map> _logger;
        private readonly IGamePacketFactory _packetFactory;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IMobFactory _mobFactory;
        private readonly INpcFactory _npcFactory;
        private readonly IObeliskFactory _obeliskFactory;
        private readonly ITimeService _timeService;
        private readonly IGuildRankingManager _guildRankingManager;

        public MapFactory(ILogger<Map> logger, IGamePacketFactory packetFactory, IGameDefinitionsPreloder definitionsPreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService, IGuildRankingManager guildRankingManger)
        {
            _logger = logger;
            _packetFactory = packetFactory;
            _definitionsPreloader = definitionsPreloader;
            _mobFactory = mobFactory;
            _npcFactory = npcFactory;
            _obeliskFactory = obeliskFactory;
            _timeService = timeService;
            _guildRankingManager = guildRankingManger;
        }

        /// <inheritdoc/>
        public IMap CreateMap(ushort id, MapDefinition definition, Svmap config, IGameWorld gameWorld, IEnumerable<ObeliskConfiguration> obelisks = null, IEnumerable<BossConfiguration> bosses = null)
        {
            if (obelisks is null)
                obelisks = new List<ObeliskConfiguration>();

            return new Map(id, definition, config, obelisks, bosses, _logger, _packetFactory, _definitionsPreloader, _mobFactory, _npcFactory, _obeliskFactory, _timeService) { GameWorld = gameWorld };
        }

        /// <inheritdoc/>
        public IPartyMap CreatePartyMap(ushort id, MapDefinition definition, Svmap config, IGameWorld gameWorld, IParty party, IEnumerable<BossConfiguration> bosses = null)
        {
            return new PartyMap(party, id, definition, config, bosses, _logger, _packetFactory, _definitionsPreloader, _mobFactory, _npcFactory, _obeliskFactory, _timeService) { GameWorld = gameWorld };
        }

        /// <inheritdoc/>
        public IGuildMap CreateGuildMap(ushort id, MapDefinition definition, Svmap config, IGameWorld gameWorld, uint guildId, IEnumerable<BossConfiguration> bosses = null)
        {
            if (definition.CreateType == CreateType.GRB)
                return new GRBMap(guildId, _guildRankingManager, id, definition, config, bosses, _logger, _packetFactory, _definitionsPreloader, _mobFactory, _npcFactory, _obeliskFactory, _timeService) { GameWorld = gameWorld };

            return new GuildHouseMap(guildId, _guildRankingManager, id, definition, config, _logger, _packetFactory, _definitionsPreloader, _mobFactory, _npcFactory, _obeliskFactory, _timeService) { GameWorld = gameWorld };
        }
    }
}
