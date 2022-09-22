
using Parsec.Shaiya.Skill;
using Imgeneus.Database.Constants;
using Element = Imgeneus.Database.Constants.Element;
using Parsec.Shaiya.NpcSkill;
using Imgeneus.GameDefinitions.Enums;

namespace Imgeneus.GameDefinitions
{
    public class DbSkill
    {
        public DbSkill()
        {
        }

        public DbSkill(DBSkillDataRecord record)
        {
            SkillId = (ushort)record.Id;
            SkillLevel = (byte)record.SkillLevel;
            SkillUtilizer = (UserFamily)record.Country;
            UsedByFighter = (byte)record.AttackFighter;
            UsedByDefender = (byte)record.DefenseFighter;
            UsedByRanger = (byte)record.PatrolRogue;
            UsedByArcher = (byte)record.ShootRogue;
            UsedByMage = (byte)record.AttackMage;
            UsedByPriest = (byte)record.DefenseMage;
            PreviousSkillId = (ushort)record.PrevSkill;
            ReqLevel = (ushort)record.Level;
            Grow = (byte)record.Grow;
            SkillPoint = (byte)record.Point;
            TypeShow = (TypeShow)record.TypeShow;
            TypeAttack = (TypeAttack)record.TypeAttack;
            TypeEffect = (TypeEffect)record.TypeEffect;
            TypeDetail = (TypeDetail)record.Type;
            NeedWeapon1 = (byte)record.NeedWeapon1;
            NeedWeapon2 = (byte)record.NeedWeapon2;
            NeedWeapon3 = (byte)record.NeedWeapon3;
            NeedWeapon4 = (byte)record.NeedWeapon4;
            NeedWeapon5 = (byte)record.NeedWeapon5;
            NeedWeapon6 = (byte)record.NeedWeapon6;
            NeedWeapon7 = (byte)record.NeedWeapon7;
            NeedWeapon8 = (byte)record.NeedWeapon8;
            NeedWeapon9 = (byte)record.NeedWeapon9;
            NeedWeapon10 = (byte)record.NeedWeapon10;
            NeedWeapon11 = (byte)record.NeedWeapon11;
            NeedWeapon12 = (byte)record.NeedWeapon12;
            NeedWeapon13 = (byte)record.NeedWeapon13;
            NeedWeapon14 = (byte)record.NeedWeapon14;
            NeedWeapon15 = (byte)record.NeedWeapon15;
            NeedShield = (byte)record.Shield;
            SP = (ushort)record.SP;
            MP = (ushort)record.MP;
            ReadyTime = (byte)record.ReadyTime;
            ResetTime = (ushort)record.ResetTime;
            AttackRange = (byte)record.AttackRange;
            StateType = (StateType)record.StateType;
            Element = (Element)record.AttribType;
            DisabledSkill = (ushort)record.Disable;
            SuccessType = (SuccessType)record.SuccessType;
            SuccessValue = (byte)record.SuccessValue;
            TargetType = (TargetType)record.TargetType;
            ApplyRange = (byte)record.ApplyRange;
            MultiAttack = (byte)record.MultiAttack;
            KeepTime = (int)record.KeepTime;
            Weapon1 = (byte)record.Weapon1;
            Weapon2 = (byte)record.Weapon2;
            Weaponvalue = (byte)record.WeaponValue;
            DamageType = (DamageType)record.DamageType;
            DamageHP = (ushort)record.Damage1;
            DamageSP = (ushort)record.Damage2;
            DamageMP = (ushort)record.Damage3;
            TimeDamageType = (TimeDamageType)record.TimeDamageType;
            TimeDamageHP = (ushort)record.TimeDamage1;
            TimeDamageSP = (ushort)record.TimeDamage2;
            TimeDamageMP = (ushort)record.TimeDamage3;
            AddDamageHP = (ushort)record.AddDamage1;
            AddDamageSP = (ushort)record.AddDamage2;
            AddDamageMP = (ushort)record.AddDamage3;
            AbilityType1 = (AbilityType)record.AbilityType1;
            AbilityValue1 = (ushort)record.AbilityValue1;
            AbilityType2 = (AbilityType)record.AbilityType2;
            AbilityValue2 = (ushort)record.AbilityValue2;
            AbilityType3 = (AbilityType)record.AbilityType3;
            AbilityValue3 = (ushort)record.AbilityValue3;
            AbilityType4 = (AbilityType)record.AbilityType4;
            AbilityValue4 = (ushort)record.AbilityValue4;
            AbilityType5 = (AbilityType)record.AbilityType5;
            AbilityValue5 = (ushort)record.AbilityValue5;
            AbilityType6 = (AbilityType)record.AbilityType6;
            AbilityValue6 = (ushort)record.AbilityValue6;
            AbilityType7 = (AbilityType)record.AbilityType7;
            AbilityValue7 = (ushort)record.AbilityValue7;
            AbilityType8 = (AbilityType)record.AbilityType8;
            AbilityValue8 = (ushort)record.AbilityValue8;
            AbilityType9 = (AbilityType)record.AbilityType9;
            AbilityValue9 = (ushort)record.AbilityValue9;
            AbilityType10 = (AbilityType)record.AbilityType10;
            AbilityValue10 = (ushort)record.AbilityValue10;
            HealHP = (ushort)record.Heal1;
            HealMP = (ushort)record.Heal2;
            HealSP = (ushort)record.Heal3;
            TimeHealHP = (ushort)record.TimeHeal1;
            TimeHealMP = (ushort)record.TimeHeal2;
            TimeHealSP = (ushort)record.TimeHeal3;
            DefenceType = (byte)record.DefenceType;
            DefenceValue = (byte)record.DefenceValue;
            LimitHP = (byte)record.LimitHP;
            FixRange = (Duration)record.FixRange;
        }

        public DbSkill(DBNpcSkillDataRecord record)
        {
            SkillId = (ushort)record.Id;
            SkillLevel = (byte)record.SkillLevel;
            SkillUtilizer = (UserFamily)record.Country;
            UsedByFighter = (byte)record.AttackFighter;
            UsedByDefender = (byte)record.DefenseFighter;
            UsedByRanger = (byte)record.PatrolRogue;
            UsedByArcher = (byte)record.ShootRogue;
            UsedByMage = (byte)record.AttackMage;
            UsedByPriest = (byte)record.DefenseMage;
            PreviousSkillId = (ushort)record.PrevSkill;
            ReqLevel = (ushort)record.Level;
            Grow = (byte)record.Grow;
            SkillPoint = (byte)record.Point;
            TypeShow = (TypeShow)record.TypeShow;
            TypeAttack = (TypeAttack)record.TypeAttack;
            TypeEffect = (TypeEffect)record.TypeEffect;
            TypeDetail = (TypeDetail)record.Type;
            NeedWeapon1 = (byte)record.NeedWeapon1;
            NeedWeapon2 = (byte)record.NeedWeapon2;
            NeedWeapon3 = (byte)record.NeedWeapon3;
            NeedWeapon4 = (byte)record.NeedWeapon4;
            NeedWeapon5 = (byte)record.NeedWeapon5;
            NeedWeapon6 = (byte)record.NeedWeapon6;
            NeedWeapon7 = (byte)record.NeedWeapon7;
            NeedWeapon8 = (byte)record.NeedWeapon8;
            NeedWeapon9 = (byte)record.NeedWeapon9;
            NeedWeapon10 = (byte)record.NeedWeapon10;
            NeedWeapon11 = (byte)record.NeedWeapon11;
            NeedWeapon12 = (byte)record.NeedWeapon12;
            NeedWeapon13 = (byte)record.NeedWeapon13;
            NeedWeapon14 = (byte)record.NeedWeapon14;
            NeedWeapon15 = (byte)record.NeedWeapon15;
            NeedShield = (byte)record.Shield;
            SP = (ushort)record.SP;
            MP = (ushort)record.MP;
            ReadyTime = (byte)record.ReadyTime;
            ResetTime = (ushort)record.ResetTime;
            AttackRange = (byte)record.AttackRange;
            StateType = (StateType)record.StateType;
            Element = (Element)record.AttribType;
            DisabledSkill = (ushort)record.Disable;
            SuccessType = (SuccessType)record.SuccessType;
            SuccessValue = (byte)record.SuccessValue;
            TargetType = (TargetType)record.TargetType;
            ApplyRange = (byte)record.ApplyRange;
            MultiAttack = (byte)record.MultiAttack;
            KeepTime = (int)record.KeepTime;
            Weapon1 = (byte)record.Weapon1;
            Weapon2 = (byte)record.Weapon2;
            Weaponvalue = (byte)record.WeaponValue;
            DamageType = (DamageType)record.DamageType;
            DamageHP = (ushort)record.Damage1;
            DamageSP = (ushort)record.Damage2;
            DamageMP = (ushort)record.Damage3;
            TimeDamageType = (TimeDamageType)record.TimeDamageType;
            TimeDamageHP = (ushort)record.TimeDamage1;
            TimeDamageSP = (ushort)record.TimeDamage2;
            TimeDamageMP = (ushort)record.TimeDamage3;
            AddDamageHP = (ushort)record.AddDamage1;
            AddDamageSP = (ushort)record.AddDamage2;
            AddDamageMP = (ushort)record.AddDamage3;
            AbilityType1 = (AbilityType)record.AbilityType1;
            AbilityValue1 = (ushort)record.AbilityValue1;
            AbilityType2 = (AbilityType)record.AbilityType2;
            AbilityValue2 = (ushort)record.AbilityValue2;
            AbilityType3 = (AbilityType)record.AbilityType3;
            AbilityValue3 = (ushort)record.AbilityValue3;
            AbilityType4 = (AbilityType)record.AbilityType4;
            AbilityValue4 = (ushort)record.AbilityValue4;
            AbilityType5 = (AbilityType)record.AbilityType5;
            AbilityValue5 = (ushort)record.AbilityValue5;
            AbilityType6 = (AbilityType)record.AbilityType6;
            AbilityValue6 = (ushort)record.AbilityValue6;
            AbilityType7 = (AbilityType)record.AbilityType7;
            AbilityValue7 = (ushort)record.AbilityValue7;
            AbilityType8 = (AbilityType)record.AbilityType8;
            AbilityValue8 = (ushort)record.AbilityValue8;
            AbilityType9 = (AbilityType)record.AbilityType9;
            AbilityValue9 = (ushort)record.AbilityValue9;
            AbilityType10 = (AbilityType)record.AbilityType10;
            AbilityValue10 = (ushort)record.AbilityValue10;
            HealHP = (ushort)record.Heal1;
            HealMP = (ushort)record.Heal2;
            HealSP = (ushort)record.Heal3;
            TimeHealHP = (ushort)record.TimeHeal1;
            TimeHealMP = (ushort)record.TimeHeal2;
            TimeHealSP = (ushort)record.TimeHeal3;
            DefenceType = (byte)record.DefenceType;
            DefenceValue = (byte)record.DefenceValue;
            LimitHP = (byte)record.LimitHP;
            FixRange = (Duration)record.FixRange;
        }

        /// <summary>
        /// Id of skill.
        /// </summary>
        public ushort SkillId { get; set; }

        /// <summary>
        /// Level of skill.
        /// </summary>
        public byte SkillLevel { get; set; }

        /// <summary>
        /// Which faction and profession can use this skill.
        /// </summary>
        public UserFamily SkillUtilizer { get; set; }

        /// <summary>
        /// Indicates if skill can be used by fighter. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByFighter { get; set; }

        /// <summary>
        /// Indicates if skill can be used by defender. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByDefender { get; set; }

        /// <summary>
        /// Indicates if skill can be used by ranger. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByRanger { get; set; }

        /// <summary>
        /// Indicates if skill can be used by archer. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByArcher { get; set; }

        /// <summary>
        /// Indicates if skill can be used by mage. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByMage { get; set; }

        /// <summary>
        /// Indicates if skill can be used by priest. Maybe this can be migrated to bool...
        /// </summary>
        public byte UsedByPriest { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public ushort PreviousSkillId { get; set; }

        /// <summary>
        /// Required level of skill.
        /// </summary>
        public ushort ReqLevel { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public byte Grow { get; set; }

        /// <summary>
        /// How many skill points are needed in order to learn this skill.
        /// </summary>
        public byte SkillPoint { get; set; }

        /// <summary>
        /// Category of skill. E.g. combat or special.
        /// </summary>
        public TypeShow TypeShow { get; set; }

        /// <summary>
        /// Passive, physical, magic or shooting attack.
        /// </summary>
        public TypeAttack TypeAttack { get; set; }

        /// <summary>
        /// Additional effect description.
        /// </summary>
        public TypeEffect TypeEffect { get; set; }

        /// <summary>
        /// Type detail describes what skill does.
        /// </summary>
        public TypeDetail TypeDetail { get; set; }

        /// <summary>
        /// Skill requires 1 Handed Sword.
        /// </summary>
        public byte NeedWeapon1 { get; set; }

        /// <summary>
        /// Skill requires 2 Handed Sword.
        /// </summary>
        public byte NeedWeapon2 { get; set; }

        /// <summary>
        /// Skill requires 1 Handed Axe.
        /// </summary>
        public byte NeedWeapon3 { get; set; }

        /// <summary>
        /// Skill requires 2 Handed Axe.
        /// </summary>
        public byte NeedWeapon4 { get; set; }

        /// <summary>
        /// Skill requires Double Sword.
        /// </summary>
        public byte NeedWeapon5 { get; set; }

        /// <summary>
        /// Skill requires Spear.
        /// </summary>
        public byte NeedWeapon6 { get; set; }

        /// <summary>
        /// Skill requires 1 Handed Blunt.
        /// </summary>
        public byte NeedWeapon7 { get; set; }

        /// <summary>
        /// Skill requires 2 Handed Blunt.
        /// </summary>
        public byte NeedWeapon8 { get; set; }

        /// <summary>
        /// Skill requires Reverse sword.
        /// </summary>
        public byte NeedWeapon9 { get; set; }

        /// <summary>
        /// Skill requires Dagger.
        /// </summary>
        public byte NeedWeapon10 { get; set; }

        /// <summary>
        /// Skill requires Javelin.
        /// </summary>
        public byte NeedWeapon11 { get; set; }

        /// <summary>
        /// Skill requires Staff.
        /// </summary>
        public byte NeedWeapon12 { get; set; }

        /// <summary>
        /// Skill requires Bow.
        /// </summary>
        public byte NeedWeapon13 { get; set; }

        /// <summary>
        /// Skill requires Crossbow.
        /// </summary>
        public byte NeedWeapon14 { get; set; }

        /// <summary>
        /// Skill requires Knuckle.
        /// </summary>
        public byte NeedWeapon15 { get; set; }

        /// <summary>
        /// Skill requires shield.
        /// </summary>
        public byte NeedShield { get; set; }

        /// <summary>
        /// How many stamina points requires this skill.
        /// </summary>
        public ushort SP { get; set; }

        /// <summary>
        /// How many mana points requires this skill.
        /// </summary>
        public ushort MP { get; set; }

        /// <summary>
        /// Cast time.
        /// </summary>
        public byte ReadyTime { get; set; }

        /// <summary>
        /// Time after which skill can be used again.
        /// </summary>
        public ushort ResetTime { get; set; }

        /// <summary>
        /// How many meters are needed in order to use skill.
        /// </summary>
        public byte AttackRange { get; set; }

        /// <summary>
        /// State type contains information about what bad influence debuff has on target.
        /// </summary>
        public StateType StateType { get; set; }

        /// <summary>
        /// Skill element.
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// TODO: ?
        /// </summary>
        public ushort DisabledSkill { get; set; }

        /// <summary>
        /// SuccessType is always 0 for passive skills and 1 for other.
        /// </summary>
        public SuccessType SuccessType { get; set; }

        /// <summary>
        /// Success chance in %.
        /// </summary>
        public byte SuccessValue { get; set; }

        /// <summary>
        /// What target is required for this skill.
        /// </summary>
        public TargetType TargetType { get; set; }

        /// <summary>
        /// Skill will be applied within N meters.
        /// </summary>
        public byte ApplyRange { get; set; }

        /// <summary>
        /// Used in multiple skill attacks.
        /// </summary>
        public byte MultiAttack { get; set; }

        /// <summary>
        /// Time for example for buffs. This time shows how long the skill will be applied.
        /// NB! It was ushort in original db. But I could not migrate it! Changed to int.
        /// </summary>
        public int KeepTime { get; set; }

        /// <summary>
        /// Only for passive skills; Weapon type to which passive skill speed modificator can be applied.
        /// </summary>
        public byte Weapon1 { get; set; }

        /// <summary>
        /// Only for passive skills; Weapon type to which passive skill speed modificator can be applied.
        /// </summary>
        public byte Weapon2 { get; set; }

        /// <summary>
        /// Only for passive skills; passive skill speed modificator or passive attack power up.
        /// </summary>
        public byte Weaponvalue { get; set; }

        /// <summary>
        /// Damage type.
        /// </summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageHP { get; set; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageSP { get; set; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageMP { get; set; }

        /// <summary>
        /// Time damage type.
        /// </summary>
        public TimeDamageType TimeDamageType { get; set; }

        /// <summary>
        /// Either fixed hp or % hp damage made over time.
        /// </summary>
        public ushort TimeDamageHP { get; set; }

        /// <summary>
        /// Either fixed sp or % sp damage made over time.
        /// </summary>
        public ushort TimeDamageSP { get; set; }

        /// <summary>
        /// Either fixed mp or % mp damage made over time.
        /// </summary>
        public ushort TimeDamageMP { get; set; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageHP { get; set; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageSP { get; set; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageMP { get; set; }

        public AbilityType AbilityType1 { get; set; }

        public ushort AbilityValue1 { get; set; }

        public AbilityType AbilityType2 { get; set; }

        public ushort AbilityValue2 { get; set; }

        public AbilityType AbilityType3 { get; set; }

        public ushort AbilityValue3 { get; set; }

        public AbilityType AbilityType4 { get; set; }

        public ushort AbilityValue4 { get; set; }

        public AbilityType AbilityType5 { get; set; }

        public ushort AbilityValue5 { get; set; }

        public AbilityType AbilityType6 { get; set; }

        public ushort AbilityValue6 { get; set; }

        public AbilityType AbilityType7 { get; set; }

        public ushort AbilityValue7 { get; set; }

        public AbilityType AbilityType8 { get; set; }

        public ushort AbilityValue8 { get; set; }

        public AbilityType AbilityType9 { get; set; }

        public ushort AbilityValue9 { get; set; }

        public AbilityType AbilityType10 { get; set; }

        public ushort AbilityValue10 { get; set; }

        /// <summary>
        /// How many health points can be healed.
        /// </summary>
        public ushort HealHP { get; set; }

        /// <summary>
        /// How many mana points can be healed.
        /// </summary>
        public ushort HealMP { get; set; }

        /// <summary>
        /// How many stamina points can be healed.
        /// </summary>
        public ushort HealSP { get; set; }

        /// <summary>
        /// HP healed over time.
        /// </summary>
        public ushort TimeHealHP { get; set; }

        /// <summary>
        /// MP healed over time.
        /// </summary>
        public ushort TimeHealMP { get; set; }

        /// <summary>
        /// SP healed over time.
        /// </summary>
        public ushort TimeHealSP { get; set; }

        /// <summary>
        /// For "Fleet Foot" it's value 2, which is block shoot attack for X %.
        /// For "Magic Veil" it's value 3, which is block X magic attacks.
        /// </summary>
        public byte DefenceType { get; set; }

        /// <summary>
        /// When <see cref="DefenceType"/> is 2, it's % of blocked shoot attacks.
        /// When <see cref="DefenceType"/> is 3, it's block X magic attacks.
        /// </summary>
        public byte DefenceValue { get; set; }

        /// <summary>
        /// % of hp, when this skill is activated.
        /// </summary>
        public byte LimitHP { get; set; }

        /// <summary>
        /// Is buff should be cleared after character death?
        /// </summary>
        public Duration FixRange { get; set; }
    }
}
