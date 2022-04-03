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
    public class EnchantHandlers : BaseHandler
    {
        private readonly ILinkingManager _linkingManager;
        private readonly IInventoryManager _inventoryManager;

        public EnchantHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, ILinkingManager linkingManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _linkingManager = linkingManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.ENCHANT_RATE)]
        public void HandleEnchantRate(WorldClient client, EnchantRatePacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.ItemBag, packet.ItemSlot), out var item);
            if (item is null)
                return;

            var rate = _linkingManager.GetEnchantmentRate(item);
            var gold = _linkingManager.GetEnchantmentGold(item);

            _packetFactory.SendEnchantRate(client, packet.LapisiaBag, packet.LapisiaSlot, rate, gold);
        }
    }
}
