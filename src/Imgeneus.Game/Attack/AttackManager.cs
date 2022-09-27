using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Vehicle;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Game.Attack
{
    public class AttackManager : IAttackManager
    {
        private readonly ILogger<AttackManager> _logger;
        private readonly IStatsManager _statsManager;
        private readonly ILevelProvider _levelProvider;
        private readonly IElementProvider _elementManager;
        private readonly ICountryProvider _countryProvider;
        private readonly ISpeedManager _speedManager;
        private readonly IStealthManager _stealthManager;
        private readonly IHealthManager _healthManager;
        private readonly IShapeManager _shapeManager;
        private readonly IMovementManager _movementManager;
        private readonly IVehicleManager _vehicleManager;
        private uint _ownerId;

        public AttackManager(ILogger<AttackManager> logger, IStatsManager statsManager, ILevelProvider levelProvider, IElementProvider elementManager, ICountryProvider countryProvider, ISpeedManager speedManager, IStealthManager stealthManager, IHealthManager healthManager, IShapeManager shapeManager, IMovementManager movementManager, IVehicleManager vehicleManager)
        {
            _logger = logger;
            _statsManager = statsManager;
            _levelProvider = levelProvider;
            _elementManager = elementManager;
            _countryProvider = countryProvider;
            _speedManager = speedManager;
            _stealthManager = stealthManager;
            _healthManager = healthManager;
            _shapeManager = shapeManager;
            _movementManager = movementManager;
            _vehicleManager = vehicleManager;

#if DEBUG
            _logger.LogDebug("AttackManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~AttackManager()
        {
            _logger.LogDebug("AttackManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        #region Target

        public event Action<IKillable> OnTargetChanged;

        private IKillable _target;
        public IKillable Target
        {
            get => _target;
            set
            {
                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded -= Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved -= Target_OnBuffRemoved;
                }

                _target = value;

                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded += Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved += Target_OnBuffRemoved;
                }

                OnTargetChanged?.Invoke(_target);
            }
        }

        public event Action<IKillable, Buff> TargetOnBuffAdded;

        private void Target_OnBuffAdded(uint senderId, Buff buff)
        {
            TargetOnBuffAdded?.Invoke(Target, buff);
        }

        public event Action<IKillable, Buff> TargetOnBuffRemoved;
        private void Target_OnBuffRemoved(uint senderId, Buff buff)
        {
            TargetOnBuffRemoved?.Invoke(Target, buff);
        }

        #endregion

        /// <summary>
        /// I'm not sure how exactly in original server next attack time was implemented.
        /// For now, I'm implementing it as usual date time and increase it based on attack speed and casting time.
        /// </summary>
        private DateTime _nextAttackTime;

        public int NextAttackTime
        {
            get
            {
                switch (_speedManager.TotalAttackSpeed)
                {
                    case AttackSpeed.ExteremelySlow:
                        return 3000;

                    case AttackSpeed.VerySlow:
                        return 2750;

                    case AttackSpeed.Slow:
                        return 2500;

                    case AttackSpeed.ABitSlow:
                        return 2250;

                    case AttackSpeed.Normal:
                        return 2000;

                    case AttackSpeed.ABitFast:
                        return 1750;

                    case AttackSpeed.Fast:
                        return 1500;

                    case AttackSpeed.VeryFast:
                        return 1250;

                    case AttackSpeed.ExteremelyFast:
                        return 1000;

                    default:
                        return 2000;
                }
            }
        }

        public void StartAttack()
        {
            _nextAttackTime = DateTime.UtcNow.AddMilliseconds(NextAttackTime);
            OnStartAttack?.Invoke();
        }

        public event Action OnStartAttack;

        public event Action<uint, IKillable, AttackResult> OnAttack;

        public bool IsWeaponAvailable { get; set; } = true;

        public bool IsShieldAvailable { get; set; } = true;

        public bool CanAttack(byte skillNumber, IKillable target, out AttackSuccess success)
        {
            if (_vehicleManager.IsOnVehicle)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (_shapeManager.Shape == ShapeEnum.Pig)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (!IsWeaponAvailable)
            {
                success = AttackSuccess.WrongEquipment;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && DateTime.UtcNow < _nextAttackTime)
            {
                // TODO: send not enough elapsed time?
                //_logger.Log(LogLevel.Debug, "Too fast attack.");
                success = AttackSuccess.TooFastAttack;
                return false;
            }

            if (DateTime.UtcNow < _nextAttackTime)
            {
                success = AttackSuccess.TooFastAttack;
                return false;
            }

            if (target is not null && (target.HealthManager.IsDead || !target.HealthManager.IsAttackable) && skillNumber == IAttackManager.AUTO_ATTACK_NUMBER)
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && target.CountryProvider.Country == _countryProvider.Country)
            {
                if (target is Character && ((Character)target).DuelManager.OpponentId != _ownerId)
                {
                    success = AttackSuccess.WrongTarget;
                    return false;
                }

                if (target is Mob)
                {
                    success = AttackSuccess.WrongTarget;
                    return false;
                }
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && !_speedManager.IsAbleToPhysicalAttack)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && MathExtensions.Distance(_movementManager.PosX, target.MovementManager.PosX, _movementManager.PosZ, target.MovementManager.PosZ) > WeaponAttackRange + 4 + ExtraAttackRange)
            {
                success = AttackSuccess.InsufficientRange;
                return false;
            }

            if (skillNumber != IAttackManager.AUTO_ATTACK_NUMBER && skillNumber != ISkillsManager.ITEM_SKILL_NUMBER)
            {
                success = AttackSuccess.Normal;
                return true;
            }

            success = AttackSuccess.Normal;
            return true;
        }

        public DateTime LastTimeAutoAttack { get; set; }
        public DateTime SkipAutoAttackRequestTime { get; set; }
        public bool SkipNextAutoAttack { get; set; }

        /// <summary>
        /// Usual physical attack, "auto attack".
        /// </summary>
        public void AutoAttack(IKiller sender)
        {
            StartAttack();

            LastTimeAutoAttack = DateTime.UtcNow;

            AttackResult result;

            var typeAttack = TypeAttack.PhysicalAttack;
            if (sender is Character character && character.AdditionalInfoManager.Class == CharacterProfession.Archer)
                typeAttack = TypeAttack.ShootingAttack;

            if (!AttackSuccessRate(Target, typeAttack))
            {
                result = new AttackResult(AttackSuccess.Miss, new Damage());
                OnAttack?.Invoke(_ownerId, Target, result);
                return;
            }

            result = CalculateAttackResult(Target,
                                           _elementManager.AttackElement,
                                           _statsManager.MinAttack,
                                           _statsManager.MaxAttack,
                                           _statsManager.MinMagicAttack,
                                           _statsManager.MaxMagicAttack);

            OnAttack?.Invoke(_ownerId, Target, result); // Event should go first, otherwise AI manager will clear target and it will be null.

            if (!Target.HealthManager.ReflectPhysicDamage)
                Target.HealthManager.DecreaseHP(result.Damage.HP, sender);
            else
            {
                _healthManager.InvokeMirrowDamage(result.Damage, Target);
            }

            // In AI manager, if target is killed, it's cleared via setting to null.
            // That's why after decreasing HP we must check if target is still presented, otherwise null exception is thrown.
            if (Target != null)
            {
                Target.HealthManager.CurrentSP -= result.Damage.SP;
                Target.HealthManager.CurrentMP -= result.Damage.MP;
            }
        }

        public bool AlwaysHit { get; set; }

        public bool AttackSuccessRate(IKillable target, TypeAttack typeAttack, Skill skill = null)
        {
            if (AlwaysHit)
                return true;

            if (skill is not null && target is IKiller skillsOwner && skillsOwner.SkillsManager.ResistSkills.Contains(skill.SkillId))
                return false;

            if (target.UntouchableManager.IsUntouchable)
                return false;

            if (target.UntouchableManager.BlockedMagicAttacks > 0 && typeAttack == TypeAttack.MagicAttack && !skill.IsForAlly)
            {
                target.UntouchableManager.BlockedMagicAttacks--;
                return false;
            }

            if (skill != null && (skill.StateType == StateType.FlatDamage || skill.StateType == StateType.DeathTouch || skill.Type == TypeDetail.ElementalAttack || skill.Type == TypeDetail.ElementalProtection))
                return true;

            if (skill != null && skill.UseSuccessValue)
                return new Random().Next(1, 101) < skill.SuccessValue;

            double levelDifference;
            double result;

            // Starting from here there might be not clear code.
            // This code is not my invention, it's raw implementation of ep 4 calculations.
            // You're free to change it to whatever you think fits better your server.
            switch (typeAttack)
            {
                case TypeAttack.PhysicalAttack:
                case TypeAttack.ShootingAttack:

                    if (typeAttack == TypeAttack.ShootingAttack && target.StatsManager.ConstShootingEvasionChance > 0)
                        result = 100 - target.StatsManager.ConstShootingEvasionChance;
                    else
                    {
                        levelDifference = _levelProvider.Level * 1.0 / (target.LevelProvider.Level + _levelProvider.Level);
                        var targetAttackPercent = typeAttack == TypeAttack.PhysicalAttack ?
                            target.StatsManager.PhysicalHittingChance / (target.StatsManager.PhysicalHittingChance + _statsManager.PhysicalEvasionChance)
                            :
                            target.StatsManager.ShootingHittingChance / (target.StatsManager.ShootingHittingChance + _statsManager.ShootingEvasionChance);
                        var myAttackPercent = typeAttack == TypeAttack.PhysicalAttack ?
                            _statsManager.PhysicalHittingChance / (_statsManager.PhysicalHittingChance + target.StatsManager.PhysicalEvasionChance)
                            :
                            _statsManager.ShootingHittingChance / (_statsManager.ShootingHittingChance + target.StatsManager.ShootingEvasionChance);
                        var attackPercent = targetAttackPercent * 100 - myAttackPercent * 100;
                        result = levelDifference * 160 - attackPercent;
                        if (result >= 20)
                        {
                            if (result > 99)
                                result = 99;
                        }
                        else
                        {
                            if (target is Mob)
                                result = 20;
                            else
                                result = 1;
                        }
                    }

                    return new Random().Next(1, 101) < result;

                case TypeAttack.MagicAttack:
                    levelDifference = ((target.LevelProvider.Level - _levelProvider.Level - 2) * 100 + target.LevelProvider.Level) / (target.LevelProvider.Level + _levelProvider.Level) * 1.1;
                    var fxDef = Math.Abs(levelDifference) + target.StatsManager.MagicEvasionChance;
                    if (fxDef >= 1)
                    {
                        if (fxDef > 70)
                            fxDef = 70;
                    }
                    else
                    {
                        fxDef = 1;
                    }

                    var wisDifference = Math.Abs((11 * target.StatsManager.TotalWis - 10 * _statsManager.TotalWis) / (target.StatsManager.TotalWis + _statsManager.TotalWis + 0.1) * 3.9000001);
                    var nAttackTypea = wisDifference + _statsManager.MagicHittingChance;
                    if (nAttackTypea >= 1)
                    {
                        if (nAttackTypea > 70)
                            nAttackTypea = 70;
                    }
                    else
                    {
                        nAttackTypea = 1;
                    }

                    result = nAttackTypea + fxDef;
                    if (result >= 1)
                    {
                        if (result > 90)
                            result = 90;
                    }
                    else
                    {
                        result = 1;
                    }
                    return new Random().Next(1, 101) < result;
            }
            return true;
        }

        public AttackResult CalculateAttackResult(IKillable target, Element element, int minAttack, int maxAttack, int minMagicAttack, int maxMagicAttack, Skill skill = null)
        {
            AttackResult result;
            if (skill is not null)
            {
                switch (skill.DamageType)
                {
                    case DamageType.FixedDamage:
                        result = new AttackResult(AttackSuccess.Normal, new Damage(skill.DamageHP, skill.DamageMP, skill.DamageSP));
                        break;

                    case DamageType.PlusExtraDamage:
                        result = CalculateDamage(target,
                                               skill.TypeAttack,
                                               element,
                                               minAttack,
                                               maxAttack,
                                               minMagicAttack,
                                               maxMagicAttack,
                                               skill);
                        break;

                    case DamageType.Eraser:
                        result = new AttackResult(AttackSuccess.Normal, new Damage((ushort)(_healthManager.CurrentHP * 2), 0, 0));
                        break;

                    case DamageType.HPPercentageDamage:
                        result = new AttackResult(AttackSuccess.Normal, new Damage((ushort)(target.HealthManager.MaxHP * skill.DamageHP / 100), 0, 0));
                        break;

                    case DamageType.MPPercentageDamage:
                        ushort mpDamage = (ushort)(target.HealthManager.MaxMP * (skill.DamageHP + skill.DamageMP) / 100);
                        ushort spDamage = (ushort)(target.HealthManager.MaxMP * (skill.DamageHP + skill.DamageSP) / 100);
                        result = new AttackResult(AttackSuccess.Normal, new Damage(0, mpDamage, spDamage));
                        break;

                    case DamageType.DamageCoefficient:
                        result = CalculateDamage(target,
                                               skill.TypeAttack,
                                               element,
                                               minAttack,
                                               maxAttack,
                                               minMagicAttack,
                                               maxMagicAttack,
                                               skill);
                        result.Damage.HP *= skill.DamageHP;
                        break;

                    case DamageType.RecDamagePlusExtra:
                        result = CalculateDamage(target,
                                               skill.TypeAttack,
                                               element,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               skill);
                        break;

                    case DamageType.RecDamageCoefficient:
                        result = CalculateDamage(target,
                                               skill.TypeAttack,
                                               element,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               _statsManager.TotalRec,
                                               skill);
                        result.Damage.HP *= skill.DamageHP;
                        break;

                    default:
                        throw new NotImplementedException("Not implemented damage type.");
                }
            }
            else
                result = CalculateDamage(target,
                                         TypeAttack.PhysicalAttack,
                                         element,
                                         minAttack,
                                         maxAttack,
                                         minMagicAttack,
                                         maxMagicAttack);


            if (target.HealthManager.UseSPInsteadOfHP)
            {
                if (target.HealthManager.CurrentSP > result.Damage.HP)
                {
                    result.Damage.SP = result.Damage.HP;
                    result.Damage.HP = 0;
                }
                else
                {
                    result.Damage.HP = (ushort)(result.Damage.HP - target.HealthManager.CurrentSP);
                    result.Damage.SP = (ushort)target.HealthManager.CurrentSP;
                }
            }

            if (target.HealthManager.UseMPInsteadOfHP)
            {
                if (target.HealthManager.CurrentMP > result.Damage.HP)
                {
                    result.Damage.MP = result.Damage.HP;
                    result.Damage.HP = 0;
                }
                else
                {
                    result.Damage.HP = (ushort)(result.Damage.HP - target.HealthManager.CurrentMP);
                    result.Damage.MP = (ushort)target.HealthManager.CurrentMP;
                }
            }

            return result;
        }

        private AttackResult CalculateDamage(
            IKillable target,
            TypeAttack typeAttack,
            Element attackElement,
            int minAttack,
            int maxAttack,
            int minMagicAttack,
            int maxMagicAttack,
            Skill skill = null)
        {
            double damage = 0;

            // First, calculate damage, that is made of stats, weapon and buffs.
            switch (typeAttack)
            {
                case TypeAttack.PhysicalAttack:
                    damage = new Random().Next(minAttack, maxAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                        damage += skill.AddDamageHP;
                    }
                    damage -= target.StatsManager.TotalDefense;
                    if (damage < 0)
                        damage = 1;
                    damage = damage * 1.5;
                    break;

                case TypeAttack.ShootingAttack:
                    damage = new Random().Next(minAttack, maxAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                        damage += skill.AddDamageHP;
                    }
                    damage -= target.StatsManager.TotalDefense;
                    if (damage < 0)
                        damage = 1;
                    // TODO: multiply by range to the target.
                    damage = damage * 1.5; // * 0.7 if target is too close.
                    break;

                case TypeAttack.MagicAttack:
                    damage = new Random().Next(minMagicAttack, maxMagicAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                        damage += skill.AddDamageHP;
                    }
                    damage -= target.StatsManager.TotalResistance;
                    if (damage < 0)
                        damage = 1;
                    damage = damage * 1.5;
                    break;
            }

            // Second, add element calculation.
            Element element = skill != null && skill.Element != Element.None ? skill.Element : attackElement;
            var elementFactor = GetElementFactor(element, target.ElementProvider.DefenceElement);
            damage = damage * elementFactor;

            // Third, monster shape adds additional damage.
            if (_shapeManager.Shape == ShapeEnum.Wolf)
                damage = damage * 1.2; // + 20%
            if (_shapeManager.Shape == ShapeEnum.Knight)
                damage = damage * 1.4; // + 40%

            // Fourth, calculate if critical damage should be added.
            var criticalDamage = false;
            if (new Random().Next(1, 101) < CriticalSuccessRate(target))
            {
                criticalDamage = true;
                damage += Convert.ToInt32(_statsManager.TotalLuc * new Random().NextDouble() * 1.5);
            }

            if (damage > 30000)
                damage = 30000;

            // Forth, subtract absorption value;
            ushort absorb = 0;
            if (target.Absorption < damage)
            {
                damage -= target.Absorption;
                absorb = target.Absorption;
            }
            else
            {
                absorb = Convert.ToUInt16(damage);
                damage = 0;
            }

            if (criticalDamage)
                return new AttackResult(AttackSuccess.Critical, new Damage(Convert.ToUInt16(damage), skill is null ? (ushort)0 : (ushort)(skill.DamageSP + skill.AddDamageSP), skill is null ? (ushort)0 : (ushort)(skill.DamageMP + skill.AddDamageMP)), absorb);
            else
                return new AttackResult(AttackSuccess.Normal, new Damage(Convert.ToUInt16(damage), skill is null ? (ushort)0 : (ushort)(skill.DamageSP + skill.AddDamageSP), skill is null ? (ushort)0 : (ushort)(skill.DamageMP + skill.AddDamageMP)), absorb);
        }

        /// <summary>
        /// Calculates critical rate or possibility to make critical hit.
        /// Can be only more then 5 and less than 99.
        /// </summary>
        private int CriticalSuccessRate(IKillable target)
        {
            var result = Convert.ToInt32(_statsManager.CriticalHittingChance - (target.StatsManager.TotalLuc * 0.034000002));

            if (result < 5)
                result = 5;

            if (result > 99)
                result = 99;

            return result;
        }

        public double GetElementFactor(Element attackElement, Element defenceElement)
        {
            if (attackElement == defenceElement)
                return 1;

            if (attackElement != Element.None && defenceElement == Element.None)
            {
                if (attackElement == Element.Fire1 || attackElement == Element.Earth1 || attackElement == Element.Water1 || attackElement == Element.Wind1)
                    return 1.2;
                if (attackElement == Element.Fire2 || attackElement == Element.Earth2 || attackElement == Element.Water2 || attackElement == Element.Wind2)
                    return 1.3;
            }

            if (attackElement == Element.None && defenceElement != Element.None)
            {
                if (defenceElement == Element.Fire1 || defenceElement == Element.Earth1 || defenceElement == Element.Water1 || defenceElement == Element.Wind1)
                    return 0.8;
                if (defenceElement == Element.Fire2 || defenceElement == Element.Earth2 || defenceElement == Element.Water2 || defenceElement == Element.Wind2)
                    return 0.7;
            }

            if (attackElement == Element.Water1)
            {
                if (defenceElement == Element.Fire1)
                    return 1.4;
                if (defenceElement == Element.Fire2)
                    return 1.3;

                if (defenceElement == Element.Earth1)
                    return 0.5;
                if (defenceElement == Element.Earth2)
                    return 0.4;

                return 1; // wind or water
            }

            if (attackElement == Element.Fire1)
            {
                if (defenceElement == Element.Wind1)
                    return 1.4;
                if (defenceElement == Element.Wind2)
                    return 1.3;

                if (defenceElement == Element.Water1)
                    return 0.5;
                if (defenceElement == Element.Water2)
                    return 0.4;

                return 1; // earth or fire
            }

            if (attackElement == Element.Wind1)
            {
                if (defenceElement == Element.Earth1)
                    return 1.4;
                if (defenceElement == Element.Earth2)
                    return 1.3;

                if (defenceElement == Element.Fire1)
                    return 0.5;
                if (defenceElement == Element.Fire2)
                    return 0.4;

                return 1; // wind or water
            }

            if (attackElement == Element.Earth1)
            {
                if (defenceElement == Element.Water1)
                    return 1.4;
                if (defenceElement == Element.Water2)
                    return 1.3;

                if (defenceElement == Element.Wind1)
                    return 0.5;
                if (defenceElement == Element.Wind2)
                    return 0.4;

                return 1; // earth or fire
            }

            if (attackElement == Element.Water2)
            {
                if (defenceElement == Element.Fire1)
                    return 1.6;
                if (defenceElement == Element.Fire2)
                    return 1.4;

                if (defenceElement == Element.Earth1)
                    return 0.5;
                if (defenceElement == Element.Earth2)
                    return 0.5;

                return 1; // wind or water
            }

            if (attackElement == Element.Fire2)
            {
                if (defenceElement == Element.Wind1)
                    return 1.6;
                if (defenceElement == Element.Wind2)
                    return 1.4;

                if (defenceElement == Element.Water1)
                    return 0.5;
                if (defenceElement == Element.Water2)
                    return 0.5;

                return 1; // earth or fire
            }

            if (attackElement == Element.Wind2)
            {
                if (defenceElement == Element.Earth1)
                    return 1.6;
                if (defenceElement == Element.Earth2)
                    return 1.4;

                if (defenceElement == Element.Fire1)
                    return 0.5;
                if (defenceElement == Element.Fire2)
                    return 0.5;

                return 1; // wind or water
            }

            if (attackElement == Element.Earth2)
            {
                if (defenceElement == Element.Water1)
                    return 1.6;
                if (defenceElement == Element.Water2)
                    return 1.4;

                if (defenceElement == Element.Wind1)
                    return 0.5;
                if (defenceElement == Element.Wind2)
                    return 0.5;

                return 1; // earth or fire
            }

            return 1;
        }

        public ushort WeaponAttackRange { get; set; }
        public ushort ExtraAttackRange { get; set; }
    }
}
