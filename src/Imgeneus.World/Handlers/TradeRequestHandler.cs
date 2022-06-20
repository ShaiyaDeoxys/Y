using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class TradeRequestHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public TradeRequestHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_REQUEST)]
        public void Handle(WorldClient client, TradeRequestPacket packet)
        {
            if (!_gameWorld.Players.ContainsKey(_gameSession.Character.Id) || !_gameWorld.Players.ContainsKey(packet.TradeToWhomId))
                return;

            var requester = _gameWorld.Players[_gameSession.Character.Id];
            var receiver = _gameWorld.Players[packet.TradeToWhomId];

            requester.TradeManager.PartnerId = receiver.Id;
            receiver.TradeManager.PartnerId = requester.Id;

            _packetFactory.SendTradeRequest(receiver.GameSession.Client, requester.Id);
        }
    }
}
