using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Monster
{
    public partial class Mob : BaseKillable, IKiller, IMapMember, IDisposable
    {
        private readonly ILogger<Mob> _logger;
        private readonly DbMob _dbMob;

        public ISpeedManager SpeedManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public ISkillsManager SkillsManager { get; private set; }

        public Mob(ushort mobId,
                   bool shouldRebirth,
                   MoveArea moveArea,
                   Map map,
                   ILogger<Mob> logger,
                   IDatabasePreloader databasePreloader,
                   ICountryProvider countryProvider,
                   IStatsManager statsManager,
                   IHealthManager healthManager,
                   ILevelProvider levelProvider,
                   ISpeedManager speedManager,
                   IAttackManager attackManager,
                   ISkillsManager skillsManager,
                   IBuffsManager buffsManager,
                   IElementProvider elementProvider) : base(databasePreloader, countryProvider, statsManager, healthManager, levelProvider, buffsManager, elementProvider)
        {
            _logger = logger;
            _dbMob = databasePreloader.Mobs[mobId];

            Id = map.GenerateId();
            Exp = _dbMob.Exp;
            AI = _dbMob.AI;
            ShouldRebirth = shouldRebirth;

            StatsManager.Init(Id, 0, _dbMob.Dex, 0, 0, _dbMob.Wis, _dbMob.Luc, def: _dbMob.Defense, res: _dbMob.Magic);
            LevelProvider.Level = _dbMob.Level;
            HealthManager.Init(Id, _dbMob.HP, _dbMob.MP, _dbMob.SP, _dbMob.HP, _dbMob.MP, _dbMob.SP);
            BuffsManager.Init(Id);

            CountryProvider.Init(Id, _dbMob.Fraction);

            SpeedManager = speedManager;
            SpeedManager.Init(Id);

            AttackManager = attackManager;
            AttackManager.Init(Id);

            SkillsManager = skillsManager;
            SkillsManager.Init(Id, new Skill[0]);

            ElementProvider.AttackElement = _dbMob.Element;
            ElementProvider.DefenceElement = _dbMob.Element;

            MoveArea = moveArea;
            Map = map;
            PosX = new Random().NextFloat(MoveArea.X1, MoveArea.X2);
            PosY = new Random().NextFloat(MoveArea.Y1, MoveArea.Y2);
            PosZ = new Random().NextFloat(MoveArea.Z1, MoveArea.Z2);

            IsAttack1Enabled = _dbMob.AttackOk1 != 0;
            IsAttack2Enabled = _dbMob.AttackOk2 != 0;
            IsAttack3Enabled = _dbMob.AttackOk3 != 0;

            if (ShouldRebirth)
            {
                _rebirthTimer.Interval = RespawnTimeInMilliseconds;
                _rebirthTimer.Elapsed += RebirthTimer_Elapsed;

                OnDead += MobRebirth_OnDead;
            }

            SetupAITimers();
            State = MobState.Idle;
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

        #region Untouchable 

        ///  <inheritdoc/>
        public override bool IsUntouchable
        {
            get
            {
                return State == MobState.BackToBirthPosition;
            }
        }
        #endregion

        /// <summary>
        /// Creates mob clone.
        /// </summary>
        public Mob Clone()
        {
            return new Mob(MobId, ShouldRebirth, MoveArea, Map, _logger, _databasePreloader, CountryProvider, StatsManager, HealthManager, LevelProvider, SpeedManager, AttackManager, SkillsManager, BuffsManager, ElementProvider);
        }

        public void Dispose()
        {
            ClearTimers();
        }
    }
}
