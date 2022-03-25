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
    public class TradeRemoveItemHandler : BaseHandler
    {
        private readonly ITradeManager _tradeManager;
        private readonly IGameWorld _gameWorld;

        public TradeRemoveItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITradeManager tradeManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _tradeManager = tradeManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_REMOVE_ITEM)]
        public void Handle(WorldClient client, TradeRemoveItemPacket packet)
        {
            var ok = _tradeManager.TryRemoveItem(packet.SlotInTradeWindow);
            if (ok)
            {
                _packetFactory.SendRemovedItemFromTrade(client, 1);
                _packetFactory.SendRemovedItemFromTrade(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2);

                // Decline both.
                _packetFactory.SendTradeDecide(client, 1, false);
                _packetFactory.SendTradeDecide(client, 2, false);
                _packetFactory.SendTradeDecide(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 1, false);
                _packetFactory.SendTradeDecide(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, false);
            }
        }
    }
}
