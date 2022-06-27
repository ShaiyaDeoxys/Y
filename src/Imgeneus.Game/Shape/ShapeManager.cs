using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Vehicle;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Shape
{
    public class ShapeManager : IShapeManager
    {
        private readonly ILogger<ShapeManager> _logger;
        private readonly IStealthManager _stealthManager;
        private readonly IVehicleManager _vehicleManager;
        private uint _ownerId;

        public ShapeManager(ILogger<ShapeManager> logger, IStealthManager stealthManager, IVehicleManager vehicleManager)
        {
            _logger = logger;
            _stealthManager = stealthManager;
            _vehicleManager = vehicleManager;

            _stealthManager.OnStealthChange += StealthManager_OnStealthChange;
            _vehicleManager.OnVehicleChange += VehicleManager_OnVehicleChange;

#if DEBUG
            _logger.LogDebug("ShapeManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~ShapeManager()
        {
            _logger.LogDebug("ShapeManager {hashcode} collected by GC", GetHashCode());
        }
#endif
        #region Init & Clear

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _stealthManager.OnStealthChange -= StealthManager_OnStealthChange;
            _vehicleManager.OnVehicleChange -= VehicleManager_OnVehicleChange;
        }

        #endregion

        public event Action<uint, ShapeEnum, uint, uint> OnShapeChange;

        public ShapeEnum Shape
        {
            get
            {
                if (_stealthManager.IsStealth)
                    return ShapeEnum.Stealth;

                if (MonsterLevel > 0)
                    return MonsterLevel;

                if (IsOppositeCountry)
                    if (CharacterId != 0)
                        return ShapeEnum.OppositeCountryCharacter;
                    else
                        return ShapeEnum.OppositeCountry;

                if (_vehicleManager.IsOnVehicle)
                {
                    var value1 = (byte)_vehicleManager.Mount.Grow >= 2 ? 15 : 14;
                    var value2 = _vehicleManager.Mount.Range < 2 ? _vehicleManager.Mount.Range * 2 : _vehicleManager.Mount.Range + 7;
                    var mountType = value1 + value2;
                    return (ShapeEnum)mountType;
                }

                return ShapeEnum.None;
            }
        }

        private void StealthManager_OnStealthChange(uint senderId)
        {
            OnShapeChange?.Invoke(_ownerId, Shape, 0, 0);
        }

        private void VehicleManager_OnVehicleChange(uint senderId, bool isOnVehicle)
        {
            var param1 = _vehicleManager.Mount is null ? 0 : _vehicleManager.Mount.Type;
            var param2 = _vehicleManager.Mount is null ? 0 : _vehicleManager.Mount.TypeId;
            OnShapeChange?.Invoke(_ownerId, Shape, (uint)param1, (uint)param2);
        }

        private bool _isTranformated;
        public bool IsTranformated
        {
            get => _isTranformated; set
            {
                _isTranformated = value;
                OnTranformated?.Invoke(_ownerId, _isTranformated);
            }
        }

        private ShapeEnum _monsterLevel;
        public ShapeEnum MonsterLevel
        {
            get => _monsterLevel;
            set
            {
                _monsterLevel = value;
                OnShapeChange?.Invoke(_ownerId, Shape, MobId, 0);
            }
        }

        public ushort MobId { get; set; }

        public event Action<uint, bool> OnTranformated;


        private bool _isOppositeCountry;
        public bool IsOppositeCountry
        {
            get => _isOppositeCountry;
            set
            {
                _isOppositeCountry = value;
                OnShapeChange?.Invoke(_ownerId, Shape, CharacterId, 0);
            }
        }

        public uint CharacterId { get; set; }
    }
}
