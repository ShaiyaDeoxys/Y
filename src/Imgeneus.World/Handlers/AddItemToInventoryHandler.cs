using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AddItemToInventoryHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IInventoryManager _inventoryManager;
        private static readonly object _syncObject = new();

        public AddItemToInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.ADD_ITEM)]
        public void Handle(WorldClient client, MapPickUpItemPacket packet)
        {
            lock (_syncObject)
            {
                var player = _gameWorld.Players[_gameSession.Character.Id];
                var res = player.Map.GetItem(packet.ItemId, player);
                if (res.notOnMap)
                    return;

                if (res.Item is null)
                {
                    _packetFactory.SendItemDoesNotBelong(client);
                    return;
                }

                if (res.Item.Item.Type == Item.MONEY_ITEM_TYPE)
                {
                    player.Map.RemoveItem(player.CellId, res.Item.Id);
                    player.InventoryManager.Gold += (uint)res.Item.Item.Gold;
                    _packetFactory.SendAddItem(client, res.Item.Item);
                }
                else
                {
                    var inventoryItem = _inventoryManager.AddItem(res.Item.Item, "drop_from_map");
                    if (inventoryItem is null)
                    {
                        _packetFactory.SendFullInventory(client);
                    }
                    else
                    {
                        player.Map.RemoveItem(player.CellId, res.Item.Id);

                        if (player.PartyManager.Party != null)
                            player.PartyManager.Party.MemberGetItem(player, inventoryItem);
                    }
                }
            }
        }
    }
}
