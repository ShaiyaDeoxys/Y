using BinarySerialization;
using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Serialization
{
    public class MarketEndMoney : BaseSerializable
    {
        [FieldOrder(0)]
        public uint Id { get; set; }

        [FieldOrder(1)]
        public uint MarketId { get; set; }

        [FieldOrder(2)]
        public bool IsFailed { get; set; }

        [FieldOrder(3)]
        public uint ReturnMoney { get; set; }

        [FieldOrder(4)]
        public uint Money { get; set; }

        [FieldOrder(5)]
        public int EndDate { get; set; }

        [FieldOrder(6)]
        public byte Type { get; set; }

        [FieldOrder(7)]
        public byte TypeId { get; set; }

        [FieldOrder(8)]
        public ushort Quality { get; set; }

        [FieldOrder(9)]
        public int[] Gems { get; set; } = new int[6];

        [FieldOrder(10)]
        public byte Count { get; set; }

        [FieldOrder(11)]
        public CraftName CraftName { get; }

        public MarketEndMoney(DbMarketCharacterResultMoney item)
        {
            Id = item.Id;
            MarketId = item.MarketId;
            IsFailed = !item.Success;
            ReturnMoney = item.ReturnMoney;
            Money = item.Money;
            EndDate = item.EndDate.ToShaiyaTime();
            Type = item.Market.MarketItem.Type;
            TypeId = item.Market.MarketItem.TypeId;
            Quality = item.Market.MarketItem.Quality;
            Gems[0] = item.Market.MarketItem.GemTypeId1;
            Gems[1] = item.Market.MarketItem.GemTypeId2;
            Gems[2] = item.Market.MarketItem.GemTypeId3;
            Gems[3] = item.Market.MarketItem.GemTypeId4;
            Gems[4] = item.Market.MarketItem.GemTypeId5;
            Gems[5] = item.Market.MarketItem.GemTypeId6;
            Count = item.Market.MarketItem.Count;
            CraftName = new CraftName(item.Market.MarketItem.Craftname);
        }
    }
}
