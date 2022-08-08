using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class ChangeAppearanceHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IAdditionalInfoManager _additionalInfoManager;

        public ChangeAppearanceHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, IAdditionalInfoManager additionalInfoManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _additionalInfoManager = additionalInfoManager;
        }

        [HandlerAction(PacketType.CHANGE_APPEARANCE)]
        public void Handle(WorldClient client, ChangeAppearancePacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var item);
            if (item is null || (item.Special != SpecialEffect.AppearanceChange && item.Special != SpecialEffect.SexChange))
                return;

            _inventoryManager.TryUseItem(packet.Bag, packet.Slot, skipApplyingItemEffect: true);
            _additionalInfoManager.ChangeAppearance(packet.Hair, packet.Face, packet.Size, packet.Sex);
        }
    }
}
