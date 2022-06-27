using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Shop
{
    public interface IShopManager : ISessionedService
    {
        void Init(uint ownerId);

        /// <summary>
        /// Items, that are currently in shop.
        /// </summary>
        IReadOnlyDictionary<byte, Item> Items { get; }

        /// <summary>
        /// Is shop currently opened?
        /// </summary>
        bool IsShopOpened { get; }

        /// <summary>
        /// Shop name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Begins shop.
        /// </summary>
        bool TryBegin();

        /// <summary>
        /// Closes shop.
        /// </summary>
        bool TryCancel();

        /// <summary>
        /// Tries to add item to shop.
        /// </summary>
        bool TryAddItem(byte bag, byte slot, byte shopSlot, uint price);

        /// <summary>
        /// Tries to remove item from shop.
        /// </summary>
        bool TryRemoveItem(byte shopSlot);

        /// <summary>
        /// Tries to start local shop.
        /// </summary>
        bool TryStart(string name);

        /// <summary>
        /// Tries to end local shop.
        /// </summary>
        bool TryEnd();

        /// <summary>
        /// Event, that is fired, when shop is started.
        /// </summary>
        event Action<uint, string> OnShopStarted;

        /// <summary>
        /// Event, that is fired, when shop is closed.
        /// </summary>
        event Action<uint> OnShopFinished;

        /// <summary>
        /// If player is buying something from another's player shop, we keep reference to another player's shop.
        /// </summary>
        IShopManager UseShop { get; set; }

        /// <summary>
        /// Event, that is fired, when use shop is closed by shop owner.
        /// </summary>
        event Action OnUseShopClosed;

        /// <summary>
        /// Tries to buy item in <see cref="UseShop"/>
        /// </summary>
        bool TryBuyItem(byte slot, byte count, out Item soldItem, out Item shopItem);

        /// <summary>
        /// Sells item.
        /// </summary>
        Item TrySellItem(byte slot, byte count);

        /// <summary>
        /// Event, that is fired, when number of sold items changes,
        /// i.e. when player opened shop window and see, that someone buys some item.
        /// </summary>
        event Action<byte, byte> OnSellItemCountChanged;

        /// <summary>
        /// Event, that is fired, when number of items in used shop changes.
        /// </summary>
        event Action<byte, byte> OnUseShopItemCountChanged;

        /// <summary>
        /// Event, that is fired, when some item is sold.
        /// </summary>
        event Action<byte, byte> OnSoldItem;
    }
}
