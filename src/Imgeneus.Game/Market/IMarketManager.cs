using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets.Game;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IList<DbMarketCharacterResultItems>> GetEndItems();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IList<DbMarketCharacterResultMoney>> GetEndMoney();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        Task<(bool Ok, Item Item)> TryGetItem(uint marketId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        Task<bool> TryGetMoney(uint moneyId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchCountry"></param>
        /// <param name="minLevel"></param>
        /// <param name="maxLevel"></param>
        /// <param name="grade"></param>
        /// <param name="marketItemType"></param>
        /// <returns></returns>
        Task<IList<DbMarket>> Search(MarketSearchCountry searchCountry, byte minLevel, byte maxLevel, byte grade, MarketItemType marketItemType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        Task<IList<DbMarket>> Search(byte type, byte typeId);

        /// <summary>
        /// 
        /// </summary>
        IList<DbMarket> LastSearchResults { get; }

        /// <summary>
        /// Search page in focus.
        /// </summary>
        byte PageIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        Task<(MarketBuyItemResult Ok, DbMarketCharacterResultItems Item)> TryDirectBuy(uint marketId);
    }
}
