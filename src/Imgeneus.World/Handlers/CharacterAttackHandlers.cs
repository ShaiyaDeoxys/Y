using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class CharacterAttackHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IStealthManager _stealthManager;
        private readonly IBuffsManager _buffsManager;
        private readonly ISkillsManager _skillsManager;
        private readonly IAttackManager _attackManager;

        public CharacterAttackHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IDatabasePreloader databasePreloader, IStealthManager stealthManager, IBuffsManager buffsManager, ISkillsManager skillsManager, IAttackManager attackManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _databasePreloader = databasePreloader;
            _stealthManager = stealthManager;
            _buffsManager = buffsManager;
            _skillsManager = skillsManager;
            _attackManager = attackManager;
        }

        [HandlerAction(PacketType.USE_CHARACTER_TARGET_SKILL)]
        public void Handle(WorldClient client, CharacterSkillAttackPacket packet)
        {
            var player = _gameWorld.Players[_gameSession.CharId];
            if (player is null)
                return;

            var target = player.Map.GetPlayer(packet.TargetId);
            Attack(packet.Number, player, target);
        }

        /// <summary>
        /// Uses skill or auto attack.
        /// </summary>
        private void Attack(byte skillNumber, Character player, IKillable target)
        {
            if (_stealthManager.IsStealth && !_stealthManager.IsAdminStealth)
            {
                var stealthBuff = _buffsManager.ActiveBuffs.FirstOrDefault(b => _databasePreloader.Skills[(b.SkillId, b.SkillLevel)].TypeDetail == TypeDetail.Stealth);
                stealthBuff.CancelBuff();
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER)
            {
                _attackManager.AutoAttack();
            }
            else
            {
                if (!_skillsManager.Skills.TryGetValue(skillNumber, out var skill))
                {
                    //_logger.LogWarning($"Character {Id} tries to use nonexistent skill.");
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
}
