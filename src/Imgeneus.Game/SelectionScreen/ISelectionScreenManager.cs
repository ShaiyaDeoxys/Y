using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.SelectionScreen
{
    /// <summary>
    /// Manager, that handles selection screen packets.
    /// </summary>
    public interface ISelectionScreenManager
    {
        /// <summary>
        /// Lists all player's characters.
        /// </summary>
        Task<IEnumerable<DbCharacter>> GetCharacters(int userId);

        /// <summary>
        /// Get faction of user.
        /// </summary>
        Task<Fraction> GetFaction(int userId);

        /// <summary>
        /// Sets faction to user.
        /// </summary>
        Task SetFaction(int userId, Fraction fraction);

        /// <summary>
        /// Get mode of user.
        /// </summary>
        Task<Mode> GetMaxMode(int userId);

        /// <summary>
        /// Handles creation of character.
        /// </summary>
        /// <returns>true if character is created</returns>
        Task<bool> TryCreateCharacter(int userId, CreateCharacterPacket createCharacterPacket);

        /// <summary>
        /// Tries to soft delete character (i.e. sets IsDeleted & DeleteDate). There is no hard delete.
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="id">character id</param>
        /// <returns>true if deleted, otherwise false</returns>
        Task<bool> TryDeleteCharacter(int userId, uint id);

        /// <summary>
        /// Tries to restore character (i.e. sets IsDeleted & DeleteDate).
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="id">character id</param>
        /// <returns>true if restored, otherwise false</returns>
        Task<bool> TryRestoreCharacter(int userId, uint id);

        /// <summary>
        /// Tries to rename character.
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="id">character id</param>
        /// <param name="newName">new name</param>
        /// <returns>true if renamed, otherwise false</returns>
        Task<bool> TryRenameCharacter(int userId, uint id, string newName);
    }
}
