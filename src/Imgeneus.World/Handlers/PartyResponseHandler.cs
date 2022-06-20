using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class PartyResponseHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public PartyResponseHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.PARTY_RESPONSE)]
        public void Handle(WorldClient client, PartyResponsePacket packet)
        {
            if (_gameWorld.Players.TryGetValue(packet.CharacterId, out var partyResponser))
            {
                _gameWorld.Players.TryGetValue(partyResponser.PartyManager.InviterId, out var partyRequester);
                if (partyRequester is null)
                    return;

                if (packet.IsDeclined)
                {
                    _packetFactory.SendDeclineParty(partyRequester.GameSession.Client, _gameSession.Character.Id);
                }
                else
                {
                    if (partyRequester.PartyManager.Party is null)
                    {
                        var party = new Party(_packetFactory);

                        partyRequester.PartyManager.Party = party;
                        _packetFactory.SendPartyInfo(partyRequester.GameSession.Client, party.Members.Where(m => m != partyRequester), (byte)party.Members.IndexOf(party.Leader));


                        partyResponser.PartyManager.Party = party;
                        _packetFactory.SendPartyInfo(partyResponser.GameSession.Client, party.Members.Where(m => m != partyResponser), (byte)party.Members.IndexOf(party.Leader));
                    }
                    else
                    {
                        partyResponser.PartyManager.Party = partyRequester.PartyManager.Party;
                        _packetFactory.SendPartyInfo(partyResponser.GameSession.Client, partyResponser.PartyManager.Party.Members.Where(m => m != partyRequester), (byte)partyResponser.PartyManager.Party.Members.IndexOf(partyResponser.PartyManager.Party.Leader));
                    }

                }

                partyResponser.PartyManager.InviterId = 0;
            }
        }
    }
}
