using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class QuitGameHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public QuitGameHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.QUIT_GAME)]
        public void Handle(WorldClient client, QuitGamePacket packet)
        {
            _gameSession.StartLogOff(true);
        }
    }
}
