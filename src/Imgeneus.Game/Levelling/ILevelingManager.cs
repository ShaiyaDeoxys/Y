using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Levelling
{
    public interface ILevelingManager : ISessionedService
    {
        /// <summary>
        /// Inits leveling manager.
        /// </summary>
        /// <param name="owner">character id</param>
        /// <param name="exp">exp stored in databse</param>
        void Init(uint owner, uint exp);

        /// <summary>
        /// Current experience amount.
        /// </summary>
        uint Exp { get; }

        /// <summary>
        /// Minimum experience needed for current player's level
        /// </summary>
        uint MinLevelExp { get; }

        /// <summary>
        /// Experience needed to level up to next level
        /// </summary>
        uint NextLevelExp { get; }

        /// <summary>
        /// Event, that is fired, when exp changes.
        /// </summary>
        event Action<uint> OnExpChanged;

        /// <summary>
        /// Attempts to set a new level for a character and handles the levelling logic (exp, stat points, skill points, etc)
        /// </summary>
        /// <param name="newLevel">New player level</param>
        /// <returns>Success status indicating whether it's possible to set the new level or not.</returns>
        bool TryChangeLevel(ushort newLevel);

        /// <summary>
        /// Attempts to set the experience of a player and updates the player's level if necessary.
        /// </summary>
        /// <param name="exp">New player experience</param>
        /// <returns>Success status indicating whether it's possible to set the new level or not.</returns>
        bool TryChangeExperience(uint exp);

        /// <summary>
        /// Gives a player the experience gained by killing a mob
        /// </summary>
        /// <param name="mobLevel">Killed mob's level</param>
        /// <param name="mobExp">Killed mob's experience</param>
        void AddMobExperience(ushort mobLevel, ushort mobExp);

        /// <summary>
        /// Exp multiplier.
        /// </summary>
        uint ExpGainRate { get; set; }
    }
}
