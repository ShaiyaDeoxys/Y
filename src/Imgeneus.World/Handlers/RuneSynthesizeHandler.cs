using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RuneSynthesizeHandler : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;

        public RuneSynthesizeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.RUNE_SYNTHESIZE)]
        public void Handle(WorldClient client, RuneSynthesizePacket packet)
        {
            var result = _linkingManager.TryRuneSynthesize(packet.RuneBag, packet.RuneSlot, packet.VialBag, packet.VialSlot);
            _packetFactory.SendRuneSynthesize(client, result.Success, result.PerfectRune);
        }
    }
}
