using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Skill;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Skills
{
    public class SkillsManager : ISkillsManager
    {
        private readonly ILogger<SkillsManager> _logger;
        private readonly IGameDefinitionsPreloder _definitionsPreloder;
        private readonly IDatabase _database;
        private readonly IHealthManager _healthManager;
        private readonly IAttackManager _attackManager;
        private readonly IBuffsManager _buffsManager;
        private readonly IStatsManager _statsManager;
        private readonly IElementProvider _elementProvider;
        private readonly ICountryProvider _countryProvider;
        private readonly ICharacterConfiguration _characterConfig;
        private readonly ILevelProvider _levelProvider;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly IMapProvider _mapProvider;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMovementManager _movementManager;
        private readonly IShapeManager _shapeManager;
        private readonly ISpeedManager _speedManager;
        private readonly IPartyManager _partyManager;
        private readonly IGamePacketFactory _packetFactory;
        private uint _ownerId;

        public SkillsManager(ILogger<SkillsManager> logger, IGameDefinitionsPreloder definitionsPreloder, IDatabase database, IHealthManager healthManager, IAttackManager attackManager, IBuffsManager buffsManager, IStatsManager statsManager, IElementProvider elementProvider, ICountryProvider countryProvider, ICharacterConfiguration characterConfig, ILevelProvider levelProvider, IAdditionalInfoManager additionalInfoManager, IMapProvider mapProvider, ITeleportationManager teleportationManager, IMovementManager movementManager, IShapeManager shapeManager, ISpeedManager speedManager, IPartyManager partyManager, IGamePacketFactory packetFactory)
        {
            _logger = logger;
            _definitionsPreloder = definitionsPreloder;
            _database = database;
            _healthManager = healthManager;
            _attackManager = attackManager;
            _buffsManager = buffsManager;
            _statsManager = statsManager;
            _elementProvider = elementProvider;
            _countryProvider = countryProvider;
            _characterConfig = characterConfig;
            _levelProvider = levelProvider;
            _additionalInfoManager = additionalInfoManager;
            _mapProvider = mapProvider;
            _teleportationManager = teleportationManager;
            _movementManager = movementManager;
            _shapeManager = shapeManager;
            _speedManager = speedManager;
            _partyManager = partyManager;
            _packetFactory = packetFactory;
            _levelProvider.OnLevelUp += OnLevelUp;

#if DEBUG
            _logger.LogDebug("SkillsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SkillsManager()
        {
            _logger.LogDebug("SkillsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, IEnumerable<Skill> skills, ushort skillPoint = 0)
        {
            _ownerId = ownerId;
            SkillPoints = skillPoint;

            foreach (var skill in skills)
                Skills.TryAdd(skill.Number, skill);

            foreach (var skill in Skills.Values.Where(s => s.IsPassive && s.Type != TypeDetail.Stealth))
                _buffsManager.AddBuff(skill, null);
        }

        public async Task Clear()
        {
            var character = await _database.Characters.Include(x => x.Skills).FirstAsync(x => x.Id == _ownerId);
            character.SkillPoint = SkillPoints;
            character.Skills.Clear();

            foreach (var skill in Skills)
            {
                // Save char and learned skill.
                var skillToAdd = new DbCharacterSkill()
                {
                    CharacterId = _ownerId,
                    SkillId = skill.Value.SkillId,
                    SkillLevel = skill.Value.SkillLevel,
                    Number = skill.Key
                };

                character.Skills.Add(skillToAdd);
            }

            await _database.SaveChangesAsync();

            Skills.Clear();
        }

        public void Dispose()
        {
            _levelProvider.OnLevelUp -= OnLevelUp;
        }

        #endregion

        #region Events

        public event Action<uint, IKillable, Skill, AttackResult> OnUsedSkill;

        public event Action<uint, IKillable, Skill, AttackResult> OnUsedRangeSkill;

        #endregion

        #region Skill points

        public ushort SkillPoints { get; private set; }

        public bool TrySetSkillPoints(ushort value)
        {
            SkillPoints = value;
            return true;
        }

        private void OnLevelUp(uint arg1, ushort arg2, ushort arg3)
        {
            var levelStats = _characterConfig.GetLevelStatSkillPoints(_additionalInfoManager.Grow);
            TrySetSkillPoints((ushort)(SkillPoints + levelStats.SkillPoint));
        }

        #endregion

        #region Skills

        public ConcurrentDictionary<byte, Skill> Skills { get; private set; } = new ConcurrentDictionary<byte, Skill>();

        public (bool Ok, Skill Skill) TryLearnNewSkill(ushort skillId, byte skillLevel)
        {
            if (Skills.Values.Any(s => s.SkillId == skillId && s.SkillLevel == skillLevel))
            {
                _logger.LogWarning("Character {characterId} has already learned skill {skillId} with level {skillLevel}", _ownerId, skillId, skillLevel);
                return (false, null);
            }

            // Find learned skill.
            var dbSkill = _definitionsPreloder.Skills[(skillId, skillLevel)];
            if (SkillPoints < dbSkill.SkillPoint)
            {
                _logger.LogWarning("Character {characterId} has not enough skill points  for skill {skillId} with level {skillLevel}", _ownerId, skillId, skillLevel);
                return (false, null);
            }

            byte skillNumber = 0;

            // Find out if the character has already learned the same skill, but lower level.
            var isSkillLearned = Skills.Values.FirstOrDefault(s => s.SkillId == skillId);
            // If there is skill of lower level => delete it.
            if (isSkillLearned != null)
            {
                var learnedSkill = _definitionsPreloder.Skills[(isSkillLearned.SkillId, isSkillLearned.SkillLevel)];
                if (learnedSkill is null)
                {
                    _logger.LogWarning("Learned skill {skillId} {skillLevel} is not found in db for character {characterId}", isSkillLearned.SkillId, isSkillLearned.SkillLevel, _ownerId);
                    skillNumber = Skills.Values.Select(s => s.Number).Max();
                    skillNumber++;
                }
                else
                {
                    skillNumber = isSkillLearned.Number;
                }
            }
            // No such skill. Generate new number.
            else
            {
                if (Skills.Any())
                {
                    // Find the next skill number.
                    skillNumber = Skills.Values.Select(s => s.Number).Max();
                    skillNumber++;
                }
                else
                {
                    // No learned skills at all.
                }
            }

            // Remove previously learned skill.
            if (isSkillLearned != null)
                Skills.TryRemove(skillNumber, out var removed);

            SkillPoints -= dbSkill.SkillPoint;

            var skill = new Skill(dbSkill, skillNumber, 0);
            Skills.TryAdd(skillNumber, skill);

            _logger.LogDebug("Character {characterId} learned skill {skillId} of level {skillLevel}", _ownerId, skillId, skillLevel);

            // Activate passive skill as soon as it's learned.
            if (skill.IsPassive)
                _buffsManager.AddBuff(skill, null);

            return (true, skill);
        }

        public event Action OnResetSkills;

        public bool ResetSkills()
        {
            var skillFactor = _characterConfig.GetLevelStatSkillPoints(_additionalInfoManager.Grow).SkillPoint;

            SkillPoints = (ushort)(skillFactor * (_levelProvider.Level - 1));

            OnResetSkills?.Invoke();

            foreach (var passive in _buffsManager.PassiveBuffs.ToList())
                passive.CancelBuff();

            Skills.Clear();

            return true;
        }

        #endregion

        #region Use skill

        public bool CanUseSkill(Skill skill, IKillable target, out AttackSuccess success)
        {
            if (_shapeManager.Shape == ShapeEnum.Fox || _shapeManager.Shape == ShapeEnum.Wolf || _shapeManager.Shape == ShapeEnum.Knight || _shapeManager.Shape == ShapeEnum.Pig)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (skill.IsUsedFromStealth && _shapeManager.Shape != ShapeEnum.Stealth)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (!skill.RequiredWeapons.Contains(_statsManager.WeaponType) && skill.RequiredWeapons.Count != 0)
            {
                success = AttackSuccess.WrongEquipment;
                return false;
            }

            if (skill.RequiredWeapons.Count == 0 && skill.NeedShield && !_attackManager.IsShieldAvailable)
            {
                success = AttackSuccess.WrongEquipment;
                return false;
            }

            if (skill.PrevSkillId > 0)
            {
                success = AttackSuccess.PreviousSkillRequired;
                return false;
            }

            if (_healthManager.CurrentMP < skill.NeedMP || _healthManager.CurrentSP < skill.NeedSP)
            {
                success = AttackSuccess.NotEnoughMPSP;
                return false;
            }

            if (!_speedManager.IsAbleToPhysicalAttack && !_speedManager.IsAbleToMagicAttack)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if ((skill.TypeAttack == TypeAttack.PhysicalAttack || skill.TypeAttack == TypeAttack.ShootingAttack) &&
                !_speedManager.IsAbleToPhysicalAttack)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (skill.TypeAttack == TypeAttack.MagicAttack &&
                !_speedManager.IsAbleToMagicAttack)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if ((skill.TargetType == TargetType.SelectedEnemy ||
                 skill.TargetType == TargetType.AnyEnemy ||
                 skill.TargetType == TargetType.EnemiesNearTarget)
                    &&
                (target is null || (target.HealthManager.IsDead && skill.Type != TypeDetail.Resurrection) || !target.HealthManager.IsAttackable))
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            if (target is not null && skill.Type == TypeDetail.Resurrection && !target.HealthManager.IsDead)
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            if (skill.AttackRange > 0 && (skill.TargetType == TargetType.SelectedEnemy || skill.TargetType == TargetType.EnemiesNearTarget) && MathExtensions.Distance(_movementManager.PosX, target.MovementManager.PosX, _movementManager.PosZ, target.MovementManager.PosZ) > skill.AttackRange + 4 + _attackManager.ExtraAttackRange)
            {
                success = AttackSuccess.InsufficientRange;
                return false;
            }

            if (Cooldowns.ContainsKey(skill.SkillId) && Cooldowns[skill.SkillId] > DateTime.UtcNow)
            {
                success = AttackSuccess.CooldownNotOver;
                return false;
            }

            if (target is null && (skill.TargetType == TargetType.Caster || skill.TargetType == TargetType.PartyMembers || skill.TargetType == TargetType.EnemiesNearCaster || skill.TargetType == TargetType.AlliesNearCaster || skill.TargetType == TargetType.AlliesButCaster))
            {
                success = AttackSuccess.Normal;
                return true;
            }

            if (target is Mob && (skill.Type == TypeDetail.Evolution && skill.SkillId == 77 || skill.SkillId == 677)) // Transform into mob.
            {
                success = AttackSuccess.Normal;
                return true;
            }

            if (target is Character && target.CountryProvider.Country != _countryProvider.Country && (skill.Type == TypeDetail.Evolution && skill.SkillId == 107 || skill.SkillId == 680)) // Disguise into opposite country.
            {
                success = AttackSuccess.Normal;
                return true;
            }

            if (target.CountryProvider.Country == _countryProvider.Country && skill.TargetType != TargetType.Caster && skill.TargetType != TargetType.EnemiesNearCaster)
            {
                if (target is Character targetCharacter)
                {
                    if (targetCharacter.DuelManager.IsStarted)
                    {
                        // Target is player himself, e.g. healing himself.
                        if (target.Id == _ownerId && skill.IsForAlly)
                        {
                            success = AttackSuccess.Normal;
                            return true;
                        }
                        // Target is opponent.
                        else
                        {
                            if (skill.IsForAlly)
                            {
                                success = AttackSuccess.WrongTarget;
                                return false;
                            }
                        }
                    }
                    // Can not attack its' own faction.
                    else
                    {
                        if (!skill.IsForAlly)
                        {
                            success = AttackSuccess.WrongTarget;
                            return false;
                        }
                    }
                }
                else // Taget is mob
                {
                    success = AttackSuccess.WrongTarget;
                    return false;
                }
            }
            else if (target.CountryProvider.Country != _countryProvider.Country && skill.IsForAlly)
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            success = AttackSuccess.Normal;
            return true;
        }

        public void UseSkill(Skill skill, IKiller skillOwner, IKillable target = null)
        {
            if (skill.SkillId == Skill.CHARGE_SKILL_ID || skill.SkillId == Skill.CHARGE_EP_8_SKILL_ID)
                ChargeUsedLastTime = DateTime.UtcNow;

            if (!skill.IsPassive && !skill.CanBeActivated)
                _attackManager.StartAttack();

            if (skill.NeedMP > 0 || skill.NeedSP > 0)
            {
                var oldMP = _healthManager.CurrentMP;
                _healthManager.CurrentMP = oldMP - (skill.NeedMP == 1 ? _healthManager.CurrentMP : skill.NeedMP);

                var oldSP = _healthManager.CurrentSP;
                _healthManager.CurrentSP = oldSP - (skill.NeedSP == 1 ? _healthManager.CurrentSP : skill.NeedSP);

                _healthManager.InvokeUsedMPSP((ushort)(oldMP - _healthManager.CurrentMP), (ushort)(oldSP - _healthManager.CurrentSP));
            }

            if (skill.ResetTime > 0)
                Cooldowns[skill.SkillId] = DateTime.UtcNow.AddSeconds(skill.ResetTime);

            int n = 0;
            do
            {
                var targets = GetTargets(skill, skillOwner, target).OrderByDescending(x => x == target).ToList();
                if (targets.Count == 0)
                    OnUsedSkill?.Invoke(_ownerId, target, skill, new AttackResult());

                foreach (var t in targets)
                {
                    // While implementing multiple attack I commented this out. Maybe it's not needed.
                    //if (t.IsDead)
                    //continue;

                    if (skill.TypeAttack != TypeAttack.Passive && !_attackManager.AttackSuccessRate(t, skill.TypeAttack, skill))
                    {
                        if (n == 0 && (target == t || target is null))
                            OnUsedSkill?.Invoke(_ownerId, t, skill, new AttackResult(AttackSuccess.Miss, new Damage(0, 0, 0)));

                        if (skill.MultiAttack > 1 || skill.TargetType == TargetType.EnemiesNearCaster || skill.TargetType == TargetType.EnemiesNearTarget || skill.TargetType == TargetType.AlliesButCaster || skill.TargetType == TargetType.AlliesNearCaster)
                            OnUsedRangeSkill?.Invoke(_ownerId, t, skill, new AttackResult(AttackSuccess.Miss, new Damage(0, 0, 0)));

                        continue;
                    }

                    var attackResult = _attackManager.CalculateAttackResult(t, _elementProvider.AttackElement, _statsManager.MinAttack, _statsManager.MaxAttack, _statsManager.MinMagicAttack, _statsManager.MaxMagicAttack, skill);

                    try
                    {
                        // Cancel those buffs, that are canceled, when any skill is used.
                        CancelBuffs(skill);

                        // Apply skill.
                        PerformSkill(skill, target, t, skillOwner, ref attackResult, n);

                        var reflectedDamage = (t.HealthManager.ReflectPhysicDamage && (skill.TypeAttack == TypeAttack.PhysicalAttack || skill.TypeAttack == TypeAttack.ShootingAttack))
                                              ||
                                              (t.HealthManager.ReflectMagicDamage && skill.TypeAttack == TypeAttack.MagicAttack);

                        // OnUsedSkill should go before OnUsedRangeSkill, because "Rapid Shot" was working incorrect.
                        if (n == 0 && (target == t || target is null))
                            OnUsedSkill?.Invoke(_ownerId, target, skill, skill.TargetType == TargetType.EnemiesNearCaster || skill.TargetType == TargetType.EnemiesNearTarget || skill.TargetType == TargetType.AlliesButCaster || skill.TargetType == TargetType.AlliesNearCaster || reflectedDamage || skill.MultiAttack > 0 ? new AttackResult() : attackResult);

                        if (skill.TargetType == TargetType.EnemiesNearCaster || skill.TargetType == TargetType.EnemiesNearTarget || skill.TargetType == TargetType.AlliesButCaster || skill.TargetType == TargetType.AlliesNearCaster)
                            OnUsedRangeSkill?.Invoke(_ownerId, t, skill, reflectedDamage ? new AttackResult() { Success = AttackSuccess.Miss } : attackResult);

                        if (skill.MultiAttack > 1 && skill.TargetType == TargetType.SelectedEnemy)
                            OnUsedRangeSkill?.Invoke(_ownerId, t, skill, reflectedDamage ? new AttackResult() { Success = AttackSuccess.Miss } : attackResult);

                        // Decrease hp should go after OnUsedRangeSkill, otherwise mob death animation won't play.
                        if (reflectedDamage)
                        {
                            _healthManager.InvokeMirrowDamage(attackResult.Damage, t);
                        }
                        else
                        {
                            if (attackResult.Damage.HP > 0 && skill.Type != TypeDetail.Healing && skill.Type != TypeDetail.FireThorn)
                            {
                                if (t.HealthManager.CurrentHP < attackResult.Damage.HP)
                                {
                                    attackResult.Damage.HP = (ushort)t.HealthManager.CurrentHP;
                                }
                                t.HealthManager.DecreaseHP(attackResult.Damage.HP, skillOwner);
                            }
                            if (attackResult.Damage.SP > 0 && skill.Type != TypeDetail.Healing && skill.Type != TypeDetail.FireThorn)
                            {
                                if (t.HealthManager.CurrentSP < attackResult.Damage.SP)
                                {
                                    attackResult.Damage.SP = (ushort)t.HealthManager.CurrentSP;
                                }
                                t.HealthManager.CurrentSP -= attackResult.Damage.SP;
                            }
                            if (attackResult.Damage.MP > 0 && skill.Type != TypeDetail.Healing && skill.Type != TypeDetail.FireThorn)
                            {
                                if (t.HealthManager.CurrentMP < attackResult.Damage.MP)
                                {
                                    attackResult.Damage.MP = (ushort)t.HealthManager.CurrentMP;
                                }
                                t.HealthManager.CurrentMP -= attackResult.Damage.MP;
                            }
                        }

                        if (skill.TypeEffect == TypeEffect.Buff || skill.TypeEffect == TypeEffect.BuffNoss || skill.TypeEffect == TypeEffect.Debuff)
                        {
                            if (!skill.CanBeActivated)
                                t.BuffsManager.AddBuff(skill, skillOwner);
                            else
                            {
                                if (skill.IsActivated)
                                    t.BuffsManager.AddBuff(skill, skillOwner);
                                else
                                {
                                    var buff = t.BuffsManager.ActiveBuffs.FirstOrDefault(x => x.Skill == skill);
                                    if (buff is not null)
                                        buff.CancelBuff();
                                }
                            }
                        }
                    }
                    catch (NotImplementedException)
                    {
                        _logger.LogError($"Not implemented skill type {skill.Type}");
                    }
                }

                n++;
            }
            while (n < skill.MultiAttack);
        }

        private IList<IKillable> GetTargets(Skill skill, IKiller skillOwner, IKillable target)
        {
            var targets = new List<IKillable>();
            switch (skill.TargetType)
            {
                case TargetType.None:
                    if (skillOwner is Character)
                        targets.Add(skillOwner as IKillable);
                    else
                        targets.Add(target);
                    break;

                case TargetType.Caster:
                    targets.Add(skillOwner as IKillable);
                    break;

                case TargetType.SelectedEnemy:
                    if (target != null)
                        targets.Add(target);
                    else
                        targets.Add(skillOwner as IKillable);
                    break;

                case TargetType.PartyMembers:
                    var t = skillOwner as Character;
                    if (t.PartyManager.Party != null)
                    {
                        var partyMembers = t.PartyManager.Party.GetShortMembersList(t);
                        var nearMembers = partyMembers.Where(m => m.Map == t.Map && MathExtensions.Distance(t.PosX, m.PosX, t.PosZ, m.PosZ) < skill.ApplyRange);
                        targets.AddRange(nearMembers);
                    }
                    else
                        targets.Add(skillOwner as IKillable);
                    break;

                case TargetType.EnemiesNearCaster:
                    if (skillOwner is Character)
                        targets.AddRange(_mapProvider.Map.Cells[_mapProvider.CellId].GetEnemies(skillOwner, skillOwner.MovementManager.PosX, skillOwner.MovementManager.PosZ, skill.ApplyRange));
                    else // Logic for mobs.
                        if (skill.Type == TypeDetail.Healing) // Mob 166. Healing skill 198.
                        targets.AddRange(_mapProvider.Map.Cells[_mapProvider.CellId].GetAllMobs(true).Where(x => MathExtensions.Distance(x.MovementManager.PosX, skillOwner.MovementManager.PosX, x.MovementManager.PosZ, skillOwner.MovementManager.PosZ) <= skill.ApplyRange));
                    else
                        targets.AddRange(_mapProvider.Map.Cells[_mapProvider.CellId].GetEnemies(skillOwner, skillOwner.MovementManager.PosX, skillOwner.MovementManager.PosZ, skill.ApplyRange));
                    break;

                case TargetType.EnemiesNearTarget:
                    targets.AddRange(_mapProvider.Map.Cells[_mapProvider.CellId].GetEnemies(skillOwner, target.MovementManager.PosX, target.MovementManager.PosZ, skill.ApplyRange));
                    break;

                case TargetType.AlliesNearCaster:
                    targets.AddRange(_mapProvider.Map.Cells[_mapProvider.CellId].GetPlayers(skillOwner.MovementManager.PosX, skillOwner.MovementManager.PosZ, skill.ApplyRange, country: skillOwner.CountryProvider.Country));
                    break;

                case TargetType.AlliesButCaster:
                    var owner = skillOwner as Character;
                    if (owner.PartyManager.HasParty)
                    {
                        var partyMembers = owner.PartyManager.Party.GetShortMembersList(owner);
                        var nearMembers = partyMembers.Where(m => m != owner && m.Map == owner.Map && MathExtensions.Distance(owner.PosX, m.PosX, owner.PosZ, m.PosZ) < skill.ApplyRange);
                        targets.AddRange(nearMembers);
                    }
                    break;

                default:
                    throw new NotImplementedException("Not implemented skill target.");
            }

            return targets;
        }

        private void PerformSkill(Skill skill, IKillable initialTarget, IKillable target, IKiller skillOwner, ref AttackResult attackResult, int n = 0)
        {
            switch (skill.Type)
            {
                case TypeDetail.Buff:
                case TypeDetail.SubtractingDebuff:
                case TypeDetail.PeriodicalHeal:
                case TypeDetail.PreventAttack:
                case TypeDetail.Immobilize:
                case TypeDetail.RemoveAttribute:
                case TypeDetail.ElementalAttack:
                case TypeDetail.ElementalProtection:
                case TypeDetail.Untouchable:
                case TypeDetail.Stealth:
                case TypeDetail.Sleep:
                case TypeDetail.Stun:
                case TypeDetail.BlockShootingAttack:
                case TypeDetail.Transformation:
                case TypeDetail.EnergyBlackhole:
                case TypeDetail.BlockMagicAttack:
                case TypeDetail.EtainShield:
                case TypeDetail.DamageReflection:
                case TypeDetail.PersistBarrier:
                case TypeDetail.MentalStormConfusion:
                case TypeDetail.HealthAssistant:
                case TypeDetail.DeathTouch:
                case TypeDetail.SoulMenace:
                case TypeDetail.MentalImpact:
                case TypeDetail.Provoke:
                case TypeDetail.DungeonMapScroll:
                case TypeDetail.AbilityExchange:
                    break;

                case TypeDetail.PeriodicalDebuff:
                    skill.InitialDamage = attackResult.Damage.HP;
                    break;

                case TypeDetail.Evolution:
                    if (skill.IsUsedByRanger && (skill.SkillId == 77 || skill.SkillId == 677))
                        if (initialTarget is Mob mob)
                            skill.MobShape = mob.MobId;
                        else
                            skill.MobShape = 0;

                    if (skill.IsUsedByRanger && (skill.SkillId == 107 || skill.SkillId == 680) && initialTarget is Character)
                        skill.CharacterId = initialTarget.Id;

                    target.BuffsManager.AddBuff(skill, skillOwner);
                    break;

                case TypeDetail.FireThorn:
                    attackResult = new AttackResult();
                    break;

                case TypeDetail.Healing:
                    attackResult = UsedHealingSkill(skill, target);
                    break;

                case TypeDetail.Dispel:
                    attackResult = UsedDispelSkill(skill, target);
                    break;

                case TypeDetail.UniqueHitAttack:
                case TypeDetail.MultipleHitsAttack:
                case TypeDetail.HP_MP_SP_Reduction:
                case TypeDetail.None:
                    break;

                case TypeDetail.PassiveDefence:
                case TypeDetail.WeaponMastery:
                    break;

                case TypeDetail.TownPortal:
                    var map = _mapProvider.Map.GetRebirthMap((Character)skillOwner);
                    _teleportationManager.Teleport(map.MapId, map.X, map.Y, map.Z);
                    break;

                case TypeDetail.Eraser:
                    _healthManager.DecreaseHP(_healthManager.MaxHP, skillOwner);
                    break;

                case TypeDetail.Resurrection:
                    target.MovementManager.PosX = _movementManager.PosX;
                    target.MovementManager.PosY = _movementManager.PosY;
                    target.MovementManager.PosZ = _movementManager.PosZ;
                    target.HealthManager.Rebirth();
                    break;

                case TypeDetail.Detection:
                    var playerInStealth = _mapProvider.Map.Cells[_mapProvider.CellId].GetPlayers(_movementManager.PosX, _movementManager.PosZ, skill.ApplyRange, _countryProvider.EnemyPlayersFraction).FirstOrDefault(x => x.StealthManager.IsStealth);
                    if (playerInStealth is not null)
                    {
                        var buff = playerInStealth.BuffsManager.ActiveBuffs.FirstOrDefault(x => x.IsStealth);
                        if (buff is not null)
                            buff.CancelBuff();
                    }
                    break;

                case TypeDetail.Scouting:
                    var members = new List<Character>();
                    if (_partyManager.HasParty)
                        members.AddRange(_partyManager.Party.GetShortMembersList((Character)skillOwner).Where(x => x.AttackManager.Target == target));

                    if (!members.Contains(skillOwner))
                        members.Add((Character)skillOwner);

                    foreach (var member in members)
                        _packetFactory.SendScoutingInfo(member.GameSession.Client, target.ElementProvider.DefenceElement, target.LevelProvider.Level, ((Character)target).AdditionalInfoManager.Grow);
                    break;

                case TypeDetail.EnergyDrain:
                    _healthManager.IncreaseHP(attackResult.Damage.HP);
                    _healthManager.CurrentMP += attackResult.Damage.MP;
                    _healthManager.CurrentSP += attackResult.Damage.SP;
                    _healthManager.RaiseHitpointsChange();
                    break;

                default:
                    throw new NotImplementedException("Not implemented skill type.");
            }

            if (skill.CanBeActivated)
                skill.IsActivated = !skill.IsActivated;
        }

        /// <summary>
        /// Calculates healing result.
        /// </summary>
        public AttackResult UsedHealingSkill(Skill skill, IKillable target)
        {
            var healHP = skill.HealHP > 0 ? _statsManager.TotalWis * 4 + skill.HealHP : 0;
            var healSP = skill.HealSP;
            var healMP = skill.HealMP;

            if (target.HealthManager.CurrentHP + healHP > target.HealthManager.MaxHP)
                healHP = target.HealthManager.MaxHP - target.HealthManager.CurrentHP;

            if (target.HealthManager.CurrentSP + healSP > target.HealthManager.MaxSP)
                healSP = (ushort)(target.HealthManager.MaxSP - target.HealthManager.CurrentSP);

            if (target.HealthManager.CurrentMP + healHP > target.HealthManager.MaxMP)
                healMP = (ushort)(target.HealthManager.MaxMP - target.HealthManager.CurrentMP);

            AttackResult result = new AttackResult(AttackSuccess.Normal, new Damage((ushort)healHP, healSP, healMP));

            target.HealthManager.IncreaseHP(healHP);
            target.HealthManager.CurrentMP += healMP;
            target.HealthManager.CurrentSP += healSP;

            return result;
        }

        private AttackResult UsedDispelSkill(Skill skill, IKillable target)
        {
            var debuffs = target.BuffsManager.ActiveBuffs.Where(b => b.IsDebuff).ToList();
            foreach (var debuff in debuffs)
            {
                debuff.CancelBuff();
            }

            return new AttackResult(AttackSuccess.Normal, new Damage());
        }

        #endregion

        #region Cooldown

        public ConcurrentDictionary<ushort, DateTime> Cooldowns { get; init; } = new();

        #endregion

        #region Resistanse

        public IList<ushort> ResistSkills { get; init; } = new List<ushort>();

        #endregion

        #region Helpers

        public DateTime? ChargeUsedLastTime { get; private set; } = null;

        private void CancelBuffs(Skill usedSkill)
        {
            var buffs = _buffsManager.ActiveBuffs.Where(b => b.IsCanceledWhenAttack && b.Skill != usedSkill).ToList();
            foreach (var b in buffs)
                b.CancelBuff();
        }

        #endregion
    }
}
