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
    public class RaidResponseHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;

        public RaidResponseHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.RAID_RESPONSE)]
        public void Handle(WorldClient client, RaidResponsePacket packet)
        {
            _gameWorld.Players.TryGetValue(_partyManager.InviterId, out var partyRequester);
            _partyManager.InviterId = 0;

            if (partyRequester is null)
                return;

            if (packet.IsDeclined)
            {
                _packetFactory.SendDeclineRaid(partyRequester.GameSession.Client, _gameSession.Character.Id);
                return;
            }

            _partyManager.Party = partyRequester.PartyManager.Party;
            _packetFactory.SendRaidInfo(client, _partyManager.Party as Raid);
        }
    }
}
