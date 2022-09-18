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
        public void Handle(WorldClient client, GMClearInventoryPacket packet)
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
    }
}
