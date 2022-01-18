using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class UseVehicleHandler : BaseHandler
    {
        private readonly IVehicleManager _vehicleManager;
        private readonly IInventoryManager _inventoryManager;

        public UseVehicleHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IVehicleManager vehicleManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _vehicleManager = vehicleManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.USE_VEHICLE)]
        public void Handle(WorldClient client, UseVehiclePacket packet)
        {
            if (_inventoryManager.Mount is null)
            {
                _packetFactory.SendUseVehicle(client, false, _vehicleManager.IsOnVehicle);
                return;
            }

            var ok = true;
            if (_vehicleManager.IsOnVehicle)
                ok = _vehicleManager.RemoveVehicle();
            else
                ok = _vehicleManager.CallVehicle();

            if (!ok)
                _packetFactory.SendUseVehicle(client, ok, _vehicleManager.IsOnVehicle);
        }
    }
}
