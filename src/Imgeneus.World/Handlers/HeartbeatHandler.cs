using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class HeartbeatHandler : BaseHandler
    {
        public HeartbeatHandler(IGamePacketFactory packetFactory, IGameSession gameSession) : base(packetFactory, gameSession)
        {
        }

        [HandlerAction(PacketType.HEARTBEAT)]
        public void Handle(WorldClient client, HeartbeatPacket packet)
        {
            // the client sends this packet every 120000 milliseconds
            // to-do: figure out the logic behind closing the connection
        }
    }
}