using Imgeneus.Database.Constants;
using Imgeneus.Game.Crafting;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class ChaoticSquareHandlers : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly ICraftingConfiguration _craftingConfiguration;

        public ChaoticSquareHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, ICraftingConfiguration craftingConfiguration) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _craftingConfiguration = craftingConfiguration;
        }

        [HandlerAction(PacketType.CHAOTIC_SQUARE_LIST)]
        public void Handle(WorldClient client, ChaoticSquareListPacket packet)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var squareItem))
                return;

            if (squareItem.Special != SpecialEffect.ChaoticSquare)
                return;

            var config = _craftingConfiguration.SquareItems.FirstOrDefault(x => x.Type == squareItem.Type && x.TypeId == squareItem.TypeId);
            if (config is null)
                return;

            _packetFactory.SendCraftList(client, config);
        }
    }
}
