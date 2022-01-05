using Imgeneus.Login.Packets;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Login;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.Login.Handlers
{
    [Handler]
    public class SelectServerHandler
    {
        private readonly ILoginServer _server;
        private readonly ILoginPacketFactory _loginPacketFactory;

        public SelectServerHandler(ILoginServer server, ILoginPacketFactory loginPacketFactory)
        {
            _server = server;
            _loginPacketFactory = loginPacketFactory;
        }

        [HandlerAction(PacketType.SELECT_SERVER)]
        public void Handle(LoginClient sender, SelectServerPacket packet)
        {
            var worldInfo = _server.GetWorldByID(packet.WorldId);

            if (worldInfo == null)
            {
                _loginPacketFactory.SelectServerFailed(sender, SelectServer.CannotConnect);
                return;
            }

            // For some reason, the current game.exe has version -1. Maybe this is somehow connected with decrypted packages?
            // In any case, for now client version check is disabled.
            if (false && worldInfo.BuildVersion != packet.BuildClient && packet.BuildClient != -1)
            {
                _loginPacketFactory.SelectServerFailed(sender, SelectServer.VersionDoesntMatch);
                return;
            }

            if (worldInfo.ConnectedUsers >= worldInfo.MaxAllowedUsers)
            {
                _loginPacketFactory.SelectServerFailed(sender, SelectServer.ServerSaturated);
                return;
            }

            _loginPacketFactory.SelectServerSuccess(sender, worldInfo.Host);
        }
    }
}
