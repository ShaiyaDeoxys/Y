using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Inventory;

namespace Imgeneus.World.Serialization
{
    public class SoldItem : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Bag { get; }

        [FieldOrder(1)]
        public byte Slot { get; }

        [FieldOrder(2)]
        public byte Type { get; }

        [FieldOrder(3)]
        public byte TypeId { get; }

        [FieldOrder(4)]
        public byte Count { get; }

        [FieldOrder(5)]
        public ushort Quality { get; }

        [FieldOrder(6)]
        public byte[] FromDate = new byte[4]; // ?

        [FieldOrder(7)]
        public byte[] UntilDate = new byte[4]; // ?

        [FieldOrder(8)]
        public byte[] UnknownBytes1 = new byte[22];

        [FieldOrder(9)]
        public bool IsItemDyed { get; }

        [FieldOrder(10)]
        public byte[] UnknownBytes2 = new byte[26];

        [FieldOrder(11)]
        public int[] Gems { get; }

        [FieldOrder(12)]
        public CraftName CraftName { get; }

        public SoldItem(Item item)
        {
            Bag = item.Bag;
            Slot = item.Slot;
            Type = item.Type;
            TypeId = item.TypeId;
            Count = item.Count;
            Quality = item.Quality;
            Gems = new int[] {
                item.Gem1 is null ? 0 : item.Gem1.TypeId,
                item.Gem2 is null ? 0 : item.Gem2.TypeId,
                item.Gem3 is null ? 0 : item.Gem3.TypeId,
                item.Gem4 is null ? 0 : item.Gem4.TypeId,
                item.Gem5 is null ? 0 : item.Gem5.TypeId,
                item.Gem6 is null ? 0 : item.Gem6.TypeId,
            };

            CraftName = new CraftName(item.GetCraftName());

            IsItemDyed = true;
        }
    }
}
