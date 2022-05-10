using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MoveItemInInventoryHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;

        public MoveItemInInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.INVENTORY_MOVE_ITEM)]
        public void Handle(WorldClient client, MoveItemInInventoryPacket packet)
        {
            var items = _inventoryManager.MoveItem(packet.CurrentBag, packet.CurrentSlot, packet.DestinationBag, packet.DestinationSlot);
            _packetFactory.SendMoveItem(client, items.sourceItem, items.destinationItem, _inventoryManager.Gold);
        }
     }
}
