using BinarySerialization;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Serialization;
using System.Collections.Generic;

namespace Imgeneus.Network.Serialization
{
    public class WarehouseItems : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Count { get; }

        [FieldOrder(1)]
        [FieldCount(nameof(Count))]
        public List<WarehouseItem> Items { get; } = new List<WarehouseItem>();

        public WarehouseItems(IEnumerable<Item> items)
        {
            foreach (var charItm in items)
                Items.Add(new WarehouseItem(charItm));
        }
    }
}
