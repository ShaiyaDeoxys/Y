using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Inventory;

namespace Imgeneus.World.Serialization
{
    public class WarehouseItem : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Slot { get; }

        [FieldOrder(1)]
        public byte Type { get; }

        [FieldOrder(2)]
        public byte TypeId { get; }

        [FieldOrder(3)]
        public ushort Quality { get; }

        [FieldOrder(4)]
        public int[] Gems { get; }

        [FieldOrder(5)]
        public byte Count { get; }

        [FieldOrder(6)]
        public byte[] FromDate = new byte[4];

        [FieldOrder(7)]
        public byte[] UntilDate = new byte[4];

        [FieldOrder(8)]
        public byte[] UnknownBytes1 { get; } = new byte[22];

        [FieldOrder(9)]
        public bool IsItemDyed { get; } = true;

        [FieldOrder(10)]
        public byte[] UnknownBytes2 { get; } = new byte[26];

        [FieldOrder(11)]
        public CraftName CraftName { get; }

        public WarehouseItem(Item item)
        {
            Slot = item.Slot;
            Type = item.Type;
            TypeId = item.TypeId;
            Quality = item.Quality;
            Gems = new int[] {
                item.Gem1 is null ? 0 : item.Gem1.TypeId,
                item.Gem2 is null ? 0 : item.Gem2.TypeId,
                item.Gem3 is null ? 0 : item.Gem3.TypeId,
                item.Gem4 is null ? 0 : item.Gem4.TypeId,
                item.Gem5 is null ? 0 : item.Gem5.TypeId,
                item.Gem6 is null ? 0 : item.Gem6.TypeId,
            };
            Count = item.Count;
            CraftName = new CraftName(item.GetCraftName());
            IsItemDyed = item.DyeColor.IsEnabled;
        }
    }
}
