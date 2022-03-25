using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;

namespace Imgeneus.World.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly IGamePacketFactory _packetFactory;
        protected readonly IGameSession _gameSession;

        public BaseHandler(IGamePacketFactory packetFactory, IGameSession gameSession)
        {
            _packetFactory = packetFactory;
            _gameSession = gameSession;
        }
    }
}
