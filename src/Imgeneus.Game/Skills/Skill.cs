using Imgeneus.GameDefinitions;
using Imgeneus.GameDefinitions.Enums;
using Parsec.Shaiya.Skill;
using System.Collections.Generic;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.Game.Skills
{
    public class Skill
    {
        public const ushort CHARGE_SKILL_ID = 56;
        public const ushort CHARGE_EP_8_SKILL_ID = 625;

        private readonly DbSkill _dbSkill;
        public Skill(DbSkill dbSkill, byte skillNumber, int cooldown)
        {
            Number = skillNumber;
            CooldownInSeconds = cooldown;
            _dbSkill = dbSkill;
        }

        /// <summary>
        /// Skill id.
        /// </summary>
        public ushort SkillId { get => _dbSkill.SkillId; }

        /// <summary>
        /// Skill level.
        /// </summary>
        public byte SkillLevel { get => _dbSkill.SkillLevel; }

        /// <summary>
        /// Number. This value client sends, when player used any skill.
        /// </summary>
        public byte Number;

        /// <summary>
        /// Countdown in seconds.
        /// </summary>
        public int CooldownInSeconds;

        /// <summary>
        /// Is it pure passive skill? From passive tab.
        /// </summary>
        public bool IsPassive => TypeAttack == TypeAttack.Passive &&
            Type != TypeDetail.Buff &&
            Type != TypeDetail.BlockMagicAttack &&
            Type != TypeDetail.Untouchable &&
            Type != TypeDetail.Stealth &&
            Type != TypeDetail.Sleep &&
            Type != TypeDetail.TownPortal &&
            Type != TypeDetail.BlockShootingAttack &&
            Type != TypeDetail.Transformation &&
            Type != TypeDetail.Eraser &&
            Type != TypeDetail.PeriodicalHeal &&
            Type != TypeDetail.EtainShield &&
            Type != TypeDetail.DamageReflection &&
            Type != TypeDetail.PersistBarrier &&
            Type != TypeDetail.Provoke &&
            Type != TypeDetail.DungeonMapScroll &&
            Type != TypeDetail.AbilityExchange;

        /// <summary>
        /// Skill type.
        /// </summary>
        public TypeDetail Type { get => _dbSkill.TypeDetail; }

        /// <summary>
        /// Skill is meant for allies?
        /// </summary>
        public bool IsForAlly { get => _dbSkill.TypeEffect == TypeEffect.HealingDispel || _dbSkill.TypeEffect == TypeEffect.Buff || _dbSkill.TypeEffect == TypeEffect.BuffNoss || _dbSkill.TypeDetail == TypeDetail.Resurrection; }

        /// <summary>
        /// Skill is made for rangers?
        /// </summary>
        public bool IsUsedByRanger { get => _dbSkill.UsedByRanger == 1; }

        /// <summary>
        /// To what mob are we going to transform?
        /// </summary>
        public ushort MobShape { get; set; }

        /// <summary>
        /// To which character are we going to transform?
        /// </summary>
        public uint CharacterId { get; set; }

        /// <summary>
        /// To what target this skill can be applied.
        /// </summary>
        public TargetType TargetType { get => _dbSkill.TargetType; }

        /// <summary>
        /// Passive, physical, magic or shooting attack.
        /// </summary>
        public TypeAttack TypeAttack { get => _dbSkill.TypeAttack; }

        /// <summary>
        /// Bool indicator, that shows if we need to use success value in calculations.
        /// </summary>
        public bool UseSuccessValue { get => _dbSkill.SuccessType == SuccessType.SuccessBasedOnValue; }

        /// <summary>
        /// Success rate in %.
        /// </summary>
        public byte SuccessValue { get => _dbSkill.SuccessValue; }

        /// <summary>
        /// State type contains information about what bad influence debuff has on target.
        /// </summary>
        public StateType StateType { get => _dbSkill.StateType; }

        /// <summary>
        /// Category of skill. E.g. combat or special.
        /// </summary>
        public TypeShow TypeShow;

        public TypeEffect TypeEffect { get => _dbSkill.TypeEffect; }

        /// <summary>
        /// Time after which skill can be used again.
        /// </summary>
        public ushort ResetTime { get => _dbSkill.ResetTime; }

        /// <summary>
        /// Time for example for buffs. This time shows how long the skill will be applied.
        /// </summary>
        public int KeepTime { get => _dbSkill.KeepTime; }

        /// <summary>
        /// How long character should wait until skill is casted. In milliseconds.
        /// </summary>
        public int CastTime { get => _dbSkill.ReadyTime * 250; }

        /// <summary>
        /// Damage type;
        /// </summary>
        public DamageType DamageType { get => _dbSkill.DamageType; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageHP { get => _dbSkill.DamageHP; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageSP { get => _dbSkill.DamageSP; }

        /// <summary>
        /// Const damage used, when skill makes fixed damage.
        /// </summary>
        public ushort DamageMP { get => _dbSkill.DamageMP; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageHP { get => _dbSkill.AddDamageHP; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageSP { get => _dbSkill.AddDamageSP; }

        /// <summary>
        /// Const skill damage, that is added to damage made of stats.
        /// </summary>
        public ushort AddDamageMP { get => _dbSkill.AddDamageMP; }

        /// <summary>
        /// How many health points can be healed.
        /// </summary>
        public ushort HealHP { get => _dbSkill.HealHP; }

        /// <summary>
        /// How many mana points can be healed.
        /// </summary>
        public ushort HealMP { get => _dbSkill.HealMP; }

        /// <summary>
        /// How many stamina points can be healed.
        /// </summary>
        public ushort HealSP { get => _dbSkill.HealSP; }

        /// <summary>
        /// Skill const element.
        /// </summary>
        public Element Element { get => _dbSkill.Element; }

        /// <summary>
        /// Skill will be applied within N meters.
        /// </summary>
        public byte ApplyRange { get => _dbSkill.ApplyRange; }

        /// <summary>
        /// How many meters are needed in order to use skill.
        /// </summary>
        public byte AttackRange { get => _dbSkill.AttackRange; }

        /// <summary>
        /// Skill damage will be multiplied by this number.
        /// </summary>
        public byte MultiAttack { get => _dbSkill.MultiAttack; }

        /// <summary>
        /// After character death this skill is cleared from buffs list?
        /// </summary>
        public bool ShouldClearAfterDeath { get => _dbSkill.FixRange == Duration.ClearAfterDeath; }

        public Duration Duration { get => _dbSkill.FixRange; }

        /// <summary>
        /// When skill should be activated.
        /// </summary>
        public byte LimitHP { get => _dbSkill.LimitHP; }

        /// <summary>
        /// Indicates in skill can be activated/disactivated. Skills like "Berserker's Rage".
        /// </summary>
        public bool CanBeActivated
        {
            get =>
                _dbSkill.AbilityType1 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType2 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType3 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType4 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType5 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType6 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType7 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType8 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType9 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType10 == AbilityType.SacrificeHPPercent ||
                _dbSkill.AbilityType1 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType2 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType3 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType4 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType5 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType6 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType7 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType8 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType9 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType10 == AbilityType.SacrificeMPPercent ||
                _dbSkill.AbilityType1 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType2 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType3 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType4 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType5 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType6 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType7 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType8 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType9 == AbilityType.SacrificeSPPercent ||
                _dbSkill.AbilityType10 == AbilityType.SacrificeSPPercent;
        }

        /// <summary>
        /// Case for skills such as "Berserker's Rage", that are activated and disactivated.
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// How much stamina is needed in order to use this skill.
        /// </summary>
        public ushort NeedSP { get => _dbSkill.SP; }

        /// <summary>
        /// How much mana is needed in order to use this skill.
        /// </summary>
        public ushort NeedMP { get => _dbSkill.MP; }

        /// <summary>
        /// Previous skill id.
        /// </summary>
        public ushort PrevSkillId { get => _dbSkill.PreviousSkillId; }

        /// <summary>
        /// This skill required stealth shape?
        /// </summary>
        public bool IsUsedFromStealth { get => _dbSkill.SkillId == 700 || _dbSkill.SkillId == 697 || _dbSkill.SkillId == 309 || _dbSkill.SkillId == 310; } // "Shadow Stab" or "Shadow Sleep"

        /// <summary>
        /// Needs 1 Handed Sword.
        /// </summary>
        public bool NeedWeapon1 { get => _dbSkill.NeedWeapon1 == 1; }

        /// <summary>
        /// Needs 2 Handed Sword.
        /// </summary>
        public bool NeedWeapon2 { get => _dbSkill.NeedWeapon2 == 1; }

        /// <summary>
        /// Needs 1 Handed Axe.
        /// </summary>
        public bool NeedWeapon3 { get => _dbSkill.NeedWeapon3 == 1; }

        /// <summary>
        /// Needs 2 Handed Axe.
        /// </summary>
        public bool NeedWeapon4 { get => _dbSkill.NeedWeapon4 == 1; }

        /// <summary>
        /// Needs Double Sword.
        /// </summary>
        public bool NeedWeapon5 { get => _dbSkill.NeedWeapon5 == 1; }

        /// <summary>
        /// Needs 1 Spear.
        /// </summary>
        public bool NeedWeapon6 { get => _dbSkill.NeedWeapon6 == 1; }

        /// <summary>
        /// Needs 1 Handed Blunt.
        /// </summary>
        public bool NeedWeapon7 { get => _dbSkill.NeedWeapon7 == 1; }

        /// <summary>
        /// Needs 2 Handed Blunt.
        /// </summary>
        public bool NeedWeapon8 { get => _dbSkill.NeedWeapon8 == 1; }

        /// <summary>
        /// Needs Reverse sword.
        /// </summary>
        public bool NeedWeapon9 { get => _dbSkill.NeedWeapon9 == 1; }

        /// <summary>
        /// Needs Dagger.
        /// </summary>
        public bool NeedWeapon10 { get => _dbSkill.NeedWeapon10 == 1; }

        /// <summary>
        /// Needs Javelin.
        /// </summary>
        public bool NeedWeapon11 { get => _dbSkill.NeedWeapon11 == 1; }

        /// <summary>
        /// Needs 1 Staff.
        /// </summary>
        public bool NeedWeapon12 { get => _dbSkill.NeedWeapon12 == 1; }

        /// <summary>
        /// Needs Bow.
        /// </summary>
        public bool NeedWeapon13 { get => _dbSkill.NeedWeapon13 == 1; }

        /// <summary>
        /// Needs Crossbow.
        /// </summary>
        public bool NeedWeapon14 { get => _dbSkill.NeedWeapon14 == 1; }

        /// <summary>
        /// Needs Knuckle.
        /// </summary>
        public bool NeedWeapon15 { get => _dbSkill.NeedWeapon15 == 1; }

        private List<byte> _requiredWeapons;
        public List<byte> RequiredWeapons
        {
            get
            {
                // Maybe it's not the best way to check required weapon, but i don't care.
                // I just added item type, that is right compering to NeedWeaponN in original db.
                // E.g. NeedWeapon1 means we need 1 hand sword, which is Type 1 and Type 45.
                if (_requiredWeapons is null)
                {
                    _requiredWeapons = new List<byte>();

                    if (NeedWeapon1)
                    {
                        _requiredWeapons.Add(1);
                        _requiredWeapons.Add(45);
                    }
                    if (NeedWeapon2)
                    {
                        _requiredWeapons.Add(2);
                        _requiredWeapons.Add(46);
                    }
                    if (NeedWeapon3)
                    {
                        _requiredWeapons.Add(3);
                        _requiredWeapons.Add(47);
                    }
                    if (NeedWeapon4)
                    {
                        _requiredWeapons.Add(4);
                        _requiredWeapons.Add(48);
                    }
                    if (NeedWeapon5)
                    {
                        _requiredWeapons.Add(5);
                        _requiredWeapons.Add(49);
                        _requiredWeapons.Add(50);
                    }
                    if (NeedWeapon6)
                    {
                        _requiredWeapons.Add(6);
                        _requiredWeapons.Add(51);
                        _requiredWeapons.Add(52);
                    }
                    if (NeedWeapon7)
                    {
                        _requiredWeapons.Add(7);
                        _requiredWeapons.Add(53);
                        _requiredWeapons.Add(54);
                    }
                    if (NeedWeapon8)
                    {
                        _requiredWeapons.Add(8);
                        _requiredWeapons.Add(55);
                        _requiredWeapons.Add(56);
                    }
                    if (NeedWeapon9)
                    {
                        _requiredWeapons.Add(9);
                        _requiredWeapons.Add(57);
                    }
                    if (NeedWeapon10)
                    {
                        _requiredWeapons.Add(10);
                        _requiredWeapons.Add(58);
                    }
                    if (NeedWeapon11)
                    {
                        _requiredWeapons.Add(11);
                        _requiredWeapons.Add(59);
                    }
                    if (NeedWeapon12)
                    {
                        _requiredWeapons.Add(12);
                        _requiredWeapons.Add(60);
                        _requiredWeapons.Add(61);
                    }
                    if (NeedWeapon13)
                    {
                        _requiredWeapons.Add(13);
                        _requiredWeapons.Add(62);
                        _requiredWeapons.Add(63);
                    }
                    if (NeedWeapon14)
                    {
                        _requiredWeapons.Add(14);
                        _requiredWeapons.Add(64);
                    }
                    if (NeedWeapon15)
                    {
                        _requiredWeapons.Add(15);
                        _requiredWeapons.Add(65);
                    }
                }

                return _requiredWeapons;
            }
        }

        /// <summary>
        /// Needs Shield.
        /// </summary>
        public bool NeedShield { get => _dbSkill.NeedShield == 1; }


        /// <summary>
        /// Workaround for "Deadly Poison".
        /// </summary>
        public ushort InitialDamage { get; set; }
    }
}
