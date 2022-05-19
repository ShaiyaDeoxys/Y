using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Shop
{
    public class ShopManager : IShopManager
    {
        private readonly ILogger<ShopManager> _logger;
        private readonly IInventoryManager _inventoryManager;
        private readonly IMapProvider _mapProvider;

        private int _ownerId;

        public ShopManager(ILogger<ShopManager> logger, IInventoryManager inventoryManager, IMapProvider mapProvider)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;
            _mapProvider = mapProvider;

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

        public Task Clear()
        {
            TryEnd();
            return Task.CompletedTask;
        }

        #region Begin/end

        public bool IsShopOpened { get; private set; }

        public bool TryBegin()
        {
            if (_mapProvider.Map.Id != 35 && _mapProvider.Map.Id != 36 && _mapProvider.Map.Id != 42)
                return false;

            return true;
        }

        public bool TryCancel()
        {
            if (!IsShopOpened)
                return false;

            IsShopOpened = false;
            OnShopFinished?.Invoke(_ownerId);

            return true;
        }

        #endregion

        #region Items

        public string Name { get; private set; }

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

        public bool TryRemoveItem(byte shopSlot)
        {
            if (!_items.TryRemove(shopSlot, out var item))
            {
                _logger.LogWarning("Character {id} is trying to remove non-existing item from inventory, possible cheating?", _ownerId);
                return false;
            }

            item.IsInShop = false;
            item.ShopPrice = 0;

            return true;
        }

        public event Action<int, string> OnShopStarted;
        public event Action<int> OnShopFinished;

        public bool TryStart(string name)
        {
            if (string.IsNullOrEmpty(name) || Items.Count == 0)
                return false;

            if (IsShopOpened)
                return false;

            Name = name;
            IsShopOpened = true;
            OnShopStarted?.Invoke(_ownerId, Name);

            return true;
        }

        public bool TryEnd()
        {
            if (IsShopOpened)
                TryCancel();

            foreach (var item in _items)
            {
                item.Value.IsInShop = false;
                item.Value.ShopPrice = 0;
            }

            _items.Clear();

            return true;
        }

        #endregion
    }
}
