using Imgeneus.Game.Market;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MarketHandlers : BaseHandler
    {
        private readonly IMarketManager _marketManager;

        public MarketHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IMarketManager marketManager) : base(packetFactory, gameSession)
        {
            _marketManager = marketManager;
        }

        [HandlerAction(PacketType.MARKET_GET_SELL_LIST)]
        public void SellListHandle(WorldClient client, EmptyPacket packet)
        {
            _packetFactory.SendMarketSellList(client);
        }

        [HandlerAction(PacketType.MARKET_GET_TENDER_LIST)]
        public void TenderListHandle(WorldClient client, EmptyPacket packet)
        {
            _packetFactory.SendMarketTenderList(client);
        }

        [HandlerAction(PacketType.MARKET_REGISTER_ITEM)]
        public void RegisterItemHandle(WorldClient client, MarketRegisterItemPacket packet)
        {
            var ok = _marketManager.TryRegisterItem(packet.Bag, packet.Slot, packet.Count, (MarketType)packet.MarketType, packet.MinMoney, packet.DirectMoney);
        }
    }
}
