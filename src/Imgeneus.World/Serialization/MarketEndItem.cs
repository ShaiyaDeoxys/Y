using BinarySerialization;
using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Serialization
{
    public class MarketEndItem : BaseSerializable
    {
        [FieldOrder(0)]
        public uint MarketId { get; set; }

        [FieldOrder(1)]
        public bool IsFailed { get; set; }

        [FieldOrder(2)]
        public int EndDate { get; set; }

        [FieldOrder(3)]
        public byte Type { get; set; }

        [FieldOrder(4)]
        public byte TypeId { get; set; }

        [FieldOrder(5)]
        public ushort Quality { get; set; }

        [FieldOrder(6)]
        public int[] Gems { get; set; } = new int[6];

        [FieldOrder(7)]
        public byte Count { get; set; }

        [FieldOrder(8)]
        public CraftName CraftName { get; }

        [FieldOrder(9)]
        public byte[] UnknownBytes1 { get; } = new byte[22];

        [FieldOrder(10)]
        public bool IsItemDyed { get; }

        [FieldOrder(11)]
        public byte[] UnknownBytes2 { get; } = new byte[26];

        public MarketEndItem(DbMarketCharacterResultItems result)
        {
            MarketId = result.Market.Id;
            IsFailed = !result.Success;
            EndDate = result.EndDate.ToShaiyaTime();
            Type = result.Market.MarketItem.Type;
            TypeId = result.Market.MarketItem.TypeId;
            Quality = result.Market.MarketItem.Quality;
            Gems[0] = result.Market.MarketItem.GemTypeId1;
            Gems[1] = result.Market.MarketItem.GemTypeId2;
            Gems[2] = result.Market.MarketItem.GemTypeId3;
            Gems[3] = result.Market.MarketItem.GemTypeId4;
            Gems[4] = result.Market.MarketItem.GemTypeId5;
            Gems[5] = result.Market.MarketItem.GemTypeId6;
            Count = result.Market.MarketItem.Count;
            CraftName = new CraftName(result.Market.MarketItem.Craftname);
            IsItemDyed = result.Market.MarketItem.HasDyeColor;
        }
    }
}
