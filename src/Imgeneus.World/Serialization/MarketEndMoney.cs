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
        public byte ResultType { get; set; } = 1;

        [FieldOrder(3)]
        public uint ReturnMoney { get; set; } = 456;

        [FieldOrder(4)]
        public uint Money { get; set; } = 654;

        [FieldOrder(5)]
        public int EndDate { get; set; }

        public MarketEndMoney(DbMarketCharacterResultMoney item)
        {
            Id = item.Id;
            MarketId = item.MarketId;
            EndDate = item.EndDate.ToShaiyaTime();
        }
    }
}
