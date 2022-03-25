using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class ChangeEncryptionHandler : BaseHandler
    {
        public ChangeEncryptionHandler(IGamePacketFactory packetFactory, IGameSession gameSession) : base(packetFactory, gameSession)
        {
        }

        [HandlerAction(PacketType.CHANGE_ENCRYPTION)]
        public void Handle(WorldClient client, ChangeEncryptionPacket packet)
        {
            // ?
        }
    }
}
