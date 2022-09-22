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

        private uint _ownerId;

        public ShopManager(ILogger<ShopManager> logger, IInventoryManager inventoryManager, IMapProvider mapProvider)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;
            _mapProvider = mapProvider;

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

        public void Init(uint ownerId)
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
            if (_mapProvider.Map is null || (_mapProvider.Map.Id != 35 && _mapProvider.Map.Id != 36 && _mapProvider.Map.Id != 42))
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
        public IReadOnlyDictionary<byte, Item> Items { get => new ReadOnlyDictionary<byte, Item>(_items); }

        public bool TryAddItem(byte bag, byte slot, byte shopSlot, uint price)
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

        public event Action<uint, string> OnShopStarted;
        public event Action<uint> OnShopFinished;

        public bool TryStart(string name)
        {
            if (!TryBegin())
                return false;

            if (string.IsNullOrEmpty(name) || _items.Count == 0)
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

        #region Buy in another shop

        private IShopManager _useShop;
        public IShopManager UseShop
        {
            get => _useShop;
            set
            {
                if (_useShop is not null)
                {
                    _useShop.OnShopFinished -= UseShop_OnShopFinished;
                    _useShop.OnSellItemCountChanged -= UseShop_OnSellItemCountChanged;
                }

                _useShop = value;

                if (_useShop is not null)
                {
                    _useShop.OnShopFinished += UseShop_OnShopFinished;
                    _useShop.OnSellItemCountChanged += UseShop_OnSellItemCountChanged;
                }
            }
        }

        private void UseShop_OnShopFinished(uint senderId)
        {
            UseShop = null;
            OnUseShopClosed?.Invoke();
        }

        private void UseShop_OnSellItemCountChanged(byte slot, byte count)
        {
            OnUseShopItemCountChanged?.Invoke(slot, count);
        }

        public event Action OnUseShopClosed;

        public event Action<byte, byte> OnUseShopItemCountChanged;

        public bool TryBuyItem(byte slot, byte count, out Item soldItem, out Item shopItem)
        {
            soldItem = null;
            shopItem = null;

            if (UseShop is null)
                return false;

            if (!UseShop.Items.TryGetValue(slot, out shopItem))
                return false;

            if (shopItem.Count < count)
                count = shopItem.Count;

            if (shopItem.ShopPrice * count > _inventoryManager.Gold)
                return false;

            _inventoryManager.Gold -= shopItem.ShopPrice * count;

            soldItem = UseShop.TrySellItem(slot, count);
            if (soldItem is null)
            {
                _inventoryManager.Gold += shopItem.ShopPrice * count;
                return false;
            }

            _inventoryManager.AddItem(soldItem, "from_local_shop");

            return true;
        }

        public event Action<byte, byte> OnSellItemCountChanged;

        public event Action<byte, byte> OnSoldItem;

        public Item TrySellItem(byte slot, byte count)
        {
            if (!Items.TryGetValue(slot, out var item))
                return null;

            if (item.Count < count)
                count = item.Count;

            _inventoryManager.Gold += item.ShopPrice * count;
            item.Count -= count;
            if (item.Count == 0)
            {
                _items.TryRemove(slot, out var _);
                _inventoryManager.RemoveItem(item, "sold_to_npc");
            }

            Item resultItem = item.Clone();
            resultItem.Count = count;

            OnSellItemCountChanged?.Invoke(slot, item.Count);
            OnSoldItem?.Invoke(slot, item.Count);

            return resultItem;
        }

        #endregion
    }
}
