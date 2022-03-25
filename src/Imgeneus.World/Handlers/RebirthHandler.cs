using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RebirthHandler : BaseHandler
    {
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IHealthManager _healthManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMovementManager _movementManager;

        public RebirthHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IGameWorld gameWorld, IHealthManager healthManager, ITeleportationManager teleportationManager, IMovementManager movementManager) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _healthManager = healthManager;
            _teleportationManager = teleportationManager;
            _movementManager = movementManager;
        }

        [HandlerAction(PacketType.REBIRTH_TO_NEAREST_TOWN)]
        public void Handle(WorldClient client, RebirthPacket packet)
        {
            var rebirthType = (RebirthType)packet.RebirthType;

            // TODO: implement other rebith types.

            (ushort MapId, float X, float Y, float Z) rebirthCoordinate = (0, 0, 0, 0);

            if (rebirthType == RebirthType.KillSoul)
            {
                rebirthCoordinate = _mapProvider.Map.GetRebirthMap(_gameWorld.Players[_gameSession.CharId]);
            }

            if (_mapProvider.Map.Id != rebirthCoordinate.MapId)
                _teleportationManager.Teleport(rebirthCoordinate.MapId, rebirthCoordinate.X, rebirthCoordinate.Y, rebirthCoordinate.Z);
            else
            {
                _movementManager.PosX = rebirthCoordinate.X;
                _movementManager.PosY = rebirthCoordinate.Y;
                _movementManager.PosZ = rebirthCoordinate.Z;
            }

            _healthManager.Rebirth();
        }
    }
}
