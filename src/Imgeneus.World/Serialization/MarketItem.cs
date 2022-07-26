using BinarySerialization;
using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
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
        public bool IsItemDyed { get; }

        [FieldOrder(12)]
        public byte[] UnknownBytes2 { get; } = new byte[26];

        public MarketItem(DbMarket market)
        {
            MarketId = market.Id;
            TenderMoney = market.TenderMoney;
            DirectMoney = market.DirectMoney;
            EndDate = market.EndDate.ToShaiyaTime();
            Type = market.MarketItem.Type;
            TypeId = market.MarketItem.TypeId;
            Count = market.MarketItem.Count;
            Gems[0] = market.MarketItem.GemTypeId1;
            Gems[1] = market.MarketItem.GemTypeId2;
            Gems[2] = market.MarketItem.GemTypeId3;
            Gems[3] = market.MarketItem.GemTypeId4;
            Gems[4] = market.MarketItem.GemTypeId5;
            Gems[5] = market.MarketItem.GemTypeId6;
            CraftName = new CraftName(market.MarketItem.Craftname);
            IsItemDyed = market.MarketItem.HasDyeColor;
        }
    }
}
