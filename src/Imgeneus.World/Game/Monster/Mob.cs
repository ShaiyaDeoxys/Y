using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Monster
{
    public partial class Mob : BaseKillable, IKiller, IMapMember, IDisposable
    {
        private readonly ILogger<Mob> _logger;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly DbMob _dbMob;
        private readonly MoveArea _moveArea;

        public IAIManager AIManager { get; private set; }
        public ISpeedManager SpeedManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public ISkillsManager SkillsManager { get; private set; }

        /// <summary>
        /// My scope.
        /// </summary>
        public IServiceScope Scope { get; set; }

        public Mob(ushort mobId,
                   bool shouldRebirth,
                   MoveArea moveArea,
                   ILogger<Mob> logger,
                   IDatabasePreloader databasePreloader,
                   IAIManager aiManager,
                   IItemEnchantConfiguration enchantConfig,
                   IItemCreateConfiguration itemCreateConfig,
                   ICountryProvider countryProvider,
                   IStatsManager statsManager,
                   IHealthManager healthManager,
                   ILevelProvider levelProvider,
                   ISpeedManager speedManager,
                   IAttackManager attackManager,
                   ISkillsManager skillsManager,
                   IBuffsManager buffsManager,
                   IElementProvider elementProvider,
                   IMovementManager movementManager,
                   IUntouchableManager untouchableManager,
                   IMapProvider mapProvider) : base(databasePreloader, countryProvider, statsManager, healthManager, levelProvider, buffsManager, elementProvider, movementManager, untouchableManager, mapProvider)
        {
            _logger = logger;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _dbMob = databasePreloader.Mobs[mobId];
            _moveArea = moveArea;

            AIManager = aiManager;

            Exp = _dbMob.Exp;
            ShouldRebirth = shouldRebirth;

            SpeedManager = speedManager;
            AttackManager = attackManager;
            SkillsManager = skillsManager;

            ElementProvider.ConstAttackElement = _dbMob.Element;
            ElementProvider.ConstDefenceElement = _dbMob.Element;

            if (ShouldRebirth)
            {
                _rebirthTimer.Interval = RespawnTimeInMilliseconds;
                _rebirthTimer.Elapsed += RebirthTimer_Elapsed;

                HealthManager.OnDead += MobRebirth_OnDead;
            }

            HealthManager.OnGotDamage += OnDecreaseHP;
            AIManager.OnStateChanged += AIManager_OnStateChanged;
        }

        public void Init(int ownerId)
        {
            Id = ownerId;

            StatsManager.Init(Id, 0, _dbMob.Dex, 0, 0, _dbMob.Wis, _dbMob.Luc, def: _dbMob.Defense, res: _dbMob.Magic);
            LevelProvider.Init(Id, _dbMob.Level);
            HealthManager.Init(Id, _dbMob.HP, _dbMob.MP, _dbMob.SP, _dbMob.HP, _dbMob.MP, _dbMob.SP);
            BuffsManager.Init(Id);
            CountryProvider.Init(Id, _dbMob.Fraction);
            SpeedManager.Init(Id);
            AttackManager.Init(Id);
            SkillsManager.Init(Id, new Skill[0]);

            var x = new Random().NextFloat(_moveArea.X1, _moveArea.X2);
            var y = new Random().NextFloat(_moveArea.Y1, _moveArea.Y2);
            var z = new Random().NextFloat(_moveArea.Z1, _moveArea.Z2);
            MovementManager.Init(Id, x, y, z, 0, MoveMotion.Walk);

            AIManager.Init(Id,
                           _dbMob.AI,
                           _moveArea,
                           idleTime: _dbMob.NormalTime <= 0 ? 4000 : _dbMob.NormalTime,
                           idleSpeed: _dbMob.NormalStep,
                           chaseRange: _dbMob.ChaseRange,
                           chaseSpeed: _dbMob.ChaseStep,
                           chaseTime: _dbMob.ChaseTime,
                           isAttack1Enabled: _dbMob.AttackOk1 != 0,
                           isAttack2Enabled: _dbMob.AttackOk2 != 0,
                           isAttack3Enabled: _dbMob.AttackOk3 != 0,
                           attack1Range: _dbMob.AttackRange1,
                           attack2Range: _dbMob.AttackRange2,
                           attack3Range: _dbMob.AttackRange3,
                           attackType1: _dbMob.AttackType1,
                           attackType2: _dbMob.AttackType2,
                           attackType3: _dbMob.AttackType3,
                           attackAttrib1: _dbMob.AttackAttrib1,
                           attackAttrib2: _dbMob.AttackAttrib2,
                           attack1: _dbMob.Attack1 < 0 ? (ushort)(_dbMob.Attack1 + ushort.MaxValue) : (ushort)_dbMob.Attack1,
                           attack2: _dbMob.Attack2 < 0 ? (ushort)(_dbMob.Attack2 + ushort.MaxValue) : (ushort)_dbMob.Attack2,
                           attack3: _dbMob.Attack3 < 0 ? (ushort)(_dbMob.Attack3 + ushort.MaxValue) : (ushort)_dbMob.Attack3,
                           attackTime1: _dbMob.AttackTime1,
                           attackTime2: _dbMob.AttackTime2,
                           attackTime3: _dbMob.AttackTime3);
        }

        private void AIManager_OnStateChanged(AIState newState)
        {
            if (newState == AIState.Idle)
                HealthManager.FullRecover();
        }

        /// <summary>
        /// Mob id from database.
        /// </summary>
        public ushort MobId => _dbMob.Id;

        /// <summary>
        /// Indicator, that shows if mob should rebirth after its' death.
        /// </summary>
        public bool ShouldRebirth { get; }

        /// <summary>
        /// Experience gained by a player who kills a mob.
        /// </summary>
        public short Exp { get; }

        /// <summary>
        /// During GBR how many points added to guild.
        /// </summary>
        public short GuildPoints => _dbMob.MoneyMax;

        /// <summary>
        /// Creates mob clone.
        /// </summary>
        public Mob Clone()
        {
            return new Mob(MobId, ShouldRebirth, _moveArea, _logger, _databasePreloader, AIManager, _enchantConfig, _itemCreateConfig, CountryProvider, StatsManager, HealthManager, LevelProvider, SpeedManager, AttackManager, SkillsManager, BuffsManager, ElementProvider, MovementManager, UntouchableManager, MapProvider);
        }

        public void Dispose()
        {
            HealthManager.OnDead -= MobRebirth_OnDead;
            HealthManager.OnGotDamage -= OnDecreaseHP;
            AIManager.OnStateChanged -= AIManager_OnStateChanged;

            Scope?.Dispose();
        }
    }
}
