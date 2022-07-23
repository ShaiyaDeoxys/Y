using System;

namespace Imgeneus.Game.Recover
{
    public interface IRecoverManager : IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Starts regeneration timer.
        /// </summary>
        void Start();

        /// <summary>
        /// HP regeneration % from possive skills.
        /// </summary>
        ushort ExtraHPRegeneration { get; set; }

        /// <summary>
        /// MP regeneration % from possive skills.
        /// </summary>
        ushort ExtraMPRegeneration { get; set; }

        /// <summary>
        /// SP regeneration % from possive skills.
        /// </summary>
        ushort ExtraSPRegeneration { get; set; }
    }
}
