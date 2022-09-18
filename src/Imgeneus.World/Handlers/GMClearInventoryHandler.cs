using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMClearInventoryHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IGameWorld _gameWorld;

        public GMClearInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_CLEAR_INVENTORY)]
        public void HandleClearInventory(WorldClient client, GMClearInventoryPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(x => x.AdditionalInfoManager.Name == packet.CharacterName);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CLEAR_INVENTORY);
                return;
            }

            var items = player.InventoryManager.InventoryItems.Where(x => x.Key.Bag != 0);
            foreach (var item in items)
                player.InventoryManager.InventoryItems.TryRemove((item.Key.Bag, item.Key.Slot), out _);

            _packetFactory.SendGmClearInventory(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_CLEAR_EQUIPMENT)]
        public void HandleClearEquipment(WorldClient client, GMClearInventoryPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(x => x.AdditionalInfoManager.Name == packet.CharacterName);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CLEAR_EQUIPMENT);
                return;
            }

            var items = player.InventoryManager.InventoryItems.Where(x => x.Key.Bag == 0);
            foreach (var item in items)
                player.InventoryManager.InventoryItems.TryRemove((item.Key.Bag, item.Key.Slot), out _);

            player.InventoryManager.Helmet = null;
            player.InventoryManager.Armor = null;
            player.InventoryManager.Pants = null;
            player.InventoryManager.Gauntlet = null;
            player.InventoryManager.Boots = null;
            player.InventoryManager.Weapon = null;
            player.InventoryManager.Shield = null;
            player.InventoryManager.Cape = null;
            player.InventoryManager.Amulet = null;
            player.InventoryManager.Ring1 = null;
            player.InventoryManager.Ring2 = null;
            player.InventoryManager.Bracelet1 = null;
            player.InventoryManager.Bracelet2 = null;
            player.InventoryManager.Mount = null;
            player.InventoryManager.Pet = null;
            player.InventoryManager.Costume = null;
            player.InventoryManager.Wings = null;

            _packetFactory.SendGmClearEquipment(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
