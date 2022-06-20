using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class TradeResponseHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ITradeManager _tradeManager;

        public TradeResponseHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, ITradeManager tradeManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _tradeManager = tradeManager;
        }

        [HandlerAction(PacketType.TRADE_RESPONSE)]
        public void Handle(WorldClient client, TradeResponsePacket packet)
        {
            if (packet.IsDeclined)
            {
                // TODO: do something with decline?
            }
            else
            {
                var tradeReceiver = _gameWorld.Players[_gameSession.Character.Id];
                var tradeRequester = _gameWorld.Players[tradeReceiver.TradeManager.PartnerId];
                if (tradeReceiver is null || tradeRequester is null)
                    return;

                _tradeManager.Start(tradeRequester, tradeReceiver);

                _packetFactory.SendTradeStart(tradeReceiver.GameSession.Client, tradeReceiver.TradeManager.PartnerId);
                _packetFactory.SendTradeStart(tradeRequester.GameSession.Client, tradeRequester.TradeManager.PartnerId);
            }
        }
    }
}
