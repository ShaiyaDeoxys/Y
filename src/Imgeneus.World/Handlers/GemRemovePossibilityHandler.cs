using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Collections.Generic;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GemRemovePossibilityHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly ILinkingManager _linkingManager;

        public GemRemovePossibilityHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.GEM_REMOVE_POSSIBILITY)]
        public void Handle(WorldClient client, GemRemovePossibilityPacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var item);
            if (item is null)
                return;

            double rate = 0;
            int gold = 0;

            if (packet.ShouldRemoveSpecificGem)
            {
                Gem gem = null;
                switch (packet.GemPosition)
                {
                    case 0:
                        gem = item.Gem1;
                        break;

                    case 1:
                        gem = item.Gem2;
                        break;

                    case 2:
                        gem = item.Gem3;
                        break;

                    case 3:
                        gem = item.Gem4;
                        break;

                    case 4:
                        gem = item.Gem5;
                        break;

                    case 5:
                        gem = item.Gem6;
                        break;
                }

                if (gem is null)
                    return;

                _inventoryManager.InventoryItems.TryGetValue((packet.HammerBag, packet.HammerSlot), out var hammer);

                rate = _linkingManager.GetRemoveRate(gem, hammer);
                gold = _linkingManager.GetRemoveGold(gem);
            }
            else
            {
                var gems = new List<Gem>();

                if (item.Gem1 != null)
                    gems.Add(item.Gem1);
                if (item.Gem2 != null)
                    gems.Add(item.Gem2);
                if (item.Gem3 != null)
                    gems.Add(item.Gem3);
                if (item.Gem4 != null)
                    gems.Add(item.Gem4);
                if (item.Gem5 != null)
                    gems.Add(item.Gem5);
                if (item.Gem6 != null)
                    gems.Add(item.Gem6);

                foreach (var gem in gems)
                {
                    if (rate == 0)
                        rate = 1;

                    rate *= _linkingManager.GetRemoveRate(gem, null) / 100;
                    gold += _linkingManager.GetRemoveGold(gem);
                }

                rate = rate * 100;
            }

            _packetFactory.SendGemRemovePossibility(client, rate, gold);
        }
    }
}
