using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Portals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// The virtual representation of game world.
    /// </summary>
    public interface IGameWorld
    {
        /// <summary>
        /// Starts game world.
        /// </summary>
        void Init();

        /// <summary>
        /// Connected players. Key is character id, value is character.
        /// </summary>
        ConcurrentDictionary<uint, Character> Players { get; }

        /// <summary>
        /// Loaded maps. Key is map id, value is map.
        /// </summary>
        ConcurrentDictionary<ushort, IMap> Maps { get; }

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is party id.
        /// </summary>
        ConcurrentDictionary<Guid, IPartyMap> PartyMaps { get; }

        /// <summary>
        /// Collection of map ids, that are available for GM teleport.
        /// </summary>
        IList<ushort> AvailableMapIds { get; }

        /// <summary>
        /// Ensures, that character can be loaded to map, that we got from db.
        /// NB! Mutates dbCharacter, if he can not be loaded to map for some reason!
        /// Reason can be next: map was deleted from the server, map was instance map, something went wrong and we saved wrong map id in database.
        /// </summary>
        /// <param name="dbCharacter"></param>
        void EnsureMap(DbCharacter dbCharacter, Fraction faction);

        /// <summary>
        /// Loads player into game world.
        /// </summary>
        /// <param name="newPlayer">player, that should be loaded into game world</param>
        /// <returns>true if loaded, otherwise false</returns>
        bool TryLoadPlayer(Character newPlayer);

        /// <summary>
        /// Loads player into map and send notification other players.
        /// </summary>
        void LoadPlayerInMap(uint characterId);

        /// <summary>
        /// Removes player from game world.
        /// </summary>
        void RemovePlayer(uint characterId);

        /// <summary>
        /// Checks, if player can be teleported to map.
        /// </summary>
        /// <param name="player">player to teleport</param>
        /// <param name="mapId">map index</param>
        /// <param name="reason">optional out param, that indicates the reason why teleport is not allowed for this character</param>
        /// <param name="skipLevelCheck">skips level check of map</param>
        /// <returns>true, if it can teleport</returns>
        bool CanTeleport(Character player, ushort mapId, out PortalTeleportNotAllowedReason reason, bool skipLevelCheck = false);
    }
}
