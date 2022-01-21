using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Vehicle;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHealthManager _healthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IElementProvider _elementProvider;
        private readonly IVehicleManager _vehicleManager;
        private readonly ILevelProvider _levelProvider;
        private readonly ILevelingManager _levelingManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly ISkillsManager _skillsManager;
        private readonly IBuffsManager _buffsManager;
        private readonly ICharacterConfiguration _characterConfig;
        private int _ownerId;

        public InventoryManager(ILogger<InventoryManager> logger, IDatabasePreloader databasePreloader, IDatabase database, IGameSession gameSession, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager, IElementProvider elementProvider, IVehicleManager vehicleManager, ILevelProvider levelProvider, ILevelingManager levelingManager, ICountryProvider countryProvider, IGameWorld gameWorld, IAdditionalInfoManager additionalInfoManager, ISkillsManager skillsManager, IBuffsManager buffsManager, ICharacterConfiguration characterConfiguration)
        {
            _logger = logger;
            _databasePreloader = databasePreloader;
            _database = database;
            _gameSession = gameSession;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _speedManager = speedManager;
            _elementProvider = elementProvider;
            _vehicleManager = vehicleManager;
            _levelProvider = levelProvider;
            _levelingManager = levelingManager;
            _countryProvider = countryProvider;
            _gameWorld = gameWorld;
            _additionalInfoManager = additionalInfoManager;
            _skillsManager = skillsManager;
            _buffsManager = buffsManager;
            _characterConfig = characterConfiguration;
            _speedManager.OnPassiveModificatorChanged += SpeedManager_OnPassiveModificatorChanged;

#if DEBUG
            _logger.LogDebug("InventoryManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~InventoryManager()
        {
            _logger.LogDebug("InventoryManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, IEnumerable<DbCharacterItems> items, uint gold)
        {
            _ownerId = ownerId;

            foreach (var item in items.Select(i => new Item(_databasePreloader, i)))
                InventoryItems.TryAdd((item.Bag, item.Slot), item);

            Gold = gold;

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

        public async Task Clear()
        {
            var character = await _database.Characters.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == _ownerId);
            if (character is null)
                return;

            character.Gold = Gold;

            _database.CharacterItems.RemoveRange(character.Items);

            foreach (var item in InventoryItems.Values)
            {
                var dbItem = new DbCharacterItems()
                {
                    CharacterId = _ownerId,
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
            }

            await _database.SaveChangesAsync();

            InventoryItems.Clear();
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

        public void Dispose()
        {
            _speedManager.OnPassiveModificatorChanged -= SpeedManager_OnPassiveModificatorChanged;
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

                OnEquipmentChanged?.Invoke(_ownerId, _helmet, 0);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 0", _ownerId);
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

                if (_armor != null)
                {
                    _elementProvider.ConstDefenceElement = _armor.Element;
                }
                else
                {
                    _elementProvider.ConstDefenceElement = Element.None;
                }

                TakeOnItem(_armor);

                OnEquipmentChanged?.Invoke(_ownerId, _armor, 1);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 1", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _pants, 2);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 2", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _gauntlet, 3);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 3", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _boots, 4);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 4", _ownerId);
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

                if (_weapon != null)
                {
                    _speedManager.WeaponSpeedPassiveSkillModificator.TryGetValue(_weapon.ToPassiveSkillType(), out var passiveSkillModifier);
                    _speedManager.ConstAttackSpeed = _weapon.AttackSpeed + passiveSkillModifier;

                    _elementProvider.ConstAttackElement = _weapon.Element;
                }
                else
                {
                    _speedManager.ConstAttackSpeed = 0;

                    _elementProvider.ConstAttackElement = Element.None;
                }

                TakeOnItem(_weapon);

                OnEquipmentChanged?.Invoke(_ownerId, _weapon, 5);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 5", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _shield, 6);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 6", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _cape, 7);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 7", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _amulet, 8);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 8", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _ring1, 9);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 9", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _ring2, 10);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 10", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _bracelet1, 11);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 11", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _bracelet2, 12);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 12", _ownerId);
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
                _vehicleManager.RemoveVehicle();

                _mount = value;
                if (_mount != null)
                    _vehicleManager.SummoningTime = _mount.AttackSpeed > 0 ? _mount.AttackSpeed * 1000 : _mount.AttackSpeed + 1000;
                else
                    _vehicleManager.SummoningTime = 0;

                TakeOnItem(_mount);

                OnEquipmentChanged?.Invoke(_ownerId, _mount, 13);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 13", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _pet, 14);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 14", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _costume, 15);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 15", _ownerId);
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

                OnEquipmentChanged?.Invoke(_ownerId, _wings, 16);
                _statsManager.RaiseAdditionalStatsUpdate();
                _logger.LogDebug("Character {characterId} changed equipment on slot 16", _ownerId);
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
            _healthManager.ExtraHP -= item.HP;
            _healthManager.ExtraSP -= item.SP;
            _healthManager.ExtraMP -= item.MP;
            _statsManager.ExtraDefense -= item.Defense;
            _statsManager.ExtraResistance -= item.Resistance;
            _statsManager.Absorption -= item.Absorb;

            if (item == Weapon)
            {
                _statsManager.WeaponMinAttack -= item.MinAttack;
                _statsManager.WeaponMaxAttack -= item.MaxAttack;
            }

            if (item != Weapon && item != Mount)
                _speedManager.ExtraAttackSpeed -= item.AttackSpeed;

            if (item != Mount)
                _speedManager.ExtraMoveSpeed -= item.MoveSpeed;
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
            _healthManager.ExtraHP += item.HP;
            _healthManager.ExtraSP += item.SP;
            _healthManager.ExtraMP += item.MP;
            _statsManager.ExtraDefense += item.Defense;
            _statsManager.ExtraResistance += item.Resistance;
            _statsManager.Absorption += item.Absorb;

            if (item == Weapon)
            {
                _statsManager.WeaponMinAttack += item.MinAttack;
                _statsManager.WeaponMaxAttack += item.MaxAttack;
            }

            if (item != Weapon && item != Mount)
                _speedManager.ExtraAttackSpeed += item.AttackSpeed;

            if (item != Mount)
                _speedManager.ExtraMoveSpeed += item.MoveSpeed;
        }

        #endregion

        #region Inventory

        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        public ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems { get; private set; } = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();

        public event Action<Item> OnAddItem;

        public Item AddItem(Item item)
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

            InventoryItems.TryAdd((item.Bag, item.Slot), item);

            if (item.ExpirationTime != null)
            {
                //item.OnExpiration += CharacterItem_OnExpiration;
            }

            _logger.LogDebug("Character {characterId} got item {type} {typeId}", _ownerId, item.Type, item.TypeId);

            OnAddItem?.Invoke(item);
            return item;
        }

        public event Action<Item, bool> OnRemoveItem;

        public Item RemoveItem(Item item)
        {
            // If we are giving consumable item.
            if (item.TradeQuantity < item.Count && item.TradeQuantity != 0)
            {
                var givenItem = item.Clone();
                givenItem.Count = item.TradeQuantity;

                item.Count -= item.TradeQuantity;
                item.TradeQuantity = 0;

                OnRemoveItem?.Invoke(item, item.Count == 0);

                return givenItem;
            }

            InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);

            if (item.ExpirationTime != null)
            {
                item.StopExpirationTimer();
                //item.OnExpiration -= CharacterItem_OnExpiration;
            }

            _logger.LogDebug("Character {characterId} lost item {type} {typeId}", _ownerId, item.Type, item.TypeId);

            OnRemoveItem?.Invoke(item, true);
            return item;
        }

        public (Item sourceItem, Item destinationItem) MoveItem(byte sourceBag, byte sourceSlot, byte destinationBag, byte destinationSlot)
        {
            // Find source item.
            InventoryItems.TryRemove((sourceBag, sourceSlot), out var sourceItem);
            if (sourceItem is null)
            {
                // wrong packet, source item should be always presented.
                _logger.LogError("Could not find source item for player {characterId}", _ownerId);
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
                }
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

        #region Gold

        public uint Gold { get; set; }

        #endregion

        #region Use item

        private readonly Dictionary<byte, DateTime> _itemCooldowns = new Dictionary<byte, DateTime>();

        public event Action<int, Item> OnUsedItem;

        public async Task<bool> TryUseItem(byte bag, byte slot, int? targetId = null)
        {
            InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
            {
                _logger.LogWarning("Character {id} is trying to use item, that does not exist. Possible hack?", _ownerId);
                return false;
            }

            if (!CanUseItem(item))
            {
                //_packetsHelper.SendCanNotUseItem(Client, Id);
                return false;
            }

            if (targetId != null)
            {
                if (!CanUseItemOnTarget(item, (int)targetId))
                {
                    //_packetsHelper.SendCanNotUseItem(Client, Id);
                    return false;
                }
            }

            item.Count--;
            _itemCooldowns[item.ReqIg] = DateTime.UtcNow;
            await ApplyItemEffect(item, targetId);
            OnUsedItem?.Invoke(_ownerId, item);

            if (item.Count == 0)
            {
                InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);
            }

            return true;
        }

        public bool CanUseItem(Item item)
        {
            if (item.Special == SpecialEffect.None && item.HP == 0 && item.MP == 0 && item.SP == 0 && item.SkillId == 0)
                return false;

            if (item.Type == Item.GEM_ITEM_TYPE)
                return true;

            if (item.ReqIg != 0)
            {
                if (_itemCooldowns.ContainsKey(item.ReqIg) && Item.ReqIgToCooldownInMilliseconds.ContainsKey(item.ReqIg))
                {
                    if (DateTime.UtcNow.Subtract(_itemCooldowns[item.ReqIg]).TotalMilliseconds < Item.ReqIgToCooldownInMilliseconds[item.ReqIg])
                        return false;
                }
            }

            if (item.Reqlevel > _levelProvider.Level)
                return false;

            if (_levelingManager.Grow < item.Grow)
                return false;

            switch (item.ItemClassType)
            {
                case ItemClassType.Human:
                    if (_additionalInfoManager.Race != Race.Human)
                        return false;
                    break;

                case ItemClassType.Elf:
                    if (_additionalInfoManager.Race != Race.Elf)
                        return false;
                    break;

                case ItemClassType.AllLights:
                    if (_countryProvider.Country != CountryType.Light)
                        return false;
                    break;

                case ItemClassType.Deatheater:
                    if (_additionalInfoManager.Race != Race.DeathEater)
                        return false;
                    break;

                case ItemClassType.Vail:
                    if (_additionalInfoManager.Race != Race.Vail)
                        return false;
                    break;

                case ItemClassType.AllFury:
                    if (_countryProvider.Country != CountryType.Dark)
                        return false;
                    break;
            }

            if (item.ItemClassType != ItemClassType.AllFactions)
            {
                switch (_additionalInfoManager.Class)
                {
                    case CharacterProfession.Fighter:
                        if (!item.IsForFighter)
                            return false;
                        break;

                    case CharacterProfession.Defender:
                        if (!item.IsForDefender)
                            return false;
                        break;

                    case CharacterProfession.Ranger:
                        if (!item.IsForRanger)
                            return false;
                        break;

                    case CharacterProfession.Archer:
                        if (!item.IsForArcher)
                            return false;
                        break;

                    case CharacterProfession.Mage:
                        if (!item.IsForMage)
                            return false;
                        break;

                    case CharacterProfession.Priest:
                        if (!item.IsForPriest)
                            return false;
                        break;
                }
            }

            /*switch (item.Special)
            {
                case SpecialEffect.RecreationRune:
                case SpecialEffect.AbsoluteRecreationRune:
                case SpecialEffect.RecreationRune_STR:
                case SpecialEffect.RecreationRune_DEX:
                case SpecialEffect.RecreationRune_REC:
                case SpecialEffect.RecreationRune_INT:
                case SpecialEffect.RecreationRune_WIS:
                case SpecialEffect.RecreationRune_LUC:
                    return LinkingManager.Item != null && LinkingManager.Item.IsComposable;
            }*/

            return true;
        }

        public bool CanUseItemOnTarget(Item item, int targetId)
        {
            switch (item.Special)
            {
                case SpecialEffect.MovementRune:
                /*if (_gameWorld.Players.TryGetValue(targetId, out var target))
                {
                    if (target.Party != Party)
                        return false;

                    return _gameWorld.CanTeleport(this, target.MapId, out var reason);
                }
                else
                    return false;*/

                default:
                    return true;
            }
        }

        /// <summary>
        /// Adds the effect of the item to the character.
        /// </summary>
        private async Task ApplyItemEffect(Item item, int? targetId = null)
        {
            switch (item.Special)
            {
                case SpecialEffect.None:
                    if (item.HP > 0 || item.MP > 0 || item.SP > 0)
                        UseHealingPotion(item);

                    if (item.SkillId != 0)
                        _skillsManager.UseSkill(new Skill(_databasePreloader.Skills[(item.SkillId, item.SkillLevel)], ISkillsManager.ITEM_SKILL_NUMBER, 0), _gameWorld.Players[_ownerId]);

                    break;

                case SpecialEffect.PercentHealingPotion:
                    UsePercentHealingPotion(item);
                    break;

                case SpecialEffect.HypnosisCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    break;

                case SpecialEffect.StunCure:
                    UseCureDebuffPotion(StateType.Stun);
                    break;

                case SpecialEffect.SilenceCure:
                    UseCureDebuffPotion(StateType.Silence);
                    break;

                case SpecialEffect.DarknessCure:
                    UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.StopCure:
                    UseCureDebuffPotion(StateType.Immobilize);
                    break;

                case SpecialEffect.SlowCure:
                    UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.VenomCure:
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    break;

                case SpecialEffect.DiseaseCure:
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.IllnessDelusionCure:
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.SleepStunStopSlowCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    UseCureDebuffPotion(StateType.Stun);
                    UseCureDebuffPotion(StateType.Immobilize);
                    UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.SilenceDarknessCure:
                    UseCureDebuffPotion(StateType.Silence);
                    UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.DullBadLuckCure:
                    UseCureDebuffPotion(StateType.DexDecrease);
                    UseCureDebuffPotion(StateType.Misfortunate);
                    break;

                case SpecialEffect.DoomFearCure:
                    UseCureDebuffPotion(StateType.MentalSmasher);
                    UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.FullCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    UseCureDebuffPotion(StateType.Stun);
                    UseCureDebuffPotion(StateType.Silence);
                    UseCureDebuffPotion(StateType.Darkness);
                    UseCureDebuffPotion(StateType.Immobilize);
                    UseCureDebuffPotion(StateType.Slow);
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    UseCureDebuffPotion(StateType.DexDecrease);
                    UseCureDebuffPotion(StateType.Misfortunate);
                    UseCureDebuffPotion(StateType.MentalSmasher);
                    UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.DisorderCure:
                    // ?
                    break;

                case SpecialEffect.StatResetStone:
                    await TryResetStats();
                    break;

                case SpecialEffect.GoddessBlessing:
                    UseBlessItem();
                    break;

                case SpecialEffect.AppearanceChange:
                case SpecialEffect.SexChange:
                    // Used in ChangeAppearance call.
                    break;

                case SpecialEffect.LinkingHammer:
                case SpecialEffect.PerfectLinkingHammer:
                case SpecialEffect.RecreationRune:
                case SpecialEffect.AbsoluteRecreationRune:
                    // Effect is added in linking manager.
                    break;

                case SpecialEffect.Dye:
                    // Effect is handled in dyeing manager.
                    break;

                case SpecialEffect.NameChange:
                    await UseNameChangeStone();
                    break;

                case SpecialEffect.AnotherItemGenerator:
                    // TODO: Generate another item based on item ReqVg.
                    break;

                case SpecialEffect.SkillResetStone:
                    await _skillsManager.TryResetSkills();
                    break;

                case SpecialEffect.MovementRune:
                    //if (_gameWorld.Players.TryGetValue((int)targetId, out var target))
                    //    Teleport(target.Map.Id, target.PosX, target.PosY, target.PosZ);
                    break;

                default:
                    _logger.LogError($"Uninplemented item effect {item.Special}.");
                    break;
            }
        }

        /// <summary>
        /// Uses potion, that restores hp,sp,mp.
        /// </summary>
        private void UseHealingPotion(Item potion)
        {
            _healthManager.Recover(potion.HP, potion.MP, potion.SP);
        }

        /// <summary>
        /// Cures character from some debuff.
        /// </summary>
        private void UseCureDebuffPotion(StateType debuffType)
        {
            var debuffs = _buffsManager.ActiveBuffs.Where(b => b.StateType == debuffType).ToList();
            foreach (var d in debuffs)
            {
                d.CancelBuff();
            }
        }

        /// <summary>
        /// Uses potion, that restores % of hp,sp,mp.
        /// </summary>
        private void UsePercentHealingPotion(Item potion)
        {
            var hp = Convert.ToInt32(_healthManager.MaxHP * potion.HP / 100);
            var mp = Convert.ToInt32(_healthManager.MaxMP * potion.MP / 100);
            var sp = Convert.ToInt32(_healthManager.MaxSP * potion.SP / 100);

            _healthManager.Recover(hp, mp, sp);
        }

        /// <summary>
        /// Initiates name change process
        /// </summary>
        public async Task UseNameChangeStone()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
            {
                _logger.LogError("Character {id} is not found", _ownerId);
                return;
            }

            character.IsRename = true;
            await _database.SaveChangesAsync();
        }

        /// <summary>
        /// GM item ,that increases bless amount of player's fraction.
        /// </summary>
        private void UseBlessItem()
        {
            if (_countryProvider.Country == CountryType.Light)
                Bless.Instance.LightAmount += 500;
            else
                Bless.Instance.DarkAmount += 500;
        }

        private async Task<bool> TryResetStats()
        {
            var defaultStat = _characterConfig.DefaultStats.First(s => s.Job == _additionalInfoManager.Class);
            var statPerLevel = _characterConfig.GetLevelStatSkillPoints(_levelingManager.Grow).StatPoint;

            var ok = await _statsManager.TrySetStats(defaultStat.Str,
                                                     defaultStat.Dex,
                                                     defaultStat.Rec,
                                                     defaultStat.Int,
                                                     defaultStat.Wis,
                                                     defaultStat.Luc,
                                                     (ushort)((_levelProvider.Level - 1) * statPerLevel)); // Level - 1, because we are starting with 1 level.
            if (!ok)
                return false;

            await _levelingManager.IncreasePrimaryStat((ushort)(_levelProvider.Level - 1));

            _statsManager.RaiseResetStats();
            return ok;
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

        private void SpeedManager_OnPassiveModificatorChanged(byte weaponType, byte passiveSkillModifier, bool shouldAdd)
        {
            if (Weapon is null || passiveSkillModifier == 0 || weaponType != Weapon.ToPassiveSkillType())
                return;

            if (shouldAdd)
                _speedManager.ConstAttackSpeed += passiveSkillModifier;
            else
                _speedManager.ConstAttackSpeed -= passiveSkillModifier;

        }

        #endregion
    }
}
