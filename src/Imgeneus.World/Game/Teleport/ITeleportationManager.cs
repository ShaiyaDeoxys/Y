using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone.Portals;
using System;

namespace Imgeneus.World.Game.Teleport
{
    public interface ITeleportationManager : ISessionedService, IDisposable
    {
        void Init(int ownerId);

        /// <summary>
        /// Indicator if character is teleporting between maps.
        /// </summary>
        bool IsTeleporting { get; set; }

        /// <summary>
        /// Fires event, that teleports player.
        /// </summary>
        event Action<int, ushort, float, float, float, bool> OnTeleporting;

        /// <summary>
        /// Teleports character inside one map or to another map.
        /// </summary>
        /// <param name="mapId">map id, where to teleport</param>
        /// <param name="X">x coordinate, where to teleport</param>
        /// <param name="Y">y coordinate, where to teleport</param>
        /// <param name="Z">z coordinate, where to teleport</param>
        /// <param name="teleportedByAdmin">Indicates whether the teleport was issued by an admin or not</param>
        void Teleport(ushort mapId, float x, float y, float z, bool teleportedByAdmin = false);

        /// <summary>
        /// Teleports character with the help of the portal, if it's possible.
        /// </summary>
        bool TryTeleport(byte portalIndex, out PortalTeleportNotAllowedReason reason);

        /// <summary>
        /// Fires event, when player starts casting teleport.
        /// </summary>
        event Action<int> OnCastingTeleport;

        /// <summary>
        /// When used teleport item, like teleport to capital, arena etc. there is casting timer.
        /// </summary>
        void StartCastingTeleport(ushort mapId, float x, float y, float z, bool skeepTimer = false);
    }
}
