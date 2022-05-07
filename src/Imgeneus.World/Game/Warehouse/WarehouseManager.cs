using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Warehouse
{
    public class WarehouseManager : IWarehouseManager
    {
        public const byte WAREHOUSE_BAG = 100;

        private readonly ILogger<WarehouseManager> _logger;
        private readonly IDatabase _database;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private int _ownerId;

        public WarehouseManager(ILogger<WarehouseManager> logger, IDatabase database, IDatabasePreloader databasePreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig)
        {
            _logger = logger;
            _database = database;
            _databasePreloader = databasePreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
#if DEBUG
            _logger.LogDebug("WarehouseManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~WarehouseManager()
        {
            _logger.LogDebug("WarehouseManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, IEnumerable<DbWarehouseItem> items)
        {
            _ownerId = ownerId;

            foreach (var item in items.Select(x => new Item(_databasePreloader, _enchantConfig, _itemCreateConfig, x)))
                Items.TryAdd(item.Slot, item);
        }

        public async Task Clear()
        {
            var oldItems = await _database.WarehouseItems.Where(x => x.UserId == _ownerId).ToListAsync();
            _database.WarehouseItems.RemoveRange(oldItems);

            foreach(var item in Items.Values)
            {
                var dbItem = new DbWarehouseItem()
                {
                    UserId = _ownerId,
                    Type = item.Type,
                    TypeId = item.TypeId,
                    Count = item.Count,
                    Quality = item.Quality,
                    Slot = item.Slot,
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
                    CreationTime = item.CreationTime,
                    ExpirationTime = item.ExpirationTime,
                    Craftname = item.GetCraftName()
                };
                _database.WarehouseItems.Add(dbItem);
            }

            await _database.SaveChangesAsync();

            Items.Clear();
        }

        #endregion

        #region Items

        public ConcurrentDictionary<byte, Item> Items { get; init; } = new ConcurrentDictionary<byte, Item>();

        #endregion
    }
}
