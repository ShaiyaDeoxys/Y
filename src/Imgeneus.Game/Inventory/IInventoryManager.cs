using Imgeneus.Database.Entities;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Inventory
{
    public interface IInventoryManager : ISessionedService, IDisposable
    {
        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems { get; }

        /// <summary>
        /// Is inventory full?
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Event, that is fired, when some equipment of character changes.
        /// </summary>
        event Action<uint, Item, byte> OnEquipmentChanged;

        /// <summary>
        /// Fires <see cref="OnEquipmentChanged"/>
        /// </summary>
        void RaiseEquipmentChanged(byte slot);

        /// <summary>
        /// Worm helmet.
        /// </summary>
        Item Helmet { get; set; }

        /// <summary>
        /// Worm armor.
        /// </summary>
        Item Armor { get; set; }

        /// <summary>
        /// Worm pants.
        /// </summary>
        Item Pants { get; set; }

        /// <summary>
        /// Worm gauntlet.
        /// </summary>
        Item Gauntlet { get; set; }

        /// <summary>
        /// Worm boots.
        /// </summary>
        Item Boots { get; set; }

        /// <summary>
        /// Worm weapon.
        /// </summary>
        Item Weapon { get; set; }

        /// <summary>
        /// Worm shield.
        /// </summary>
        Item Shield { get; set; }

        /// <summary>
        /// Worm cape.
        /// </summary>
        Item Cape { get; set; }

        /// <summary>
        /// Worm amulet.
        /// </summary>
        Item Amulet { get; set; }

        /// <summary>
        /// Worm ring1.
        /// </summary>
        Item Ring1 { get; set; }

        /// <summary>
        /// Worm ring2.
        /// </summary>
        Item Ring2 { get; set; }

        /// <summary>
        /// Worm bracelet1.
        /// </summary>
        Item Bracelet1 { get; set; }

        /// <summary>
        /// Worm bracelet2.
        /// </summary>
        Item Bracelet2 { get; set; }

        /// <summary>
        /// Worm mount.
        /// </summary>
        Item Mount { get; set; }

        /// <summary>
        /// Worm pet.
        /// </summary>
        Item Pet { get; set; }

        /// <summary>
        /// Worm costume.
        /// </summary>
        Item Costume { get; set; }

        /// <summary>
        /// Worm wings.
        /// </summary>
        Item Wings { get; set; }

        /// <summary>
        /// Inits character inventory with items from db.
        /// </summary>
        /// <param name="owner">character db id</param>
        /// <param name="items">items loaded from database</param>
        /// <param name="gold">gold amount from database</param>
        void Init(uint owner, IEnumerable<DbCharacterItems> items, uint gold);

        event Action<Item> OnAddItem;

        /// <summary>
        /// Adds item to player's inventory.
        /// </summary>
        /// <param name="item">ite, that we want to add</param>
        Item AddItem(Item item, string source, bool silent = false);

        event Action<Item, bool> OnRemoveItem;

        /// <summary>
        /// Removes item from inventory
        /// </summary>
        /// <param name="item">item, that we want to remove</param>
        Item RemoveItem(Item item, string source);

        /// <summary>
        /// Moves item inside inventory.
        /// </summary>
        /// <param name="currentBag">current bag id</param>
        /// <param name="currentSlot">current slot id</param>
        /// <param name="destinationBag">bag id, where item should be moved</param>
        /// <param name="destinationSlot">slot id, where item should be moved</param>
        public (Item sourceItem, Item destinationItem) MoveItem(byte currentBag, byte currentSlot, byte destinationBag, byte destinationSlot);

        /// <summary>
        /// Money, that belongs to player.
        /// </summary>
        uint Gold { get; set; }

        /// <summary>
        /// Event, that is fired, when player uses any item from inventory.
        /// </summary>
        event Action<uint, Item> OnUsedItem;

        /// <summary>
        /// Use item from inventory.
        /// </summary>
        /// <param name="bag">bag, where item is situated</param>
        /// <param name="slot">slot, where item is situated</param>
        /// <param name="targetId">id of another player</param>
        /// <param name="skipApplyingItemEffect">some items like lucky cms and hammers do not have any effect but are still "usable"</param>
        /// <param name="count">how many items are used at once</param>
        Task<bool> TryUseItem(byte bag, byte slot, uint? targetId = null, bool skipApplyingItemEffect = false, byte count = 1);

        /// <summary>
        /// Checks if item can be used. E.g. cooldown is over, required level is right etc.
        /// </summary>
        bool CanUseItem(Item item);

        /// <summary>
        /// Checks if item can be used on another player.
        /// </summary>
        bool CanUseItemOnTarget(Item item, uint targetId);

        /// <summary>
        /// Buys item from npc store.
        /// </summary>
        /// <param name="product">product to buy</param>
        /// <param name="discount">discount, e.g. in guild house npc can sell items cheaper</param>
        /// <param name="count">how many items player want to buy</param>
        Item BuyItem(NpcProduct product, byte count, float discount, out BuyResult result);

        /// <summary>
        /// Sells item.
        /// </summary>
        /// <param name="item">item to sell</param>
        /// <param name="count">how many item player want to sell</param>
        Item SellItem(Item item, byte count);

        /// <summary>
        /// Event, that is fired, when item expires.
        /// </summary>
        event Action<Item> OnItemExpired;

        /// <summary>
        /// TODO: why it's in inventory?
        /// </summary>
        bool TryResetStats();
    }
}
