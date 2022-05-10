using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Warehouse
{
    public class WarehouseManager : IWarehouseManager
    {
        public const byte WAREHOUSE_BAG = 100;
        public const byte GUILD_WAREHOUSE_BAG = 255;

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
            Items = new(_items);

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

        public void Init(int ownerId, int? guildId, IEnumerable<DbWarehouseItem> items)
        {
            _ownerId = ownerId;
            GuildId = guildId;

            foreach (var item in items.Select(x => new Item(_databasePreloader, _enchantConfig, _itemCreateConfig, x)))
                _items.TryAdd(item.Slot, item);

            Items = new(_items); // Keep it. ReadOnlyDictionary has bug with Items.Values update.
        }

        public async Task Clear()
        {
            var oldItems = await _database.WarehouseItems.Where(x => x.UserId == _ownerId).ToListAsync();
            _database.WarehouseItems.RemoveRange(oldItems);

            foreach (var item in _items.Values)
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

            _items.Clear();
        }

        #endregion

        #region Items

        public bool IsDoubledWarehouse { get; set; }

        private ConcurrentDictionary<byte, Item> _items { get; init; } = new();
        public ReadOnlyDictionary<byte, Item> Items { get; private set; }

        public bool TryAdd(byte bag, byte slot, Item item)
        {
            if (bag != WAREHOUSE_BAG && bag != GUILD_WAREHOUSE_BAG)
                return false;

            if (bag == WAREHOUSE_BAG)
                return _items.TryAdd(slot, item);

            if (bag == GUILD_WAREHOUSE_BAG)
            {
                if (!GuildId.HasValue)
                {
                    _logger.LogError("Can not load guild house warehouse, no guild id provided. Character id {id}", _ownerId);
                    return false;
                }

                var dbGuildItem = new DbGuildWarehouseItem()
                {
                    GuildId = GuildId.Value,
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
                _database.GuildWarehouseItems.Add(dbGuildItem);
                var count = _database.SaveChanges();
                return count > 0;
            }

            return false;
        }

        public bool TryRemove(byte bag, byte slot, out Item item)
        {
            item = null;

            if (bag != WAREHOUSE_BAG && bag != GUILD_WAREHOUSE_BAG)
                return false;

            if (bag == WAREHOUSE_BAG)
            {
                if (slot >= 120 && !IsDoubledWarehouse)
                {
                    // Game should check if it's possible to put item in 4,5,6 tab.
                    // If packet still came, probably player is cheating.
                    _logger.LogError("Could not put item into double warehouse for {characterId}", _ownerId);
                    return false;
                }

                _items.TryRemove(slot, out item);
                return true;
            }

            if (bag == GUILD_WAREHOUSE_BAG)
            {
                /*_database.GuildWarehouseItems
                    .AsNoTracking()
                    .FirstOrDefault();*/
            }

            return true;
        }

        public async Task<ICollection<DbGuildWarehouseItem>> GetGuildItems()
        {
            if (!GuildId.HasValue)
                return new List<DbGuildWarehouseItem>();

            return await _database.GuildWarehouseItems.Where(x => x.GuildId == GuildId.Value).ToListAsync();
        }

        #endregion

        #region Guild items

        public int? GuildId { get; set; }

        #endregion
    }
}
