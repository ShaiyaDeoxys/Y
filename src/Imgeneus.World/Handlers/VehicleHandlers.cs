using Imgeneus.Core.Extensions;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class VehicleHandlers : BaseHandler
    {
        private readonly IVehicleManager _vehicleManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IMapProvider _mapProvider;
        private readonly ICountryProvider _countryProvider;
        private readonly IMovementManager _movementManager;

        public VehicleHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IVehicleManager vehicleManager, IInventoryManager inventoryManager, IMapProvider mapProvider, ICountryProvider countryProvider, IMovementManager movementManager) : base(packetFactory, gameSession)
        {
            _vehicleManager = vehicleManager;
            _inventoryManager = inventoryManager;
            _mapProvider = mapProvider;
            _countryProvider = countryProvider;
            _movementManager = movementManager;
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

        [HandlerAction(PacketType.VEHICLE_REQUEST)]
        public void Handle(WorldClient client, VehicleRequestPacket packet)
        {
            if (!_vehicleManager.IsOnVehicle || _vehicleManager.Vehicle2CharacterID != 0)
            {
                _packetFactory.SendVehicleResponse(client, VehicleResponse.Error);
                return;
            }

            var player = _mapProvider.Map.GetPlayer(packet.CharacterId);
            if (player is null || player.VehicleManager.IsOnVehicle || player.StealthManager.IsStealth || player.CountryProvider.Country != _countryProvider.Country || MathExtensions.Distance(_movementManager.PosX, player.PosX, _movementManager.PosZ, player.PosZ) > 20)
            {
                _packetFactory.SendVehicleResponse(client, VehicleResponse.Error);
                return;
            }

            player.VehicleManager.VehicleRequesterID = _gameSession.Character.Id;
            _packetFactory.SendVehicleRequest(player.GameSession.Client, _gameSession.Character.Id);
        }

        [HandlerAction(PacketType.VEHICLE_RESPONSE)]
        public void Handle(WorldClient client, VehicleResponsePacket packet)
        {
            if (_vehicleManager.IsOnVehicle)
            {
                return;
            }

            var player = _mapProvider.Map.GetPlayer(_vehicleManager.VehicleRequesterID);
            if (player is null || !player.VehicleManager.IsOnVehicle || player.VehicleManager.Vehicle2CharacterID != 0 || player.CountryProvider.Country != _countryProvider.Country || MathExtensions.Distance(_movementManager.PosX, player.PosX, _movementManager.PosZ, player.PosZ) > 20)
            {
                return;
            }

            if (packet.Rejected)
            {
                _packetFactory.SendVehicleResponse(player.GameSession.Client, VehicleResponse.Rejected);
            }
            else
            {
                _vehicleManager.Vehicle2CharacterID = _vehicleManager.VehicleRequesterID;
                _vehicleManager.VehicleRequesterID = 0;

                _packetFactory.SendVehicleResponse(player.GameSession.Client, VehicleResponse.Accepted);
            }
        }

        [HandlerAction(PacketType.USE_VEHICLE_2)]
        public void Handle(WorldClient client, UseVehicle2Packet packet)
        {
            if (_vehicleManager.Vehicle2CharacterID == 0)
                return;

            _vehicleManager.Vehicle2CharacterID = 0;
        }
    }
}
