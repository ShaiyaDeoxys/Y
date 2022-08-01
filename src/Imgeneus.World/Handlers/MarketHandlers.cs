using Imgeneus.Database.Constants;
using Imgeneus.Game.Market;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MarketHandlers : BaseHandler
    {
        private readonly IMarketManager _marketManager;
        private readonly IInventoryManager _inventoryManager;

        public MarketHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IMarketManager marketManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _marketManager = marketManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.MARKET_GET_SELL_LIST)]
        public async Task SellListHandle(WorldClient client, EmptyPacket packet)
        {
            var items = await _marketManager.GetSellItems();
            _packetFactory.SendMarketSellList(client, items);
        }

        [HandlerAction(PacketType.MARKET_GET_TENDER_LIST)]
        public void TenderListHandle(WorldClient client, EmptyPacket packet)
        {
            _packetFactory.SendMarketTenderList(client);
        }

        [HandlerAction(PacketType.MARKET_REGISTER_ITEM)]
        public async Task RegisterItemHandle(WorldClient client, MarketRegisterItemPacket packet)
        {
            var result = await _marketManager.TryRegisterItem(packet.Bag, packet.Slot, packet.Count, (MarketType)packet.MarketType, packet.MinMoney, packet.DirectMoney);
            _packetFactory.SendMarketItemRegister(client, result.Ok, result.MarketItem, result.Item, _inventoryManager.Gold);
        }

        [HandlerAction(PacketType.MARKET_UNREGISTER_ITEM)]
        public async Task UnregisterItemHandle(WorldClient client, MarketUnregisterItemPacket packet)
        {
            var result = await _marketManager.TryUnregisterItem(packet.MarketId);
            _packetFactory.SendMarketItemUnregister(client, result.Ok, result.Result);
        }

        [HandlerAction(PacketType.MARKET_GET_END_ITEM_LIST)]
        public async Task GetEndItemsHandle(WorldClient client, EmptyPacket packet)
        {
            var items = await _marketManager.GetEndItems();
            _packetFactory.SendMarketEndItems(client, items);
        }

        [HandlerAction(PacketType.MARKET_GET_END_MONEY_LIST)]
        public async Task GetEndMoneyHandle(WorldClient client, EmptyPacket packet)
        {
            var items = await _marketManager.GetEndMoney();
            _packetFactory.SendMarketEndMoney(client, items);
        }

        [HandlerAction(PacketType.MARKET_GET_ITEM)]
        public async Task GetItemHandle(WorldClient client, MarketGetPacket packet)
        {
            var result = await _marketManager.TryGetItem(packet.Id);
            _packetFactory.SendMarketGetItem(client, result.Ok, packet.Id, result.Item);
        }

        [HandlerAction(PacketType.MARKET_GET_MONEY)]
        public async Task GetMoneyHandle(WorldClient client, MarketGetPacket packet)
        {
            var ok = await _marketManager.TryGetMoney(packet.Id);
            _packetFactory.SendMarketGetMoney(client, ok, packet.Id, _inventoryManager.Gold);
        }

        [HandlerAction(PacketType.MARKET_SEARCH_FIRST)]
        public async Task SearchHandle(WorldClient client, MarketSearchFirstPacket packet)
        {
            var results = await _marketManager.Search(packet.SearchCountry, packet.MinLevel, packet.MaxLevel, packet.Grade, packet.MarketItemType);
            _packetFactory.SendMarketSearchSection(client, 0, results.Count > 7 ? (byte)1 : (byte)0, results.Take(7).ToList());
        }

        [HandlerAction(PacketType.MARKET_SEARCH_FIRST_ITEM_ID)]
        public async Task SearchItemIdHandle(WorldClient client, MarketSearchFirstItemIdPacket packet)
        {
            var results = await _marketManager.Search(packet.Type, packet.TypeId);
            _packetFactory.SendMarketSearchSection(client, 0, results.Count > 7 ? (byte)1 : (byte)0, results.Take(7).ToList());
        }

        [HandlerAction(PacketType.MARKET_SEARCH_SECTION)]
        public void SearchSectionHandle(WorldClient client, MarketSearchSectionPacket packet)
        {
            if (packet.Action == MarketSearchAction.MoveNext)
                _marketManager.PageIndex++;
            if (packet.Action == MarketSearchAction.MovePrev)
                _marketManager.PageIndex--;

            _packetFactory.SendMarketSearchSection(client,
                                                   _marketManager.PageIndex,
                                                   (byte)(_marketManager.PageIndex + 1),
                                                   _marketManager.LastSearchResults.Skip(7 * _marketManager.PageIndex).Take(7).ToList());
        }

        [HandlerAction(PacketType.MARKET_DIRECT_BUY)]
        public async Task DirectBuyHandle(WorldClient client, MarketDirectBuyPacket packet)
        {
            var result = await _marketManager.TryDirectBuy(packet.MarketId);
            _packetFactory.SendMarketDirectBuy(client,
                                               result.Ok,
                                               _inventoryManager.Gold,
                                               result.Item);
        }

        [HandlerAction(PacketType.MARKET_ADD_CONCERT_MARKET)]
        public async Task AddFavoriteHandle(WorldClient client, MarketAddFavoritePacket packet)
        {
            var result = await _marketManager.AddFavorite(packet.MarketId);
            _packetFactory.SendMarketAddFavorite(client, result.Ok, result.Item);
        }

        [HandlerAction(PacketType.MARKET_GET_CONCERN_LIST)]
        public async Task GetFavoriteListHandle(WorldClient client, EmptyPacket packet)
        {
            var results = await _marketManager.GetFavorites();
            _packetFactory.SendMarketFavorites(client, results);
        }

        [HandlerAction(PacketType.MARKET_REMOVE_CONCERT_MARKET)]
        public async Task RemoveFavoriteHandle(WorldClient client, MarketAddFavoritePacket packet)
        {
            var ok = await _marketManager.RemoveFavorite(packet.MarketId);
            _packetFactory.SendMarketRemoveFavorite(client, ok, packet.MarketId);
        }

        [HandlerAction(PacketType.MARKET_CONCERT_REMOVE_ALL)]
        public async Task RemoveAllFavoriteHandle(WorldClient client, EmptyPacket packet)
        {
            var ok = await _marketManager.RemoveAllFavorite();
            _packetFactory.SendMarketRemoveAllFavorite(client, ok);
        }

        [HandlerAction(PacketType.MARKET_TENDER)]
        public async Task BidHandle(WorldClient client, MarketTenderPacket packet)
        {
            // TODO: handle bid logic.
        }
    }
}
