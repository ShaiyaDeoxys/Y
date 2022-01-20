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
    public class GemAddPossibilityHandler : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;
        private readonly IInventoryManager _inventoryManager;

        public GemAddPossibilityHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.GEM_ADD_POSSIBILITY)]
        public void Handle(WorldClient client, GemAddPossibilityPacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.GemBag, packet.GemSlot), out var gem);
            if (gem is null || gem.Type != Item.GEM_ITEM_TYPE)
                return;

            Item hammer = null;
            if (packet.HammerBag != 0)
                _inventoryManager.InventoryItems.TryGetValue((packet.HammerBag, packet.HammerSlot), out hammer);

            var rate = _linkingManager.GetRate(gem, hammer);
            var gold = _linkingManager.GetGold(gem);

            _packetFactory.SendGemPossibility(client, rate, gold);
        }
    }
}
