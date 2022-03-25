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
    public class RemoveGemHandler : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;
        private readonly IInventoryManager _inventoryManager;

        public RemoveGemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.GEM_REMOVE)]
        public void Handle(WorldClient client, GemRemovePacket packet)
        {
            var result = _linkingManager.RemoveGem(packet.Bag, packet.Slot, packet.ShouldRemoveSpecificGem, packet.GemPosition, packet.HammerBag, packet.HammerSlot);
            _packetFactory.SendRemoveGem(client, result.Success, result.Item, result.Slot, result.SavedGems, _inventoryManager.Gold);
        }
    }
}
