using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Inventory
{
    public class InventoryManager : IInventoryManager
    {
        private readonly ILogger _logger;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IDatabase _database;
        private readonly IGameSession _gameSession;
        private readonly IStatsManager _statsManager;
        public InventoryManager(ILogger<InventoryManager> logger, IDatabasePreloader databasePreloader, IDatabase database, IGameSession gameSession, IStatsManager statsManager)
        {
            _logger = logger;
            _databasePreloader = databasePreloader;
            _database = database;
            _gameSession = gameSession;
            _statsManager = statsManager;

#if DEBUG
            _logger.LogDebug($"InventoryManager {GetHashCode()} created");
#endif
        }

#if DEBUG
        ~InventoryManager()
        {
            _logger.LogDebug($"InventoryManager {GetHashCode()} collected by GC");
        }
#endif

        #region Init & Clear

        public void Init(IEnumerable<DbCharacterItems> items)
        {
            Clear();

            foreach (var item in items.Select(i => new Item(_databasePreloader, i)))
                InventoryItems.TryAdd((item.Bag, item.Slot), item);

            InitEquipment();
        }

        /// <summary>
        /// Initializes equipped items.
        /// </summary>
        private void InitEquipment()
        {
            Item item;

            InventoryItems.TryGetValue((0, 0), out item);
            Helmet = item;

            InventoryItems.TryGetValue((0, 1), out item);
            Armor = item;

            InventoryItems.TryGetValue((0, 2), out item);
            Pants = item;

            InventoryItems.TryGetValue((0, 3), out item);
            Gauntlet = item;

            InventoryItems.TryGetValue((0, 4), out item);
            Boots = item;

            InventoryItems.TryGetValue((0, 5), out item);
            Weapon = item;

            InventoryItems.TryGetValue((0, 6), out item);
            Shield = item;

            InventoryItems.TryGetValue((0, 7), out item);
            Cape = item;

            InventoryItems.TryGetValue((0, 8), out item);
            Amulet = item;

            InventoryItems.TryGetValue((0, 9), out item);
            Ring1 = item;

            InventoryItems.TryGetValue((0, 10), out item);
            Ring2 = item;

            InventoryItems.TryGetValue((0, 11), out item);
            Bracelet1 = item;

            InventoryItems.TryGetValue((0, 12), out item);
            Bracelet2 = item;

            InventoryItems.TryGetValue((0, 13), out item);
            Mount = item;

            InventoryItems.TryGetValue((0, 14), out item);
            Pet = item;

            InventoryItems.TryGetValue((0, 15), out item);
            Costume = item;

            InventoryItems.TryGetValue((0, 16), out item);
            Wings = item;
        }

        public void Clear()
        {
            InventoryItems.Clear();
        }

        public void Dispose()
        {
            Clear();
            Helmet = null;
            Armor = null;
            Pants = null;
            Gauntlet = null;
            Boots = null;
            Weapon = null;
            Shield = null;
            Cape = null;
            Amulet = null;
            Ring1 = null;
            Ring2 = null;
            Bracelet1 = null;
            Bracelet2 = null;
            Mount = null;
            Pet = null;
            Costume = null;
            Wings = null;
        }

        #endregion

        #region Equipment

        /// <summary>
        /// Event, that is fired, when some equipment of character changes.
        /// </summary>
        public event Action<int, Item, byte> OnEquipmentChanged;

        private Item _helmet;
        public Item Helmet
        {
            get => _helmet;
            set
            {

                TakeOffItem(_helmet);
                _helmet = value;
                TakeOnItem(_helmet);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _helmet, 0);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 0");
            }
        }

        private Item _armor;
        public Item Armor
        {
            get => _armor;
            set
            {
                TakeOffItem(_armor);
                _armor = value;
                TakeOnItem(_armor);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _armor, 1);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 1");
            }
        }

        private Item _pants;
        public Item Pants
        {
            get => _pants;
            set
            {
                TakeOffItem(_pants);
                _pants = value;
                TakeOnItem(_pants);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _pants, 2);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 2");
            }
        }

        private Item _gauntlet;
        public Item Gauntlet
        {
            get => _gauntlet;
            set
            {
                TakeOffItem(_gauntlet);
                _gauntlet = value;
                TakeOnItem(_gauntlet);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _gauntlet, 3);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 3");
            }
        }

        private Item _boots;
        public Item Boots
        {
            get => _boots;
            set
            {
                TakeOffItem(_boots);
                _boots = value;
                TakeOnItem(_boots);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _boots, 4);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 4");
            }
        }

        private Item _weapon;
        public Item Weapon
        {
            get => _weapon;
            set
            {
                TakeOffItem(_weapon);
                _weapon = value;

                //if (_weapon != null)
                //    SetWeaponSpeed(_weapon.AttackSpeed);
                //else
                //    SetWeaponSpeed(0);

                TakeOnItem(_weapon);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _weapon, 5);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 5");
            }
        }

        private Item _shield;
        public Item Shield
        {
            get => _shield;
            set
            {
                TakeOffItem(_shield);
                _shield = value;
                TakeOnItem(_shield);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _shield, 6);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 6");
            }
        }

        private Item _cape;
        public Item Cape
        {
            get => _cape;
            set
            {
                TakeOffItem(_cape);
                _cape = value;
                TakeOnItem(_cape);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _cape, 7);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 7");
            }
        }

        private Item _amulet;
        public Item Amulet
        {
            get => _amulet;
            set
            {
                TakeOffItem(_amulet);
                _amulet = value;
                TakeOnItem(_amulet);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _amulet, 8);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 8");
            }
        }

        private Item _ring1;
        public Item Ring1
        {
            get => _ring1;
            set
            {
                TakeOffItem(_ring1);
                _ring1 = value;
                TakeOnItem(_ring1);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _ring1, 9);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 9");
            }
        }

        private Item _ring2;
        public Item Ring2
        {
            get => _ring2;
            set
            {
                TakeOffItem(_ring2);
                _ring2 = value;
                TakeOnItem(_ring2);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _ring2, 10);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 10");
            }
        }

        private Item _bracelet1;
        public Item Bracelet1
        {
            get => _bracelet1;
            set
            {
                TakeOffItem(_bracelet1);
                _bracelet1 = value;
                TakeOnItem(_bracelet1);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _bracelet1, 11);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 11");
            }
        }

        private Item _bracelet2;
        public Item Bracelet2
        {
            get => _bracelet2;
            set
            {
                TakeOffItem(_bracelet2);
                _bracelet2 = value;
                TakeOnItem(_bracelet2);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _bracelet2, 12);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 12");
            }
        }

        private Item _mount;
        public Item Mount
        {
            get => _mount;
            set
            {
                TakeOffItem(_mount);

                // Remove mount if user was mounted while switching mount
                //RemoveVehicle();

                _mount = value;
                TakeOnItem(_mount);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _mount, 13);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 13");
            }
        }

        private Item _pet;
        public Item Pet
        {
            get => _pet;
            set
            {
                TakeOffItem(_pet);
                _pet = value;
                TakeOnItem(_pet);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _pet, 14);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 14");
            }
        }

        private Item _costume;
        public Item Costume
        {
            get => _costume;
            set
            {
                TakeOffItem(_costume);
                _costume = value;
                TakeOnItem(_costume);

                //if (Client != null)
                //    SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _costume, 15);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 15");
            }
        }

        private Item _wings;
        public Item Wings
        {
            get => _wings;
            set
            {
                TakeOffItem(_wings);
                _wings = value;
                TakeOnItem(_wings);

                //if (Client != null)
                //SendAdditionalStats();

                OnEquipmentChanged?.Invoke(_gameSession.CharId, _wings, 16);
                _logger.LogDebug($"Character {_gameSession.CharId} changed equipment on slot 16");
            }
        }

        /// <summary>
        /// Method, that is called, when character takes off some equipped item.
        /// </summary>
        private void TakeOffItem(Item item)
        {
            if (item is null)
                return;

            _statsManager.ExtraStr -= item.Str;
            _statsManager.ExtraDex -= item.Dex;
            _statsManager.ExtraRec -= item.Rec;
            _statsManager.ExtraInt -= item.Int;
            _statsManager.ExtraLuc -= item.Luc;
            _statsManager.ExtraWis -= item.Wis;
            _statsManager.ExtraHP -= item.HP;
            _statsManager.ExtraSP -= item.SP;
            _statsManager.ExtraMP -= item.MP;
            _statsManager.ExtraDefense -= item.Defense;
            _statsManager.ExtraResistance -= item.Resistance;
            _statsManager.Absorption -= item.Absorb;

            /* if (item != Weapon && item != Mount)
                 SetAttackSpeedModifier(-1 * item.AttackSpeed);

             if (item != Mount)
                 MoveSpeed -= item.MoveSpeed;*/
        }

        /// <summary>
        /// Method, that is called, when character takes on some item.
        /// </summary>
        private void TakeOnItem(Item item)
        {
            if (item is null)
                return;

            _statsManager.ExtraStr += item.Str;
            _statsManager.ExtraDex += item.Dex;
            _statsManager.ExtraRec += item.Rec;
            _statsManager.ExtraInt += item.Int;
            _statsManager.ExtraLuc += item.Luc;
            _statsManager.ExtraWis += item.Wis;
            _statsManager.ExtraHP += item.HP;
            _statsManager.ExtraSP += item.SP;
            _statsManager.ExtraMP += item.MP;
            _statsManager.ExtraDefense += item.Defense;
            _statsManager.ExtraResistance += item.Resistance;
            _statsManager.Absorption += item.Absorb;

            /*if (item != Weapon && item != Mount)
                SetAttackSpeedModifier(item.AttackSpeed);

            if (item != Mount)
                MoveSpeed += item.MoveSpeed;*/
        }

        #endregion

        #region Inventory

        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        public ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems { get; private set; } = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();

        public async Task<Item> AddItem(Item item)
        {
            // Find free space.
            var free = FindFreeSlotInInventory();

            // Calculated bag slot can not be 0, because 0 means worn item. Newly created item can not be worn.
            if (free.Bag == 0 || free.Slot == -1)
            {
                return null;
            }

            item.Bag = free.Bag;
            item.Slot = (byte)free.Slot;

            var dbItem = new DbCharacterItems()
            {
                CharacterId = _gameSession.CharId,
                Type = item.Type,
                TypeId = item.TypeId,
                Count = item.Count,
                Quality = item.Quality,
                Bag = item.Bag,
                Slot = item.Slot,
                GemTypeId1 = item.Gem1 is null ? 0 : item.Gem1.TypeId,
                GemTypeId2 = item.Gem2 is null ? 0 : item.Gem2.TypeId,
                GemTypeId3 = item.Gem3 is null ? 0 : item.Gem3.TypeId,
                GemTypeId4 = item.Gem4 is null ? 0 : item.Gem4.TypeId,
                GemTypeId5 = item.Gem5 is null ? 0 : item.Gem5.TypeId,
                GemTypeId6 = item.Gem6 is null ? 0 : item.Gem6.TypeId,
                HasDyeColor = item.DyeColor.IsEnabled,
                DyeColorAlpha = item.DyeColor.Alpha,
                DyeColorSaturation = item.DyeColor.Saturation,
                DyeColorR = item.DyeColor.R,
                DyeColorG = item.DyeColor.G,
                DyeColorB = item.DyeColor.B,
                CreationTime = item.CreationTime,
                ExpirationTime = item.ExpirationTime
            };

            _database.CharacterItems.Add(dbItem);

            var count = await _database.SaveChangesAsync();
            if (count != 1)
            {
                _logger.LogError($"Could not crete item for player {_gameSession.CharId}");
                return null;
            }

            InventoryItems.TryAdd((item.Bag, item.Slot), item);

            if (item.ExpirationTime != null)
            {
                //item.OnExpiration += CharacterItem_OnExpiration;
            }

            _logger.LogDebug($"Character {_gameSession.CharId} got item {item.Type} {item.TypeId}");
            return item;
        }

        public async Task<Item> RemoveItem(Item item)
        {
            var dbItem = _database.CharacterItems.FirstOrDefault(x => x.CharacterId == _gameSession.CharId && x.Bag == item.Bag && x.Slot == item.Slot);
            if (dbItem is null)
            {
                _logger.LogError($"Could not find item count during remove for character {_gameSession.CharId}");
                return null;
            }

            // If we are giving consumable item.
            if (item.TradeQuantity < item.Count && item.TradeQuantity != 0)
            {
                var givenItem = item.Clone();
                givenItem.Count = item.TradeQuantity;

                item.Count -= item.TradeQuantity;
                item.TradeQuantity = 0;


                dbItem.Count = item.Count;
                await _database.SaveChangesAsync();

                return givenItem;
            }

            _database.CharacterItems.Remove(dbItem);
            var count = await _database.SaveChangesAsync();
            if (count != 1)
            {
                _logger.LogError($"Could not remove item for character {_gameSession.CharId}");
                return null;
            }

            InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);

            if (item.ExpirationTime != null)
            {
                item.StopExpirationTimer();
                //item.OnExpiration -= CharacterItem_OnExpiration;
            }

            _logger.LogDebug($"Character {_gameSession.CharId} lost item {item.Type} {item.TypeId}");
            return item;
        }

        public async Task<(Item sourceItem, Item destinationItem)> MoveItem(byte sourceBag, byte sourceSlot, byte destinationBag, byte destinationSlot)
        {
            bool swapping = false;

            // Find source item.
            InventoryItems.TryRemove((sourceBag, sourceSlot), out var sourceItem);
            if (sourceItem is null)
            {
                // wrong packet, source item should be always presented.
                _logger.LogError($"Could not find source item for player {_gameSession.CharId}");
                return (null, null);
            }

            // Check, if any other item is at destination slot.
            InventoryItems.TryRemove((destinationBag, destinationSlot), out var destinationItem);
            if (destinationItem is null)
            {
                // No item at destination place.
                // Since there is no destination item we will use source item as destination.
                // The only change, that we need to do is to set new bag and slot.
                destinationItem = sourceItem;
                destinationItem.Bag = destinationBag;
                destinationItem.Slot = destinationSlot;

                sourceItem = new Item(_databasePreloader, 0, 0) { Bag = sourceBag, Slot = sourceSlot }; // empty item.
            }
            else
            {
                // There is some item at destination place.
                if (sourceItem.Type == destinationItem.Type &&
                    sourceItem.TypeId == destinationItem.TypeId &&
                    destinationItem.IsJoinable &&
                    destinationItem.Count + sourceItem.Count <= destinationItem.MaxCount)
                {
                    // Increase destination item count, if they are joinable.
                    destinationItem.Count += sourceItem.Count;

                    sourceItem = new Item(_databasePreloader, 0, 0) { Bag = sourceBag, Slot = sourceSlot }; // empty item.
                }
                else
                {
                    // Swap them.
                    destinationItem.Bag = sourceBag;
                    destinationItem.Slot = sourceSlot;

                    sourceItem.Bag = destinationBag;
                    sourceItem.Slot = destinationSlot;
                    swapping = true;
                }
            }

            // Add new items to database.
            var dbSourceItem = _database.CharacterItems.FirstOrDefault(x => x.Bag == sourceBag && x.Slot == sourceSlot && x.CharacterId == _gameSession.CharId);
            var dbDestinationItem = _database.CharacterItems.FirstOrDefault(x => x.Bag == destinationBag && x.Slot == destinationSlot && x.CharacterId == _gameSession.CharId);

            dbSourceItem.Bag = destinationBag;
            dbSourceItem.Slot = destinationSlot;

            if (swapping)
            {
                dbDestinationItem.Bag = sourceBag;
                dbDestinationItem.Slot = sourceSlot;
            }
            else
            {
                dbSourceItem.Count = destinationItem.Count;

                if (dbDestinationItem != null)
                    _database.CharacterItems.Remove(dbDestinationItem);
            }

            var count = await _database.SaveChangesAsync();
            if ((!swapping && count < 1) || (swapping && count != 2))
            {
                _logger.LogError($"Could not move item for player {_gameSession.CharId}");
                return (null, null);
            }

            // Update equipment if needed.
            if (sourceBag == 0 && destinationBag != 0)
            {
                switch (sourceSlot)
                {
                    case 0:
                        Helmet = null;
                        break;
                    case 1:
                        Armor = null;
                        break;
                    case 2:
                        Pants = null;
                        break;
                    case 3:
                        Gauntlet = null;
                        break;
                    case 4:
                        Boots = null;
                        break;
                    case 5:
                        Weapon = null;
                        break;
                    case 6:
                        Shield = null;
                        break;
                    case 7:
                        Cape = null;
                        break;
                    case 8:
                        Amulet = null;
                        break;
                    case 9:
                        Ring1 = null;
                        break;
                    case 10:
                        Ring2 = null;
                        break;
                    case 11:
                        Bracelet1 = null;
                        break;
                    case 12:
                        Bracelet2 = null;
                        break;
                    case 13:
                        Mount = null;
                        break;
                    case 14:
                        Pet = null;
                        break;
                    case 15:
                        Costume = null;
                        break;
                    case 16:
                        Wings = null;
                        break;
                }
            }

            if (destinationBag == 0)
            {
                var item = sourceItem.Bag == destinationBag && sourceItem.Slot == destinationSlot ? sourceItem : destinationItem;
                switch (item.Slot)
                {
                    case 0:
                        Helmet = item;
                        break;
                    case 1:
                        Armor = item;
                        break;
                    case 2:
                        Pants = item;
                        break;
                    case 3:
                        Gauntlet = item;
                        break;
                    case 4:
                        Boots = item;
                        break;
                    case 5:
                        Weapon = item;
                        break;
                    case 6:
                        Shield = item;
                        break;
                    case 7:
                        Cape = item;
                        break;
                    case 8:
                        Amulet = item;
                        break;
                    case 9:
                        Ring1 = item;
                        break;
                    case 10:
                        Ring2 = item;
                        break;
                    case 11:
                        Bracelet1 = item;
                        break;
                    case 12:
                        Bracelet2 = item;
                        break;
                    case 13:
                        Mount = item;
                        break;
                    case 14:
                        Pet = item;
                        break;
                    case 15:
                        Costume = item;
                        break;
                    case 16:
                        Wings = item;
                        break;
                }
            }

            if (sourceItem.Type != 0 && sourceItem.TypeId != 0)
                InventoryItems.TryAdd((sourceItem.Bag, sourceItem.Slot), sourceItem);

            if (destinationItem.Type != 0 && destinationItem.TypeId != 0)
                InventoryItems.TryAdd((destinationItem.Bag, destinationItem.Slot), destinationItem);

            return (sourceItem, destinationItem);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Tries to find free slot in inventory.
        /// </summary>
        /// <returns>tuple of bag and slot; slot is -1 if there is no free slot</returns>
        private (byte Bag, int Slot) FindFreeSlotInInventory()
        {
            byte bagSlot = 0;
            int freeSlot = -1;

            if (InventoryItems.Count > 0)
            {
                var maxBag = 5;
                var maxSlots = 24;

                // Go though all bags and try to find any free slot.
                // Start with 1, because 0 is worn items.
                for (byte i = 1; i <= maxBag; i++)
                {
                    var bagItems = InventoryItems.Where(itm => itm.Value.Bag == i).OrderBy(b => b.Value.Slot);
                    for (var j = 0; j < maxSlots; j++)
                    {
                        if (!bagItems.Any(b => b.Value.Slot == j))
                        {
                            freeSlot = j;
                            break;
                        }
                    }

                    if (freeSlot != -1)
                    {
                        bagSlot = i;
                        break;
                    }
                }
            }
            else
            {
                bagSlot = 1; // Start with 1, because 0 is worn items.
                freeSlot = 0;
            }

            return (bagSlot, freeSlot);
        }

        #endregion
    }
}
