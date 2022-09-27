using Imgeneus.Core.Extensions;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.GameDefinitions.Constants;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using Element = Imgeneus.Database.Constants.Element;
using Timer = System.Timers.Timer;

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
        private readonly IGameDefinitionsPreloder _definitionsPreloder;
        private readonly ISpeedManager _speedManager;
        private readonly IHealthManager _healthManager;
        private readonly IBuffsManager _buffsManager;
        private uint _ownerId;

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

        public AIManager(ILogger<AIManager> logger, IMovementManager movementManager, ICountryProvider countryProvider, IAttackManager attackManager, IUntouchableManager untouchableManager, IMapProvider mapProvider, ISkillsManager skillsManager, IStatsManager statsManager, IElementProvider elementProvider, IGameDefinitionsPreloder definitionsPreloder, ISpeedManager speedManager, IHealthManager healthManager, IBuffsManager buffsManager)
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
            _definitionsPreloder = definitionsPreloder;
            _speedManager = speedManager;
            _healthManager = healthManager;
            _buffsManager = buffsManager;
            _attackManager.OnTargetChanged += AttackManager_OnTargetChanged;
            _healthManager.OnGotDamage += OnDecreaseHP;
            _buffsManager.OnBuffAdded += OnBuffAdded;
            _buffsManager.OnBuffRemoved += OnBuffRemoved;
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

        private bool _initialized = false;

        public void Init(uint ownerId,
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

            if (_initialized) // When rebirth mob, we are reusing the same ai manager.
                return;

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

            _initialized = true;
        }

        public void Dispose()
        {
            _attackManager.OnTargetChanged -= AttackManager_OnTargetChanged;
            _healthManager.OnGotDamage -= OnDecreaseHP;
            _buffsManager.OnBuffAdded -= OnBuffAdded;
            _buffsManager.OnBuffRemoved -= OnBuffRemoved;

            ClearTimers();
            Agro.Clear();
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

        public void Start()
        {
            State = AIState.Idle;
        }

        public void Stop()
        {
            State = AIState.Stopped;
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

        private AIState _state = AIState.Stopped;

        private object _stateSyncObject = new();

        public AIState State
        {
            get
            {
                return _state;
            }

            private set
            {
                lock (_stateSyncObject)
                {
                    // State machine Idle => Chase => Too far away?
                    // Yes: BackToBirthPosition => Idle
                    // No: ReadyToAttack => Chase

                    if (_state == AIState.Idle && (value != AIState.Chase && value != AIState.Stopped && (AI == MobAI.Relic && value != AIState.ReadyToAttack)))
                    {
#if DEBUG
                        _logger.LogError("Can not go from {a} to {b}", _state, value);
#endif
                        return;
                    }

                    if (_state == AIState.Chase && value != AIState.ReadyToAttack && value != AIState.BackToBirthPosition && value != AIState.Stopped)
                    {
#if DEBUG
                        _logger.LogError("Can not go from {a} to {b}", _state, value);
#endif
                        return;
                    }

                    if (_state == AIState.ReadyToAttack && (value != AIState.Chase && value != AIState.Stopped && (AI == MobAI.Relic && value != AIState.ReadyToAttack && value != AIState.Idle)))
                    {
#if DEBUG
                        _logger.LogError("Can not go from {a} to {b}", _state, value);
#endif
                        return;
                    }

                    if (_state == AIState.BackToBirthPosition && value != AIState.Idle)
                    {
#if DEBUG
                        _logger.LogError("Can not go from {a} to {b}", _state, value);
#endif
                        return;
                    }

                    if (_state == AIState.Stopped && value != AIState.Idle)
                    {
#if DEBUG
                        _logger.LogError("Can not go from {a} to {b}", _state, value);
#endif
                        return;
                    }

                    _state = value;

#if DEBUG
                    _logger.LogDebug("AI {hashcode} changed state to {state}.", GetHashCode(), _state);
#endif

                    switch (_state)
                    {
                        case AIState.Idle:

#if !DEBUG
                            // Idle timer generates a mob walk. Not available for altars.
                            if (AI != MobAI.Relic)
                                _idleTimer.Start();
#endif

                            // If this is combat mob start watching as soon as it's in idle state.
                            if (AI != MobAI.Peaceful && AI != MobAI.Peaceful2)
                                _watchTimer.Start();

                            _untouchableManager.IsUntouchable = false;
                            StartPosX = -1;
                            StartPosZ = -1;
                            _movementManager.MoveMotion = MoveMotion.Walk;

                            if (AI != MobAI.Guard)
                                _healthManager.FullRecover();

                            Agro.Clear();
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

                        case AIState.Stopped:
                            _idleTimer.Stop();
                            _watchTimer.Stop();
                            _chaseTimer.Stop();
                            _attackTimer.Stop();
                            _backToBirthPositionTimer.Stop();
                            break;

                        default:
                            _logger.LogWarning("Not implemented mob state: {state}.", _state);
                            break;
                    }

                    OnStateChanged?.Invoke(_state);
                }
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
                case MobAI.Relic:
                    if (_chaseSpeed > 0)
                        State = AIState.Chase;
                    else
                        if (_attackManager.Target != null && MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ) <= _chaseRange)
                        State = AIState.ReadyToAttack;
                    else
                        State = AIState.Idle;
                    break;

                default:
                    _logger.LogWarning("AI {hashcode} has not implement ai type - {AI}, falling back to combative type.", GetHashCode(), AI);
                    if (_chaseSpeed > 0)
                        State = AIState.Chase;
                    else
                        if (_attackManager.Target != null && MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ) <= _chaseRange)
                        State = AIState.ReadyToAttack;
                    else
                        State = AIState.Idle;
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

                SelectActionBasedOnAI();
            }
        }

        /// <summary>
        /// When target (player) goes into stealth or turns into a mob, mob returns to its' original place.
        /// </summary>
        private void Target_OnStealth(uint sender)
        {
            if ((_target as Character).StealthManager.IsStealth)
                _attackManager.Target = null;
        }

        /// <summary>
        /// When target is dead, mob returns to its' original place.
        /// </summary>
        /// <param name="senderId">player, that is dead</param>
        /// <param name="killer">player's killer</param>
        private void Target_OnDead(uint senderId, IKiller killer)
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
            var x = new Random().NextFloat(MoveArea.X1, MoveArea.X2);
            var z = new Random().NextFloat(MoveArea.Z1, MoveArea.Z2);

            Move(x, z);

#if DEBUG
            _logger.LogDebug("AI {hashcode} walks to new position x={x} z={z}.", GetHashCode(), x, z);
#endif

            _movementManager.RaisePositionChanged();
        }


        #endregion

        #region Watch

        private void WatchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (State != AIState.Idle)
                return;

            var ok = TryGetEnemy();
            if (!ok)
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
                    return character.StealthManager.IsStealth == false &&
                           character.ShapeManager.Shape != ShapeEnum.Fox &&
                           character.ShapeManager.Shape != ShapeEnum.Wolf &&
                           character.ShapeManager.Shape != ShapeEnum.Knight &&
                           character.ShapeManager.Shape != ShapeEnum.Chicken &&
                           character.ShapeManager.Shape != ShapeEnum.Dog &&
                           character.ShapeManager.Shape != ShapeEnum.Horse &&
                           character.ShapeManager.Shape != ShapeEnum.Pig &&
                           character.ShapeManager.Shape != ShapeEnum.Mob;

                return true;
            });

            // No enemies, keep watching.
            if (!anyVisibleEnemy)
            {
                _watchTimer.Start();
                return false;
            }

            // There is some player in vision.
            _attackManager.Target = enemies.FirstOrDefault(x =>
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

            _chaseTimer.Enabled = true;
            _attackTimer.Enabled = true;

            _chaseTimer.Start();
        }

        /// <summary>
        /// Stops chasing player.
        /// </summary>
        private void StopChasing()
        {
#if DEBUG
            _logger.LogDebug("AI {id} stopped chasing.", _ownerId);
#endif
            _chaseTimer.Stop();
            _attackTimer.Stop();
        }

        private void ChaseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_attackManager.Target is null)
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} target is already cleared.", GetHashCode());
#endif
                if (State == AIState.Chase || State == AIState.ReadyToAttack && !_healthManager.IsDead)
                    State = AIState.BackToBirthPosition;
                return;
            }

            var distanceToTarget = MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ);
#if DEBUG
            _logger.LogInformation("AI {hashcode} distance to target {distanceToTarget}.", GetHashCode(), distanceToTarget);
#endif

            if ((distanceToTarget - AttackRange1) < DELTA ||
                (distanceToTarget - AttackRange2) < DELTA ||
                (distanceToTarget - AttackRange3) < DELTA)
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

        public void Move(float x, float z)
        {
#if DEBUG
            _logger.LogDebug("AI {hashcode} is moving to target: x - {x}, z - {z}, target x - {targetX}, target z - {targetZ}", GetHashCode(), _movementManager.PosX, _movementManager.PosZ, x, z);
#endif

            if (MathExtensions.Distance(_movementManager.PosX, x, _movementManager.PosZ, z) < DELTA)
                return;

            if (_chaseSpeed == 0 || _chaseTime == 0)
                return;

            if (_speedManager.Immobilize)
                return;

            var speed = State == AIState.Idle ? _idleSpeed : _chaseSpeed * 1.0;
            if (MathExtensions.Distance(_movementManager.PosX, x, _movementManager.PosZ, z) < 5)
                speed = 2;

            var time = State == AIState.Idle ? _idleTime : _chaseTime;

            var now = DateTime.UtcNow;
            var mobVector = new Vector2(_movementManager.PosX, _movementManager.PosZ);
            var destinationVector = new Vector2(x, z);

            var normalizedVector = Vector2.Normalize(destinationVector - mobVector);
            var deltaTime = now.Subtract(_lastMoveUpdate);
            var deltaMilliseconds = deltaTime.TotalMilliseconds > 2000 ? 500 : deltaTime.TotalMilliseconds;
            var temp = normalizedVector * (float)(speed / (time) * deltaMilliseconds);
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

        private float _startX = -1;
        /// <summary>
        /// Position x, where mob started chasing.
        /// </summary>
        private float StartPosX
        {
            get => _startX;
            set
            {
                if (value != -1 && _startX == -1)
                    _startX = value;

                if (value == -1 && _startX != -1)
                    _startX = value;
            }
        }

        private float _startZ = -1;
        /// <summary>
        /// Position z, where mob started chasing.
        /// </summary>
        private float StartPosZ
        {
            get => _startZ;
            set
            {
                if (value != -1 && _startZ == -1)
                    _startZ = value;

                if (value == -1 && _startZ != -1)
                    _startZ = value;
            }
        }

        /// <summary>
        /// Is mob too far away from its' area?
        /// </summary>
        private bool IsTooFarAway
        {
            get
            {
                var distance = MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ);
#if DEBUG
                _logger.LogDebug("AI {hashcode} distance to start chasing location is {distance}.", GetHashCode(), distance);
#endif
                return distance > 45;
            }
        }

        /// <summary>
        /// Returns mob back to birth position.
        /// </summary>
        private void ReturnToBirthPosition()
        {
            if (MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ) - 1 > DELTA)
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
            if (MathExtensions.Distance(_movementManager.PosX, StartPosX, _movementManager.PosZ, StartPosZ) - 1 > DELTA)
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
            var target = _attackManager.Target;
            if (target is null)
            {
                _attackTimer.Interval = 1000;
                _attackTimer.Start();
                return;
            }

            var distanceToTarget = MathExtensions.Distance(_movementManager.PosX, _attackManager.Target.MovementManager.PosX, _movementManager.PosZ, _attackManager.Target.MovementManager.PosZ);
            var now = DateTime.UtcNow;
            int delay = 1000;
            var attackId = RandomiseAttack(now);
            var useAttack1 = attackId == 1;
            var useAttack2 = attackId == 2;
            var useAttack3 = attackId == 3;

            if (useAttack1 && ((distanceToTarget - AttackRange1) < DELTA || AttackRange1 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 1.", GetHashCode());
#endif
                Attack(AttackType1, AttackAttrib1, Attack1, AttackPlus1, AttackRange1);
                _lastAttack1Time = now;
                delay = AttackTime1 > 0 ? AttackTime1 : 5000;
            }

            if (useAttack2 && ((distanceToTarget - AttackRange2) < DELTA || AttackRange2 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 2.", GetHashCode());
#endif
                Attack(AttackType2, AttackAttrib2, Attack2, AttackPlus2, AttackRange2);
                _lastAttack2Time = now;
                delay = AttackTime2 > 0 ? AttackTime2 : 5000;
            }

            if (useAttack3 && ((distanceToTarget - AttackRange3) < DELTA || AttackRange3 == 0))
            {
#if DEBUG
                _logger.LogDebug("AI {hashcode} used attack 3.", GetHashCode());
#endif
                Attack(AttackType3, Element.None, Attack3, AttackPlus3, AttackRange3);
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
        public void Attack(ushort skillId, Element element, ushort minAttack, ushort additionalDamage, int attackRange)
        {
            var isMeleeAttack = false;
            Skill skill = null;
            if (skillId == 0) // Usual melee attack.
            {
                isMeleeAttack = true;
            }
            else
            {
                if (_definitionsPreloder.Skills.TryGetValue((skillId, 100), out var dbSkill))
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
                _attackManager.WeaponAttackRange = (ushort)attackRange;
                _elementProvider.AttackSkillElement = _elementProvider.ConstAttackElement;

                if (_attackManager.CanAttack(IAttackManager.AUTO_ATTACK_NUMBER, _attackManager.Target, out var _))
                    _attackManager.AutoAttack(Owner);
            }
            else
            {
                try
                {
                    _elementProvider.AttackSkillElement = element;

                    var canUseSkill = _skillsManager.CanUseSkill(skill, skill.Type == TypeDetail.Healing ? Owner as IKillable : _attackManager.Target, out var reason);
                    if (canUseSkill)
                        _skillsManager.UseSkill(skill, Owner, skill.Type == TypeDetail.Healing ? Owner as IKillable : _attackManager.Target);
                    else if (reason == AttackSuccess.CooldownNotOver)
                        if (_attackManager.CanAttack(IAttackManager.AUTO_ATTACK_NUMBER, _attackManager.Target, out var _))
                            _attackManager.AutoAttack(Owner);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to use skill, reason: {message}. Fallback to melee attack.", ex.Message);

                    _statsManager.WeaponMinAttack = minAttack;
                    _statsManager.WeaponMaxAttack = minAttack + additionalDamage;
                    _attackManager.WeaponAttackRange = (ushort)attackRange;

                    if (_attackManager.CanAttack(IAttackManager.AUTO_ATTACK_NUMBER, _attackManager.Target, out var _))
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

        #region Target selection

        /// <summary>
        /// Mob's agro, where key is id of player and value is agro points.
        /// </summary>
        public ConcurrentDictionary<uint, int> Agro { get; init; } = new();

        /// <summary>
        /// Selects target based on agro points.
        /// </summary>
        public IKillable SelectAgroTarget()
        {
            if (Agro.Count == 0)
                return null;

            KeyValuePair<uint, int> maxAgro = new();
            foreach (var x in Agro)
            {
                if (maxAgro.Key == 0)
                    maxAgro = x;

                if (x.Value > maxAgro.Value)
                    maxAgro = x;
            }

            return _mapProvider.Map.GetPlayer(maxAgro.Key);
        }

        /// <summary>
        /// When user hits mob, it automatically turns on ai.
        /// </summary>
        private void OnDecreaseHP(uint senderId, IKiller damageMaker, int damage)
        {
            if (!_healthManager.IsDead)
            {
                if (damageMaker is Character character && character.HealthManager.IsAttackable)
                {
                    if (!Agro.ContainsKey(character.Id))
                        Agro.TryAdd(character.Id, 0);

                    Agro[character.Id] += damage / 2 + character.StatsManager.TotalRec * 2;
                    _attackManager.Target = SelectAgroTarget();
                }
            }
        }

        private void OnBuffAdded(uint senderId, Buff buff)
        {
            var character = buff.BuffCreator as Character;
            if (character is null)
            {
                _logger.LogError("Mob can not get buffs from not player!");
                return;
            }

            if (buff.IsDebuff)
            {
                if (!Agro.ContainsKey(character.Id))
                    Agro.TryAdd(character.Id, 0);

                Agro[character.Id] += character.StatsManager.TotalRec * 2;
                _attackManager.Target = SelectAgroTarget();
            }

            if (buff.Skill.Type == TypeDetail.Provoke)
            {
                _attackManager.Target = character;
            }
        }

        private void OnBuffRemoved(uint senderId, Buff buff)
        {
            if (buff.Skill.Type == TypeDetail.Provoke)
            {
                _attackManager.Target = SelectAgroTarget();
            }
        }

        #endregion
    }
}
