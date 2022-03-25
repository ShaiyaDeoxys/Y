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
    public class RaidKickHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;

        public RaidKickHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.RAID_KICK)]
        public void Handle(WorldClient client, RaidKickPacket packet)
        {
            if (!_partyManager.IsPartyLead || _partyManager.Party is not Raid)
                return;

            if (!_gameWorld.Players.TryGetValue(packet.CharacterId, out var kickMember))
                return;

            if (kickMember.PartyManager.Party != _partyManager.Party)
                return;

            _partyManager.Party.KickMember(kickMember);
            kickMember.PartyManager.Party = null;
        }
    }
}
