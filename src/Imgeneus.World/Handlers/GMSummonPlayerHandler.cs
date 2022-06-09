using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMSummonPlayerHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IMapProvider _mapProvider;
        private readonly IMovementManager _movementManager;

        public GMSummonPlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IMapProvider mapProvider, IMovementManager movementManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _mapProvider = mapProvider;
            _movementManager = movementManager;
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_TELEPORT_PLAYER)]
        public void HandleUsTeleportPlayer(WorldClient client, GMTeleportPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;
            var ok = Handle(player, packet.MapId, packet.X, 10, packet.Z);

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_TELEPORT_PLAYER);
        }

        [HandlerAction(PacketType.GM_TELEPORT_PLAYER)]
        public void HandleTeleportPlayer(WorldClient client, GMTeleportPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;
            var ok = Handle(player, packet.MapId, packet.X, 10, packet.Z);

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_TELEPORT_PLAYER);
        }

        [HandlerAction(PacketType.GM_SUMMON_PLAYER)]
        public void HandleSummonPlayer(WorldClient client, GMSummonPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;
            var ok = Handle(player, _mapProvider.Map.Id, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ);

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_SUMMON_PLAYER);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_SUMMON_PLAYER)]
        public void HandleUsSummonPlayer(WorldClient client, GMSummonPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;
            var ok = Handle(player, _mapProvider.Map.Id, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ);

            if (ok)
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_SUMMON_PLAYER);
        }

        private bool Handle(Character target, ushort mapId, float x, float y, float z)
        {
            if (target == null)
                return false;

            if (!_gameWorld.Maps.ContainsKey(mapId))
                return false;

            target.TeleportationManager.Teleport(mapId, x, y, z, teleportedByAdmin: true, summonedByAdmin: true);

            return true;
        }
    }
}
