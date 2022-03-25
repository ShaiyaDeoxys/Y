using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DyeConfirmHandler : BaseHandler
    {
        private readonly IDyeingManager _dyeingManager;
        private readonly IInventoryManager _inventoryManager;

        public DyeConfirmHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDyeingManager dyeingManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _dyeingManager = dyeingManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.DYE_CONFIRM)]
        public async void Handle(WorldClient client, DyeConfirmPacket packet)
        {
            var result = await _dyeingManager.Dye(packet.DyeItemBag, packet.DyeItemSlot, packet.TargetItemBag, packet.TargetItemSlot);
            _packetFactory.SendDyeConfirm(client, result.Ok, result.Color);
            
            if (result.Ok && packet.TargetItemBag == 0)
                _inventoryManager.RaiseEquipmentChanged(packet.TargetItemSlot);
        }
    }
}
