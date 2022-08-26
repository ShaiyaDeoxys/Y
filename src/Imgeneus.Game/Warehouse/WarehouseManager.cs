using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Packets;
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
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly IGameWorld _gameWorld;
        private readonly IGamePacketFactory _packetFactory;
        private int _userId;
        private uint _characterId;

        public WarehouseManager(ILogger<WarehouseManager> logger, IDatabase database, IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, IGameWorld gameWorld, IGamePacketFactory packetFactory)
        {
            _logger = logger;
            _database = database;
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _gameWorld = gameWorld;
            _packetFactory = packetFactory;
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

        public void Init(int userId, uint characterId, uint? guildId, IEnumerable<DbWarehouseItem> items)
        {
            _userId = userId;
            _characterId = characterId;
            GuildId = guildId;

            foreach (var item in items.Select(x => new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, x)))
                _items.TryAdd(item.Slot, item);

            Items = new(_items); // Keep it. ReadOnlyDictionary has bug with Items.Values update.
        }

        public async Task Clear()
        {
            var oldItems = await _database.WarehouseItems.Where(x => x.UserId == _userId).ToListAsync();
            _database.WarehouseItems.RemoveRange(oldItems);

            foreach (var item in _items.Values)
            {
                var dbItem = new DbWarehouseItem()
                {
                    UserId = _userId,
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
                    _logger.LogError("Can not load guild house warehouse, no guild id provided. Character id {id}", _userId);
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

                var ok = _database.SaveChanges() > 0;
                if (ok)
                {
                    var me = _gameWorld.Players[_characterId];
                    foreach (var member in me.GuildManager.GuildMembers)
                    {
                        if (!_gameWorld.Players.ContainsKey(member.Id) || me.Id == member.Id)
                            continue;

                        var player = _gameWorld.Players[member.Id];
                        _packetFactory.SendGuildWarehouseItemAdd(player.GameSession.Client, dbGuildItem, me.Id);
                    }
                }

                return ok;
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
                    _logger.LogError("Could not put item into double warehouse for {characterId}", _userId);
                    return false;
                }

                _items.TryRemove(slot, out item);
                return true;
            }

            if (bag == GUILD_WAREHOUSE_BAG)
            {
                var dbItem = _database.GuildWarehouseItems
                                      .FirstOrDefault(x => x.GuildId == GuildId && x.Slot == slot);

                if (dbItem is null)
                    return true; // No item, nothing to remove.

                _database.GuildWarehouseItems.Remove(dbItem);
                var ok = _database.SaveChanges() > 0;
                if (ok)
                {
                    item = new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, dbItem);

                    var me = _gameWorld.Players[_characterId];
                    foreach (var member in me.GuildManager.GuildMembers)
                    {
                        if (!_gameWorld.Players.ContainsKey(member.Id)) // || me.Id == member.Id) Looks like there is some bug in client game.exe and bag 255 is not processed correctly.
                            continue;

                        var player = _gameWorld.Players[member.Id];
                        _packetFactory.SendGuildWarehouseItemRemove(player.GameSession.Client, dbItem, me.Id);
                    }
                }

                return ok;
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

        public uint? GuildId { get; set; }

        #endregion
    }
}
