using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Country;
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
        private readonly ICountryProvider _countryProvider;

        public PartySearchInviteHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, ICountryProvider countryProvider) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _countryProvider = countryProvider;
        }

        [HandlerAction(PacketType.PARTY_SEARCH_INVITE)]
        public void Handle(WorldClient client, PartySearchInvitePacket packet)
        {
            var requestedPlayer = _mapProvider.Map.PartySearchers.FirstOrDefault(p => p.CountryProvider.Country == _countryProvider.Country && p.AdditionalInfoManager.Name == packet.Name);

            if (requestedPlayer is null)
                requestedPlayer = _mapProvider.Map.Players.Values.FirstOrDefault(p => p.CountryProvider.Country == _countryProvider.Country && p.AdditionalInfoManager.Name == packet.Name);

            if (requestedPlayer != null && !requestedPlayer.PartyManager.HasParty)
            {
                requestedPlayer.PartyManager.InviterId = _gameSession.Character.Id;
                _packetFactory.SendPartyRequest(requestedPlayer.GameSession.Client, _gameSession.Character.Id);
            }
        }
    }
}
