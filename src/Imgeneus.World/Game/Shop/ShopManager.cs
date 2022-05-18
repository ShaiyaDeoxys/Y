using Imgeneus.World.Game.Inventory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Imgeneus.World.Game.Shop
{
    public class ShopManager : IShopManager
    {
        private readonly ILogger<ShopManager> _logger;
        private readonly IInventoryManager _inventoryManager;

        private int _ownerId;

        public ShopManager(ILogger<ShopManager> logger, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;

            Items = new ReadOnlyDictionary<byte, Item>(_items);

#if DEBUG
            _logger.LogDebug("ShopManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~ShopManager()
        {
            _logger.LogDebug("ShopManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        #region Begin/end

        public bool IsShopOpened { get; private set; }

        public void Begin()
        {
            IsShopOpened = true;
        }

        #endregion

        #region Items

        private ConcurrentDictionary<byte, Item> _items { get; init; } = new();
        public IReadOnlyDictionary<byte, Item> Items { get; init; }

        public bool TryAddItem(byte bag, byte slot, byte shopSlot, int price)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((bag, slot), out var item))
            {
                _logger.LogWarning("Character {id} is trying to add non-existing item from inventory, possible cheating?", _ownerId);
                return false;
            }

            if (_items.ContainsKey(shopSlot))
            {
                _logger.LogWarning("Character {id} is trying to add item from inventory to same slot, possible cheating?", _ownerId);
                return false;
            }


            if (item.IsInShop)
            {
                _logger.LogWarning("Character {id} is trying to add item from inventory twice, possible cheating?", _ownerId);
                return false;
            }

            item.IsInShop = true;
            item.ShopPrice = price;

            return _items.TryAdd(shopSlot, item);
        }

        #endregion
    }
}
