using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AddGemHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly ILinkingManager _linkingManager;

        public AddGemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.GEM_ADD)]
        public void Handle(WorldClient client, GemAddPacket packet)
        {
            var result = _linkingManager.AddGem(packet.Bag, packet.Slot, packet.DestinationBag, packet.DestinationSlot, packet.HammerBag, packet.HammerSlot);
            _packetFactory.SendAddGem(client, result.Success, result.Gem, result.Item, result.Slot, _inventoryManager.Gold, result.Hammer);
        }
    }
}
