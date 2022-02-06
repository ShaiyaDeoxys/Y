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
            var tradeRequester = _gameWorld.Players[_gameSession.CharId];
            var tradeReceiver = _gameWorld.Players[packet.TradeToWhomId];

            tradeRequester.TradeManager.PartnerId = tradeReceiver.Id;
            tradeReceiver.TradeManager.PartnerId = tradeRequester.Id;

            _packetFactory.SendTradeRequest(tradeReceiver.GameSession.Client, tradeRequester.Id);
        }
    }
}
