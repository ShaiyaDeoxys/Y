using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class PartySummonHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;
        private readonly ITeleportationManager _teleportationManager;

        public PartySummonHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld, ITeleportationManager teleportationManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
            _teleportationManager = teleportationManager;
        }

        [HandlerAction(PacketType.PARTY_CALL_ANSWER)]
        public void Handle(WorldClient client, PartySummonAnswerPacket packet)
        {
            if (!_partyManager.HasParty || _partyManager.Party.SummonRequest is null)
                return;

            var summoner = _gameWorld.Players[_partyManager.Party.SummonRequest.OwnerId];
            if (summoner is null)
                return;

            _partyManager.SetSummonAnswer(!packet.IsDeclined);

            if (!packet.IsDeclined)
                _teleportationManager.Teleport(summoner.Map.Id, summoner.PosX, summoner.PosY, summoner.PosZ);

            _packetFactory.SendSummonAnswer(summoner.GameSession.Client, _gameSession.Character.Id, packet.IsDeclined);
        }
    }
}
