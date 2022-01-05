using Imgeneus.World.Packets;

namespace Imgeneus.World.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly IGamePacketFactory _packetFactory;

        public BaseHandler(IGamePacketFactory packetFactory)
        {
            _packetFactory = packetFactory;
        }
    }
}
