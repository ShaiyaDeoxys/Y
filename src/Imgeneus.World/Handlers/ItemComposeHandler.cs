using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class ItemComposeHandler : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;

        public ItemComposeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.ITEM_COMPOSE)]
        public void Handle(WorldClient client, ItemComposePacket packet)
        {
            var result = _linkingManager.Compose(packet.RuneBag, packet.RuneSlot, packet.ItemBag, packet.ItemSlot);
            _packetFactory.SendComposition(client, result.Success, result.Item);
        }

        [HandlerAction(PacketType.ITEM_COMPOSE_ABSOLUTE)]
        public void HandleAbsolute(WorldClient client, ItemComposeAbsolutePacket packet)
        {
            var result = _linkingManager.AbsoluteCompose(packet.RuneBag, packet.RuneSlot, packet.ItemBag, packet.ItemSlot);
            _packetFactory.SendAbsoluteComposition(client, result.Success, result.Item);
        }
    }
}
