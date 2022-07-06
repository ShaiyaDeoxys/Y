using Imgeneus.World.Game.Player;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Speed
{
    public interface ISpeedManager : IDisposable
    {
        /// <summary>
        /// Inits speed manager.
        /// </summary>
        void Init(uint ownerId);

        #region Attack

        /// <summary>
        /// Is it possible to make physical attack.
        /// </summary>
        bool IsAbleToPhysicalAttack { get; set; }

        /// <summary>
        /// Is it possible to make magic attack.
        /// </summary>
        bool IsAbleToMagicAttack { get; set; }

        /// <summary>
        /// Attack speed.
        /// </summary>
        AttackSpeed TotalAttackSpeed { get; }

        /// <summary>
        /// Attack speed from buffs.
        /// </summary>
        int ExtraAttackSpeed { get; set; }

        /// <summary>
        /// For player attack speed of weapon. For mob it's always the same - 5(normal).
        /// </summary>
        int ConstAttackSpeed { get; set; }

        /// <summary>
        /// Weapon speed calculated from passive skill. Key is weapon, value is speed modificator.
        /// </summary>
        public Dictionary<byte, byte> WeaponSpeedPassiveSkillModificator { get; }

        #endregion

        #region Move

        /// <summary>
        /// Move speed.
        /// </summary>
        MoveSpeed TotalMoveSpeed { get; }

        /// <summary>
        /// Move speed from buffs.
        /// </summary>
        int ExtraMoveSpeed { get; set; }

        /// <summary>
        /// Usually 2 which is normal.
        /// </summary>
        int ConstMoveSpeed { get; set; }

        /// <summary>
        /// Can not move?
        /// </summary>
        bool Immobilize { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Event, that is fired, when attack or move speed changes.
        /// </summary>
        event Action<uint, AttackSpeed, MoveSpeed> OnAttackOrMoveChanged;

        /// <summary>
        /// Raises <see cref="OnAttackOrMoveChanged"/> event.
        /// </summary>
        void RaiseMoveAndAttackSpeed();

        /// <summary>
        /// Event, that is fired when weapom passive skill is learned/reset.
        /// </summary>
        event Action<byte, byte, bool> OnPassiveModificatorChanged;

        /// <summary>
        /// Raises <see cref="OnPassiveModificatorChanged"/> event.
        /// </summary>
        void RaisePassiveModificatorChanged(byte weaponType, byte passiveSkillModifier, bool shouldAdd);

        #endregion
    }
}
