using Imgeneus.World.Game.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Speed
{
    public class SpeedManager : ISpeedManager
    {
        private readonly ILogger<SpeedManager> _logger;

        protected int _ownerId;

        public SpeedManager(ILogger<SpeedManager> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("BaseSpeedManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SpeedManager()
        {
            _logger.LogDebug("BaseSpeedManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        #region Attack speed

        public Dictionary<byte, byte> WeaponSpeedPassiveSkillModificator { get; init; } = new Dictionary<byte, byte>();

        private int _constAttackSpeed = 5; // 5 == normal by default.
        public int ConstAttackSpeed { get => _constAttackSpeed; set { _constAttackSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private int _extraAttackSpeed;
        public int ExtraAttackSpeed { get => _extraAttackSpeed; set { _extraAttackSpeed = value; RaiseMoveAndAttackSpeed(); } }

        public AttackSpeed TotalAttackSpeed
        {
            get
            {

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
        public int ExtraMoveSpeed { get => _extraMoveSpeed; set { _extraMoveSpeed = value; RaiseMoveAndAttackSpeed(); } }

        public MoveSpeed TotalMoveSpeed
        {
            get
            {
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

        public event Action<int, AttackSpeed, MoveSpeed> OnAttackOrMoveChanged;
        public event Action<byte, byte, bool> OnPassiveModificatorChanged;

        public void RaiseMoveAndAttackSpeed()
        {
            OnAttackOrMoveChanged?.Invoke(_ownerId, TotalAttackSpeed, TotalMoveSpeed);
        }

        public void RaisePassiveModificatorChanged(byte weaponType, byte passiveSkillModifier, bool shouldAdd)
        {
            OnPassiveModificatorChanged?.Invoke(weaponType, passiveSkillModifier, shouldAdd);
        }

        #endregion

        /*
        * 
       /// <summary>
       /// How fast character can make new hit.
       /// </summary>
       public override AttackSpeed AttackSpeed
       {
           get
           {
               if (_weaponSpeed == 0)
                   return AttackSpeed.None;

               var weaponType = InventoryManager.Weapon.ToPassiveSkillType();
               _weaponSpeedPassiveSkillModificator.TryGetValue(weaponType, out var passiveSkillModifier);

               var finalSpeed = _weaponSpeed + _attackSpeedModifier + passiveSkillModifier;

               if (finalSpeed < 0)
                   return AttackSpeed.ExteremelySlow;

               if (finalSpeed > 9)
                   return AttackSpeed.ExteremelyFast;

               return (AttackSpeed)finalSpeed;
           }
       }
        */
    }
}
