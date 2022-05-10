using BinarySerialization;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Serialization
{
    public class GuildWarehouseItem : BaseSerializable
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
        public byte[] UnknownBytes1 { get; } = new byte[22];

        [FieldOrder(7)]
        public bool IsItemDyed { get; }

        [FieldOrder(8)]
        public byte[] UnknownBytes2 { get; } = new byte[26];

        [FieldOrder(9)]
        public CraftName CraftName { get; }

        public GuildWarehouseItem(DbGuildWarehouseItem item)
        {
            Slot = item.Slot;
            Type = item.Type;
            TypeId = item.TypeId;
            Quality = item.Quality;
            Gems = new int[] {
                item.GemTypeId1,
                item.GemTypeId2,
                item.GemTypeId3,
                item.GemTypeId4,
                item.GemTypeId5,
                item.GemTypeId6,
            };
            Count = item.Count;
            CraftName = new CraftName(item.Craftname);
            IsItemDyed = item.HasDyeColor;
        }
    }
}
