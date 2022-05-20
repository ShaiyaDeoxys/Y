using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Shop
{
    public interface IShopManager : ISessionedService
    {
        void Init(int ownerId);

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
        bool TryAddItem(byte bag, byte slot, byte shopSlot, int price);

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
        event Action<int, string> OnShopStarted;

        /// <summary>
        /// Event, that is fired, when shop is closed.
        /// </summary>
        event Action<int> OnShopFinished;
    }
}
