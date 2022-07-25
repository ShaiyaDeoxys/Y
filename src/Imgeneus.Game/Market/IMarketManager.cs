namespace Imgeneus.Game.Market
{
    public interface IMarketManager
    {
        void Init(uint ownerId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="slot"></param>
        /// <param name="count"></param>
        /// <param name="marketType"></param>
        /// <param name="minMoney"></param>
        /// <param name="directMoney"></param>
        /// <returns></returns>
        bool TryRegisterItem(byte bag, byte slot, byte count, MarketType marketType, uint minMoney, uint directMoney);
    }
}
