using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class EnterPortalHandler : BaseHandler
    {
        private readonly ITeleportationManager _teleportationManager;

        public EnterPortalHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITeleportationManager teleportationManager) : base(packetFactory, gameSession)
        {
            _teleportationManager = teleportationManager;
        }

        [HandlerAction(PacketType.CHARACTER_ENTERED_PORTAL)]
        public void Handle(WorldClient client, CharacterEnteredPortalPacket packet)
        {
            var success = _teleportationManager.TryTeleport(packet.PortalId, out var reason);

            if (!success)
                _packetFactory.SendPortalTeleportNotAllowed(client, reason);
        }
    }
}
