using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stealth;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace Imgeneus.World.Game.Vehicle
{
    public class VehicleManager : IVehicleManager
    {
        private readonly ILogger<VehicleManager> _logger;
        private readonly IStealthManager _stealthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IHealthManager _healthManager;
        private int _ownerId;

        public VehicleManager(ILogger<VehicleManager> logger, IStealthManager stealthManager, ISpeedManager speedManager, IHealthManager healthManager)
        {
            _logger = logger;
            _stealthManager = stealthManager;
            _speedManager = speedManager;
            _healthManager = healthManager;

            _healthManager.HP_Changed += HealthManager_HP_Changed;
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

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _healthManager.HP_Changed -= HealthManager_HP_Changed;
            _summonVehicleTimer.Elapsed -= SummonVehicleTimer_Elapsed;
        }

        #endregion

        #region Summmoning

        public int SummoningTime { get; set; }

        public event Action<int> OnStartSummonVehicle;

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
            OnUsedVehicle?.Invoke(true, true);
        }

        #endregion

        #region Vehicle

        public event Action<int, bool> OnVehicleChange;

        public event Action<bool, bool> OnUsedVehicle;

        private bool _isOnVehicle;
        public bool IsOnVehicle
        {
            get => _isOnVehicle;
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

        private void HealthManager_HP_Changed(int senderId, HitpointArgs args)
        {
            if (args.OldValue > args.NewValue)
                RemoveVehicle();
        }

        #endregion

        #region Passenger

        public event Action<int, int> OnVehiclePassengerChanged;

        private int _vehicle2CharacterID;

        private int _vehicleOwnerId;

        public int Vehicle2CharacterID
        {
            get => _vehicle2CharacterID;
            set
            {
                if (_vehicle2CharacterID == value)
                    return;

                _vehicle2CharacterID = value;
                /*OnVehiclePassengerChanged?.Invoke(this, _vehicle2CharacterID);

                if (_vehicle2CharacterID == 0)
                {
                    _vehicleOwner.OnShapeChange -= VehicleOwner_OnShapeChange;
                    _vehicleOwner = null;
                }
                else
                {
                    _vehicleOwner = _gameWorld.Players[_vehicle2CharacterID];
                    _vehicleOwner.OnShapeChange += VehicleOwner_OnShapeChange;
                }*/
            }
        }

        /*private void VehicleOwner_OnShapeChange(Character sender)
        {
            if (!sender.IsOnVehicle)
            {
                Vehicle2CharacterID = 0;
                IsOnVehicle = false;
            }
        }*/

        public int VehicleRequesterID { get; set; }

        #endregion
    }
}
