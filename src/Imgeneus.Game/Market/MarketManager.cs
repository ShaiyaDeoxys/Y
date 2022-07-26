using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.Game.Market
{
    public class MarketManager : IMarketManager
    {
        private readonly ILogger<MarketManager> _logger;
        private readonly IDatabase _database;
        private readonly IInventoryManager _inventoryManager;

        private uint _ownerId;

        public MarketManager(ILogger<MarketManager> logger, IDatabase database, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _database = database;
            _inventoryManager = inventoryManager;
#if DEBUG
            _logger.LogDebug("MarketManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~MarketManager()
        {
            _logger.LogDebug("MarketManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public async Task<IList<DbMarket>> GetSellItems()
        {
            return await _database.Market.Include(x => x.MarketItem).Where(x => x.CharacterId == _ownerId && !x.IsDeleted).ToListAsync();
        }

        public async Task<(bool Ok, DbMarket MarketItem, Item Item)> TryRegisterItem(byte bag, byte slot, byte count, MarketType marketType, uint minMoney, uint directMoney)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((bag, slot), out var item))
                return (false, null, null);

            var marketFee = (minMoney / 500000 + 1) * 360;
            if(_inventoryManager.Gold < marketFee)
                return (false, null, null);

            if (item.Count < count)
                count = item.Count;

            var market = new DbMarket()
            {
                CharacterId = _ownerId,
                MinMoney = minMoney,
                TenderMoney = minMoney,
                DirectMoney = directMoney,
                MarketType = marketType,
                EndDate = GetEndDate(marketType),
                MarketItem = new DbMarketItem()
                {
                    Type = item.Type,
                    TypeId = item.TypeId,
                    Count = count,
                    Craftname = item.GetCraftName(),
                    GemTypeId1 = item.Gem1 is null ? 0 : item.Gem1.TypeId,
                    GemTypeId2 = item.Gem1 is null ? 0 : item.Gem2.TypeId,
                    GemTypeId3 = item.Gem1 is null ? 0 : item.Gem3.TypeId,
                    GemTypeId4 = item.Gem1 is null ? 0 : item.Gem4.TypeId,
                    GemTypeId5 = item.Gem1 is null ? 0 : item.Gem5.TypeId,
                    GemTypeId6 = item.Gem1 is null ? 0 : item.Gem6.TypeId,
                    HasDyeColor = item.DyeColor.IsEnabled,
                    DyeColorAlpha = item.DyeColor.Alpha,
                    DyeColorSaturation = item.DyeColor.Saturation,
                    DyeColorR = item.DyeColor.R,
                    DyeColorG = item.DyeColor.G,
                    DyeColorB = item.DyeColor.B,
                    Quality = item.Quality
                }
            };

            _database.Market.Add(market);

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
            {
                item.Count -= count;
                if (item.Count == 0)
                    _inventoryManager.RemoveItem(item);

                _inventoryManager.Gold -= marketFee;
            }

            return (ok, market, item);
        }

        private DateTime GetEndDate(MarketType marketType)
        {
            switch (marketType)
            {
                case MarketType.Hour7:
                    return DateTime.UtcNow.AddHours(7);

                case MarketType.Hour24:
                    return DateTime.UtcNow.AddHours(24);

                case MarketType.Day3:
                    return DateTime.UtcNow.AddDays(3);

                default:
                    return DateTime.UtcNow.AddDays(3);
            }
        }

        public async Task<(bool Ok, DbMarketCharacterResultItems Result)> TryUnregisterItem(uint marketId)
        {
            var market = await _database.Market.Include(x => x.MarketItem).FirstOrDefaultAsync(x => x.Id == marketId && x.CharacterId == _ownerId);
            if (market is null)
                return (false, null);

            market.IsDeleted = true;

            var result = new DbMarketCharacterResultItems()
            {
                CharacterId = _ownerId,
                MarketId = market.Id,
                Market = market,
                Success = false,
                EndDate = DateTime.UtcNow.AddDays(14)
            };
            _database.MarketResults.Add(result);

            var ok = (await _database.SaveChangesAsync()) > 0;
            return (ok, result);
        }
    }
}
