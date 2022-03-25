using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidChangeLeaderHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;

        public RaidChangeLeaderHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.RAID_CHANGE_LEADER)]
        public void Handle(WorldClient client, RaidChangeLeaderPacket packet)
        {
            if (!_partyManager.IsPartyLead || _partyManager.Party is not Raid)
                return;

            if (!_gameWorld.Players.TryGetValue(packet.CharacterId, out var newRaidLeader))
                return;

            if (newRaidLeader.PartyManager.Party != _partyManager.Party)
                return;

            _partyManager.Party.Leader = newRaidLeader;
        }
    }
}
