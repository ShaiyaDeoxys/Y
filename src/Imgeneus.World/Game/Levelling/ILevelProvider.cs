using System;

namespace Imgeneus.World.Game.Levelling
{
    public interface ILevelProvider
    {
        void Init(int ownerId, ushort level);

        /// <summary>
        /// Current level.
        /// </summary>
        ushort Level { get; set; }

        /// <summary>
        /// Event that's fired when a player level's up
        /// </summary>
        event Action<int, ushort, ushort> OnLevelUp;
    }
}
