using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Blessing;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.Logs;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Game.Inventory
{
    public class InventoryManager : IInventoryManager
    {
        public const byte MAX_BAG = 5;
        public const byte MAX_SLOT = 24;

        private readonly ILogger _logger;
        private readonly IGameDefinitionsPreloder _gameDefinitions;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly IDatabase _database;
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
        private readonly IAttackManager _attackManager;
        private readonly IPartyManager _partyManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IChatManager _chatManager;
        private readonly IWarehouseManager _warehouseManager;
        private readonly IBlessManager _blessManager;
        private readonly ILogsManager _logsManager;
        private uint _ownerId;

        public InventoryManager(ILogger<InventoryManager> logger, IGameDefinitionsPreloder gameDefinitions, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, IDatabase database, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager, IElementProvider elementProvider, IVehicleManager vehicleManager, ILevelProvider levelProvider, ILevelingManager levelingManager, ICountryProvider countryProvider, IGameWorld gameWorld, IAdditionalInfoManager additionalInfoManager, ISkillsManager skillsManager, IBuffsManager buffsManager, ICharacterConfiguration characterConfiguration, IAttackManager attackManager, IPartyManager partyManager, ITeleportationManager teleportationManager, IChatManager chatManager, IWarehouseManager warehouseManager, IBlessManager blessManager, ILogsManager logsManager)
        {
            _logger = logger;
            _gameDefinitions = gameDefinitions;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _database = database;
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
            _attackManager = attackManager;
            _partyManager = partyManager;
            _teleportationManager = teleportationManager;
            _chatManager = chatManager;
            _warehouseManager = warehouseManager;
            _blessManager = blessManager;
            _logsManager = logsManager;
            _speedManager.OnPassiveModificatorChanged += SpeedManager_OnPassiveModificatorChanged;
            _partyManager.OnSummoned += PartyManager_OnSummoned;
            _teleportationManager.OnCastingTeleportFinished += TeleportationManager_OnCastingTeleportFinished;

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

        public void Init(uint ownerId, IEnumerable<DbCharacterItems> items, uint gold)
        {
            _ownerId = ownerId;

            foreach (var item in items.Select(x => new Item(_gameDefinitions, _enchantConfig, _itemCreateConfig, x)))
            {
                InventoryItems.TryAdd((item.Bag, item.Slot), item);

                if (item.IsExpirable)
                    item.OnExpiration += Item_OnExpiration;
            }

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
                    ExpirationTime = item.ExpirationTime,
                    Craftname = item.GetCraftName()
                };

                _database.CharacterItems.Add(dbItem);

                item.OnExpiration -= Item_OnExpiration;
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

            _itemCooldowns.Clear();
        }

        public void Dispose()
        {
            _speedManager.OnPassiveModificatorChanged -= SpeedManager_OnPassiveModificatorChanged;
            _partyManager.OnSummoned -= PartyManager_OnSummoned;
            _teleportationManager.OnCastingTeleportFinished -= TeleportationManager_OnCastingTeleportFinished;
        }

        #endregion

        #region Equipment

        /// <summary>
        /// Event, that is fired, when some equipment of character changes.
        /// </summary>
        public event Action<uint, Item, byte> OnEquipmentChanged;

        private Item _helmet;
        public Item Helmet
        {
            get => _helmet;
            set
            {

                TakeOffItem(_helmet);
                _helmet = value;
                TakeOnItem(_helmet);

                RaiseEquipmentChanged(0);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(1);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(2);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(3);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(4);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                    _attackManager.IsWeaponAvailable = true;
                    _attackManager.WeaponAttackRange = _weapon.AttackRange;
                    _statsManager.WeaponType = _weapon.Type;
                }
                else
                {
                    _speedManager.ConstAttackSpeed = 0;

                    _elementProvider.ConstAttackElement = Element.None;

                    _attackManager.IsWeaponAvailable = false;
                    _attackManager.WeaponAttackRange = 0;
                    _statsManager.WeaponType = 0;
                }

                TakeOnItem(_weapon);

                RaiseEquipmentChanged(5);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                if (_shield != null)
                {
                    _attackManager.IsShieldAvailable = true;
                }
                else
                {
                    _attackManager.IsShieldAvailable = false;
                }

                TakeOnItem(_shield);

                RaiseEquipmentChanged(6);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(7);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(8);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(9);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(10);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(11);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(12);
                _statsManager.RaiseAdditionalStatsUpdate();
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
                _vehicleManager.Mount = _mount;
                if (_mount != null)
                    _vehicleManager.SummoningTime = _mount.AttackSpeed > 0 ? _mount.AttackSpeed * 1000 : _mount.AttackSpeed + 1000;
                else
                    _vehicleManager.SummoningTime = 0;

                TakeOnItem(_mount);

                RaiseEquipmentChanged(13);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(14);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(15);
                _statsManager.RaiseAdditionalStatsUpdate();
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

                RaiseEquipmentChanged(16);
                _statsManager.RaiseAdditionalStatsUpdate();
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


            if (item == Weapon)
            {
                _statsManager.WeaponMinAttack -= item.MinAttack;
                _statsManager.WeaponMaxAttack -= item.MaxAttack;
            }
            else
                _statsManager.Absorption -= item.Absorb;

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

            if (item == Weapon)
            {
                _statsManager.WeaponMinAttack += item.MinAttack;
                _statsManager.WeaponMaxAttack += item.MaxAttack;
            }
            else
                _statsManager.Absorption += item.Absorb;

            if (item != Weapon && item != Mount)
                _speedManager.ExtraAttackSpeed += item.AttackSpeed;

            if (item != Mount)
                _speedManager.ExtraMoveSpeed += item.MoveSpeed;
        }

        public void RaiseEquipmentChanged(byte slot)
        {
            switch (slot)
            {
                case 0:
                    OnEquipmentChanged?.Invoke(_ownerId, _helmet, slot);
                    break;

                case 1:
                    OnEquipmentChanged?.Invoke(_ownerId, _armor, slot);
                    break;

                case 2:
                    OnEquipmentChanged?.Invoke(_ownerId, _pants, slot);
                    break;

                case 3:
                    OnEquipmentChanged?.Invoke(_ownerId, _gauntlet, slot);
                    break;

                case 4:
                    OnEquipmentChanged?.Invoke(_ownerId, _boots, slot);
                    break;

                case 5:
                    OnEquipmentChanged?.Invoke(_ownerId, _weapon, slot);
                    break;

                case 6:
                    OnEquipmentChanged?.Invoke(_ownerId, _shield, slot);
                    break;

                case 7:
                    OnEquipmentChanged?.Invoke(_ownerId, _cape, slot);
                    break;

                case 8:
                    OnEquipmentChanged?.Invoke(_ownerId, _amulet, slot);
                    break;

                case 9:
                    OnEquipmentChanged?.Invoke(_ownerId, _ring1, slot);
                    break;

                case 10:
                    OnEquipmentChanged?.Invoke(_ownerId, _ring2, slot);
                    break;

                case 11:
                    OnEquipmentChanged?.Invoke(_ownerId, _bracelet1, slot);
                    break;

                case 12:
                    OnEquipmentChanged?.Invoke(_ownerId, _bracelet2, slot);
                    break;

                case 13:
                    OnEquipmentChanged?.Invoke(_ownerId, _mount, slot);
                    break;

                case 14:
                    OnEquipmentChanged?.Invoke(_ownerId, _pet, slot);
                    break;

                case 15:
                    OnEquipmentChanged?.Invoke(_ownerId, _costume, slot);
                    break;

                case 16:
                    OnEquipmentChanged?.Invoke(_ownerId, _wings, slot);
                    break;
            }

            _logger.LogDebug("Character {characterId} changed equipment on slot {slot}", _ownerId, slot);
        }

        #endregion

        #region Inventory

        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        public ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems { get; private set; } = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();

        public event Action<Item> OnAddItem;

        public bool IsFull
        {
            get
            {
                var free = FindFreeSlotInInventory();
                if (free.Bag == 0 || free.Slot == -1)
                    return true;
                else
                    return false;
            }
        }

        private object _syncAddRemoveItem = new();

        public Item AddItem(Item item, string source, bool silent = false)
        {
            lock (_syncAddRemoveItem)
            {
                if (item.Type == 0 || item.TypeId == 0)
                {
                    _logger.LogError("Wrong item type or id.");
                    return null;
                }
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
                    item.OnExpiration += Item_OnExpiration;
                }

                _logger.LogDebug("Character {characterId} got item {type} {typeId}", _ownerId, item.Type, item.TypeId);
                _logsManager.LogAddItem(_ownerId,
                    item.Type,
                    item.TypeId,
                    item.Count,
                    item.Gem1 is null ? 0 : item.Gem1.TypeId,
                    item.Gem2 is null ? 0 : item.Gem2.TypeId,
                    item.Gem3 is null ? 0 : item.Gem3.TypeId,
                    item.Gem4 is null ? 0 : item.Gem4.TypeId,
                    item.Gem5 is null ? 0 : item.Gem5.TypeId,
                    item.Gem6 is null ? 0 : item.Gem6.TypeId,
                    item.DyeColor.IsEnabled,
                    item.DyeColor.R,
                    item.DyeColor.G,
                    item.DyeColor.B,
                    item.GetCraftName(),
                    source);

                if (!silent)
                    OnAddItem?.Invoke(item);

                return item;
            }
        }

        public event Action<Item, bool> OnRemoveItem;

        public Item RemoveItem(Item item, string source)
        {
            lock (_syncAddRemoveItem)
            {
                // If we are giving consumable item.
                if (item.TradeQuantity < item.Count && item.TradeQuantity != 0)
                {
                    var givenItem = item.Clone();
                    givenItem.Count = item.TradeQuantity;

                    item.Count -= item.TradeQuantity;
                    item.TradeQuantity = 0;

                    OnRemoveItem?.Invoke(item, item.Count == 0);
                    _logsManager.LogRemoveItem(_ownerId,
                        item.Type,
                        item.TypeId,
                        item.Count,
                        item.Gem1 is null ? 0 : item.Gem1.TypeId,
                        item.Gem2 is null ? 0 : item.Gem2.TypeId,
                        item.Gem3 is null ? 0 : item.Gem3.TypeId,
                        item.Gem4 is null ? 0 : item.Gem4.TypeId,
                        item.Gem5 is null ? 0 : item.Gem5.TypeId,
                        item.Gem6 is null ? 0 : item.Gem6.TypeId,
                        item.DyeColor.IsEnabled,
                        item.DyeColor.R,
                        item.DyeColor.G,
                        item.DyeColor.B,
                        item.GetCraftName(),
                        source);

                    return givenItem;
                }

                InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);

                if (item.ExpirationTime != null)
                {
                    item.StopExpirationTimer();
                    item.OnExpiration -= Item_OnExpiration;
                }

                _logger.LogDebug("Character {characterId} lost item {type} {typeId}", _ownerId, item.Type, item.TypeId);
                _logsManager.LogRemoveItem(_ownerId,
                    item.Type,
                    item.TypeId,
                    item.Count,
                    item.Gem1 is null ? 0 : item.Gem1.TypeId,
                    item.Gem2 is null ? 0 : item.Gem2.TypeId,
                    item.Gem3 is null ? 0 : item.Gem3.TypeId,
                    item.Gem4 is null ? 0 : item.Gem4.TypeId,
                    item.Gem5 is null ? 0 : item.Gem5.TypeId,
                    item.Gem6 is null ? 0 : item.Gem6.TypeId,
                    item.DyeColor.IsEnabled,
                    item.DyeColor.R,
                    item.DyeColor.G,
                    item.DyeColor.B,
                    item.GetCraftName(),
                    source);

                OnRemoveItem?.Invoke(item, true);
                return item;
            }
        }

        public (Item sourceItem, Item destinationItem) MoveItem(byte sourceBag, byte sourceSlot, byte destinationBag, byte destinationSlot)
        {
            lock (_syncAddRemoveItem)
            {
                // Find source item.
                Item sourceItem;
                if (sourceBag != WarehouseManager.WAREHOUSE_BAG && sourceBag != WarehouseManager.GUILD_WAREHOUSE_BAG)
                    InventoryItems.TryRemove((sourceBag, sourceSlot), out sourceItem);
                else
                    _warehouseManager.TryRemove(sourceBag, sourceSlot, out sourceItem);

                if (sourceItem is null)
                {
                    // wrong packet, source item should be always presented.
                    _logger.LogError("Could not find source item for player {characterId}", _ownerId);
                    return (null, null);
                }

                if (sourceBag == WarehouseManager.WAREHOUSE_BAG || sourceBag == WarehouseManager.GUILD_WAREHOUSE_BAG)
                {
                    var fee = (uint)Math.Round(sourceItem.Price * 0.05);
                    if (Gold < fee)
                    {
                        // Game should check if it's possible to take out item from warehouse.
                        // If packet still came, probably player is cheating.
                        _warehouseManager.TryAdd(sourceBag, sourceSlot, sourceItem);
                        _logger.LogError("Could not take out item from warehouse for {characterId}", _ownerId);
                        return (null, null);
                    }
                    else
                        Gold -= fee;
                }

                // Check, if any other item is at destination slot.
                Item destinationItem;
                if (destinationBag != WarehouseManager.WAREHOUSE_BAG && destinationBag != WarehouseManager.GUILD_WAREHOUSE_BAG)
                    InventoryItems.TryRemove((destinationBag, destinationSlot), out destinationItem);
                else
                {
                    var ok = _warehouseManager.TryRemove(destinationBag, destinationSlot, out destinationItem);
                    if (!ok)
                    {
                        InventoryItems.TryAdd((sourceBag, sourceSlot), sourceItem);
                        return (null, null);
                    }
                }

                if (destinationItem is null)
                {
                    // No item at destination place.
                    // Since there is no destination item we will use source item as destination.
                    // The only change, that we need to do is to set new bag and slot.
                    destinationItem = sourceItem;
                    destinationItem.Bag = destinationBag;
                    destinationItem.Slot = destinationSlot;

                    sourceItem = new Item(_gameDefinitions, _enchantConfig, _itemCreateConfig, 0, 0) { Bag = sourceBag, Slot = sourceSlot }; // empty item.
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

                        sourceItem = new Item(_gameDefinitions, _enchantConfig, _itemCreateConfig, 0, 0) { Bag = sourceBag, Slot = sourceSlot }; // empty item.
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
                    if (sourceItem.Bag != WarehouseManager.WAREHOUSE_BAG && sourceItem.Bag != WarehouseManager.GUILD_WAREHOUSE_BAG)
                        InventoryItems.TryAdd((sourceItem.Bag, sourceItem.Slot), sourceItem);
                    else
                        _warehouseManager.TryAdd(sourceItem.Bag, sourceItem.Slot, sourceItem);

                if (destinationItem.Type != 0 && destinationItem.TypeId != 0)
                    if (destinationItem.Bag != WarehouseManager.WAREHOUSE_BAG && destinationItem.Bag != WarehouseManager.GUILD_WAREHOUSE_BAG)
                        InventoryItems.TryAdd((destinationItem.Bag, destinationItem.Slot), destinationItem);
                    else
                        _warehouseManager.TryAdd(destinationItem.Bag, destinationItem.Slot, destinationItem);

                return (sourceItem, destinationItem);
            }
        }

        #endregion

        #region Gold

        public uint Gold { get; set; }

        #endregion

        #region Use item

        private readonly Dictionary<byte, DateTime> _itemCooldowns = new Dictionary<byte, DateTime>();

        public event Action<uint, Item> OnUsedItem;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async Task<bool> TryUseItem(byte bag, byte slot, uint? targetId = null, bool skillApplyingItemEffect = false, byte count = 1)
        {
            await _semaphore.WaitAsync();

            InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
            {
                _logger.LogWarning("Character {id} is trying to use item, that does not exist. Possible hack?", _ownerId);
                _semaphore.Release();
                return false;
            }

            if (item.Count < count)
            {
                _logger.LogWarning("Character {id} is trying to use more items then presented in inventory.", _ownerId);
                _semaphore.Release();
                return false;
            }

            if (!CanUseItem(item))
            {
                _semaphore.Release();
                return false;
            }

            if (targetId != null)
            {
                if (!CanUseItemOnTarget(item, (uint)targetId))
                {
                    _semaphore.Release();
                    return false;
                }
            }

            bool ok;
            if (skillApplyingItemEffect)
                ok = true;
            else
                ok = await ApplyItemEffect(item, targetId);

            if (ok)
            {
                item.Count -= count;
                _itemCooldowns[item.ReqIg] = DateTime.UtcNow;

                OnUsedItem?.Invoke(_ownerId, item);
                _logsManager.LogRemoveItem(_ownerId,
                        item.Type,
                        item.TypeId,
                        item.Count,
                        item.Gem1 is null ? 0 : item.Gem1.TypeId,
                        item.Gem2 is null ? 0 : item.Gem2.TypeId,
                        item.Gem3 is null ? 0 : item.Gem3.TypeId,
                        item.Gem4 is null ? 0 : item.Gem4.TypeId,
                        item.Gem5 is null ? 0 : item.Gem5.TypeId,
                        item.Gem6 is null ? 0 : item.Gem6.TypeId,
                        item.DyeColor.IsEnabled,
                        item.DyeColor.R,
                        item.DyeColor.G,
                        item.DyeColor.B,
                        item.GetCraftName(),
                        "used_item");

                if (item.Count == 0)
                    InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);
            }

            _semaphore.Release();
            return ok;
        }

        public bool CanUseItem(Item item)
        {
            if (item.Type == Item.START_QUEST_ITEM_TYPE)
                return true;

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

            if (_additionalInfoManager.Grow < item.Grow)
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

            if (item.ItemClassType != ItemClassType.AllFactions && item.ItemClassType != ItemClassType.AllLights && item.ItemClassType != ItemClassType.AllFury)
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

            if (item.Special == SpecialEffect.GuildHouseTeleport)
            {
                var character = _database.Characters
                                         .Include(x => x.Guild)
                                         .AsNoTracking()
                                         .FirstOrDefault(x => x.Id == _ownerId);
                if (character is null || character.Guild is null || !character.Guild.HasHouse || character.Guild.KeepEtin > character.Guild.Etin)
                    return false;
            }

            return true;
        }

        public bool CanUseItemOnTarget(Item item, uint targetId)
        {
            switch (item.Special)
            {
                case SpecialEffect.MovementRune:
                    if (_gameWorld.Players.TryGetValue(targetId, out var target))
                    {
                        if (target.PartyManager.Party != _partyManager.Party)
                            return false;

                        return _gameWorld.CanTeleport(_gameWorld.Players[_ownerId], target.MapProvider.NextMapId, out var reason);
                    }
                    else
                        return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Adds the effect of the item to the character.
        /// </summary>
        private async Task<bool> ApplyItemEffect(Item item, uint? targetId = null)
        {
            var ok = false;

            switch (item.Special)
            {
                case SpecialEffect.None:
                    if (item.HP > 0 || item.MP > 0 || item.SP > 0)
                    {
                        UseHealingPotion(item);
                        ok = true;
                    }

                    if (item.SkillId != 0)
                    {
                        var skill = new Skill(_gameDefinitions.Skills[(item.SkillId, item.SkillLevel)], ISkillsManager.ITEM_SKILL_NUMBER, 0);
                        var me = _gameWorld.Players[_ownerId];

                        if (_skillsManager.CanUseSkill(skill, me, out var s))
                        {
                            _skillsManager.UseSkill(skill, me);
                            ok = true;
                        }
                    }

                    if (item.Type == Item.START_QUEST_ITEM_TYPE)
                        ok = true;

                    break;

                case SpecialEffect.PercentHealingPotion:
                    ok = UsePercentHealingPotion(item);
                    break;

                case SpecialEffect.HypnosisCure:
                    ok = UseCureDebuffPotion(StateType.Sleep);
                    break;

                case SpecialEffect.StunCure:
                    ok = UseCureDebuffPotion(StateType.Stun);
                    break;

                case SpecialEffect.SilenceCure:
                    ok = UseCureDebuffPotion(StateType.Silence);
                    break;

                case SpecialEffect.DarknessCure:
                    ok = UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.StopCure:
                    ok = UseCureDebuffPotion(StateType.Immobilize);
                    break;

                case SpecialEffect.SlowCure:
                    ok = UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.VenomCure:
                    ok = UseCureDebuffPotion(StateType.HPDamageOverTime);
                    break;

                case SpecialEffect.DiseaseCure:
                    ok = UseCureDebuffPotion(StateType.SPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.IllnessDelusionCure:
                    ok = UseCureDebuffPotion(StateType.HPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.SPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.SleepStunStopSlowCure:
                    ok = UseCureDebuffPotion(StateType.Sleep) &&
                         UseCureDebuffPotion(StateType.Stun) &&
                         UseCureDebuffPotion(StateType.Immobilize) &&
                         UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.SilenceDarknessCure:
                    ok = UseCureDebuffPotion(StateType.Silence) &&
                         UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.DullBadLuckCure:
                    ok = UseCureDebuffPotion(StateType.DexDecrease) &&
                         UseCureDebuffPotion(StateType.Misfortunate);
                    break;

                case SpecialEffect.DoomFearCure:
                    ok = UseCureDebuffPotion(StateType.MentalSmasher) &&
                         UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.FullCure:
                    ok = UseCureDebuffPotion(StateType.Sleep) &&
                         UseCureDebuffPotion(StateType.Stun) &&
                         UseCureDebuffPotion(StateType.Silence) &&
                         UseCureDebuffPotion(StateType.Darkness) &&
                         UseCureDebuffPotion(StateType.Immobilize) &&
                         UseCureDebuffPotion(StateType.Slow) &&
                         UseCureDebuffPotion(StateType.HPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.SPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.MPDamageOverTime) &&
                         UseCureDebuffPotion(StateType.DexDecrease) &&
                         UseCureDebuffPotion(StateType.Misfortunate) &&
                         UseCureDebuffPotion(StateType.MentalSmasher) &&
                         UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.DisorderCure:
                    // ?
                    ok = false;
                    break;

                case SpecialEffect.StatResetStone:
                    ok = TryResetStats();
                    break;

                case SpecialEffect.GoddessBlessing:
                    ok = UseBlessItem();
                    break;

                case SpecialEffect.NameChange:
                    ok = await UseNameChangeStone();
                    break;

                case SpecialEffect.AnotherItemGenerator:
                    var items = item.GenerateItems();

                    if (items.Count == 0) // Could not generate items.
                        return false;

                    if (InventoryItems.Count + items.Count > MAX_BAG * MAX_SLOT - 1) // no enough free place
                        return false;

                    foreach (var x in items)
                        AddItem(x, "generated_from_another_item");

                    return true;

                case SpecialEffect.SkillResetStone:
                    ok = _skillsManager.ResetSkills();
                    break;

                case SpecialEffect.MovementRune:
                    if (_gameWorld.Players.TryGetValue((uint)targetId, out var target))
                    {
                        _teleportationManager.Teleport(target.Map.Id, target.PosX, target.PosY, target.PosZ);
                        ok = true;
                    }
                    break;

                case SpecialEffect.PartySummon:
                    _partyManager.SummonMembers(summonItem: item);
                    break;

                case SpecialEffect.TownTeleport:
                    if (item.NpcId == 0)
                    {
                        _logger.LogWarning("Town portal item ({type}, {typeId}) does not have npc id", item.Type, item.TypeId);
                        return false;
                    }

                    if (!_gameDefinitions.NPCs.TryGetValue((NpcType.GateKeeper, (short)item.NpcId), out var npc))
                        return false;


                    var gatekeeper = (GateKeeper)npc;
                    if (gatekeeper.GateTargets.Count < item.TradeQuantity)
                        return false;

                    var gate = gatekeeper.GateTargets[item.TradeQuantity];
                    _teleportationManager.StartCastingTeleport((ushort)gate.MapId, gate.Position.X, gate.Position.Y, gate.Position.Z, item);
                    break;

                case SpecialEffect.CapitalTeleport:
                    IMap capital;
                    if (_countryProvider.Country == CountryType.Light)
                        capital = _gameWorld.Maps[35];
                    else
                        capital = _gameWorld.Maps[36];

                    var spawn = capital.GetNearestSpawn(0, 0, 0, _countryProvider.Country);

                    _teleportationManager.StartCastingTeleport(capital.Id, spawn.X, spawn.Y, spawn.Z, item);
                    break;

                case SpecialEffect.BootleggeryTeleport:
                    (ushort MapId, float X, float Y, float Z) bootleggery;
                    if (_countryProvider.Country == CountryType.Light)
                        bootleggery = (42, 78.6f, 12.3f, 28f);
                    else
                        bootleggery = (42, 23.4f, 12.4f, 106.4f);

                    _teleportationManager.StartCastingTeleport(bootleggery.MapId, bootleggery.X, bootleggery.Y, bootleggery.Z, item);
                    break;


                case SpecialEffect.ArenaTeleport:
                    (ushort MapId, float X, float Y, float Z) arena;
                    if (_countryProvider.Country == CountryType.Light)
                        arena = (40, 128, 3, 82);
                    else
                        arena = (40, 58, 3, 82);

                    _teleportationManager.StartCastingTeleport(arena.MapId, arena.X, arena.Y, arena.Z, item);
                    break;

                case SpecialEffect.GuildHouseTeleport:
                    (ushort MapId, float X, float Y, float Z) guildHouse;
                    if (_countryProvider.Country == CountryType.Light)
                        guildHouse = (51, 491.6f, 42.7f, 324.9f);
                    else
                        guildHouse = (52, 482.3f, 42.7f, 327.2f);

                    _teleportationManager.StartCastingTeleport(guildHouse.MapId, guildHouse.X, guildHouse.Y, guildHouse.Z, item);
                    break;

                case SpecialEffect.TeleportationStone:
                    if (targetId is null)
                        return false;

                    if (targetId > _teleportationManager.MaxSavedPoints)
                        return false;

                    if (!_teleportationManager.SavedPositions.TryGetValue((byte)targetId, out var savedPlace))
                        return false;

                    _teleportationManager.StartCastingTeleport(savedPlace.MapId, savedPlace.X, savedPlace.Y, savedPlace.Z, item);
                    break;

                case SpecialEffect.MessageToServer:
                    _chatManager.IsMessageToServer = true;
                    return true;

                case SpecialEffect.DungeonMap:
                    if (item.SkillId != 0)
                    {
                        var skill = new Skill(_gameDefinitions.Skills[(item.SkillId, item.SkillLevel)], ISkillsManager.ITEM_SKILL_NUMBER, 0);
                        var me = _gameWorld.Players[_ownerId];

                        _skillsManager.UseSkill(skill, me);
                        ok = true;
                    }
                    return ok;

                default:
                    _logger.LogError("Uninplemented item effect {special}.", item.Special);
                    break;
            }

            return ok;
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
        private bool UseCureDebuffPotion(StateType debuffType)
        {
            var debuffs = _buffsManager.ActiveBuffs.Where(b => b.Skill.StateType == debuffType).ToList();
            foreach (var d in debuffs)
                d.CancelBuff();

            return true;
        }

        /// <summary>
        /// Uses potion, that restores % of hp,sp,mp.
        /// </summary>
        private bool UsePercentHealingPotion(Item potion)
        {
            var hp = Convert.ToInt32(_healthManager.MaxHP * potion.HP / 100);
            var mp = Convert.ToInt32(_healthManager.MaxMP * potion.MP / 100);
            var sp = Convert.ToInt32(_healthManager.MaxSP * potion.SP / 100);

            _healthManager.Recover(hp, mp, sp);
            return true;
        }

        /// <summary>
        /// Initiates name change process
        /// </summary>
        public async Task<bool> UseNameChangeStone()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
            {
                _logger.LogError("Character {id} is not found", _ownerId);
                return false;
            }

            character.IsRename = true;
            var count = await _database.SaveChangesAsync();
            return count > 0;
        }

        /// <summary>
        /// GM item ,that increases bless amount of player's fraction.
        /// </summary>
        private bool UseBlessItem()
        {
            if (_countryProvider.Country == CountryType.Light)
                _blessManager.LightAmount += 500;
            else
                _blessManager.DarkAmount += 500;

            return true;
        }

        public bool TryResetStats()
        {
            var defaultStat = _characterConfig.DefaultStats.First(s => s.Job == _additionalInfoManager.Class);
            var statPerLevel = _characterConfig.GetLevelStatSkillPoints(_additionalInfoManager.Grow).StatPoint;

            var ok = _statsManager.TrySetStats(defaultStat.Str,
                                               defaultStat.Dex,
                                               defaultStat.Rec,
                                               defaultStat.Int,
                                               defaultStat.Wis,
                                               defaultStat.Luc,
                                               (ushort)((_levelProvider.Level - 1) * statPerLevel)); // Level - 1, because we are starting with 1 level.
            if (!ok)
                return false;

            _statsManager.IncreasePrimaryStat((ushort)(_levelProvider.Level - 1));
            _statsManager.RaiseResetStats();

            return ok;
        }

        private void PartyManager_OnSummoned(uint senderId)
        {
            if (_ownerId != senderId)
                return;

            UseCastingItem(_partyManager.Party.SummonRequest.SummonItem);
        }

        private void TeleportationManager_OnCastingTeleportFinished()
        {
            UseCastingItem(_teleportationManager.CastingItem);
        }

        private void UseCastingItem(Item item)
        {
            if (item is null)
                return;

            item.Count--;
            _itemCooldowns[item.ReqIg] = DateTime.UtcNow;

            OnUsedItem?.Invoke(_ownerId, item);

            if (item.Count == 0)
                InventoryItems.TryRemove((item.Bag, item.Slot), out var _);
        }

        #endregion

        #region Buy/sell

        public Item BuyItem(NpcProduct product, byte count, float discount, out BuyResult result)
        {
            result = BuyResult.Unknown;

            _gameDefinitions.Items.TryGetValue((product.Type, product.TypeId), out var dbItem);
            if (dbItem is null)
            {
                _logger.LogError($"Trying to buy not presented item(type={product.Type},typeId={product.TypeId}).");
                return null;
            }

            var price = discount > 0 ? dbItem.Buy * (1 - discount) : dbItem.Buy;
            if (price * count > Gold) // Not enough money.
            {
                result = BuyResult.NotEnoughMoney;
                return null;
            }

            var freeSlot = FindFreeSlotInInventory();
            if (freeSlot.Slot == -1) // No free slot.
            {
                result = BuyResult.NoFreeSlot;
                return null;
            }

            Gold = (uint)(Gold - price * count);

            var item = new Item(_gameDefinitions, _enchantConfig, _itemCreateConfig, dbItem.Type, dbItem.TypeId);
            item.Count = count;

            result = BuyResult.Success;

            return AddItem(item, "buy_from_npc");
        }

        public Item SellItem(Item item, byte count)
        {
            if (!InventoryItems.ContainsKey((item.Bag, item.Slot)))
                return null;

            item.TradeQuantity = count > item.Count ? item.Count : count;
            Gold = (uint)(Gold + item.Sell * item.TradeQuantity);
            return RemoveItem(item, "sold_to_npc");
        }

        #endregion

        #region Expiration

        public event Action<Item> OnItemExpired;

        private void Item_OnExpiration(Item item)
        {
            OnItemExpired?.Invoke(item);
            RemoveItem(item, "item_expired");
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
                // Go though all bags and try to find any free slot.
                // Start with 1, because 0 is worn items.
                for (byte i = 1; i <= MAX_BAG; i++)
                {
                    var bagItems = InventoryItems.Where(itm => itm.Value.Bag == i).OrderBy(b => b.Value.Slot);
                    for (var j = 0; j < MAX_SLOT; j++)
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
