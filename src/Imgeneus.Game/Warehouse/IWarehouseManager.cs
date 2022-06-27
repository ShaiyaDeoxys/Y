using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Warehouse
{
    public interface IWarehouseManager: ISessionedService
    {
        void Init(int userId, uint characterId, uint? guildId, IEnumerable<DbWarehouseItem> items);

        /// <summary>
        /// User's stored items.
        /// </summary>
        ReadOnlyDictionary<byte, Item> Items { get; }

        /// <summary>
        /// Can player put items in 4,5,6 tabs?
        /// </summary>
        bool IsDoubledWarehouse { get; set; }

        /// <summary>
        /// Guild id.
        /// </summary>
        uint? GuildId { get; set; }

        /// <summary>
        /// Tries to add item to warehouse.
        /// </summary>
        bool TryAdd(byte bag, byte slot, Item item);

        /// <summary>
        /// Tries to remove item from warehouse.
        /// </summary>
        bool TryRemove(byte bag, byte slot, out Item item);
        
        /// <summary>
        /// Loads guild items from database.
        /// </summary>
        Task<ICollection<DbGuildWarehouseItem>> GetGuildItems();
    }
}
