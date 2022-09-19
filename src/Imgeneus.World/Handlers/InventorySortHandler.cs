using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class InventorySortHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;

        public InventorySortHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.INVENTORY_SORT)]
        public void Handle(WorldClient client, InventorySortPacket packet)
        {
            foreach (var item in packet.Items)
            {
                var items = _inventoryManager.MoveItem(item.SourceBag, item.SourceSlot, item.DestinationBag, item.DestinationSlot);
                _packetFactory.SendMoveItem(client, items.sourceItem, items.destinationItem, _inventoryManager.Gold);
            }

            _packetFactory.SendInventorySort(client);
        }
    }
}
