using BinarySerialization;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;
using System.Collections.Generic;

namespace Imgeneus.World.Serialization
{
    public class GuildWarehouseItems : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Count { get; }

        [FieldOrder(1)]
        [FieldCount(nameof(Count))]
        public List<GuildWarehouseItem> Items { get; } = new List<GuildWarehouseItem>();

        public GuildWarehouseItems(IEnumerable<DbGuildWarehouseItem> items)
        {
            foreach (var item in items)
                Items.Add(new GuildWarehouseItem(item));
        }
    }
}
