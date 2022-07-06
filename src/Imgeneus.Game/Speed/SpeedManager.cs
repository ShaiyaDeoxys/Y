using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Stealth;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Speed
{
    public class SpeedManager : ISpeedManager
    {
        private readonly ILogger<SpeedManager> _logger;
        private readonly IStealthManager _stealthManager;
        protected uint _ownerId;

        public SpeedManager(ILogger<SpeedManager> logger, IStealthManager stealthManager)
        {
            _logger = logger;
            _stealthManager = stealthManager;

            _stealthManager.OnStealthChange += StealthManager_OnStealthChange;
#if DEBUG
            _logger.LogDebug("SpeedManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SpeedManager()
        {
            _logger.LogDebug("SpeedManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _stealthManager.OnStealthChange -= StealthManager_OnStealthChange;
        }

        #endregion

        #region Attack speed

        private bool _isAbleToPhysicalAttack = true;
        public bool IsAbleToPhysicalAttack { get => _isAbleToPhysicalAttack; set { _isAbleToPhysicalAttack = value; RaiseMoveAndAttackSpeed(); } }

        private bool _isAbleToMagicAttack = true;
        public bool IsAbleToMagicAttack { get => _isAbleToMagicAttack; set { _isAbleToMagicAttack = value; RaiseMoveAndAttackSpeed(); } }

        public Dictionary<byte, byte> WeaponSpeedPassiveSkillModificator { get; init; } = new Dictionary<byte, byte>();

        private int _constAttackSpeed = 0;
        public int ConstAttackSpeed { get => _constAttackSpeed; set { _constAttackSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private int _extraAttackSpeed;
        public int ExtraAttackSpeed
        {
            get => _extraAttackSpeed;
            set
            {
                if (_extraAttackSpeed == value)
                    return;

                _extraAttackSpeed = value;
                RaiseMoveAndAttackSpeed();
            }
        }

        public AttackSpeed TotalAttackSpeed
        {
            get
            {
                if (!IsAbleToPhysicalAttack)
                    return AttackSpeed.CanNotAttack;

                if (ConstAttackSpeed == 0)
                    return AttackSpeed.None;

                var finalSpeed = ConstAttackSpeed + ExtraAttackSpeed;

                if (finalSpeed < 0)
                    return AttackSpeed.ExteremelySlow;

                if (finalSpeed > 9)
                    return AttackSpeed.ExteremelyFast;

                return (AttackSpeed)finalSpeed;
            }
        }

        #endregion

        #region Move speed

        private int _constMoveSpeed = 2; // 2 == normal by default.
        public int ConstMoveSpeed { get => _constMoveSpeed; set { _constMoveSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private int _extraMoveSpeed;
        public int ExtraMoveSpeed
        {
            get => _extraMoveSpeed;
            set
            {
                if (_extraMoveSpeed == value)
                    return;

                _extraMoveSpeed = value;
                RaiseMoveAndAttackSpeed();
            }
        }

        private bool _immobilize;
        public bool Immobilize { get => _immobilize; set { _immobilize = value; RaiseMoveAndAttackSpeed(); } }

        public MoveSpeed TotalMoveSpeed
        {
            get
            {
                if (Immobilize)
                    return MoveSpeed.CanNotMove;

                if (_stealthManager.IsAdminStealth)
                    return MoveSpeed.VeryFast;

                var finalSpeed = ConstMoveSpeed + ExtraMoveSpeed;

                if (finalSpeed < 0)
                    return MoveSpeed.VerySlow;

                if (finalSpeed > 4)
                    return MoveSpeed.VeryFast;

                return (MoveSpeed)finalSpeed;
            }
        }

        #endregion

        #region Evenets

        public event Action<uint, AttackSpeed, MoveSpeed> OnAttackOrMoveChanged;
        public event Action<byte, byte, bool> OnPassiveModificatorChanged;

        public void RaiseMoveAndAttackSpeed()
        {
            OnAttackOrMoveChanged?.Invoke(_ownerId, TotalAttackSpeed, TotalMoveSpeed);
        }

        public void RaisePassiveModificatorChanged(byte weaponType, byte passiveSkillModifier, bool shouldAdd)
        {
            OnPassiveModificatorChanged?.Invoke(weaponType, passiveSkillModifier, shouldAdd);
        }

        private void StealthManager_OnStealthChange(uint senderId)
        {
            RaiseMoveAndAttackSpeed();
        }

        #endregion
    }
}
