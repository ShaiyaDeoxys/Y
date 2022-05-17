using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Numerics;
using System.Timers;

namespace Imgeneus.World.Game.AI
{
    public class AIManager : IAIManager
    {
        private readonly ILogger<AIManager> _logger;
        private readonly IMovementManager _movementManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IAttackManager _attackManager;
        private readonly IUntouchableManager _untouchableManager;
        private readonly IMapProvider _mapProvider;
        private readonly ISkillsManager _skillsManager;
        private readonly IStatsManager _statsManager;
        private readonly IElementProvider _elementProvider;
        private readonly IDatabasePreloader _databasePreloader;

        private int _ownerId;

        private IKiller _owner;
        private IKiller Owner
        {
            get
            {
                if (_mapProvider.Map is null || _mapProvider.CellId == -1) // Still not loaded into map.
                    return null;

                if (_owner is not null)
                    return _owner;

                switch (AI)
                {
                    case MobAI.Guard:
                        _owner = _mapProvider.Map.GetNPC(_mapProvider.CellId, _ownerId) as GuardNpc;
                        break;

                    default:
                        _owner = _mapProvider.Map.GetMob(_mapProvider.CellId, _ownerId);
                        break;
                }

                if (_owner is null)
                {
                    _logger.LogError("Could not find AI {hashcode} in game world.", GetHashCode());
                }

                return _owner;
            }
        }

        public AIManager(ILogger<AIManager> logger, IMovementManager movementManager, ICountryProvider countryProvider, IAttackManager attackManager, IUntouchableManager untouchableManager, IMapProvider mapProvider, ISkillsManager skillsManager, IStatsManager statsManager, IElementProvider elementProvider, IDatabasePreloader databasePreloader)
        {
            _logger = logger;
            _movementManager = movementManager;
            _countryProvider = countryProvider;
            _attackManager = attackManager;
            _untouchableManager = untouchableManager;
            _mapProvider = mapProvider;
            _skillsManager = skillsManager;
            _statsManager = statsManager;
            _elementProvider = elementProvider;
            _databasePreloader = databasePreloader;

            _attackManager.OnTargetChanged += AttackManager_OnTargetChanged;
#if DEBUG
            _logger.LogDebug("AIManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~AIManager()
        {
            _logger.LogDebug("AIManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(int ownerId,
                         MobAI aiType,
                         MoveArea moveArea,
                         int idleTime = 4000,
                         byte chaseRange = 10,
                         byte chaseSpeed = 5,
                         int chaseTime = 1000,
                         byte idleSpeed = 1,
                         bool isAttack1Enabled = false,
                         bool isAttack2Enabled = false,
                         bool isAttack3Enabled = false,
                         byte attack1Range = 1,
                         byte attack2Range = 1,
                         byte attack3Range = 1,
                         ushort attackType1 = 0,
                         ushort attackType2 = 0,
                         ushort attackType3 = 0,
                         Element attackAttrib1 = Element.None,
                         Element attackAttrib2 = Element.None,
                         Element attackAttrib3 = Element.None,
                         ushort attack1 = 0,
                         ushort attack2 = 0,
                         ushort attack3 = 0,
                         ushort attackPlus1 = 0,
                         ushort attackPlus2 = 0,
                         ushort attackPlus3 = 0,
                         int attackTime1 = 0,
                         int attackTime2 = 0,
                         int attackTime3 = 0)
        {
            _ownerId = ownerId;

            AI = aiType;
            MoveArea = moveArea;

            _idleTime = idleTime;
            _chaseRange = chaseRange;
            _chaseSpeed = chaseSpeed;
            _chaseTime = chaseTime;

            _idleSpeed = idleSpeed;

            IsAttack1Enabled = isAttack1Enabled;
            IsAttack2Enabled = isAttack2Enabled;
            IsAttack3Enabled = isAttack3Enabled;

            AttackRange1 = attack1Range;
            AttackRange2 = attack2Range;
            AttackRange3 = attack3Range;

            AttackType1 = attackType1;
            AttackType2 = attackType2;
            AttackType3 = attackType3;

            AttackAttrib1 = attackAttrib1;
            AttackAttrib2 = attackAttrib2;
            AttackAttrib3 = attackAttrib3;

            Attack1 = attack1;
            Attack2 = attack2;
            Attack3 = attack3;

            AttackPlus1 = attackPlus1;
            AttackPlus2 = attackPlus2;
            AttackPlus3 = attackPlus3;

            AttackTime1 = attackTime1;
            AttackTime2 = attackTime2;
            AttackTime3 = attackTime3;

            SetupAITimers();

            State = AIState.Idle;
        }

        public void Dispose()
        {
            _attackManager.OnTargetChanged -= AttackManager_OnTargetChanged;

            ClearTimers();
        }

        #endregion

        #region AI timers

        /// <summary>
        /// Any action, that mob makes should do though this timer.
        /// </summary>
        private Timer _attackTimer = new Timer();

        /// <summary>
        /// Mob walks around each N seconds, when he is in idle state.
        /// </summary>
        private readonly Timer _idleTimer = new Timer();

        /// <summary>
        /// This timer triggers call to map in order to get list of players near by.
        /// </summary>
        private readonly Timer _watchTimer = new Timer();

        /// <summary>
        /// Chase timer triggers check if mob should follow user.
        /// </summary>
        private readonly Timer _chaseTimer = new Timer();

        /// <summary>
        /// Back to birth position timer.
        /// </summary>
        private Timer _backToBirthPositionTimer = new Timer();

        /// <summary>
        /// Configures ai timers.
        /// </summary>
        private void SetupAITimers()
        {
            _maxIdleTime = _idleTime * 10;
            _idleTimer.Interval = _idleRandom.NextDouble(1000, _maxIdleTime);
            _idleTimer.AutoReset = false;
            _idleTimer.Elapsed += IdleTimer_Elapsed;

            _watchTimer.Interval = 1000; // 1 second
            _watchTimer.AutoReset = false;
            _watchTimer.Elapsed += WatchTimer_Elapsed;

            _chaseTimer.Interval = 500; // 0.5 second
            _chaseTimer.AutoReset = false;
            _chaseTimer.Elapsed += ChaseTimer_Elapsed;

            _attackTimer.AutoReset = false;
            _attackTimer.Elapsed += AttackTimer_Elapsed;

            _backToBirthPositionTimer.Interval = 500; // 0.5 second
            _backToBirthPositionTimer.AutoReset = false;
            _backToBirthPositionTimer.Elapsed += BackToBirthPositionTimer_Elapsed;
        }

        /// <summary>
        /// Clears ai timers.
        /// </summary>
        private void ClearTimers()
        {
            _idleTimer.Elapsed -= IdleTimer_Elapsed;
            _watchTimer.Elapsed -= WatchTimer_Elapsed;
            _chaseTimer.Elapsed -= ChaseTimer_Elapsed;
            _attackTimer.Elapsed -= AttackTimer_Elapsed;
            _backToBirthPositionTimer.Elapsed -= BackToBirthPositionTimer_Elapsed;

            _idleTimer.Stop();
            _watchTimer.Stop();
            _chaseTimer.Stop();
            _attackTimer.Stop();
            _backToBirthPositionTimer.Stop();
        }

        #endregion

        #region AI

        /// <summary>
        /// Mob's ai type.
        /// </summary>
        public MobAI AI { get; private set; }

        /// <summary>
        /// Delta between positions.
        /// </summary>
        public readonly float DELTA = 1f;

        private AIState _state = AIState.Idle;

        public AIState State
        {
            get
            {
                return _state;
            }

            private set
            {
                _state = value;

#if DEBUG
                _logger.LogDebug("AI {hashcode} changed state to {state}.", GetHashCode(), _state);
#endif

                switch (_state)
                {
                    case AIState.Idle:
                        // Idle timer generates a mob walk. Not available for altars.
                        if (AI != MobAI.Relic)
                            _idleTimer.Start();

                        // If this is combat mob start watching as soon as it's in idle state.
                        if (AI != MobAI.Peaceful && AI != MobAI.Peaceful2)
                            _watchTimer.Start();

                        _untouchableManager.IsUntouchable = false;
                        StartPosX = -1;
                        StartPosZ = -1;
                        _movementManager.MoveMotion = MoveMotion.Walk;
                        break;

                    case AIState.Chase:
                        StartChasing();
                        break;

                    case AIState.ReadyToAttack:
                        UseAttack();
                        break;

                    case AIState.BackToBirthPosition:
                        StopChasing();
                        ReturnToBirthPosition();
                        _untouchableManager.IsUntouchable = true;
                        break;

                    default:
                        _logger.LogWarning("Not implemented mob state: {state}.", _state);
                        break;
                }

                OnStateChanged?.Invoke(_state);
            }
        }

        public event Action<AIState> OnStateChanged;

        /// <summary>
        /// Turns on ai of mob, based on its' type.
        /// </summary>
        public void SelectActionBasedOnAI()
        {
            switch (AI)
            {
                case MobAI.Combative:
                case MobAI.Peaceful:
                case MobAI.Guard:
                    if (_chaseSpeed > 0)
                        State = AIState.Chase;
                    else
                        if (_attackManager.Target != null && MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ) <= _chaseRange)
                        State = AIState.ReadyToAttack;
                    else
                        State = AIState.Idle;
                    break;

                case MobAI.Relic:
                    if (_attackManager.Target != null && MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ) <= _chaseRange)
                        State = AIState.ReadyToAttack;
                    else
                        State = AIState.Idle;
                    break;

                default:
                    _logger.LogWarning("AI {hashcode} has not implement ai type - {AI}, falling back to combative type.", GetHashCode(), AI);
                    State = AIState.Chase;
                    break;
            }
        }

        #endregion

        #region Target

        private IKillable _target;

        private void AttackManager_OnTargetChanged(IKillable newTarget)
        {
            if (_target != null)
            {
                _target.HealthManager.OnDead -= Target_OnDead;
                _target.HealthManager.OnIsAttackableChanged -= Target_OnIsAttackableChanged;

                if (_target is Character player)
                    player.StealthManager.OnStealthChange -= Target_OnStealth;
            }

            _target = newTarget;

            if (_target != null)
            {
                _target.HealthManager.OnDead += Target_OnDead;
                _target.HealthManager.OnIsAttackableChanged += Target_OnIsAttackableChanged;

                if (_target is Character player)
                    player.StealthManager.OnStealthChange += Target_OnStealth;
            }

            if (_target is null)
                State = AIState.BackToBirthPosition;
        }

        /// <summary>
        /// When target (player) goes into stealth or turns into a mob, mob returns to its' original place.
        /// </summary>
        private void Target_OnStealth(int sender)
        {
            if ((_target as Character).StealthManager.IsStealth)
                _attackManager.Target = null;
        }

        /// <summary>
        /// When target is dead, mob returns to its' original place.
        /// </summary>
        /// <param name="senderId">player, that is dead</param>
        /// <param name="killer">player's killer</param>
        private void Target_OnDead(int senderId, IKiller killer)
        {
            _attackManager.Target = null;
        }

        private void Target_OnIsAttackableChanged(bool isAttackable)
        {
            if (!isAttackable)
                _attackManager.Target = null;
        }

        #endregion

        #region Idle

        private byte _idleSpeed = 1;

        /// <summary>
        /// Generates random interval for idle walking.
        /// </summary>
        private readonly Random _idleRandom = new Random();

        private int _idleTime = 4000;

        /// <summary>
        /// Max idle time.
        /// </summary>
        private double _maxIdleTime;

        private void IdleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (State != AIState.Idle)
                return;

            GenerateRandomIdlePosition();

            _idleTimer.Interval = _idleRandom.NextDouble(_maxIdleTime / 2, _maxIdleTime);
            _idleTimer.Start();
        }


        /// <summary>
        /// Generates new position for idle move.
        /// </summary>
        private void GenerateRandomIdlePosition()
        {
            float x1 = _movementManager.PosX - _idleSpeed;
            if (x1 < MoveArea.X1)
                x1 = MoveArea.X1;
            float x2 = _movementManager.PosX + _idleSpeed;
            if (x2 > MoveArea.X2)
                x2 = MoveArea.X2;

            float z1 = _movementManager.PosZ - _idleSpeed;
            if (z1 < MoveArea.Z1)
                z1 = MoveArea.Z1;
            float z2 = _movementManager.PosZ + _idleSpeed;
            if (z2 < MoveArea.Z2)
                z2 = MoveArea.Z2;

            var x = new Random().NextFloat(x1, x2);
            var z = new Random().NextFloat(z1, z2);

            Move(x, z);

#if DEBUG
            _logger.LogDebug("AI {hashcode} walks to new position x={PosX} y={PosY} z={PosZ}.", GetHashCode(), _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ);
#endif

            _movementManager.RaisePositionChanged();
        }


        #endregion

        #region Watch

        private void WatchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (State != AIState.Idle)
                return;

            if (TryGetEnemy())
                SelectActionBasedOnAI();
            else
                _watchTimer.Start();
        }

        public bool TryGetEnemy()
        {
            if (Owner is null) // Still not loaded into map.
                return false;

            var enemies = _mapProvider.Map.Cells[_mapProvider.CellId].GetEnemies(Owner, _movementManager.PosX, _movementManager.PosZ, _chaseRange);

            var anyVisibleEnemy = enemies.Any(x =>
            {
                if (x is Character character)
                    return character.StealthManager.IsStealth == false;

                return true;
            });

            // No enemies, keep watching.
            if (!anyVisibleEnemy)
            {
                _watchTimer.Start();
                return false;
            }

            // There is some player in vision.
            _attackManager.Target = enemies.First(x =>
            {
                if (x is Character character)
                    return character.StealthManager.IsStealth == false;

                return true;
            });

            return _attackManager.Target != null;
        }

        #endregion

        #region Chase

        /// <summary>
        /// AI speed, when it's chasing player.
        /// </summary>
        private byte _chaseSpeed = 5;

        /// <summary>
        /// How far away AI can chase player.
        /// </summary>
        private byte _chaseRange = 10;

        /// <summary>
        /// Delay between actions in chase state.
        /// </summary>
        private int _chaseTime = 1;

        /// <summary>
        /// Start chasing player.
        /// </summary>
        private void StartChasing()
        {
            _movementManager.MoveMotion = MoveMotion.Run;

            StartPosX = _movementManager.PosX;
            StartPosZ = _movementManager.PosZ;

            _chaseTimer.Start();
        }

        /// <summary>
        /// Stops chasing player.
        /// </summary>
        private void StopChasing()
        {
            _chaseTimer.Stop();
        }

        private void ChaseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_attackManager.Target is null)
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} target is already cleared.", GetHashCode());
#endif
                State = AIState.BackToBirthPosition;
                return;
            }

            var distanceToPlayer = Math.Round(MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ));
            if (distanceToPlayer <= AttackRange1 || distanceToPlayer <= AttackRange2 || distanceToPlayer <= AttackRange3)
            {
                State = AIState.ReadyToAttack;
                //_chaseTimer.Start();
                return;
            }

            Move(_attackManager.Target.MovementManager.PosX, _attackManager.Target.MovementManager.PosZ);

            if (IsTooFarAway)
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} is too far away from its' birth position, returing home.", GetHashCode());
#endif
                State = AIState.BackToBirthPosition;
            }
            else
            {
                _chaseTimer.Start();
            }
        }

        #endregion

        #region Move

        /// <summary>
        /// AI's move area. It can not move further than this area.
        /// </summary>
        public MoveArea MoveArea { get; private set; }

        /// <summary>
        /// Since when we sent the last update to players about mob position.
        /// </summary>
        private DateTime _lastMoveUpdateSent;

        /// <summary>
        /// Used for calculation delta time.
        /// </summary>
        private DateTime _lastMoveUpdate;

        /// <summary>
        /// Moves AI to the specified position.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="z">z coordinate</param>
        private void Move(float x, float z)
        {
#if DEBUG
            _logger.LogDebug("AI {hashcode} is moving to target: x - {x}, z - {z}, target x - {targetX}, target z - {targetZ}", GetHashCode(), _movementManager.PosX, _movementManager.PosZ, x, z);
#endif

            if (MathExtensions.Distance(_movementManager.PosX, x, _movementManager.PosZ, x) < DELTA)
                return;

            if (_chaseSpeed == 0 || _chaseTime == 0)
                return;

            var now = DateTime.UtcNow;
            var mobVector = new Vector2(_movementManager.PosX, _movementManager.PosZ);
            var destinationVector = new Vector2(x, z);

            var normalizedVector = Vector2.Normalize(destinationVector - mobVector);
            var deltaTime = now.Subtract(_lastMoveUpdate);
            var deltaMilliseconds = deltaTime.TotalMilliseconds > 2000 ? 500 : deltaTime.TotalMilliseconds;
            var temp = normalizedVector * (float)(State == AIState.Idle ? _idleSpeed : _chaseSpeed * 1.0 / (State == AIState.Idle ? _idleTime : _chaseTime) * deltaMilliseconds);
            _movementManager.PosX += float.IsNaN(temp.X) ? 0 : temp.X;
            _movementManager.PosZ += float.IsNaN(temp.Y) ? 0 : temp.Y;

#if DEBUG
            _logger.LogDebug("AI {hashcode} position: x - {x}, z - {z}", GetHashCode(), _movementManager.PosX, _movementManager.PosZ);
#endif

            _lastMoveUpdate = now;

            // Send update to players, that mob position has changed.
            if (DateTime.UtcNow.Subtract(_lastMoveUpdateSent).TotalMilliseconds > 1000)
            {
                _movementManager.RaisePositionChanged();
                _lastMoveUpdateSent = now;
            }
        }

        #endregion

        #region Return to birth place

        /// <summary>
        /// Position x, where mob started chasing.
        /// </summary>
        private float StartPosX = -1;

        /// <summary>
        /// Position z, where mob started chasing.
        /// </summary>
        private float StartPosZ = -1;

        /// <summary>
        /// Is mob too far away from its' area?
        /// </summary>
        private bool IsTooFarAway
        {
            get
            {
                return MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ) > 45;
            }
        }

        /// <summary>
        /// Returns mob back to birth position.
        /// </summary>
        private void ReturnToBirthPosition()
        {
            if (Math.Round(MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ)) > DELTA)
            {
                _backToBirthPositionTimer.Start();
            }
            else
            {
                State = AIState.Idle;
            }
        }

        private void BackToBirthPositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Math.Round(MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ)) > DELTA)
            {
                Move(StartPosX, StartPosZ);
                _backToBirthPositionTimer.Start();
            }
            else
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} reached birth position, back to idle state.", GetHashCode());
#endif
                State = AIState.Idle;
            }
        }

        #endregion

        #region Attack

        /// <summary>
        /// When time from the last attack elapsed, we can decide what to do next.
        /// </summary>
        private void AttackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SelectActionBasedOnAI();
        }

        /// <summary>
        /// Uses 1 from 3 available attacks.
        /// </summary>
        public void UseAttack()
        {
            var distanceToPlayer = Math.Round(MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ));
            var now = DateTime.UtcNow;
            int delay = 1000;
            var attackId = RandomiseAttack(now);
            var useAttack1 = attackId == 1;
            var useAttack2 = attackId == 2;
            var useAttack3 = attackId == 3;

            if (useAttack1 && (distanceToPlayer <= AttackRange1 || AttackRange1 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 1.", GetHashCode());
#endif
                Attack(AttackType1, AttackAttrib1, Attack1, AttackPlus1);
                _lastAttack1Time = now;
                delay = AttackTime1 > 0 ? AttackTime1 : 5000;
            }

            if (useAttack2 && (distanceToPlayer <= AttackRange2 || AttackRange2 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 2.", GetHashCode());
#endif
                Attack(AttackType2, AttackAttrib2, Attack2, AttackPlus2);
                _lastAttack2Time = now;
                delay = AttackTime2 > 0 ? AttackTime2 : 5000;
            }

            if (useAttack3 && (distanceToPlayer <= AttackRange3 || AttackRange3 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 3.", GetHashCode());
#endif
                Attack(AttackType3, Element.None, Attack3, AttackPlus3);
                _lastAttack3Time = now;
                delay = AttackTime3 > 0 ? AttackTime3 : 5000;
            }

            _attackTimer.Interval = delay;
            _attackTimer.Start();
        }

        /// <summary>
        /// Randomly selects the next attack.
        /// </summary>
        /// <param name="now">now time</param>
        /// <returns>attack type: 1, 2, 3 or 0, when can not attack</returns>
        private byte RandomiseAttack(DateTime now)
        {
            var useAttack1 = false;
            var useAttack2 = false;
            var useAttack3 = false;

            int chanceForAttack1 = 0;
            int chanceForAttack2 = 0;
            int chanceForAttack3 = 0;

            if (IsAttack1Enabled && IsAttack2Enabled && IsAttack3Enabled)
            {
                if (now.Subtract(_lastAttack1Time).TotalMilliseconds >= AttackTime1)
                    chanceForAttack1 = 60;
                else
                    chanceForAttack1 = 0;

                if (now.Subtract(_lastAttack2Time).TotalMilliseconds >= AttackTime2)
                    chanceForAttack2 = 85;
                else
                    chanceForAttack2 = 0;

                if (now.Subtract(_lastAttack3Time).TotalMilliseconds >= AttackTime3)
                    chanceForAttack3 = 100;
                else
                    chanceForAttack3 = 0;
            }
            else if (IsAttack1Enabled && IsAttack2Enabled && !IsAttack3Enabled)
            {
                if (now.Subtract(_lastAttack1Time).TotalMilliseconds >= AttackTime1)
                    chanceForAttack1 = 70;
                else
                    chanceForAttack1 = 0;

                if (now.Subtract(_lastAttack2Time).TotalMilliseconds >= AttackTime2)
                    chanceForAttack2 = 100;
                else
                    chanceForAttack2 = 0;

                chanceForAttack3 = 0;
            }
            else if (IsAttack1Enabled && !IsAttack2Enabled && !IsAttack3Enabled)
            {
                if (now.Subtract(_lastAttack1Time).TotalMilliseconds >= AttackTime1)
                    chanceForAttack1 = 100;
                else
                    chanceForAttack1 = 0;

                chanceForAttack2 = 0;
                chanceForAttack3 = 0;
            }
            if (!IsAttack1Enabled && !IsAttack2Enabled && !IsAttack3Enabled)
            {
                chanceForAttack1 = 0;
                chanceForAttack2 = 0;
                chanceForAttack3 = 0;
            }

            var random = new Random().Next(1, 100);
            if (random <= chanceForAttack1)
                useAttack1 = true;
            else if (random > chanceForAttack1 && random <= chanceForAttack2)
                useAttack2 = true;
            else if (random > chanceForAttack2 && random <= chanceForAttack3)
                useAttack3 = true;

            if (useAttack1)
                return 1;
            else if (useAttack2)
                return 2;
            else if (useAttack3)
                return 3;
            else
                return 0;
        }

        /// <summary>
        /// Uses some attack.
        /// </summary>
        /// <param name="skillId">skill id</param>
        /// <param name="minAttack">min damage</param>
        /// <param name="element">element</param>
        /// <param name="additionalDamage">plus damage</param>
        public void Attack(ushort skillId, Element element, ushort minAttack, ushort additionalDamage)
        {
            var isMeleeAttack = false;
            Skill skill = null;
            if (skillId == 0) // Usual melee attack.
            {
                isMeleeAttack = true;
            }
            else
            {
                if (_databasePreloader.Skills.TryGetValue((skillId, 100), out var dbSkill))
                {
                    skill = new Skill(dbSkill, 0, 0);
                }
                else
                {
                    isMeleeAttack = true;
                    _logger.LogError("AI {hashcode} used unknow skill {skillId}, fallback to melee attack.", GetHashCode(), skillId);
                }
            }

            if (isMeleeAttack)
            {
                _statsManager.WeaponMinAttack = minAttack;
                _statsManager.WeaponMaxAttack = minAttack + additionalDamage;
                _elementProvider.AttackSkillElement = _elementProvider.ConstAttackElement;

                _attackManager.AutoAttack(Owner);
            }
            else
            {
                try
                {
                    _elementProvider.AttackSkillElement = element;
                    _skillsManager.UseSkill(skill, Owner, _attackManager.Target);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to use skill, reason: {message}. Fallback to melee attack.", ex.Message);

                    _statsManager.WeaponMinAttack = minAttack;
                    _statsManager.WeaponMaxAttack = minAttack + additionalDamage;

                    _attackManager.AutoAttack(Owner);
                }
            }
        }

        #endregion

        #region Attack 1

        /// <summary>
        /// Time since the last attack 1.
        /// </summary>
        private DateTime _lastAttack1Time;

        /// <summary>
        /// Indicator of attack 1.
        /// </summary>
        private bool IsAttack1Enabled;

        /// <summary>
        /// Range.
        /// </summary>
        private byte AttackRange1;

        /// <summary>
        /// List of skills (NpcSkills.SData).
        /// </summary>
        private ushort AttackType1;

        /// <summary>
        /// Element.
        /// </summary>
        private Element AttackAttrib1;

        /// <summary>
        /// Min damage.
        /// </summary>
        private ushort Attack1;

        /// <summary>
        /// Additional damage.
        /// </summary>
        private ushort AttackPlus1;

        /// <summary>
        /// Delay.
        /// </summary>
        private int AttackTime1;

        #endregion

        #region Attack 2

        /// <summary>
        /// Time since the last attack 2.
        /// </summary>
        private DateTime _lastAttack2Time;

        /// <summary>
        /// Indicator of attack 2.
        /// </summary>
        private bool IsAttack2Enabled;

        /// <summary>
        /// Range.
        /// </summary>
        private byte AttackRange2;

        /// <summary>
        /// List of skills (NpcSkills.SData).
        /// </summary>
        private ushort AttackType2;

        /// <summary>
        /// Element.
        /// </summary>
        private Element AttackAttrib2;

        /// <summary>
        /// Min damage.
        /// </summary>
        private ushort Attack2;

        /// <summary>
        /// Additional damage.
        /// </summary>
        private ushort AttackPlus2;

        /// <summary>
        /// Delay.
        /// </summary>
        private int AttackTime2;

        #endregion

        #region Attack 3

        /// <summary>
        /// Time since the last attack 3.
        /// </summary>
        private DateTime _lastAttack3Time;

        /// <summary>
        /// Indicator of attack 3.
        /// </summary>
        private bool IsAttack3Enabled;

        /// <summary>
        /// Range.
        /// </summary>
        private byte AttackRange3;

        /// <summary>
        /// List of skills (NpcSkills.SData).
        /// </summary>
        private ushort AttackType3;

        /// <summary>
        /// Element.
        /// </summary>
        private Element AttackAttrib3;

        /// <summary>
        /// Min damage.
        /// </summary>
        private ushort Attack3;

        /// <summary>
        /// Additional damage.
        /// </summary>
        private ushort AttackPlus3;

        /// <summary>
        /// Delay.
        /// </summary>
        private int AttackTime3;

        #endregion
    }
}
