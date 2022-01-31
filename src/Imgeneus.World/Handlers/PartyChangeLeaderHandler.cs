using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class PartyChangeLeaderHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public PartyChangeLeaderHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.PARTY_CHANGE_LEADER)]
        public void Handle(WorldClient client, PartyChangeLeaderPacket packet)
        {
            if (!_partyManager.IsPartyLead)
                return;

            var newLeader = _partyManager.Party.Members.FirstOrDefault(m => m.Id == packet.CharacterId);
            if (newLeader != null)
                _partyManager.Party.Leader = newLeader;
        }
    }
}
