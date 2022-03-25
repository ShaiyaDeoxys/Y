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
    public class TradeDecideHandler : BaseHandler
    {
        private readonly ITradeManager _tradeManager;
        private readonly IGameWorld _gameWorld;

        public TradeDecideHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITradeManager tradeManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _tradeManager = tradeManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_DECIDE)]
        public void Handle(WorldClient client, TradeDecidePacket packet)
        {
            if (packet.IsDecided)
            {
                _tradeManager.TraderDecideConfirm();

                // 1 means sender, 2 means partner.
                _packetFactory.SendTradeDecide(client, 1, true);
                _packetFactory.SendTradeDecide(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, true);
            }
            else
            {
                _tradeManager.TradeDecideDecline();

                // Decline both.
                _packetFactory.SendTradeDecide(client, 1, false);
                _packetFactory.SendTradeDecide(client, 2, false);
                _packetFactory.SendTradeDecide(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 1, false);
                _packetFactory.SendTradeDecide(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, false);
            }
        }
    }
}
