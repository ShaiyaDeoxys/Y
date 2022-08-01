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
        /// Registers item from inventory for sale.
        /// </summary>
        /// <param name="bag">bag in inventory</param>
        /// <param name="slot">slot in inventory</param>
        /// <param name="count">number of items</param>
        /// <param name="marketType">for how long item will be on market</param>
        /// <param name="minMoney">start price</param>
        /// <param name="directMoney">price when item can be bought directly, without bid</param>
        Task<(bool Ok, DbMarket MarketItem, Item Item)> TryRegisterItem(byte bag, byte slot, byte count, MarketType marketType, uint minMoney, uint directMoney);
        
        /// <summary>
        /// Unregisters item from market.
        /// </summary>
        Task<(bool Ok, DbMarketCharacterResultItems Result)> TryUnregisterItem(uint marketId);

        /// <summary>
        /// List of bought or unsold items.
        /// </summary>
        Task<IList<DbMarketCharacterResultItems>> GetEndItems();

        /// <summary>
        /// List of gold for sold item.s
        /// </summary>
        Task<IList<DbMarketCharacterResultMoney>> GetEndMoney();

        /// <summary>
        /// Gets item from market into inventory.
        /// </summary>
        Task<(bool Ok, Item Item)> TryGetItem(uint marketId);

        /// <summary>
        /// Gets gold from market into inventory.
        /// </summary>
        Task<bool> TryGetMoney(uint moneyId);

        /// <summary>
        /// Search items.
        /// </summary>
        Task<IList<DbMarket>> Search(MarketSearchCountry searchCountry, byte minLevel, byte maxLevel, byte grade, MarketItemType marketItemType);

        /// <summary>
        /// Search same items by it's type and id.
        /// </summary>
        Task<IList<DbMarket>> Search(byte type, byte typeId);

        /// <summary>
        /// Last saved search result.
        /// </summary>
        IList<DbMarket> LastSearchResults { get; }

        /// <summary>
        /// Search page in focus.
        /// </summary>
        byte PageIndex { get; set; }

        /// <summary>
        /// Buy item for it's direct price.
        /// </summary>
        Task<(MarketBuyItemResult Ok, DbMarketCharacterResultItems Item)> TryDirectBuy(uint marketId);
        
        /// <summary>
        /// Add item to favorites.
        /// </summary>
        Task<(MarketAddFavoriteResult Ok, DbMarket Item)> AddFavorite(uint marketId);

        /// <summary>
        /// List of favorite items.
        /// </summary>
        Task<IList<DbMarketCharacterFavorite>> GetFavorites();

        /// <summary>
        /// Remove item from favorites.
        /// </summary>
        Task<bool> RemoveFavorite(uint marketId);

        /// <summary>
        /// Removes all favorite items.
        /// </summary>
        Task<bool> RemoveAllFavorite();
    }
}
