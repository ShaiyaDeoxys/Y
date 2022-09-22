using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RemoveItemFromInventoryHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IInventoryManager _inventoryManager;

        public RemoveItemFromInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession,  IGameWorld gameWorld, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.REMOVE_ITEM)]
        public void Handle(WorldClient client, RemoveItemPacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var item);

            if (item is null || item.AccountRestriction == ItemAccountRestrictionType.AccountRestricted || item.AccountRestriction == ItemAccountRestrictionType.CharacterRestricted)
                return;

            item.TradeQuantity = packet.Count <= item.Count ? packet.Count : item.Count;

            var removedItem = _inventoryManager.RemoveItem(item, "drop_on_map");
            if (removedItem is null)
                return;

            var player = _gameWorld.Players[_gameSession.Character.Id];
            player.Map.AddItem(new MapItem(removedItem, player, player.PosX, player.PosY, player.PosZ));
        }
    }
}
