using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class NpcSellItemHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;

        public NpcSellItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.NPC_SELL_ITEM)]
        public void Handle(WorldClient client, NpcSellItemPacket packet)
        {
            if (packet.Bag == 0) // Worn item can not be sold, player should take it off first.
                return;

            _inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var itemToSell);
            if (itemToSell is null) // Item for sale not found.
                return;

            var fullSold = itemToSell.Count <= packet.Count;

            var soldItem = _inventoryManager.SellItem(itemToSell, packet.Count);
            if (soldItem != null)
            {
                if (fullSold)
                    itemToSell.Count = 0;

                _packetFactory.SendSoldItem(client, true, itemToSell, _inventoryManager.Gold);
            }
            else
            {
                _packetFactory.SendSoldItem(client, true, null, _inventoryManager.Gold);
            }
        }
    }
}
