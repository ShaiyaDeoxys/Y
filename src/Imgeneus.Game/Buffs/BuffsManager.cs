using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Recover;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Parsec.Shaiya.Skill;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Game.Buffs
{
    public class BuffsManager : IBuffsManager
    {
        private readonly ILogger<BuffsManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameDefinitionsPreloder _definitionsPreloder;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IElementProvider _elementProvider;
        private readonly IUntouchableManager _untouchableManager;
        private readonly IStealthManager _stealthManager;
        private readonly ILevelingManager _levelingManager;
        private readonly IAttackManager _attackManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IWarehouseManager _warehouseManager;
        private readonly IShapeManager _shapeManager;
        private readonly ICastProtectionManager _castProtectionManager;
        private readonly IMovementManager _movementManager;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IRecoverManager _recoverManager;
        private uint _ownerId;

        public BuffsManager(ILogger<BuffsManager> logger, IDatabase database, IGameDefinitionsPreloder definitionsPreloder, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager, IElementProvider elementProvider, IUntouchableManager untouchableManager, IStealthManager stealthManager, ILevelingManager levelingManager, IAttackManager attackManager, ITeleportationManager teleportationManager, IWarehouseManager warehouseManager, IShapeManager shapeManager, ICastProtectionManager castProtectionManager, IMovementManager movementManager, IAdditionalInfoManager additionalInfoManager, IMapProvider mapProvider, IGameWorld gameWorld, IRecoverManager recoverManager)
        {
            _logger = logger;
            _database = database;
            _definitionsPreloder = definitionsPreloder;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _speedManager = speedManager;
            _elementProvider = elementProvider;
            _untouchableManager = untouchableManager;
            _stealthManager = stealthManager;
            _levelingManager = levelingManager;
            _attackManager = attackManager;
            _teleportationManager = teleportationManager;
            _warehouseManager = warehouseManager;
            _shapeManager = shapeManager;
            _castProtectionManager = castProtectionManager;
            _movementManager = movementManager;
            _additionalInfoManager = additionalInfoManager;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _recoverManager = recoverManager;
            _healthManager.OnDead += HealthManager_OnDead;
            _healthManager.OnGotDamage += HealthManager_OnGotDamage;
            _attackManager.OnStartAttack += AttackManager_OnStartAttack;
            _untouchableManager.OnBlockedMagicAttacksChanged += UntouchableManager_OnBlockedMagicAttacksChanged;
            _movementManager.OnMove += MovementManager_OnMove;

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

        public void Init(uint ownerId, IEnumerable<DbCharacterActiveBuff> initBuffs = null)
        {
            _ownerId = ownerId;

            if (initBuffs != null)
            {
                var buffs = new List<Buff>();
                foreach (var b in initBuffs)
                {
                    var buff = Buff.FromDbCharacterActiveBuff(b, _definitionsPreloder.Skills[(b.SkillId, b.SkillLevel)]);
                    buff.Id = GenerateId();
                    buffs.Add(buff);
                }

                ActiveBuffs.AddRange(buffs);
            }
        }

        public async Task Clear()
        {
            var oldBuffs = _database.ActiveBuffs.Where(b => b.CharacterId == _ownerId);
            _database.ActiveBuffs.RemoveRange(oldBuffs);

            foreach (var b in ActiveBuffs)
            {
                if (b.CanBeActivatedAndDisactivated)
                    continue;

                var dbBuff = new DbCharacterActiveBuff()
                {
                    CharacterId = _ownerId,
                    SkillId = b.Skill.SkillId,
                    SkillLevel = b.Skill.SkillLevel,
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
            _healthManager.OnGotDamage -= HealthManager_OnGotDamage;
            _attackManager.OnStartAttack -= AttackManager_OnStartAttack;
            _untouchableManager.OnBlockedMagicAttacksChanged -= UntouchableManager_OnBlockedMagicAttacksChanged;
            _movementManager.OnMove -= MovementManager_OnMove;
        }

        #endregion

        #region Active buffs

        private uint _counter;
        private object _syncObject = new();

        private uint GenerateId()
        {
            uint id = 0;
            lock (_syncObject)
            {
                _counter++;
                id = _counter;
            }
            return id;
        }

        public ObservableRangeCollection<Buff> ActiveBuffs { get; private set; } = new ObservableRangeCollection<Buff>();

        public event Action<uint, Buff> OnBuffAdded;

        public event Action<uint, Buff> OnBuffRemoved;

        public event Action<uint, Buff, AttackResult> OnSkillKeep;

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
            if (skill.TypeEffect == TypeEffect.Debuff && DebuffResistances.Contains(skill.StateType))
                return null;

            DateTime resetTime;
            if (skill.CanBeActivated || (skill.KeepTime == 0 && skill.IsPassive))
            {
                resetTime = DateTime.UtcNow.AddDays(100);
            }
            else
            {
                if (skill.KeepTime == 0 && skill.Type == TypeDetail.DungeonMapScroll)
                    resetTime = DateTime.UtcNow.AddHours(1);
                else
                    if (skill.KeepTime == 0 && !skill.IsPassive)
                    {
                        _logger.LogWarning("Buff {skill.SkillId} {skill.SkillLevel} has 0 keep time. Please, check.", skill.SkillId, skill.SkillLevel);
                        return null;
                    }

                switch (skill.Duration)
                {
                    case Duration.DurationInMinutes:
                        resetTime = DateTime.UtcNow.AddMinutes(skill.KeepTime);
                        break;

                    case Duration.DurationInHours:
                        resetTime = DateTime.UtcNow.AddHours(skill.KeepTime);
                        break;

                    case Duration.ClearAfterDeath:
                    default:
                        resetTime = DateTime.UtcNow.AddSeconds(skill.KeepTime);
                        break;
                }
            }
            Buff buff;

            if (skill.IsPassive)
            {
                buff = PassiveBuffs.FirstOrDefault(b => b.Skill.SkillId == skill.SkillId);
            }
            else
            {
                buff = ActiveBuffs.FirstOrDefault(b => b.Skill.SkillId == skill.SkillId);
            }

            if (buff != null) // We already have such buff. Try to update reset time.
            {
                if (buff.IsDebuff && _untouchableManager.BlockDebuffs)
                    return null;

                if (buff.Skill.SkillLevel > skill.SkillLevel)
                {
                    // Do nothing, if target already has higher lvl buff.
                    return buff;
                }
                else
                {
                    // If buffs are the same level, we should only update reset time.
                    if (buff.Skill.SkillLevel == skill.SkillLevel)
                    {
                        buff.ResetTime = resetTime;
                        buff.InitialDamage = skill.InitialDamage;

                        // Send update of buff.
                        if (!buff.Skill.IsPassive)
                            OnBuffAdded?.Invoke(_ownerId, buff);
                    }

                    if (buff.Skill.SkillLevel < skill.SkillLevel)
                    {
                        // Remove old buff.
                        if (buff.Skill.IsPassive)
                            buff.CancelBuff();
                        else
                            buff.CancelBuff();

                        // Create new one with a higher level.
                        buff = new Buff(creator, skill)
                        {
                            ResetTime = resetTime,
                            InitialDamage = skill.InitialDamage,
                            Id = GenerateId()
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
                    ResetTime = resetTime,
                    InitialDamage = skill.InitialDamage,
                    Id = GenerateId()
                };

                if (buff.IsDebuff && _untouchableManager.BlockDebuffs)
                    return null;

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
            var skill = _definitionsPreloder.Skills[(buff.Skill.SkillId, buff.Skill.SkillLevel)];
            switch (skill.TypeDetail)
            {
                case TypeDetail.Buff:
                case TypeDetail.PassiveDefence:
                    if (skill.AbilityType1 == AbilityType.SacrificeStr)
                        _statsManager.SacrificedStrPercent += skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeDex)
                        _statsManager.SacrificedDexPercent += skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeRec)
                        _statsManager.SacrificedRecPercent += skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeInt)
                        _statsManager.SacrificedIntPercent += skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeWis)
                        _statsManager.SacrificedWisPercent += skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeLuc)
                        _statsManager.SacrificedLucPercent += skill.AbilityValue2;

                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();

                    if (skill.TimeHealHP != 0 || skill.TimeHealMP != 0 || skill.TimeHealSP != 0)
                    {
                        buff.TimeHealHP = skill.TimeHealHP;
                        buff.TimeHealMP = skill.TimeHealMP;
                        buff.TimeHealSP = skill.TimeHealSP;
                        buff.OnPeriodicalHeal += Buff_OnPeriodicalHeal;
                        buff.StartPeriodicalHeal();
                    }
                    break;

                case TypeDetail.SubtractingDebuff:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.Transformation:
                    CancelSprinter();

                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    _shapeManager.IsTranformated = true;
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
                    buff.TimeDamageType = skill.TimeDamageType > 0 ?
                                              skill.TimeDamageType == TimeDamageType.DoublePrevious ? TimeDamageType.DoublePrevious : TimeDamageType.Percentage
                                              :
                                              TimeDamageType.None;
                    buff.OnPeriodicalDebuff += Buff_OnPeriodicalDebuff;
                    buff.StartPeriodicalDebuff();
                    break;

                case TypeDetail.Immobilize:
                    _speedManager.Immobilize = true;
                    break;

                case TypeDetail.Sleep:
                case TypeDetail.Stun:
                    _speedManager.Immobilize = true;
                    _speedManager.IsAbleToPhysicalAttack = false;
                    _speedManager.IsAbleToMagicAttack = false;
                    break;

                case TypeDetail.PreventAttack:
                    if (skill.StateType == StateType.Silence)
                        _speedManager.IsAbleToPhysicalAttack = false;
                    if (skill.StateType == StateType.Darkness)
                        _speedManager.IsAbleToMagicAttack = false;
                    break;

                case TypeDetail.Stealth:
                    _stealthManager.IsStealth = true;
                    CancelSprinter();
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

                case TypeDetail.WeaponPowerUp:
                    if (skill.Weapon1 != 0)
                    {
                        _statsManager.WeaponAttackPassiveSkillModificator.Add(skill.Weapon1, skill.Weaponvalue);
                        _statsManager.RaiseAdditionalStatsUpdate();
                    }
                    if (skill.Weapon2 != 0)
                    {
                        _statsManager.WeaponAttackPassiveSkillModificator.Add(skill.Weapon2, skill.Weaponvalue);
                        _statsManager.RaiseAdditionalStatsUpdate();
                    }
                    break;

                case TypeDetail.ShieldMastery:
                    _statsManager.ShieldDefencePassiveSkillModificator = skill.Weaponvalue;
                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.RemoveAttribute:
                    _elementProvider.IsRemoveElement = true;
                    break;

                case TypeDetail.ElementalAttack:
                    var elementSkin = ActiveBuffs.FirstOrDefault(b => b.IsElementalWeapon && b != buff);
                    if (elementSkin != null)
                        elementSkin.CancelBuff();

                    _elementProvider.AttackSkillElement = (Element)skill.Element;
                    break;

                case TypeDetail.ElementalProtection:
                    var elementWeapon = ActiveBuffs.FirstOrDefault(b => b.IsElementalProtection && b != buff);
                    if (elementWeapon != null)
                        elementWeapon.CancelBuff();

                    _elementProvider.DefenceSkillElement = (Element)skill.Element;
                    break;

                case TypeDetail.Untouchable:
                    _untouchableManager.IsUntouchable = true;
                    break;

                case TypeDetail.BlockShootingAttack:
                    _statsManager.ConstShootingEvasionChance += skill.DefenceValue;
                    break;

                case TypeDetail.BlockMagicAttack:
                    _untouchableManager.BlockedMagicAttacks = skill.DefenceValue;
                    break;

                case TypeDetail.EtainShield:
                    _untouchableManager.BlockDebuffs = true;
                    break;

                case TypeDetail.DamageReflection:
                    _healthManager.ReflectPhysicDamage = skill.AbilityValue1 == 1;
                    _healthManager.ReflectMagicDamage = skill.AbilityValue3 == 1;
                    break;

                case TypeDetail.PersistBarrier:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                    _castProtectionManager.ProtectAlliesCasting = true;
                    _castProtectionManager.ProtectCastingRange = skill.ApplyRange;
                    _castProtectionManager.ProtectCastingSkill = (skill.SkillId, skill.SkillLevel);
                    break;

                case TypeDetail.FireThorn:
                    buff.PeriodicalHP = skill.DamageHP;
                    buff.PeriodicalSP = skill.DamageSP;
                    buff.PeriodicalMP = skill.DamageMP;
                    buff.PeriodicalDamageRange = skill.ApplyRange;
                    buff.OnPeriodicalDamage += Buff_OnPeriodicalDamage;
                    buff.StartPeriodicalDamage();
                    break;

                case TypeDetail.Evolution:
                    CancelSprinter();

                    if (skill.UsedByPriest == 1)
                    {
                        if (skill.TypeEffect == TypeEffect.Buff)
                            _shapeManager.MonsterLevel = (ShapeEnum)(9 + skill.SkillLevel);
                        else
                            _shapeManager.MonsterLevel = ShapeEnum.Pig;
                    }
                    else if (buff.Skill.IsUsedByRanger)
                    {
                        if (buff.Skill.SkillId == 107 || buff.Skill.SkillId == 680)
                        {
                            _shapeManager.CharacterId = buff.Skill.CharacterId == _ownerId ? 0 : buff.Skill.CharacterId;
                            _shapeManager.IsOppositeCountry = true;
                            break;
                        }

                        if (buff.Skill.MobShape == 0)
                            _shapeManager.MonsterLevel = (ShapeEnum)(3 + skill.SkillLevel);
                        else
                        {
                            _shapeManager.MobId = buff.Skill.MobShape;
                            _shapeManager.MonsterLevel = ShapeEnum.Mob;
                            _additionalInfoManager.FakeName = string.Empty;
                            _additionalInfoManager.FakeGuildName = string.Empty;
                            _additionalInfoManager.RaiseNameChange();
                        }
                    }
                    break;

                case TypeDetail.PotentialDefence:
                    if (PassiveBuffs.Contains(buff))
                        PontentialDefenceHpPercent = skill.LimitHP;

                    if (ActiveBuffs.Contains(buff))
                    {
                        ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                        ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true, buff, skill);
                        ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true, buff, skill);
                        ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true, buff, skill);
                        ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true, buff, skill);
                        ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true, buff, skill);
                        ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true, buff, skill);
                        ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true, buff, skill);
                        ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true, buff, skill);
                        ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true, buff, skill);
                    }
                    break;

                case TypeDetail.HealthAssistant:
                    if (skill.SkillLevel > 1)
                    {
                        _healthManager.UseSPInsteadOfHP = true;
                        _healthManager.UseMPInsteadOfHP = true;
                    }
                    else
                        _healthManager.UseSPInsteadOfHP = true;
                    break;

                case TypeDetail.DeathTouch:
                    buff.DeathThouchTime = skill.KeepTime;
                    buff.OnDeathTouch += Buff_OnDeathTouch;
                    buff.StartDeathThouchTimer();
                    break;

                case TypeDetail.AbilityExchange:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.Provoke:
                    break;

                case TypeDetail.DungeonMapScroll:
                    break;

                case TypeDetail.EnergyBlackhole:
                    break;

                case TypeDetail.Interpretation:
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
            var skill = _definitionsPreloder.Skills[(buff.Skill.SkillId, buff.Skill.SkillLevel)];
            switch (skill.TypeDetail)
            {
                case TypeDetail.Buff:
                case TypeDetail.PassiveDefence:
                    if (skill.AbilityType1 == AbilityType.SacrificeStr)
                        _statsManager.SacrificedStrPercent -= skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeDex)
                        _statsManager.SacrificedDexPercent -= skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeRec)
                        _statsManager.SacrificedRecPercent -= skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeInt)
                        _statsManager.SacrificedIntPercent -= skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeWis)
                        _statsManager.SacrificedWisPercent -= skill.AbilityValue2;
                    if (skill.AbilityType1 == AbilityType.SacrificeLuc)
                        _statsManager.SacrificedLucPercent -= skill.AbilityValue2;

                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();

                    if (skill.TimeHealHP != 0 || skill.TimeHealMP != 0 || skill.TimeHealSP != 0)
                    {
                        buff.OnPeriodicalHeal -= Buff_OnPeriodicalHeal;
                    }
                    break;

                case TypeDetail.SubtractingDebuff:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, true, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, true, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, true, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, true, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, true, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, true, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, true, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, true, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, true, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.Transformation:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false, buff, skill);
                    ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false, buff, skill);
                    ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false, buff, skill);
                    ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false, buff, skill);
                    ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false, buff, skill);
                    ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false, buff, skill);
                    ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false, buff, skill);
                    ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false, buff, skill);
                    ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    _shapeManager.IsTranformated = false;
                    break;

                case TypeDetail.PeriodicalHeal:
                    buff.OnPeriodicalHeal -= Buff_OnPeriodicalHeal;
                    break;

                case TypeDetail.PeriodicalDebuff:
                    buff.OnPeriodicalDebuff -= Buff_OnPeriodicalDebuff;
                    break;

                case TypeDetail.Immobilize:
                    _speedManager.Immobilize = ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Immobilize);
                    break;

                case TypeDetail.Sleep:
                case TypeDetail.Stun:
                    _speedManager.IsAbleToPhysicalAttack = !ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Sleep || b.Skill.Type == TypeDetail.Stun || b.Skill.StateType == StateType.Silence);
                    _speedManager.IsAbleToMagicAttack = !ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Sleep || b.Skill.Type == TypeDetail.Stun || b.Skill.StateType == StateType.Darkness);
                    _speedManager.Immobilize = ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Immobilize);
                    break;

                case TypeDetail.PreventAttack:
                    _speedManager.IsAbleToPhysicalAttack = !ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Sleep || b.Skill.Type == TypeDetail.Stun || b.Skill.StateType == StateType.Silence);
                    _speedManager.IsAbleToMagicAttack = !ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Sleep || b.Skill.Type == TypeDetail.Stun || b.Skill.StateType == StateType.Darkness);
                    break;

                case TypeDetail.Stealth:
                    _stealthManager.IsStealth = ActiveBuffs.Any(b => b.Skill.Type == TypeDetail.Stealth);
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

                case TypeDetail.WeaponPowerUp:
                    if (skill.Weapon1 != 0)
                    {
                        _statsManager.WeaponAttackPassiveSkillModificator.Remove(skill.Weapon1);
                        _statsManager.RaiseAdditionalStatsUpdate();
                    }
                    if (skill.Weapon2 != 0)
                    {
                        _statsManager.WeaponAttackPassiveSkillModificator.Remove(skill.Weapon2);
                        _statsManager.RaiseAdditionalStatsUpdate();
                    }
                    break;

                case TypeDetail.ShieldMastery:
                    _statsManager.ShieldDefencePassiveSkillModificator = 0;
                    _statsManager.RaiseAdditionalStatsUpdate();
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

                case TypeDetail.BlockShootingAttack:
                    _statsManager.ConstShootingEvasionChance -= skill.DefenceValue;
                    break;

                case TypeDetail.BlockMagicAttack:
                    _untouchableManager.BlockedMagicAttacks = 0;
                    break;

                case TypeDetail.EtainShield:
                    _untouchableManager.BlockDebuffs = false;
                    break;

                case TypeDetail.DamageReflection:
                    _healthManager.ReflectPhysicDamage = false;
                    _healthManager.ReflectMagicDamage = false;
                    break;

                case TypeDetail.PersistBarrier:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                    _castProtectionManager.ProtectAlliesCasting = false;
                    _castProtectionManager.ProtectCastingRange = 0;
                    _castProtectionManager.ProtectCastingSkill = (0, 0);
                    break;

                case TypeDetail.FireThorn:
                    buff.OnPeriodicalDamage -= Buff_OnPeriodicalDamage;
                    break;

                case TypeDetail.Evolution:
                    if (skill.UsedByPriest == 1)
                        _shapeManager.MonsterLevel = 0;
                    else if (buff.Skill.IsUsedByRanger)
                    {
                        if (buff.Skill.SkillId == 107 || buff.Skill.SkillId == 680)
                        {
                            _shapeManager.CharacterId = 0;
                            _shapeManager.IsOppositeCountry = false;
                            break;
                        }
                        else
                        {
                            _shapeManager.MobId = 0;
                            _shapeManager.MonsterLevel = ShapeEnum.None;
                            _additionalInfoManager.FakeName = null;
                            _additionalInfoManager.FakeGuildName = null;
                            _additionalInfoManager.RaiseNameChange();
                        }
                    }
                    break;

                case TypeDetail.PotentialDefence:
                    if (!PassiveBuffs.Any(x => x.Skill.Type == TypeDetail.PotentialDefence))
                        PontentialDefenceHpPercent = 0;

                    if (!ActiveBuffs.Contains(buff))
                    {
                        ApplyAbility(skill.AbilityType1, skill.AbilityValue1, false, buff, skill);
                        ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false, buff, skill);
                        ApplyAbility(skill.AbilityType3, skill.AbilityValue3, false, buff, skill);
                        ApplyAbility(skill.AbilityType4, skill.AbilityValue4, false, buff, skill);
                        ApplyAbility(skill.AbilityType5, skill.AbilityValue5, false, buff, skill);
                        ApplyAbility(skill.AbilityType6, skill.AbilityValue6, false, buff, skill);
                        ApplyAbility(skill.AbilityType7, skill.AbilityValue7, false, buff, skill);
                        ApplyAbility(skill.AbilityType8, skill.AbilityValue8, false, buff, skill);
                        ApplyAbility(skill.AbilityType9, skill.AbilityValue9, false, buff, skill);
                        ApplyAbility(skill.AbilityType10, skill.AbilityValue10, false, buff, skill);
                    }
                    break;

                case TypeDetail.HealthAssistant:
                    _healthManager.UseSPInsteadOfHP = false;
                    _healthManager.UseMPInsteadOfHP = false;
                    break;

                case TypeDetail.DeathTouch:
                    buff.OnDeathTouch -= Buff_OnDeathTouch;
                    break;

                case TypeDetail.AbilityExchange:
                    ApplyAbility(skill.AbilityType1, skill.AbilityValue1, true, buff, skill);
                    ApplyAbility(skill.AbilityType2, skill.AbilityValue2, false, buff, skill);

                    _statsManager.RaiseAdditionalStatsUpdate();
                    break;

                case TypeDetail.Provoke:
                    break;

                case TypeDetail.DungeonMapScroll:
                    break;

                case TypeDetail.EnergyBlackhole:
                    break;

                case TypeDetail.Interpretation:
                    break;

                default:
                    _logger.LogError("Not implemented buff skill type {skillType}.", skill.TypeDetail);
                    break;
            }
        }

        private void ApplyAbility(AbilityType abilityType, ushort abilityValue, bool addAbility, Buff buff, DbSkill skill)
        {
            switch (abilityType)
            {
                case AbilityType.None:
                    return;

                case AbilityType.PhysicalAttackRate:
                    if (addAbility)
                        _statsManager.ExtraPhysicalHittingChance += abilityValue;
                    else
                        _statsManager.ExtraPhysicalHittingChance -= abilityValue;
                    return;

                case AbilityType.ShootingAttackRate:
                    if (addAbility)
                        _statsManager.ExtraShootingHittingChance += abilityValue;
                    else
                        _statsManager.ExtraShootingHittingChance -= abilityValue;
                    return;

                case AbilityType.PhysicalEvasionRate:
                    if (addAbility)
                        _statsManager.ExtraPhysicalEvasionChance += abilityValue;
                    else
                        _statsManager.ExtraPhysicalEvasionChance -= abilityValue;
                    return;

                case AbilityType.ShootingEvasionRate:
                    if (addAbility)
                        _statsManager.ExtraShootingEvasionChance += abilityValue;
                    else
                        _statsManager.ExtraShootingEvasionChance -= abilityValue;
                    return;

                case AbilityType.MagicAttackRate:
                    if (addAbility)
                        _statsManager.ExtraMagicHittingChance += abilityValue;
                    else
                        _statsManager.ExtraMagicHittingChance -= abilityValue;
                    return;

                case AbilityType.MagicEvasionRate:
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

                case AbilityType.ExpGainRate:
                    if (addAbility)
                        _levelingManager.ExpGainRate += abilityValue;
                    else
                        _levelingManager.ExpGainRate -= abilityValue;
                    return;

                case AbilityType.BlueDragonCharm:
                    if (addAbility)
                    {
                        _teleportationManager.MaxSavedPoints = 2;
                        _levelingManager.ExpGainRate += 120;
                    }
                    else
                    {
                        _teleportationManager.MaxSavedPoints = 1;
                        _levelingManager.ExpGainRate -= 120;
                    }
                    return;

                case AbilityType.WhiteTigerCharm:
                    if (addAbility)
                    {
                        _teleportationManager.MaxSavedPoints = 4;
                        _levelingManager.ExpGainRate += 120;
                    }
                    else
                    {
                        _teleportationManager.MaxSavedPoints = 1;
                        _levelingManager.ExpGainRate -= 120;
                    }
                    return;

                case AbilityType.RedPhoenixCharm:
                    if (addAbility)
                    {
                        _teleportationManager.MaxSavedPoints = 4;
                        _levelingManager.ExpGainRate += 120;
                        _warehouseManager.IsDoubledWarehouse = true;
                    }
                    else
                    {
                        _teleportationManager.MaxSavedPoints = 1;
                        _levelingManager.ExpGainRate -= 120;
                        _warehouseManager.IsDoubledWarehouse = false;
                    }
                    return;

                case AbilityType.WarehouseSize:
                    _warehouseManager.IsDoubledWarehouse = addAbility;
                    return;

                case AbilityType.SacrificeHPPercent:
                    if (addAbility)
                    {
                        buff.TimeHPDamage = abilityValue;
                        buff.TimeDamageType = TimeDamageType.Percentage;
                        buff.OnPeriodicalDebuff += Buff_OnPeriodicalDebuff;
                        buff.RepeatTime = skill.KeepTime;
                        buff.StartPeriodicalDebuff();
                    }
                    else
                    {
                        buff.OnPeriodicalDebuff -= Buff_OnPeriodicalDebuff;
                    }
                    return;

                case AbilityType.SacrificeMPPercent:
                    if (addAbility)
                    {
                        buff.TimeMPDamage = abilityValue;
                        buff.TimeDamageType = TimeDamageType.Percentage;
                        buff.OnPeriodicalDebuff += Buff_OnPeriodicalDebuff;
                        buff.RepeatTime = skill.KeepTime;
                        buff.StartPeriodicalDebuff();
                    }
                    else
                    {
                        buff.OnPeriodicalDebuff -= Buff_OnPeriodicalDebuff;
                    }
                    return;

                case AbilityType.SacrificeStr:
                case AbilityType.SacrificeDex:
                case AbilityType.SacrificeRec:
                case AbilityType.SacrificeInt:
                case AbilityType.SacrificeWis:
                case AbilityType.SacrificeLuc:
                    break;

                case AbilityType.IncreaseStrBySacrificing:
                    _statsManager.IncreaseStrBySacrificing = addAbility;
                    break;

                case AbilityType.IncreaseDexBySacrificing:
                    _statsManager.IncreaseDexBySacrificing = addAbility;
                    break;

                case AbilityType.IncreaseRecBySacrificing:
                    _statsManager.IncreaseRecBySacrificing = addAbility;
                    break;

                case AbilityType.IncreaseIntBySacrificing:
                    _statsManager.IncreaseIntBySacrificing = addAbility;
                    break;

                case AbilityType.IncreaseWisBySacrificing:
                    _statsManager.IncreaseWisBySacrificing = addAbility;
                    break;

                case AbilityType.IncreaseLucBySacrificing:
                    _statsManager.IncreaseLucBySacrificing = addAbility;
                    break;

                case AbilityType.IncreasePhysicalDefenceByPercent:
                    _statsManager.DefencePersent = addAbility ? abilityValue : 0;
                    break;

                case AbilityType.IncreaseMagicDefenceByPercent:
                    _statsManager.ResistancePersent = addAbility ? abilityValue : 0;
                    break;

                case AbilityType.ReduceCastingTime:
                    _castProtectionManager.ReduceCastingTime = addAbility;
                    break;

                case AbilityType.AttackRange:
                    _attackManager.ExtraAttackRange = addAbility ? abilityValue : (ushort)0;
                    break;

                case AbilityType.HPRegeneration:
                    _recoverManager.ExtraHPRegeneration = addAbility ? abilityValue : (ushort)0;
                    break;

                case AbilityType.MPRegeneration:
                    _recoverManager.ExtraMPRegeneration = addAbility ? abilityValue : (ushort)0;
                    break;

                case AbilityType.SPRegeneration:
                    _recoverManager.ExtraSPRegeneration = addAbility ? abilityValue : (ushort)0;
                    break;

                default:
                    _logger.LogError($"Not implemented ability type {abilityType}");
                    break;
            }
        }

        private void Buff_OnPeriodicalHeal(Buff buff, AttackResult healResult)
        {
            var healedSomething = false;

            if (_healthManager.CurrentHP != _healthManager.MaxHP && healResult.Damage.HP != 0)
            {
                if (healResult.Damage.HP + _healthManager.CurrentHP > _healthManager.MaxHP)
                    healResult.Damage.HP = (ushort)(_healthManager.MaxHP - _healthManager.CurrentHP);

                _healthManager.IncreaseHP(healResult.Damage.HP);
                healedSomething = true;
            }
            else
                healResult.Damage.HP = 0;

            if (_healthManager.CurrentMP != _healthManager.MaxMP && healResult.Damage.MP != 0)
            {
                if (healResult.Damage.MP + _healthManager.CurrentMP > _healthManager.MaxMP)
                    healResult.Damage.MP = (ushort)(_healthManager.MaxMP - _healthManager.CurrentMP);

                _healthManager.CurrentMP += healResult.Damage.MP;
                healedSomething = true;
            }
            else
                healResult.Damage.MP = 0;

            if (_healthManager.CurrentSP != _healthManager.MaxSP && healResult.Damage.SP != 0)
            {
                if (healResult.Damage.SP + _healthManager.CurrentSP > _healthManager.MaxSP)
                    healResult.Damage.SP = (ushort)(_healthManager.MaxSP - _healthManager.CurrentSP);

                _healthManager.CurrentSP += healResult.Damage.SP;
                healedSomething = true;
            }
            else
                healResult.Damage.SP = 0;

            if (healedSomething)
                OnSkillKeep?.Invoke(_ownerId, buff, healResult);
        }

        private void Buff_OnPeriodicalDebuff(Buff buff, AttackResult debuffResult)
        {
            var damage = debuffResult.Damage;

            if (buff.TimeDamageType == TimeDamageType.Percentage)
            {
                damage = new Damage(
                    Convert.ToUInt16(_healthManager.MaxHP * debuffResult.Damage.HP * 1.0 / 100),
                    Convert.ToUInt16(_healthManager.MaxSP * debuffResult.Damage.SP * 1.0 / 100),
                    Convert.ToUInt16(_healthManager.MaxMP * debuffResult.Damage.MP * 1.0 / 100));
            }

            if (buff.TimeDamageType == TimeDamageType.DoublePrevious)
            {
                if (buff.DamageCounter == 0)
                    damage = new Damage(debuffResult.Damage.HP, 0, 0);
                else
                {
                    buff.InitialDamage = (ushort)Math.Round(buff.InitialDamage * 1.2);
                    damage = new Damage(buff.InitialDamage, 0, 0);
                }
                buff.DamageCounter++;
            }

            if (buff.CanBeActivatedAndDisactivated && (_healthManager.CurrentHP <= damage.HP || _healthManager.CurrentMP <= damage.MP || _healthManager.CurrentSP <= damage.SP))
            {
                buff.CancelBuff();
                return;
            }

            if (!buff.CanBeActivatedAndDisactivated)
                OnSkillKeep?.Invoke(_ownerId, buff, new AttackResult(AttackSuccess.Normal, damage));

            _healthManager.DecreaseHP(damage.HP, buff.BuffCreator);
            _healthManager.CurrentMP -= damage.MP;
            _healthManager.CurrentSP -= damage.SP;

            if (buff.CanBeActivatedAndDisactivated)
                _healthManager.RaiseHitpointsChange();
        }

        public event Action<uint, IKillable, Skill, AttackResult> OnPeriodicalDamage;

        private void Buff_OnPeriodicalDamage(Buff buff, AttackResult result)
        {
            if (_mapProvider.Map is null)
                return;

            var enemies = _mapProvider.Map.Cells[_mapProvider.CellId].GetEnemies(_gameWorld.Players[_ownerId], _movementManager.PosX, _movementManager.PosZ, buff.PeriodicalDamageRange);
            foreach (var enemy in enemies)
            {
                enemy.HealthManager.DecreaseHP(result.Damage.HP, _gameWorld.Players[_ownerId]);
                enemy.HealthManager.CurrentSP -= result.Damage.SP;
                enemy.HealthManager.CurrentMP -= result.Damage.MP;

                OnPeriodicalDamage?.Invoke(_ownerId, enemy, buff.Skill, result);
            }
        }

        private void Buff_OnDeathTouch(Buff buff)
        {
            _healthManager.DecreaseHP(_healthManager.CurrentHP, buff.BuffCreator);
        }

        #endregion

        #region Clear buffs on some condition

        /// <summary>
        /// When pontential defence buff will be activated?
        /// </summary>
        public ushort PontentialDefenceHpPercent { get; private set; }

        /// <summary>
        /// When pontential defence buff was last used?
        /// </summary>
        public DateTime LastPontentialDefence { get; private set; }

        private void HealthManager_OnDead(uint senderId, IKiller killer)
        {
            var buffs = ActiveBuffs.Where(b => b.Skill.ShouldClearAfterDeath).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        private void HealthManager_OnGotDamage(uint senderId, IKiller damageMaker, int damage)
        {
            var buffs = ActiveBuffs.Where(b => b.IsCanceledWhenDamage).ToList();
            foreach (var b in buffs)
                b.CancelBuff();

            if (PontentialDefenceHpPercent > 0)
                if (_healthManager.CurrentHP * 100 / _healthManager.MaxHP <= PontentialDefenceHpPercent &&
                    DateTime.UtcNow.Subtract(LastPontentialDefence).TotalMinutes > 3)
                {
                    var buff = PassiveBuffs.FirstOrDefault(x => x.Skill.Type == TypeDetail.PotentialDefence);
                    if (buff is null)
                        return;

                    var copy = new Buff(null, new Skill(_definitionsPreloder.Skills[(buff.Skill.SkillId, buff.Skill.SkillLevel)], 0, 0));
                    copy.ResetTime = DateTime.UtcNow.AddSeconds(30);
                    copy.Id = GenerateId();

                    ActiveBuffs.Add(copy);
                    LastPontentialDefence = DateTime.UtcNow;
                }
        }

        private void AttackManager_OnStartAttack()
        {
            var buffs = ActiveBuffs.Where(b => b.IsCanceledWhenAttack).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        private void UntouchableManager_OnBlockedMagicAttacksChanged(byte blockedAttacksLeft)
        {
            if (blockedAttacksLeft == 0)
            {
                var buff = ActiveBuffs.FirstOrDefault(b => b.IsBlockMagicAttack);
                if (buff != null)
                    buff.CancelBuff();
            }
        }

        private void MovementManager_OnMove(uint senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            var buffs = ActiveBuffs.Where(b => b.IsCanceledWhenMoving).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        private void CancelSprinter()
        {
            var sprinterBuff = ActiveBuffs.FirstOrDefault(b => b.Skill.SkillId == 681 || b.Skill.SkillId == 114); // 114 (old ep) 681 (new ep) are unique numbers for sprinter buff.
            if (sprinterBuff != null)
                sprinterBuff.CancelBuff();
        }

        #endregion

        #region Resistances

        public IList<StateType> DebuffResistances { get; init; } = new List<StateType>();

        #endregion
    }
}
