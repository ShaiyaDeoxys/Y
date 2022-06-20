using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class TargetHandlers : BaseHandler
    {
        private readonly IMapProvider _mapProvider;
        private readonly IAttackManager _attackManager;
        private readonly IGameWorld _gameWorld;

        public TargetHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IAttackManager attackManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _attackManager = attackManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TARGET_SELECT_MOB)]
        public void HandleMobTarget(WorldClient client, MobInTargetPacket packet)
        {
            var mob = _mapProvider.Map.GetMob(_gameWorld.Players[_gameSession.Character.Id].CellId, packet.TargetId);
            if (mob is null)
                return;

            _attackManager.Target = mob;
            _packetFactory.SendMobInTarget(client, mob);
        }

        [HandlerAction(PacketType.TARGET_SELECT_CHARACTER)]
        public void HandlePlayerTarget(WorldClient client, PlayerInTargetPacket packet)
        {
            var player = _mapProvider.Map.GetPlayer(packet.TargetId);
            if (player is null)
                return;

            _attackManager.Target = player;
            _packetFactory.SendPlayerInTarget(client, player);
        }

        [HandlerAction(PacketType.TARGET_CLEAR)]
        public void HandleClear(WorldClient client, TargetClearPacket packet)
        {
            _attackManager.Target = null;
        }

        [HandlerAction(PacketType.TARGET_GET_CHARACTER_BUFFS)]
        public void HandleGetPlayerBuffs(WorldClient client, TargetCharacterGetBuffs packet)
        {
            var target = _mapProvider.Map.GetPlayer(packet.TargetId);
            if (target != null)
            {
                _attackManager.Target = target;
                _packetFactory.SendCurrentBuffs(client, target);
            }
        }

        [HandlerAction(PacketType.TARGET_GET_MOB_BUFFS)]
        public void HandleGetMobBuffs(WorldClient client, TargetMobGetBuffs packet)
        {
            var target = _mapProvider.Map.GetMob(_gameWorld.Players[_gameSession.Character.Id].CellId, packet.TargetId);
            if (target != null)
            {
                _attackManager.Target = target;
                _packetFactory.SendCurrentBuffs(client, target);
            }
        }

        [HandlerAction(PacketType.TARGET_MOB_GET_STATE)]
        public void HandleGetMobState(WorldClient client, TargetGetMobStatePacket packet)
        {
            var target = _mapProvider.Map.GetMob(_gameWorld.Players[_gameSession.Character.Id].CellId, packet.MobId);
            if (target != null)
            {
                _attackManager.Target = target;
                _packetFactory.SendMobPosition(client, target.Id, target.MovementManager.PosX, target.MovementManager.PosZ, target.MovementManager.MoveMotion);
                _packetFactory.SendMobState(client, target);
            }
        }

        [HandlerAction(PacketType.TARGET_CHARACTER_GET_HP)]
        public void HandleGetMobHP(WorldClient client, TargetPlayerGetHPPacket packet)
        {
            var target = _mapProvider.Map.GetPlayer(packet.TargetId);
            if (target != null)
            {
                _packetFactory.SendTargetHP(client, target.Id, target.HealthManager.CurrentHP);
            }
        }
    }
}
