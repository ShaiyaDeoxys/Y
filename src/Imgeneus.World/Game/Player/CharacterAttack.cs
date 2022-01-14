using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Monster;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character : IKillable, IKiller
    {
        #region Target

        private BaseKillable _target;
        public BaseKillable Target
        {
            get => _target; set
            {
                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded -= Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved -= Target_OnBuffRemoved;
                }

                _target = value;

                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded += Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved += Target_OnBuffRemoved;
                    TargetChanged(Target);
                }
            }
        }

        private void Target_OnBuffAdded(int senderId, Buff buff)
        {
            SendTargetAddBuff(senderId, buff, Target is Mob);
        }

        private void Target_OnBuffRemoved(int senderId, Buff buff)
        {
            SendTargetRemoveBuff(senderId, buff, Target is Mob);
        }

        #endregion

        #region Damage calculation

        /// <summary>
        /// Uses skill or auto attack.
        /// </summary>
        private void Attack(byte skillNumber, IKillable target = null)
        {
            if (StealthManager.IsStealth && !StealthManager.IsAdminStealth)
            {
                var stealthBuff = BuffsManager.ActiveBuffs.FirstOrDefault(b => _databasePreloader.Skills[(b.SkillId, b.SkillLevel)].TypeDetail == TypeDetail.Stealth);
                stealthBuff.CancelBuff();
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER)
            {
                AttackManager.AutoAttack();
            }
            else
            {
                if (!SkillsManager.Skills.TryGetValue(skillNumber, out var skill))
                {
                    _logger.LogWarning($"Character {Id} tries to use nonexistent skill.");
                    return;
                }

                if (skill.CastTime == 0)
                {
                    SkillsManager.UseSkill(skill, this, target);
                }
                else
                {
                    SkillsManager.StartCasting(skill, target);
                }
            }
        }

        #endregion
    }
}
