using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Inventory;
using System.Collections.Generic;

namespace Imgeneus.World.Serialization
{
    public class MyShopItems : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Count { get; }

        [FieldOrder(1)]
        [FieldCount(nameof(Count))]
        public List<MyShopItem> Items { get; } = new List<MyShopItem>();

        public MyShopItems(IReadOnlyDictionary<byte, Item> items)
        {
            foreach(var slot in items.Keys)
                Items.Add(new MyShopItem(slot, items[slot]));
        }
    }
}
