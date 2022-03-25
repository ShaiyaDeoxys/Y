using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Untouchable;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Buffs
{
    public class BuffsManager : IBuffsManager
    {
        private readonly ILogger<BuffsManager> _logger;
        private readonly IDatabase _database;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IElementProvider _elementProvider;
        private readonly IUntouchableManager _untouchableManager;
        private readonly IStealthManager _stealthManager;
        private int _ownerId;

        public BuffsManager(ILogger<BuffsManager> logger, IDatabase database, IDatabasePreloader databasePreloader, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager, IElementProvider elementProvider, IUntouchableManager untouchableManager, IStealthManager stealthManager)
        {
            _logger = logger;
            _database = database;
            _databasePreloader = databasePreloader;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _speedManager = speedManager;
            _elementProvider = elementProvider;
            _untouchableManager = untouchableManager;
            _stealthManager = stealthManager;

            _healthManager.OnDead += HealthManager_OnDead;
            _healthManager.HP_Changed += HealthManager_HP_Changed;

            ActiveBuffs.CollectionChanged += ActiveBuffs_CollectionChanged;
            PassiveBuffs.CollectionChanged += PassiveBuffs_CollectionChanged;

#if DEBUG
            _logger.LogDebug("BuffsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~BuffsManager()
        {
            _logger.LogDebug("BuffsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, IEnumerable<DbCharacterActiveBuff> initBuffs = null)
        {
            _ownerId = ownerId;

            if (initBuffs != null)
            {
                var buffs = new List<Buff>();
                foreach (var b in initBuffs)
                    buffs.Add(Buff.FromDbCharacterActiveBuff(b, _databasePreloader.SkillsById[b.SkillId]));

                ActiveBuffs.AddRange(buffs);
            }
        }

        public async Task Clear()
        {
            var oldBuffs = _database.ActiveBuffs.Where(b => b.CharacterId == _ownerId);
            _database.ActiveBuffs.RemoveRange(oldBuffs);

            foreach (var b in ActiveBuffs)
            {
                var dbBuff = new DbCharacterActiveBuff()
                {
                    CharacterId = _ownerId,
                    SkillId = b.SkillUniqueId,
                    ResetTime = b.ResetTime
                };
                _database.ActiveBuffs.Add(dbBuff);
            }

            await _database.SaveChangesAsync();

            // Cancel buff after saving it to db, as buffs are not shared between sessions.
            foreach (var buff in ActiveBuffs.ToList())
                buff.CancelBuff();

            foreach (var buff in PassiveBuffs.ToList())
                buff.CancelBuff();
        }

        public void Dispose()
        {
            ActiveBuffs.CollectionChanged -= ActiveBuffs_CollectionChanged;
            PassiveBuffs.CollectionChanged -= PassiveBuffs_CollectionChanged;
            _healthManager.OnDead -= HealthManager_OnDead;
            _healthManager.HP_Changed -= HealthManager_HP_Changed;
        }

        #endregion

        #region Active buffs

        public ObservableRangeCollection<Buff> ActiveBuffs { get; private set; } = new ObservableRangeCollection<Buff>();

        public event Action<int, Buff> OnBuffAdded;

        public event Action<int, Buff> OnBuffRemoved;

        public event Action<int, Buff, AttackResult> OnSkillKeep;

        /// <summary>
        /// Fired, when new buff added or old deleted.
        /// </summary>
        private void ActiveBuffs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Buff newBuff in e.NewItems)
                {
                    newBuff.OnReset += ActiveBuff_OnReset;
                    ApplyBuffSkill(newBuff);
                }

                // Case, when we are starting up and all skills are added with AddRange call.
                if (e.NewItems.Count != 1)
                {
                    return;
                }

                OnBuffAdded?.Invoke(_ownerId, (Buff)e.NewItems[0]);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var buff = (Buff)e.OldItems[0];
                RelieveBuffSkill(buff);
                OnBuffRemoved?.Invoke(_ownerId, buff);
            }
        }

        private void ActiveBuff_OnReset(Buff sender)
        {
            sender.OnReset -= ActiveBuff_OnReset;
            sender.OnPeriodicalHeal -= Buff_OnPeriodicalHeal;
            sender.OnPeriodicalDebuff -= Buff_OnPeriodicalDebuff;

            ActiveBuffs.Remove(sender);
        }

        #endregion

        #region Passive buffs

        public ObservableRangeCollection<Buff> PassiveBuffs { get; private set; } = new ObservableRangeCollection<Buff>();

        private void PassiveBuffs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Buff newBuff in e.NewItems)
                {
                    newBuff.OnReset += PassiveBuff_OnReset;
                    ApplyBuffSkill(newBuff);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var buff = (Buff)e.OldItems[0];
                RelieveBuffSkill(buff);
            }
        }

        private void PassiveBuff_OnReset(Buff sender)
        {
            sender.OnReset -= PassiveBuff_OnReset;
            PassiveBuffs.Remove(sender);
        }

        #endregion

        #region Add/Remove buff

        public Buff AddBuff(Skill skill, IKiller creator)
        {
            var resetTime = skill.KeepTime == 0 ? DateTime.UtcNow.AddDays(10) : DateTime.UtcNow.AddSeconds(skill.KeepTime);
            Buff buff;

            if (skill.IsPassive)
            {
                buff = PassiveBuffs.FirstOrDefault(b => b.SkillId == skill.SkillId);
            }
            else
            {
                buff = ActiveBuffs.FirstOrDefault(b => b.SkillId == skill.SkillId);
            }

            if (buff != null) // We already have such buff. Try to update reset time.
            {
                if (buff.SkillLevel > skill.SkillLevel)
                {
                    // Do nothing, if target already has higher lvl buff.
                    return buff;
                }
                else
                {
                    // If buffs are the same level, we should only update reset time.
                    if (buff.SkillLevel == skill.SkillLevel)
                    {
                        buff.ResetTime = resetTime;

                        // Send update of buff.
                        if (!buff.IsPassive)
                            OnBuffAdded?.Invoke(_ownerId, buff);
                    }

                    if (buff.SkillLevel < skill.SkillLevel)
                    {
                        // Remove old buff.
                        if (buff.IsPassive)
                            buff.CancelBuff();
                        else
                            buff.CancelBuff();

                        // Create new one with a higher level.
                        buff = new Buff(creator, skill)
                        {
                            ResetTime = resetTime
                        };
                        if (skill.IsPassive)
                            PassiveBuffs.Add(buff);
                        else
                            ActiveBuffs.Add(buff);
                    }
                }
            }
            else
            {
                // It's a new buff.
                buff = new Buff(creator, skill)
                {
                    ResetTime = resetTime
                };
                if (skill.IsPassive)
                    PassiveBuffs.Add(buff);
                else
                    ActiveBuffs.Add(buff);
            }

            return buff;
        }

        #endregion

        #region Buff effects

        /// <summary>
        /// Applies buff effect.
        /// </summary>
        protected void ApplyBuffSkill(Buff buff)
        {
            var skill = _databasePreloader.Skills[(buff.SkillId, buff.SkillLevel)];
            switch (skill.TypeDetail)
            {
                case TypeDetail.Buff:
                case TypeDetail.PassiveDefence:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.SubtractingDebuff:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.PeriodicalHeal:
                    buff.TimeHealHP = skill.TimeHealHP;
                    buff.TimeHealMP = skill.TimeHealMP;
                    buff.TimeHealSP = skill.TimeHealSP;
                    buff.OnPeriodicalHeal += Buff_OnPeriodicalHeal;
                    buff.StartPeriodicalHeal();
                    break;

                case TypeDetail.PeriodicalDebuff:
                    buff.TimeHPDamage = skill.TimeDamageHP;
                    buff.TimeMPDamage = skill.TimeDamageMP;
                    buff.TimeSPDamage = skill.TimeDamageSP;
                    buff.TimeDamageType = skill.TimeDamageType;
                    buff.OnPeriodicalDebuff += Buff_OnPeriodicalDebuff;
                    buff.StartPeriodicalDebuff();
                    break;

                case TypeDetail.Immobilize:
                    _speedManager.Immobilize = true;
                    break;

                case TypeDetail.PreventAttack:
                    // ?
                    break;

                case TypeDetail.Stealth:
                    _stealthManager.IsStealth = true;

                    var sprinterBuff = ActiveBuffs.FirstOrDefault(b => b.SkillId == 681 || b.SkillId == 114); // 114 (old ep) 681 (new ep) are unique numbers for sprinter buff.
                    if (sprinterBuff != null)
                        sprinterBuff.CancelBuff();
                    break;

                case TypeDetail.WeaponMastery:
                    if (skill.Weapon1 != 0)
                    {
                        _speedManager.WeaponSpeedPassiveSkillModificator.Add(skill.Weapon1, skill.Weaponvalue);
                        _speedManager.RaisePassiveModificatorChanged(skill.Weapon1, skill.Weaponvalue, true);
                    }

                    if (skill.Weapon2 != 0)
                    {
                        _speedManager.WeaponSpeedPassiveSkillModificator.Add(skill.Weapon2, skill.Weaponvalue);
                        _speedManager.RaisePassiveModificatorChanged(skill.Weapon2, skill.Weaponvalue, true);
                    }

                    break;

                case TypeDetail.RemoveAttribute:
                    _elementProvider.IsRemoveElement = true;
                    break;

                case TypeDetail.ElementalAttack:
                    var elementSkin = ActiveBuffs.FirstOrDefault(b => b.IsElementalWeapon && b != buff);
                    if (elementSkin != null)
                        elementSkin.CancelBuff();

                    _elementProvider.AttackSkillElement = skill.Element;
                    break;

                case TypeDetail.ElementalProtection:
                    var elementWeapon = ActiveBuffs.FirstOrDefault(b => b.IsElementalProtection && b != buff);
                    if (elementWeapon != null)
                        elementWeapon.CancelBuff();

                    _elementProvider.DefenceSkillElement = skill.Element;
                    break;

                case TypeDetail.Untouchable:
                    _untouchableManager.IsUntouchable = true;
                    break;

                default:
                    _logger.LogError("Not implemented buff skill type {skillType}.", skill.TypeDetail);
                    break;
            }
        }


        /// <summary>
        /// Removes buff effect.
        /// </summary>
        protected void RelieveBuffSkill(Buff buff)
        {
            var skill = _databasePreloader.Skills[(buff.SkillId, buff.SkillLevel)];
            switch (skill.TypeDetail)
            {
                case TypeDetail.Buff:
                case TypeDetail.PassiveDefence:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.SubtractingDebuff:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.PeriodicalHeal:
                    buff.OnPeriodicalHeal -= Buff_OnPeriodicalHeal;
                    break;

                case TypeDetail.PeriodicalDebuff:
                    buff.OnPeriodicalDebuff -= Buff_OnPeriodicalDebuff;
                    break;

                case TypeDetail.Immobilize:
                    _speedManager.Immobilize = ActiveBuffs.Any(b => _databasePreloader.Skills[(b.SkillId, b.SkillLevel)].TypeDetail == TypeDetail.Immobilize);
                    break;

                case TypeDetail.PreventAttack:
                    // ?
                    break;

                case TypeDetail.Stealth:
                    _stealthManager.IsStealth = ActiveBuffs.Any(b => _databasePreloader.Skills[(b.SkillId, b.SkillLevel)].TypeDetail == TypeDetail.Stealth);
                    break;

                case TypeDetail.WeaponMastery:
                    if (skill.Weapon1 != 0)
                    {
                        _speedManager.WeaponSpeedPassiveSkillModificator.Remove(skill.Weapon1);
                        _speedManager.RaisePassiveModificatorChanged(skill.Weapon1, skill.Weaponvalue, false);
                    }

                    if (skill.Weapon2 != 0)
                    {
                        _speedManager.WeaponSpeedPassiveSkillModificator.Remove(skill.Weapon2);
                        _speedManager.RaisePassiveModificatorChanged(skill.Weapon2, skill.Weaponvalue, false);
                    }
                    break;

                case TypeDetail.RemoveAttribute:
                    _elementProvider.IsRemoveElement = false;
                    break;

                case TypeDetail.ElementalAttack:
                    _elementProvider.AttackSkillElement = Element.None;
                    break;

                case TypeDetail.ElementalProtection:
                    _elementProvider.DefenceSkillElement = Element.None;
                    break;

                case TypeDetail.Untouchable:
                    _untouchableManager.IsUntouchable = ActiveBuffs.Any(b => b.IsUntouchable);
                    break;

                default:
                    _logger.LogError("Not implemented buff skill type {skillType}.", skill.TypeDetail);
                    break;
            }
        }

        private void ApplyAbility(AbilityType abilityType, ushort abilityValue, bool addAbility)
        {
            switch (abilityType)
            {
                case AbilityType.None:
                    return;

                case AbilityType.PhysicalAttackRate:
                case AbilityType.ShootingAttackRate:
                    if (addAbility)
                        _statsManager.ExtraPhysicalHittingChance += abilityValue;
                    else
                        _statsManager.ExtraPhysicalHittingChance -= abilityValue;
                    return;

                case AbilityType.PhysicalEvationRate:
                case AbilityType.ShootingEvationRate:
                    if (addAbility)
                        _statsManager.ExtraPhysicalEvasionChance += abilityValue;
                    else
                        _statsManager.ExtraPhysicalEvasionChance -= abilityValue;
                    return;

                case AbilityType.MagicAttackRate:
                    if (addAbility)
                        _statsManager.ExtraMagicHittingChance += abilityValue;
                    else
                        _statsManager.ExtraMagicHittingChance -= abilityValue;
                    return;

                case AbilityType.MagicEvationRate:
                    if (addAbility)
                        _statsManager.ExtraMagicEvasionChance += abilityValue;
                    else
                        _statsManager.ExtraMagicEvasionChance -= abilityValue;
                    return;

                case AbilityType.CriticalAttackRate:
                    if (addAbility)
                        _statsManager.ExtraCriticalHittingChance += abilityValue;
                    else
                        _statsManager.ExtraCriticalHittingChance -= abilityValue;
                    return;

                case AbilityType.PhysicalAttackPower:
                case AbilityType.ShootingAttackPower:
                    if (addAbility)
                        _statsManager.ExtraPhysicalAttackPower += abilityValue;
                    else
                        _statsManager.ExtraPhysicalAttackPower -= abilityValue;
                    return;

                case AbilityType.MagicAttackPower:
                    if (addAbility)
                        _statsManager.ExtraMagicAttackPower += abilityValue;
                    else
                        _statsManager.ExtraMagicAttackPower -= abilityValue;
                    return;

                case AbilityType.Str:
                    if (addAbility)
                        _statsManager.ExtraStr += abilityValue;
                    else
                        _statsManager.ExtraStr -= abilityValue;
                    return;

                case AbilityType.Rec:
                    if (addAbility)
                        _statsManager.ExtraRec += abilityValue;
                    else
                        _statsManager.ExtraRec -= abilityValue;
                    return;

                case AbilityType.Int:
                    if (addAbility)
                        _statsManager.ExtraInt += abilityValue;
                    else
                        _statsManager.ExtraInt -= abilityValue;
                    return;

                case AbilityType.Wis:
                    if (addAbility)
                        _statsManager.ExtraWis += abilityValue;
                    else
                        _statsManager.ExtraWis -= abilityValue;
                    return;

                case AbilityType.Dex:
                    if (addAbility)
                        _statsManager.ExtraDex += abilityValue;
                    else
                        _statsManager.ExtraDex -= abilityValue;
                    return;

                case AbilityType.Luc:
                    if (addAbility)
                        _statsManager.ExtraLuc += abilityValue;
                    else
                        _statsManager.ExtraLuc -= abilityValue;
                    return;

                case AbilityType.HP:
                    if (addAbility)
                        _healthManager.ExtraHP += abilityValue;
                    else
                        _healthManager.ExtraHP -= abilityValue;
                    break;

                case AbilityType.MP:
                    if (addAbility)
                        _healthManager.ExtraMP += abilityValue;
                    else
                        _healthManager.ExtraMP -= abilityValue;
                    break;

                case AbilityType.SP:
                    if (addAbility)
                        _healthManager.ExtraSP += abilityValue;
                    else
                        _healthManager.ExtraSP -= abilityValue;
                    break;

                case AbilityType.PhysicalDefense:
                case AbilityType.ShootingDefense:
                    if (addAbility)
                        _statsManager.ExtraDefense += abilityValue;
                    else
                        _statsManager.ExtraDefense -= abilityValue;
                    return;

                case AbilityType.MagicResistance:
                    if (addAbility)
                        _statsManager.ExtraResistance += abilityValue;
                    else
                        _statsManager.ExtraResistance -= abilityValue;
                    return;

                case AbilityType.MoveSpeed:
                    if (addAbility)
                        _speedManager.ExtraMoveSpeed += abilityValue;
                    else
                        _speedManager.ExtraMoveSpeed -= abilityValue;
                    return;

                case AbilityType.AttackSpeed:
                    if (addAbility)
                        _speedManager.ExtraAttackSpeed += abilityValue;
                    else
                        _speedManager.ExtraAttackSpeed -= abilityValue;
                    return;

                case AbilityType.AbsorptionAura:
                    if (addAbility)
                        _statsManager.Absorption += abilityValue;
                    else
                        _statsManager.Absorption -= abilityValue;
                    return;

                default:
                    throw new NotImplementedException($"Not implemented ability type {abilityType}");
            }
        }

        private void Buff_OnPeriodicalHeal(Buff buff, AttackResult healResult)
        {
            _healthManager.IncreaseHP(healResult.Damage.HP);
            _healthManager.CurrentMP += healResult.Damage.MP;
            _healthManager.CurrentSP += healResult.Damage.SP;

            OnSkillKeep?.Invoke(_ownerId, buff, healResult);
        }

        private void Buff_OnPeriodicalDebuff(Buff buff, AttackResult debuffResult)
        {
            var damage = debuffResult.Damage;

            if (buff.TimeDamageType == TimeDamageType.Percent)
            {
                damage = new Damage(
                    Convert.ToUInt16(_healthManager.CurrentHP * debuffResult.Damage.HP * 1.0 / 100),
                    Convert.ToUInt16(_healthManager.CurrentSP * debuffResult.Damage.SP * 1.0 / 100),
                    Convert.ToUInt16(_healthManager.CurrentMP * debuffResult.Damage.MP * 1.0 / 100));
            }

            _healthManager.DecreaseHP(damage.HP, buff.BuffCreator);
            _healthManager.CurrentMP -= damage.MP;
            _healthManager.CurrentSP -= damage.SP;

            OnSkillKeep?.Invoke(_ownerId, buff, new AttackResult(AttackSuccess.Normal, damage));
        }

        #endregion

        #region Clear buffs on some condition

        private void HealthManager_OnDead(int senderId, IKiller killer)
        {
            var buffs = ActiveBuffs.Where(b => b.ShouldClearAfterDeath).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        private void HealthManager_HP_Changed(int senderId, HitpointArgs args)
        {
            var buffs = ActiveBuffs.Where(b => b.IsStealth).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        #endregion
    }
}
