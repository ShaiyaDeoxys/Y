using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.Game.Market
{
    public interface IMarketManager
    {
        void Init(uint ownerId);

        /// <summary>
        /// Items, that player is currently selling.
        /// </summary>
        Task<IList<DbMarket>> GetSellItems();

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
        Task<(bool Ok, DbMarket MarketItem, Item Item)> TryRegisterItem(byte bag, byte slot, byte count, MarketType marketType, uint minMoney, uint directMoney);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        Task<(bool Ok, DbMarketCharacterResultItems Result)> TryUnregisterItem(uint marketId);
    }
}
