using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

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

            var rateBooster = _inventoryManager.InventoryItems.Values.FirstOrDefault(x => x.Special == SpecialEffect.EnchantEnhancer);

            var rates = new int[10];
            for (var i = 0; i < 10; i++)
            {
                _inventoryManager.InventoryItems.TryGetValue((packet.LapisiaBag[i], packet.LapisiaSlot[i]), out var lapisia);
                if (lapisia is null)
                    continue;

                rates[i] = _linkingManager.GetEnchantmentRate(item, lapisia, rateBooster);
            }

            var gold = _linkingManager.GetEnchantmentGold(item);

            _packetFactory.SendEnchantRate(client, packet.LapisiaBag, packet.LapisiaSlot, rates, gold);
        }

        [HandlerAction(PacketType.ENCHANT_ADD)]
        public void HandleEnchantAdd(WorldClient client, EnchantAddPacket packet)
        {
            var result = _linkingManager.TryEnchant(packet.ItemBag, packet.ItemSlot, packet.LapisiaBag, packet.LapisiaSlot);
            _packetFactory.SendEnchantAdd(client, result.Success, result.Lapisia, result.Item, _inventoryManager.Gold, packet.IsAutoEnchant, result.SafetyScrollLeft);
        }
    }
}
