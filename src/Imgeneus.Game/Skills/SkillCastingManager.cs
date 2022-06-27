using Imgeneus.World.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Teleport;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace Imgeneus.Game.Skills
{
    public class SkillCastingManager : ISkillCastingManager
    {
        private readonly ILogger<SkillCastingManager> _logger;
        private readonly IMovementManager _movementManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IHealthManager _healthManager;
        private readonly ISkillsManager _skillsManager;
        private readonly IBuffsManager _buffsManager;
        private readonly IGameWorld _gameWorld;
        private readonly ICastProtectionManager _castProtectionManager;
        private uint _ownerId;

        public SkillCastingManager(ILogger<SkillCastingManager> logger, IMovementManager movementManager, ITeleportationManager teleportationManager, IHealthManager healthManager, ISkillsManager skillsManager, IBuffsManager buffsManager, IGameWorld gameWorld, ICastProtectionManager castProtectionManager)
        {
            _logger = logger;
            _movementManager = movementManager;
            _teleportationManager = teleportationManager;
            _healthManager = healthManager;
            _skillsManager = skillsManager;
            _buffsManager = buffsManager;
            _gameWorld = gameWorld;
            _castProtectionManager = castProtectionManager;

            _castTimer.Elapsed += CastTimer_Elapsed;
            _movementManager.OnMove += MovementManager_OnMove;
            _healthManager.OnGotDamage += HealthManager_OnGotDamage;
            _buffsManager.OnBuffAdded += BuffsManager_OnBuffAdded;
            _teleportationManager.OnTeleporting += TeleportationManager_OnTeleporting;

#if DEBUG
            _logger.LogDebug("SkillCastingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SkillCastingManager()
        {
            _logger.LogDebug("SkillCastingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _castTimer.Elapsed -= CastTimer_Elapsed;
            _movementManager.OnMove -= MovementManager_OnMove;
            _healthManager.OnGotDamage -= HealthManager_OnGotDamage;
            _buffsManager.OnBuffAdded -= BuffsManager_OnBuffAdded;
            _teleportationManager.OnTeleporting -= TeleportationManager_OnTeleporting;
        }

        #endregion

        /// <summary>
        /// The timer, that is starting skill after cast time.
        /// </summary>
        private Timer _castTimer = new Timer();

        public Skill SkillInCast { get; private set; }

        /// <summary>
        /// Target for which we are casting spell.
        /// </summary>
        private IKillable _targetInCast;

        /// <summary>
        /// Event, that is fired, when user starts casting.
        /// </summary>
        public event Action<uint, IKillable, Skill> OnSkillCastStarted;

        public void StartCasting(Skill skill, IKillable target)
        {
            if (!_skillsManager.CanUseSkill(skill, target, out var success))
                return;

            SkillInCast = skill;
            _targetInCast = target;
            _castTimer.Interval = _castProtectionManager.ReduceCastingTime ? skill.CastTime / 2 : skill.CastTime;
            _castTimer.Start();
            OnSkillCastStarted?.Invoke(_ownerId, _targetInCast, skill);
        }

        /// <summary>
        /// When time for casting has elapsed.
        /// </summary>
        private void CastTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _castTimer.Stop();
            if (_skillsManager.CanUseSkill(SkillInCast, _targetInCast, out var success))
                _skillsManager.UseSkill(SkillInCast, _gameWorld.Players[_ownerId], _targetInCast);

            SkillInCast = null;
            _targetInCast = null;
        }

        private void MovementManager_OnMove(uint senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            CancelCasting();
        }

        private void TeleportationManager_OnTeleporting(uint senderId, ushort mapId, float x, float y, float z, bool teleportedByAdmin, bool summonedByAdmin)
        {
            CancelCasting();
        }

        private void HealthManager_OnGotDamage(uint senderId, IKiller gamageMaker, int damage)
        {
            if (_castProtectionManager.IsCastProtected)
                return;

            CancelCasting();
        }

        private void BuffsManager_OnBuffAdded(uint senderId, Buff buff)
        {
            if (_castProtectionManager.IsCastProtected)
                return;

            if (buff.IsDebuff)
                CancelCasting();
        }

        public void CancelCasting()
        {
            _castTimer.Stop();
            SkillInCast = null;
            _targetInCast = null;
        }
    }
}
