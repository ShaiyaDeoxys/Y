using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Parsec.Shaiya.Item;

namespace Imgeneus.GameDefinitions
{
    public class DbItem
    {
        public DbItem()
        {

        }

        public DbItem(DBItemDataRecord record)
        {
            Type = (byte)record.ItemType;
            TypeId = (byte)record.ItemTypeId;
            Reqlevel = (ushort)record.Level;
            Country = (ItemClassType)record.Country;
            Attackfighter = (byte)record.AttackFighter;
            Defensefighter = (byte)record.DefenseFighter;
            Patrolrogue = (byte)record.PatrolRogue;
            Shootrogue = (byte)record.ShootRogue;
            Attackmage = (byte)record.AttackMage;
            Defensemage = (byte)record.DefenseMage;
            Grow = (Mode)record.Grow;
            ReqStr = (ushort)record.Str;
            ReqDex = (ushort)record.Dex;
            ReqRec = (ushort)record.Rec;
            ReqInt = (ushort)record.Int;
            ReqWis = (ushort)record.Wis;
            Reqluc = (ushort)record.Luc;
            ReqVg = (ushort)record.Vg;
            ReqOg = (ItemAccountRestrictionType)record.Og;
            ReqIg = (byte)record.Ig;
            Range = (ushort)record.Range;
            AttackTime = (byte)record.AttackTime;
            Element = (Element)record.Attrib;
            Special = (SpecialEffect)record.Special;
            Slot = (byte)record.Slot;
            Quality = (ushort)record.Quality;
            MinAttack = (ushort)record.Effect1;
            PlusAttack = (ushort)record.Effect2;
            Defense = (ushort)record.Effect3;
            Resistance = (ushort)record.Effect4;
            ConstHP = (ushort)record.ConstHp;
            ConstSP = (ushort)record.ConstSp;
            ConstMP = (ushort)record.ConstMp;
            ConstStr = (ushort)record.ConstStr;
            ConstDex = (ushort)record.ConstDex;
            ConstRec = (ushort)record.ConstRec;
            ConstInt = (ushort)record.ConstInt;
            ConstWis = (ushort)record.ConstWis;
            ConstLuc = (ushort)record.ConstLuc;
            Speed = (byte)record.Speed;
            Exp = (byte)record.Exp;
            Buy = (int)record.Buy;
            Sell = (int)record.Sell;
            Grade = (ushort)record.Grade;
            Count = (byte)record.Count;
            Duration = (uint)record.Duration;
        }

        public string ItemName { get; set; }

        /// <summary>
        /// Type of item.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Type id of item.
        /// </summary>
        public byte TypeId { get; set; }

        /// <summary>
        /// Level, that is needed in order to use this item.
        /// </summary>
        public ushort Reqlevel { get; set; }

        /// <summary>
        /// Fury, lights or both.
        /// </summary>
        public ItemClassType Country { get; set; }

        /// <summary>
        /// Indicates if it can be used by Fighter. TODO: maybe migrate to bool.
        /// </summary>
        public byte Attackfighter { get; set; }

        /// <summary>
        /// Indicates if it can be used by Defender. TODO: maybe migrate to bool.
        /// </summary>
        public byte Defensefighter { get; set; }

        /// <summary>
        /// Indicates if it can be used by Ranger. TODO: maybe migrate to bool.
        /// </summary>
        public byte Patrolrogue { get; set; }

        /// <summary>
        /// Indicates if it can be used by Archer. TODO: maybe migrate to bool.
        /// </summary>
        public byte Shootrogue { get; set; }

        /// <summary>
        /// Indicates if it can be used by Mage. TODO: maybe migrate to bool.
        /// </summary>
        public byte Attackmage { get; set; }

        /// <summary>
        /// Indicates if it can be used by Priest. TODO: maybe migrate to bool.
        /// </summary>
        public byte Defensemage { get; set; }

        /// <summary>
        /// Easy, normal, ultimate.
        /// </summary>
        public Mode Grow { get; set; }

        /// <summary>
        /// Required strength to use it.
        /// </summary>
        public ushort ReqStr { get; set; }

        /// <summary>
        /// Defines "color" of item.
        /// </summary>
        public ushort ReqDex { get; set; }

        /// <summary>
        /// For lapisia, it's success rate.
        /// </summary>
        public ushort ReqRec { get; set; }

        /// <summary>
        /// Required level.
        /// </summary>
        public ushort ReqInt { get; set; }

        /// <summary>
        /// Max number of stats, that can be created during item composition (rec rune).
        /// </summary>
        public ushort ReqWis { get; set; }

        /// <summary>
        /// Required luc to use it.
        /// </summary>
        public ushort Reqluc { get; set; }

        /// <summary>
        /// For linking hammer, it's how many times it increases the success linking rate.
        /// For lapis, if it's set to 1, lapis can break equipment while unsuccessful linking.
        /// For items, that generate other items (e.g. "Mystra's Box") it's index in file PSM_Client\Bin\Data\ItemCreate.ini
        /// For lapisia, if it's to 1, lapisia can break equipment while unsuccessful enchantment.
        /// For ep 8 teleport items, it's type id of npc.
        /// </summary>
        public ushort ReqVg { get; set; }

        /// <summary>
        /// Account restrictions, i.e. trade/untradeable.
        /// </summary>
        public ItemAccountRestrictionType ReqOg { get; set; }

        /// <summary>
        /// Item cooldown.
        /// For lapis it's value of linking success.
        /// </summary>
        public byte ReqIg { get; set; }

        /// <summary>
        /// From how far away character can use this item.
        /// For mounts, its value specifies which character shape we should use.
        /// For items, that activates skills, it's skill id.
        /// For lapisia, it's min level.
        /// </summary>
        public ushort Range { get; set; }

        /// <summary>
        /// How fast this item.
        /// For mounts it's casting time in seconds.
        /// For items, that activates skills, it's skill level.
        /// For lapisia, it's max level.
        /// </summary>
        public byte AttackTime { get; set; }

        /// <summary>
        /// Item element.
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// Special effect.
        /// </summary>
        public SpecialEffect Special { get; set; }

        /// <summary>
        /// For item, how many free slots item has.
        /// For gem, how many slots lapis will take.
        /// </summary>
        public byte Slot { get; set; }

        /// <summary>
        /// Max quality of item.
        /// </summary>
        public ushort Quality { get; set; }

        /// <summary>
        /// Min attack.
        /// </summary>
        public ushort MinAttack { get; set; }

        /// <summary>
        /// Min attack + this = max attack.
        /// </summary>
        public ushort PlusAttack { get; set; }

        /// <summary>
        /// Physical defense.
        /// </summary>
        public ushort Defense { get; set; }

        /// <summary>
        /// Magic resistance.
        /// </summary>
        public ushort Resistance { get; set; }

        /// <summary>
        /// How much it adds heal points.
        /// </summary>
        public ushort ConstHP { get; set; }

        /// <summary>
        /// How much it adds stamina points.
        /// </summary>
        public ushort ConstSP { get; set; }

        /// <summary>
        /// How much it adds mana points.
        /// </summary>
        public ushort ConstMP { get; set; }

        /// <summary>
        /// How much it adds str points.
        /// </summary>
        public ushort ConstStr { get; set; }

        /// <summary>
        /// How much it adds dex points.
        /// </summary>
        public ushort ConstDex { get; set; }

        /// <summary>
        /// How much it adds rec points.
        /// </summary>
        public ushort ConstRec { get; set; }

        /// <summary>
        /// How much it adds int points.
        /// </summary>
        public ushort ConstInt { get; set; }

        /// <summary>
        /// How much it adds wis points.
        /// </summary>
        public ushort ConstWis { get; set; }

        /// <summary>
        /// How much it adds luc points.
        /// </summary>
        public ushort ConstLuc { get; set; }

        /// <summary>
        /// How fast this item.
        /// </summary>
        public byte Speed { get; set; }

        /// <summary>
        /// For lapis, it's absorption value.
        /// </summary>
        public byte Exp { get; set; }

        /// <summary>
        /// How much money character needs in order to buy this item.
        /// </summary>
        public int Buy { get; set; }

        /// <summary>
        /// How much money char will get if he sells this item.
        /// </summary>
        public int Sell { get; set; }

        /// <summary>
        /// Grade is used to generate drop from mob. Each mob has drop rate of some items, that are part of some grade.
        /// Imagine fox lvl2 it's drop grade is 1, which is apple, gold apple and green apple.
        /// </summary>
        public ushort Grade { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public ushort Drop { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public byte Server { get; set; }

        /// <summary>
        /// Max count.
        /// </summary>
        public byte Count { get; set; }

        /// <summary>
        /// Duration time in seconds for items that can expire
        /// </summary>
        public uint Duration { get; set; }
    }
}
