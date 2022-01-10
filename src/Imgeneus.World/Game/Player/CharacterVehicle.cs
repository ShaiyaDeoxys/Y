using System;
using System.Timers;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
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
                    _summonVehicleTimer.Interval = InventoryManager.Mount.AttackSpeed > 0 ? InventoryManager.Mount.AttackSpeed * 1000 : InventoryManager.Mount.AttackSpeed + 1000;
                    _summonVehicleTimer.Start();
                    OnStartSummonVehicle?.Invoke(this);
                }
                else
                {
                    _summonVehicleTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Event, that is fired, when the player starts summoning mount.
        /// </summary>
        public event Action<Character> OnStartSummonVehicle;

        private bool _isOnVehicle;
        /// <summary>
        /// Indicator if character is on mount now.
        /// </summary>
        public bool IsOnVehicle
        {
            get => _isOnVehicle;
            private set
            {
                if (_isOnVehicle == value)
                    return;

                _isOnVehicle = value;

                OnShapeChange?.Invoke(this);
                InvokeAttackOrMoveChanged();
            }
        }

        /// <summary>
        /// Tries to summon vehicle(mount).
        /// </summary>
        /// <param name="skipSummoning">Indicates whether the summon casting time should be skipped or not.</param>
        public void CallVehicle(bool skipSummoning = false)
        {
            if (InventoryManager.Mount is null || IsStealth)
                return;

            if (skipSummoning)
                IsOnVehicle = true;
            else
                IsSummmoningVehicle = true;
        }

        /// <summary>
        /// Unmounts vehicle(mount).
        /// </summary>
        public void RemoveVehicle()
        {
            IsOnVehicle = false;
            Vehicle2CharacterID = 0;
        }

        /// <summary>
        /// Stops summon timer.
        /// </summary>
        public void CancelVehicleSummon()
        {
            IsSummmoningVehicle = false;
        }

        private void SummonVehicleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendUseVehicle(true, true);
            IsOnVehicle = true;
        }

        /// <summary>
        /// Event, that is fired, when 2 vehicle character changes.
        /// </summary>
        public Action<Character, int> OnVehiclePassengerChanged;

        private int _vehicle2CharacterID;

        private Character _vehicleOwner;

        /// <summary>
        /// Id of player, that is vehicle owner (2 places mount).
        /// </summary>
        public int Vehicle2CharacterID
        {
            get => _vehicle2CharacterID;
            set
            {
                if (_vehicle2CharacterID == value)
                    return;

                _vehicle2CharacterID = value;
                OnVehiclePassengerChanged?.Invoke(this, _vehicle2CharacterID);

                if (_vehicle2CharacterID == 0)
                {
                    _vehicleOwner.OnShapeChange -= VehicleOwner_OnShapeChange;
                    _vehicleOwner = null;
                }
                else
                {
                    _vehicleOwner = _gameWorld.Players[_vehicle2CharacterID];
                    _vehicleOwner.OnShapeChange += VehicleOwner_OnShapeChange;
                }
            }
        }

        private void VehicleOwner_OnShapeChange(Character sender)
        {
            if (!sender.IsOnVehicle)
            {
                Vehicle2CharacterID = 0;
                IsOnVehicle = false;
            }
        }

        /// <summary>
        /// Id of player, who has sent vehicle request.
        /// </summary>
        public int VehicleRequesterID { get; set; }
    }
}
