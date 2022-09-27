using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class NotImplementedHandlers : BaseHandler
    {
        public NotImplementedHandlers(IGamePacketFactory packetFactory, IGameSession gameSession) : base(packetFactory, gameSession)
        {
        }

        [HandlerAction(PacketType.INFINITE_SANCTUARY)]
        public void HandleInfiniteSanctuary(WorldClient client, EmptyPacket packet)
        {
            // Not gonna implement any time soon.
        }
    }
}
