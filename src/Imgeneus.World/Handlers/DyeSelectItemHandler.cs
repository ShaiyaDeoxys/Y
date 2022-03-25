using Imgeneus.Database.Constants;
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
    public class DyeSelectItemHandler : BaseHandler
    {
        private readonly IDyeingManager _dyeingManager;

        public DyeSelectItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDyeingManager dyeingManager) : base(packetFactory, gameSession)
        {
            _dyeingManager = dyeingManager;
        }

        [HandlerAction(PacketType.DYE_SELECT_ITEM)]
        public void Handle(WorldClient client, DyeSelectItemPacket packet)
        {
            var ok = _dyeingManager.SelectItem(packet.DyeItemBag, packet.DyeItemSlot, packet.TargetItemBag, packet.TargetItemSlot);
            _packetFactory.SendSelectDyeItem(client, ok);
        }
    }
}
