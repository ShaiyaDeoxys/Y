using Imgeneus.GameDefinitions;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMGetItemHandler : BaseHandler
    {
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly IInventoryManager _inventoryManager;

        public GMGetItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.GM_COMMAND_GET_ITEM)]
        public void Handle(WorldClient client, GMGetItemPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var itemCount = packet.Count;
            var ok = false;

            while (itemCount > 0)
            {
                var newItem = new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, packet.Type, packet.TypeId, itemCount);

                var item = _inventoryManager.AddItem(newItem, "gm_created");
                if (item != null)
                {
                    ok = true;
                }

                itemCount -= newItem.Count;
            }

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_COMMAND_GET_ITEM);
        }
    }
}
