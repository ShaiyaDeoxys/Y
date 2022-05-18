using System;

namespace Imgeneus.World.Game.Shop
{
    public interface IShopManager
    {
        void Init(int ownerId);

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
        void Begin();

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
        /// Event, that is fired, when shop is started.
        /// </summary>
        event Action<int, string> OnShopStarted;
    }
}
