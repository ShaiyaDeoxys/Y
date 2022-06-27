using System;

namespace Imgeneus.Game.Skills
{
    public interface ICastProtectionManager : IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Casting can not be interrupted by debuff or damage.
        /// </summary>
        bool IsCastProtected { get; }

        /// <summary>
        /// Protects party members from cast interrupting.
        /// </summary>
        bool ProtectAlliesCasting { get; set; }

        /// <summary>
        /// Range for <see cref="ProtectAlliesCasting"/>.
        /// </summary>
        byte ProtectCastingRange { get; set; }

        /// <summary>
        /// Skill info, that will protect casting.
        /// </summary>
        (ushort SkillId, byte SkillLevel) ProtectCastingSkill { get; set; }

        /// <summary>
        /// Reduces casting time.
        /// </summary>
        bool ReduceCastingTime { get; set; }
    }
}
