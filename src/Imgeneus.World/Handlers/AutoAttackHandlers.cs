using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AutoAttackHandlers : BaseHandler
    {
        private readonly IMapProvider _mapProvider;
        private readonly IAttackManager _attackManager;
        private readonly IGameWorld _gameWorld;

        public AutoAttackHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IAttackManager attackManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _attackManager = attackManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.AUTO_ATTACK_STOP)]
        public void HandleAttackStart(WorldClient client, AttackStartPacket packet)
        {
            // Not sure, but maybe I should not permit any attack start?
        }

        [HandlerAction(PacketType.CHARACTER_CHARACTER_AUTO_ATTACK)]
        public void HandleAutoAttackOnPlayer(WorldClient client, CharacterAutoAttackPacket packet)
        {
            if (_attackManager.SkipNextAutoAttack && DateTime.UtcNow.Subtract(_attackManager.SkipAutoAttackRequestTime).TotalMilliseconds < _attackManager.NextAttackTime * 1.2)
                return;

            var target = _mapProvider.Map.GetPlayer(packet.TargetId);
            if (target is null)
                return;

            _attackManager.Target = target;

            if (_attackManager.CanAttack(IAttackManager.AUTO_ATTACK_NUMBER, target, out var success))
                _attackManager.AutoAttack(_gameWorld.Players[_gameSession.Character.Id]);
            else
                _packetFactory.SendAutoAttackFailed(client, _gameSession.Character.Id, target, success);
        }

        [HandlerAction(PacketType.CHARACTER_MOB_AUTO_ATTACK)]
        public void HandleAutoAttackOnMob(WorldClient client, MobAutoAttackPacket packet)
        {
            if (_attackManager.SkipNextAutoAttack && DateTime.UtcNow.Subtract(_attackManager.SkipAutoAttackRequestTime).TotalMilliseconds < _attackManager.NextAttackTime * 1.2)
                return;

            var target = _mapProvider.Map.GetMob(_gameWorld.Players[_gameSession.Character.Id].CellId, packet.TargetId);
            if (target is null)
                return;

            _attackManager.Target = target;

            if (_attackManager.CanAttack(255, target, out var success))
                _attackManager.AutoAttack(_gameWorld.Players[_gameSession.Character.Id]);
            else if (success != AttackSuccess.TooFastAttack)
                _packetFactory.SendAutoAttackFailed(client, _gameSession.Character.Id, target, success);
        }
    }
}
