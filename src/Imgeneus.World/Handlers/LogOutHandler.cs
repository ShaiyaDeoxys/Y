using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class LogOutHandler : BaseHandler
    {
        public LogOutHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ISelectionScreenManager selectionScreenManager) : base(packetFactory, gameSession)
        {
        }

        [HandlerAction(PacketType.LOGOUT)]
        public void Handle(WorldClient client, LogOutPacket packet)
        {
            _gameSession.StartLogOff();
        }
    }
}
