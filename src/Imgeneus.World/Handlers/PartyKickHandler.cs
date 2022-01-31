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
    public class PartyKickHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public PartyKickHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.PARTY_KICK)]
        public void Handle(WorldClient client, PartyKickPacket packet)
        {
            if (!_partyManager.IsPartyLead)
                return;

            var playerToKick = _partyManager.Party.Members.FirstOrDefault(m => m.Id == packet.CharacterId);
            if (playerToKick != null)
            {
                _partyManager.Party.KickMember(playerToKick);
                playerToKick.PartyManager.Party = null;
            }
        }
    }
}
