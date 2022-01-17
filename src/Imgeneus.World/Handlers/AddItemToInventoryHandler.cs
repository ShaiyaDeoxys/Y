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
    public class AddItemToInventoryHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IInventoryManager _inventoryManager;

        public AddItemToInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.ADD_ITEM)]
        public async Task Handle(WorldClient client, MapPickUpItemPacket packet)
        {
            var player = _gameWorld.Players[_gameSession.CharId];
            var mapItem = player.Map.GetItem(packet.ItemId, player);
            if (mapItem is null)
            {
                _packetFactory.SendItemDoesNotBelong(client);
                return;
            }

            if (mapItem.Item.Type == Item.MONEY_ITEM_TYPE)
            {
                player.Map.RemoveItem(player.CellId, mapItem.Id);
                mapItem.Item.Bag = 1;
                player.InventoryManager.Gold = player.InventoryManager.Gold + (uint)mapItem.Item.Gem1.TypeId;
                _packetFactory.SendAddItem(client, mapItem.Item);
            }
            else
            {
                var inventoryItem = await _inventoryManager.AddItem(mapItem.Item);
                if (inventoryItem is null)
                {
                    _packetFactory.SendFullInventory(client);
                }
                else
                {
                    player.Map.RemoveItem(player.CellId, mapItem.Id);
                    _packetFactory.SendAddItem(client, inventoryItem);

                    if (player.Party != null)
                        player.Party.MemberGetItem(player, inventoryItem);
                }
            }
        }
    }
}
