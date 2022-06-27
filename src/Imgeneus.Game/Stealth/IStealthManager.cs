using System;

namespace Imgeneus.World.Game.Stealth
{
    public interface IStealthManager
    {
        void Init(uint ownerId);

        /// <summary>
        /// Admin stealth, that is changed by /char on/off command.
        /// </summary>
        bool IsAdminStealth { get; set; }

        /// <summary>
        /// Is player visible or not.
        /// </summary>
        bool IsStealth { get; set; }

        /// <summary>
        /// Event, that is fired, when player goes into/out stealth.
        /// </summary>
        event Action<uint> OnStealthChange;
    }
}
