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
    public class TradeAddItemHandler : BaseHandler
    {
        private readonly ITradeManager _tradeManager;
        private readonly IGameWorld _gameWorld;

        public TradeAddItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITradeManager tradeManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _tradeManager = tradeManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_OWNER_ADD_ITEM)]
        public void Handle(WorldClient client, TradeAddItemPacket packet)
        {
            var ok = _tradeManager.TryAddItem(packet.Bag, packet.Slot, packet.Quantity, packet.SlotInTradeWindow, out var tradeItem);
            if (ok)
            {
                _packetFactory.SendAddedItemToTrade(client, packet.Bag, packet.Slot, packet.Quantity, packet.SlotInTradeWindow);
                _packetFactory.SendAddedItemToTrade(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, tradeItem, packet.Quantity, packet.SlotInTradeWindow);
            }
        }
    }
}
