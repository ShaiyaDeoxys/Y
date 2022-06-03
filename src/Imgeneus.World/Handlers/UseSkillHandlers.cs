using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class UseSkillHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ISkillsManager _skillsManager;
        private readonly IAttackManager _attackManager;
        private readonly IMapProvider _mapProvider;

        public UseSkillHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, ISkillsManager skillsManager, IAttackManager attackManager, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _skillsManager = skillsManager;
            _attackManager = attackManager;
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.USE_CHARACTER_TARGET_SKILL)]
        public void HandleUseSkillOnPlayer(WorldClient client, CharacterSkillAttackPacket packet)
        {
            var player = _gameWorld.Players[_gameSession.CharId];
            if (player is null)
                return;

            var target = packet.TargetId == 0 ? null : _mapProvider.Map.GetPlayer(packet.TargetId);

            UseSkill(client, packet.Number, player, target);
        }

        [HandlerAction(PacketType.USE_MOB_TARGET_SKILL)]
        public void HandleUseSkillOnMob(WorldClient client, MobSkillAttackPacket packet)
        {
            var player = _gameWorld.Players[_gameSession.CharId];
            if (player is null)
                return;

            var target = _mapProvider.Map.GetMob(player.CellId, packet.TargetId);
            if (target is null)
                return;

            UseSkill(client, packet.Number, player, target);
        }

        private void UseSkill(WorldClient client, byte number, Character player, IKillable target)
        {
            _skillsManager.Skills.TryGetValue(number, out var skill);
            if (skill is null)
                return;

            if (!_attackManager.CanAttack(skill.Number, target, out var success))
            {
                if (success != AttackSuccess.TooFastAttack)
                    _packetFactory.SendUseSkillFailed(client, player.Id, skill, target, success);
                return;
            }

            if (!_skillsManager.CanUseSkill(skill, target, out success))
            {
                _packetFactory.SendUseSkillFailed(client, player.Id, skill, target, success);
                return;
            }

            if (skill.CastTime == 0)
            {
                _skillsManager.UseSkill(skill, player, target);
            }
            else
            {
                _skillsManager.StartCasting(skill, target);
            }
        }
    }
}
