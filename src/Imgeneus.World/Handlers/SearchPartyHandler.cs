using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
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
    public class SearchPartyHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IPartyManager _partyManager;
        private readonly IMapProvider _mapProvider;
        private readonly ICountryProvider _countryProvider;

        public SearchPartyHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IPartyManager partyManager, IMapProvider mapProvider, ICountryProvider countryProvider) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _partyManager = partyManager;
            _mapProvider = mapProvider;
            _countryProvider = countryProvider;
        }

        [HandlerAction(PacketType.PARTY_SEARCH_REGISTRATION)]
        public void Handle(WorldClient client, PartySearchRegistrationPacket packet)
        {
            if (_partyManager.HasParty)
                return;

            var player = _gameWorld.Players[_gameSession.Character.Id];
            _mapProvider.Map.RegisterSearchForParty(player);

            _packetFactory.SendRegisteredInPartySearch(client, true);

            var searchers = _mapProvider.Map.PartySearchers.Where(s => s.CountryProvider.Country == _countryProvider.Country && s != player);
            if (searchers.Any())
                _packetFactory.SendPartySearchList(client, searchers.Take(30));
        }
    }
}
