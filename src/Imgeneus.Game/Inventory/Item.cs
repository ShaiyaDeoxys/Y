using System;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Dyeing;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Warehouse;
using Timer = System.Timers.Timer;
using Imgeneus.GameDefinitions;

namespace Imgeneus.World.Game.Inventory
{
    public class Item : IDisposable
    {
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly DbItem _dbItem;

        /// <summary>
        /// Unique type, used only for drop money on map.
        /// </summary>
        public const byte MONEY_ITEM_TYPE = 26;

        /// <summary>
        /// 30 type is always lapis.
        /// </summary>
        public const byte GEM_ITEM_TYPE = 30;

        /// <summary>
        /// 30 type always starts quest.
        /// </summary>
        public const byte START_QUEST_ITEM_TYPE = 29;

        public byte Bag;
        public byte Slot;
        public byte Type;
        public byte TypeId;
        public ushort Quality;

        public Gem Gem1;
        public Gem Gem2;
        public Gem Gem3;
        public Gem Gem4;
        public Gem Gem5;
        public Gem Gem6;

        public byte Count;

        public byte EnchantmentLevel;

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, DbCharacterItems dbCharacterItem) : this(definitionsPreloader, enchantConfig, itemCreateConfig, dbCharacterItem.Type, dbCharacterItem.TypeId, dbCharacterItem.Count)
        {
            Bag = dbCharacterItem.Bag;
            Slot = dbCharacterItem.Slot;
            Quality = dbCharacterItem.Quality;

            CreationTime = dbCharacterItem.CreationTime;
            ExpirationTime = dbCharacterItem.ExpirationTime;

            if (!string.IsNullOrWhiteSpace(dbCharacterItem.Craftname))
                ParseCraftname(dbCharacterItem.Craftname);

            if (dbCharacterItem.HasDyeColor)
                DyeColor = new DyeColor(dbCharacterItem.DyeColorAlpha, dbCharacterItem.DyeColorSaturation, dbCharacterItem.DyeColorR, dbCharacterItem.DyeColorG, dbCharacterItem.DyeColorB);

            if (dbCharacterItem.GemTypeId1 != 0)
                Gem1 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId1, 0);
            if (dbCharacterItem.GemTypeId2 != 0)
                Gem2 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId2, 1);
            if (dbCharacterItem.GemTypeId3 != 0)
                Gem3 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId3, 2);
            if (dbCharacterItem.GemTypeId4 != 0)
                Gem4 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId4, 3);
            if (dbCharacterItem.GemTypeId5 != 0)
                Gem5 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId5, 4);
            if (dbCharacterItem.GemTypeId6 != 0)
                Gem6 = new Gem(definitionsPreloader, dbCharacterItem.GemTypeId6, 5);
        }

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, DbWarehouseItem dbWarehouseItem) : this(definitionsPreloader, enchantConfig, itemCreateConfig, dbWarehouseItem.Type, dbWarehouseItem.TypeId, dbWarehouseItem.Count) 
        {
            Bag = WarehouseManager.WAREHOUSE_BAG;
            Slot = dbWarehouseItem.Slot;
            Quality = dbWarehouseItem.Quality;

            CreationTime = dbWarehouseItem.CreationTime;
            ExpirationTime = dbWarehouseItem.ExpirationTime;

            if (!string.IsNullOrWhiteSpace(dbWarehouseItem.Craftname))
                ParseCraftname(dbWarehouseItem.Craftname);

            if (dbWarehouseItem.HasDyeColor)
                DyeColor = new DyeColor(dbWarehouseItem.DyeColorAlpha, dbWarehouseItem.DyeColorSaturation, dbWarehouseItem.DyeColorR, dbWarehouseItem.DyeColorG, dbWarehouseItem.DyeColorB);

            if (dbWarehouseItem.GemTypeId1 != 0)
                Gem1 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId1, 0);
            if (dbWarehouseItem.GemTypeId2 != 0)
                Gem2 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId2, 1);
            if (dbWarehouseItem.GemTypeId3 != 0)
                Gem3 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId3, 2);
            if (dbWarehouseItem.GemTypeId4 != 0)
                Gem4 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId4, 3);
            if (dbWarehouseItem.GemTypeId5 != 0)
                Gem5 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId5, 4);
            if (dbWarehouseItem.GemTypeId6 != 0)
                Gem6 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId6, 5);
        }

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, DbGuildWarehouseItem dbWarehouseItem) : this(definitionsPreloader, enchantConfig, itemCreateConfig, dbWarehouseItem.Type, dbWarehouseItem.TypeId, dbWarehouseItem.Count)
        {
            Bag = WarehouseManager.GUILD_WAREHOUSE_BAG;
            Slot = dbWarehouseItem.Slot;
            Quality = dbWarehouseItem.Quality;

            CreationTime = dbWarehouseItem.CreationTime;
            ExpirationTime = dbWarehouseItem.ExpirationTime;

            if (!string.IsNullOrWhiteSpace(dbWarehouseItem.Craftname))
                ParseCraftname(dbWarehouseItem.Craftname);

            if (dbWarehouseItem.HasDyeColor)
                DyeColor = new DyeColor(dbWarehouseItem.DyeColorAlpha, dbWarehouseItem.DyeColorSaturation, dbWarehouseItem.DyeColorR, dbWarehouseItem.DyeColorG, dbWarehouseItem.DyeColorB);

            if (dbWarehouseItem.GemTypeId1 != 0)
                Gem1 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId1, 0);
            if (dbWarehouseItem.GemTypeId2 != 0)
                Gem2 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId2, 1);
            if (dbWarehouseItem.GemTypeId3 != 0)
                Gem3 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId3, 2);
            if (dbWarehouseItem.GemTypeId4 != 0)
                Gem4 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId4, 3);
            if (dbWarehouseItem.GemTypeId5 != 0)
                Gem5 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId5, 4);
            if (dbWarehouseItem.GemTypeId6 != 0)
                Gem6 = new Gem(definitionsPreloader, dbWarehouseItem.GemTypeId6, 5);
        }

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, DbMarketItem dbMarketItem) : this(definitionsPreloader, enchantConfig, itemCreateConfig, dbMarketItem.Type, dbMarketItem.TypeId, dbMarketItem.Count)
        {
            Quality = dbMarketItem.Quality;

            if (!string.IsNullOrWhiteSpace(dbMarketItem.Craftname))
                ParseCraftname(dbMarketItem.Craftname);

            if (dbMarketItem.HasDyeColor)
                DyeColor = new DyeColor(dbMarketItem.DyeColorAlpha, dbMarketItem.DyeColorSaturation, dbMarketItem.DyeColorR, dbMarketItem.DyeColorG, dbMarketItem.DyeColorB);

            if (dbMarketItem.GemTypeId1 != 0)
                Gem1 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId1, 0);
            if (dbMarketItem.GemTypeId2 != 0)
                Gem2 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId2, 1);
            if (dbMarketItem.GemTypeId3 != 0)
                Gem3 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId3, 2);
            if (dbMarketItem.GemTypeId4 != 0)
                Gem4 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId4, 3);
            if (dbMarketItem.GemTypeId5 != 0)
                Gem5 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId5, 4);
            if (dbMarketItem.GemTypeId6 != 0)
                Gem6 = new Gem(definitionsPreloader, dbMarketItem.GemTypeId6, 5);
        }

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, BankItem bankItem) : this(definitionsPreloader, enchantConfig, itemCreateConfig, bankItem.Type, bankItem.TypeId, bankItem.Count)
        {
        }

        public Item(IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, byte type, byte typeId, byte count = 1)
        {
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            Type = type;
            TypeId = typeId;
            Count = count;
            CreationTime = DateTime.UtcNow;

            if (Type != 0 && TypeId != 0 && Type != MONEY_ITEM_TYPE)
            {
                _dbItem = _definitionsPreloader.Items[(Type, TypeId)];

                // Prevent Count from exceeding MaxCount and from being 0 (zero)
                var newCount = count > MaxCount ? MaxCount : count;
                Count = newCount < 1 ? (byte)1 : newCount;

                // Set quality to maximum quality
                Quality = _dbItem.Quality;

                // Temporary item check
                if (_dbItem.Duration > 0)
                {
                    // Get ExpirationTime based on DbItem
                    ExpirationTime = CreationTime.AddSeconds(_dbItem.Duration);
                }
            }

            if (IsExpirable)
            {
                var interval = (DateTime)ExpirationTime - DateTime.UtcNow;

                // Don't start timer for items that have more than 12 hours left
                if (interval.TotalHours > 12)
                    return;

                _expirationTimer.Interval = interval.TotalMilliseconds;
                _expirationTimer.AutoReset = false;
                _expirationTimer.Elapsed += ExpirationTimer_Elapsed;

                _expirationTimer.Start();
            }
        }

        #region Trade

        public byte TradeQuantity;

        #endregion

        #region Extra stats

        private int ConstStr => _dbItem.ConstStr;
        private int ConstDex => _dbItem.ConstDex;
        private int ConstRec => _dbItem.ConstRec;
        private int ConstInt => _dbItem.ConstInt;
        private int ConstLuc => _dbItem.ConstLuc;
        private int ConstWis => _dbItem.ConstWis;
        private int ConstHP => _dbItem.ConstHP;
        private int ConstMP => _dbItem.ConstMP;
        private int ConstSP => _dbItem.ConstSP;
        private byte ConstAttackSpeed => _dbItem.AttackTime;
        private byte ConstMoveSpeed => _dbItem.Speed;
        private ushort ConstDefense => _dbItem.Defense;
        private ushort ConstResistance => _dbItem.Resistance;
        private ushort ConstMinAttack => _dbItem.MinAttack;
        private ushort ConstPlusAttack => _dbItem.PlusAttack;
        private Element ConstElement => _dbItem.Element;
        public int Sell => _dbItem.Sell;
        public byte ReqIg => _dbItem.ReqIg;
        public ushort SkillId => _dbItem.Range;
        public byte SkillLevel => _dbItem.AttackTime;
        public ushort Reqlevel { get => _dbItem.Reqlevel; }
        public ItemClassType ItemClassType { get => _dbItem.Country; }
        public bool IsForFighter { get => _dbItem.Attackfighter == 1; }
        public bool IsForDefender { get => _dbItem.Defensefighter == 1; }
        public bool IsForRanger { get => _dbItem.Patrolrogue == 1; }
        public bool IsForArcher { get => _dbItem.Shootrogue == 1; }
        public bool IsForMage { get => _dbItem.Attackmage == 1; }
        public bool IsForPriest { get => _dbItem.Defensemage == 1; }

        /// <summary>
        /// Str contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Str
        {
            get
            {
                ushort gemsStr = 0;

                if (Gem1 != null)
                    gemsStr += Gem1.Str;
                if (Gem2 != null)
                    gemsStr += Gem2.Str;
                if (Gem3 != null)
                    gemsStr += Gem3.Str;
                if (Gem4 != null)
                    gemsStr += Gem4.Str;
                if (Gem5 != null)
                    gemsStr += Gem5.Str;
                if (Gem6 != null)
                    gemsStr += Gem6.Str;

                return ConstStr + gemsStr + ComposedStr;
            }
        }

        /// <summary>
        /// Dex contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Dex
        {
            get
            {
                ushort gemsDex = 0;

                if (Gem1 != null)
                    gemsDex += Gem1.Dex;
                if (Gem2 != null)
                    gemsDex += Gem2.Dex;
                if (Gem3 != null)
                    gemsDex += Gem3.Dex;
                if (Gem4 != null)
                    gemsDex += Gem4.Dex;
                if (Gem5 != null)
                    gemsDex += Gem5.Dex;
                if (Gem6 != null)
                    gemsDex += Gem6.Dex;

                return ConstDex + gemsDex + ComposedDex;
            }
        }

        /// <summary>
        /// Rec contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Rec
        {
            get
            {
                ushort gemsRec = 0;

                if (Gem1 != null)
                    gemsRec += Gem1.Rec;
                if (Gem2 != null)
                    gemsRec += Gem2.Rec;
                if (Gem3 != null)
                    gemsRec += Gem3.Rec;
                if (Gem4 != null)
                    gemsRec += Gem4.Rec;
                if (Gem5 != null)
                    gemsRec += Gem5.Rec;
                if (Gem6 != null)
                    gemsRec += Gem6.Rec;

                return ConstRec + gemsRec + ComposedRec;
            }
        }

        /// <summary>
        /// Int contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Int
        {
            get
            {
                ushort gemsInt = 0;

                if (Gem1 != null)
                    gemsInt += Gem1.Int;
                if (Gem2 != null)
                    gemsInt += Gem2.Int;
                if (Gem3 != null)
                    gemsInt += Gem3.Int;
                if (Gem4 != null)
                    gemsInt += Gem4.Int;
                if (Gem5 != null)
                    gemsInt += Gem5.Int;
                if (Gem6 != null)
                    gemsInt += Gem6.Int;

                return ConstInt + gemsInt + ComposedInt;
            }
        }

        /// <summary>
        /// Luc contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Luc
        {
            get
            {
                ushort gemsLuc = 0;

                if (Gem1 != null)
                    gemsLuc += Gem1.Luc;
                if (Gem2 != null)
                    gemsLuc += Gem2.Luc;
                if (Gem3 != null)
                    gemsLuc += Gem3.Luc;
                if (Gem4 != null)
                    gemsLuc += Gem4.Luc;
                if (Gem5 != null)
                    gemsLuc += Gem5.Luc;
                if (Gem6 != null)
                    gemsLuc += Gem6.Luc;

                return ConstLuc + gemsLuc + ComposedLuc;
            }
        }

        /// <summary>
        /// Wis contains yellow(default) stat + orange stat (take it from craft name later).
        /// </summary>
        public int Wis
        {
            get
            {
                ushort gemsWis = 0;

                if (Gem1 != null)
                    gemsWis += Gem1.Wis;
                if (Gem2 != null)
                    gemsWis += Gem2.Wis;
                if (Gem3 != null)
                    gemsWis += Gem3.Wis;
                if (Gem4 != null)
                    gemsWis += Gem4.Wis;
                if (Gem5 != null)
                    gemsWis += Gem5.Wis;
                if (Gem6 != null)
                    gemsWis += Gem6.Wis;

                return ConstWis + gemsWis + ComposedWis;
            }
        }

        /// <summary>
        /// HP stats.
        /// </summary>
        public int HP
        {
            get
            {
                ushort gemsHP = 0;

                if (Gem1 != null)
                    gemsHP += Gem1.HP;
                if (Gem2 != null)
                    gemsHP += Gem2.HP;
                if (Gem3 != null)
                    gemsHP += Gem3.HP;
                if (Gem4 != null)
                    gemsHP += Gem4.HP;
                if (Gem5 != null)
                    gemsHP += Gem5.HP;
                if (Gem6 != null)
                    gemsHP += Gem6.HP;

                return ConstHP + gemsHP + ComposedHP;
            }
        }

        /// <summary>
        /// MP stats.
        /// </summary>
        public int MP
        {
            get
            {
                ushort gemsMP = 0;

                if (Gem1 != null)
                    gemsMP += Gem1.MP;
                if (Gem2 != null)
                    gemsMP += Gem2.MP;
                if (Gem3 != null)
                    gemsMP += Gem3.MP;
                if (Gem4 != null)
                    gemsMP += Gem4.MP;
                if (Gem5 != null)
                    gemsMP += Gem5.MP;
                if (Gem6 != null)
                    gemsMP += Gem6.MP;

                return ConstMP + gemsMP + ComposedMP;
            }
        }

        /// <summary>
        /// SP stats.
        /// </summary>
        public int SP
        {
            get
            {
                ushort gemsSP = 0;

                if (Gem1 != null)
                    gemsSP += Gem1.SP;
                if (Gem2 != null)
                    gemsSP += Gem2.SP;
                if (Gem3 != null)
                    gemsSP += Gem3.SP;
                if (Gem4 != null)
                    gemsSP += Gem4.SP;
                if (Gem5 != null)
                    gemsSP += Gem5.SP;
                if (Gem6 != null)
                    gemsSP += Gem6.SP;

                return ConstSP + gemsSP + ComposedSP;
            }
        }

        public byte AttackSpeed
        {
            get
            {
                byte gemsSpeed = 0;

                if (Gem1 != null)
                    gemsSpeed += Gem1.AttackSpeed;
                if (Gem2 != null)
                    gemsSpeed += Gem2.AttackSpeed;
                if (Gem3 != null)
                    gemsSpeed += Gem3.AttackSpeed;
                if (Gem4 != null)
                    gemsSpeed += Gem4.AttackSpeed;
                if (Gem5 != null)
                    gemsSpeed += Gem5.AttackSpeed;
                if (Gem6 != null)
                    gemsSpeed += Gem6.AttackSpeed;

                return (byte)(ConstAttackSpeed + gemsSpeed);
            }
        }

        public byte MoveSpeed
        {
            get
            {
                byte gemsSpeed = 0;

                if (Gem1 != null)
                    gemsSpeed += Gem1.MoveSpeed;
                if (Gem2 != null)
                    gemsSpeed += Gem2.MoveSpeed;
                if (Gem3 != null)
                    gemsSpeed += Gem3.MoveSpeed;
                if (Gem4 != null)
                    gemsSpeed += Gem4.MoveSpeed;
                if (Gem5 != null)
                    gemsSpeed += Gem5.MoveSpeed;
                if (Gem6 != null)
                    gemsSpeed += Gem6.MoveSpeed;

                return (byte)(ConstMoveSpeed + gemsSpeed);
            }
        }

        public int Defense
        {
            get
            {
                int gemDefense = 0;
                if (Gem1 != null)
                    gemDefense += Gem1.Defense;
                if (Gem2 != null)
                    gemDefense += Gem2.Defense;
                if (Gem3 != null)
                    gemDefense += Gem3.Defense;
                if (Gem4 != null)
                    gemDefense += Gem4.Defense;
                if (Gem5 != null)
                    gemDefense += Gem5.Defense;
                if (Gem6 != null)
                    gemDefense += Gem6.Defense;

                return ConstDefense + gemDefense;
            }
        }

        public int Resistance
        {
            get
            {
                int gemResistance = 0;
                if (Gem1 != null)
                    gemResistance += Gem1.Resistance;
                if (Gem2 != null)
                    gemResistance += Gem2.Resistance;
                if (Gem3 != null)
                    gemResistance += Gem3.Resistance;
                if (Gem4 != null)
                    gemResistance += Gem4.Resistance;
                if (Gem5 != null)
                    gemResistance += Gem5.Resistance;
                if (Gem6 != null)
                    gemResistance += Gem6.Resistance;

                return ConstResistance + gemResistance;
            }
        }

        public int MinAttack
        {
            get
            {
                int gemMinAttack = 0;
                if (Gem1 != null)
                    gemMinAttack += Gem1.MinAttack;
                if (Gem2 != null)
                    gemMinAttack += Gem2.MinAttack;
                if (Gem3 != null)
                    gemMinAttack += Gem3.MinAttack;
                if (Gem4 != null)
                    gemMinAttack += Gem4.MinAttack;
                if (Gem5 != null)
                    gemMinAttack += Gem5.MinAttack;
                if (Gem6 != null)
                    gemMinAttack += Gem6.MinAttack;

                var suffix = EnchantmentLevel > 9 ? $"{EnchantmentLevel}" : $"0{EnchantmentLevel}";
                var enchant = _enchantConfig.LapisianEnchantAddValue[$"WeaponStep{suffix}"];

                return ConstMinAttack + gemMinAttack + enchant;
            }
        }

        public int MaxAttack
        {
            get
            {
                int gemPlusAttack = 0;
                if (Gem1 != null)
                    gemPlusAttack += Gem1.PlusAttack;
                if (Gem2 != null)
                    gemPlusAttack += Gem2.PlusAttack;
                if (Gem3 != null)
                    gemPlusAttack += Gem3.PlusAttack;
                if (Gem4 != null)
                    gemPlusAttack += Gem4.PlusAttack;
                if (Gem5 != null)
                    gemPlusAttack += Gem5.PlusAttack;
                if (Gem6 != null)
                    gemPlusAttack += Gem6.PlusAttack;

                var suffix = EnchantmentLevel > 9 ? $"{EnchantmentLevel}" : $"0{EnchantmentLevel}";
                var enchant = _enchantConfig.LapisianEnchantAddValue[$"WeaponStep{suffix}"];

                return ConstMinAttack + gemPlusAttack + ConstPlusAttack + enchant;
            }
        }

        public Element Element
        {
            get
            {
                if (ConstElement != Element.None)
                    return ConstElement;

                if (Gem1 != null && Gem1.Element != Element.None)
                    return Gem1.Element;

                if (Gem2 != null && Gem2.Element != Element.None)
                    return Gem2.Element;

                if (Gem3 != null && Gem3.Element != Element.None)
                    return Gem3.Element;

                if (Gem4 != null && Gem4.Element != Element.None)
                    return Gem4.Element;

                if (Gem5 != null && Gem5.Element != Element.None)
                    return Gem5.Element;

                if (Gem6 != null && Gem6.Element != Element.None)
                    return Gem6.Element;

                return Element.None;
            }
        }

        /// <summary>
        /// Items with "Absorption Lapis" can reduce damage.
        /// </summary>
        public ushort Absorb
        {
            get
            {
                ushort absorb = 0;

                if (Gem1 != null)
                    absorb += Gem1.Absorb;
                if (Gem2 != null)
                    absorb += Gem2.Absorb;
                if (Gem3 != null)
                    absorb += Gem3.Absorb;
                if (Gem4 != null)
                    absorb += Gem4.Absorb;
                if (Gem5 != null)
                    absorb += Gem5.Absorb;
                if (Gem6 != null)
                    absorb += Gem6.Absorb;

                var suffix = EnchantmentLevel > 9 ? $"{EnchantmentLevel}" : $"0{EnchantmentLevel}";
                var enchant = _enchantConfig.LapisianEnchantAddValue[$"DefenseStep{suffix}"];

                return (ushort)(_dbItem.Exp + absorb + enchant);
            }
        }

        public ushort AttackRange
        {
            get
            {
                return _dbItem.Range;
            }
        }

        /// <summary>
        /// Special effect like teleport/cure/summon etc.
        /// </summary>
        public SpecialEffect Special => _dbItem.Special;

        /// <summary>
        /// If item can be given away.
        /// </summary>
        public ItemAccountRestrictionType AccountRestriction => _dbItem.ReqOg;

        /// <summary>
        /// For mounts, its value specifies which character shape we should use.
        /// </summary>
        public ushort Range => _dbItem.Range;

        /// <summary>
        /// Can be used in basic, ultimate etc. mode.
        /// </summary>
        public Mode Grow => _dbItem.Grow;

        /// <summary>
        /// Defines "color" of item.
        /// </summary>
        public ushort ReqDex => _dbItem.ReqDex;

        /// <summary>
        /// For linking hammer, it's how many times it increases the success linking rate.
        /// </summary>
        public ushort LinkingRate => _dbItem.ReqVg;

        /// <summary>
        /// Lapis or lapisia can break item while unsuccessful linking or enchantment.
        /// </summary>
        public bool CanBreakItem => _dbItem.ReqVg > 0;

        #endregion

        #region Craft name stats (orange stats)

        public const string DEFAULT_CRAFT_NAME = "00000000000000000000";

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedStr { get; set; }

        /// <summary>
        /// Orange dex stat.
        /// </summary>
        public int ComposedDex { get; set; }

        /// <summary>
        /// Orange rec stat.
        /// </summary>
        public int ComposedRec { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedInt { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedLuc { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedWis { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedHP { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedMP { get; set; }

        /// <summary>
        /// Orange str stat.
        /// </summary>
        public int ComposedSP { get; set; }

        /// <summary>
        /// Max number of composed stats.
        /// </summary>
        public ushort ReqWis { get => _dbItem.ReqWis; }

        /// <summary>
        /// Bool indicator, that shows if item can be recreated (use rec runes on it).
        /// </summary>
        public bool IsComposable { get => _dbItem != null && ReqWis > 0; }

        /// <summary>
        /// Generates craft name, that is stored in db.
        /// </summary>
        public string GetCraftName()
        {
            if (!IsComposable)
                return DEFAULT_CRAFT_NAME;

            var strBuilder = new StringBuilder();

            if (ComposedStr > 9)
                strBuilder.Append($"{ComposedStr}");
            else
                strBuilder.Append($"0{ComposedStr}");

            if (ComposedDex > 9)
                strBuilder.Append($"{ComposedDex}");
            else
                strBuilder.Append($"0{ComposedDex}");

            if (ComposedRec > 9)
                strBuilder.Append($"{ComposedRec}");
            else
                strBuilder.Append($"0{ComposedRec}");

            if (ComposedInt > 9)
                strBuilder.Append($"{ComposedInt}");
            else
                strBuilder.Append($"0{ComposedInt}");

            if (ComposedWis > 9)
                strBuilder.Append($"{ComposedWis}");
            else
                strBuilder.Append($"0{ComposedWis}");

            if (ComposedLuc > 9)
                strBuilder.Append($"{ComposedLuc}");
            else
                strBuilder.Append($"0{ComposedLuc}");

            var hp = ComposedHP / 100;
            if (hp > 9)
                strBuilder.Append($"{hp}");
            else
                strBuilder.Append($"0{hp}");

            var mp = ComposedMP / 100;
            if (mp > 9)
                strBuilder.Append($"{mp}");
            else
                strBuilder.Append($"0{mp}");

            var sp = ComposedSP / 100;
            if (sp > 9)
                strBuilder.Append($"{sp}");
            else
                strBuilder.Append($"0{sp}");

            if (EnchantmentLevel > 9)
                strBuilder.Append($"{EnchantmentLevel}");
            else
                strBuilder.Append($"0{EnchantmentLevel}");

            return strBuilder.ToString();
        }

        /// <summary>
        /// Parses db craft name into numbers.
        /// </summary>
        private void ParseCraftname(string craftname)
        {
            if (craftname.Length != 20)
                return;

            for (var i = 0; i <= 18; i += 2)
            {
                var strBuilder = new StringBuilder();
                strBuilder.Append(craftname[i]);
                strBuilder.Append(craftname[i + 1]);
                if (byte.TryParse(strBuilder.ToString(), out var number))
                {
                    switch (i)
                    {
                        case 0:
                            ComposedStr = number;
                            break;

                        case 2:
                            ComposedDex = number;
                            break;

                        case 4:
                            ComposedRec = number;
                            break;

                        case 6:
                            ComposedInt = number;
                            break;

                        case 8:
                            ComposedWis = number;
                            break;

                        case 10:
                            ComposedLuc = number;
                            break;

                        case 12:
                            ComposedHP = number * 100;
                            break;

                        case 14:
                            ComposedMP = number * 100;
                            break;

                        case 16:
                            ComposedSP = number * 100;
                            break;

                        case 18:
                            EnchantmentLevel = number;
                            break;
                    }
                }
            }
        }

        #endregion

        #region Max count

        public byte MaxCount => _dbItem.Count;

        /// <summary>
        /// Consumables and lapis are joinable objects. I.e. count can be > 1.
        /// </summary>
        public bool IsJoinable
        {
            get => MaxCount > 1;
        }

        #endregion

        #region Dye color

        /// <summary>
        /// Unique color.
        /// </summary>
        public DyeColor DyeColor { get; set; }

        #endregion

        #region Expiration

        /// <summary>
        /// Time at which the item was created.
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Time at which the item expires.
        /// </summary>
        public DateTime? ExpirationTime { get; }

        /// <summary>
        /// Item has a fixed duration and is removed from the player's inventory after that duration has passed.
        /// </summary>
        public bool IsExpirable => ExpirationTime != null;

        /// <summary>
        /// Timer used by expirable items.
        /// </summary>
        private Timer _expirationTimer = new Timer();

        private void ExpirationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnExpiration?.Invoke(this);
            _expirationTimer.Elapsed -= ExpirationTimer_Elapsed;
        }

        /// <summary>
        /// Event that's fired when an item has expired.
        /// </summary>
        public event Action<Item> OnExpiration;

        /// <summary>
        /// Stops expiration timer.
        /// </summary>
        public void StopExpirationTimer()
        {
            _expirationTimer.Stop();
        }

        #endregion

        #region Gold item

        public int Gold { get; set; }

        public Item(int gold)
        {
            Type = MONEY_ITEM_TYPE;
            Gold = gold;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Number of still free slots.
        /// </summary>
        public byte FreeSlots
        {
            get
            {
                byte count = 0;
                switch (_dbItem.Slot)
                {
                    case 0:
                        break;

                    case 1:
                        if (Gem1 is null)
                            count++;
                        break;

                    case 2:
                        if (Gem1 is null)
                            count++;
                        if (Gem2 is null)
                            count++;
                        break;

                    case 3:
                        if (Gem1 is null)
                            count++;
                        if (Gem2 is null)
                            count++;
                        if (Gem3 is null)
                            count++;
                        break;

                    case 4:
                        if (Gem1 is null)
                            count++;
                        if (Gem2 is null)
                            count++;
                        if (Gem3 is null)
                            count++;
                        if (Gem4 is null)
                            count++;
                        break;

                    case 5:
                        if (Gem1 is null)
                            count++;
                        if (Gem2 is null)
                            count++;
                        if (Gem3 is null)
                            count++;
                        if (Gem4 is null)
                            count++;
                        if (Gem5 is null)
                            count++;
                        break;

                    case 6:
                        if (Gem1 is null)
                            count++;
                        if (Gem2 is null)
                            count++;
                        if (Gem3 is null)
                            count++;
                        if (Gem4 is null)
                            count++;
                        if (Gem5 is null)
                            count++;
                        if (Gem6 is null)
                            count++;
                        break;


                    default:
                        return 0;
                }

                return count;
            }
        }

        /// <summary>
        /// Checks if item already has such gem linked.
        /// </summary>
        /// <param name="typeId">gem type id</param>
        /// <returns>true, if item already has such gem</returns>
        public bool ContainsGem(byte typeId)
        {
            return Gem1 != null && Gem1.TypeId == typeId ||
                   Gem2 != null && Gem2.TypeId == typeId ||
                   Gem3 != null && Gem3.TypeId == typeId ||
                   Gem4 != null && Gem4.TypeId == typeId ||
                   Gem5 != null && Gem5.TypeId == typeId ||
                   Gem6 != null && Gem6.TypeId == typeId;
        }

        public bool IsCloakSlot
        {
            get => Slot == 7;
        }

        private static readonly List<byte> AllWeaponIds = new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 59, 60, 61, 62, 63, 64, 65 };

        public bool IsWeapon
        {
            get => AllWeaponIds.Contains(Type);
        }

        private static readonly List<byte> AllArmorIds = new List<byte>() { 16, 17, 18, 20, 21, 31, 32, 33, 35, 36, 67, 68, 70, 71, 72, 73, 74, 76, 77, 82, 83, 85, 86, 87, 88, 89, 91, 92 };

        public bool IsArmor
        {
            get => AllArmorIds.Contains(Type);
        }

        private static readonly List<byte> AllShields = new List<byte>() { 69, 84 };
        private readonly IItemCreateConfiguration _itemCreateConfig;

        public bool IsShield
        {
            get => AllShields.Contains(Type);
        }

        public bool IsMount
        {
            get => Type == 42;
        }

        public bool IsPet
        {
            get => Type == 120;
        }

        public bool IsCostume
        {
            get => Type == 150;
        }

        public bool IsWing
        {
            get => Type == 121;
        }

        /// <summary>
        /// Transforms weapon type to passive skill type.
        /// </summary>
        public byte ToPassiveSkillType()
        {
            switch (Type)
            {
                case 1:
                case 45:
                    return 1; // 1 Handed Sword

                case 2:
                case 46:
                    return 2; // 2 Handed Sword.

                case 3:
                case 47:
                    return 3; // 1 Handed Axe.

                case 4:
                case 48:
                    return 4; // 2 Handed Axe.

                case 5:
                case 49:
                case 50:
                    return 5; // Double sword.

                case 6:
                case 51:
                case 52:
                    return 6; // Spear.

                case 7:
                case 53:
                case 54:
                    return 7; // 1 Handed blunt.

                case 8:
                case 55:
                case 56:
                    return 8; // 2 Handed blunt.

                case 9:
                case 57:
                    return 9; // Reverse sword.

                case 11:
                case 59:
                    return 11; // Javelin.

                case 12:
                case 60:
                case 61:
                    return 12; // Staff.

                case 13:
                case 62:
                case 63:
                    return 13; // Bow.

                case 14:
                case 64:
                    return 14; // Crossbow.

                case 15:
                case 65:
                    return 15; // Knuckle.

                default:
                    return 0;
            }
        }

        public static Dictionary<byte, int> ReqIgToCooldownInMilliseconds = new Dictionary<byte, int>()
        {
            { 1, 15000 },
            { 2, 20000 },
            { 3, 25000 },
            { 4, 30000 },
            { 5, 60000 },
            { 6, 120000 },
            { 7, 3000000 },
            { 8, 0 },
            { 9, 0 },
            { 10, 600000 },
            { 11, 2000 }
        };

        #endregion

        #region Enchantment

        public byte MinEnchantLevel => (byte)_dbItem.Range;
        public byte MaxEnchantLevel => _dbItem.AttackTime;
        public ushort EnchantRate => _dbItem.ReqRec;
        public bool IsWeaponLapisia => _dbItem.Reqlevel > 0;
        public bool IsArmorLapisia => _dbItem.Country > 0;

        #endregion

        #region Teleport

        public ushort NpcId => _dbItem.ReqVg;

        #endregion

        #region Buy/Sell

        public int Price => _dbItem.Buy;

        #endregion

        #region Another item generation

        public IList<Item> GenerateItems()
        {
            var items = new List<Item>();

            if (_dbItem.ReqVg > 0) // Generate 1 item.
            {
                if (!_itemCreateConfig.ItemCreateInfo.ContainsKey(_dbItem.ReqVg))
                    return items;

                var possibilities = new List<ushort>();
                foreach (var possibility in _itemCreateConfig.ItemCreateInfo[_dbItem.ReqVg])
                {
                    var temp = new ushort[possibility.Weight];
                    Array.Fill(temp, possibility.Grade);
                    possibilities.AddRange(temp);
                }

                var random = new Random();
                var index = random.Next(possibilities.Count);

                var itemsByGrade = _definitionsPreloader.ItemsByGrade[possibilities[index]];
                var dbItem = itemsByGrade[random.Next(items.Count)];

                items.Add(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, dbItem.Type, dbItem.TypeId));
            }
            else 
            {
                // TODO: generate many items.
                // https://www.elitepvpers.com/forum/shaiya-pserver-development/4022975-request-drop-bag.html#post34170021
            }

            return items;
        }

        #endregion

        #region Shop

        public bool IsInShop { get; set; }

        public uint ShopPrice { get; set; }

        #endregion

        public Item Clone()
        {
            return new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, Type, TypeId)
            {
                Bag = Bag,
                Slot = Slot,
                Type = Type,
                TypeId = TypeId,
                Quality = Quality,
                Count = Count,
                DyeColor = DyeColor
            };
        }

        public void Dispose()
        {
            if (ExpirationTime != null)
            {
                _expirationTimer.Elapsed -= ExpirationTimer_Elapsed;
            }
        }
    }
}
