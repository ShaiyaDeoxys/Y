using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Health;
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

        public RebirthHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IGameWorld gameWorld, IHealthManager healthManager, ITeleportationManager teleportationManager) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _healthManager = healthManager;
            _teleportationManager = teleportationManager;
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

            _healthManager.Rebirth();
            _teleportationManager.Teleport(rebirthCoordinate.MapId, rebirthCoordinate.X, rebirthCoordinate.Y, rebirthCoordinate.Z);
        }
    }
}
