using Imgeneus.Game.Skills;
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
using System;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class UseSkillHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ISkillsManager _skillsManager;
        private readonly IAttackManager _attackManager;
        private readonly IMapProvider _mapProvider;
        private readonly ISkillCastingManager _skillCastingManager;

        public UseSkillHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, ISkillsManager skillsManager, IAttackManager attackManager, IMapProvider mapProvider, ISkillCastingManager skillCastingManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _skillsManager = skillsManager;
            _attackManager = attackManager;
            _mapProvider = mapProvider;
            _skillCastingManager = skillCastingManager;
        }

        [HandlerAction(PacketType.USE_CHARACTER_TARGET_SKILL)]
        public void HandleUseSkillOnPlayer(WorldClient client, CharacterSkillAttackPacket packet)
        {
            _packetFactory.SendAutoAttackStop(client);

            var player = _gameWorld.Players[_gameSession.Character.Id];
            if (player is null)
                return;

            var target = packet.TargetId == 0 ? null : _mapProvider.Map.GetPlayer(packet.TargetId);

            UseSkill(client, packet.Number, player, target);
        }

        [HandlerAction(PacketType.USE_MOB_TARGET_SKILL)]
        public void HandleUseSkillOnMob(WorldClient client, MobSkillAttackPacket packet)
        {
            _packetFactory.SendAutoAttackStop(client);

            var player = _gameWorld.Players[_gameSession.Character.Id];
            if (player is null)
                return;

            var target = _mapProvider.Map.GetMob(player.CellId, packet.TargetId);
            if (target is null && packet.TargetId != -1)
                return;

            UseSkill(client, packet.Number, player, target);
        }

        private void UseSkill(WorldClient client, byte number, Character player, IKillable target)
        {
            _skillsManager.Skills.TryGetValue(number, out var skill);
            if (skill is null)
                return;

            _attackManager.SkipNextAutoAttack = false;

            if (!_attackManager.CanAttack(skill.Number, target, out var success))
            {
                if (success != AttackSuccess.TooFastAttack)
                {
                    _packetFactory.SendUseSkillFailed(client, player.Id, skill, target, success);
                }
                else
                {
                    _attackManager.SkipNextAutoAttack = true;
                    _attackManager.SkipAutoAttackRequestTime = DateTime.UtcNow;
                    _packetFactory.SendUseSkillFailed(client, player.Id, skill, target, AttackSuccess.CooldownNotOver);
                }
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
                _skillCastingManager.StartCasting(skill, target);
            }
        }
    }
}
