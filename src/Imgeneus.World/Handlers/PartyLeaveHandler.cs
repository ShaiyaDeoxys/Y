using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class PartyLeaveHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public PartyLeaveHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.PARTY_LEAVE)]
        public void Handle(WorldClient client, PartyLeavePacket packet)
        {
            _partyManager.Party = null;
        }
    }
}
