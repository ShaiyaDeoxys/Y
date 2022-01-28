using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DyeRerollHandler : BaseHandler
    {
        private readonly IDyeingManager _dyeingManager;

        public DyeRerollHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDyeingManager dyeingManager) : base(packetFactory, gameSession)
        {
            _dyeingManager = dyeingManager;
        }

        [HandlerAction(PacketType.DYE_REROLL)]
        public void Handle(WorldClient client, DyeRerollPacket packet)
        {
            _dyeingManager.Reroll();
            _packetFactory.SendDyeColors(client, _dyeingManager.AvailableColors);
        }
    }
}
