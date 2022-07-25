using BinarySerialization;

namespace Imgeneus.World.Serialization
{
    public class MarketSellItem: MarketItem
    {
        public MarketSellItem(uint marketId) : base(marketId)
        {
        }

        [FieldOrder(0)]
        public uint GuaranteeMoney { get; set; } = 123;
    }
}
