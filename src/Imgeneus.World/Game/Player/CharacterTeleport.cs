using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Portals;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Teleports character with the help of the portal, if it's possible.
        /// </summary>
        public bool TryTeleport(byte portalIndex, out PortalTeleportNotAllowedReason reason)
        {
            reason = PortalTeleportNotAllowedReason.Unknown;
            return false;
            /*reason = PortalTeleportNotAllowedReason.Unknown;
            if (MapProvider.Map.Portals.Count <= portalIndex)
            {
                _logger.LogWarning($"Unknown portal {portalIndex} for map {MapProvider.Map.Id}. Send from character {Id}.");
                return false;
            }

            var portal = MapProvider.Map.Portals[portalIndex];
            if (!portal.IsInPortalZone(PosX, PosY, PosZ))
            {
                _logger.LogWarning($"Character position is not in portal, map {MapProvider.Map.Id}. Portal index {portalIndex}. Send from character {Id}.");
                return false;
            }

            if (!portal.IsSameFaction(CountryProvider.Country))
            {
                return false;
            }

            if (!portal.IsRightLevel(LevelProvider.Level))
            {
                return false;
            }

            if (_gameWorld.CanTeleport(this, portal.MapId, out reason))
            {
                Teleport(portal.MapId, portal.Destination_X, portal.Destination_Y, portal.Destination_Z);
                return true;
            }
            else
            {
                return false;
            }*/
        }
    }
}
