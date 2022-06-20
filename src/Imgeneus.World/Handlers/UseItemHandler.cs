using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class UseItemHandler : BaseHandler
    {
        private readonly IInventoryManager _invetoryManager;

        public UseItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager invetoryManager) : base(packetFactory, gameSession)
        {
            _invetoryManager = invetoryManager;
        }

        [HandlerAction(PacketType.USE_ITEM)]
        public async Task Handle(WorldClient client, UseItemPacket packet)
        {
            var ok = await _invetoryManager.TryUseItem(packet.Bag, packet.Slot);
            if (!ok)
                _packetFactory.SendCanNotUseItem(client, _gameSession.Character.Id);
        }

        [HandlerAction(PacketType.USE_ITEM_2)]
        public async Task Handle(WorldClient client, UseItem2Packet packet)
        {
            var ok = await _invetoryManager.TryUseItem(packet.Bag, packet.Slot, packet.TargetId);
            if (!ok)
                _packetFactory.SendCanNotUseItem(client, _gameSession.Character.Id);
        }
    }
}
