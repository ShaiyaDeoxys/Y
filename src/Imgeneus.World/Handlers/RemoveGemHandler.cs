using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RemoveGemHandler : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;

        public RemoveGemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.GEM_REMOVE)]
        public void Handle(WorldClient client, GemRemovePacket packet)
        {
        }
    }
}
