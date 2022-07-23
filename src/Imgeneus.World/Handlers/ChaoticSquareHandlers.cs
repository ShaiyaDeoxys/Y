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
        private readonly ICraftingManager _craftingManager;

        public ChaoticSquareHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, ICraftingConfiguration craftingConfiguration, ICraftingManager craftingManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _craftingConfiguration = craftingConfiguration;
            _craftingManager = craftingManager;
        }

        [HandlerAction(PacketType.CHAOTIC_SQUARE_LIST)]
        public void GetListHandle(WorldClient client, ChaoticSquareListPacket packet)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var squareItem))
                return;

            if (squareItem.Special != SpecialEffect.ChaoticSquare)
                return;

            var config = _craftingConfiguration.SquareItems.FirstOrDefault(x => x.Type == squareItem.Type && x.TypeId == squareItem.TypeId);
            if (config is null)
                return;

            _craftingManager.ChaoticSquare = (config.Type, config.TypeId);

            _packetFactory.SendCraftList(client, config);
        }

        [HandlerAction(PacketType.CHAOTIC_SQUARE_RECIPE)]
        public void GetRecipeHandle(WorldClient client, ChaoticSquareRecipePacket packet)
        {
            var config = _craftingConfiguration.SquareItems.FirstOrDefault(x => x.Type == _craftingManager.ChaoticSquare.Type && x.TypeId == _craftingManager.ChaoticSquare.TypeId);
            if (config is null)
                return;

            var recipe = config.Recipes.FirstOrDefault(x => x.Type == packet.Type && x.TypeId == packet.TypeId);
            if (recipe is null)
                return;

            _packetFactory.SendCraftRecipe(client, recipe);
        }

        [HandlerAction(PacketType.CHAOTIC_SQUARE_CREATE)]
        public void CreateHandle(WorldClient client, ChaoticSquareCreatePacket packet)
        {
            var ok = _craftingManager.TryCraft(packet.Bag, packet.Slot, packet.Index, packet.HammerBag, packet.HammerSlot);
            _packetFactory.SendCraftSuccess(client, ok);
        }
    }
}
