using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Inventory
{
    public interface IInventoryManager : ISessionedService
    {
        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems { get; }

        /// <summary>
        /// Event, that is fired, when some equipment of character changes.
        /// </summary>
        event Action<int, Item, byte> OnEquipmentChanged;

        /// <summary>
        /// Worm helmet.
        /// </summary>
        Item Helmet { get; }

        /// <summary>
        /// Worm armor.
        /// </summary>
        Item Armor { get; }

        /// <summary>
        /// Worm pants.
        /// </summary>
        Item Pants { get; }

        /// <summary>
        /// Worm gauntlet.
        /// </summary>
        Item Gauntlet { get; }

        /// <summary>
        /// Worm boots.
        /// </summary>
        Item Boots { get; }

        /// <summary>
        /// Worm weapon.
        /// </summary>
        Item Weapon { get; }

        /// <summary>
        /// Worm shield.
        /// </summary>
        Item Shield { get; }

        /// <summary>
        /// Worm cape.
        /// </summary>
        Item Cape { get; }

        /// <summary>
        /// Worm amulet.
        /// </summary>
        Item Amulet { get; }

        /// <summary>
        /// Worm ring1.
        /// </summary>
        Item Ring1 { get; }

        /// <summary>
        /// Worm ring2.
        /// </summary>
        Item Ring2 { get; }

        /// <summary>
        /// Worm bracelet1.
        /// </summary>
        Item Bracelet1 { get; }

        /// <summary>
        /// Worm bracelet2.
        /// </summary>
        Item Bracelet2 { get; }

        /// <summary>
        /// Worm mount.
        /// </summary>
        Item Mount { get; }

        /// <summary>
        /// Worm pet.
        /// </summary>
        Item Pet { get; }

        /// <summary>
        /// Worm costume.
        /// </summary>
        Item Costume { get; }

        /// <summary>
        /// Worm wings.
        /// </summary>
        Item Wings { get; }

        /// <summary>
        /// Inits character inventory with items from db.
        /// </summary>
        /// <param name="owner">character db id</param>
        /// <param name="items">items loaded from database</param>
        /// <param name="gold">gold amount from database</param>
        void Init(int owner, IEnumerable<DbCharacterItems> items, uint gold);

        /// <summary>
        /// Adds item to player's inventory.
        /// </summary>
        /// <param name="item">ite, that we want to add</param>
        Task<Item> AddItem(Item item);

        /// <summary>
        /// Removes item from inventory
        /// </summary>
        /// <param name="item">item, that we want to remove</param>
        Task<Item> RemoveItem(Item item);

        /// <summary>
        /// Moves item inside inventory.
        /// </summary>
        /// <param name="currentBag">current bag id</param>
        /// <param name="currentSlot">current slot id</param>
        /// <param name="destinationBag">bag id, where item should be moved</param>
        /// <param name="destinationSlot">slot id, where item should be moved</param>
        public Task<(Item sourceItem, Item destinationItem)> MoveItem(byte currentBag, byte currentSlot, byte destinationBag, byte destinationSlot);

        /// <summary>
        /// Money, that belongs to player.
        /// </summary>
        uint Gold { get; set; }
    }
}
