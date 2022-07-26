using BinarySerialization;
using Imgeneus.Database.Entities;

namespace Imgeneus.World.Serialization
{
    public class MarketSellItem: MarketItem
    {
        public MarketSellItem(DbMarket market) : base(market)
        {
        }

        [FieldOrder(0)]
        public uint GuaranteeMoney { get; set; } = 123;
    }
}
