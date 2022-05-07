using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Warehouse
{
    public interface IWarehouseManager: ISessionedService
    {
        void Init(int ownerId, IEnumerable<DbWarehouseItem> items);

        /// <summary>
        /// Stored items.
        /// </summary>
        ConcurrentDictionary<byte, Item> Items { get; }
    }
}
