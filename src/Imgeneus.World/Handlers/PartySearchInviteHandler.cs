using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class PartySearchInviteHandler : BaseHandler
    {
        private readonly IMapProvider _mapProvider;

        public PartySearchInviteHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.PARTY_SEARCH_INVITE)]
        public void Handle(WorldClient client, PartySearchInvitePacket packet)
        {
            var requestedPlayer = _mapProvider.Map.PartySearchers.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (requestedPlayer != null && !requestedPlayer.PartyManager.HasParty)
            {
                requestedPlayer.PartyManager.InviterId = _gameSession.CharId;
                _packetFactory.SendPartyRequest(requestedPlayer.GameSession.Client, _gameSession.CharId);
            }
        }
    }
}
