using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Zone;
using System.Collections.Generic;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Game.Guild;
using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Vehicle;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private void HandleMotion(MotionPacket packet)
        {
            if (packet.Motion == Motion.None || packet.Motion == Motion.Sit)
            {
                Motion = packet.Motion;
            }

            _logger.LogDebug($"Character {Id} sends motion {packet.Motion}");
            OnMotion?.Invoke(this, packet.Motion);
        }
    }
}
