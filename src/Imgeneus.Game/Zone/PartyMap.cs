using Imgeneus.Database.Preload;
using Imgeneus.Game.Monster;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Svmap;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Zone
{
    /// <summary>
    /// Instance map, only for parties.
    /// </summary>
    public class PartyMap : Map, IPartyMap
    {
        private readonly IParty _party;

        /// <inheritdoc/>
        public override bool IsInstance { get => true; }

        /// <inheritdoc/>
        public Guid PartyId => _party.Id;

        /// <inheritdoc/>
        public event Action<IPartyMap> OnAllMembersLeft;

        public PartyMap(IParty party, ushort id, MapDefinition definition, Svmap config, IEnumerable<BossConfiguration> bosses, ILogger<Map> logger, IGamePacketFactory packetFactory, IGameDefinitionsPreloder definitionsPreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
            : base(id, definition, config, new List<ObeliskConfiguration>(), bosses, logger, packetFactory, definitionsPreloader, mobFactory, npcFactory, obeliskFactory, timeService)
        {
            _party = party;

            if (_party != null)
                _party.AllMembersLeft += Party_AllMembersLeft;
        }

        private void Party_AllMembersLeft()
        {
            _party.AllMembersLeft -= Party_AllMembersLeft;

            if (Players.Count == 0)
                OnAllMembersLeft?.Invoke(this);
        }

        public override bool UnloadPlayer(uint characterId, bool exitGame = false)
        {
            var result = base.UnloadPlayer(characterId, exitGame);

            if (_party is null || (_party.Members.Count <= 1 && Players.Count == 0))
            {
                OnAllMembersLeft?.Invoke(this);
            }

            return result;
        }
    }
}
