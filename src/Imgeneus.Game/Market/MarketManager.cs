using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Imgeneus.Game.Market
{
    public class MarketManager : IMarketManager
    {
        private readonly ILogger<MarketManager> _logger;
        private readonly IDatabase _database;
        private readonly IInventoryManager _inventoryManager;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private uint _ownerId;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public MarketManager(ILogger<MarketManager> logger, IDatabase database, IInventoryManager inventoryManager, IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig)
        {
            _logger = logger;
            _database = database;
            _inventoryManager = inventoryManager;
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
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

            var expiredMarkets = _database.Market.Where(x => x.CharacterId == _ownerId && x.EndDate < DateTime.UtcNow && !x.IsDeleted).ToList();
            foreach (var market in expiredMarkets)
            {
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
            }

            _database.SaveChanges();
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
            if (_inventoryManager.Gold < marketFee)
                return (false, null, null);

            if (_database.Market.Where(x => x.CharacterId == _ownerId && !x.IsDeleted && x.EndDate > DateTime.UtcNow).Count() >= 5)
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
                    GemTypeId2 = item.Gem2 is null ? 0 : item.Gem2.TypeId,
                    GemTypeId3 = item.Gem3 is null ? 0 : item.Gem3.TypeId,
                    GemTypeId4 = item.Gem4 is null ? 0 : item.Gem4.TypeId,
                    GemTypeId5 = item.Gem5 is null ? 0 : item.Gem5.TypeId,
                    GemTypeId6 = item.Gem6 is null ? 0 : item.Gem6.TypeId,
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
                    _inventoryManager.RemoveItem(item, "registered_on_market");

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

        public async Task<IList<DbMarketCharacterResultItems>> GetEndItems()
        {
            await _semaphore.WaitAsync();

            var results = await _database.MarketResults
                                  .Include(x => x.Market)
                                  .ThenInclude(x => x.MarketItem)
                                  .Where(x => x.CharacterId == _ownerId && x.EndDate > DateTime.UtcNow)
                                  .ToListAsync();

            _semaphore.Release();

            return results;
        }

        public async Task<IList<DbMarketCharacterResultMoney>> GetEndMoney()
        {
            await _semaphore.WaitAsync();

            var results = await _database.MarketMoneys
                                  .Include(x => x.Market)
                                  .ThenInclude(x => x.MarketItem)
                                  .Where(x => x.CharacterId == _ownerId && x.EndDate > DateTime.UtcNow)
                                  .ToListAsync();

            _semaphore.Release();

            return results;
        }

        public async Task<(bool Ok, Item Item)> TryGetItem(uint marketId)
        {
            var result = await _database.MarketResults
                                  .Include(x => x.Market)
                                  .ThenInclude(x => x.MarketItem)
                                  .FirstOrDefaultAsync(x => x.MarketId == marketId && x.CharacterId == _ownerId);

            if (result is null || _inventoryManager.IsFull)
                return (false, null);

            var marketItem = result.Market.MarketItem;
            _database.MarketResults.Remove(result);

            var ok = (await _database.SaveChangesAsync()) > 0;
            Item item = null;
            if (ok)
                item = _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, marketItem), "buy_from_market", true);

            return (ok, item);
        }

        public async Task<bool> TryGetMoney(uint moneyId)
        {
            var money = await _database.MarketMoneys.FirstOrDefaultAsync(x => x.Id == moneyId && x.CharacterId == _ownerId);
            if (money is null)
                return false;

            _database.MarketMoneys.Remove(money);

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
                _inventoryManager.Gold += money.ReturnMoney;

            return ok;
        }

        public IList<DbMarket> LastSearchResults { get; private set; } = new List<DbMarket>();
        public byte PageIndex { get; set; }

        public async Task<IList<DbMarket>> Search(MarketSearchCountry searchCountry, byte minLevel, byte maxLevel, byte grade, MarketItemType marketItemType)
        {
            var items = await _database.Market
                                       .Include(x => x.MarketItem)
                                       .Where(x => x.EndDate > DateTime.UtcNow && !x.IsDeleted)
                                       .ToListAsync();

            var resultItems = new List<DbMarket>();
            foreach (var item in items)
            {
                var config = _definitionsPreloader.Items[(item.MarketItem.Type, item.MarketItem.TypeId)];

                bool shouldAdd = true;

                if (searchCountry == MarketSearchCountry.Light)
                    if (config.Country == ItemClassType.AllFury || config.Country == ItemClassType.Deatheater || config.Country == ItemClassType.Vail)
                        shouldAdd = shouldAdd && false;

                if (searchCountry == MarketSearchCountry.Dark)
                    if (config.Country == ItemClassType.AllLights || config.Country == ItemClassType.Human || config.Country == ItemClassType.Elf)
                        shouldAdd = shouldAdd && false;

                if (config.Reqlevel < minLevel || config.Reqlevel > maxLevel)
                    shouldAdd = shouldAdd && false;

                if (config.ReqDex != grade && grade != 255)
                    shouldAdd = shouldAdd && false;

                if (shouldAdd)
                    switch (marketItemType)
                    {
                        case MarketItemType.TwoHandedWeapon:
                            shouldAdd = config.Type == 2 || config.Type == 4 || config.Type == 46 || config.Type == 48;
                            break;

                        case MarketItemType.SharpenWeapon:
                            shouldAdd = config.Type == 1 || config.Type == 3 || config.Type == 45 || config.Type == 47;
                            break;

                        case MarketItemType.DualWeapon:
                            shouldAdd = config.Type == 5 || config.Type == 49 || config.Type == 50;
                            break;

                        case MarketItemType.Spear:
                            shouldAdd = config.Type == 6 || config.Type == 51 || config.Type == 52;
                            break;

                        case MarketItemType.HeavyWeapon:
                            shouldAdd = config.Type == 8 || config.Type == 55 || config.Type == 56;
                            break;

                        case MarketItemType.LogWeapon:
                            shouldAdd = config.Type == 9 || config.Type == 57 || config.Type == 15 || config.Type == 65;
                            break;

                        case MarketItemType.DaggerWeapon:
                            shouldAdd = config.Type == 10 || config.Type == 58;
                            break;

                        case MarketItemType.Staff:
                            shouldAdd = config.Type == 12 || config.Type == 60 || config.Type == 61;
                            break;

                        case MarketItemType.Bow:
                            shouldAdd = config.Type == 13 || config.Type == 62 || config.Type == 63;
                            break;

                        case MarketItemType.Projectile:
                            shouldAdd = config.Type == 11 || config.Type == 59 || config.Type == 14 || config.Type == 64;
                            break;

                        case MarketItemType.Helmet:
                            shouldAdd = config.Type == 16 || config.Type == 31 || config.Type == 72 || config.Type == 87;
                            break;

                        case MarketItemType.UpperArmor:
                            shouldAdd = config.Type == 17 || config.Type == 32 || config.Type == 67 || config.Type == 82 || config.Type == 73 || config.Type == 88;
                            break;

                        case MarketItemType.LowerArmor:
                            shouldAdd = config.Type == 18 || config.Type == 33 || config.Type == 68 || config.Type == 83 || config.Type == 74 || config.Type == 89;
                            break;

                        case MarketItemType.Gloves:
                            shouldAdd = config.Type == 20 || config.Type == 35 || config.Type == 70 || config.Type == 85 || config.Type == 76 || config.Type == 91;
                            break;

                        case MarketItemType.Shoes:
                            shouldAdd = config.Type == 21 || config.Type == 36 || config.Type == 71 || config.Type == 86 || config.Type == 77 || config.Type == 92;
                            break;

                        case MarketItemType.Coat:
                            shouldAdd = config.Type == 24 || config.Type == 39;
                            break;

                        case MarketItemType.Shield:
                            shouldAdd = config.Type == 69 || config.Type == 84;
                            break;

                        case MarketItemType.Costume:
                            shouldAdd = config.Type == 150;
                            break;

                        case MarketItemType.Necklace:
                            shouldAdd = config.Type == 23 || config.Type == 96;
                            break;

                        case MarketItemType.Ring:
                            shouldAdd = config.Type == 22 || config.Type == 37;
                            break;

                        case MarketItemType.Bracelet:
                            shouldAdd = config.Type == 40 || config.Type == 97;
                            break;

                        case MarketItemType.Lapis:
                            shouldAdd = config.Type == 30 || config.Type == 98;
                            break;

                        case MarketItemType.Lapisia:
                            shouldAdd = config.Type == 95;
                            break;

                        case MarketItemType.Mount:
                            shouldAdd = config.Type == 42;
                            break;

                        case MarketItemType.HighQualityConsumableItem:
                            shouldAdd = config.Type == 100 || config.Type == 101 || config.Type == 102;
                            break;

                        case MarketItemType.OtherItems:
                            shouldAdd = config.Type != 2 && config.Type != 4 && config.Type != 46 && config.Type != 48 &&
                                        config.Type != 1 && config.Type != 3 && config.Type != 45 && config.Type != 47 &&
                                        config.Type != 5 && config.Type != 49 && config.Type != 50 &&
                                        config.Type != 6 && config.Type != 51 && config.Type != 52 &&
                                        config.Type != 8 && config.Type != 55 && config.Type != 56 &&
                                        config.Type != 9 && config.Type != 57 && config.Type != 15 && config.Type != 65 &&
                                        config.Type != 10 && config.Type != 58 &&
                                        config.Type != 12 && config.Type != 60 && config.Type != 61 &&
                                        config.Type != 13 && config.Type != 62 && config.Type != 63 &&
                                        config.Type != 11 && config.Type != 59 && config.Type != 14 && config.Type != 64 &&
                                        config.Type != 16 && config.Type != 31 && config.Type != 72 && config.Type != 87 &&
                                        config.Type != 17 && config.Type != 32 && config.Type != 67 && config.Type != 82 && config.Type != 73 && config.Type != 88 &&
                                        config.Type != 18 && config.Type != 33 && config.Type != 68 && config.Type != 83 && config.Type != 74 && config.Type != 89 &&
                                        config.Type != 20 && config.Type != 35 && config.Type != 70 && config.Type != 85 && config.Type != 76 && config.Type != 91 &&
                                        config.Type != 21 && config.Type != 36 && config.Type != 71 && config.Type != 86 && config.Type != 77 && config.Type != 92 &&
                                        config.Type != 24 && config.Type != 39 &&
                                        config.Type != 69 && config.Type != 84 &&
                                        config.Type != 150 &&
                                        config.Type != 23 && config.Type != 96 &&
                                        config.Type != 22 && config.Type != 37 &&
                                        config.Type != 40 && config.Type != 97 &&
                                        config.Type != 30 && config.Type != 98 &&
                                        config.Type != 95 &&
                                        config.Type != 42 &&
                                        config.Type != 100 && config.Type != 101 && config.Type != 102;
                            break;

                        case MarketItemType.None:
                        default:
                            break;
                    }


                if (shouldAdd)
                    resultItems.Add(item);
            }

            resultItems = resultItems.OrderByDescending(x => x.EndDate).ToList();
            LastSearchResults = resultItems;
            PageIndex = 0;

            return resultItems;
        }

        public async Task<IList<DbMarket>> Search(byte type, byte typeId)
        {
            var items = await _database.MarketItems
                                      .Include(x => x.Market)
                                      .Where(x => x.Type == type && x.TypeId == typeId && !x.Market.IsDeleted)
                                      .ToListAsync();
            var resultItems = new List<DbMarket>();
            foreach (var item in items)
            {
                item.Market.MarketItem = item;
                resultItems.Add(item.Market);
            }

            resultItems = resultItems.OrderByDescending(x => x.EndDate).ToList();
            LastSearchResults = resultItems;
            PageIndex = 0;

            return resultItems;
        }

        public async Task<(MarketBuyItemResult Ok, DbMarketCharacterResultItems Item)> TryDirectBuy(uint marketId)
        {
            var market = await _database.Market
                                        .Include(x => x.MarketItem)
                                        .FirstOrDefaultAsync(x => x.Id == marketId && !x.IsDeleted);
            if (market is null)
                return (MarketBuyItemResult.AuctionClosed, null);

            if (market.CharacterId == _ownerId)
                return (MarketBuyItemResult.MyAuction, null);

            if (_inventoryManager.Gold < market.DirectMoney)
                return (MarketBuyItemResult.NotEnoughMoney, null);

            market.IsDeleted = true;

            var result = new DbMarketCharacterResultItems()
            {
                CharacterId = _ownerId,
                MarketId = market.Id,
                Market = market,
                Success = true,
                EndDate = DateTime.UtcNow.AddDays(14)
            };
            _database.MarketResults.Add(result);

            var money = new DbMarketCharacterResultMoney()
            {
                CharacterId = market.CharacterId,
                MarketId = market.Id,
                Success = true,
                Money = market.DirectMoney,
                ReturnMoney = market.DirectMoney,
                EndDate = DateTime.UtcNow.AddDays(14)
            };
            _database.MarketMoneys.Add(money);

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
                _inventoryManager.Gold -= market.DirectMoney;

            return (MarketBuyItemResult.Ok, result);
        }

        public async Task<(MarketAddFavoriteResult Ok, DbMarket Item)> AddFavorite(uint marketId)
        {
            var market = await _database.Market
                                        .Include(x => x.MarketItem)
                                        .FirstOrDefaultAsync(x => x.Id == marketId && !x.IsDeleted);
            if (market == null)
                return (MarketAddFavoriteResult.Sold, null);

            var favorites = await _database.MarketFavorites.Where(x => x.CharacterId == _ownerId).ToListAsync();
            if (favorites.Count >= 5)
                return (MarketAddFavoriteResult.CanNotAdd, null);

            if (favorites.Any(x => x.MarketId == marketId))
                return (MarketAddFavoriteResult.AlreadyAdded, null);

            var favorite = new DbMarketCharacterFavorite()
            {
                MarketId = marketId,
                CharacterId = _ownerId
            };

            _database.MarketFavorites.Add(favorite);

            await _semaphore.WaitAsync();

            var ok = (await _database.SaveChangesAsync()) > 0;

            _semaphore.Release();

            return (MarketAddFavoriteResult.Ok, market);
        }

        public async Task<IList<DbMarketCharacterFavorite>> GetFavorites()
        {
            var favorites = await _database.MarketFavorites
                                           .Include(x => x.Market)
                                           .ThenInclude(x => x.MarketItem)
                                           .Where(x => x.CharacterId == _ownerId && !x.Market.IsDeleted)
                                           .ToListAsync();
            return favorites;
        }

        public async Task<bool> RemoveFavorite(uint marketId)
        {
            var favorite = await _database.MarketFavorites.FirstOrDefaultAsync(x => x.MarketId == marketId && x.CharacterId == _ownerId);
            if (favorite is null)
                return false;

            _database.MarketFavorites.Remove(favorite);

            return (await _database.SaveChangesAsync()) > 0;
        }

        public async Task<bool> RemoveAllFavorite()
        {
            var favorites = await _database.MarketFavorites.Where(x => x.CharacterId == _ownerId).ToListAsync();
            _database.MarketFavorites.RemoveRange(favorites);
            return (await _database.SaveChangesAsync()) > 0;
        }
    }
}
