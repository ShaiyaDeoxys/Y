using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidMovePlayerHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public RaidMovePlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.RAID_MOVE_PLAYER)]
        public void Handle(WorldClient client, RaidMovePlayerPacket packet)
        {
            if (_partyManager.Party is not Raid || (!_partyManager.IsPartyLead && !_partyManager.IsPartySubLeader))
                return;

            (_partyManager.Party as Raid).MoveCharacter(packet.SourceIndex, packet.DestinationIndex);
        }
    }
}
