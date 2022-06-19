using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Login;
using InterServer.Server;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.Login.Handlers
{
    [Handler]
    public class HeartbeatHandler
    {
        private readonly IInterServer _interServer;

        public HeartbeatHandler(IInterServer interServer)
        {
            _interServer = interServer;
        }

        [HandlerAction(PacketType.HEARTBEAT)]
        public void Handle(LoginClient sender, HeartbeatPacket packet)
        {
            // the client sends this packet every 120000 milliseconds
            // to-do: figure out the logic behind closing the connection
        }
    }
}