using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stealth;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Vehicle
{
    public class VehicleManager : IVehicleManager
    {
        private readonly ILogger<VehicleManager> _logger;
        private readonly IStealthManager _stealthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IHealthManager _healthManager;
        private readonly IGameWorld _gameWorld;
        private uint _ownerId;

        public VehicleManager(ILogger<VehicleManager> logger, IStealthManager stealthManager, ISpeedManager speedManager, IHealthManager healthManager, IGameWorld gameWorld)
        {
            _logger = logger;
            _stealthManager = stealthManager;
            _speedManager = speedManager;
            _healthManager = healthManager;
            _gameWorld = gameWorld;

            _healthManager.OnGotDamage += HealthManager_OnGotDamage;
            _summonVehicleTimer.Elapsed += SummonVehicleTimer_Elapsed;
#if DEBUG
            _logger.LogDebug("VehicleManager {hashcode} created", GetHashCode());
#endif
        }


#if DEBUG
        ~VehicleManager()
        {
            _logger.LogDebug("VehicleManager {hashcode} collected by GC", GetHashCode());
        }
#endif
        #region Init & Clear

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _healthManager.OnGotDamage -= HealthManager_OnGotDamage;
            _summonVehicleTimer.Elapsed -= SummonVehicleTimer_Elapsed;
        }

        #endregion

        #region Summmoning

        public int SummoningTime { get; set; }

        public event Action<uint> OnStartSummonVehicle;

        private bool _isSummmoningVehicle;

        private readonly Timer _summonVehicleTimer = new Timer()
        {
            AutoReset = false
        };

        /// <summary>
        /// Is player currently summoning vehicle?
        /// </summary>
        public bool IsSummmoningVehicle
        {
            get => _isSummmoningVehicle;
            private set
            {
                _isSummmoningVehicle = value;
                if (_isSummmoningVehicle)
                {
                    _summonVehicleTimer.Interval = SummoningTime;
                    _summonVehicleTimer.Start();
                    OnStartSummonVehicle?.Invoke(_ownerId);
                }
                else
                {
                    _summonVehicleTimer.Stop();
                }
            }
        }

        public void CancelVehicleSummon()
        {
            IsSummmoningVehicle = false;
        }

        private void SummonVehicleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsOnVehicle = true;
            OnUsedVehicle?.Invoke(true, IsOnVehicle);
        }

        #endregion

        #region Vehicle

        public Item Mount { get; set; }

        public event Action<uint, bool> OnVehicleChange;

        public event Action<bool, bool> OnUsedVehicle;

        private bool _isOnVehicle;
        public bool IsOnVehicle
        {
            get => _isOnVehicle || Vehicle2CharacterID != 0;
            private set
            {
                if (_isOnVehicle == value)
                    return;

                _isOnVehicle = value;

                OnVehicleChange?.Invoke(_ownerId, _isOnVehicle);
                _speedManager.ConstMoveSpeed = _isOnVehicle ? 4 : 2;
            }
        }

        public bool CallVehicle(bool skipSummoning = false)
        {
            if (_stealthManager.IsStealth)
                return false;

            if (skipSummoning)
                IsOnVehicle = true;
            else
                IsSummmoningVehicle = true;

            return true;
        }

        public bool RemoveVehicle()
        {
            IsOnVehicle = false;
            Vehicle2CharacterID = 0;
            return true;
        }

        private void HealthManager_OnGotDamage(uint senderId, IKiller damageMaker, int damage)
        {
            RemoveVehicle();
        }

        #endregion

        #region Passenger

        public event Action<uint, uint> OnVehiclePassengerChanged;

        private uint _vehicle2CharacterID;

        private Character _vehicleOwner;

        public uint Vehicle2CharacterID
        {
            get => _vehicle2CharacterID;
            set
            {
                if (_vehicle2CharacterID == value)
                    return;

                _vehicle2CharacterID = value;
                OnVehiclePassengerChanged?.Invoke(_ownerId, _vehicle2CharacterID);

                if (_vehicle2CharacterID == 0)
                {
                    _vehicleOwner.ShapeManager.OnShapeChange -= VehicleOwner_OnShapeChange;
                    _vehicleOwner = null;
                }
                else
                {
                    _vehicleOwner = _gameWorld.Players[_vehicle2CharacterID];
                    _vehicleOwner.ShapeManager.OnShapeChange += VehicleOwner_OnShapeChange;
                }
            }
        }

        private void VehicleOwner_OnShapeChange(uint senderId, ShapeEnum shape, uint param1, uint param2)
        {
            var sender = _gameWorld.Players[senderId];
            if (!sender.VehicleManager.IsOnVehicle)
            {
                Vehicle2CharacterID = 0;
            }
        }

        public uint VehicleRequesterID { get; set; }

        #endregion
    }
}
