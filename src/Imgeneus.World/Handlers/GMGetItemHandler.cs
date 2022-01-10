using Imgeneus.Database.Preload;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMGetItemHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IInventoryManager _inventoryManager;

        public GMGetItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IDatabasePreloader databasePreloader, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _databasePreloader = databasePreloader;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.GM_COMMAND_GET_ITEM)]
        public async Task Handle(WorldClient client, GMGetItemPacket packet)
        {
            if (!IsAdmin())
                return;

            var itemCount = packet.Count;
            var ok = false;

            while (itemCount > 0)
            {
                var newItem = new Item(_databasePreloader, packet.Type, packet.TypeId, itemCount);

                var item = await _inventoryManager.AddItem(newItem);
                if (item != null)
                {
                    _packetFactory.SendAddItem(client, item);
                    if (item.ExpirationTime != null)
                        _packetFactory.SendItemExpiration(client, item);

                    ok = true;
                }

                itemCount -= newItem.Count;
            }

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_COMMAND_GET_ITEM);
        }

        public bool IsAdmin()
        {
            return _gameWorld.Players[_gameSession.CharId].IsAdmin;
        }
    }
}
