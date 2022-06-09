using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidJoinHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;
        private readonly ICountryProvider _countryProvider;

        public RaidJoinHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld, ICountryProvider countryProvider) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
            _countryProvider = countryProvider;
        }

        [HandlerAction(PacketType.RAID_JOIN)]
        public void Handle(WorldClient client, RaidJoinPacket packet)
        {
            if (_partyManager.Party != null) // Player is already in party.
            {
                _packetFactory.SendPartyError(client, PartyErrorType.RaidNotFound);
                return;
            }

            var raidMember = _gameWorld.Players.Values.FirstOrDefault(m => m.AdditionalInfoManager.Name == packet.CharacterName);
            if (raidMember is null || raidMember.CountryProvider.Country != _countryProvider.Country || !(raidMember.PartyManager.Party is Raid))
                _packetFactory.SendPartyError(client, PartyErrorType.RaidNotFound);
            else
            {
                if ((raidMember.PartyManager.Party as Raid).AutoJoin)
                {
                    _partyManager.Party = raidMember.PartyManager.Party;
                    if (_partyManager.Party is null)
                        _packetFactory.SendPartyError(client, PartyErrorType.RaidNoFreePlace);
                    else
                        _packetFactory.SendRaidInfo(client, _partyManager.Party as Raid);
                }
                else
                {
                    _packetFactory.SendPartyError(client, PartyErrorType.RaidNoAutoJoin);
                }
            }
        }
    }
}
