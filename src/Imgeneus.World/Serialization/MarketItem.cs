using BinarySerialization;
using Imgeneus.Core.Extensions;
using Imgeneus.Network.Serialization;
using System;

namespace Imgeneus.World.Serialization
{
    public class MarketItem : BaseSerializable
    {
        [FieldOrder(0)]
        public uint MarketId { get; set; }

        [FieldOrder(1)]
        public uint TenderMoney { get; set; }

        [FieldOrder(2)]
        public uint DirectMoney { get; set; }

        [FieldOrder(3)]
        public int EndDate { get; set; }

        [FieldOrder(4)]
        public byte Type { get; set; }

        [FieldOrder(5)]
        public byte TypeId { get; set; }

        [FieldOrder(6)]
        public ushort Quality { get; set; }

        [FieldOrder(7)]
        public int[] Gems { get; set; } = new int[6];

        [FieldOrder(8)]
        public byte Count { get; set; }

        [FieldOrder(9)]
        public CraftName CraftName { get; }

        [FieldOrder(10)]
        public byte[] UnknownBytes1 { get; } = new byte[22];

        [FieldOrder(11)]
        public bool IsItemDyed { get; } = true;

        [FieldOrder(12)]
        public byte[] UnknownBytes2 { get; } = new byte[26];

        public MarketItem(uint marketId)
        {
            MarketId = marketId;
            TenderMoney = 3;
            DirectMoney = 4;
            EndDate = DateTime.UtcNow.AddDays(5).ToShaiyaTime();
            Type = 16;
            TypeId = 6;
            Count = 1;
            Gems[0] = 1;
            Gems[1] = 2;
            Gems[5] = 15;
            CraftName = new CraftName("10223344556610203000");
        }
    }
}
