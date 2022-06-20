using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DuelRequestHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;
        private readonly IGameWorld _gameWorld;

        public DuelRequestHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.DUEL_REQUEST)]
        public void Handle(WorldClient client, DuelRequestPacket packet)
        {
            if (!_gameWorld.Players.ContainsKey(_gameSession.Character.Id) || !_gameWorld.Players.ContainsKey(packet.DuelToWhomId))
                return;

            var requester = _gameWorld.Players[_gameSession.Character.Id];
            var receiver = _gameWorld.Players[packet.DuelToWhomId];

            requester.DuelManager.OpponentId = receiver.Id;
            receiver.DuelManager.OpponentId = requester.Id;

            _packetFactory.SendWaitingDuel(requester.GameSession.Client, requester.Id, receiver.Id);
            _packetFactory.SendWaitingDuel(receiver.GameSession.Client, requester.Id, receiver.Id);
        }
    }
}
