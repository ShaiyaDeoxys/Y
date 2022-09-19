using Imgeneus.GameDefinitions;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMCreateMobHandler : BaseHandler
    {
        private readonly IGameDefinitionsPreloder _gameDefinitions;
        private readonly IMovementManager _movementManager;
        private readonly IMobFactory _mobFactory;
        private readonly IMapProvider _mapProvider;

        public GMCreateMobHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameDefinitionsPreloder gameDefinitions, IMovementManager movementManager, IMobFactory mobFactory, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _gameDefinitions = gameDefinitions;
            _movementManager = movementManager;
            _mobFactory = mobFactory;
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.GM_CREATE_MOB)]
        public void HandleOriginal(WorldClient client, GMCreateMobPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_gameDefinitions.Mobs.ContainsKey(packet.MobId))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CREATE_MOB);
                return;
            }

            Handle(client, packet);

            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_CREATE_MOB)]
        public void HandleUsl(WorldClient client, GMCreateMobPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_gameDefinitions.Mobs.ContainsKey(packet.MobId))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_CREATE_MOB);
                return;
            }

            Handle(client, packet);

            _packetFactory.SendGmCommandSuccess(client);
        }

        private void Handle(WorldClient client, GMCreateMobPacket packet)
        {
            for (int i = 0; i < packet.NumberOfMobs; i++)
            {
                // TODO: calculate move area.
                var moveArea = new MoveArea(_movementManager.PosX > 10 ? _movementManager.PosX - 10 : 1, _movementManager.PosX + 10, _movementManager.PosY > 10 ? _movementManager.PosY - 10 : _movementManager.PosY, _movementManager.PosY + 10, _movementManager.PosZ > 10 ? _movementManager.PosZ - 10 : 1, _movementManager.PosZ + 10);

                var mob = _mobFactory.CreateMob(packet.MobId, false, moveArea);

                _mapProvider.Map.AddMob(mob);
            }
        }
    }
}
