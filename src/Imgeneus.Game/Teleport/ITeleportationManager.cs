using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone.Portals;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Teleport
{
    public interface ITeleportationManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId, IEnumerable<DbCharacterSavePositions> savedPositions);

        /// <summary>
        /// Indicator if character is teleporting between maps.
        /// </summary>
        bool IsTeleporting { get; set; }

        /// <summary>
        /// Fires event, that teleports player.
        /// </summary>
        event Action<uint, ushort, float, float, float, bool, bool> OnTeleporting;

        /// <summary>
        /// Teleports character inside one map or to another map.
        /// </summary>
        /// <param name="mapId">map id, where to teleport</param>
        /// <param name="X">x coordinate, where to teleport</param>
        /// <param name="Y">y coordinate, where to teleport</param>
        /// <param name="Z">z coordinate, where to teleport</param>
        /// <param name="teleportedByAdmin">Indicates whether the teleport was issued by an admin or not</param>
        void Teleport(ushort mapId, float x, float y, float z, bool teleportedByAdmin = false, bool summonedByAdmin = false);

        /// <summary>
        /// Teleports character with the help of the portal, if it's possible.
        /// </summary>
        bool TryTeleport(byte portalIndex, out PortalTeleportNotAllowedReason reason);

        /// <summary>
        /// Fires event, when player starts casting teleport.
        /// </summary>
        event Action<uint> OnCastingTeleport;

        /// <summary>
        /// Fires event, when player finished casting teleport.
        /// </summary>
        event Action OnCastingTeleportFinished;

        /// <summary>
        /// Where player is going to teleport.
        /// </summary>
        (ushort MapId, float X, float Y, float Z) CastingPosition { get; }

        /// <summary>
        /// When used teleport item, like teleport to capital, arena etc. there is casting timer.
        /// </summary>
        void StartCastingTeleport(ushort mapId, float x, float y, float z, Item item, bool skeepTimer = false);

        /// <summary>
        /// Item, that is currently in cast.
        /// </summary>
        Item CastingItem { get; }

        /// <summary>
        /// Maximum number of saved places. Items like "Blue Dragon Charm" can change this value.
        /// </summary>
        byte MaxSavedPoints { get; set; }

        /// <summary>
        /// Tries to save position.
        /// </summary>
        bool TrySavePosition(byte index, ushort mapId, float x, float y, float z);

        /// <summary>
        /// Character's saved positions.
        /// </summary>
        IReadOnlyDictionary<byte, (ushort MapId, float X, float Y, float Z)> SavedPositions { get; }

        /// <summary>
        /// Teleport within 10 sec to the nearest town.
        /// </summary>
        void StartTownTeleport();
    }
}
