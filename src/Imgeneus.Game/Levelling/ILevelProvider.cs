using System;

namespace Imgeneus.World.Game.Levelling
{
    public interface ILevelProvider
    {
        void Init(uint ownerId, ushort level);

        /// <summary>
        /// Current level.
        /// </summary>
        ushort Level { get; set; }

        /// <summary>
        /// Event that's fired when a player level's up
        /// </summary>
        event Action<uint, ushort, ushort> OnLevelUp;
    }
}
